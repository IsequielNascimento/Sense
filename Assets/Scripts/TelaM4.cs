using System.Collections;
using TMPro;
using UnityEngine;

public class TelaM4 : MonoBehaviour
{
    private TextMeshPro textoPrincipal;

    private GameObject barraContainer;
    private SpriteRenderer barraPreenchimento;
    private float barraLargura;

    private SpriteRenderer led;

    private GameObject alertaContainer;
    private TextMeshPro alertaTexto;

    private GameObject anguloContainer;
    private TextMeshPro anguloTexto;

    private Coroutine rotinaProgresso;
    private Coroutine rotinaPiscar;

    private float largura = 0.1f;
    private float altura = 0.06f;

    private static Sprite spriteBranco;
    private static Sprite spriteCirculo;

    private static readonly Color CorTexto = Color.white;
    private static readonly Color CorBarraFundo = new Color(0f, 0f, 0f, 0.55f);
    private static readonly Color CorBarraPreenchimento = new Color(1f, 0.54f, 0f);
    private static readonly Color CorAlertaFundo = new Color(0.72f, 0.11f, 0.11f, 0.92f);
    private static readonly Color CorAnguloFundo = new Color(1f, 1f, 1f, 0.95f);
    private static readonly Color CorAnguloTexto = new Color(0.13f, 0.13f, 0.13f);
    private static readonly Color CorLedVerde = new Color(0.2f, 0.9f, 0.25f);
    private static readonly Color CorLedVermelho = new Color(0.95f, 0.15f, 0.12f);

    public static TelaM4 CriarEm(SpriteRenderer display)
    {
        if (display == null)
        {
            Debug.LogError("[TelaM4] 'display' não pode ser nulo.");
            return null;
        }

        var go = new GameObject("TelaM4 (dinâmica)");
        go.transform.SetParent(display.transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        var tela = go.AddComponent<TelaM4>();
        tela.Construir(display);
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

    private void DefinirProgresso(float fracao)
    {
        fracao = Mathf.Clamp01(fracao);
        var t = barraPreenchimento.transform;
        t.localScale = new Vector3(barraLargura * fracao, t.localScale.y, 1f);
        t.localPosition = new Vector3(-barraLargura * 0.5f + barraLargura * fracao * 0.5f, 0f, 0f);
    }

    private void AplicarLed(string estado)
    {
        if (rotinaPiscar != null) { StopCoroutine(rotinaPiscar); rotinaPiscar = null; }
        led.enabled = true;

        if (string.IsNullOrEmpty(estado) || estado.ToLower() == "nenhum")
        {
            led.gameObject.SetActive(false);
            return;
        }

        string e = estado.ToLower();
        led.gameObject.SetActive(true);

        if (!e.StartsWith("verde") && !e.StartsWith("vermelho"))
        {
            Debug.LogWarning($"[TelaM4] Estado de LED '{estado}' não reconhecido; usando verde.");
        }

        led.color = e.StartsWith("vermelho") ? CorLedVermelho : CorLedVerde;

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

    private void Construir(SpriteRenderer display)
    {
        if (display.sprite != null)
        {
            largura = display.sprite.bounds.size.x;
            altura = display.sprite.bounds.size.y;
        }

        int ordemBase = display.sortingOrder;
        int layerId = display.sortingLayerID;

        textoPrincipal = CriarTexto("TextoPrincipal",
            new Vector3(0f, altura * 0.12f, 0f),
            new Vector2(largura * 0.9f, altura * 0.5f),
            CorTexto, layerId, ordemBase + 3);

        barraLargura = largura * 0.8f;
        float barraAlturaLocal = altura * 0.1f;
        barraContainer = new GameObject("BarraProgresso");
        barraContainer.transform.SetParent(transform, false);
        barraContainer.transform.localPosition = new Vector3(0f, -altura * 0.32f, 0f);

        CriarRetangulo("Fundo", barraContainer.transform, Vector3.zero,
            new Vector2(barraLargura, barraAlturaLocal), CorBarraFundo, layerId, ordemBase + 1);
        barraPreenchimento = CriarRetangulo("Preenchimento", barraContainer.transform, Vector3.zero,
            new Vector2(barraLargura, barraAlturaLocal * 0.7f), CorBarraPreenchimento, layerId, ordemBase + 2);

        led = CriarLed(new Vector3(largura * 0.42f, altura * 0.38f, 0f),
            altura * 0.12f, layerId, ordemBase + 6);

        alertaContainer = new GameObject("Alerta");
        alertaContainer.transform.SetParent(transform, false);
        CriarRetangulo("Fundo", alertaContainer.transform, Vector3.zero,
            new Vector2(largura * 0.94f, altura * 0.42f), CorAlertaFundo, layerId, ordemBase + 4);
        alertaTexto = CriarTexto("Texto",
            Vector3.zero, new Vector2(largura * 0.88f, altura * 0.38f),
            CorTexto, layerId, ordemBase + 5, alertaContainer.transform);

        anguloContainer = new GameObject("CaixaAngulo");
        anguloContainer.transform.SetParent(transform, false);
        anguloContainer.transform.localPosition = new Vector3(0f, -altura * 0.75f, 0f);
        CriarRetangulo("Fundo", anguloContainer.transform, Vector3.zero,
            new Vector2(largura * 0.96f, altura * 0.32f), CorAnguloFundo, layerId, ordemBase + 4);
        anguloTexto = CriarTexto("Texto",
            Vector3.zero, new Vector2(largura * 0.9f, altura * 0.28f),
            CorAnguloTexto, layerId, ordemBase + 5, anguloContainer.transform);

        LimparTudo();
    }

    private TextMeshPro CriarTexto(string nome, Vector3 posicaoLocal, Vector2 tamanho,
        Color cor, int layerId, int ordem, Transform pai = null)
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
        tmp.textWrappingMode = TextWrappingModes.Normal;

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
        if (IdiomaManager.Instance != null) return IdiomaManager.Instance.ObterIdioma();
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
