using System.Linq;
using NUnit.Framework;

public class PerfilDeDisplayDeA2Tests
{
    #region MARK: Fixture

    private const string CodigoA2 = "A2";
    private const string AcaoAumentarTempoLimite = "Aumentar tempo limite";
    private const string AcaoVerificarAvarias = "Verificar possíveis avarias no conjunto válvula / atuador";
    private const int QuantidadeDeQuadrosAumentarTempoLimite = 7;

    private AlertaOficial a2;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a2 = CatalogoDeAlertas.Obter(CodigoA2, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA2);
    }

    private SequenciaDeQuadrosM4 EtapaAumentarTempoLimite => perfil.EtapaOficial(0);
    private SequenciaDeQuadrosM4 EtapaAvarias => perfil.EtapaOficial(1);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA2_MantemAsDuasAcoesDaPagina11()
    {
        Assert.That(a2, Is.Not.Null);
        Assert.That(a2.Nome, Is.EqualTo("TEMPO LIMITE"));
        Assert.That(a2.Acoes.Count, Is.EqualTo(2));
        Assert.That(a2.Acoes[0], Is.EqualTo(AcaoAumentarTempoLimite));
        Assert.That(a2.Acoes[1], Is.EqualTo(AcaoVerificarAvarias));
    }

    [Test]
    public void PerfilDeA2_TemExatamenteDuasEtapasOficiais()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA2));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(2));
        Assert.That(perfil.CorrespondeAoCatalogo(a2), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A2NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A24"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA2));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(10));
    }

    #endregion

    #region MARK: Sem camada de animacao dedicada (usa a Base Layer, igual ao A8)

    [Test]
    public void PerfilDeA2_NaoDeclaraLayerDedicada()
    {
        Assert.That(perfil.Layer, Is.Null);
    }

    #endregion

    #region MARK: Navegacao ate C6 - TEMPO MAX CAL, pagina 52

    [Test]
    public void EtapaAumentarTempoLimite_TemSeteQuadrosSemTextoVazio()
    {
        Assert.That(EtapaAumentarTempoLimite.Quantidade, Is.EqualTo(QuantidadeDeQuadrosAumentarTempoLimite));

        foreach (QuadroDeDisplayM4 quadro in EtapaAumentarTempoLimite.Quadros)
        {
            Assert.That(quadro.TextoLcd, Is.Not.Empty);
            Assert.That(quadro.Instrucao, Is.Not.Null.And.Not.Empty);
        }
    }

    [Test]
    public void EntradaNoMenu_UsaB2PorSeisSegundos()
    {
        QuadroDeDisplayM4 entrada = EtapaAumentarTempoLimite.Em(1);

        Assert.That(entrada.TextoLcd, Is.EqualTo("MENU"));
        Assert.That(entrada.Instrucao, Does.Contain("B2"));
        Assert.That(entrada.Instrucao, Does.Contain("6 segundos"));
        Assert.That(entrada.ProgressoSegundos, Is.EqualTo(6f));
        Assert.That(entrada.Animacao, Is.EqualTo(AnimacaoDeBotaoM4.B2));
    }

    [Test]
    public void NavegacaoAteC6_MencionaTempoMaxCal()
    {
        QuadroDeDisplayM4 navegacao = EtapaAumentarTempoLimite.Em(3);

        Assert.That(navegacao.TextoLcd, Is.EqualTo("C6"));
        Assert.That(navegacao.Instrucao, Does.Contain("C6"));
        Assert.That(navegacao.Instrucao, Does.Contain("TEMPO MAX CAL"));
    }

    [Test]
    public void AjusteDeValor_PermiteEntre10E120Segundos()
    {
        QuadroDeDisplayM4 ajuste = EtapaAumentarTempoLimite.Em(4);

        Assert.That(ajuste.TextoLcd, Is.EqualTo("C6"));
        Assert.That(ajuste.Instrucao, Does.Contain("10"));
        Assert.That(ajuste.Instrucao, Does.Contain("120"));
    }

    [Test]
    public void Confirmacao_IndicaQueOAlertaA2FoiEliminado()
    {
        QuadroDeDisplayM4 confirmacao = EtapaAumentarTempoLimite.Em(5);

        Assert.That(confirmacao.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(confirmacao.Instrucao, Does.Contain("A2"));
        Assert.That(confirmacao.Instrucao, Does.Contain("eliminado"));
    }

    #endregion

    #region MARK: Segundo passo, mesmo destaque visual do A1

    [Test]
    public void EtapaAvarias_TemUmUnicoQuadroComOVfxDeDestaque()
    {
        Assert.That(EtapaAvarias.Quantidade, Is.EqualTo(1));

        QuadroDeDisplayM4 quadro = EtapaAvarias.Primeiro;
        Assert.That(quadro.Instrucao, Is.EqualTo(AcaoVerificarAvarias));
        Assert.That(quadro.Vfx, Is.EqualTo("DestaqueAtuadorCopo"));
        Assert.That(quadro.Animacao, Is.Null.Or.Empty);
    }

    #endregion

    #region MARK: Animacao do botao citado em cada passo

    [Test]
    public void AnimacoesDeA2_SeguemOsBotoesCitadosEmCadaPasso()
    {
        Assert.That(
            EtapaAumentarTempoLimite.Quadros.Concat(EtapaAvarias.Quadros).Select(quadro => quadro.Animacao),
            Is.EqualTo(new[]
            {
                null,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B123,
                AnimacaoDeBotaoM4.B123,
                null,
                AnimacaoDeBotaoM4.B1,
                null,
            }));
    }

    #endregion

    #region MARK: Composicao sobre as etapas guiadas

    [Test]
    public void EtapasDeA2_ExpandemAsDuasAcoesOficiais()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a2, EtapasGuiadasDeAlerta.Criar(a2));

        Assert.That(etapas, Has.Length.EqualTo(QuantidadeDeQuadrosAumentarTempoLimite + 1));
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.tutorial)), Is.True);
        Assert.That(etapas.Last().vfx, Is.EqualTo("DestaqueAtuadorCopo"));
        Assert.That(etapas.Select(etapa => etapa.animacao), Is.EqualTo(
            EtapaAumentarTempoLimite.Quadros.Concat(EtapaAvarias.Quadros)
                .Select(quadro => quadro.Animacao ?? string.Empty)));
    }

    #endregion
}
