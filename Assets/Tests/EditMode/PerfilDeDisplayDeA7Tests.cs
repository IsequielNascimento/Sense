using System.Linq;
using NUnit.Framework;

public class PerfilDeDisplayDeA7Tests
{
    #region MARK: Fixture

    private const string CodigoA7 = "A7";
    private const string AcaoVerificarAcionamentoManual = "Verificar acionamento manual da válvula";
    private const string AcaoVerificarConexoesPneumaticas = "Verificar conexões pneumáticas";
    private const int LimiteDeCaracteresDaInstrucao = 120;

    private AlertaOficial a7;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a7 = CatalogoDeAlertas.Obter(CodigoA7, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA7);
    }

    private SequenciaDeQuadrosM4 EtapaAcionamentoManual => perfil.EtapaOficial(0);
    private SequenciaDeQuadrosM4 EtapaConexoesPneumaticas => perfil.EtapaOficial(1);

    private string[] TodasAsInstrucoes => perfil.EtapasOficiais
        .SelectMany(etapa => etapa.Quadros)
        .Select(quadro => quadro.Instrucao)
        .ToArray();

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA7_MantemAsDuasAcoesDaPagina76()
    {
        Assert.That(a7, Is.Not.Null);
        Assert.That(a7.Nome, Is.EqualTo("MUDANÇA INESPERADA"));
        Assert.That(a7.Padrao, Is.EqualTo("sempre ligado"));
        Assert.That(a7.OQueE, Is.EqualTo("a posição da válvula é continuamente monitorada"));
        Assert.That(
            a7.QuandoOcorre,
            Is.EqualTo("alerta devido a uma mudança de posição não esperada, independente do comando da solenoide"));
        Assert.That(a7.Acoes.Count, Is.EqualTo(2));
        Assert.That(a7.Acoes[0], Is.EqualTo(AcaoVerificarAcionamentoManual));
        Assert.That(a7.Acoes[1], Is.EqualTo(AcaoVerificarConexoesPneumaticas));
    }

    [Test]
    public void CatalogoDeA7_TemDuasAcoesEUmLocalSo()
    {
        Assert.That(a7.Locais.Count, Is.EqualTo(1));
        Assert.That(a7.Locais[0], Is.EqualTo("em campo"));
        Assert.That(a7.Acoes.Count, Is.GreaterThan(a7.Locais.Count));
    }

    [Test]
    public void PerfilDeA7_ExpandeAsDuasAcoesOficiaisSemInventarUmaTerceira()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA7));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(2));
        Assert.That(perfil.CorrespondeAoCatalogo(a7), Is.True);
        Assert.That(perfil.MecanismoDeAtivacaoConfirmado, Is.False);
        Assert.That(perfil.Layer, Is.Null);
    }

    [Test]
    public void RegistroDePerfis_FechaOCatalogoComOsVinteEQuatroAlertas()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A10"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA7));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(24));

        foreach (AlertaOficial alerta in CatalogoDeAlertas.Carregar("pt"))
        {
            Assert.That(
                PerfisDeDisplayDeAlerta.Existe(alerta.Codigo),
                Is.True,
                $"{alerta.Codigo} está no catálogo e precisa de perfil de display.");
        }
    }

    #endregion

    #region MARK: Passo a passo auto explicativo

    [Test]
    public void NenhumQuadro_RepeteOTextoResumidoDaTabelaDeResolucao()
    {
        Assert.That(TodasAsInstrucoes, Has.None.EqualTo(AcaoVerificarAcionamentoManual));
        Assert.That(TodasAsInstrucoes, Has.None.EqualTo(AcaoVerificarConexoesPneumaticas));
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
            Assert.That(quadro.TextoLcd, Is.EqualTo(CodigoA7));
            Assert.That(quadro.Instrucao, Is.Not.Null.And.Not.Empty);
            Assert.That(quadro.Instrucao, Is.EqualTo(quadro.Instrucao.Trim()));
        }
    }

    #endregion

    #region MARK: O diagnostico do A7 e movimento sem comando, nao falha de resposta

    [Test]
    public void PrimeiroQuadroDeCadaEtapa_DizQueAValvulaSeMoveuSemComando()
    {
        Assert.That(EtapaAcionamentoManual.Primeiro.Instrucao, Does.StartWith("Confirme o alerta A7"));
        Assert.That(EtapaAcionamentoManual.Primeiro.Instrucao, Does.Contain("sem comando da solenoide"));
        Assert.That(EtapaAcionamentoManual.Primeiro.Instrucao, Does.Contain("Não é falha de resposta"));
        Assert.That(EtapaAcionamentoManual.Primeiro.Instrucao, Does.Contain("movimento sozinho"));

        Assert.That(EtapaConexoesPneumaticas.Primeiro.Instrucao, Does.Contain("sem comando"));
        Assert.That(EtapaConexoesPneumaticas.Primeiro.Instrucao, Does.Contain("conexões pneumáticas"));
    }

    [Test]
    public void NenhumaInstrucao_DescreveFalhaDeRespostaAUmComando()
    {
        Assert.That(TodasAsInstrucoes, Has.None.Contains("30°"),
            "esse é o diagnóstico do A1, não o do A7.");
        Assert.That(TodasAsInstrucoes, Has.None.Contains("C6"),
            "esse é o diagnóstico do A2, não o do A7.");
        Assert.That(TodasAsInstrucoes, Has.None.Contains("TEMPO MAX CAL"),
            "esse é o diagnóstico do A2, não o do A7.");
        Assert.That(TodasAsInstrucoes, Has.None.Contains("diferentes entre si"),
            "esse é o diagnóstico do A3, não o do A7.");
        Assert.That(TodasAsInstrucoes, Has.None.Contains("ausência de movimento"),
            "esse é o diagnóstico do A5, não o do A7.");
        Assert.That(TodasAsInstrucoes, Has.None.Contains("não executa o comando"),
            "esse é o diagnóstico do A6, não o do A7.");
    }

    #endregion

    #region MARK: Etapa do acionamento manual, paginas 7 e 48

    [Test]
    public void EtapaAcionamentoManual_GuiaAProcuraDoAcionadorFisicoEmNoveQuadros()
    {
        Assert.That(EtapaAcionamentoManual.Quantidade, Is.EqualTo(9));
        Assert.That(EtapaAcionamentoManual.Em(1).Instrucao, Does.Contain("sem contato"));
        Assert.That(EtapaAcionamentoManual.Em(1).Instrucao, Does.Contain("Hall"));
        Assert.That(EtapaAcionamentoManual.Em(2).Instrucao, Does.Contain("acionador manual com trava"));
        Assert.That(EtapaAcionamentoManual.Em(3).Instrucao, Does.Contain("chave de fenda"));
        Assert.That(EtapaAcionamentoManual.Em(4).Instrucao, Does.Contain("destrave"));
    }

    [Test]
    public void EtapaAcionamentoManual_ExplicaPorQueODisplayNaoAnunciouOMovimento()
    {
        Assert.That(EtapaAcionamentoManual.Em(5).Instrucao, Does.Contain("Forc Aberta"));
        Assert.That(EtapaAcionamentoManual.Em(6).Instrucao, Does.Contain("acionador físico"));
        Assert.That(EtapaAcionamentoManual.Em(6).Instrucao, Does.Contain("não gera mensagem no display"));
        Assert.That(EtapaAcionamentoManual.Em(7).Instrucao, Does.Contain("sistema de controle"));
    }

    [Test]
    public void EtapaAcionamentoManual_TerminaEncaminhandoParaAPneumatica()
    {
        Assert.That(EtapaAcionamentoManual.Ultimo.Instrucao, Does.Contain("veio pelo ar"));
        Assert.That(EtapaAcionamentoManual.Ultimo.Instrucao, Does.Contain("conexões pneumáticas"));
    }

    #endregion

    #region MARK: Etapa das conexoes pneumaticas, paginas 16 e 17

    [Test]
    public void EtapaConexoesPneumaticas_PercorreAsPortasEAsMangueirasEmOitoQuadros()
    {
        Assert.That(EtapaConexoesPneumaticas.Quantidade, Is.EqualTo(8));
        Assert.That(EtapaConexoesPneumaticas.Em(1).Instrucao, Does.Contain("entrada de ar 1"));
        Assert.That(EtapaConexoesPneumaticas.Em(1).Instrucao, Does.Contain("saída de ar 4"));
        Assert.That(EtapaConexoesPneumaticas.Em(1).Instrucao, Does.Contain("saída de ar 2"));
        Assert.That(EtapaConexoesPneumaticas.Em(2).Instrucao, Does.Contain("trocadas"));
        Assert.That(EtapaConexoesPneumaticas.Em(3).Instrucao, Does.Contain("vazamento"));
        Assert.That(EtapaConexoesPneumaticas.Em(4).Instrucao, Does.Contain("Desligue a linha"));
        Assert.That(EtapaConexoesPneumaticas.Em(5).Instrucao, Does.Contain("tamponamento"));
        Assert.That(EtapaConexoesPneumaticas.Em(6).Instrucao, Does.Contain("NPT ou BSP"));
    }

    #endregion

    #region MARK: Destaque piscante no lugar da verificacao fisica

    [Test]
    public void OperadorEmCampo_VeOndeOlharEmCadaVerificacao()
    {
        foreach (int indice in new[] { 0, 2, 3, 4 })
        {
            Assert.That(
                EtapaAcionamentoManual.Em(indice).Vfx,
                Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueAtuadorCopo));
        }

        foreach (int indice in new[] { 0, 1, 5, 6 })
        {
            Assert.That(
                EtapaConexoesPneumaticas.Em(indice).Vfx,
                Is.EqualTo(PerfisDeDisplayDeAlerta.DestaquePneumatica));
        }

        foreach (int indice in new[] { 2, 3, 4 })
        {
            Assert.That(
                EtapaConexoesPneumaticas.Em(indice).Vfx,
                Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueMangueiras));
        }
    }

    [Test]
    public void QuadrosSemVerificacaoFisica_NaoAcendemDestaqueNoModelo()
    {
        foreach (int indice in new[] { 1, 5, 6, 7, 8 })
        {
            Assert.That(EtapaAcionamentoManual.Em(indice).Vfx, Is.Null,
                $"o quadro {indice} do acionamento manual não é de verificação física e não deve destacar peça.");
        }

        Assert.That(EtapaConexoesPneumaticas.Ultimo.Vfx, Is.Null,
            "o quadro de confirmação não é de verificação física e não deve destacar peça.");
    }

    #endregion

    #region MARK: Leds e confirmacao

    [Test]
    public void OUltimoQuadro_ConfirmaAResolucaoComOsLedsDesligados()
    {
        QuadroDeDisplayM4 ultimo = EtapaConexoesPneumaticas.Ultimo;

        Assert.That(ultimo.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(ultimo.LedPiscando, Is.False);
        Assert.That(ultimo.Instrucao, Does.StartWith("Verifique a confirmação"));
        Assert.That(ultimo.Instrucao, Does.Contain("só se move sob comando"));
    }

    [Test]
    public void EnquantoOAlertaEstaAtivo_OLedVermelhoPisca()
    {
        var ativos = perfil.EtapasOficiais
            .SelectMany(etapa => etapa.Quadros)
            .Where(quadro => quadro != EtapaConexoesPneumaticas.Ultimo)
            .ToList();

        Assert.That(ativos.All(quadro => quadro.Leds == EstadoLedsM4.Alerta), Is.True);
        Assert.That(ativos.All(quadro => quadro.LedPiscando), Is.True);
    }

    #endregion

    #region MARK: Animacao do botao citado em cada passo

    [Test]
    public void AnimacoesDeA7_SeguemOsBotoesCitadosEmCadaPasso()
    {
        Assert.That(
            EtapaAcionamentoManual.Quadros.Select(quadro => quadro.Animacao),
            Is.EqualTo(new[]
            {
                null,
                null,
                null,
                null,
                null,
                AnimacaoDeBotaoM4.B12,
                null,
                null,
                null,
            }));

        Assert.That(
            EtapaConexoesPneumaticas.Quadros.Select(quadro => quadro.Animacao),
            Is.EqualTo(new string[] { null, null, null, null, null, null, null, null }));
    }

    [Test]
    public void TodaAnimacaoDeA7_SaiDoTextoDoPassoENaoDeUmValorFixo()
    {
        foreach (QuadroDeDisplayM4 quadro in perfil.EtapasOficiais.SelectMany(etapa => etapa.Quadros))
        {
            Assert.That(quadro.Animacao, Is.EqualTo(AnimacaoDeBotaoM4.Derivar(quadro.Instrucao)));
        }
    }

    #endregion

    #region MARK: Composicao sobre as etapas guiadas

    [Test]
    public void EtapasDeA7_ExpandemAsDuasAcoesOficiaisEmDezessetePassosGuiados()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a7, EtapasGuiadasDeAlerta.Criar(a7));

        Assert.That(etapas, Has.Length.EqualTo(17));
        Assert.That(etapas[0].tutorial, Does.StartWith("Confirme o alerta A7"));
        Assert.That(etapas[0].vfx, Is.EqualTo("DestaqueAtuadorCopo"));
        Assert.That(etapas[9].vfx, Is.EqualTo("DestaquePneumatica"));
        Assert.That(etapas[11].vfx, Is.EqualTo("DestaqueMangueiras"));
        Assert.That(etapas.All(TelaM4.EtapaTemConteudo), Is.True);
    }

    #endregion
}
