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
    public GameObject backgroundBlur;
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

    private CanvasGroup blurCanvasGroup;
    private string idiomaAtual = "pt";
    private Problema problemaAtual;

    private void Start()
    {
        campoDePesquisa.onSubmit.AddListener(_ => Pesquisar());
        campoDePesquisa.onValueChanged.AddListener(_ => Pesquisar());

        templateProblema.SetActive(false);
        painelDetalhes.SetActive(false);

        if (backgroundBlur != null)
        {
            blurCanvasGroup = backgroundBlur.GetComponent<CanvasGroup>();
            if (blurCanvasGroup != null) blurCanvasGroup.alpha = 0f;
            backgroundBlur.SetActive(false);
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
        painelDetalhes.SetActive(false);
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

        painelDetalhes.SetActive(true);

        // --- CORREÇÃO: Chama o script novo para ABRIR ---
        PainelDeslizante painelScript = painelDetalhes.GetComponent<PainelDeslizante>();
        if (painelScript != null)
        {
            painelScript.Abrir();
        }

        if (backgroundBlur != null)
        {
            StopCoroutine("FadeIn");
            StartCoroutine(FadeIn(0.4f));
        }
    }

    // --- CORREÇÃO CRÍTICA AQUI ---
    // O seu arquivo antigo chamava 'FecharPopup()'. 
    // Agora chamamos o painelScript.Fechar() que usa as âncoras corretas.
    public void OcultarPainelDetalhes()
    {
        PainelDeslizante painelScript = painelDetalhes.GetComponent<PainelDeslizante>();
        
        if (painelScript != null)
        {
            painelScript.Fechar(); // Manda fechar usando animação de âncora
        }
        else
        {
            painelDetalhes.SetActive(false);
        }

        if (backgroundBlur != null)
        {
            StopCoroutine("FadeOut");
            StartCoroutine(FadeOut(0.4f));
        }
    }

    public void VerPassoAPasso()
    {
        CarregarBancoDeDadosMontagem.GarantirBancoCarregado();
        var selecionado = new GameObject("ProblemaSelecionadoAR");
        var holder = selecionado.AddComponent<ProblemaSelecionadoAR>();
        holder.idProblema = problemaAtual.id; 
        DontDestroyOnLoad(selecionado);
        ProblemaSelecionadoAR.Instance.idProblema = problemaAtual.id;
        SceneManager.LoadScene("AR_Cena_UIToolkit");
    }

    IEnumerator FadeIn(float duracao)
    {
        backgroundBlur.SetActive(true);
        float tempo = 0f;
        while (tempo < duracao)
        {
            if (blurCanvasGroup != null)
                blurCanvasGroup.alpha = Mathf.Lerp(0f, 1f, tempo / duracao);
            tempo += Time.deltaTime;
            yield return null;
        }
        if (blurCanvasGroup != null) blurCanvasGroup.alpha = 1f;
    }

    IEnumerator FadeOut(float duracao)
    {
        float tempo = 0f;
        float inicio = (blurCanvasGroup != null) ? blurCanvasGroup.alpha : 1f;
        while (tempo < duracao)
        {
            if (blurCanvasGroup != null)
                blurCanvasGroup.alpha = Mathf.Lerp(inicio, 0f, tempo / duracao);
            tempo += Time.deltaTime;
            yield return null;
        }
        if (blurCanvasGroup != null) blurCanvasGroup.alpha = 0f;
        backgroundBlur.SetActive(false);
    }

    [System.Serializable]
    private class Wrapper
    {
        public TitulosUI titulos;
        public Problema[] problemas;
    }
}