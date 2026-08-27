using System.Linq;
using NUnit.Framework;

public class PerfilDeDisplayDeA25Tests
{
    #region MARK: Fixture

    private const string CodigoA25 = "A25";
    private const string AcaoVerificarConexaoDasSaidas = "Verificar conexão das saídas";
    private const int LimiteDeCaracteresDaInstrucao = 120;

    private AlertaOficial a25;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a25 = CatalogoDeAlertas.Obter(CodigoA25, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA25);
    }

    private SequenciaDeQuadrosM4 EtapaSaidas => perfil.EtapaOficial(0);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA25_MantemAAcaoUnicaComOsDoisLocaisDaPagina76()
    {
        Assert.That(a25, Is.Not.Null);
        Assert.That(a25.Nome, Is.EqualTo("SAÍDA CURTO"));
        Assert.That(a25.Padrao, Is.EqualTo("habilitado"));
        Assert.That(a25.Acoes.Count, Is.EqualTo(1));
        Assert.That(a25.Acoes[0], Is.EqualTo(AcaoVerificarConexaoDasSaidas));
        Assert.That(a25.Locais.Count, Is.EqualTo(2));
        Assert.That(a25.Locais[0], Is.EqualTo("em campo"));
        Assert.That(a25.Locais[1], Is.EqualTo("painel de controle"));
    }

    [Test]
    public void PerfilDeA25_TemUmaSequenciaPorAcao_NaoUmaPorLocal()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA25));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(1));
        Assert.That(perfil.CorrespondeAoCatalogo(a25), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A25NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A10"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA25));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A6"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(24));
    }

    #endregion

    #region MARK: Passo a passo auto explicativo, no padrao do A5

    [Test]
    public void NenhumQuadro_RepeteOTextoResumidoDaTabelaDeResolucao()
    {
        Assert.That(
            EtapaSaidas.Quadros.Select(quadro => quadro.Instrucao),
            Has.None.EqualTo(AcaoVerificarConexaoDasSaidas));
    }

    [Test]
    public void Instrucoes_CabemNaCaixaDeTextoDoPassoAPasso()
    {
        foreach (QuadroDeDisplayM4 quadro in EtapaSaidas.Quadros)
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
        foreach (QuadroDeDisplayM4 quadro in EtapaSaidas.Quadros)
        {
            Assert.That(quadro.TextoLcd, Is.EqualTo(CodigoA25));
            Assert.That(quadro.Instrucao, Is.Not.Null.And.Not.Empty);
            Assert.That(quadro.Instrucao, Is.EqualTo(quadro.Instrucao.Trim()));
        }
    }

    #endregion

    #region MARK: O A25 so existe no modelo IO-Link, pagina 55

    [Test]
    public void UmQuadro_AvisaQueOAlertaSoExisteNoModeloIoLink()
    {
        Assert.That(EtapaSaidas.Em(1).Instrucao, Does.Contain("IO-Link"));
        Assert.That(EtapaSaidas.Em(1).Instrucao, Does.Contain("apenas"));
    }

    [Test]
    public void PrimeiroQuadro_DizQueOCurtoPodeEstarNaSaidaOuNaCarga()
    {
        Assert.That(EtapaSaidas.Primeiro.Instrucao, Does.StartWith("Confirme o alerta A25"));
        Assert.That(EtapaSaidas.Primeiro.Instrucao, Does.Contain("saídas PNP"));
        Assert.That(EtapaSaidas.Primeiro.Instrucao, Does.Contain("carga"));
    }

    #endregion

    #region MARK: Material do diagrama de conexao PNP, pagina 23

    [Test]
    public void EtapaSaidas_TrazOQueCadaSaidaIndicaEOLimiteDeChaveamento()
    {
        Assert.That(EtapaSaidas.Quantidade, Is.EqualTo(10));
        Assert.That(EtapaSaidas.Em(2).Instrucao, Does.Contain("saída 1"));
        Assert.That(EtapaSaidas.Em(2).Instrucao, Does.Contain("saída 2"));
        Assert.That(EtapaSaidas.Em(3).Instrucao, Does.StartWith("PERIGO"));
        Assert.That(EtapaSaidas.Em(4).Instrucao, Does.Contain("50 mA"));
        Assert.That(EtapaSaidas.Em(6).Instrucao, Does.Contain("SINK"));
        Assert.That(EtapaSaidas.Em(7).Instrucao, Does.Contain("M12"));
    }

    [Test]
    public void OsDoisLocaisOficiais_ViramPassosProprios()
    {
        Assert.That(EtapaSaidas.Em(5).Instrucao, Does.Contain("painel de controle"));
        Assert.That(EtapaSaidas.Em(7).Instrucao, Does.Contain("terminal aparafusável interno"));
    }

    #endregion

    #region MARK: Nota normativa dos alertas 19 a 25, pagina 11

    [Test]
    public void UmQuadro_ExplicaQueDesligarOAlertaNaoApagaAIndicacaoFisica()
    {
        Assert.That(EtapaSaidas.Em(8).Instrucao, Does.Contain("notificação no aplicativo"));
        Assert.That(EtapaSaidas.Em(8).Instrucao, Does.Contain("indicação física"));
    }

    #endregion

    #region MARK: Destaque piscante no lugar da verificacao fisica

    [Test]
    public void OperadorEmCampo_VeOModuloEletronicoNasVerificacoesDeConexao()
    {
        Assert.That(EtapaSaidas.Primeiro.Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueModuloEletronico));
        Assert.That(EtapaSaidas.Em(7).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueModuloEletronico));
    }

    [Test]
    public void QuadrosSemVerificacaoFisica_NaoAcendemDestaqueNoModelo()
    {
        foreach (int indice in new[] { 1, 2, 3, 4, 5, 6, 8, 9 })
        {
            Assert.That(EtapaSaidas.Em(indice).Vfx, Is.Null,
                $"o quadro {indice} não é de verificação física e não deve destacar peça.");
        }
    }

    [Test]
    public void OA25_NaoDestacaOBlocoPneumatico()
    {
        Assert.That(
            EtapaSaidas.Quadros.Select(quadro => quadro.Vfx),
            Has.None.EqualTo(PerfisDeDisplayDeAlerta.DestaquePneumatica),
            "o A25 é um defeito das saídas eletrônicas, não da parte pneumática.");
    }

    #endregion

    #region MARK: Leds, animacao e composicao

    [Test]
    public void OUltimoQuadro_ConfirmaAResolucaoComOsLedsDesligados()
    {
        QuadroDeDisplayM4 ultimo = EtapaSaidas.Ultimo;

        Assert.That(ultimo.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(ultimo.LedPiscando, Is.False);
        Assert.That(ultimo.Instrucao, Does.StartWith("Verifique a confirmação"));
        Assert.That(ultimo.Instrucao, Does.Contain("A25 é eliminado"));
    }

    [Test]
    public void EnquantoOAlertaEstaAtivo_OLedVermelhoPisca()
    {
        var ativos = EtapaSaidas.Quadros.Where(quadro => quadro != EtapaSaidas.Ultimo).ToList();

        Assert.That(ativos.All(quadro => quadro.Leds == EstadoLedsM4.Alerta), Is.True);
        Assert.That(ativos.All(quadro => quadro.LedPiscando), Is.True);
    }

    [Test]
    public void AnimacoesDeA25_FicamVaziasPorqueNenhumPassoUsaOsBotoes()
    {
        Assert.That(
            EtapaSaidas.Quadros.Select(quadro => quadro.Animacao),
            Is.EqualTo(new string[] { null, null, null, null, null, null, null, null, null, null }));

        Assert.That(perfil.Layer, Is.Null, "o A25 roda na Base Layer.");
    }

    [Test]
    public void EtapasDeA25_ExpandemAAcaoOficialEmDezPassosGuiados()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a25, EtapasGuiadasDeAlerta.Criar(a25));

        Assert.That(etapas, Has.Length.EqualTo(10));
        Assert.That(etapas[0].tutorial, Does.StartWith("Confirme o alerta A25"));
        Assert.That(etapas[0].vfx, Is.EqualTo("DestaqueModuloEletronico"));
        Assert.That(etapas.All(TelaM4.EtapaTemConteudo), Is.True);
    }

    #endregion
}
