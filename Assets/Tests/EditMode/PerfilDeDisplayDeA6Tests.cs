using System.Linq;
using NUnit.Framework;

public class PerfilDeDisplayDeA6Tests
{
    #region MARK: Fixture

    private const string CodigoA6 = "A6";
    private const string AcaoVerificarDefeitoNaSolenoide = "Verifica se a solenoide está com defeito";
    private const string AcaoVerificarSinalEletrico = "Verificar a falta sinal elétrico para solenoide";
    private const int LimiteDeCaracteresDaInstrucao = 120;

    private AlertaOficial a6;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a6 = CatalogoDeAlertas.Obter(CodigoA6, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA6);
    }

    private SequenciaDeQuadrosM4 EtapaSolenoide => perfil.EtapaOficial(0);
    private SequenciaDeQuadrosM4 EtapaSinalEletrico => perfil.EtapaOficial(1);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA6_MantemAsDuasAcoesDaPagina76()
    {
        Assert.That(a6, Is.Not.Null);
        Assert.That(a6.Nome, Is.EqualTo("FALHA DE COMANDO"));
        Assert.That(a6.Padrao, Is.EqualTo("sempre ligado"));
        Assert.That(a6.Acoes.Count, Is.EqualTo(2));
        Assert.That(a6.Acoes[0], Is.EqualTo(AcaoVerificarDefeitoNaSolenoide));
        Assert.That(a6.Acoes[1], Is.EqualTo(AcaoVerificarSinalEletrico));
        Assert.That(a6.Locais.Count, Is.EqualTo(2));
        Assert.That(a6.Locais[0], Is.EqualTo("solenoide"));
        Assert.That(a6.Locais[1], Is.EqualTo("em campo"));
    }

    [Test]
    public void PerfilDeA6_ExpandeAsDuasAcoesOficiaisSemInventarUmaTerceira()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA6));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(2));
        Assert.That(perfil.CorrespondeAoCatalogo(a6), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A6NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A10"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA6));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A23"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(24));
    }

    #endregion

    #region MARK: Passo a passo auto explicativo, no padrao do A5

    [Test]
    public void NenhumQuadro_RepeteOTextoResumidoDaTabelaDeResolucao()
    {
        var instrucoes = perfil.EtapasOficiais
            .SelectMany(etapa => etapa.Quadros)
            .Select(quadro => quadro.Instrucao)
            .ToList();

        Assert.That(instrucoes, Has.None.EqualTo(AcaoVerificarDefeitoNaSolenoide));
        Assert.That(instrucoes, Has.None.EqualTo(AcaoVerificarSinalEletrico));
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
            Assert.That(quadro.TextoLcd, Is.EqualTo(CodigoA6));
            Assert.That(quadro.Instrucao, Is.Not.Null.And.Not.Empty);
            Assert.That(quadro.Instrucao, Is.EqualTo(quadro.Instrucao.Trim()));
        }
    }

    #endregion

    #region MARK: O diagnostico do A6 e a valvula nao responder ao comando, pagina 77

    [Test]
    public void PrimeiroQuadro_DizQueAValvulaNaoExecutaOComandoDaSolenoide()
    {
        Assert.That(EtapaSolenoide.Primeiro.Instrucao, Does.StartWith("Confirme o alerta A6"));
        Assert.That(EtapaSolenoide.Primeiro.Instrucao, Does.Contain("não executa o comando"));
        Assert.That(EtapaSolenoide.Em(1).Instrucao, Does.Contain("elétrica ou pneumática"));
    }

    [Test]
    public void NenhumaInstrucao_TrataOA6ComoMudancaInesperadaDePosicao()
    {
        var instrucoes = perfil.EtapasOficiais
            .SelectMany(etapa => etapa.Quadros)
            .Select(quadro => quadro.Instrucao)
            .ToList();

        Assert.That(instrucoes, Has.None.Contains("mudança de posição"),
            "esse é o diagnóstico do A7, não o do A6.");
    }

    #endregion

    #region MARK: Etapa da solenoide com defeito, paginas 17, 19 e 77

    [Test]
    public void EtapaSolenoide_GuiaAInspecaoDaBobinaEmDezQuadros()
    {
        Assert.That(EtapaSolenoide.Quantidade, Is.EqualTo(10));
        Assert.That(EtapaSolenoide.Em(2).Instrucao, Does.StartWith("PERIGO"));
        Assert.That(EtapaSolenoide.Em(3).Instrucao, Does.Contain("energia residual"));
        Assert.That(EtapaSolenoide.Em(5).Instrucao, Does.Contain("quatro parafusos"));
        Assert.That(EtapaSolenoide.Em(6).Instrucao, Does.Contain("dois parafusos de fixação"));
        Assert.That(EtapaSolenoide.Em(7).Instrucao, Does.Contain("anel de vedação"));
        Assert.That(EtapaSolenoide.Ultimo.Instrucao, Does.Contain("sinal elétrico"));
    }

    #endregion

    #region MARK: Etapa do sinal eletrico, paginas 18, 20, 22 e 23

    [Test]
    public void EtapaSinalEletrico_SegueOsDiagramasDeConexaoEmDezQuadros()
    {
        Assert.That(EtapaSinalEletrico.Quantidade, Is.EqualTo(10));
        Assert.That(EtapaSinalEletrico.Primeiro.Instrucao, Does.Contain("falta de sinal elétrico"));
        Assert.That(EtapaSinalEletrico.Em(1).Instrucao, Does.Contain("terminal aparafusável interno"));
        Assert.That(EtapaSinalEletrico.Em(3).Instrucao, Does.Contain("S1+"));
        Assert.That(EtapaSinalEletrico.Em(4).Instrucao, Does.Contain("IO-Link"));
        Assert.That(EtapaSinalEletrico.Em(6).Instrucao, Does.Contain("prensa cabos"));
        Assert.That(EtapaSinalEletrico.Em(7).Instrucao, Does.Contain("solenoide externa"));
        Assert.That(EtapaSinalEletrico.Em(8).Instrucao, Does.Contain("resina"));
    }

    #endregion

    #region MARK: Destaque piscante no lugar da verificacao fisica

    [Test]
    public void OperadorEmCampo_VeOndeOlharEmCadaVerificacao()
    {
        foreach (int indice in new[] { 4, 5, 6, 8 })
        {
            Assert.That(EtapaSolenoide.Em(indice).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaquePneumatica));
        }

        foreach (int indice in new[] { 0, 1, 5, 8 })
        {
            Assert.That(
                EtapaSinalEletrico.Em(indice).Vfx,
                Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueModuloEletronico));
        }
    }

    [Test]
    public void QuadrosSemVerificacaoFisica_NaoAcendemDestaqueNoModelo()
    {
        foreach (int indice in new[] { 0, 1, 2, 3, 7, 9 })
        {
            Assert.That(EtapaSolenoide.Em(indice).Vfx, Is.Null,
                $"o quadro {indice} da solenoide não é de verificação física e não deve destacar peça.");
        }

        foreach (int indice in new[] { 2, 3, 4, 6, 7, 9 })
        {
            Assert.That(EtapaSinalEletrico.Em(indice).Vfx, Is.Null,
                $"o quadro {indice} do sinal elétrico não é de verificação física e não deve destacar peça.");
        }
    }

    [Test]
    public void NenhumQuadro_DestacaUmaPecaQueNaoExisteNoModelo()
    {
        var vfx = perfil.EtapasOficiais
            .SelectMany(etapa => etapa.Quadros)
            .Select(quadro => quadro.Vfx)
            .Where(nome => !string.IsNullOrEmpty(nome))
            .Distinct();

        Assert.That(vfx, Is.EquivalentTo(new[]
        {
            PerfisDeDisplayDeAlerta.DestaquePneumatica,
            PerfisDeDisplayDeAlerta.DestaqueModuloEletronico,
        }));
    }

    #endregion

    #region MARK: Leds e confirmacao

    [Test]
    public void OUltimoQuadro_ConfirmaAResolucaoComOsLedsDesligados()
    {
        QuadroDeDisplayM4 ultimo = EtapaSinalEletrico.Ultimo;

        Assert.That(ultimo.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(ultimo.LedPiscando, Is.False);
        Assert.That(ultimo.Instrucao, Does.StartWith("Verifique a confirmação"));
        Assert.That(ultimo.Instrucao, Does.Contain("alerta A6 é eliminado"));
    }

    [Test]
    public void EnquantoOAlertaEstaAtivo_OLedVermelhoPisca()
    {
        var ativos = perfil.EtapasOficiais
            .SelectMany(etapa => etapa.Quadros)
            .Where(quadro => quadro != EtapaSinalEletrico.Ultimo)
            .ToList();

        Assert.That(ativos.All(quadro => quadro.Leds == EstadoLedsM4.Alerta), Is.True);
        Assert.That(ativos.All(quadro => quadro.LedPiscando), Is.True);
    }

    #endregion

    #region MARK: Animacao do botao citado em cada passo

    [Test]
    public void AnimacoesDeA6_FicamVaziasPorqueNenhumPassoUsaOsBotoes()
    {
        Assert.That(
            EtapaSolenoide.Quadros.Select(quadro => quadro.Animacao),
            Is.EqualTo(new string[] { null, null, null, null, null, null, null, null, null, null }));

        Assert.That(
            EtapaSinalEletrico.Quadros.Select(quadro => quadro.Animacao),
            Is.EqualTo(new string[] { null, null, null, null, null, null, null, null, null, null }));

        Assert.That(perfil.Layer, Is.Null, "o A6 roda na Base Layer.");
    }

    #endregion

    #region MARK: Composicao sobre as etapas guiadas

    [Test]
    public void EtapasDeA6_ExpandemAsDuasAcoesOficiaisEmVintePassosGuiados()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a6, EtapasGuiadasDeAlerta.Criar(a6));

        Assert.That(etapas, Has.Length.EqualTo(20));
        Assert.That(etapas[0].tutorial, Does.StartWith("Confirme o alerta A6"));
        Assert.That(etapas[4].vfx, Is.EqualTo("DestaquePneumatica"));
        Assert.That(etapas[10].vfx, Is.EqualTo("DestaqueModuloEletronico"));
        Assert.That(etapas.All(TelaM4.EtapaTemConteudo), Is.True);
    }

    #endregion
}
