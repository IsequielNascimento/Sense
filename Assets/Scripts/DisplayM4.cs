using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Controla o texto renderizado diretamente sobre o display 3D do M4.
/// A referência pode ser atribuída pelo Inspector ou encontrada entre os
/// filhos (e irmãos próximos) do objeto que possui este componente.
/// </summary>
public sealed class DisplayM4 : MonoBehaviour
{
    private const string CaminhoFonteLCD = "M4Display/DSEG14Classic SDF";
    private const string CaminhoMaterialLCD = "M4Display/DSEG14Classic M4 LCD";

    [SerializeField] private TMP_Text texto;

    [Header("Estilo LCD")]
    [SerializeField] private TMP_FontAsset fonteLCD;
    [SerializeField] private Material materialPresetLCD;

    [Header("Fundo do LCD")]
    [SerializeField] private Renderer fundoLCD;
    [SerializeField] private Color corFundoLCD = new Color32(150, 150, 150, 255);

    private MaterialPropertyBlock propriedadesDoFundo;
    private SpriteRenderer[] spritesLegados;
    private GameObject[] canvasesLegados;

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
        // Alguns clips antigos ainda avaliam objetos do display legado. Manter a
        // supressão no fim do frame impede que a textura reapareça sob o TMP.
        if (texto != null && texto.gameObject.activeInHierarchy)
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

        AplicarEstiloLCD();

        string textoNormalizado = (valor ?? string.Empty).Trim().ToUpperInvariant();
        texto.gameObject.SetActive(textoNormalizado.Length > 0);
        texto.text = textoNormalizado;
        AplicarFundoLCD();
        DesativarDisplaysLegados();
    }

    public void Limpar()
    {
        ResolverReferencia();

        if (texto == null) return;

        texto.text = string.Empty;
        texto.gameObject.SetActive(false);
    }

    public void Preparar()
    {
        ResolverReferencia();
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

        // Permite a estrutura criada no prefab em que DisplayM4 e Text (TMP)
        // são filhos irmãos de DisplayDynamic.
        if (texto == null && transform.parent != null)
        {
            texto = transform.parent.GetComponentInChildren<TMP_Text>(true);
        }

        // Fallback para o componente instalado no GerenciadorVisual do prefab.
        // Restringe a busca ao grupo DisplayDynamic para não capturar textos da UI do app.
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

        if (fonteLCD != null)
        {
            texto.font = fonteLCD;
        }

        if (materialPresetLCD != null)
        {
            texto.fontSharedMaterial = materialPresetLCD;
        }

        texto.color = new Color32(8, 8, 8, 255);
        texto.fontStyle = FontStyles.Normal;
        texto.characterSpacing = 0f;
        texto.lineSpacing = -10f;
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
