using System.Linq;
using NUnit.Framework;

public class PerfilDeDisplayDeA4Tests
{
    #region MARK: Fixture

    private const string CodigoA4 = "A4";
    private const string AcaoVerificarArComprimido = "Verificar fornecimento de ar comprimido";
    private const int QuantidadeDeQuadros = 12;

    private AlertaOficial a4;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a4 = CatalogoDeAlertas.Obter(CodigoA4, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA4);
    }

    private SequenciaDeQuadrosM4 EtapaUnica => perfil.EtapaOficial(0);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA4_MantemAAcaoUnicaDaPagina76()
    {
        Assert.That(a4, Is.Not.Null);
        Assert.That(a4.Nome, Is.EqualTo("FORA DE FAIXA"));
        Assert.That(a4.Padrao, Is.EqualTo("sempre ligado"));
        Assert.That(a4.Acoes.Count, Is.EqualTo(1));
        Assert.That(a4.Acoes[0], Is.EqualTo(AcaoVerificarArComprimido));
        Assert.That(a4.Locais.Count, Is.EqualTo(1));
        Assert.That(a4.Locais[0], Is.EqualTo("gerador de ar comprimido"));
    }

    [Test]
    public void PerfilDeA4_TemExatamenteUmaEtapaOficial()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA4));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(1));
        Assert.That(perfil.CorrespondeAoCatalogo(a4), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A4NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A6"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA4));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A3"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(13));
    }

    #endregion

    #region MARK: Passo a passo auto explicativo, no padrao do A3

    [Test]
    public void EtapaUnica_TemDozeQuadrosSemTextoVazio()
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
    public void NenhumQuadro_RepeteOTextoResumidoDaTabelaDeResolucao()
    {
        var instrucoes = EtapaUnica.Quadros.Select(quadro => quadro.Instrucao).ToList();

        Assert.That(instrucoes, Has.None.EqualTo(AcaoVerificarArComprimido));
        Assert.That(instrucoes.All(instrucao => instrucao.Length >= 100), Is.True,
            "todo passo do A4 precisa explicar o que fazer, e não só nomear a ação.");
    }

    [Test]
    public void PrimeiroQuadro_ExplicaOsCincoGrausEAPressaoDaCalibracao()
    {
        QuadroDeDisplayM4 estadoInicial = EtapaUnica.Primeiro;

        Assert.That(estadoInicial.TextoLcd, Is.EqualTo(CodigoA4));
        Assert.That(estadoInicial.Instrucao, Does.StartWith("Confirme o alerta A4"));
        Assert.That(estadoInicial.Instrucao, Does.Contain("5°"));
        Assert.That(estadoInicial.Instrucao, Does.Contain("calibração"));
    }

    [Test]
    public void QuadrosDePressao_TrazemAFaixaDeOperacaoEOLimiteDestrutivo()
    {
        string faixa = EtapaUnica.Em(2).Instrucao;
        string limite = EtapaUnica.Em(3).Instrucao;

        Assert.That(faixa, Does.Contain("3 e 8 bar"));
        Assert.That(faixa, Does.Contain("6 bar"));
        Assert.That(limite, Does.Contain("10 bar"));
        Assert.That(limite, Does.Contain("danificado permanentemente"));
    }

    #endregion

    #region MARK: Destaque piscante no lugar da verificacao

    [Test]
    public void OperadorEmCampo_VeOndeOlharAntesDeQualquerMedicao()
    {
        Assert.That(EtapaUnica.Em(1).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaquePneumatica));
        Assert.That(EtapaUnica.Em(2).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueMangueiras));
        Assert.That(EtapaUnica.Em(4).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaquePneumatica));
    }

    [Test]
    public void QuadrosDeMenu_NaoAcendemDestaqueNoModelo()
    {
        foreach (int indice in new[] { 5, 6, 7, 8, 9, 10, 11 })
        {
            Assert.That(EtapaUnica.Em(indice).Vfx, Is.Null,
                $"o quadro {indice} é de menu e não deve destacar peça.");
        }
    }

    #endregion

    #region MARK: As duas saidas do alerta

    [Test]
    public void PrimeiraSaida_ResolveNoGeradorSemMexerNoMonitor()
    {
        QuadroDeDisplayM4 regulagem = EtapaUnica.Em(4);

        Assert.That(regulagem.Instrucao, Does.StartWith("Primeira saída"));
        Assert.That(regulagem.Instrucao, Does.Contain("gerador"));
        Assert.That(regulagem.Instrucao, Does.Contain("sem mexer no monitor"));
    }

    [Test]
    public void SegundaSaida_AjustaC17ERefazACalibracao()
    {
        Assert.That(EtapaUnica.Em(5).TextoLcd, Is.EqualTo("MENU"));
        Assert.That(EtapaUnica.Em(5).Instrucao, Does.StartWith("Segunda saída"));
        Assert.That(EtapaUnica.Em(5).ProgressoSegundos, Is.EqualTo(6f));

        Assert.That(EtapaUnica.Em(6).TextoLcd, Is.EqualTo("MENU\nCONFIG"));
        Assert.That(EtapaUnica.Em(7).TextoLcd, Is.EqualTo("C17"));
        Assert.That(EtapaUnica.Em(8).TextoLcd, Is.EqualTo("C17"));

        Assert.That(EtapaUnica.Em(9).TextoLcd, Is.EqualTo("FAST\nSETUP"));
        Assert.That(EtapaUnica.Em(9).ProgressoSegundos, Is.EqualTo(6f));
        Assert.That(EtapaUnica.Em(10).TextoLcd, Is.EqualTo("CERTO"));
    }

    [Test]
    public void QuadroFinal_ConfirmaComOsLedsApagados()
    {
        QuadroDeDisplayM4 confirmacao = EtapaUnica.Ultimo;

        Assert.That(confirmacao.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(confirmacao.LedPiscando, Is.False);
        Assert.That(confirmacao.Instrucao, Does.StartWith("Verifique a confirmação"));
        Assert.That(confirmacao.Instrucao, Does.Contain("A4 é eliminado"));
    }

    #endregion

    #region MARK: Animacao do botao citado em cada passo

    [Test]
    public void AnimacoesDeA4_SeguemOsBotoesCitadosEmCadaPasso()
    {
        Assert.That(
            EtapaUnica.Quadros.Select(quadro => quadro.Animacao),
            Is.EqualTo(new[]
            {
                null,
                null,
                null,
                null,
                null,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B123,
                AnimacaoDeBotaoM4.B123,
                AnimacaoDeBotaoM4.B3,
                AnimacaoDeBotaoM4.B2,
                null,
            }));
    }

    #endregion

    #region MARK: Composicao sobre as etapas guiadas

    [Test]
    public void EtapasDeA4_ExpandemAAcaoOficialEmPassosPraticos()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a4, EtapasGuiadasDeAlerta.Criar(a4));

        Assert.That(etapas, Has.Length.EqualTo(QuantidadeDeQuadros));
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.tutorial)), Is.True);
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.textoDisplay)), Is.True);
    }

    #endregion
}
