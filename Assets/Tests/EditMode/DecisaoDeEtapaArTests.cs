using NUnit.Framework;

public class DecisaoDeEtapaArTests
{
    #region MARK: Fixture

    private const string CamadaPadrao = "Base Layer";
    private const string CamadaDeProblema = "A1";
    private const string AnimacaoValida = "animacao_montagem";

    private static readonly string[] AnimacoesVazias = { null, "", "   " };

    #endregion

    #region MARK: Etapa sem animacao

    [TestCaseSource(nameof(AnimacoesVazias))]
    public void AnimacaoAusente_NaoPossuiAnimacaoENuncaEMontagem(string animacao)
    {
        Assert.That(DecisaoDeEtapaAr.PossuiAnimacao(animacao), Is.False);
        Assert.That(DecisaoDeEtapaAr.EhMontagem(animacao, CamadaPadrao, CamadaPadrao), Is.False);
        Assert.That(DecisaoDeEtapaAr.EhMontagem(animacao, CamadaDeProblema, CamadaPadrao), Is.False);
        Assert.That(DecisaoDeEtapaAr.EhMontagem(animacao, null, CamadaPadrao), Is.False);
        Assert.That(DecisaoDeEtapaAr.EhMontagem(animacao, string.Empty, CamadaPadrao), Is.False);
    }

    #endregion

    #region MARK: Etapa com animacao

    [Test]
    public void AnimacaoPreenchida_PossuiAnimacao()
    {
        Assert.That(DecisaoDeEtapaAr.PossuiAnimacao(AnimacaoValida), Is.True);
    }

    [Test]
    public void AnimacaoNaCamadaPadrao_EMontagem()
    {
        Assert.That(DecisaoDeEtapaAr.EhMontagem(AnimacaoValida, CamadaPadrao, CamadaPadrao), Is.True);
    }

    [TestCase(null)]
    [TestCase("")]
    public void AnimacaoSemCamadaAlvo_EMontagem(string camadaAlvo)
    {
        Assert.That(DecisaoDeEtapaAr.EhMontagem(AnimacaoValida, camadaAlvo, CamadaPadrao), Is.True);
    }

    [TestCase("A1")]
    [TestCase("A4")]
    public void AnimacaoEmCamadaDeProblema_NaoEMontagem(string camadaAlvo)
    {
        Assert.That(DecisaoDeEtapaAr.EhMontagem(AnimacaoValida, camadaAlvo, CamadaPadrao), Is.False);
        Assert.That(DecisaoDeEtapaAr.PossuiAnimacao(AnimacaoValida), Is.True);
    }

    #endregion
}
