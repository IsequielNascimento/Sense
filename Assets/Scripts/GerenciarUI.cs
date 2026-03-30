using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement; 

public class GerenciarUI : MonoBehaviour
{
    [Header("Configuração de Cena")]
    [Tooltip("Nome da cena AR com Canvas que será carregada")]
    public string nomeCenaARCanvas = "ARMudanca";

    [Header("Referências UI")]
    public TMP_InputField campoDePesquisa;
    public GameObject painelDetalhes;
    public TMP_Text holderTitulosUI;
    public TMP_Text tituloProblema;
    public TMP_Text tituloTitulosUI;
    public TMP_Text subtituloTitulosUI;
    public TMP_Text descricao;
    public TMP_Text descricaoProblema;
    public TMP_Text solucao;
    public TMP_Text solucaoProblema;
    public TMP_Text textpasso;
    
    [Header("Resultados")]
    public GameObject templateProblema;
    public Transform listaDeResultados;

    private string idiomaAtual = "pt";
    private Problema problemaAtual;

    private void Start()
    {
        campoDePesquisa.onSubmit.AddListener(_ => Pesquisar());
        campoDePesquisa.onValueChanged.AddListener(_ => Pesquisar());

        templateProblema.SetActive(false);
        
        // Correção do Bug de Dois Cliques: 
        // Em vez de desligar o objeto na força bruta, chamamos a função do PainelDeslizante.
        PainelDeslizante painelScript = painelDetalhes.GetComponent<PainelDeslizante>();
        if (painelScript != null)
        {
            painelScript.FecharInstantaneamente();
        }
        else
        {
            painelDetalhes.SetActive(false);
        }

        if (IdiomaManager.Instance != null)
        {
            idiomaAtual = IdiomaManager.Instance.ObterIdioma();
            TrocarIdioma(idiomaAtual);
        }
    }

    public void TrocarIdioma(string novoIdioma)
    {
        idiomaAtual = novoIdioma;
        TextAsset arquivo = Resources.Load<TextAsset>($"banco_de_dados_{idiomaAtual}");
        if (arquivo != null)
        {
            Wrapper wrapper = JsonUtility.FromJson<Wrapper>(arquivo.text);
            if (wrapper.titulos != null)
            {
                tituloTitulosUI.text = wrapper.titulos.titulo_cena;
                subtituloTitulosUI.text = wrapper.titulos.subtitulo;
                holderTitulosUI.text = wrapper.titulos.holder;
            }
            CarregarBancoDeDados.Problemas = new List<Problema>(wrapper.problemas);
            Pesquisar();
        }
        else
        {
            Debug.LogError($"Arquivo banco_de_dados_{idiomaAtual}.json não encontrado.");
        }
        
        // Garante que o painel fecha ao trocar de idioma
        PainelDeslizante painelScript = painelDetalhes.GetComponent<PainelDeslizante>();
        if (painelScript != null) painelScript.FecharInstantaneamente();
        else painelDetalhes.SetActive(false);
    }

    public void Pesquisar()
    {
        foreach (Transform filho in listaDeResultados)
        {
            if (filho != templateProblema.transform) Destroy(filho.gameObject);
        }

        if (CarregarBancoDeDados.Problemas == null) return;

        string termo = campoDePesquisa.text.ToLower();

        List<Problema> resultados = string.IsNullOrEmpty(termo)
            ? CarregarBancoDeDados.Problemas
            : CarregarBancoDeDados.Problemas
                .Where(p => p.titulo.ToLower().StartsWith(termo))
                .Concat(CarregarBancoDeDados.Problemas
                    .Where(p => p.titulo.ToLower().Contains(termo) && !p.titulo.ToLower().StartsWith(termo)))
                .ToList();

        foreach (var problema in resultados)
        {
            GameObject item = Instantiate(templateProblema, listaDeResultados);
            item.SetActive(true);
            item.GetComponentInChildren<TMP_Text>().text = problema.titulo;

            Button botao = item.GetComponent<Button>();
            botao.onClick.RemoveAllListeners();
            botao.onClick.AddListener(() => MostrarDetalhes(problema));
        }
    }

    void MostrarDetalhes(Problema problema)
    {
        problemaAtual = problema;

        tituloProblema.text = problema.titulo;
        descricaoProblema.text = problema.descricao;
        descricao.text = problema.descricao2;
        solucaoProblema.text = problema.solucao;
        solucao.text = problema.solucao2;
        textpasso.text = problema.botaopasso;

        PainelDeslizante painelScript = painelDetalhes.GetComponent<PainelDeslizante>();
        if (painelScript != null)
        {
            painelScript.Abrir();
        }
        else
        {
            painelDetalhes.SetActive(true);
        }
    }

    public void OcultarPainelDetalhes()
    {
        PainelDeslizante painelScript = painelDetalhes.GetComponent<PainelDeslizante>();
        
        if (painelScript != null)
        {
            painelScript.Fechar(); 
        }
        else
        {
            painelDetalhes.SetActive(false);
        }
    }

    public void VerPassoAPasso()
    {
        CarregarBancoDeDadosMontagem.GarantirBancoCarregado();
        
        if (ControleDeCena.Instance != null)
        {
            ControleDeCena.Instance.DefinirOrigem("problema");
        }

        if (ProblemaSelecionadoAR.Instance == null)
        {
            var selecionado = new GameObject("ProblemaSelecionadoAR");
            var holder = selecionado.AddComponent<ProblemaSelecionadoAR>();
            holder.idProblema = problemaAtual.id; 
            DontDestroyOnLoad(selecionado);
        }
        else
        {
            ProblemaSelecionadoAR.Instance.idProblema = problemaAtual.id;
        }

        SceneManager.LoadScene(nomeCenaARCanvas);
    }

    [System.Serializable]
    private class Wrapper
    {
        public TitulosUI titulos;
        public Problema[] problemas;
    }
}