using TMPro;
using UnityEngine;
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

    private IReadOnlyList<AlertaOficial> alertas = new List<AlertaOficial>();
    private AlertaOficial alertaAtual;

    private void Start()
    {
        campoDePesquisa.onSubmit.AddListener(_ => Pesquisar());
        campoDePesquisa.onValueChanged.AddListener(_ => Pesquisar());

        templateProblema.SetActive(false);
        AtualizarDisponibilidadeDoPassoAPasso(false);

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

            Button botao = item.GetComponent<Button>();
            botao.onClick.RemoveAllListeners();
            botao.onClick.AddListener(() => MostrarDetalhes(alerta));
        }
    }

    void MostrarDetalhes(AlertaOficial alerta)
    {
        alertaAtual = alerta;

        tituloProblema.text = $"{alerta.Codigo} - {alerta.Nome}";
        descricaoProblema.text = alerta.OQueE;
        descricao.text = alerta.QuandoOcorre;
        solucaoProblema.text = string.Join("\n", alerta.Acoes);
        solucao.text = string.Join("\n", alerta.Locais);

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
        if (!PossuiVinculoEstruturalComExperienciaAr(alertaAtual)) return;
    }

    #region MARK: Vinculo com experiencia AR

    private static bool PossuiVinculoEstruturalComExperienciaAr(AlertaOficial alerta)
    {
        return false;
    }

    private void AtualizarDisponibilidadeDoPassoAPasso(bool disponivel)
    {
        if (textpasso == null) return;

        Button botao = textpasso.GetComponentInParent<Button>();
        if (botao != null) botao.gameObject.SetActive(disponivel);
    }

    #endregion
}
