using NUnit.Framework;

public class RegrasDePulsoDeDestaqueTests
{
    #region MARK: Fixture

    private const float Periodo = 1.2f;
    private const float Tolerancia = 0.0001f;

    private static readonly float[] PeriodosInvalidos = { 0f, -0.5f };

    #endregion

    #region MARK: Extremos do ciclo

    [Test]
    public void InicioDoCiclo_EstaNaIntensidadeMaxima()
    {
        Assert.That(
            RegrasDePulsoDeDestaque.Intensidade(0f, Periodo),
            Is.EqualTo(RegrasDePulsoDeDestaque.IntensidadeMaxima).Within(Tolerancia));
    }

    [Test]
    public void MetadeDoCiclo_EstaNaIntensidadeMinima()
    {
        Assert.That(
            RegrasDePulsoDeDestaque.Intensidade(Periodo / 2f, Periodo),
            Is.EqualTo(RegrasDePulsoDeDestaque.IntensidadeMinima).Within(Tolerancia));
    }

    [Test]
    public void CicloCompleto_VoltaAIntensidadeMaxima()
    {
        Assert.That(
            RegrasDePulsoDeDestaque.Intensidade(Periodo, Periodo),
            Is.EqualTo(RegrasDePulsoDeDestaque.IntensidadeMaxima).Within(Tolerancia));
    }

    #endregion

    #region MARK: Suavidade do pulso

    [Test]
    public void UmQuartoDoCiclo_EstaNoMeioDaFaixa()
    {
        float meio = (RegrasDePulsoDeDestaque.IntensidadeMinima + RegrasDePulsoDeDestaque.IntensidadeMaxima) / 2f;

        Assert.That(
            RegrasDePulsoDeDestaque.Intensidade(Periodo / 4f, Periodo),
            Is.EqualTo(meio).Within(Tolerancia));
    }

    [Test]
    public void QualquerInstante_FicaDentroDaFaixaDeIntensidade()
    {
        for (int passo = 0; passo <= 240; passo++)
        {
            float tempo = passo * Periodo / 60f;
            float intensidade = RegrasDePulsoDeDestaque.Intensidade(tempo, Periodo);

            Assert.That(intensidade, Is.InRange(
                RegrasDePulsoDeDestaque.IntensidadeMinima - Tolerancia,
                RegrasDePulsoDeDestaque.IntensidadeMaxima + Tolerancia));
        }
    }

    [Test]
    public void OContornoNuncaDesaparece_AIntensidadeMinimaEMaiorQueZero()
    {
        Assert.That(RegrasDePulsoDeDestaque.IntensidadeMinima, Is.GreaterThan(0f));
    }

    #endregion

    #region MARK: Avanco da fase

    [Test]
    public void FaseAvanca_SomandoODeltaEnquantoCabeNoCiclo()
    {
        Assert.That(
            RegrasDePulsoDeDestaque.AvancarFase(0.5f, 0.3f, Periodo),
            Is.EqualTo(0.8f).Within(Tolerancia));
    }

    [Test]
    public void FaseAoPassarDoPeriodo_VoltaAoInicioDoCiclo()
    {
        Assert.That(
            RegrasDePulsoDeDestaque.AvancarFase(1f, 0.4f, Periodo),
            Is.EqualTo(0.2f).Within(Tolerancia));
    }

    [Test]
    public void DeltaMaiorQueVariosCiclos_ContinuaDentroDoCiclo()
    {
        Assert.That(
            RegrasDePulsoDeDestaque.AvancarFase(0f, 5f, Periodo),
            Is.EqualTo(0.2f).Within(Tolerancia));
    }

    [TestCaseSource(nameof(PeriodosInvalidos))]
    public void FaseComPeriodoInvalido_ReiniciaEmZero(float periodo)
    {
        Assert.That(
            RegrasDePulsoDeDestaque.AvancarFase(0.5f, 0.3f, periodo),
            Is.EqualTo(0f).Within(Tolerancia));
    }

    #endregion

    #region MARK: Periodo invalido

    [TestCaseSource(nameof(PeriodosInvalidos))]
    public void PeriodoInvalido_MantemOContornoNaIntensidadeMaxima(float periodo)
    {
        Assert.That(
            RegrasDePulsoDeDestaque.Intensidade(0.4f, periodo),
            Is.EqualTo(RegrasDePulsoDeDestaque.IntensidadeMaxima).Within(Tolerancia));
    }

    #endregion
}
