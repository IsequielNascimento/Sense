using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

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

    [Header("Cor por status da animação")]
    public Color corAnimacaoPendente = new Color(0.62f, 0.62f, 0.62f, 1f);

    private IReadOnlyList<AlertaOficial> alertas = new List<AlertaOficial>();
    private AlertaOficial alertaAtual;

    private void Start()
    {
        campoDePesquisa.onSubmit.AddListener(_ => Pesquisar());
        campoDePesquisa.onValueChanged.AddListener(_ => Pesquisar());

        templateProblema.SetActive(false);
        AtualizarDisponibilidadeDoPassoAPasso(false);

        HabilitarAutoSizing(descricaoProblema);
        HabilitarAutoSizing(solucaoProblema);

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
        TitulosUIBanco banco = LocalizedDatabase.Load<TitulosUIBanco>(LocalizedDatabase.ProblemasPath);
        if (banco.titulos != null)
        {
            tituloTitulosUI.text = banco.titulos.titulo_cena;
            subtituloTitulosUI.text = banco.titulos.subtitulo;
            holderTitulosUI.text = banco.titulos.holder;
        }

        alertas = CatalogoDeAlertas.Carregar(LocalizedDatabase.CurrentLanguage);
        Pesquisar();

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

        IReadOnlyList<AlertaOficial> resultados = BuscaDeAlertas.Buscar(alertas, campoDePesquisa.text);

        foreach (AlertaOficial alerta in resultados)
        {
            GameObject item = Instantiate(templateProblema, listaDeResultados);
            item.SetActive(true);
            item.GetComponentInChildren<TMP_Text>().text = $"{alerta.Codigo} - {alerta.Nome}";

            AplicarStatusDaAnimacao(item, alerta);

            Button botao = item.GetComponent<Button>();
            botao.onClick.RemoveAllListeners();
            botao.onClick.AddListener(() => MostrarDetalhes(alerta));
        }
    }

    private void AplicarStatusDaAnimacao(GameObject item, AlertaOficial alerta)
    {
        if (AnimacoesDeAlertaConcluidas.EstaConcluida(alerta.Codigo)) return;

        Image fundo = item.GetComponent<Image>();
        if (fundo != null) fundo.color = corAnimacaoPendente;
    }

    void MostrarDetalhes(AlertaOficial alerta)
    {
        alertaAtual = alerta;

        DetalheDeAlertaFormatado detalhe = FormatadorDeDetalheDeAlerta.Formatar(alerta, LocalizedDatabase.CurrentLanguage);

        tituloProblema.text = detalhe.Titulo;
        descricaoProblema.text = detalhe.Descricao;
        solucaoProblema.text = detalhe.Solucao;

        AtualizarDisponibilidadeDoPassoAPasso(alerta != null);

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
        if (alertaAtual == null) return;

        ControleDeCena.Instance?.DefinirOrigem(OrigemCena.Problema);

        ProblemaSelecionadoAR selecao = ProblemaSelecionadoAR.ObterOuCriar();

        if (alertaAtual.PossuiCenarioAnimado)
        {
            selecao.Selecionar(alertaAtual.ScenarioResourceKey, alertaAtual.ScenarioResourceKey);
        }
        else
        {
            selecao.SelecionarAlertaOficial(alertaAtual.Codigo);
        }

        SceneManager.LoadScene(Scenes.ArUiToolkit);
    }

    private static void HabilitarAutoSizing(TMP_Text campo)
    {
        campo.fontSizeMax = campo.fontSize;
        campo.enableAutoSizing = true;
    }

    #region MARK: Botao da experiencia AR

    private void AtualizarDisponibilidadeDoPassoAPasso(bool disponivel)
    {
        if (textpasso == null) return;

        Button botao = textpasso.GetComponentInParent<Button>(true);
        if (botao != null) botao.gameObject.SetActive(disponivel);
    }

    #endregion
}
