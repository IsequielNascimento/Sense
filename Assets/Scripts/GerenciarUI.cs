using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement; 

public class GerenciarUI : MonoBehaviour
{
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

    private List<Problema> problemas = new List<Problema>();
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

        TrocarIdioma(null);
    }

    public void TrocarIdioma(string novoIdioma)
    {
        BancoProblemas banco = LocalizedDatabase.Load<BancoProblemas>(LocalizedDatabase.ProblemasPath);
        if (banco.titulos != null)
        {
            tituloTitulosUI.text = banco.titulos.titulo_cena;
            subtituloTitulosUI.text = banco.titulos.subtitulo;
            holderTitulosUI.text = banco.titulos.holder;
        }

        problemas = new List<Problema>(banco.problemas ?? new Problema[0]);
        Pesquisar();
        
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

        string termo = campoDePesquisa.text.ToLower();

        List<Problema> resultados = string.IsNullOrEmpty(termo)
            ? problemas
            : problemas
                .Where(p => p.titulo.ToLower().StartsWith(termo))
                .Concat(problemas
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
        if (ControleDeCena.Instance != null)
        {
            ControleDeCena.Instance.DefinirOrigem(OrigemCena.Problema);
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

        SceneManager.LoadScene(Scenes.ArUiToolkit);
    }

}
