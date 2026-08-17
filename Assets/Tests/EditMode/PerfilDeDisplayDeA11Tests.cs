using System.Linq;
using NUnit.Framework;

public class PerfilDeDisplayDeA11Tests
{
    #region MARK: Fixture

    private const string CodigoA11 = "A11";
    private const string AcaoAjusteOuReset = "Aumentar número de ciclos ou resetar o contador";
    private const string AcaoDesligarAlerta = "Desligar alerta";
    private const int QuantidadeDeQuadrosAjusteOuReset = 8;
    private const int QuantidadeDeQuadrosDesligarAlerta = 5;

    private AlertaOficial a11;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a11 = CatalogoDeAlertas.Obter(CodigoA11, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA11);
    }

    private SequenciaDeQuadrosM4 EtapaAjusteOuReset => perfil.EtapaOficial(0);
    private SequenciaDeQuadrosM4 EtapaDesligarAlerta => perfil.EtapaOficial(1);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA11_MantemAsDuasAcoesDaPagina76()
    {
        Assert.That(a11, Is.Not.Null);
        Assert.That(a11.Nome, Is.EqualTo("CONTADO PARCIAL"));
        Assert.That(a11.Padrao, Is.EqualTo("desabilitado"));
        Assert.That(a11.Acoes.Count, Is.EqualTo(2));
        Assert.That(a11.Acoes[0], Is.EqualTo(AcaoAjusteOuReset));
        Assert.That(a11.Acoes[0], Does.Contain(" ou "));
        Assert.That(a11.Acoes[1], Is.EqualTo(AcaoDesligarAlerta));
        Assert.That(a11.Locais.Count, Is.EqualTo(2));
    }

    [Test]
    public void PerfilDeA11_TemExatamenteDuasEtapasOficiais()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA11));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(2));
        Assert.That(perfil.CorrespondeAoCatalogo(a11), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A11NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A24"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA11));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A8"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(4));
    }

    #endregion

    #region MARK: Quadros do LCD

    [Test]
    public void EtapaAjusteOuReset_TemOitoQuadrosSemTextoVazio()
    {
        Assert.That(EtapaAjusteOuReset.Quantidade, Is.EqualTo(QuantidadeDeQuadrosAjusteOuReset));

        foreach (QuadroDeDisplayM4 quadro in EtapaAjusteOuReset.Quadros)
        {
            Assert.That(quadro.TextoLcd, Is.Not.Empty);
            Assert.That(quadro.TextoLcd, Is.EqualTo(quadro.TextoLcd.Trim()));
            Assert.That(quadro.Instrucao, Is.Not.Null.And.Not.Empty);
            Assert.That(quadro.Instrucao, Is.EqualTo(quadro.Instrucao.Trim()));
        }
    }

    [Test]
    public void EtapaDesligarAlerta_TemCincoQuadrosSemTextoVazio()
    {
        Assert.That(EtapaDesligarAlerta.Quantidade, Is.EqualTo(QuantidadeDeQuadrosDesligarAlerta));

        foreach (QuadroDeDisplayM4 quadro in EtapaDesligarAlerta.Quadros)
        {
            Assert.That(quadro.TextoLcd, Is.Not.Empty);
            Assert.That(quadro.TextoLcd, Is.EqualTo(quadro.TextoLcd.Trim()));
            Assert.That(quadro.Instrucao, Is.Not.Null.And.Not.Empty);
            Assert.That(quadro.Instrucao, Is.EqualTo(quadro.Instrucao.Trim()));
        }
    }

    [Test]
    public void EntradaNoMenu_UsaB2PorSeisSegundosEmAmbasAsEtapas()
    {
        QuadroDeDisplayM4 entradaAjusteOuReset = EtapaAjusteOuReset.Em(1);
        QuadroDeDisplayM4 entradaDesligarAlerta = EtapaDesligarAlerta.Em(1);

        Assert.That(entradaAjusteOuReset.TextoLcd, Is.EqualTo("MENU"));
        Assert.That(entradaAjusteOuReset.Instrucao, Does.Contain("B2"));
        Assert.That(entradaAjusteOuReset.Instrucao, Does.Contain("6 segundos"));
        Assert.That(entradaAjusteOuReset.ProgressoSegundos, Is.EqualTo(6f));

        Assert.That(entradaDesligarAlerta.TextoLcd, Is.EqualTo("MENU"));
        Assert.That(entradaDesligarAlerta.Instrucao, Does.Contain("B2"));
        Assert.That(entradaDesligarAlerta.Instrucao, Does.Contain("6 segundos"));
        Assert.That(entradaDesligarAlerta.ProgressoSegundos, Is.EqualTo(6f));
    }

    #endregion

    #region MARK: Separacao entre ajuste, reset e desativacao

    [Test]
    public void AlternativaDeAjuste_ConfirmaLimiteSemIndicarReset()
    {
        QuadroDeDisplayM4 alternativaAjuste = EtapaAjusteOuReset.Em(4);
        QuadroDeDisplayM4 confirmacaoAjuste = EtapaAjusteOuReset.Em(5);

        Assert.That(alternativaAjuste.Instrucao, Does.Contain("Alternativa 1"));
        Assert.That(alternativaAjuste.Instrucao, Does.Contain("limite"));
        Assert.That(confirmacaoAjuste.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(confirmacaoAjuste.Instrucao, Does.Contain("limite"));
        Assert.That(confirmacaoAjuste.Instrucao, Does.Not.Contain("reset"));
        Assert.That(confirmacaoAjuste.Instrucao, Does.Contain("contador total não é alterado"));
    }

    [Test]
    public void AlternativaDeReset_UsaC18EZeraOContadorParcialSemAlterarOTotal()
    {
        QuadroDeDisplayM4 alternativaReset = EtapaAjusteOuReset.Em(6);
        QuadroDeDisplayM4 confirmacaoReset = EtapaAjusteOuReset.Em(7);

        Assert.That(alternativaReset.TextoLcd, Is.EqualTo("C18"));
        Assert.That(alternativaReset.Instrucao, Does.Contain("Alternativa 2"));
        Assert.That(confirmacaoReset.TextoLcd, Is.EqualTo("C18"));
        Assert.That(confirmacaoReset.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(confirmacaoReset.Instrucao, Does.Contain("voltou a zero"));
        Assert.That(confirmacaoReset.Instrucao, Does.Contain("contador total não é alterado"));
    }

    [Test]
    public void Desativacao_NaoSugereResetDoContador()
    {
        QuadroDeDisplayM4 confirmacaoDesligar = EtapaDesligarAlerta.Ultimo;

        Assert.That(confirmacaoDesligar.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(confirmacaoDesligar.Instrucao, Does.Contain("desligado"));
        Assert.That(confirmacaoDesligar.Instrucao, Does.Contain("não reseta"));
    }

    [Test]
    public void ConfirmacoesDeAjusteResetEDesativacao_SaoTextualmenteDistintas()
    {
        string confirmacaoAjuste = EtapaAjusteOuReset.Em(5).Instrucao;
        string confirmacaoReset = EtapaAjusteOuReset.Em(7).Instrucao;
        string confirmacaoDesligar = EtapaDesligarAlerta.Ultimo.Instrucao;

        Assert.That(confirmacaoAjuste, Is.Not.EqualTo(confirmacaoReset));
        Assert.That(confirmacaoReset, Is.Not.EqualTo(confirmacaoDesligar));
        Assert.That(confirmacaoAjuste, Is.Not.EqualTo(confirmacaoDesligar));
    }

    #endregion

    #region MARK: LED e mecanismo sem evidencia normativa

    [Test]
    public void PerfilDeA11_NaoInventaLedVermelhoSemEvidenciaNormativa()
    {
        Assert.That(EtapaAjusteOuReset.Quadros.All(quadro => !quadro.LedPiscando), Is.True);
        Assert.That(EtapaAjusteOuReset.Quadros.All(quadro => quadro.Leds == EstadoLedsM4.Desligado), Is.True);
        Assert.That(EtapaDesligarAlerta.Quadros.All(quadro => !quadro.LedPiscando), Is.True);
        Assert.That(EtapaDesligarAlerta.Quadros.All(quadro => quadro.Leds == EstadoLedsM4.Desligado), Is.True);
    }

    [Test]
    public void PerfilDeA11_NaoAfirmaMecanismoDeAtivacaoConfirmado()
    {
        Assert.That(perfil.MecanismoDeAtivacaoConfirmado, Is.False);
    }

    #endregion

    #region MARK: Navegacao determinista

    [Test]
    public void Navegacao_ComecaNoPrimeiroQuadroDaPrimeiraEtapa()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);

        Assert.That(navegacao.IndiceDaEtapaOficial, Is.EqualTo(0));
        Assert.That(navegacao.IndiceDoQuadro, Is.EqualTo(0));
        Assert.That(navegacao.QuadroAtual.TextoLcd, Is.EqualTo(CodigoA11));
    }

    [Test]
    public void ProximoQuadro_PercorreOsQuadrosSemSairDaEtapaOficial()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);

        for (int i = 1; i < EtapaAjusteOuReset.Quantidade; i++)
        {
            Assert.That(navegacao.ProximoQuadro(), Is.True);
            Assert.That(navegacao.IndiceDoQuadro, Is.EqualTo(i));
            Assert.That(navegacao.IndiceDaEtapaOficial, Is.EqualTo(0));
        }

        Assert.That(navegacao.ProximoQuadro(), Is.False);
        Assert.That(navegacao.EstaNoUltimoQuadro, Is.True);
    }

    [Test]
    public void Repetir_ReiniciaNoPrimeiroQuadroDaEtapaAtual()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);
        navegacao.ProximoQuadro();
        navegacao.ProximoQuadro();

        navegacao.Repetir();

        Assert.That(navegacao.IndiceDoQuadro, Is.EqualTo(0));
        Assert.That(navegacao.QuadroAtual.TextoLcd, Is.EqualTo(CodigoA11));
    }

    [Test]
    public void Avancar_MoveParaASegundaEtapaOficialUmaUnicaVez()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);

        Assert.That(navegacao.Avancar(), Is.True);
        Assert.That(navegacao.IndiceDaEtapaOficial, Is.EqualTo(1));
        Assert.That(navegacao.IndiceDoQuadro, Is.EqualTo(0));
        Assert.That(navegacao.Avancar(), Is.False);
    }

    [Test]
    public void Voltar_RetornaParaAPrimeiraEtapaOficialUmaUnicaVez()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);
        navegacao.Avancar();

        Assert.That(navegacao.Voltar(), Is.True);
        Assert.That(navegacao.IndiceDaEtapaOficial, Is.EqualTo(0));
        Assert.That(navegacao.Voltar(), Is.False);
    }

    [Test]
    public void Reiniciar_VoltaAoEstadoInicial()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);
        navegacao.Avancar();
        navegacao.ProximoQuadro();

        navegacao.Reiniciar();

        Assert.That(navegacao.IndiceDaEtapaOficial, Is.EqualTo(0));
        Assert.That(navegacao.IndiceDoQuadro, Is.EqualTo(0));
    }

    #endregion

    #region MARK: Limpeza do estado visual anterior

    [Test]
    public void AplicarQuadro_LimpaTodoOEstadoVisualResidual()
    {
        var etapa = new Etapa
        {
            tutorial = AcaoAjusteOuReset,
            animacao = string.Empty,
            textoDisplay = "residual",
            alerta = "alerta residual",
            leds = QuadroDeDisplayM4.LedAberto,
            textoAngulo = "90",
            alertaTempoExcedido = "estourou",
            progressoSegundos = 5f,
            progressoEstoura = true,
        };

        EtapaAjusteOuReset.Primeiro.Aplicar(etapa);

        Assert.That(etapa.textoDisplay, Is.EqualTo(CodigoA11));
        Assert.That(etapa.leds, Is.EqualTo(QuadroDeDisplayM4.LedApagado));
        Assert.That(etapa.alerta, Is.Empty);
        Assert.That(etapa.textoAngulo, Is.Empty);
        Assert.That(etapa.alertaTempoExcedido, Is.Empty);
        Assert.That(etapa.progressoSegundos, Is.EqualTo(0f));
        Assert.That(etapa.progressoEstoura, Is.False);
        Assert.That(etapa.animacao, Is.Empty);
    }

    #endregion

    #region MARK: Composicao sobre as etapas guiadas

    [Test]
    public void EtapasDeA11_ExpandemAsDuasAcoesOficiaisEmPassosPraticos()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a11, EtapasGuiadasDeAlerta.Criar(a11));

        Assert.That(a11.Acoes, Has.Count.EqualTo(2));
        Assert.That(etapas, Has.Length.EqualTo(QuantidadeDeQuadrosAjusteOuReset + QuantidadeDeQuadrosDesligarAlerta));
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.tutorial)), Is.True);
        Assert.That(etapas.All(etapa => string.IsNullOrEmpty(etapa.animacao)), Is.True);
        Assert.That(etapas.All(TelaM4.EtapaTemConteudo), Is.True);
        Assert.That(etapas[1].progressoSegundos, Is.EqualTo(6f));
        Assert.That(etapas[QuantidadeDeQuadrosAjusteOuReset + 1].progressoSegundos, Is.EqualTo(6f));
    }

    #endregion
}
