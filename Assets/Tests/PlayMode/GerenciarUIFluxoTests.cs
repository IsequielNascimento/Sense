using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class GerenciarUIFluxoTests
{
    private const string TipoQualificado = "GerenciarUI, Assembly-CSharp";
    private const string ChaveIdioma = "idioma";
    private const string IdiomaDoTeste = "pt";
    private const string CodigoDoAlerta = "A9";
    private const string Separador = " - ";

    private static readonly BindingFlags MembroPublico = BindingFlags.Public | BindingFlags.Instance;

    private static readonly string[] CamposDeTexto =
    {
        "holderTitulosUI", "tituloProblema", "tituloTitulosUI", "subtituloTitulosUI",
        "descricao", "descricaoProblema", "solucao", "solucaoProblema", "textpasso",
    };

    private readonly Dictionary<string, TMP_Text> textos = new Dictionary<string, TMP_Text>();

    private bool idiomaEstavaDefinido;
    private string idiomaOriginal;

    private Type tipoGerenciarUI;
    private Component gerenciarUI;
    private GameObject raiz;
    private GameObject painelDetalhes;
    private GameObject templateProblema;
    private GameObject botaoPassoAPasso;
    private Transform listaDeResultados;
    private TMP_InputField campoDePesquisa;

    [SetUp]
    public void ForcarIdiomaDoTeste()
    {
        idiomaEstavaDefinido = PlayerPrefs.HasKey(ChaveIdioma);
        idiomaOriginal = PlayerPrefs.GetString(ChaveIdioma, string.Empty);

        PlayerPrefs.SetString(ChaveIdioma, IdiomaDoTeste);
    }

    [TearDown]
    public void RestaurarIdiomaEDesmontarCena()
    {
        if (raiz != null) UnityEngine.Object.DestroyImmediate(raiz);

        raiz = null;
        textos.Clear();

        if (idiomaEstavaDefinido) PlayerPrefs.SetString(ChaveIdioma, idiomaOriginal);
        else PlayerPrefs.DeleteKey(ChaveIdioma);

        PlayerPrefs.Save();
    }

    [UnityTest]
    public IEnumerator FluxoDeAlertas_ListaBuscaA9AbreDetalheERetorna()
    {
        MontarCena();

        yield return null;

        Assert.That(templateProblema.activeSelf, Is.False, "Start deveria ocultar o template.");
        Assert.That(painelDetalhes.activeSelf, Is.False, "Start deveria fechar o painel de detalhes.");
        Assert.That(botaoPassoAPasso.activeSelf, Is.False, "O botao da experiencia AR deveria iniciar oculto.");

        int totalInicial = Resultados().Count;
        Assert.That(totalInicial, Is.EqualTo(24), "A lista deveria exibir exatamente os 24 alertas oficiais.");

        AlertaOficial alertaA9 = CatalogoDeAlertas.Carregar(IdiomaDoTeste)
            .Single(alerta => alerta.Codigo == CodigoDoAlerta);
        string rotuloEsperado = $"{alertaA9.Codigo}{Separador}{alertaA9.Nome}";

        campoDePesquisa.text = CodigoDoAlerta;

        yield return null;

        Assert.That(Resultados().Count, Is.EqualTo(1), $"A busca por '{CodigoDoAlerta}' deveria retornar um alerta.");

        Transform itemPorCodigo = LocalizarAlerta();
        Assert.That(itemPorCodigo, Is.Not.Null, $"A busca pelo codigo '{CodigoDoAlerta}' nao retornou o alerta.");
        Assert.That(Rotulo(itemPorCodigo), Is.EqualTo(rotuloEsperado));

        campoDePesquisa.text = alertaA9.Nome;

        yield return null;

        Transform itemPorNome = LocalizarAlerta();
        Assert.That(Resultados().Count, Is.EqualTo(1), "A busca pelo nome oficial deveria retornar um alerta.");
        Assert.That(itemPorNome, Is.Not.Null, $"A busca pelo nome '{alertaA9.Nome}' nao retornou o alerta {CodigoDoAlerta}.");
        Assert.That(Rotulo(itemPorNome), Is.EqualTo(rotuloEsperado));
        Assert.That(textos["tituloProblema"].text, Is.Empty, "O detalhe nao deveria estar preenchido antes do clique.");

        itemPorNome.GetComponent<Button>().onClick.Invoke();

        DetalheDeAlertaFormatado detalhe = FormatadorDeDetalheDeAlerta.Formatar(alertaA9);

        Assert.That(painelDetalhes.activeSelf, Is.True, "O clique deveria abrir o painel de detalhes.");
        Assert.That(textos["tituloProblema"].text, Is.EqualTo(detalhe.Titulo));
        Assert.That(textos["descricaoProblema"].text, Is.EqualTo(detalhe.Descricao));
        Assert.That(textos["solucaoProblema"].text, Is.EqualTo(detalhe.Solucao));
        Assert.That(detalhe.Descricao, Does.Contain(FormatadorDeDetalheDeAlerta.RotuloOQueE));
        Assert.That(detalhe.Descricao, Does.Contain(FormatadorDeDetalheDeAlerta.RotuloQuandoOcorre));
        Assert.That(detalhe.Solucao, Does.Contain(FormatadorDeDetalheDeAlerta.RotuloOQueFazer));
        Assert.That(detalhe.Solucao, Does.Contain(FormatadorDeDetalheDeAlerta.RotuloOnde));
        Assert.That(detalhe.Solucao, Does.Contain(FormatadorDeDetalheDeAlerta.RotuloPadrao));
        Assert.That(detalhe.Solucao, Does.Contain("Manual, p. 5"));
        Assert.That(
            botaoPassoAPasso.activeSelf,
            Is.True,
            $"O detalhe do alerta {CodigoDoAlerta} deveria liberar o botao de ver passo a passo.");

        Invocar("OcultarPainelDetalhes");

        yield return null;

        Assert.That(painelDetalhes.activeSelf, Is.False, "O retorno deveria fechar o painel de detalhes.");
        Assert.That(LocalizarAlerta(), Is.Not.Null, "A lista deveria continuar disponivel apos o retorno.");
    }

    private void MontarCena()
    {
        tipoGerenciarUI = Type.GetType(TipoQualificado);
        Assert.That(tipoGerenciarUI, Is.Not.Null, $"Tipo '{TipoQualificado}' nao encontrado.");

        raiz = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
        raiz.SetActive(false);
        raiz.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        campoDePesquisa = CriarCampoDePesquisa(raiz.transform);
        painelDetalhes = CriarFilho("PainelDetalhes", raiz.transform);
        listaDeResultados = CriarFilho("ListaDeResultados", raiz.transform).transform;
        templateProblema = CriarTemplate(listaDeResultados);
        botaoPassoAPasso = CriarFilho("BotaoPassoAPasso", raiz.transform);
        botaoPassoAPasso.AddComponent<Button>();

        foreach (string nome in CamposDeTexto)
        {
            Transform pai = nome == "textpasso" ? botaoPassoAPasso.transform : raiz.transform;
            textos[nome] = CriarTexto(nome, pai);
        }

        gerenciarUI = raiz.AddComponent(tipoGerenciarUI);

        DefinirCampo("campoDePesquisa", campoDePesquisa);
        DefinirCampo("painelDetalhes", painelDetalhes);
        DefinirCampo("templateProblema", templateProblema);
        DefinirCampo("listaDeResultados", listaDeResultados);

        foreach (KeyValuePair<string, TMP_Text> texto in textos)
        {
            DefinirCampo(texto.Key, texto.Value);
        }

        raiz.SetActive(true);
    }

    private void DefinirCampo(string nome, object valor)
    {
        FieldInfo campo = tipoGerenciarUI.GetField(nome, MembroPublico);

        Assert.That(campo, Is.Not.Null, $"GerenciarUI nao expoe o campo publico '{nome}'.");

        campo.SetValue(gerenciarUI, valor);
    }

    private void Invocar(string nome)
    {
        MethodInfo metodo = tipoGerenciarUI.GetMethod(nome, MembroPublico, null, Type.EmptyTypes, null);

        Assert.That(metodo, Is.Not.Null, $"GerenciarUI nao expoe o metodo publico '{nome}()'.");

        metodo.Invoke(gerenciarUI, null);
    }

    private List<Transform> Resultados()
    {
        var resultados = new List<Transform>();

        foreach (Transform filho in listaDeResultados)
        {
            if (filho == templateProblema.transform) continue;
            if (!filho.gameObject.activeSelf) continue;

            resultados.Add(filho);
        }

        return resultados;
    }

    private Transform LocalizarAlerta()
    {
        foreach (Transform item in Resultados())
        {
            if (Rotulo(item).StartsWith(CodigoDoAlerta + Separador, StringComparison.Ordinal)) return item;
        }

        return null;
    }

    private static string Rotulo(Transform item)
    {
        TMP_Text texto = item.GetComponentInChildren<TMP_Text>();

        return texto == null ? string.Empty : texto.text;
    }

    private static GameObject CriarFilho(string nome, Transform pai)
    {
        var filho = new GameObject(nome, typeof(RectTransform));
        filho.transform.SetParent(pai, false);

        return filho;
    }

    private static TMP_Text CriarTexto(string nome, Transform pai)
    {
        TMP_Text texto = CriarFilho(nome, pai).AddComponent<TextMeshProUGUI>();
        texto.text = string.Empty;

        return texto;
    }

    private static TMP_InputField CriarCampoDePesquisa(Transform pai)
    {
        GameObject alvo = CriarFilho("CampoDePesquisa", pai);
        alvo.SetActive(false);

        TMP_Text visor = CriarTexto("Text", alvo.transform);

        TMP_InputField campo = alvo.AddComponent<TMP_InputField>();
        campo.textViewport = (RectTransform)alvo.transform;
        campo.textComponent = visor;
        campo.text = string.Empty;

        alvo.SetActive(true);

        return campo;
    }

    private static GameObject CriarTemplate(Transform lista)
    {
        GameObject template = CriarFilho("TemplateProblema", lista);
        template.AddComponent<TextMeshProUGUI>();
        template.AddComponent<Button>();

        return template;
    }
}
