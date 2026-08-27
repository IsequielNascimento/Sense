using System.Linq;
using NUnit.Framework;

public class PerfilDeDisplayDeA19Tests
{
    #region MARK: Fixture

    private const string CodigoA19 = "A19";
    private const string AcaoVerificarFonte = "Verificar fonte de alimentação";
    private const int QuantidadeDeQuadros = 16;
    private const int LimiteDeCaracteresDaInstrucao = 120;

    private AlertaOficial a19;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a19 = CatalogoDeAlertas.Obter(CodigoA19, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA19);
    }

    private SequenciaDeQuadrosM4 EtapaUnica => perfil.EtapaOficial(0);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA19_MantemAAcaoUnicaDaPagina76()
    {
        Assert.That(a19, Is.Not.Null);
        Assert.That(a19.Nome, Is.EqualTo("ALIMENTAÇÃO ALTA"));
        Assert.That(a19.Padrao, Is.EqualTo("habilitado"));
        Assert.That(a19.Acoes.Count, Is.EqualTo(1));
        Assert.That(a19.Acoes[0], Is.EqualTo(AcaoVerificarFonte));
        Assert.That(a19.Locais.Count, Is.EqualTo(1));
        Assert.That(a19.Locais[0], Is.EqualTo("painel de controle"));
    }

    [Test]
    public void PerfilDeA19_TemExatamenteUmaEtapaOficial()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA19));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(1));
        Assert.That(perfil.CorrespondeAoCatalogo(a19), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A19NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A10"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA19));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A20"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(24));
    }

    #endregion

    #region MARK: Quadros do LCD

    [Test]
    public void EtapaUnica_TemDezesseisQuadrosSemTextoVazio()
    {
        Assert.That(EtapaUnica.Quantidade, Is.EqualTo(QuantidadeDeQuadros));

        foreach (QuadroDeDisplayM4 quadro in EtapaUnica.Quadros)
        {
            Assert.That(quadro.TextoLcd, Is.Not.Empty);
            Assert.That(quadro.TextoLcd, Is.EqualTo(quadro.TextoLcd.Trim()));
            Assert.That(quadro.Instrucao, Is.Not.Null.And.Not.Empty);
            Assert.That(quadro.Instrucao, Is.EqualTo(quadro.Instrucao.Trim()));
        }
    }

    [Test]
    public void Instrucoes_CabemNaCaixaDeTextoDoPassoAPasso()
    {
        foreach (QuadroDeDisplayM4 quadro in EtapaUnica.Quadros)
        {
            Assert.That(
                quadro.Instrucao.Length,
                Is.LessThanOrEqualTo(LimiteDeCaracteresDaInstrucao),
                $"Instrução longa demais no quadro '{quadro.TextoLcd}'.");
        }
    }

    [Test]
    public void NenhumQuadro_RepeteOTextoResumidoDaTabelaDeResolucao()
    {
        Assert.That(
            EtapaUnica.Quadros.Select(quadro => quadro.Instrucao),
            Has.None.EqualTo(AcaoVerificarFonte));
    }

    #endregion

    #region MARK: Limites de tensao por versao do modulo

    [Test]
    public void EstadoInicial_TrazOsLimitesSuperioresDoManual()
    {
        QuadroDeDisplayM4 estadoInicial = EtapaUnica.Primeiro;

        Assert.That(estadoInicial.TextoLcd, Is.EqualTo(CodigoA19));
        Assert.That(estadoInicial.Instrucao, Does.StartWith("Confirme o alerta A19"));
        Assert.That(estadoInicial.Instrucao, Does.Contain("32Vcc"));
        Assert.That(estadoInicial.Instrucao, Does.Contain("AS-Interface"));
        Assert.That(estadoInicial.Instrucao, Does.Contain("10%"));
    }

    [Test]
    public void DiagnosticoDeA19_NaoUsaOsLimitesInferioresDoA20()
    {
        foreach (QuadroDeDisplayM4 quadro in EtapaUnica.Quadros)
        {
            Assert.That(quadro.Instrucao, Does.Not.Contain("27V"),
                $"o quadro '{quadro.TextoLcd}' usa o limite do A20.");
            Assert.That(quadro.Instrucao, Does.Not.Contain("22,8V"),
                $"o quadro '{quadro.TextoLcd}' usa o limite do A20.");
        }
    }

    [Test]
    public void FaixaAceita_DependeDaVersaoEOMenuDeviceIdentificaOModulo()
    {
        Assert.That(EtapaUnica.Em(1).Instrucao, Does.Contain("versão"));
        Assert.That(EtapaUnica.Em(3).Instrucao, Does.Contain("MENU DEVICE"));
        Assert.That(EtapaUnica.Em(4).TextoLcd, Does.Contain("DEVICE"));
        Assert.That(EtapaUnica.Em(5).TextoLcd, Is.EqualTo("d1"));
        Assert.That(EtapaUnica.Em(6).Instrucao, Does.Contain("part number").IgnoreCase);
    }

    [Test]
    public void MenuDevice_EApresentadoComoApenasDeConsulta()
    {
        Assert.That(EtapaUnica.Em(4).Instrucao, Does.Contain("não altera"));
    }

    #endregion

    #region MARK: Seguranca antes de manusear a fiacao

    [Test]
    public void AntesDeAbrirOCompartimento_AvisaSobreFonteEEnergiaResidual()
    {
        Assert.That(EtapaUnica.Em(8).Instrucao, Does.StartWith("PERIGO"));
        Assert.That(EtapaUnica.Em(8).Instrucao, Does.Contain("religamento"));
        Assert.That(EtapaUnica.Em(9).Instrucao, Does.Contain("energia residual"));
    }

    [Test]
    public void OndeAAlimentacaoChega_AcendeOModuloEletronico()
    {
        Assert.That(EtapaUnica.Em(10).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueModuloEletronico));
        Assert.That(EtapaUnica.Em(11).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueModuloEletronico));
    }

    [Test]
    public void QuadrosDeMenuEDePainel_NaoAcendemDestaqueNoModelo()
    {
        foreach (int indice in new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 12, 13, 14, 15 })
        {
            Assert.That(EtapaUnica.Em(indice).Vfx, Is.Null,
                $"o quadro {indice} não deve destacar peça.");
        }
    }

    #endregion

    #region MARK: Verificacao fora do monitor e nota normativa

    [Test]
    public void ConferenciaPrincipal_EstaNoPainelDeControle()
    {
        Assert.That(EtapaUnica.Em(12).Instrucao, Does.Contain("painel de controle"));
        Assert.That(EtapaUnica.Em(12).Instrucao, Does.Contain("fora do monitor"));
    }

    [Test]
    public void NotaNormativa_ExplicaQueDesabilitarSoApagaANotificacao()
    {
        Assert.That(EtapaUnica.Em(14).Instrucao, Does.Contain("aplicativo"));
        Assert.That(EtapaUnica.Em(14).Instrucao, Does.Contain("A25"));
    }

    [Test]
    public void QuadroFinal_ConfirmaComOsLedsApagados()
    {
        QuadroDeDisplayM4 confirmacao = EtapaUnica.Ultimo;

        Assert.That(confirmacao.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(confirmacao.LedPiscando, Is.False);
        Assert.That(confirmacao.Instrucao, Does.StartWith("Verifique a confirmação"));
    }

    #endregion

    #region MARK: Animacao do botao citado em cada passo

    [Test]
    public void AnimacoesDeA19_SeguemOsBotoesCitadosEmCadaPasso()
    {
        Assert.That(
            EtapaUnica.Quadros.Select(quadro => quadro.Animacao),
            Is.EqualTo(new[]
            {
                null,
                null,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B3,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B23,
                null,
                AnimacaoDeBotaoM4.B1,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
            }));
    }

    #endregion

    #region MARK: Composicao sobre as etapas guiadas

    [Test]
    public void EtapasDeA19_ExpandemAAcaoOficialEmPassosPraticos()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a19, EtapasGuiadasDeAlerta.Criar(a19));

        Assert.That(etapas, Has.Length.EqualTo(QuantidadeDeQuadros));
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.tutorial)), Is.True);
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.textoDisplay)), Is.True);
    }

    #endregion
}
