using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

public class PerfilDeDisplayDeA1Tests
{
    #region MARK: Fixture

    private const string CodigoA1 = "A1";
    private const string AcaoVerificarArComprimido = "Verificar o fornecimento de ar comprimido";
    private const string AcaoVerificarAvarias = "Verificar possíveis avarias no conjunto válvula / atuador";
    private const int LimiteDeCaracteresDaInstrucao = 120;

    private static readonly Regex MencaoDeBotao = new Regex(@"\bB[123]\b");

    private AlertaOficial a1;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a1 = CatalogoDeAlertas.Obter(CodigoA1, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA1);
    }

    private SequenciaDeQuadrosM4 EtapaArComprimido => perfil.EtapaOficial(0);
    private SequenciaDeQuadrosM4 EtapaAvarias => perfil.EtapaOficial(1);

    private string[] TodasAsInstrucoes => perfil.EtapasOficiais
        .SelectMany(etapa => etapa.Quadros)
        .Select(quadro => quadro.Instrucao)
        .ToArray();

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA1_MantemAsDuasAcoesDaPagina11()
    {
        Assert.That(a1, Is.Not.Null);
        Assert.That(a1.Nome, Is.EqualTo("ÂNGULO MÍNIMO"));
        Assert.That(a1.Padrao, Is.EqualTo("sempre ligado"));
        Assert.That(a1.Acoes.Count, Is.EqualTo(2));
        Assert.That(a1.Acoes[0], Is.EqualTo(AcaoVerificarArComprimido));
        Assert.That(a1.Acoes[1], Is.EqualTo(AcaoVerificarAvarias));
        Assert.That(a1.Locais.Count, Is.EqualTo(2));
        Assert.That(a1.Locais[0], Is.EqualTo("gerador de ar comprimido"));
        Assert.That(a1.Locais[1], Is.EqualTo("em campo"));
    }

    [Test]
    public void PerfilDeA1_ExpandeAsDuasAcoesOficiaisSemInventarUmaTerceira()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA1));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(2));
        Assert.That(perfil.CorrespondeAoCatalogo(a1), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A1NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A24"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA1));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A2"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A3"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(13));
    }

    #endregion

    #region MARK: Passo a passo auto explicativo, no padrao do A3 e do A5

    [Test]
    public void NenhumQuadro_RepeteOTextoResumidoDaTabelaDeResolucao()
    {
        Assert.That(TodasAsInstrucoes, Has.None.EqualTo(AcaoVerificarArComprimido));
        Assert.That(TodasAsInstrucoes, Has.None.EqualTo(AcaoVerificarAvarias));
    }

    [Test]
    public void Instrucoes_CabemNaCaixaDeTextoDoPassoAPasso()
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

    #region MARK: Camada de animacao dedicada, sem passo de botao

    [Test]
    public void PerfilDeA1_UsaALayerProblema1EmVezDaBaseLayer()
    {
        Assert.That(perfil.Layer, Is.EqualTo("Problema 1"));
    }

    [Test]
    public void NenhumaInstrucao_CitaB1B2OuB3PorqueACamadaProblema1NaoTemEssesEstados()
    {
        foreach (string instrucao in TodasAsInstrucoes)
        {
            Assert.That(
                MencaoDeBotao.IsMatch(instrucao),
                Is.False,
                $"A camada Problema 1 não tem animação de botão: '{instrucao}'.");
        }
    }

    [Test]
    public void AnimacoesDeA1_SaoApenasOMovimentoDaValvulaNoQuadroDoAnguloCurto()
    {
        Assert.That(
            EtapaArComprimido.Quadros.Select(quadro => quadro.Animacao),
            Is.EqualTo(new[]
            {
                PerfisDeDisplayDeAlerta.AnimacaoProblema1,
                null,
                null,
                null,
                null,
                null,
                null,
            }));

        Assert.That(
            EtapaAvarias.Quadros.Select(quadro => quadro.Animacao),
            Is.EqualTo(new string[] { null, null, null, null, null, null, null }));
    }

    #endregion

    #region MARK: O diagnostico do A1 e curso curto, nao ausencia nem inconsistencia

    [Test]
    public void PrimeiroQuadroDeCadaEtapa_DizQueOCursoDaValvulaFicouCurto()
    {
        Assert.That(EtapaArComprimido.Primeiro.TextoLcd, Is.EqualTo(CodigoA1));
        Assert.That(EtapaArComprimido.Primeiro.Instrucao, Does.StartWith("Confirme o alerta A1"));
        Assert.That(EtapaArComprimido.Primeiro.Instrucao, Does.Contain("a válvula se moveu"));
        Assert.That(EtapaArComprimido.Primeiro.Instrucao, Does.Contain("abaixo de 30°"));

        Assert.That(EtapaAvarias.Primeiro.Instrucao, Does.Contain("curso curto"));
    }

    [Test]
    public void NenhumaInstrucao_FalaDePontosInconsistentesNemDeAusenciaDeMovimento()
    {
        Assert.That(TodasAsInstrucoes, Has.None.Contains("diferentes entre si"),
            "esse é o diagnóstico do A3, não o do A1.");
        Assert.That(TodasAsInstrucoes, Has.None.Contains("não registrou movimento nenhum"),
            "esse é o diagnóstico do A5, não o do A1.");
        Assert.That(TodasAsInstrucoes, Has.None.Contains("ausência de movimento"),
            "esse é o diagnóstico do A5, não o do A1.");
    }

    #endregion

    #region MARK: Etapa do gerador de ar comprimido, paginas 16 e 17

    [Test]
    public void EtapaArComprimido_GuiaAVerificacaoNoGeradorEmSeteQuadros()
    {
        Assert.That(EtapaArComprimido.Quantidade, Is.EqualTo(7));
        Assert.That(EtapaArComprimido.Quadros.All(quadro => quadro.TextoLcd == CodigoA1), Is.True);
    }

    [Test]
    public void EtapaArComprimido_TrazAFaixaDeOperacaoEOLimiteDestrutivo()
    {
        Assert.That(EtapaArComprimido.Em(2).Instrucao, Does.Contain("3 e 8 bar"));
        Assert.That(EtapaArComprimido.Em(2).Instrucao, Does.Contain("6 bar"));
        Assert.That(EtapaArComprimido.Em(3).Instrucao, Does.Contain("10 bar (150 psi)"));
        Assert.That(EtapaArComprimido.Em(3).Instrucao, Does.Contain("danificado permanentemente"));
    }

    [Test]
    public void CausasDePressaoInsuficiente_ViramPassosProprios()
    {
        Assert.That(EtapaArComprimido.Em(4).Instrucao, Does.Contain("mangueiras"));
        Assert.That(EtapaArComprimido.Em(5).Instrucao, Does.Contain("entopem"));
        Assert.That(EtapaArComprimido.Ultimo.Instrucao, Does.Contain("não está na linha"));
    }

    #endregion

    #region MARK: Etapa do conjunto valvula / atuador, paginas 11, 17 e 49

    [Test]
    public void EtapaAvarias_InspecionaOConjuntoEEncerraNaCalibracaoEmSeteQuadros()
    {
        Assert.That(EtapaAvarias.Quantidade, Is.EqualTo(7));
        Assert.That(EtapaAvarias.Quadros.All(quadro => quadro.TextoLcd == CodigoA1), Is.True);
        Assert.That(EtapaAvarias.Em(1).Instrucao, Does.Contain("NAMUR"));
        Assert.That(EtapaAvarias.Em(1).Instrucao, Does.Contain("38 mm"));
        Assert.That(EtapaAvarias.Em(2).Instrucao, Does.Contain("diafragma"));
        Assert.That(EtapaAvarias.Em(3).Instrucao, Does.Contain("saída de ar 4"));
        Assert.That(EtapaAvarias.Em(3).Instrucao, Does.Contain("saída de ar 2"));
        Assert.That(EtapaAvarias.Em(4).Instrucao, Does.Contain("NPT ou BSP"));
    }

    [Test]
    public void EtapaAvarias_PedeARepeticaoDaCalibracaoSemDescreverOsBotoes()
    {
        Assert.That(EtapaAvarias.Em(5).Instrucao, Does.Contain("refaça a auto calibração"));
        Assert.That(EtapaAvarias.Em(5).Instrucao, Does.Not.Contain("FAST"));
        Assert.That(EtapaAvarias.Em(5).Instrucao, Does.Not.Contain("chaveiro"));
    }

    [Test]
    public void OUltimoQuadro_ConfirmaAResolucaoComOsLedsDesligados()
    {
        QuadroDeDisplayM4 ultimo = EtapaAvarias.Ultimo;

        Assert.That(ultimo.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(ultimo.LedPiscando, Is.False);
        Assert.That(ultimo.Instrucao, Does.StartWith("Verifique a confirmação"));
        Assert.That(ultimo.Instrucao, Does.Contain("alerta A1 é eliminado"));
    }

    [Test]
    public void EnquantoOAlertaEstaAtivo_OLedVermelhoPisca()
    {
        var ativos = perfil.EtapasOficiais
            .SelectMany(etapa => etapa.Quadros)
            .Where(quadro => quadro != EtapaAvarias.Ultimo)
            .ToList();

        Assert.That(ativos.All(quadro => quadro.Leds == EstadoLedsM4.Alerta), Is.True);
        Assert.That(ativos.All(quadro => quadro.LedPiscando), Is.True);
    }

    #endregion

    #region MARK: Destaque piscante no lugar da verificacao fisica

    [Test]
    public void OperadorEmCampo_VeOndeOlharEmCadaVerificacao()
    {
        Assert.That(EtapaArComprimido.Em(1).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaquePneumatica));
        Assert.That(EtapaArComprimido.Em(4).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueMangueiras));

        foreach (int indice in new[] { 0, 1, 2 })
        {
            Assert.That(EtapaAvarias.Em(indice).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueAtuadorCopo));
        }
    }

    [Test]
    public void QuadrosSemVerificacaoFisica_NaoAcendemDestaqueNoModelo()
    {
        foreach (int indice in new[] { 0, 2, 3, 5, 6 })
        {
            Assert.That(EtapaArComprimido.Em(indice).Vfx, Is.Null,
                $"o quadro {indice} do gerador não é de verificação física e não deve destacar peça.");
        }

        foreach (int indice in new[] { 3, 4, 5, 6 })
        {
            Assert.That(EtapaAvarias.Em(indice).Vfx, Is.Null,
                $"o quadro {indice} do conjunto não é de verificação física e não deve destacar peça.");
        }
    }

    #endregion

    #region MARK: Composicao sobre as etapas guiadas

    [Test]
    public void EtapasDeA1_ExpandemAsDuasAcoesOficiaisEmQuatorzePassosGuiados()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a1, EtapasGuiadasDeAlerta.Criar(a1));

        Assert.That(etapas, Has.Length.EqualTo(14));
        Assert.That(etapas[0].tutorial, Does.StartWith("Confirme o alerta A1"));
        Assert.That(etapas[0].animacao, Is.EqualTo("PROBLEMA1"));
        Assert.That(etapas[1].vfx, Is.EqualTo("DestaquePneumatica"));
        Assert.That(etapas[7].vfx, Is.EqualTo("DestaqueAtuadorCopo"));
        Assert.That(etapas.All(TelaM4.EtapaTemConteudo), Is.True);
    }

    #endregion
}
