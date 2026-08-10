using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class TelaM4BarraTests
{
    private GameObject ancora;
    private TelaM4 tela;

    #region MARK - Construção da tela para validação

    [SetUp]
    public void CriarTela()
    {
        ancora = new GameObject("Ancora");
        tela = TelaM4.CriarEm(ancora.transform, new Vector2(0.038032f, 0.042522635f), 0, 0);
    }

    [TearDown]
    public void DestruirTela()
    {
        Object.DestroyImmediate(ancora);
    }

    #endregion

    #region MARK - Posição e orientação do conteúdo

    [Test]
    public void TextoPrincipal_UsaPosicaoERotacaoPositiva()
    {
        var texto = tela.transform.Find("TextoPrincipal");

        Assert.That(texto.localPosition.x, Is.EqualTo(-0.0057f).Within(1e-5f));
        Assert.That(texto.localPosition.y, Is.EqualTo(-0.0003f).Within(1e-5f));
        Assert.That(texto.localEulerAngles.z, Is.EqualTo(90f).Within(1e-4f));
    }

    #endregion

    #region MARK - Bargraph segmentado

    [Test]
    public void Barra_CriaDezSegmentosPretos()
    {
        var barra = tela.transform.Find("BarraProgresso");
        var segmentos = barra.GetComponentsInChildren<SpriteRenderer>(true);

        Assert.That(segmentos, Has.Length.EqualTo(10));
        Assert.That(segmentos.All(segmento => segmento.color.r < 0.1f), Is.True);
    }

    [TestCase(0f, 0)]
    [TestCase(0.4f, 4)]
    [TestCase(0.41f, 5)]
    [TestCase(1f, 10)]
    public void DefinirProgresso_AtivaBlocosCompletos(float progresso, int esperados)
    {
        tela.DefinirProgresso(progresso);

        var segmentos = tela.transform.Find("BarraProgresso").GetComponentsInChildren<SpriteRenderer>(true);
        Assert.That(segmentos.Count(segmento => segmento.enabled), Is.EqualTo(esperados));
    }

    #endregion
}
