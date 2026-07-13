using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;

public sealed class DisplayM4 : MonoBehaviour
{
    private const string CaminhoFonteLCD = "M4Display/DSEG14Classic SDF";
    private const string CaminhoMaterialLCD = "M4Display/DSEG14Classic M4 LCD";

    [SerializeField] private TMP_Text texto;

    [Header("Layout composto")]
    [SerializeField] private TMP_Text textoPercentual;
    [SerializeField] private TMP_Text textoOperacao;
    [SerializeField] private TMP_Text textoBargraph;
    [SerializeField, Range(3, 10)] private int quantidadeSegmentos = 6;
    [SerializeField, Min(0f)] private float duracaoAnimacaoBargraph = 1.2f;

    [Header("Estilo LCD")]
    [SerializeField] private TMP_FontAsset fonteLCD;
    [SerializeField] private Material materialPresetLCD;

    [Header("Fundo do LCD")]
    [SerializeField] private Renderer fundoLCD;
    [SerializeField] private Color corFundoLCD = new Color32(150, 150, 150, 255);

    private MaterialPropertyBlock propriedadesDoFundo;
    private SpriteRenderer[] spritesLegados;
    private GameObject[] canvasesLegados;
    private Coroutine animacaoBargraph;
    private float progressoAtual;

    public bool EstaConfigurado
    {
        get
        {
            ResolverReferencia();
            return texto != null;
        }
    }

    private void Awake()
    {
        Preparar();
    }

    private void LateUpdate()
    {
        if ((textoOperacao != null && textoOperacao.gameObject.activeInHierarchy) ||
            (textoPercentual != null && textoPercentual.gameObject.activeInHierarchy) ||
            (textoBargraph != null && textoBargraph.gameObject.activeInHierarchy))
        {
            DesativarDisplaysLegados();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ResolverReferencia();
        ResolverFundoLCD();
        AplicarEstiloLCD();
        AplicarFundoLCD();
    }
#endif

    public void Mostrar(string valor)
    {
        ResolverReferencia();

        if (texto == null)
        {
            Debug.LogError("[DisplayM4] Nenhum componente TMP_Text foi configurado.", this);
            return;
        }

        string textoNormalizado = (valor ?? string.Empty).Trim().ToUpperInvariant();

        if (textoNormalizado.Length == 0)
        {
            Limpar();
            return;
        }

        GarantirLayoutComposto();
        AplicarEstiloLCD();

        string[] linhas = textoNormalizado.Split('\n');
        if (linhas.Length >= 2 && TentarLerPercentual(linhas[0], out float percentual))
        {
            MostrarCalibracao(percentual, linhas[1], true);
        }
        else if (linhas.Length >= 2)
        {
            PararAnimacaoBargraph();
            MostrarDuasLinhas(linhas[0], linhas[1]);
        }
        else
        {
            PararAnimacaoBargraph();
            MostrarTextoCentral(textoNormalizado);
        }

        AplicarFundoLCD();
        DesativarDisplaysLegados();
    }

    public void MostrarCalibracao(float percentual, string operacao, bool animar = true)
    {
        GarantirLayoutComposto();
        AplicarEstiloLCD();

        textoOperacao.gameObject.SetActive(true);
        textoOperacao.text = (operacao ?? string.Empty).Trim().ToUpperInvariant();
        ConfigurarRect(textoOperacao, new Vector2(20f, 2.8f), new Vector2(0f, -0.05f), 13f, 27f);

        textoPercentual.gameObject.SetActive(true);
        textoBargraph.gameObject.SetActive(true);
        ConfigurarRect(textoPercentual, new Vector2(20f, 2.5f), new Vector2(0f, 1.75f), 11f, 24f);
        ConfigurarRect(textoBargraph, new Vector2(20f, 1.8f), new Vector2(0f, -1.9f), 10f, 24f);

        float destino = Mathf.Clamp(percentual, 0f, 100f);
        PararAnimacaoBargraph();

        if (animar && duracaoAnimacaoBargraph > 0f && !Mathf.Approximately(progressoAtual, destino))
        {
            animacaoBargraph = StartCoroutine(AnimarBargraph(progressoAtual, destino));
        }
        else
        {
            AtualizarCalibracao(destino);
        }
    }

    public void Limpar()
    {
        ResolverReferencia();

        if (texto == null) return;

        PararAnimacaoBargraph();

        foreach (TMP_Text campo in CamposDoDisplay())
        {
            if (campo == null) continue;
            campo.text = string.Empty;
            campo.gameObject.SetActive(false);
        }
    }

    public void Preparar()
    {
        ResolverReferencia();
        GarantirLayoutComposto();
        ResolverFundoLCD();
        AplicarEstiloLCD();
        AplicarFundoLCD();
        DesativarDisplaysLegados();
    }

    public static DisplayM4 LocalizarOuCriar(Transform raiz)
    {
        if (raiz == null) return null;

        DisplayM4[] controladores = raiz.GetComponentsInChildren<DisplayM4>(true);
        foreach (DisplayM4 controlador in controladores)
        {
            controlador.ResolverReferencia();
            if (controlador.texto != null)
            {
                controlador.Preparar();
                return controlador;
            }
        }

        TMP_Text textoEncontrado = LocalizarTextoNaRaiz(raiz);
        if (textoEncontrado == null)
        {
            if (controladores.Length == 0)
            {
                DisplayM4 controladorDeFundo = raiz.gameObject.AddComponent<DisplayM4>();
                controladorDeFundo.Preparar();
                return controladorDeFundo;
            }

            controladores[0].Preparar();
            return controladores[0];
        }

        DisplayM4 novoControlador = controladores.Length > 0
            ? controladores[0]
            : textoEncontrado.gameObject.AddComponent<DisplayM4>();

        novoControlador.texto = textoEncontrado;
        novoControlador.Preparar();
        return novoControlador;
    }

    private void ResolverReferencia()
    {
        if (texto != null) return;

        texto = GetComponent<TMP_Text>();
        if (texto == null)
        {
            texto = GetComponentInChildren<TMP_Text>(true);
        }

        if (texto == null && transform.parent != null)
        {
            texto = transform.parent.GetComponentInChildren<TMP_Text>(true);
        }

        if (texto == null)
        {
            texto = LocalizarTextoNaRaiz(transform.root);
        }
    }

    private static TMP_Text LocalizarTextoNaRaiz(Transform raiz)
    {
        TMP_Text textoSobTela = null;

        foreach (TMP_Text candidato in raiz.GetComponentsInChildren<TMP_Text>(true))
        {
            if (PossuiAncestralComNome(candidato.transform, "DisplayDynamic"))
            {
                return candidato;
            }

            if (textoSobTela == null && PossuiAncestralComNome(candidato.transform, "TELA"))
            {
                textoSobTela = candidato;
            }
        }

        return textoSobTela;
    }

    private static bool PossuiAncestralComNome(Transform candidato, string nome)
    {
        for (Transform atual = candidato; atual != null; atual = atual.parent)
        {
            if (atual.name == nome) return true;
        }

        return false;
    }

    private void GarantirLayoutComposto()
    {
        if (texto == null) return;

        textoOperacao ??= texto;
        Transform pai = texto.transform.parent;

        textoPercentual ??= EncontrarCampo(pai, "DisplayPercentual");
        textoBargraph ??= EncontrarCampo(pai, "DisplayBargraph");

        if (textoPercentual == null)
        {
            textoPercentual = CriarCampo("DisplayPercentual", pai);
        }

        if (textoBargraph == null)
        {
            textoBargraph = CriarCampo("DisplayBargraph", pai);
        }

        textoOperacao.gameObject.name = "DisplayOperacao";
    }

    private TMP_Text CriarCampo(string nome, Transform pai)
    {
        TMP_Text campo = Instantiate(texto, pai, false);
        campo.gameObject.name = nome;
        campo.text = string.Empty;
        campo.gameObject.SetActive(false);
        return campo;
    }

    private static TMP_Text EncontrarCampo(Transform pai, string nome)
    {
        if (pai == null) return null;

        Transform encontrado = pai.Find(nome);
        return encontrado != null ? encontrado.GetComponent<TMP_Text>() : null;
    }

    private void MostrarTextoCentral(string valor)
    {
        progressoAtual = 0f;
        textoPercentual.gameObject.SetActive(false);
        textoBargraph.gameObject.SetActive(false);
        textoOperacao.gameObject.SetActive(true);
        textoOperacao.text = valor;
        ConfigurarRect(textoOperacao, new Vector2(20f, 5f), Vector2.zero, 16f, 30f);
    }

    private void MostrarDuasLinhas(string superior, string operacao)
    {
        progressoAtual = 0f;
        textoBargraph.gameObject.SetActive(false);

        textoPercentual.gameObject.SetActive(true);
        textoPercentual.text = superior;
        ConfigurarRect(textoPercentual, new Vector2(20f, 2.5f), new Vector2(0f, 1.35f), 11f, 24f);

        textoOperacao.gameObject.SetActive(true);
        textoOperacao.text = operacao;
        ConfigurarRect(textoOperacao, new Vector2(20f, 3f), new Vector2(0f, -0.8f), 13f, 28f);
    }

    private static void ConfigurarRect(
        TMP_Text campo,
        Vector2 tamanho,
        Vector2 posicao,
        float tamanhoMinimo,
        float tamanhoMaximo)
    {
        RectTransform rect = campo.rectTransform;
        rect.sizeDelta = tamanho;
        rect.anchoredPosition = posicao;
        campo.enableAutoSizing = true;
        campo.fontSizeMin = tamanhoMinimo;
        campo.fontSizeMax = tamanhoMaximo;
        campo.alignment = TextAlignmentOptions.Center;
        campo.enableWordWrapping = false;
        campo.overflowMode = TextOverflowModes.Truncate;
    }

    private static bool TentarLerPercentual(string valor, out float percentual)
    {
        string numero = valor.Replace("%", string.Empty).Replace(',', '.').Trim();
        return float.TryParse(numero, NumberStyles.Float, CultureInfo.InvariantCulture, out percentual);
    }

    private IEnumerator AnimarBargraph(float inicio, float destino)
    {
        float tempo = 0f;

        while (tempo < duracaoAnimacaoBargraph)
        {
            tempo += Time.deltaTime;
            float t = duracaoAnimacaoBargraph <= 0f ? 1f : Mathf.Clamp01(tempo / duracaoAnimacaoBargraph);
            AtualizarCalibracao(Mathf.Lerp(inicio, destino, t));
            yield return null;
        }

        AtualizarCalibracao(destino);
        animacaoBargraph = null;
    }

    private void AtualizarCalibracao(float percentual)
    {
        progressoAtual = Mathf.Clamp(percentual, 0f, 100f);
        textoPercentual.text = $"{progressoAtual:0.0}%";

        int segmentosAtivos = Mathf.Clamp(
            Mathf.CeilToInt(progressoAtual / 100f * quantidadeSegmentos),
            0,
            quantidadeSegmentos);

        textoBargraph.text = new string('|', segmentosAtivos)
            .PadRight(quantidadeSegmentos, ' ');
    }

    private void PararAnimacaoBargraph()
    {
        if (animacaoBargraph == null) return;

        StopCoroutine(animacaoBargraph);
        animacaoBargraph = null;
    }

    private IEnumerable<TMP_Text> CamposDoDisplay()
    {
        yield return textoOperacao ?? texto;
        yield return textoPercentual;
        yield return textoBargraph;
    }

    private void AplicarEstiloLCD()
    {
        if (texto == null) return;

        if (fonteLCD == null)
        {
            fonteLCD = Resources.Load<TMP_FontAsset>(CaminhoFonteLCD);
        }

        if (materialPresetLCD == null)
        {
            materialPresetLCD = Resources.Load<Material>(CaminhoMaterialLCD);
        }

        foreach (TMP_Text campo in CamposDoDisplay())
        {
            if (campo == null) continue;

            if (fonteLCD != null)
            {
                campo.font = fonteLCD;
            }

            if (materialPresetLCD != null)
            {
                campo.fontSharedMaterial = materialPresetLCD;
            }

            campo.color = new Color32(8, 8, 8, 255);
            campo.fontStyle = FontStyles.Normal;
            campo.characterSpacing = 0f;
            campo.lineSpacing = -10f;
        }
    }

    private void ResolverFundoLCD()
    {
        if (fundoLCD != null) return;

        foreach (Renderer candidato in transform.root.GetComponentsInChildren<Renderer>(true))
        {
            if (candidato.name == "TELA")
            {
                fundoLCD = candidato;
                break;
            }
        }
    }

    private void AplicarFundoLCD()
    {
        if (fundoLCD == null) return;

        propriedadesDoFundo ??= new MaterialPropertyBlock();
        fundoLCD.GetPropertyBlock(propriedadesDoFundo);
        propriedadesDoFundo.SetColor("_BaseColor", corFundoLCD);
        propriedadesDoFundo.SetColor("_Color", corFundoLCD);
        fundoLCD.SetPropertyBlock(propriedadesDoFundo);
    }

    private void DesativarDisplaysLegados()
    {
        ResolverDisplaysLegados();

        foreach (SpriteRenderer sprite in spritesLegados)
        {
            if (sprite == null) continue;
            sprite.enabled = false;
            sprite.gameObject.SetActive(false);
        }

        foreach (GameObject canvas in canvasesLegados)
        {
            if (canvas != null) canvas.SetActive(false);
        }
    }

    private void ResolverDisplaysLegados()
    {
        if (spritesLegados != null && canvasesLegados != null) return;

        Transform raiz = transform.root;
        var sprites = new List<SpriteRenderer>();
        var canvases = new List<GameObject>();

        foreach (SpriteRenderer sprite in raiz.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sprite.name == "Square") sprites.Add(sprite);
        }

        foreach (Transform candidato in raiz.GetComponentsInChildren<Transform>(true))
        {
            if (candidato.name == "Canvas Display") canvases.Add(candidato.gameObject);
        }

        spritesLegados = sprites.ToArray();
        canvasesLegados = canvases.ToArray();
    }
}
