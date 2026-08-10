using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[ExecuteAlways]
public class ControladorLedsM4 : MonoBehaviour
{
    #region MARK - Configuração visual

    [SerializeField] private Renderer rendererLeds;
    [SerializeField] private EstadoLedsM4 estadoInicial = EstadoLedsM4.Aberto;
    [SerializeField] private IndicacaoVisualM4 indicacaoVisual = IndicacaoVisualM4.Padrao;
    [SerializeField, ColorUsage(false, true)] private Color corVerde = new Color(0.05f, 1f, 0.12f);
    [SerializeField, ColorUsage(false, true)] private Color corLaranja = new Color(1f, 0.24f, 0.015f);
    [SerializeField, ColorUsage(false, true)] private Color corVermelha = new Color(1f, 0.025f, 0.01f);
    [SerializeField, Min(0f)] private float intensidadeEmissao = 8f;
    [SerializeField, Min(0.05f)] private float intervaloPiscar = 0.35f;

    #endregion

    #region MARK - Estado de execução

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private MaterialPropertyBlock propriedades;
    private Coroutine rotinaPiscar;
    private EstadoLedsM4 estadoAtual;
    private CorLedsM4? corDiretaAtual;
    private bool emitir = true;
    private bool possuiEstadoAplicado;
    private bool piscarEstadoAtual;

    #endregion

    public EstadoLedsM4 EstadoAtual => estadoAtual;
    public IndicacaoVisualM4 IndicacaoVisual => indicacaoVisual;
    public Renderer RendererLeds => rendererLeds;

    private void OnEnable()
    {
        ResolverRenderer();
        if (!possuiEstadoAplicado)
        {
            Aplicar(estadoInicial);
            return;
        }

        ReaplicarEstadoAtual();
    }

    private void OnDisable()
    {
        PararPiscar();
    }

    private void OnValidate()
    {
        ResolverRenderer();
        Aplicar(estadoInicial);
    }

    public void Aplicar(EstadoLedsM4 estado)
    {
        possuiEstadoAplicado = true;
        estadoAtual = estado;
        corDiretaAtual = null;
        piscarEstadoAtual = estado == EstadoLedsM4.Alerta;
        PararPiscar();
        emitir = true;
        AtualizarMaterial();

        if (piscarEstadoAtual && Application.isPlaying && isActiveAndEnabled)
        {
            rotinaPiscar = StartCoroutine(Piscar());
        }
    }

    public void DefinirIndicacaoVisual(IndicacaoVisualM4 valor)
    {
        indicacaoVisual = valor;
        AtualizarMaterial(corDiretaAtual);
    }

    public bool Aplicar(string estado)
    {
        if (string.IsNullOrWhiteSpace(estado) || estado.Equals("nenhum", System.StringComparison.OrdinalIgnoreCase))
        {
            Aplicar(EstadoLedsM4.Desligado);
            return true;
        }

        string normalizado = estado.Trim().ToLowerInvariant();
        if (normalizado.StartsWith("abert"))
        {
            Aplicar(EstadoLedsM4.Aberto);
            return true;
        }

        if (normalizado.StartsWith("fechad"))
        {
            Aplicar(EstadoLedsM4.Fechado);
            return true;
        }

        if (normalizado.StartsWith("alert"))
        {
            Aplicar(EstadoLedsM4.Alerta);
            return true;
        }

        bool piscar = normalizado.EndsWith("piscando");
        CorLedsM4 cor;
        if (normalizado.StartsWith("verde")) cor = CorLedsM4.Verde;
        else if (normalizado.StartsWith("laranja")) cor = CorLedsM4.Laranja;
        else if (normalizado.StartsWith("vermelho")) cor = CorLedsM4.Vermelho;
        else return false;

        AplicarCorDireta(cor, piscar);
        return true;
    }

    private void ResolverRenderer()
    {
        if (rendererLeds != null) return;

        foreach (var rendererEncontrado in transform.root.GetComponentsInChildren<Renderer>(true))
        {
            if (rendererEncontrado.name.Equals("LEDS", System.StringComparison.OrdinalIgnoreCase))
            {
                rendererLeds = rendererEncontrado;
                return;
            }
        }
    }

    private void AplicarCorDireta(CorLedsM4 cor, bool piscar)
    {
        possuiEstadoAplicado = true;
        corDiretaAtual = cor;
        piscarEstadoAtual = piscar;
        PararPiscar();
        emitir = true;
        AtualizarMaterial(cor);

        if (piscar && Application.isPlaying && isActiveAndEnabled)
        {
            rotinaPiscar = StartCoroutine(Piscar(cor));
        }
    }

    private void ReaplicarEstadoAtual()
    {
        PararPiscar();
        emitir = true;
        AtualizarMaterial(corDiretaAtual);

        if (piscarEstadoAtual && Application.isPlaying && isActiveAndEnabled)
        {
            rotinaPiscar = StartCoroutine(Piscar(corDiretaAtual));
        }
    }

    private IEnumerator Piscar(CorLedsM4? corDireta = null)
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloPiscar);
            emitir = !emitir;
            AtualizarMaterial(corDireta);
        }
    }

    private void PararPiscar()
    {
        if (rotinaPiscar == null) return;
        StopCoroutine(rotinaPiscar);
        rotinaPiscar = null;
    }

    private void AtualizarMaterial(CorLedsM4? corDireta = null)
    {
        if (rendererLeds == null) return;
        if (propriedades == null) propriedades = new MaterialPropertyBlock();

        CorLedsM4 corResolvida = corDireta ?? RegrasLedsM4.Resolver(estadoAtual, indicacaoVisual);
        Color cor = ObterCor(corResolvida);
        Color emissao = emitir && corResolvida != CorLedsM4.Desligado
            ? cor.linear * intensidadeEmissao
            : Color.black;

        rendererLeds.GetPropertyBlock(propriedades);
        propriedades.SetColor(BaseColorId, cor);
        propriedades.SetColor(EmissionColorId, emissao);
        rendererLeds.SetPropertyBlock(propriedades);
    }

    private Color ObterCor(CorLedsM4 cor)
    {
        switch (cor)
        {
            case CorLedsM4.Verde: return corVerde;
            case CorLedsM4.Laranja: return corLaranja;
            case CorLedsM4.Vermelho: return corVermelha;
            default: return new Color(0.08f, 0.08f, 0.08f);
        }
    }
}
