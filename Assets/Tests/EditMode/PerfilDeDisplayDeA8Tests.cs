using System.Linq;
using NUnit.Framework;

public class PerfilDeDisplayDeA8Tests
{
    #region MARK: Fixture

    private const string CodigoA8 = "A8";
    private const string AcaoOficialDeA8 = "Colocar o monitor no modo seguro";
    private const string LocalOficialDeA8 = "menu modo seguro";
    private const int QuantidadeDePassosGuiados = 9;
    private const int IndiceDaConfirmacao = 7;

    private AlertaOficial a8;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a8 = CatalogoDeAlertas.Obter(CodigoA8, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA8);
    }

    private SequenciaDeQuadrosM4 EtapaUnica => perfil.EtapaOficial(0);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA8_MantemAAcaoUnicaDaPagina76()
    {
        Assert.That(a8, Is.Not.Null);
        Assert.That(a8.Nome, Is.EqualTo("MODO SEGURO"));
        Assert.That(a8.Padrao, Is.EqualTo("sempre ligado"));
        Assert.That(a8.Acoes.Count, Is.EqualTo(1));
        Assert.That(a8.Acoes[0], Is.EqualTo(AcaoOficialDeA8));
        Assert.That(a8.Locais.Count, Is.EqualTo(1));
        Assert.That(a8.Locais[0], Is.EqualTo(LocalOficialDeA8));
    }

    [Test]
    public void PerfilDeA8_TemExatamenteUmaEtapaOficial()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA8));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(1));
        Assert.That(perfil.CorrespondeAoCatalogo(a8), Is.True);
    }

    [Test]
    public void PerfilDeA8_NaoUsaOCenarioLegado()
    {
        Assert.That(a8.PossuiCenarioAnimado, Is.False);
        Assert.That(a8.ScenarioResourceKey, Is.Empty);
    }

    [Test]
    public void RegistroDePerfis_NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A24"), Is.Null);
        Assert.That(
            PerfisDeDisplayDeAlerta.CodigosComPerfil,
            Is.EquivalentTo(new[]
            {
                CodigoA8,
                PerfisDeDisplayDeAlerta.CodigoA11,
                PerfisDeDisplayDeAlerta.CodigoA12,
            }));
    }

    #endregion

    #region MARK: Quadros do LCD

    [Test]
    public void EtapaOficialDeA8_ReproduzOsQuadrosDaRotaLocalPorSenha()
    {
        string[] quadros = EtapaUnica.Quadros.Select(quadro => quadro.TextoLcd).ToArray();

        Assert.That(quadros, Is.EqualTo(new[]
        {
            "A8",
            "MENU",
            "MENU\nCONFIG",
            "C16\nMODO S",
            "SENHA",
            "HABILI",
            "1234",
            "C16\nMODO S",
            "SAIR",
        }));
    }

    [Test]
    public void QuadrosDeA8_NaoAlteramConfiguracaoAlemDeC16()
    {
        string[] configuracoes = EtapaUnica.Quadros
            .Select(quadro => quadro.TextoLcd.Split('\n')[0])
            .Where(CodigosOficiais.EhConfiguracao)
            .Distinct()
            .ToArray();

        Assert.That(configuracoes, Is.EqualTo(new[] { "C16" }));
    }

    [Test]
    public void QuadrosDeA8_NaoTemTextoVazioNemEspacoSobrando()
    {
        foreach (QuadroDeDisplayM4 quadro in EtapaUnica.Quadros)
        {
            Assert.That(quadro.TextoLcd, Is.Not.Empty);
            Assert.That(quadro.TextoLcd, Is.EqualTo(quadro.TextoLcd.Trim()));
        }
    }

    [Test]
    public void CadaQuadroDeA8_TemInstrucaoPraticaAutossuficiente()
    {
        Assert.That(EtapaUnica.Quantidade, Is.EqualTo(QuantidadeDePassosGuiados));

        foreach (QuadroDeDisplayM4 quadro in EtapaUnica.Quadros)
        {
            Assert.That(quadro.Instrucao, Is.Not.Null.And.Not.Empty);
            Assert.That(quadro.Instrucao, Is.EqualTo(quadro.Instrucao.Trim()));
        }
    }

    [Test]
    public void EntradaNoMenu_UsaB2PorSeisSegundosEBargraph()
    {
        QuadroDeDisplayM4 entrada = EtapaUnica.Em(1);

        Assert.That(entrada.TextoLcd, Is.EqualTo("MENU"));
        Assert.That(entrada.Instrucao, Does.Contain("B2"));
        Assert.That(entrada.Instrucao, Does.Contain("6 segundos"));
        Assert.That(entrada.ProgressoSegundos, Is.EqualTo(6f));
    }

    #endregion

    #region MARK: LED conforme a Nota 4 da pagina 53

    [Test]
    public void LedDeA8_PiscaVermelhoAteAConfirmacaoDoModoSeguro()
    {
        for (int i = 0; i < IndiceDaConfirmacao; i++)
        {
            QuadroDeDisplayM4 quadro = EtapaUnica.Em(i);

            Assert.That(quadro.Leds, Is.EqualTo(EstadoLedsM4.Alerta), $"quadro {i}");
            Assert.That(quadro.LedPiscando, Is.True, $"quadro {i}");
            Assert.That(quadro.EstadoDeLedDaEtapa(), Is.EqualTo(QuadroDeDisplayM4.LedVermelhoPiscando), $"quadro {i}");
        }
    }

    [Test]
    public void LedDeA8_ParaDePiscarNaConfirmacaoEPermaneceApagadoNaSaida()
    {
        for (int i = IndiceDaConfirmacao; i < EtapaUnica.Quantidade; i++)
        {
            QuadroDeDisplayM4 quadro = EtapaUnica.Em(i);

            Assert.That(quadro.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
            Assert.That(quadro.LedPiscando, Is.False);
            Assert.That(quadro.EstadoDeLedDaEtapa(), Is.EqualTo(QuadroDeDisplayM4.LedApagado));
        }
    }

    [Test]
    public void QuadroNaoAlerta_NaoPodePiscar()
    {
        Assert.Throws<System.ArgumentException>(
            () => new QuadroDeDisplayM4("C16", EstadoLedsM4.Desligado, ledPiscando: true));
    }

    #endregion

    #region MARK: Mecanismo local documentado na Figura 105

    [Test]
    public void PerfilDeA8_UsaSenhaLocalDocumentadaNoFluxograma()
    {
        Assert.That(perfil.MecanismoDeAtivacaoConfirmado, Is.True);

        string[] textos = EtapaUnica.Quadros.Select(quadro => quadro.TextoLcd).ToArray();

        Assert.That(textos, Does.Contain("SENHA"));
        Assert.That(textos, Does.Contain("HABILI"));
        Assert.That(textos, Does.Contain("1234"));
        Assert.That(EtapaUnica.Em(6).Instrucao, Does.Contain("apenas o exemplo do manual"));
    }

    #endregion

    #region MARK: Navegacao determinista

    [Test]
    public void Navegacao_ComecaNoPrimeiroQuadroDaPrimeiraEtapa()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);

        Assert.That(navegacao.IndiceDaEtapaOficial, Is.EqualTo(0));
        Assert.That(navegacao.IndiceDoQuadro, Is.EqualTo(0));
        Assert.That(navegacao.EstaNoPrimeiroQuadro, Is.True);
        Assert.That(navegacao.QuadroAtual.TextoLcd, Is.EqualTo("A8"));
    }

    [Test]
    public void ProximoQuadro_PercorreOsQuadrosSemSairDaEtapaOficial()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);

        for (int i = 1; i < EtapaUnica.Quantidade; i++)
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
        Assert.That(navegacao.QuadroAtual.TextoLcd, Is.EqualTo("A8"));
    }

    [Test]
    public void AvancarEVoltar_NaoSaemDaUnicaEtapaOficialDeA8()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);

        Assert.That(navegacao.Avancar(), Is.False);
        Assert.That(navegacao.Voltar(), Is.False);
        Assert.That(navegacao.IndiceDaEtapaOficial, Is.EqualTo(0));
    }

    [Test]
    public void Reiniciar_VoltaAoEstadoInicial()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);
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
            tutorial = AcaoOficialDeA8,
            animacao = string.Empty,
            textoDisplay = "residual",
            alerta = "alerta residual",
            leds = QuadroDeDisplayM4.LedAberto,
            textoAngulo = "90",
            alertaTempoExcedido = "estourou",
            progressoSegundos = 5f,
            progressoEstoura = true,
        };

        EtapaUnica.Primeiro.Aplicar(etapa);

        Assert.That(etapa.textoDisplay, Is.EqualTo("A8"));
        Assert.That(etapa.leds, Is.EqualTo(QuadroDeDisplayM4.LedVermelhoPiscando));
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
    public void EtapasDeA8_ExpandemAAcaoOficialEmPassosPraticos()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a8, EtapasGuiadasDeAlerta.Criar(a8));

        Assert.That(a8.Acoes, Has.Count.EqualTo(1));
        Assert.That(etapas, Has.Length.EqualTo(QuantidadeDePassosGuiados));
        Assert.That(etapas.Select(etapa => etapa.textoDisplay), Is.EqualTo(
            EtapaUnica.Quadros.Select(quadro => quadro.TextoLcd)));
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.tutorial)), Is.True);
        Assert.That(etapas.All(etapa => string.IsNullOrEmpty(etapa.animacao)), Is.True);
        Assert.That(etapas.All(TelaM4.EtapaTemConteudo), Is.True);
        Assert.That(etapas[1].progressoSegundos, Is.EqualTo(6f));
        Assert.That(etapas[IndiceDaConfirmacao].leds, Is.EqualTo(QuadroDeDisplayM4.LedApagado));
    }

    [Test]
    public void AlertaSemPerfil_MantemAsEtapasIntactas()
    {
        AlertaOficial a24 = CatalogoDeAlertas.Obter("A24", "pt");

        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a24, EtapasGuiadasDeAlerta.Criar(a24));

        Assert.That(etapas.Length, Is.EqualTo(a24.Acoes.Count));
        Assert.That(etapas[0].textoDisplay, Is.Null.Or.Empty);
        Assert.That(etapas[0].animacao, Is.Empty);
    }

    #endregion

    #region MARK: Catalogo oficial preservado

    [Test]
    public void CatalogoOficial_Mantem24AlertasSemA10()
    {
        Assert.That(CodigosOficiais.Alertas.Count, Is.EqualTo(24));
        Assert.That(CodigosOficiais.Alertas, Does.Not.Contain("A10"));
        Assert.That(CodigosOficiais.EhValido("A10"), Is.False);
        Assert.That(CodigosOficiais.EhAlerta("A8"), Is.True);
        Assert.That(CodigosOficiais.EhConfiguracao("C16"), Is.True);
    }

    #endregion
}
