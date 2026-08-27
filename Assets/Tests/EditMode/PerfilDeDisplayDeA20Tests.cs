using System.Linq;
using NUnit.Framework;

public class PerfilDeDisplayDeA20Tests
{
    #region MARK: Fixture

    private const string CodigoA20 = "A20";
    private const string CodigoA19 = "A19";
    private const string AcaoVerificarFonte = "Verificar fonte de alimentação";
    private const int QuantidadeDeQuadros = 16;
    private const int LimiteDeCaracteresDaInstrucao = 120;

    private AlertaOficial a20;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a20 = CatalogoDeAlertas.Obter(CodigoA20, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA20);
    }

    private SequenciaDeQuadrosM4 EtapaUnica => perfil.EtapaOficial(0);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA20_MantemAAcaoUnicaDaPagina76()
    {
        Assert.That(a20, Is.Not.Null);
        Assert.That(a20.Nome, Is.EqualTo("ALIMENTAÇÃO BAIXA"));
        Assert.That(a20.Padrao, Is.EqualTo("habilitado"));
        Assert.That(a20.Acoes.Count, Is.EqualTo(1));
        Assert.That(a20.Acoes[0], Is.EqualTo(AcaoVerificarFonte));
        Assert.That(a20.Locais.Count, Is.EqualTo(1));
        Assert.That(a20.Locais[0], Is.EqualTo("painel de controle"));
    }

    [Test]
    public void PerfilDeA20_TemExatamenteUmaEtapaOficial()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA20));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(1));
        Assert.That(perfil.CorrespondeAoCatalogo(a20), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A20NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A10"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA20));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA19));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(24));
    }

    #endregion

    #region MARK: Reuso do motor de alimentacao do A19

    [Test]
    public void A20ReutilizaAFundacaoDeA19SemDuplicarOMotor()
    {
        SequenciaDeQuadrosM4 etapaDeA19 = PerfisDeDisplayDeAlerta.Obter(CodigoA19).EtapaOficial(0);

        Assert.That(EtapaUnica.Quantidade, Is.EqualTo(etapaDeA19.Quantidade));

        for (int i = 1; i < EtapaUnica.Quantidade; i++)
        {
            Assert.That(EtapaUnica.Em(i).Instrucao, Is.EqualTo(etapaDeA19.Em(i).Instrucao), $"quadro {i}");
            Assert.That(EtapaUnica.Em(i).Vfx, Is.EqualTo(etapaDeA19.Em(i).Vfx), $"quadro {i}");
        }
    }

    [Test]
    public void CadaAlerta_MostraOProprioCodigoNoDisplayMesmoCompartilhandoOMotor()
    {
        SequenciaDeQuadrosM4 etapaDeA19 = PerfisDeDisplayDeAlerta.Obter(CodigoA19).EtapaOficial(0);

        for (int i = 0; i < EtapaUnica.Quantidade; i++)
        {
            string lcdDeA20 = EtapaUnica.Em(i).TextoLcd;
            string lcdDeA19 = etapaDeA19.Em(i).TextoLcd;

            if (lcdDeA19 == CodigoA19)
            {
                Assert.That(lcdDeA20, Is.EqualTo(CodigoA20), $"o quadro {i} deve mostrar o código do próprio alerta.");
                continue;
            }

            Assert.That(lcdDeA20, Is.EqualTo(lcdDeA19), $"o quadro {i} é de menu e deve ser igual nos dois.");
        }
    }

    [Test]
    public void DiagnosticoInicial_DistingueA20DeA19()
    {
        QuadroDeDisplayM4 inicialDeA20 = EtapaUnica.Primeiro;
        QuadroDeDisplayM4 inicialDeA19 = PerfisDeDisplayDeAlerta.Obter(CodigoA19).EtapaOficial(0).Primeiro;

        Assert.That(inicialDeA20.TextoLcd, Is.EqualTo(CodigoA20));
        Assert.That(inicialDeA19.TextoLcd, Is.EqualTo(CodigoA19));
        Assert.That(inicialDeA20.Instrucao, Is.Not.EqualTo(inicialDeA19.Instrucao));
        Assert.That(inicialDeA20.Instrucao, Does.Contain("abaixo"));
    }

    #endregion

    #region MARK: Limites de tensao por versao do modulo

    [Test]
    public void EstadoInicial_TrazOsLimitesInferioresDoManual()
    {
        QuadroDeDisplayM4 estadoInicial = EtapaUnica.Primeiro;

        Assert.That(estadoInicial.Instrucao, Does.StartWith("Confirme o alerta A20"));
        Assert.That(estadoInicial.Instrucao, Does.Contain("27V"));
        Assert.That(estadoInicial.Instrucao, Does.Contain("22,8V"));
        Assert.That(estadoInicial.Instrucao, Does.Contain("AS-Interface"));
    }

    [Test]
    public void DiagnosticoDeA20_NaoUsaOLimiteSuperiorDoA19()
    {
        foreach (QuadroDeDisplayM4 quadro in EtapaUnica.Quadros)
        {
            Assert.That(quadro.Instrucao, Does.Not.Contain("32Vcc"),
                $"o quadro '{quadro.TextoLcd}' usa o limite do A19.");
        }
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

    #region MARK: Seguranca e destaque

    [Test]
    public void AntesDeAbrirOCompartimento_AvisaSobreFonteEEnergiaResidual()
    {
        Assert.That(EtapaUnica.Em(8).Instrucao, Does.StartWith("PERIGO"));
        Assert.That(EtapaUnica.Em(9).Instrucao, Does.Contain("energia residual"));
    }

    [Test]
    public void OndeAAlimentacaoChega_AcendeOModuloEletronico()
    {
        Assert.That(EtapaUnica.Em(10).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueModuloEletronico));
        Assert.That(EtapaUnica.Em(11).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueModuloEletronico));
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
    public void AnimacoesDeA20_SeguemOsBotoesCitadosEmCadaPasso()
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
    public void EtapasDeA20_ExpandemAAcaoOficialEmPassosPraticos()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a20, EtapasGuiadasDeAlerta.Criar(a20));

        Assert.That(etapas, Has.Length.EqualTo(QuantidadeDeQuadros));
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.tutorial)), Is.True);
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.textoDisplay)), Is.True);
    }

    #endregion
}
