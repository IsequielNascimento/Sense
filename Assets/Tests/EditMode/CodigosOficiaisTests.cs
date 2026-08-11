using NUnit.Framework;

public class CodigosOficiaisTests
{
    #region MARK - Catálogo do manual

    [Test]
    public void A10_NaoExisteNoManual()
    {
        Assert.That(CodigosOficiais.EhValido("A10"), Is.False);
        Assert.That(CodigosOficiais.Alertas.Count, Is.EqualTo(24));
    }

    [TestCase("A1")]
    [TestCase("A9")]
    [TestCase("A11")]
    [TestCase("A25")]
    [TestCase("C1")]
    [TestCase("C23")]
    public void CodigosDaTabela3_SaoValidos(string codigo)
    {
        Assert.That(CodigosOficiais.EhValido(codigo), Is.True);
    }

    [TestCase("A26")]
    [TestCase("C24")]
    [TestCase("A0")]
    [TestCase("")]
    [TestCase(null)]
    public void CodigosForaDaTabela_SaoInvalidos(string codigo)
    {
        Assert.That(CodigosOficiais.EhValido(codigo), Is.False);
    }

    [Test]
    public void EhAlerta_DistingueAlertasDeConfiguracoes()
    {
        Assert.That(CodigosOficiais.EhAlerta("A18"), Is.True);
        Assert.That(CodigosOficiais.EhAlerta("C18"), Is.False);
    }

    [Test]
    public void EhConfiguracao_DistingueConfiguracoesDeAlertas()
    {
        Assert.That(CodigosOficiais.EhConfiguracao("C18"), Is.True);
        Assert.That(CodigosOficiais.EhConfiguracao("A18"), Is.False);
    }

    [TestCase("A26")]
    [TestCase("C24")]
    [TestCase("")]
    [TestCase(null)]
    public void EhConfiguracao_ForaDaTabela_EInvalido(string codigo)
    {
        Assert.That(CodigosOficiais.EhConfiguracao(codigo), Is.False);
    }

    #endregion

}
