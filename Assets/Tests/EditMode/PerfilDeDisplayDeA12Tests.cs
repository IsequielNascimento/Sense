using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class PerfilDeDisplayDeA12Tests
{
    #region MARK: Fixture

    private const string CodigoA12 = "A12";
    private const string AcaoResetarContador = "Resetar o contador";
    private const string AcaoDesligarAlerta = "Desligar o alerta";
    private const int QuantidadeDeQuadrosResetarContador = 7;
    private const int QuantidadeDeQuadrosDesligarAlerta = 7;
    private const string ModelPath = "Assets/Prefab/Teste/M4SMARTTeste.fbx";
    private const string PrefabPath = "Assets/Resources/M4Problem12/M4SMARTTesteProblema12.prefab";

    private AlertaOficial a12;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a12 = CatalogoDeAlertas.Obter(CodigoA12, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA12);
    }

    private SequenciaDeQuadrosM4 EtapaResetarContador => perfil.EtapaOficial(0);
    private SequenciaDeQuadrosM4 EtapaDesligarAlerta => perfil.EtapaOficial(1);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA12_MantemAsDuasAcoesDaPagina76()
    {
        Assert.That(a12, Is.Not.Null);
        Assert.That(a12.Nome, Is.EqualTo("CONTADOR TOTAL"));
        Assert.That(a12.Padrao, Is.EqualTo("desabilitado"));
        Assert.That(a12.Acoes.Count, Is.EqualTo(2));
        Assert.That(a12.Acoes[0], Is.EqualTo(AcaoResetarContador));
        Assert.That(a12.Acoes[1], Is.EqualTo(AcaoDesligarAlerta));
        Assert.That(a12.Locais.Count, Is.EqualTo(2));
    }

    [Test]
    public void PerfilDeA12_TemExatamenteDuasEtapasOficiais()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA12));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(2));
        Assert.That(perfil.CorrespondeAoCatalogo(a12), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A12NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A24"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA12));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A8"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A11"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A1"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A2"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A14"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(13));
    }

    #endregion

    #region MARK: Quadros do LCD

    [Test]
    public void EtapaResetarContador_TemSeteQuadrosSemTextoVazio()
    {
        Assert.That(EtapaResetarContador.Quantidade, Is.EqualTo(QuantidadeDeQuadrosResetarContador));

        foreach (QuadroDeDisplayM4 quadro in EtapaResetarContador.Quadros)
        {
            Assert.That(quadro.TextoLcd, Is.Not.Empty);
            Assert.That(quadro.TextoLcd, Is.EqualTo(quadro.TextoLcd.Trim()));
            Assert.That(quadro.Instrucao, Is.Not.Null.And.Not.Empty);
            Assert.That(quadro.Instrucao, Is.EqualTo(quadro.Instrucao.Trim()));
        }
    }

    [Test]
    public void EtapaDesligarAlerta_TemSeteQuadrosSemTextoVazio()
    {
        Assert.That(EtapaDesligarAlerta.Quantidade, Is.EqualTo(QuantidadeDeQuadrosDesligarAlerta));

        foreach (QuadroDeDisplayM4 quadro in EtapaDesligarAlerta.Quadros)
        {
            Assert.That(quadro.TextoLcd, Is.Not.Empty);
            Assert.That(quadro.TextoLcd, Is.EqualTo(quadro.TextoLcd.Trim()));
            Assert.That(quadro.Instrucao, Is.Not.Null.And.Not.Empty);
            Assert.That(quadro.Instrucao, Is.EqualTo(quadro.Instrucao.Trim()));
        }
    }

    [Test]
    public void EntradaNoMenu_UsaB2PorSeisSegundosEmAmbasAsEtapas()
    {
        QuadroDeDisplayM4 entradaResetarContador = EtapaResetarContador.Em(1);
        QuadroDeDisplayM4 entradaDesligarAlerta = EtapaDesligarAlerta.Em(1);

        Assert.That(entradaResetarContador.TextoLcd, Is.EqualTo("MENU"));
        Assert.That(entradaResetarContador.Instrucao, Does.Contain("B2"));
        Assert.That(entradaResetarContador.Instrucao, Does.Contain("6 segundos"));
        Assert.That(entradaResetarContador.ProgressoSegundos, Is.EqualTo(6f));

        Assert.That(entradaDesligarAlerta.TextoLcd, Is.EqualTo("MENU"));
        Assert.That(entradaDesligarAlerta.Instrucao, Does.Contain("B2"));
        Assert.That(entradaDesligarAlerta.Instrucao, Does.Contain("6 segundos"));
        Assert.That(entradaDesligarAlerta.ProgressoSegundos, Is.EqualTo(6f));
    }

    #endregion

    #region MARK: Vida util, aviso IMPORTANTE e reset pelo C19

    [Test]
    public void EstadoInicial_InformaQueOContadorTotalMedeAVidaUtil()
    {
        QuadroDeDisplayM4 estadoInicial = EtapaResetarContador.Primeiro;

        Assert.That(estadoInicial.TextoLcd, Is.EqualTo(CodigoA12));
        Assert.That(estadoInicial.Instrucao, Does.Contain("vida útil"));
    }

    [Test]
    public void AvisoImportante_AparecePeloC19AntesDaConfirmacaoDoReset()
    {
        QuadroDeDisplayM4 navegacaoC19 = EtapaResetarContador.Em(3);
        QuadroDeDisplayM4 aviso = EtapaResetarContador.Em(4);
        QuadroDeDisplayM4 confirmacaoSim = EtapaResetarContador.Em(5);

        Assert.That(navegacaoC19.TextoLcd, Is.EqualTo("C19\nRESET"));
        Assert.That(navegacaoC19.Instrucao, Does.Contain("C19"));
        Assert.That(aviso.TextoLcd, Is.EqualTo("C19\nRESET"));
        Assert.That(aviso.Instrucao, Does.StartWith("IMPORTANTE"));
        Assert.That(aviso.Instrucao, Does.Contain("vida útil"));
        Assert.That(aviso.Instrucao, Does.Contain("válvula diferente"));
        Assert.That(aviso.Instrucao, Does.Contain("não é uma rotina comum"));
        Assert.That(confirmacaoSim.TextoLcd, Is.EqualTo("SIM"));
        Assert.That(confirmacaoSim.Instrucao, Does.Contain("B2"));
    }

    [Test]
    public void ConfirmacaoDoReset_ZeraOTotalSemAlterarDadosNaoRelacionados()
    {
        QuadroDeDisplayM4 confirmacaoReset = EtapaResetarContador.Ultimo;

        Assert.That(confirmacaoReset.TextoLcd, Is.EqualTo("C19\nRESET"));
        Assert.That(confirmacaoReset.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(confirmacaoReset.Instrucao, Does.Contain("voltou a zero"));
        Assert.That(confirmacaoReset.Instrucao, Does.Contain("contador parcial e as demais configurações não são alterados"));
    }

    #endregion

    #region MARK: Separacao entre reset e desativacao

    [Test]
    public void Desativacao_UsaOMenuAlertaSemResetarOContador()
    {
        QuadroDeDisplayM4 menuAlerta = EtapaDesligarAlerta.Em(3);
        QuadroDeDisplayM4 opcaoA12 = EtapaDesligarAlerta.Em(4);
        QuadroDeDisplayM4 desabilitar = EtapaDesligarAlerta.Em(5);
        QuadroDeDisplayM4 confirmacaoDesligar = EtapaDesligarAlerta.Ultimo;

        Assert.That(menuAlerta.TextoLcd, Is.EqualTo("MENU\nALERTA"));
        Assert.That(opcaoA12.TextoLcd, Is.EqualTo("A12\nCONTAD"));
        Assert.That(desabilitar.TextoLcd, Is.EqualTo("DESABI"));
        Assert.That(confirmacaoDesligar.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(confirmacaoDesligar.Instrucao, Does.Contain("desligado"));
        Assert.That(confirmacaoDesligar.Instrucao, Does.Contain("não reseta"));
    }

    [Test]
    public void ConfirmacoesDeResetEDesativacao_SaoVisualmenteDistintas()
    {
        QuadroDeDisplayM4 confirmacaoReset = EtapaResetarContador.Ultimo;
        QuadroDeDisplayM4 confirmacaoDesligar = EtapaDesligarAlerta.Ultimo;

        Assert.That(confirmacaoReset.TextoLcd, Is.Not.EqualTo(confirmacaoDesligar.TextoLcd));
        Assert.That(confirmacaoReset.Instrucao, Is.Not.EqualTo(confirmacaoDesligar.Instrucao));
    }

    #endregion

    #region MARK: LED e mecanismo sem evidencia normativa

    [Test]
    public void PerfilDeA12_NaoInventaLedVermelhoSemEvidenciaNormativa()
    {
        Assert.That(EtapaResetarContador.Quadros.All(quadro => !quadro.LedPiscando), Is.True);
        Assert.That(EtapaResetarContador.Quadros.All(quadro => quadro.Leds == EstadoLedsM4.Desligado), Is.True);
        Assert.That(EtapaDesligarAlerta.Quadros.All(quadro => !quadro.LedPiscando), Is.True);
        Assert.That(EtapaDesligarAlerta.Quadros.All(quadro => quadro.Leds == EstadoLedsM4.Desligado), Is.True);
    }

    [Test]
    public void PerfilDeA12_NaoAfirmaMecanismoDeAtivacaoConfirmado()
    {
        Assert.That(perfil.MecanismoDeAtivacaoConfirmado, Is.False);
    }

    #endregion

    #region MARK: Navegacao determinista

    [Test]
    public void Navegacao_ComecaNoPrimeiroQuadroDaPrimeiraEtapa()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);

        Assert.That(navegacao.IndiceDaEtapaOficial, Is.EqualTo(0));
        Assert.That(navegacao.IndiceDoQuadro, Is.EqualTo(0));
        Assert.That(navegacao.QuadroAtual.TextoLcd, Is.EqualTo(CodigoA12));
    }

    [Test]
    public void ProximoQuadro_PercorreOsQuadrosSemSairDaEtapaOficial()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);

        for (int i = 1; i < EtapaResetarContador.Quantidade; i++)
        {
            Assert.That(navegacao.ProximoQuadro(), Is.True);
            Assert.That(navegacao.IndiceDoQuadro, Is.EqualTo(i));
            Assert.That(navegacao.IndiceDaEtapaOficial, Is.EqualTo(0));
        }

        Assert.That(navegacao.ProximoQuadro(), Is.False);
        Assert.That(navegacao.EstaNoUltimoQuadro, Is.True);
    }

    [Test]
    public void Repetir_ReiniciaNoPrimeiroQuadroDaEtapaAtual()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);
        navegacao.ProximoQuadro();
        navegacao.ProximoQuadro();

        navegacao.Repetir();

        Assert.That(navegacao.IndiceDoQuadro, Is.EqualTo(0));
        Assert.That(navegacao.QuadroAtual.TextoLcd, Is.EqualTo(CodigoA12));
    }

    [Test]
    public void Avancar_MoveParaASegundaEtapaOficialUmaUnicaVez()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);

        Assert.That(navegacao.Avancar(), Is.True);
        Assert.That(navegacao.IndiceDaEtapaOficial, Is.EqualTo(1));
        Assert.That(navegacao.IndiceDoQuadro, Is.EqualTo(0));
        Assert.That(navegacao.Avancar(), Is.False);
    }

    [Test]
    public void Voltar_RetornaParaAPrimeiraEtapaOficialUmaUnicaVez()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);
        navegacao.Avancar();

        Assert.That(navegacao.Voltar(), Is.True);
        Assert.That(navegacao.IndiceDaEtapaOficial, Is.EqualTo(0));
        Assert.That(navegacao.Voltar(), Is.False);
    }

    [Test]
    public void Reiniciar_VoltaAoEstadoInicial()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);
        navegacao.Avancar();
        navegacao.ProximoQuadro();

        navegacao.Reiniciar();

        Assert.That(navegacao.IndiceDaEtapaOficial, Is.EqualTo(0));
        Assert.That(navegacao.IndiceDoQuadro, Is.EqualTo(0));
    }

    #endregion

    #region MARK: Limpeza do estado visual anterior

    [Test]
    public void AplicarQuadro_LimpaTodoOEstadoVisualResidual()
    {
        var etapa = new Etapa
        {
            tutorial = AcaoResetarContador,
            animacao = string.Empty,
            textoDisplay = "residual",
            alerta = "alerta residual",
            leds = QuadroDeDisplayM4.LedAberto,
            textoAngulo = "90",
            alertaTempoExcedido = "estourou",
            progressoSegundos = 5f,
            progressoEstoura = true,
        };

        EtapaResetarContador.Primeiro.Aplicar(etapa);

        Assert.That(etapa.textoDisplay, Is.EqualTo(CodigoA12));
        Assert.That(etapa.leds, Is.EqualTo(QuadroDeDisplayM4.LedApagado));
        Assert.That(etapa.alerta, Is.Empty);
        Assert.That(etapa.textoAngulo, Is.Empty);
        Assert.That(etapa.alertaTempoExcedido, Is.Empty);
        Assert.That(etapa.progressoSegundos, Is.EqualTo(0f));
        Assert.That(etapa.progressoEstoura, Is.False);
        Assert.That(etapa.animacao, Is.Empty);
    }

    #endregion

    #region MARK: Animacao do botao citado em cada passo

    [Test]
    public void AnimacoesDeA12_SeguemOsBotoesCitadosEmCadaPasso()
    {
        Assert.That(
            EtapaResetarContador.Quadros.Concat(EtapaDesligarAlerta.Quadros).Select(quadro => quadro.Animacao),
            Is.EqualTo(new[]
            {
                null,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B123,
                null,
                AnimacaoDeBotaoM4.B123,
                null,
                null,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B3,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B23,
                AnimacaoDeBotaoM4.B123,
                null,
            }));
    }

    #endregion

    #region MARK: Composicao sobre as etapas guiadas

    [Test]
    public void EtapasDeA12_ExpandemAsDuasAcoesOficiaisEmPassosPraticos()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a12, EtapasGuiadasDeAlerta.Criar(a12));

        Assert.That(a12.Acoes, Has.Count.EqualTo(2));
        Assert.That(etapas, Has.Length.EqualTo(QuantidadeDeQuadrosResetarContador + QuantidadeDeQuadrosDesligarAlerta));
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.tutorial)), Is.True);
        Assert.That(etapas.Select(etapa => etapa.animacao), Is.EqualTo(
            EtapaResetarContador.Quadros.Concat(EtapaDesligarAlerta.Quadros).Select(quadro => quadro.Animacao ?? string.Empty)));
        Assert.That(etapas.All(TelaM4.EtapaTemConteudo), Is.True);
        Assert.That(etapas[1].progressoSegundos, Is.EqualTo(6f));
        Assert.That(etapas[QuantidadeDeQuadrosResetarContador + 1].progressoSegundos, Is.EqualTo(6f));
    }

    #endregion

    #region MARK: Modelo visual obrigatorio

    private static readonly Type TipoModeloDeAlertaDisplay =
        Type.GetType("ModeloDeAlertaDisplay, Assembly-CSharp");

    private static bool UsaM4SmartTeste(string codigo)
    {
        return (bool)TipoModeloDeAlertaDisplay
            .GetMethod("UsaM4SmartTeste")
            .Invoke(null, new object[] { codigo });
    }

    private static GameObject ResolverModelo(string codigo)
    {
        return TipoModeloDeAlertaDisplay
            .GetMethod("Resolver")
            .Invoke(null, new object[] { codigo }) as GameObject;
    }

    [Test]
    public void ModeloDeA12_ResolveOPrefabBaseadoEmM4SmartTeste()
    {
        Assert.That(TipoModeloDeAlertaDisplay, Is.Not.Null);
        Assert.That(UsaM4SmartTeste(CodigoA12), Is.True);
        Assert.That(UsaM4SmartTeste("A8"), Is.True);
        Assert.That(ResolverModelo("A24"), Is.Null);

        GameObject prefab = ResolverModelo(CodigoA12);

        Assert.That(prefab, Is.Not.Null);
        Assert.That(AssetDatabase.GetAssetPath(prefab), Is.EqualTo(PrefabPath));
        Assert.That(AssetDatabase.GetDependencies(PrefabPath), Does.Contain(ModelPath));
        Assert.That(AssetDatabase.GetDependencies(PrefabPath),
            Has.None.Contains("M4_Smart_Final"));
        Assert.That(prefab.GetComponentsInChildren<Transform>(true)
            .Any(item => item.name == "DisplayDynamic"), Is.True);
        Assert.That(prefab.GetComponentsInChildren<MonoBehaviour>(true)
            .Any(component => component.GetType().Name == "GerenciadorVisual"), Is.True);
        Assert.That(prefab.GetComponentInChildren<ControladorLedsM4>(true), Is.Not.Null);
    }

    #endregion
}
