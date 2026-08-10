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
    public void TextoPrincipal_UsaPosicaoERotacaoAprovadas()
    {
        var layout = new TelaM4Layout(new Vector2(0.038032f, 0.042522635f));

        Assert.That(layout.TextoPrincipalPosicao.x, Is.EqualTo(-0.0057f).Within(1e-5f));
        Assert.That(layout.TextoPrincipalPosicao.y, Is.EqualTo(-0.0003f).Within(1e-5f));
        Assert.That(layout.ConteudoRotacaoZ, Is.EqualTo(90f));
    }

    [Test]
    public void Barra_LarguraEhCinquentaEOitoPorCentoDaLarguraTotal()
    {
        var layout = new TelaM4Layout(new Vector2(2f, 1f));

        Assert.That(layout.BarraLargura, Is.EqualTo(1.16f).Within(1e-5f));
        Assert.That(
            layout.BarraSegmentoLargura * TelaM4.QuantidadeSegmentosBarra +
            layout.BarraEspacamento * (TelaM4.QuantidadeSegmentosBarra - 1),
            Is.EqualTo(layout.BarraLargura).Within(1e-5f));
    }

    [Test]
    public void Led_DiametroEscalaComAAltura()
    {
        var layout = new TelaM4Layout(new Vector2(2f, 1f));

        Assert.That(layout.LedDiametro, Is.EqualTo(0.12f).Within(1e-5f));
    }

    [Test]
    public void Angulo_ContainerFicaAbaixoDoCentro()
    {
        var layout = new TelaM4Layout(new Vector2(2f, 1f));
        var rotacaoInversa = Quaternion.Inverse(Quaternion.Euler(0f, 0f, layout.ConteudoRotacaoZ));
        var deslocamentoLogico = rotacaoInversa * (layout.AnguloContainerPosicao - layout.TextoPrincipalPosicao);

        Assert.That(deslocamentoLogico.x, Is.EqualTo(0f).Within(1e-5f));
        Assert.That(deslocamentoLogico.y, Is.EqualTo(-0.75f).Within(1e-5f));
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
