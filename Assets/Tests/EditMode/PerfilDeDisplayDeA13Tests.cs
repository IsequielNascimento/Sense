using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class PerfilDeDisplayDeA13Tests
{
    #region MARK: Fixture

    private const string CodigoA13 = "A13";
    private const string AcaoResetarAlerta = "Resetar alerta";
    private const string AcaoDesligarAlerta = "Desligar alerta";
    private const int QuantidadeDeQuadrosResetarAlerta = 7;
    private const int QuantidadeDeQuadrosDesligarAlerta = 7;
    private const string ModelPath = "Assets/Prefab/Teste/M4SMARTTeste.fbx";
    private const string PrefabPath = "Assets/Resources/M4Problem13/M4SMARTTesteProblema13.prefab";

    private AlertaOficial a13;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a13 = CatalogoDeAlertas.Obter(CodigoA13, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA13);
    }

    private SequenciaDeQuadrosM4 EtapaResetarAlerta => perfil.EtapaOficial(0);
    private SequenciaDeQuadrosM4 EtapaDesligarAlerta => perfil.EtapaOficial(1);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA13_MantemAsDuasAcoesDaPagina76()
    {
        Assert.That(a13, Is.Not.Null);
        Assert.That(a13.Nome, Is.EqualTo("ALERTA DIAS"));
        Assert.That(a13.Padrao, Is.EqualTo("desabilitado"));
        Assert.That(a13.Acoes.Count, Is.EqualTo(2));
        Assert.That(a13.Acoes[0], Is.EqualTo(AcaoResetarAlerta));
        Assert.That(a13.Acoes[1], Is.EqualTo(AcaoDesligarAlerta));
        Assert.That(a13.Locais.Count, Is.EqualTo(2));
    }

    [Test]
    public void PerfilDeA13_TemExatamenteDuasEtapasOficiais()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA13));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(2));
        Assert.That(perfil.CorrespondeAoCatalogo(a13), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A13NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A24"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA13));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A8"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A11"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A12"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(4));
    }

    #endregion

    #region MARK: Quadros do LCD

    [Test]
    public void EtapaResetarAlerta_TemSeteQuadrosSemTextoVazio()
    {
        Assert.That(EtapaResetarAlerta.Quantidade, Is.EqualTo(QuantidadeDeQuadrosResetarAlerta));

        foreach (QuadroDeDisplayM4 quadro in EtapaResetarAlerta.Quadros)
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
        QuadroDeDisplayM4 entradaResetarAlerta = EtapaResetarAlerta.Em(1);
        QuadroDeDisplayM4 entradaDesligarAlerta = EtapaDesligarAlerta.Em(1);

        Assert.That(entradaResetarAlerta.TextoLcd, Is.EqualTo("MENU"));
        Assert.That(entradaResetarAlerta.Instrucao, Does.Contain("B2"));
        Assert.That(entradaResetarAlerta.Instrucao, Does.Contain("6 segundos"));
        Assert.That(entradaResetarAlerta.ProgressoSegundos, Is.EqualTo(6f));

        Assert.That(entradaDesligarAlerta.TextoLcd, Is.EqualTo("MENU"));
        Assert.That(entradaDesligarAlerta.Instrucao, Does.Contain("B2"));
        Assert.That(entradaDesligarAlerta.Instrucao, Does.Contain("6 segundos"));
        Assert.That(entradaDesligarAlerta.ProgressoSegundos, Is.EqualTo(6f));
    }

    [Test]
    public void AmbasAsEtapas_PercorremMenuAlertaAteA13()
    {
        foreach (SequenciaDeQuadrosM4 etapa in new[] { EtapaResetarAlerta, EtapaDesligarAlerta })
        {
            Assert.That(etapa.Em(2).TextoLcd, Is.EqualTo("MENU\nCONFIG"));
            Assert.That(etapa.Em(3).TextoLcd, Is.EqualTo("MENU\nALERTA"));
            Assert.That(etapa.Em(4).TextoLcd, Is.EqualTo("A13\nALERTA"));
        }
    }

    #endregion

    #region MARK: Reset da contagem de dias

    [Test]
    public void EstadoInicial_IdentificaA13EOsDiasTrabalhados()
    {
        QuadroDeDisplayM4 estadoInicial = EtapaResetarAlerta.Primeiro;

        Assert.That(estadoInicial.TextoLcd, Is.EqualTo(CodigoA13));
        Assert.That(estadoInicial.Instrucao, Does.Contain("dias"));
    }

    [Test]
    public void Reset_UsaLimparEIniciaNovaContagemSemMexerNosCiclos()
    {
        QuadroDeDisplayM4 limpar = EtapaResetarAlerta.Em(5);
        QuadroDeDisplayM4 confirmacaoReset = EtapaResetarAlerta.Ultimo;

        Assert.That(limpar.TextoLcd, Is.EqualTo("LIMPAR"));
        Assert.That(limpar.Instrucao, Does.Contain("B2"));
        Assert.That(confirmacaoReset.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(confirmacaoReset.Instrucao, Does.Contain("nova contagem"));
        Assert.That(confirmacaoReset.Instrucao, Does.Contain("parcial e total não são alterados"));
    }

    #endregion

    #region MARK: Separacao entre reset e desativacao

    [Test]
    public void Desativacao_NaoApagaAContagemDeDias()
    {
        QuadroDeDisplayM4 desabilitar = EtapaDesligarAlerta.Em(5);
        QuadroDeDisplayM4 confirmacaoDesligar = EtapaDesligarAlerta.Ultimo;

        Assert.That(desabilitar.TextoLcd, Is.EqualTo("DESABI"));
        Assert.That(confirmacaoDesligar.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(confirmacaoDesligar.Instrucao, Does.Contain("desligado"));
        Assert.That(confirmacaoDesligar.Instrucao, Does.Contain("não é apagada"));
    }

    [Test]
    public void PrimeiraEtapaNaoDesliga_ESegundaEtapaNaoReseta()
    {
        Assert.That(EtapaResetarAlerta.Quadros.Any(quadro => quadro.TextoLcd == "DESABI"), Is.False);
        Assert.That(EtapaDesligarAlerta.Quadros.Any(quadro => quadro.TextoLcd == "LIMPAR"), Is.False);
        Assert.That(EtapaDesligarAlerta.Primeiro.Instrucao, Does.Contain("sem resetar"));
    }

    [Test]
    public void ConfirmacoesDeResetEDesativacao_SaoTextualmenteDistintas()
    {
        Assert.That(
            EtapaResetarAlerta.Ultimo.Instrucao,
            Is.Not.EqualTo(EtapaDesligarAlerta.Ultimo.Instrucao));
    }

    #endregion

    #region MARK: LED e mecanismo sem evidencia normativa

    [Test]
    public void PerfilDeA13_NaoInventaLedVermelhoSemEvidenciaNormativa()
    {
        Assert.That(EtapaResetarAlerta.Quadros.All(quadro => !quadro.LedPiscando), Is.True);
        Assert.That(EtapaResetarAlerta.Quadros.All(quadro => quadro.Leds == EstadoLedsM4.Desligado), Is.True);
        Assert.That(EtapaDesligarAlerta.Quadros.All(quadro => !quadro.LedPiscando), Is.True);
        Assert.That(EtapaDesligarAlerta.Quadros.All(quadro => quadro.Leds == EstadoLedsM4.Desligado), Is.True);
    }

    [Test]
    public void PerfilDeA13_NaoAfirmaMecanismoDeAtivacaoConfirmado()
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
        Assert.That(navegacao.QuadroAtual.TextoLcd, Is.EqualTo(CodigoA13));
    }

    [Test]
    public void ProximoQuadro_PercorreOsQuadrosSemSairDaEtapaOficial()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);

        for (int i = 1; i < EtapaResetarAlerta.Quantidade; i++)
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
        Assert.That(navegacao.QuadroAtual.TextoLcd, Is.EqualTo(CodigoA13));
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
            tutorial = AcaoResetarAlerta,
            animacao = string.Empty,
            textoDisplay = "residual",
            alerta = "alerta residual",
            leds = QuadroDeDisplayM4.LedAberto,
            textoAngulo = "90",
            alertaTempoExcedido = "estourou",
            progressoSegundos = 5f,
            progressoEstoura = true,
        };

        EtapaResetarAlerta.Primeiro.Aplicar(etapa);

        Assert.That(etapa.textoDisplay, Is.EqualTo(CodigoA13));
        Assert.That(etapa.leds, Is.EqualTo(QuadroDeDisplayM4.LedApagado));
        Assert.That(etapa.alerta, Is.Empty);
        Assert.That(etapa.textoAngulo, Is.Empty);
        Assert.That(etapa.alertaTempoExcedido, Is.Empty);
        Assert.That(etapa.progressoSegundos, Is.EqualTo(0f));
        Assert.That(etapa.progressoEstoura, Is.False);
        Assert.That(etapa.animacao, Is.Empty);
    }

    #endregion

    #region MARK: Composicao sobre as etapas guiadas

    [Test]
    public void EtapasDeA13_ExpandemAsDuasAcoesOficiaisEmPassosPraticos()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a13, EtapasGuiadasDeAlerta.Criar(a13));

        Assert.That(a13.Acoes, Has.Count.EqualTo(2));
        Assert.That(etapas, Has.Length.EqualTo(QuantidadeDeQuadrosResetarAlerta + QuantidadeDeQuadrosDesligarAlerta));
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.tutorial)), Is.True);
        Assert.That(
            etapas.All(etapa => etapa.tutorial.Contains("B2")
                ? etapa.animacao == PerfisDeDisplayDeAlerta.AnimacaoB2
                : string.IsNullOrEmpty(etapa.animacao)),
            Is.True);
        Assert.That(etapas.All(TelaM4.EtapaTemConteudo), Is.True);
        Assert.That(etapas[1].progressoSegundos, Is.EqualTo(6f));
        Assert.That(etapas[QuantidadeDeQuadrosResetarAlerta + 1].progressoSegundos, Is.EqualTo(6f));
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
    public void ModeloDeA13_ResolveOPrefabBaseadoEmM4SmartTeste()
    {
        Assert.That(TipoModeloDeAlertaDisplay, Is.Not.Null);
        Assert.That(UsaM4SmartTeste(CodigoA13), Is.True);
        Assert.That(ResolverModelo("A24"), Is.Null);

        GameObject prefab = ResolverModelo(CodigoA13);

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
