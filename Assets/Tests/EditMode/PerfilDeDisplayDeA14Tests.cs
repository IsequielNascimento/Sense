using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class PerfilDeDisplayDeA14Tests
{
    #region MARK: Fixture

    private const string CodigoA14 = "A14";
    private const string AcaoDefinirNovaData = "Definir nova data";
    private const string AcaoDesligarAlerta = "Desligar alerta";
    private const int QuantidadeDeQuadrosDefinirNovaData = 8;
    private const int QuantidadeDeQuadrosDesligarAlerta = 7;
    private const string ExemploDeData = "31 12\n2023";
    private const int LimiteDeCaracteresDaInstrucao = 120;
    private const string ModelPath = "Assets/Prefab/Teste/M4SMARTTeste.fbx";
    private const string PrefabPath = "Assets/Resources/M4Problem1/M4SMARTTesteProblema1.prefab";

    private AlertaOficial a14;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a14 = CatalogoDeAlertas.Obter(CodigoA14, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA14);
    }

    private SequenciaDeQuadrosM4 EtapaDefinirNovaData => perfil.EtapaOficial(0);
    private SequenciaDeQuadrosM4 EtapaDesligarAlerta => perfil.EtapaOficial(1);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA14_MantemAsDuasAcoesDaPagina76()
    {
        Assert.That(a14, Is.Not.Null);
        Assert.That(a14.Nome, Is.EqualTo("ALERTA DATA"));
        Assert.That(a14.Padrao, Is.EqualTo("desabilitado"));
        Assert.That(a14.Acoes.Count, Is.EqualTo(2));
        Assert.That(a14.Acoes[0], Is.EqualTo(AcaoDefinirNovaData));
        Assert.That(a14.Acoes[1], Is.EqualTo(AcaoDesligarAlerta));
        Assert.That(a14.Locais.Count, Is.EqualTo(2));
    }

    [Test]
    public void PerfilDeA14_TemExatamenteDuasEtapasOficiais()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA14));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(2));
        Assert.That(perfil.CorrespondeAoCatalogo(a14), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A14NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A24"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA14));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A8"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A13"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(13));
    }

    #endregion

    #region MARK: Quadros do LCD

    [Test]
    public void EtapaDefinirNovaData_TemOitoQuadrosSemTextoVazio()
    {
        Assert.That(EtapaDefinirNovaData.Quantidade, Is.EqualTo(QuantidadeDeQuadrosDefinirNovaData));

        foreach (QuadroDeDisplayM4 quadro in EtapaDefinirNovaData.Quadros)
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
    public void Instrucoes_CabemNaCaixaDeTextoDoPassoAPasso()
    {
        foreach (SequenciaDeQuadrosM4 etapa in new[] { EtapaDefinirNovaData, EtapaDesligarAlerta })
        {
            foreach (QuadroDeDisplayM4 quadro in etapa.Quadros)
            {
                Assert.That(
                    quadro.Instrucao.Length,
                    Is.LessThanOrEqualTo(LimiteDeCaracteresDaInstrucao),
                    $"Instrução longa demais no quadro '{quadro.TextoLcd}'.");
            }
        }
    }

    [Test]
    public void Instrucoes_NaoCitamOManualNemJustificamEscolhas()
    {
        string[] termosProibidos = { "Figura", "manual", "página", "exemplo" };

        foreach (SequenciaDeQuadrosM4 etapa in new[] { EtapaDefinirNovaData, EtapaDesligarAlerta })
        {
            foreach (QuadroDeDisplayM4 quadro in etapa.Quadros)
            {
                foreach (string termo in termosProibidos)
                {
                    Assert.That(
                        quadro.Instrucao,
                        Does.Not.Contain(termo).IgnoreCase,
                        $"Instrução do quadro '{quadro.TextoLcd}' cita '{termo}'.");
                }
            }
        }
    }

    [Test]
    public void EntradaNoMenu_UsaB2PorSeisSegundosEmAmbasAsEtapas()
    {
        QuadroDeDisplayM4 entradaDefinirNovaData = EtapaDefinirNovaData.Em(1);
        QuadroDeDisplayM4 entradaDesligarAlerta = EtapaDesligarAlerta.Em(1);

        Assert.That(entradaDefinirNovaData.TextoLcd, Is.EqualTo("MENU"));
        Assert.That(entradaDefinirNovaData.Instrucao, Does.Contain("B2"));
        Assert.That(entradaDefinirNovaData.Instrucao, Does.Contain("6 segundos"));
        Assert.That(entradaDefinirNovaData.ProgressoSegundos, Is.EqualTo(6f));

        Assert.That(entradaDesligarAlerta.TextoLcd, Is.EqualTo("MENU"));
        Assert.That(entradaDesligarAlerta.Instrucao, Does.Contain("B2"));
        Assert.That(entradaDesligarAlerta.Instrucao, Does.Contain("6 segundos"));
        Assert.That(entradaDesligarAlerta.ProgressoSegundos, Is.EqualTo(6f));
    }

    [Test]
    public void AmbasAsEtapas_PercorremMenuAlertaAteA14()
    {
        foreach (SequenciaDeQuadrosM4 etapa in new[] { EtapaDefinirNovaData, EtapaDesligarAlerta })
        {
            Assert.That(etapa.Em(2).TextoLcd, Is.EqualTo("MENU\nCONFIG"));
            Assert.That(etapa.Em(3).TextoLcd, Is.EqualTo("MENU\nALERTA"));
            Assert.That(etapa.Em(4).TextoLcd, Is.EqualTo("A14\nALERTA"));
        }
    }

    #endregion

    #region MARK: Edicao da data e relogio C13

    [Test]
    public void EstadoInicial_IndicaQueORelogioC13PrecisaEstarAjustado()
    {
        QuadroDeDisplayM4 estadoInicial = EtapaDefinirNovaData.Primeiro;

        Assert.That(estadoInicial.TextoLcd, Is.EqualTo(CodigoA14));
        Assert.That(estadoInicial.Instrucao, Does.Contain("C13"));
    }

    [Test]
    public void EdicaoDaData_UsaHabilitarEOsComandosDeDigito()
    {
        QuadroDeDisplayM4 habilitar = EtapaDefinirNovaData.Em(5);
        QuadroDeDisplayM4 edicao = EtapaDefinirNovaData.Em(6);

        Assert.That(habilitar.TextoLcd, Is.EqualTo("HABILI"));
        Assert.That(habilitar.Instrucao, Does.Contain("B2"));
        Assert.That(edicao.TextoLcd, Is.EqualTo(ExemploDeData));
        Assert.That(edicao.Instrucao, Does.Contain("B1"));
        Assert.That(edicao.Instrucao, Does.Contain("B3"));
        Assert.That(edicao.Instrucao, Does.Contain("3 segundos"));
    }

    [Test]
    public void DataDoManual_NaoEApresentadaComoDadoDeProducao()
    {
        QuadroDeDisplayM4 edicao = EtapaDefinirNovaData.Em(6);

        Assert.That(edicao.Instrucao, Does.Contain("data da manutenção"));

        int quadrosComData = EtapaDefinirNovaData.Quadros
            .Concat(EtapaDesligarAlerta.Quadros)
            .Count(quadro => quadro.TextoLcd == ExemploDeData);

        Assert.That(quadrosComData, Is.EqualTo(1));
    }

    [Test]
    public void ConfirmacaoDaData_AvisaQueCancelarNaoSalvaAlteracaoParcial()
    {
        QuadroDeDisplayM4 confirmacao = EtapaDefinirNovaData.Ultimo;

        Assert.That(confirmacao.TextoLcd, Is.EqualTo("A14\nALERTA"));
        Assert.That(confirmacao.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(confirmacao.Instrucao, Does.Contain("Data salva"));
        Assert.That(confirmacao.Instrucao, Does.Contain("B1"));
        Assert.That(confirmacao.Instrucao, Does.Contain("não salva nada"));
    }

    #endregion

    #region MARK: Separacao entre definir data e desativacao

    [Test]
    public void Desativacao_NaoAlteraORelogioC13()
    {
        QuadroDeDisplayM4 desabilitar = EtapaDesligarAlerta.Em(5);
        QuadroDeDisplayM4 confirmacaoDesligar = EtapaDesligarAlerta.Ultimo;

        Assert.That(desabilitar.TextoLcd, Is.EqualTo("DESABI"));
        Assert.That(confirmacaoDesligar.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(confirmacaoDesligar.Instrucao, Does.Contain("desligado"));
        Assert.That(confirmacaoDesligar.Instrucao, Does.Contain("C13 não é alterado"));
    }

    [Test]
    public void PrimeiraEtapaNaoDesliga_ESegundaEtapaNaoEditaData()
    {
        Assert.That(EtapaDefinirNovaData.Quadros.Any(quadro => quadro.TextoLcd == "DESABI"), Is.False);
        Assert.That(EtapaDesligarAlerta.Quadros.Any(quadro => quadro.TextoLcd == "HABILI"), Is.False);
        Assert.That(EtapaDesligarAlerta.Primeiro.Instrucao, Does.Contain("sem definir nova data"));
    }

    [Test]
    public void ConfirmacoesDeDataEDesativacao_SaoTextualmenteDistintas()
    {
        Assert.That(
            EtapaDefinirNovaData.Ultimo.Instrucao,
            Is.Not.EqualTo(EtapaDesligarAlerta.Ultimo.Instrucao));
    }

    #endregion

    #region MARK: LED e mecanismo sem evidencia normativa

    [Test]
    public void PerfilDeA14_NaoInventaLedVermelhoSemEvidenciaNormativa()
    {
        Assert.That(EtapaDefinirNovaData.Quadros.All(quadro => !quadro.LedPiscando), Is.True);
        Assert.That(EtapaDefinirNovaData.Quadros.All(quadro => quadro.Leds == EstadoLedsM4.Desligado), Is.True);
        Assert.That(EtapaDesligarAlerta.Quadros.All(quadro => !quadro.LedPiscando), Is.True);
        Assert.That(EtapaDesligarAlerta.Quadros.All(quadro => quadro.Leds == EstadoLedsM4.Desligado), Is.True);
    }

    [Test]
    public void PerfilDeA14_NaoAfirmaMecanismoDeAtivacaoConfirmado()
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
        Assert.That(navegacao.QuadroAtual.TextoLcd, Is.EqualTo(CodigoA14));
    }

    [Test]
    public void ProximoQuadro_PercorreOsQuadrosSemSairDaEtapaOficial()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);

        for (int i = 1; i < EtapaDefinirNovaData.Quantidade; i++)
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
        Assert.That(navegacao.QuadroAtual.TextoLcd, Is.EqualTo(CodigoA14));
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
            tutorial = AcaoDefinirNovaData,
            animacao = string.Empty,
            textoDisplay = "residual",
            alerta = "alerta residual",
            leds = QuadroDeDisplayM4.LedAberto,
            textoAngulo = "90",
            alertaTempoExcedido = "estourou",
            progressoSegundos = 5f,
            progressoEstoura = true,
        };

        EtapaDefinirNovaData.Primeiro.Aplicar(etapa);

        Assert.That(etapa.textoDisplay, Is.EqualTo(CodigoA14));
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
    public void AnimacoesDeA14_SeguemOsBotoesCitadosEmCadaPasso()
    {
        Assert.That(
            EtapaDefinirNovaData.Quadros.Concat(EtapaDesligarAlerta.Quadros).Select(quadro => quadro.Animacao),
            Is.EqualTo(new[]
            {
                null,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B3,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B23,
                AnimacaoDeBotaoM4.B23,
                AnimacaoDeBotaoM4.B123,
                AnimacaoDeBotaoM4.B1,
                null,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B3,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B23,
                AnimacaoDeBotaoM4.B23,
                null,
            }));
    }

    #endregion

    #region MARK: Composicao sobre as etapas guiadas

    [Test]
    public void EtapasDeA14_ExpandemAsDuasAcoesOficiaisEmPassosPraticos()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a14, EtapasGuiadasDeAlerta.Criar(a14));

        Assert.That(a14.Acoes, Has.Count.EqualTo(2));
        Assert.That(etapas, Has.Length.EqualTo(QuantidadeDeQuadrosDefinirNovaData + QuantidadeDeQuadrosDesligarAlerta));
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.tutorial)), Is.True);
        Assert.That(etapas.Select(etapa => etapa.animacao), Is.EqualTo(
            EtapaDefinirNovaData.Quadros.Concat(EtapaDesligarAlerta.Quadros).Select(quadro => quadro.Animacao ?? string.Empty)));
        Assert.That(etapas.All(TelaM4.EtapaTemConteudo), Is.True);
        Assert.That(etapas[1].progressoSegundos, Is.EqualTo(6f));
        Assert.That(etapas[QuantidadeDeQuadrosDefinirNovaData + 1].progressoSegundos, Is.EqualTo(6f));
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
    public void ModeloDeA14_ResolveOPrefabBaseadoEmM4SmartTeste()
    {
        Assert.That(TipoModeloDeAlertaDisplay, Is.Not.Null);
        Assert.That(UsaM4SmartTeste(CodigoA14), Is.True);
        Assert.That(ResolverModelo("A24"), Is.Null);

        GameObject prefab = ResolverModelo(CodigoA14);

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
