using System.Linq;
using NUnit.Framework;

public class PerfilDeDisplayDeA2Tests
{
    #region MARK: Fixture

    private const string CodigoA2 = "A2";
    private const string AcaoAumentarTempoLimite = "Aumentar tempo limite";
    private const string AcaoVerificarAvarias = "Verificar possíveis avarias no conjunto válvula / atuador";
    private const int QuantidadeDeQuadrosAumentarTempoLimite = 7;
    private const int QuantidadeDeQuadrosAvarias = 9;
    private const int LimiteDeCaracteresDaInstrucao = 120;

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

    private string[] TodasAsInstrucoes => perfil.EtapasOficiais
        .SelectMany(etapa => etapa.Quadros)
        .Select(quadro => quadro.Instrucao)
        .ToArray();

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA2_MantemAsDuasAcoesDaPagina11()
    {
        Assert.That(a2, Is.Not.Null);
        Assert.That(a2.Nome, Is.EqualTo("TEMPO LIMITE"));
        Assert.That(a2.Padrao, Is.EqualTo("sempre ligado"));
        Assert.That(a2.Acoes.Count, Is.EqualTo(2));
        Assert.That(a2.Acoes[0], Is.EqualTo(AcaoAumentarTempoLimite));
        Assert.That(a2.Acoes[1], Is.EqualTo(AcaoVerificarAvarias));
        Assert.That(a2.Locais.Count, Is.EqualTo(2));
        Assert.That(a2.Locais[0], Is.EqualTo("menu tempo max. cal"));
        Assert.That(a2.Locais[1], Is.EqualTo("em campo"));
    }

    [Test]
    public void PerfilDeA2_ExpandeAsDuasAcoesOficiaisSemInventarUmaTerceira()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA2));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(2));
        Assert.That(perfil.CorrespondeAoCatalogo(a2), Is.True);
        Assert.That(perfil.MecanismoDeAtivacaoConfirmado, Is.False);
    }

    [Test]
    public void RegistroDePerfis_A2NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A24"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA2));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(13));
    }

    #endregion

    #region MARK: Sem camada de animacao dedicada (usa a Base Layer, igual ao A8)

    [Test]
    public void PerfilDeA2_NaoDeclaraLayerDedicada()
    {
        Assert.That(perfil.Layer, Is.Null);
    }

    #endregion

    #region MARK: Passo a passo auto explicativo, no padrao do A3 e do A5

    [Test]
    public void NenhumQuadro_RepeteOTextoResumidoDaTabelaDeResolucao()
    {
        Assert.That(TodasAsInstrucoes, Has.None.EqualTo(AcaoAumentarTempoLimite));
        Assert.That(TodasAsInstrucoes, Has.None.EqualTo(AcaoVerificarAvarias));
    }

    [Test]
    public void TodaInstrucao_CabeNaCaixaDeTutorialDoDispositivo()
    {
        foreach (QuadroDeDisplayM4 quadro in perfil.EtapasOficiais.SelectMany(etapa => etapa.Quadros))
        {
            Assert.That(
                quadro.Instrucao.Length,
                Is.LessThanOrEqualTo(LimiteDeCaracteresDaInstrucao),
                $"Instrução longa demais no quadro '{quadro.TextoLcd}'.");
        }
    }

    [Test]
    public void TodoQuadro_TemTextoDeLcdEInstrucaoSemEspacoNasPontas()
    {
        foreach (QuadroDeDisplayM4 quadro in perfil.EtapasOficiais.SelectMany(etapa => etapa.Quadros))
        {
            Assert.That(quadro.TextoLcd, Is.Not.Null.And.Not.Empty);
            Assert.That(quadro.TextoLcd, Is.EqualTo(quadro.TextoLcd.Trim()));
            Assert.That(quadro.Instrucao, Is.Not.Null.And.Not.Empty);
            Assert.That(quadro.Instrucao, Is.EqualTo(quadro.Instrucao.Trim()));
        }
    }

    #endregion

    #region MARK: O diagnostico do A2 e lentidao, nao curso curto nem ausencia de movimento

    [Test]
    public void PrimeiroQuadroDeCadaEtapa_DizQueAValvulaCompletaOCursoMasDemora()
    {
        Assert.That(EtapaAumentarTempoLimite.Primeiro.TextoLcd, Is.EqualTo(CodigoA2));
        Assert.That(EtapaAumentarTempoLimite.Primeiro.Instrucao, Does.StartWith("Confirme o alerta A2"));
        Assert.That(EtapaAumentarTempoLimite.Primeiro.Instrucao, Does.Contain("mais tempo"));
        Assert.That(EtapaAumentarTempoLimite.Primeiro.Instrucao, Does.Contain("C6 TEMPO MAX CAL"));

        Assert.That(EtapaAvarias.Primeiro.Instrucao, Does.Contain("curso completo"));
        Assert.That(EtapaAvarias.Primeiro.Instrucao, Does.Contain("lento"));
    }

    [Test]
    public void NenhumaInstrucao_UsaODiagnosticoDeA1DeA3OuDeA5()
    {
        Assert.That(TodasAsInstrucoes, Has.None.Contains("30°"),
            "o ângulo mínimo é o diagnóstico do A1, não o do A2.");
        Assert.That(TodasAsInstrucoes, Has.None.Contains("curso curto"),
            "o curso curto é o diagnóstico do A1, não o do A2.");
        Assert.That(TodasAsInstrucoes, Has.None.Contains("diferentes entre si"),
            "os pontos divergentes entre os ciclos são o diagnóstico do A3, não o do A2.");
        Assert.That(TodasAsInstrucoes, Has.None.Contains("não registrou movimento"),
            "a ausência de movimento é o diagnóstico do A5, não o do A2.");
        Assert.That(TodasAsInstrucoes, Has.None.Contains("ausência de movimento"));
    }

    [Test]
    public void AEtapaDeCampo_ProcuraAsAvariasQueCausamLentidao()
    {
        string[] instrucoes = EtapaAvarias.Quadros.Select(quadro => quadro.Instrucao).ToArray();
        string texto = string.Join(" ", instrucoes);

        Assert.That(texto, Does.Contain("restringe a passagem de ar"));
        Assert.That(texto, Does.Contain("atrito"));
        Assert.That(texto, Does.Contain("emperramento"));
        Assert.That(texto, Does.Contain("subdimensionado"));
        Assert.That(texto, Does.Contain("lentidão"));
    }

    #endregion

    #region MARK: Navegacao ate C6 - TEMPO MAX CAL, paginas 51 e 52

    [Test]
    public void EtapaAumentarTempoLimite_TemSeteQuadrosSemTextoVazio()
    {
        Assert.That(EtapaAumentarTempoLimite.Quantidade, Is.EqualTo(QuantidadeDeQuadrosAumentarTempoLimite));

        foreach (QuadroDeDisplayM4 quadro in EtapaAumentarTempoLimite.Quadros)
        {
            Assert.That(quadro.TextoLcd, Is.Not.Empty);
            Assert.That(quadro.Instrucao, Is.Not.Null.And.Not.Empty);
            Assert.That(quadro.Vfx, Is.Null.Or.Empty);
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
        Assert.That(ajuste.Instrucao, Does.Contain("B1"));
        Assert.That(ajuste.Instrucao, Does.Contain("B2"));
        Assert.That(ajuste.Instrucao, Does.Contain("B3"));
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

    #region MARK: Etapa em campo, paginas 16 e 49

    [Test]
    public void EtapaAvarias_TemNoveQuadrosGuiados()
    {
        Assert.That(EtapaAvarias.Quantidade, Is.EqualTo(QuantidadeDeQuadrosAvarias));
        Assert.That(EtapaAvarias.Quadros.Take(6).All(quadro => quadro.TextoLcd == CodigoA2), Is.True);
    }

    [Test]
    public void EtapaAvarias_DestacaAsPecasCertasNosQuadrosCertos()
    {
        Assert.That(
            EtapaAvarias.Quadros.Select(quadro => quadro.Vfx),
            Is.EqualTo(new[]
            {
                PerfisDeDisplayDeAlerta.DestaqueAtuadorCopo,
                PerfisDeDisplayDeAlerta.DestaqueMangueiras,
                PerfisDeDisplayDeAlerta.DestaquePneumatica,
                PerfisDeDisplayDeAlerta.DestaqueAtuadorCopo,
                PerfisDeDisplayDeAlerta.DestaqueAtuadorCopo,
                null,
                null,
                null,
                null,
            }));
    }

    [Test]
    public void EtapaAvarias_TerminaRefazendoAAutoCalibracao()
    {
        QuadroDeDisplayM4 fastSetup = EtapaAvarias.Em(6);
        Assert.That(fastSetup.TextoLcd, Does.Contain("FAST"));
        Assert.That(fastSetup.Instrucao, Does.Contain("polo Norte"));
        Assert.That(fastSetup.Instrucao, Does.Contain("B3"));
        Assert.That(fastSetup.ProgressoSegundos, Is.EqualTo(6f));

        QuadroDeDisplayM4 certo = EtapaAvarias.Em(7);
        Assert.That(certo.TextoLcd, Is.EqualTo("CERTO"));
        Assert.That(certo.Instrucao, Does.Contain("polo Sul"));
        Assert.That(certo.Instrucao, Does.Contain("B2"));

        QuadroDeDisplayM4 confirmacao = EtapaAvarias.Ultimo;
        Assert.That(confirmacao.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(confirmacao.Instrucao, Does.Contain("A2"));
        Assert.That(confirmacao.Instrucao, Does.Contain("eliminado"));
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
                null,
                null,
                null,
                null,
                null,
                AnimacaoDeBotaoM4.B3,
                AnimacaoDeBotaoM4.B2,
                null,
            }));
    }

    #endregion

    #region MARK: Composicao sobre as etapas guiadas

    [Test]
    public void EtapasDeA2_ExpandemAsDuasAcoesOficiais()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a2, EtapasGuiadasDeAlerta.Criar(a2));

        Assert.That(etapas, Has.Length.EqualTo(
            QuantidadeDeQuadrosAumentarTempoLimite + QuantidadeDeQuadrosAvarias));
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.tutorial)), Is.True);
        Assert.That(etapas.Select(etapa => etapa.animacao), Is.EqualTo(
            EtapaAumentarTempoLimite.Quadros.Concat(EtapaAvarias.Quadros)
                .Select(quadro => quadro.Animacao ?? string.Empty)));
    }

    #endregion
}
