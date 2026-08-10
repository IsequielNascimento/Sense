using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class ControladorLedsM4Tests
{
    private GameObject raiz;
    private Renderer rendererLeds;
    private ControladorLedsM4 controlador;

    #region MARK - Preparação do renderer de teste

    [SetUp]
    public void CriarControlador()
    {
        raiz = new GameObject("M4");
        raiz.SetActive(false);

        var leds = GameObject.CreatePrimitive(PrimitiveType.Quad);
        leds.name = "LEDS";
        leds.transform.SetParent(raiz.transform);
        rendererLeds = leds.GetComponent<Renderer>();

        controlador = raiz.AddComponent<ControladorLedsM4>();
        var serializado = new SerializedObject(controlador);
        serializado.FindProperty("rendererLeds").objectReferenceValue = rendererLeds;
        serializado.ApplyModifiedPropertiesWithoutUndo();
        raiz.SetActive(true);
    }

    [TearDown]
    public void DestruirControlador()
    {
        Object.DestroyImmediate(raiz);
    }

    private Color ObterEmissao()
    {
        var propriedades = new MaterialPropertyBlock();
        rendererLeds.GetPropertyBlock(propriedades);
        return propriedades.GetColor("_EmissionColor");
    }

    #endregion

    #region MARK - Aplicação visual dos estados

    [Test]
    public void Aberto_AplicaEmissaoPredominantementeVerde()
    {
        controlador.Aplicar(EstadoLedsM4.Aberto);

        var emissao = ObterEmissao();
        Assert.That(emissao.g, Is.GreaterThan(emissao.r));
        Assert.That(emissao.g, Is.GreaterThan(emissao.b));
    }

    [Test]
    public void Fechado_AplicaEmissaoLaranja()
    {
        controlador.Aplicar(EstadoLedsM4.Fechado);

        var emissao = ObterEmissao();
        Assert.That(emissao.r, Is.GreaterThan(emissao.g));
        Assert.That(emissao.g, Is.GreaterThan(emissao.b));
    }

    [Test]
    public void IndicacaoInvertida_TrocaACorDaPosicaoAberta()
    {
        controlador.DefinirIndicacaoVisual(IndicacaoVisualM4.Invertida);
        controlador.Aplicar(EstadoLedsM4.Aberto);

        var emissao = ObterEmissao();
        Assert.That(emissao.r, Is.GreaterThan(emissao.g));
    }

    [Test]
    public void Alerta_AplicaEmissaoPredominantementeVermelha()
    {
        controlador.Aplicar(EstadoLedsM4.Alerta);

        var emissao = ObterEmissao();
        Assert.That(emissao.r, Is.GreaterThan(emissao.g * 5f));
    }

    [Test]
    public void OnEnable_NaoSobrescreveVermelhoAplicadoAntesDaAtivacao()
    {
        raiz.SetActive(false);

        controlador.Aplicar("vermelho");
        raiz.SetActive(true);

        var emissao = ObterEmissao();
        Assert.That(emissao.r, Is.GreaterThan(emissao.g * 5f));
    }

    [Test]
    public void Desligado_RemoveEmissao()
    {
        controlador.Aplicar(EstadoLedsM4.Desligado);

        Assert.That(ObterEmissao(), Is.EqualTo(Color.black));
    }

    #endregion
}
