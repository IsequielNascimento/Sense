using NUnit.Framework;

public class RegrasLedsM4Tests
{
    #region MARK - Indicação visual padrão do manual

    [Test]
    public void Padrao_AbertoUsaVerde()
    {
        Assert.That(RegrasLedsM4.Resolver(EstadoLedsM4.Aberto, IndicacaoVisualM4.Padrao), Is.EqualTo(CorLedsM4.Verde));
    }

    [Test]
    public void Padrao_FechadoUsaLaranja()
    {
        Assert.That(RegrasLedsM4.Resolver(EstadoLedsM4.Fechado, IndicacaoVisualM4.Padrao), Is.EqualTo(CorLedsM4.Laranja));
    }

    #endregion

    #region MARK - Indicação visual invertida C4

    [Test]
    public void Invertida_AbertoUsaLaranja()
    {
        Assert.That(RegrasLedsM4.Resolver(EstadoLedsM4.Aberto, IndicacaoVisualM4.Invertida), Is.EqualTo(CorLedsM4.Laranja));
    }

    [Test]
    public void Invertida_FechadoUsaVerde()
    {
        Assert.That(RegrasLedsM4.Resolver(EstadoLedsM4.Fechado, IndicacaoVisualM4.Invertida), Is.EqualTo(CorLedsM4.Verde));
    }

    #endregion

    #region MARK - Alertas independem da inversão

    [TestCase(IndicacaoVisualM4.Padrao)]
    [TestCase(IndicacaoVisualM4.Invertida)]
    public void Alerta_UsaVermelho(IndicacaoVisualM4 indicacaoVisual)
    {
        Assert.That(RegrasLedsM4.Resolver(EstadoLedsM4.Alerta, indicacaoVisual), Is.EqualTo(CorLedsM4.Vermelho));
    }

    #endregion
}
