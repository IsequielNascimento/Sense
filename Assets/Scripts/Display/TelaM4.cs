using System.Collections;
using TMPro;
using UnityEngine;

public class TelaM4 : MonoBehaviour
{
    private TextMeshPro textoPrincipal;

    private GameObject barraContainer;
    private SpriteRenderer[] segmentosBarra;

    private SpriteRenderer led;
    private ControladorLedsM4 controladorLeds;

    private GameObject alertaContainer;
    private TextMeshPro alertaTexto;

    private GameObject anguloContainer;
    private TextMeshPro anguloTexto;

    private Coroutine rotinaProgresso;
    private Coroutine rotinaPiscar;

    private static Sprite spriteBranco;
    private static Sprite spriteCirculo;

    private static TMP_FontAsset fonteLcdAsset;
    private static Material materialLcd;
    private static bool fonteLcdCarregada;

    private const string CaminhoFonteLcd = "M4Display/DSEG14Classic SDF";
    private const string CaminhoMaterialLcd = "M4Display/DSEG14Classic M4 LCD";

    private static readonly Color CorTexto = Color.white;
    private static readonly Color CorTextoLcd = new Color32(8, 8, 8, 255);
    private static readonly Color CorBarra = new Color32(18, 18, 18, 255);
    private static readonly Color CorAlertaFundo = new Color(0.72f, 0.11f, 0.11f, 0.92f);
    private static readonly Color CorAnguloFundo = new Color(1f, 1f, 1f, 0.95f);
    private static readonly Color CorLedVerde = new Color(0.2f, 0.9f, 0.25f);
    private static readonly Color CorLedLaranja = new Color(1f, 0.38f, 0.04f);
    private static readonly Color CorLedVermelho = new Color(0.95f, 0.15f, 0.12f);

    public const int QuantidadeSegmentosBarra = 10;

    public static TelaM4 CriarEm(SpriteRenderer display)
    {
        if (display == null)
        {
            Debug.LogError("[TelaM4] 'display' não pode ser nulo.");
            return null;
        }

        Vector2 dimensoes = display.sprite != null
            ? (Vector2)display.sprite.bounds.size
            : new Vector2(0.1f, 0.06f);

        return CriarEm(display.transform, dimensoes, display.sortingLayerID, display.sortingOrder);
    }

    public static TelaM4 CriarEm(Transform ancora, Vector2 dimensoes, int sortingLayerID, int sortingOrderBase)
    {
        if (ancora == null)
        {
            Debug.LogError("[TelaM4] 'ancora' não pode ser nula.");
            return null;
        }

        var go = new GameObject("TelaM4 (dinâmica)");
        go.transform.SetParent(ancora, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        var tela = go.AddComponent<TelaM4>();
        tela.Construir(dimensoes, sortingLayerID, sortingOrderBase);
        return tela;
    }

    public static bool EtapaTemConteudo(Etapa etapa)
    {
        return etapa != null && (
            !string.IsNullOrEmpty(etapa.textoDisplay) ||
            !string.IsNullOrEmpty(etapa.alerta) ||
            !string.IsNullOrEmpty(etapa.leds) ||
            !string.IsNullOrEmpty(etapa.textoAngulo) ||
            etapa.progressoSegundos > 0f);
    }

    public void Aplicar(Etapa etapa)
    {
        PararRotinas();

        if (etapa == null)
        {
            LimparTudo();
            return;
        }

        bool temTexto = !string.IsNullOrEmpty(etapa.textoDisplay);
        textoPrincipal.gameObject.SetActive(temTexto);
        if (temTexto) textoPrincipal.text = etapa.textoDisplay;

        AplicarLed(etapa.leds);
        MostrarAlerta(etapa.alerta);

        bool temBarra = etapa.progressoSegundos > 0f;
        barraContainer.SetActive(temBarra);
        if (temBarra)
        {
            DefinirProgresso(0f);
            if (gameObject.activeInHierarchy)
            {
                rotinaProgresso = StartCoroutine(AnimarProgresso(etapa));
            }
        }

        bool temAngulo = !string.IsNullOrEmpty(etapa.textoAngulo);
        anguloContainer.SetActive(temAngulo);
        if (temAngulo) anguloTexto.text = etapa.textoAngulo;
    }

    /// <summary>
    /// Exibe um estado estático do display para inspeção e captura no Unity.
    /// Diferente de <see cref="Aplicar"/>, a barra não anima e permanece na
    /// fração informada.
    /// </summary>
    public void AplicarPreview(string textoDisplay, string estadoLeds, float progresso)
    {
        PararRotinas();

        bool temTexto = !string.IsNullOrEmpty(textoDisplay);
        textoPrincipal.gameObject.SetActive(temTexto);
        if (temTexto) textoPrincipal.text = textoDisplay;

        AplicarLed(estadoLeds);
        MostrarAlerta(null);

        barraContainer.SetActive(true);
        DefinirProgresso(progresso);

        anguloContainer.SetActive(false);
    }

    private void LimparTudo()
    {
        textoPrincipal.gameObject.SetActive(false);
        barraContainer.SetActive(false);
        led.gameObject.SetActive(false);
        alertaContainer.SetActive(false);
        anguloContainer.SetActive(false);
    }

    private void PararRotinas()
    {
        if (rotinaProgresso != null) { StopCoroutine(rotinaProgresso); rotinaProgresso = null; }
        if (rotinaPiscar != null) { StopCoroutine(rotinaPiscar); rotinaPiscar = null; }
        if (led != null) led.enabled = true;
    }

    private IEnumerator AnimarProgresso(Etapa etapa)
    {
        float tempo = 0f;
        while (tempo < etapa.progressoSegundos)
        {
            DefinirProgresso(tempo / etapa.progressoSegundos);
            tempo += Time.deltaTime;
            yield return null;
        }
        DefinirProgresso(1f);

        if (etapa.progressoEstoura)
        {
            string mensagem = string.IsNullOrEmpty(etapa.alertaTempoExcedido)
                ? TextoPadraoTempoExcedido(ObterIdioma())
                : etapa.alertaTempoExcedido;
            MostrarAlerta(mensagem);
            AplicarLed("vermelho_piscando");
        }
    }

    public void DefinirProgresso(float fracao)
    {
        int ativos = CalcularSegmentosAtivos(fracao);
        for (int i = 0; i < segmentosBarra.Length; i++)
        {
            segmentosBarra[i].enabled = i < ativos;
        }
    }

    public static int CalcularSegmentosAtivos(float fracao)
    {
        return Mathf.CeilToInt(Mathf.Clamp01(fracao) * QuantidadeSegmentosBarra);
    }

    private void AplicarLed(string estado)
    {
        if (rotinaPiscar != null) { StopCoroutine(rotinaPiscar); rotinaPiscar = null; }
        led.enabled = true;

        if (controladorLeds != null && controladorLeds.Aplicar(estado))
        {
            led.gameObject.SetActive(false);
            return;
        }

        if (string.IsNullOrEmpty(estado) || estado.ToLower() == "nenhum")
        {
            led.gameObject.SetActive(false);
            return;
        }

        string e = estado.ToLower();
        led.gameObject.SetActive(true);

        if (e.StartsWith("abert")) e = "verde";
        else if (e.StartsWith("fechad")) e = "laranja";
        else if (e.StartsWith("alert")) e = "vermelho_piscando";

        if (!e.StartsWith("verde") && !e.StartsWith("laranja") && !e.StartsWith("vermelho"))
        {
            Debug.LogWarning($"[TelaM4] Estado de LED '{estado}' não reconhecido; usando verde.");
        }

        if (e.StartsWith("vermelho")) led.color = CorLedVermelho;
        else if (e.StartsWith("laranja")) led.color = CorLedLaranja;
        else led.color = CorLedVerde;

        if (e.EndsWith("piscando") && gameObject.activeInHierarchy)
        {
            rotinaPiscar = StartCoroutine(PiscarLed());
        }
    }

    private IEnumerator PiscarLed()
    {
        while (true)
        {
            led.enabled = !led.enabled;
            yield return new WaitForSeconds(0.35f);
        }
    }

    private void MostrarAlerta(string mensagem)
    {
        bool tem = !string.IsNullOrEmpty(mensagem);
        alertaContainer.SetActive(tem);
        if (tem) alertaTexto.text = mensagem;
    }

    private void Construir(Vector2 dimensoes, int layerId, int ordemBase)
    {
        var layout = new TelaM4Layout(dimensoes);
        controladorLeds = GetComponentInParent<ControladorLedsM4>();

        textoPrincipal = CriarTexto("TextoPrincipal",
            layout.TextoPrincipalPosicao, layout.TextoPrincipalTamanho,
            CorTextoLcd, layerId, ordemBase + 3, fonteLcd: true);
        textoPrincipal.transform.localRotation = Quaternion.Euler(0f, 0f, layout.ConteudoRotacaoZ);

        barraContainer = new GameObject("BarraProgresso");
        barraContainer.transform.SetParent(transform, false);
        barraContainer.transform.localPosition = layout.BarraContainerPosicao;
        barraContainer.transform.localRotation = Quaternion.Euler(0f, 0f, layout.ConteudoRotacaoZ);
        ConstruirBarraSegmentada(layout, layerId, ordemBase + 2);
        DefinirProgresso(0f);

        led = CriarLed(layout.LedPosicao, layout.LedDiametro, layerId, ordemBase + 6);

        alertaContainer = new GameObject("Alerta");
        alertaContainer.transform.SetParent(transform, false);
        alertaContainer.transform.localPosition = layout.AlertaContainerPosicao;
        alertaContainer.transform.localRotation = Quaternion.Euler(0f, 0f, layout.ConteudoRotacaoZ);
        CriarRetangulo("Fundo", alertaContainer.transform, Vector3.zero,
            layout.AlertaFundoTamanho, CorAlertaFundo, layerId, ordemBase + 4);
        alertaTexto = CriarTexto("Texto",
            Vector3.zero, layout.AlertaTextoTamanho,
            CorTexto, layerId, ordemBase + 5, fonteLcd: false, pai: alertaContainer.transform);

        anguloContainer = new GameObject("CaixaAngulo");
        anguloContainer.transform.SetParent(transform, false);
        anguloContainer.transform.localPosition = layout.AnguloContainerPosicao;
        anguloContainer.transform.localRotation = Quaternion.Euler(0f, 0f, layout.ConteudoRotacaoZ);
        CriarRetangulo("Fundo", anguloContainer.transform, Vector3.zero,
            layout.AnguloFundoTamanho, CorAnguloFundo, layerId, ordemBase + 4);
        anguloTexto = CriarTexto("Texto",
            Vector3.zero, layout.AnguloTextoTamanho,
            CorTextoLcd, layerId, ordemBase + 5, fonteLcd: true, pai: anguloContainer.transform);

        LimparTudo();
    }

    private void ConstruirBarraSegmentada(TelaM4Layout layout, int layerId, int ordem)
    {
        segmentosBarra = new SpriteRenderer[QuantidadeSegmentosBarra];
        float passo = layout.BarraSegmentoLargura + layout.BarraEspacamento;
        float inicio = -layout.BarraLargura * 0.5f + layout.BarraSegmentoLargura * 0.5f;

        for (int i = 0; i < QuantidadeSegmentosBarra; i++)
        {
            segmentosBarra[i] = CriarRetangulo(
                $"Segmento {i + 1:00}",
                barraContainer.transform,
                new Vector3(inicio + passo * i, 0f, 0f),
                new Vector2(layout.BarraSegmentoLargura, layout.BarraAltura),
                CorBarra,
                layerId,
                ordem);
        }
    }

    private TextMeshPro CriarTexto(string nome, Vector3 posicaoLocal, Vector2 tamanho,
        Color cor, int layerId, int ordem, bool fonteLcd, Transform pai = null)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(pai != null ? pai : transform, false);
        go.transform.localPosition = posicaoLocal;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.rectTransform.sizeDelta = tamanho;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = cor;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 0.01f;
        tmp.fontSizeMax = 8f;
        tmp.textWrappingMode = fonteLcd ? TextWrappingModes.NoWrap : TextWrappingModes.Normal;
        tmp.overflowMode = TextOverflowModes.Truncate;
        tmp.maxVisibleLines = 2;

        if (fonteLcd)
        {
            CarregarFonteLcd();
            if (fonteLcdAsset != null) tmp.font = fonteLcdAsset;
            if (materialLcd != null) tmp.fontSharedMaterial = materialLcd;
        }

        var renderer = go.GetComponent<MeshRenderer>();
        renderer.sortingLayerID = layerId;
        renderer.sortingOrder = ordem;
        return tmp;
    }

    private SpriteRenderer CriarRetangulo(string nome, Transform pai, Vector3 posicaoLocal,
        Vector2 tamanho, Color cor, int layerId, int ordem)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        go.transform.localPosition = posicaoLocal;
        go.transform.localScale = new Vector3(tamanho.x, tamanho.y, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = ObterSpriteBranco();
        sr.color = cor;
        sr.sortingLayerID = layerId;
        sr.sortingOrder = ordem;
        return sr;
    }

    private SpriteRenderer CriarLed(Vector3 posicaoLocal, float diametro, int layerId, int ordem)
    {
        var go = new GameObject("Led");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = posicaoLocal;
        go.transform.localScale = new Vector3(diametro, diametro, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = ObterSpriteCirculo();
        sr.sortingLayerID = layerId;
        sr.sortingOrder = ordem;
        return sr;
    }

    private static void CarregarFonteLcd()
    {
        if (fonteLcdCarregada) return;
        fonteLcdCarregada = true;

        fonteLcdAsset = Resources.Load<TMP_FontAsset>(CaminhoFonteLcd);
        if (fonteLcdAsset == null)
        {
            Debug.LogError($"[TelaM4] Fonte LCD não encontrada em 'Resources/{CaminhoFonteLcd}'.");
        }

        materialLcd = Resources.Load<Material>(CaminhoMaterialLcd);
        if (materialLcd == null)
        {
            Debug.LogError($"[TelaM4] Material LCD não encontrado em 'Resources/{CaminhoMaterialLcd}'.");
        }
    }

    private static Sprite ObterSpriteBranco()
    {
        if (spriteBranco == null)
        {
            var tex = Texture2D.whiteTexture;
            spriteBranco = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), tex.width);
            spriteBranco.name = "TelaM4_Branco";
        }
        return spriteBranco;
    }

    private static Sprite ObterSpriteCirculo()
    {
        if (spriteCirculo == null)
        {
            const int tam = 64;
            var tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
            float raio = tam * 0.5f - 1f;
            Vector2 centro = new Vector2(tam * 0.5f, tam * 0.5f);
            var pixels = new Color32[tam * tam];
            for (int y = 0; y < tam; y++)
            {
                for (int x = 0; x < tam; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), centro);
                    float alpha = Mathf.Clamp01((raio - dist) / 1.5f);
                    pixels[y * tam + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            spriteCirculo = Sprite.Create(tex, new Rect(0, 0, tam, tam),
                new Vector2(0.5f, 0.5f), tam);
            spriteCirculo.name = "TelaM4_Circulo";
        }
        return spriteCirculo;
    }

    private static string ObterIdioma()
    {
        return PlayerPrefs.GetString("idioma", "pt");
    }

    private static string TextoPadraoTempoExcedido(string idioma)
    {
        switch (idioma)
        {
            case "en": return "Time limit exceeded";
            case "es": return "Tiempo límite excedido";
            case "fr": return "Délai dépassé";
            default: return "Tempo limite excedido";
        }
    }
}
