using NUnit.Framework;
using UnityEngine;

public class TelaM4LayoutTests
{
    #region MARK - Proporções escalam com as dimensões

    [Test]
    public void TextoPrincipal_OcupaNoventaPorCentoDaLargura()
    {
        var layout = new TelaM4Layout(new Vector2(2f, 1f));

        Assert.That(layout.TextoPrincipalTamanho.x, Is.EqualTo(1.8f).Within(1e-5f));
        Assert.That(layout.TextoPrincipalTamanho.y, Is.EqualTo(0.5f).Within(1e-5f));
    }

    [Test]
    public void Barra_LarguraEhOitentaPorCentoDaLarguraTotal()
    {
        var layout = new TelaM4Layout(new Vector2(2f, 1f));

        Assert.That(layout.BarraLargura, Is.EqualTo(1.6f).Within(1e-5f));
    }

    [Test]
    public void Led_DiametroEscalaComAAltura()
    {
        var layout = new TelaM4Layout(new Vector2(2f, 1f));

        Assert.That(layout.LedDiametro, Is.EqualTo(0.12f).Within(1e-5f));
        Assert.That(layout.LedPosicao, Is.EqualTo(new Vector3(0.84f, 0.38f, 0f)));
    }

    [Test]
    public void Angulo_ContainerFicaAbaixoDoCentro()
    {
        var layout = new TelaM4Layout(new Vector2(2f, 1f));

        Assert.That(layout.AnguloContainerPosicao, Is.EqualTo(new Vector3(0f, -0.75f, 0f)));
        Assert.That(layout.AnguloFundoTamanho, Is.EqualTo(new Vector2(1.92f, 0.32f)));
    }

    #endregion

    #region MARK - Dimensões degeneradas

    [Test]
    public void DimensoesZero_ProduzLayoutColapsadoSemErro()
    {
        var layout = new TelaM4Layout(Vector2.zero);

        Assert.That(layout.BarraLargura, Is.EqualTo(0f));
        Assert.That(layout.TextoPrincipalTamanho, Is.EqualTo(Vector2.zero));
    }

    #endregion
}
