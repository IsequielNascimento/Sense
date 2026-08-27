using System.Linq;
using NUnit.Framework;

public class PerfilDeDisplayDeA9Tests
{
    #region MARK: Fixture

    private const string CodigoA9 = "A9";
    private const string AcaoVerificarArComprimido = "Verificar o fornecimento de ar comprimido";
    private const int QuantidadeDeQuadros = 13;

    private AlertaOficial a9;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a9 = CatalogoDeAlertas.Obter(CodigoA9, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA9);
    }

    private SequenciaDeQuadrosM4 EtapaUnica => perfil.EtapaOficial(0);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA9_MantemAAcaoUnicaDaPagina76()
    {
        Assert.That(a9, Is.Not.Null);
        Assert.That(a9.Nome, Is.EqualTo("MÁX. PRESSÃO NA LINHA"));
        Assert.That(a9.Padrao, Is.EqualTo("sempre ligado"));
        Assert.That(a9.Acoes.Count, Is.EqualTo(1));
        Assert.That(a9.Acoes[0], Is.EqualTo(AcaoVerificarArComprimido));
        Assert.That(a9.Locais.Count, Is.EqualTo(1));
        Assert.That(a9.Locais[0], Is.EqualTo("gerador de ar comprimido"));
    }

    [Test]
    public void PerfilDeA9_TemExatamenteUmaEtapaOficial()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA9));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(1));
        Assert.That(perfil.CorrespondeAoCatalogo(a9), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A9NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A6"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA9));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A4"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(13));
    }

    #endregion

    #region MARK: Passo a passo auto explicativo, no padrao do A4

    [Test]
    public void EtapaUnica_TemTrezeQuadrosSemTextoVazio()
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
            "todo passo do A9 precisa explicar o que fazer, e não só nomear a ação.");
    }

    [Test]
    public void PrimeiroQuadro_ExplicaOsNoveBarEQueOAlertaNaoSeDesliga()
    {
        QuadroDeDisplayM4 estadoInicial = EtapaUnica.Primeiro;

        Assert.That(estadoInicial.TextoLcd, Is.EqualTo(CodigoA9));
        Assert.That(estadoInicial.Instrucao, Does.StartWith("Confirme o alerta A9"));
        Assert.That(estadoInicial.Instrucao, Does.Contain("9 bar"));
        Assert.That(estadoInicial.Instrucao, Does.Contain("não se desliga pelo menu"));
    }

    #endregion

    #region MARK: O aviso normativo da pagina 5 vira passo

    [Test]
    public void AvisoDosNoveBar_ApareceComoPassoProprio()
    {
        QuadroDeDisplayM4 aviso = EtapaUnica.Em(1);

        Assert.That(aviso.Instrucao, Does.StartWith("IMPORTANTE"));
        Assert.That(aviso.Instrucao, Does.Contain("risco de danos ao equipamento"));
        Assert.That(aviso.Instrucao, Does.Contain("9 bar (130,5 psi)"));
    }

    [Test]
    public void QuadrosDePressao_TrazemOLimiteDestrutivoEAFaixaDeOperacao()
    {
        string limite = EtapaUnica.Em(2).Instrucao;
        string faixa = EtapaUnica.Em(5).Instrucao;

        Assert.That(limite, Does.Contain("10 bar"));
        Assert.That(limite, Does.Contain("danificado permanentemente"));
        Assert.That(faixa, Does.Contain("3 a 8 bar"));
        Assert.That(faixa, Does.Contain("6 bar"));
    }

    [Test]
    public void QuadroDoA17_SeparaOAvisoConfiguravelDoLimiteFixoDoA9()
    {
        QuadroDeDisplayM4 comparacao = EtapaUnica.Em(6);

        Assert.That(comparacao.Instrucao, Does.Contain("A17"));
        Assert.That(comparacao.Instrucao, Does.Contain("20%"));
        Assert.That(comparacao.Instrucao, Does.Contain("7,2 bar"));
    }

    #endregion

    #region MARK: Destaque piscante no lugar da verificacao

    [Test]
    public void OperadorEmCampo_VeOndeOlharAntesDeQualquerMedicao()
    {
        Assert.That(EtapaUnica.Em(3).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaquePneumatica));
        Assert.That(EtapaUnica.Em(4).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueMangueiras));
        Assert.That(EtapaUnica.Em(5).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaquePneumatica));
    }

    [Test]
    public void QuadrosSemVerificacaoFisica_NaoAcendemDestaqueNoModelo()
    {
        foreach (int indice in new[] { 0, 1, 2, 6, 7, 8, 9, 10, 11, 12 })
        {
            Assert.That(EtapaUnica.Em(indice).Vfx, Is.Null,
                $"o quadro {indice} não é de verificação física e não deve destacar peça.");
        }
    }

    #endregion

    #region MARK: A rota do menu ate a referencia em C17

    [Test]
    public void RotaDoMenu_ConfereAReferenciaDePressaoEmC17()
    {
        Assert.That(EtapaUnica.Em(7).TextoLcd, Is.EqualTo("MENU"));
        Assert.That(EtapaUnica.Em(7).ProgressoSegundos, Is.EqualTo(6f));

        Assert.That(EtapaUnica.Em(8).TextoLcd, Is.EqualTo("MENU\nCONFIG"));
        Assert.That(EtapaUnica.Em(9).TextoLcd, Is.EqualTo("C17"));
        Assert.That(EtapaUnica.Em(10).TextoLcd, Is.EqualTo("C17"));
        Assert.That(EtapaUnica.Em(11).TextoLcd, Is.EqualTo("SAIR"));
    }

    [Test]
    public void QuadroFinal_ConfirmaComOsLedsApagados()
    {
        QuadroDeDisplayM4 confirmacao = EtapaUnica.Ultimo;

        Assert.That(confirmacao.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(confirmacao.LedPiscando, Is.False);
        Assert.That(confirmacao.Instrucao, Does.StartWith("Verifique a confirmação"));
        Assert.That(confirmacao.Instrucao, Does.Contain("A9 é eliminado"));
    }

    #endregion

    #region MARK: Animacao do botao citado em cada passo

    [Test]
    public void AnimacoesDeA9_SeguemOsBotoesCitadosEmCadaPasso()
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
                null,
                null,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B123,
                AnimacaoDeBotaoM4.B123,
                AnimacaoDeBotaoM4.B1,
                null,
            }));
    }

    #endregion

    #region MARK: Composicao sobre as etapas guiadas

    [Test]
    public void EtapasDeA9_ExpandemAAcaoOficialEmPassosPraticos()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a9, EtapasGuiadasDeAlerta.Criar(a9));

        Assert.That(etapas, Has.Length.EqualTo(QuantidadeDeQuadros));
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.tutorial)), Is.True);
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.textoDisplay)), Is.True);
    }

    #endregion
}
