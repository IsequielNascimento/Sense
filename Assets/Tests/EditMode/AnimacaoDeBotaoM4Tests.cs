using NUnit.Framework;

public class AnimacaoDeBotaoM4Tests
{
    #region MARK: Sem mencao a botao

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("Verifique a confirmação: o alerta A13 foi desligado.")]
    [TestCase("Use C16 e A12 para conferir os códigos oficiais.")]
    [TestCase("O exemplo de senha 1234 não cita botão nenhum.")]
    public void InstrucaoSemBotao_NaoDerivaAnimacao(string instrucao)
    {
        Assert.That(AnimacaoDeBotaoM4.Derivar(instrucao), Is.Null);
    }

    #endregion

    #region MARK: Um unico botao

    [TestCase("Aproxime o polo Sul do chaveiro magnético do botão B2 por mais de 6 segundos.", AnimacaoDeBotaoM4.B2)]
    [TestCase("Mantenha B1 por mais de 3 segundos para sair.", AnimacaoDeBotaoM4.B1)]
    [TestCase("Use B3 para avançar até MENU ALERTA.", AnimacaoDeBotaoM4.B3)]
    [TestCase("Pressione B2 e depois B2 novamente para confirmar.", AnimacaoDeBotaoM4.B2)]
    public void InstrucaoComUmBotao_DerivaAAnimacaoDaqueleBotao(string instrucao, string esperada)
    {
        Assert.That(AnimacaoDeBotaoM4.Derivar(instrucao), Is.EqualTo(esperada));
    }

    #endregion

    #region MARK: Dois botoes distintos

    [TestCase("Segure B1 e depois pressione B2 para confirmar.", AnimacaoDeBotaoM4.B12)]
    [TestCase("B1 diminui e B3 aumenta o valor.", AnimacaoDeBotaoM4.B13)]
    [TestCase("Use B3 para selecionar HABILITAR e pressione B2 para definir a senha.", AnimacaoDeBotaoM4.B23)]
    [TestCase("Avance com B3 até A14 e pressione B2.", AnimacaoDeBotaoM4.B23)]
    public void InstrucaoComDoisBotoes_DerivaOParCitado(string instrucao, string esperada)
    {
        Assert.That(AnimacaoDeBotaoM4.Derivar(instrucao), Is.EqualTo(esperada));
    }

    #endregion

    #region MARK: Os tres botoes

    [TestCase("Use B3 para avançar até C16. B1 volta à opção anterior e B2 entra em C16.")]
    [TestCase("Em C16, selecione SENHA com B1 ou B3 e pressione B2.")]
    [TestCase("B1 decrementa, B3 incrementa e B2 seleciona.")]
    public void InstrucaoComOsTresBotoes_DerivaAAnimacaoCombinada(string instrucao)
    {
        Assert.That(AnimacaoDeBotaoM4.Derivar(instrucao), Is.EqualTo(AnimacaoDeBotaoM4.B123));
    }

    #endregion

    #region MARK: Tokens que nao sao botao

    [TestCase("O código B4 não existe no M4.")]
    [TestCase("A sigla AB2 faz parte de outra palavra.")]
    [TestCase("B12 não é um botão do painel.")]
    public void TokenParecidoComBotao_NaoDerivaAnimacao(string instrucao)
    {
        Assert.That(AnimacaoDeBotaoM4.Derivar(instrucao), Is.Null);
    }

    #endregion

    #region MARK: Nomes dos estados do Animator

    [Test]
    public void NomesDerivados_CasamComOsEstadosDoAnimator()
    {
        Assert.That(AnimacaoDeBotaoM4.B1, Is.EqualTo("B1Button"));
        Assert.That(AnimacaoDeBotaoM4.B2, Is.EqualTo("B2Button"));
        Assert.That(AnimacaoDeBotaoM4.B3, Is.EqualTo("B3Button"));
        Assert.That(AnimacaoDeBotaoM4.B12, Is.EqualTo("B12Button"));
        Assert.That(AnimacaoDeBotaoM4.B13, Is.EqualTo("B13Button"));
        Assert.That(AnimacaoDeBotaoM4.B23, Is.EqualTo("B23Button"));
        Assert.That(AnimacaoDeBotaoM4.B123, Is.EqualTo("B123Button"));
        Assert.That(AnimacaoDeBotaoM4.Todas, Is.EquivalentTo(new[]
        {
            "B1Button", "B2Button", "B3Button", "B12Button", "B13Button", "B23Button", "B123Button",
        }));
    }

    #endregion
}
