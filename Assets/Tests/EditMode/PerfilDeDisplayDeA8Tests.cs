using System.Linq;
using NUnit.Framework;

public class PerfilDeDisplayDeA8Tests
{
    #region MARK: Fixture

    private const string CodigoA8 = "A8";
    private const string AcaoOficialDeA8 = "Colocar o monitor no modo seguro";
    private const string LocalOficialDeA8 = "menu modo seguro";

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
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A11"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A24"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Is.EqualTo(new[] { CodigoA8 }));
    }

    #endregion

    #region MARK: Quadros do LCD

    [Test]
    public void EtapaOficialDeA8_ComecaIdentificandoOCodigoETerminaNoModoSeguro()
    {
        Assert.That(EtapaUnica.Primeiro.TextoLcd, Is.EqualTo("A8"));
        Assert.That(EtapaUnica.Em(1).TextoLcd, Is.EqualTo("MODO SEGURO"));
        Assert.That(EtapaUnica.Em(2).TextoLcd, Is.EqualTo("C16"));
        Assert.That(EtapaUnica.Ultimo.TextoLcd, Is.EqualTo("MODO SEGURO"));
    }

    [Test]
    public void QuadrosDeA8_NaoAlteramConfiguracaoAlemDeC16()
    {
        string[] configuracoes = EtapaUnica.Quadros
            .Select(quadro => quadro.TextoLcd)
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

    #endregion

    #region MARK: LED conforme a Nota 4 da pagina 53

    [Test]
    public void LedDeA8_PiscaVermelhoAteAConfirmacaoDoModoSeguro()
    {
        for (int i = 0; i < EtapaUnica.Quantidade - 1; i++)
        {
            QuadroDeDisplayM4 quadro = EtapaUnica.Em(i);

            Assert.That(quadro.Leds, Is.EqualTo(EstadoLedsM4.Alerta), $"quadro {i}");
            Assert.That(quadro.LedPiscando, Is.True, $"quadro {i}");
            Assert.That(quadro.EstadoDeLedDaEtapa(), Is.EqualTo(QuadroDeDisplayM4.LedVermelhoPiscando), $"quadro {i}");
        }
    }

    [Test]
    public void LedDeA8_ParaDePiscarSomenteNaConfirmacao()
    {
        Assert.That(EtapaUnica.Ultimo.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(EtapaUnica.Ultimo.LedPiscando, Is.False);
        Assert.That(EtapaUnica.Ultimo.EstadoDeLedDaEtapa(), Is.EqualTo(QuadroDeDisplayM4.LedApagado));
    }

    [Test]
    public void QuadroNaoAlerta_NaoPodePiscar()
    {
        Assert.Throws<System.ArgumentException>(
            () => new QuadroDeDisplayM4("C16", EstadoLedsM4.Desligado, ledPiscando: true));
    }

    #endregion

    #region MARK: Decisao pendente de produto

    [Test]
    public void PerfilDeA8_NaoAfirmaMecanismoDeAtivacao()
    {
        Assert.That(perfil.MecanismoDeAtivacaoConfirmado, Is.False);

        foreach (QuadroDeDisplayM4 quadro in EtapaUnica.Quadros)
        {
            string texto = quadro.TextoLcd.ToUpperInvariant();

            Assert.That(texto, Does.Not.Contain("SENHA"));
            Assert.That(texto, Does.Not.Contain("GRUPO"));
            Assert.That(texto, Does.Not.Match(@"\d{4}"));
        }
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
    public void EtapasDeA8_RecebemODisplaySemMudarAQuantidadeOficial()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a8, EtapasGuiadasDeAlerta.Criar(a8));

        Assert.That(etapas.Length, Is.EqualTo(1));
        Assert.That(etapas[0].tutorial, Is.EqualTo(AcaoOficialDeA8));
        Assert.That(etapas[0].animacao, Is.Empty);
        Assert.That(etapas[0].textoDisplay, Is.EqualTo("A8"));
        Assert.That(etapas[0].leds, Is.EqualTo(QuadroDeDisplayM4.LedVermelhoPiscando));
        Assert.That(TelaM4.EtapaTemConteudo(etapas[0]), Is.True);
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
