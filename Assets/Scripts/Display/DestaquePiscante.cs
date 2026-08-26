using UnityEngine;

[DisallowMultipleComponent]
public class DestaquePiscante : MonoBehaviour
{
    #region MARK - Configuração visual

    [SerializeField] private Renderer rendererDestaque;
    [SerializeField, Min(0.05f)] private float periodoSegundos = RegrasDePulsoDeDestaque.PeriodoPadraoSegundos;

    #endregion

    #region MARK - Estado de execução

    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private MaterialPropertyBlock propriedades;
    private Color corBase = Color.white;
    private float fase;

    #endregion

    public float PeriodoSegundos => periodoSegundos;
    public Renderer RendererDestaque => rendererDestaque;

    private void OnEnable()
    {
        ResolverRenderer();
        LerCorBase();
        fase = 0f;
        Aplicar(RegrasDePulsoDeDestaque.IntensidadeMaxima);
    }

    private void Update()
    {
        Aplicar(AvancarTempo(Time.deltaTime));
    }

    public float AvancarTempo(float deltaSegundos)
    {
        fase = RegrasDePulsoDeDestaque.AvancarFase(fase, deltaSegundos, periodoSegundos);

        return RegrasDePulsoDeDestaque.Intensidade(fase, periodoSegundos);
    }

    private void ResolverRenderer()
    {
        if (rendererDestaque == null) rendererDestaque = GetComponent<Renderer>();
    }

    private void LerCorBase()
    {
        Material material = rendererDestaque != null ? rendererDestaque.sharedMaterial : null;

        if (material != null && material.HasProperty(ColorId)) corBase = material.GetColor(ColorId);
    }

    private void Aplicar(float intensidade)
    {
        if (rendererDestaque == null) return;
        if (propriedades == null) propriedades = new MaterialPropertyBlock();

        rendererDestaque.GetPropertyBlock(propriedades);
        propriedades.SetColor(ColorId, new Color(
            corBase.r * intensidade,
            corBase.g * intensidade,
            corBase.b * intensidade,
            corBase.a));
        rendererDestaque.SetPropertyBlock(propriedades);
    }
}
