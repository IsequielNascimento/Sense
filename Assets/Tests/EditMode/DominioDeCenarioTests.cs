using NUnit.Framework;

public class DominioDeCenarioTests
{
    #region MARK - Valores centralizados

    [TestCase(DominioDeCenario.Treinamento)]
    [TestCase(DominioDeCenario.Configuracao)]
    [TestCase(DominioDeCenario.Manutencao)]
    [TestCase(DominioDeCenario.Alerta)]
    public void ValoresCentralizados_SaoValidos(string dominio)
    {
        Assert.That(DominioDeCenario.EhValido(dominio), Is.True);
    }

    [TestCase("Alerta ")]
    [TestCase("alerta")]
    [TestCase("Diagnostico")]
    [TestCase("")]
    [TestCase(null)]
    public void ForaDaListaCentralizada_EInvalido(string dominio)
    {
        Assert.That(DominioDeCenario.EhValido(dominio), Is.False);
    }

    #endregion

    #region MARK - Elegibilidade para alerta publico

    [Test]
    public void SomenteDominioAlertaComPrimaryCodeOficial_PodeSerApresentado()
    {
        var estrutura = new EstruturaCenario
        {
            scenarioId = "X",
            dominio = DominioDeCenario.Alerta,
            primaryCode = "A18",
        };

        Assert.That(DominioDeCenario.PodeSerApresentadoComoAlertaPublico(estrutura), Is.True);
    }

    [Test]
    public void DominioDiferenteDeAlerta_NaoPodeSerApresentado()
    {
        var estrutura = new EstruturaCenario
        {
            scenarioId = "X",
            dominio = DominioDeCenario.Treinamento,
            primaryCode = "A18",
        };

        Assert.That(DominioDeCenario.PodeSerApresentadoComoAlertaPublico(estrutura), Is.False);
    }

    [Test]
    public void DominioAlertaSemPrimaryCodeOficial_NaoPodeSerApresentado()
    {
        var estrutura = new EstruturaCenario
        {
            scenarioId = "X",
            dominio = DominioDeCenario.Alerta,
            primaryCode = "C12",
        };

        Assert.That(DominioDeCenario.PodeSerApresentadoComoAlertaPublico(estrutura), Is.False);
    }

    [Test]
    public void EstruturaNula_NaoPodeSerApresentada()
    {
        Assert.That(DominioDeCenario.PodeSerApresentadoComoAlertaPublico(null), Is.False);
    }

    #endregion
}
