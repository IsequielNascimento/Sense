using System.Linq;
using NUnit.Framework;

public class PerfilDeDisplayDeA5Tests
{
    #region MARK: Fixture

    private const string CodigoA5 = "A5";
    private const string AcaoVerificarArComprimido = "Verificar o fornecimento de ar comprimido";
    private const string AcaoVerificarAvarias = "Verificar possíveis avarias no conjunto válvula / atuador";
    private const int LimiteDeCaracteresDaInstrucao = 120;

    private AlertaOficial a5;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a5 = CatalogoDeAlertas.Obter(CodigoA5, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA5);
    }

    private SequenciaDeQuadrosM4 EtapaArComprimido => perfil.EtapaOficial(0);
    private SequenciaDeQuadrosM4 EtapaAvarias => perfil.EtapaOficial(1);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA5_MantemAsDuasAcoesDaPagina76()
    {
        Assert.That(a5, Is.Not.Null);
        Assert.That(a5.Nome, Is.EqualTo("SEM MOVIMENTO"));
        Assert.That(a5.Padrao, Is.EqualTo("sempre ligado"));
        Assert.That(a5.Acoes.Count, Is.EqualTo(2));
        Assert.That(a5.Acoes[0], Is.EqualTo(AcaoVerificarArComprimido));
        Assert.That(a5.Acoes[1], Is.EqualTo(AcaoVerificarAvarias));
        Assert.That(a5.Locais.Count, Is.EqualTo(2));
        Assert.That(a5.Locais[0], Is.EqualTo("gerador de ar comprimido"));
        Assert.That(a5.Locais[1], Is.EqualTo("em campo"));
    }

    [Test]
    public void PerfilDeA5_ExpandeAsDuasAcoesOficiaisSemInventarUmaTerceira()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA5));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(2));
        Assert.That(perfil.CorrespondeAoCatalogo(a5), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A5NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A6"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA5));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A3"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(13));
    }

    #endregion

    #region MARK: Passo a passo auto explicativo, no padrao do A3

    [Test]
    public void NenhumQuadro_RepeteOTextoResumidoDaTabelaDeResolucao()
    {
        var instrucoes = perfil.EtapasOficiais
            .SelectMany(etapa => etapa.Quadros)
            .Select(quadro => quadro.Instrucao)
            .ToList();

        Assert.That(instrucoes, Has.None.EqualTo(AcaoVerificarArComprimido));
        Assert.That(instrucoes, Has.None.EqualTo(AcaoVerificarAvarias));
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

    #region MARK: O diagnostico do A5 e falta de movimento, nao movimento inconsistente

    [Test]
    public void PrimeiroQuadroDeCadaEtapa_DizQueAValvulaNaoSeMoveu()
    {
        Assert.That(EtapaArComprimido.Primeiro.TextoLcd, Is.EqualTo(CodigoA5));
        Assert.That(EtapaArComprimido.Primeiro.Instrucao, Does.StartWith("Confirme o alerta A5"));
        Assert.That(EtapaArComprimido.Primeiro.Instrucao, Does.Contain("não registrou movimento nenhum"));
        Assert.That(EtapaArComprimido.Primeiro.Instrucao, Does.Contain("ausência de movimento"));

        Assert.That(EtapaAvarias.Primeiro.Instrucao, Does.Contain("válvula parada"));
        Assert.That(EtapaAvarias.Primeiro.Instrucao, Does.Contain("travamento"));
    }

    [Test]
    public void NenhumaInstrucao_FalaDePontosInconsistentesEntreOsCiclos()
    {
        var instrucoes = perfil.EtapasOficiais
            .SelectMany(etapa => etapa.Quadros)
            .Select(quadro => quadro.Instrucao)
            .ToList();

        Assert.That(instrucoes, Has.None.Contains("diferentes entre si"),
            "esse é o diagnóstico do A3, não o do A5.");
        Assert.That(instrucoes, Has.None.Contains("mesmo curso em todos os ciclos"));
    }

    #endregion

    #region MARK: Etapa do gerador de ar comprimido, paginas 16 e 17

    [Test]
    public void EtapaArComprimido_GuiaAVerificacaoNoGeradorEmSeteQuadros()
    {
        Assert.That(EtapaArComprimido.Quantidade, Is.EqualTo(7));
        Assert.That(EtapaArComprimido.Quadros.All(quadro => quadro.TextoLcd == CodigoA5), Is.True);
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
    public void CausasDeArBloqueado_ViramPassosProprios()
    {
        Assert.That(EtapaArComprimido.Em(4).Instrucao, Does.Contain("mangueira"));
        Assert.That(EtapaArComprimido.Em(5).Instrucao, Does.Contain("entupir"));
        Assert.That(EtapaArComprimido.Ultimo.Instrucao, Does.Contain("não está na linha"));
    }

    [Test]
    public void EtapaArComprimido_NaoDisparaAnimacaoDeBotaoPorqueEUmaVerificacaoEmCampo()
    {
        Assert.That(
            EtapaArComprimido.Quadros.Where(quadro => !string.IsNullOrEmpty(quadro.Animacao)),
            Is.Empty);
    }

    #endregion

    #region MARK: Etapa do conjunto valvula / atuador, paginas 11, 13 e 17

    [Test]
    public void EtapaAvarias_InspecionaOConjuntoERefazACalibracaoEmOitoQuadros()
    {
        Assert.That(EtapaAvarias.Quantidade, Is.EqualTo(8));
        Assert.That(EtapaAvarias.Em(1).Instrucao, Does.Contain("NAMUR"));
        Assert.That(EtapaAvarias.Em(1).Instrucao, Does.Contain("38 mm"));
        Assert.That(EtapaAvarias.Em(2).Instrucao, Does.Contain("diafragma"));
        Assert.That(EtapaAvarias.Em(3).Instrucao, Does.Contain("saída de ar 4"));
        Assert.That(EtapaAvarias.Em(3).Instrucao, Does.Contain("saída de ar 2"));
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

        foreach (int indice in new[] { 3, 4, 5, 6, 7 })
        {
            Assert.That(EtapaAvarias.Em(indice).Vfx, Is.Null,
                $"o quadro {indice} do conjunto não é de verificação física e não deve destacar peça.");
        }
    }

    #endregion

    #region MARK: Procedimento de auto calibracao da pagina 49

    [Test]
    public void RefazerACalibracao_SegueOsBotoesEOsTemposDoManual()
    {
        var quadros = EtapaAvarias.Quadros;

        Assert.That(quadros[4].TextoLcd, Is.EqualTo("FAST\nSETUP"));
        Assert.That(quadros[4].ProgressoSegundos, Is.EqualTo(6f));

        Assert.That(quadros[5].TextoLcd, Is.EqualTo("CERTO"));

        Assert.That(quadros[6].TextoLcd, Is.EqualTo("ABORT"));
        Assert.That(quadros[6].ProgressoSegundos, Is.EqualTo(3f));
    }

    [Test]
    public void OUltimoQuadro_ConfirmaAResolucaoComOsLedsDesligados()
    {
        QuadroDeDisplayM4 ultimo = EtapaAvarias.Ultimo;

        Assert.That(ultimo.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(ultimo.LedPiscando, Is.False);
        Assert.That(ultimo.Instrucao, Does.StartWith("Verifique a confirmação"));
        Assert.That(ultimo.Instrucao, Does.Contain("alerta A5 é eliminado"));
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

    #region MARK: Animacao do botao citado em cada passo

    [Test]
    public void AnimacoesDeA5_SeguemOsBotoesCitadosEmCadaPasso()
    {
        Assert.That(
            EtapaArComprimido.Quadros.Select(quadro => quadro.Animacao),
            Is.EqualTo(new string[] { null, null, null, null, null, null, null }));

        Assert.That(
            EtapaAvarias.Quadros.Select(quadro => quadro.Animacao),
            Is.EqualTo(new[]
            {
                null,
                null,
                null,
                null,
                AnimacaoDeBotaoM4.B3,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B1,
                null,
            }));
    }

    #endregion

    #region MARK: Composicao sobre as etapas guiadas

    [Test]
    public void EtapasDeA5_ExpandemAsDuasAcoesOficiaisEmQuinzePassosGuiados()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a5, EtapasGuiadasDeAlerta.Criar(a5));

        Assert.That(etapas, Has.Length.EqualTo(15));
        Assert.That(etapas[0].tutorial, Does.StartWith("Confirme o alerta A5"));
        Assert.That(etapas[1].vfx, Is.EqualTo("DestaquePneumatica"));
        Assert.That(etapas[7].vfx, Is.EqualTo("DestaqueAtuadorCopo"));
        Assert.That(etapas.All(TelaM4.EtapaTemConteudo), Is.True);
    }

    #endregion
}
