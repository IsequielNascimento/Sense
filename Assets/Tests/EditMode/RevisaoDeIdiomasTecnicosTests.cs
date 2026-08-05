using NUnit.Framework;

public class RevisaoDeIdiomasTecnicosTests
{
    #region MARK: Estado por idioma

    [Test]
    public void Pt_EAprovado()
    {
        Assert.That(RevisaoDeIdiomasTecnicos.ObterEstado("pt"), Is.EqualTo(EstadoDeRevisao.Aprovado));
    }

    [TestCase("en")]
    [TestCase("es")]
    [TestCase("fr")]
    public void EnEsFr_SaoPendentesDeRevisao(string idioma)
    {
        Assert.That(RevisaoDeIdiomasTecnicos.ObterEstado(idioma), Is.EqualTo(EstadoDeRevisao.PendenteRevisao));
    }

    [TestCase("")]
    [TestCase(null)]
    [TestCase("PT")]
    public void IdiomaVazioOuMaiusculo_ResolveParaAprovado(string idioma)
    {
        Assert.That(RevisaoDeIdiomasTecnicos.ObterEstado(idioma), Is.EqualTo(EstadoDeRevisao.Aprovado));
    }

    [TestCase("de")]
    public void IdiomaDesconhecido_ResolveParaPendenteRevisao(string idioma)
    {
        Assert.That(RevisaoDeIdiomasTecnicos.ObterEstado(idioma), Is.EqualTo(EstadoDeRevisao.PendenteRevisao));
    }

    #endregion

    #region MARK: Idioma tecnico efetivo

    [Test]
    public void Pt_ResolveParaPt()
    {
        Assert.That(RevisaoDeIdiomasTecnicos.ResolverIdiomaTecnico("pt"), Is.EqualTo("pt"));
    }

    [TestCase("en")]
    [TestCase("es")]
    [TestCase("fr")]
    public void IdiomasPendentes_ResolvemParaPt(string idioma)
    {
        Assert.That(RevisaoDeIdiomasTecnicos.ResolverIdiomaTecnico(idioma), Is.EqualTo("pt"));
    }

    [TestCase("")]
    [TestCase(null)]
    [TestCase("  ")]
    [TestCase("de")]
    public void IdiomaVazioOuDesconhecido_ResolveParaPt(string idioma)
    {
        Assert.That(RevisaoDeIdiomasTecnicos.ResolverIdiomaTecnico(idioma), Is.EqualTo("pt"));
    }

    [Test]
    public void Idioma_ECaseInsensitive()
    {
        Assert.That(RevisaoDeIdiomasTecnicos.ObterEstado("PT"), Is.EqualTo(EstadoDeRevisao.Aprovado));
        Assert.That(RevisaoDeIdiomasTecnicos.ResolverIdiomaTecnico("EN"), Is.EqualTo("pt"));
    }

    #endregion
}
