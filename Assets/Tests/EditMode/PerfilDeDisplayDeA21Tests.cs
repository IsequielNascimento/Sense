using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class PerfilDeDisplayDeA21Tests
{
    #region MARK: Fixture

    private const string CodigoA21 = "A21";
    private const string AcaoVerificarTemperatura = "Verificar temperatura do processo";
    private const int QuantidadeDeQuadros = 7;
    private const int LimiteDeCaracteresDaInstrucao = 120;
    private const string ModelPath = "Assets/Prefab/Teste/M4SMARTTeste.fbx";
    private const string PrefabPath = "Assets/Resources/M4Problem1/M4SMARTTesteProblema1.prefab";

    private AlertaOficial a21;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a21 = CatalogoDeAlertas.Obter(CodigoA21, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA21);
    }

    private SequenciaDeQuadrosM4 EtapaUnica => perfil.EtapaOficial(0);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA21_MantemAAcaoUnicaDaPagina76()
    {
        Assert.That(a21, Is.Not.Null);
        Assert.That(a21.Nome, Is.EqualTo("TEMPERATURA ALTA"));
        Assert.That(a21.Padrao, Is.EqualTo("habilitado"));
        Assert.That(a21.Acoes.Count, Is.EqualTo(1));
        Assert.That(a21.Acoes[0], Is.EqualTo(AcaoVerificarTemperatura));
        Assert.That(a21.Locais.Count, Is.EqualTo(1));
        Assert.That(a21.Locais[0], Is.EqualTo("em campo"));
    }

    [Test]
    public void PerfilDeA21_TemExatamenteUmaEtapaOficial()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA21));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(1));
        Assert.That(perfil.CorrespondeAoCatalogo(a21), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A21NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A24"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A22"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA21));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(6));
    }

    #endregion

    #region MARK: Quadros do LCD

    [Test]
    public void EtapaUnica_TemSeteQuadrosSemTextoVazio()
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
    public void Instrucoes_NaoCitamOManualNemJustificamEscolhas()
    {
        string[] termosProibidos = { "Figura", "manual", "página", "exemplo" };

        foreach (QuadroDeDisplayM4 quadro in EtapaUnica.Quadros)
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

    [Test]
    public void EntradaNoMenu_UsaB2PorSeisSegundos()
    {
        QuadroDeDisplayM4 entrada = EtapaUnica.Em(1);

        Assert.That(entrada.TextoLcd, Is.EqualTo("MENU"));
        Assert.That(entrada.Instrucao, Does.Contain("B2"));
        Assert.That(entrada.Instrucao, Does.Contain("6 segundos"));
        Assert.That(entrada.ProgressoSegundos, Is.EqualTo(6f));
    }

    #endregion

    #region MARK: Temperatura interna, limite e inspecao

    [Test]
    public void EstadoInicial_ApresentaOLimiteSuperiorComprovado()
    {
        QuadroDeDisplayM4 estadoInicial = EtapaUnica.Primeiro;

        Assert.That(estadoInicial.TextoLcd, Is.EqualTo(CodigoA21));
        Assert.That(estadoInicial.Instrucao, Does.Contain("70°"));
    }

    [Test]
    public void LimiteDoManual_NaoInventaUnidadeNemConversao()
    {
        string[] unidadesInventadas = { "70°C", "70 °C", "°F", "Fahrenheit", "Celsius", "Kelvin" };

        foreach (QuadroDeDisplayM4 quadro in EtapaUnica.Quadros)
        {
            foreach (string unidade in unidadesInventadas)
            {
                Assert.That(
                    quadro.Instrucao,
                    Does.Not.Contain(unidade).IgnoreCase,
                    $"Instrução do quadro '{quadro.TextoLcd}' inventa a unidade '{unidade}'.");
            }
        }
    }

    [Test]
    public void FluxoUsaOModoDeTemperaturaDoDisplayPeloC3()
    {
        QuadroDeDisplayM4 menuDisplay = EtapaUnica.Em(3);
        QuadroDeDisplayM4 selecaoTemperatura = EtapaUnica.Em(4);

        Assert.That(menuDisplay.TextoLcd, Is.EqualTo("C3\nDISPLA"));
        Assert.That(menuDisplay.Instrucao, Does.Contain("C3"));
        Assert.That(selecaoTemperatura.TextoLcd, Is.EqualTo("TEMPER"));
        Assert.That(selecaoTemperatura.Instrucao, Does.Contain("TEMPERATURA"));
    }

    [Test]
    public void QuadroFinal_MostraTemperaturaInternaEPedeInspecaoEmCampo()
    {
        QuadroDeDisplayM4 saida = EtapaUnica.Em(5);
        QuadroDeDisplayM4 inspecao = EtapaUnica.Ultimo;

        Assert.That(saida.TextoLcd, Is.EqualTo("SAIR"));
        Assert.That(inspecao.TextoLcd, Is.EqualTo("TEMPER"));
        Assert.That(inspecao.Instrucao, Does.Contain("temperatura interna"));
        Assert.That(inspecao.Instrucao, Does.Contain("em campo"));
    }

    [Test]
    public void Fluxo_NaoSimulaReparoDesmontagemOuMedicaoExterna()
    {
        string[] termosProibidos =
        {
            "desmont", "remova", "retire", "parafus", "substitu", "troque", "termômetro", "sensor externo",
        };

        foreach (QuadroDeDisplayM4 quadro in EtapaUnica.Quadros)
        {
            foreach (string termo in termosProibidos)
            {
                Assert.That(
                    quadro.Instrucao,
                    Does.Not.Contain(termo).IgnoreCase,
                    $"Instrução do quadro '{quadro.TextoLcd}' simula '{termo}'.");
            }
        }
    }

    #endregion

    #region MARK: Ausencia de fluxo de desativacao

    [Test]
    public void PerfilDeA21_NaoAdicionaFluxoDeDesabilitarAlerta()
    {
        Assert.That(EtapaUnica.Quadros.Any(quadro => quadro.TextoLcd == "DESABI"), Is.False);
        Assert.That(EtapaUnica.Quadros.Any(quadro => quadro.TextoLcd == "HABILI"), Is.False);
        Assert.That(EtapaUnica.Quadros.Any(quadro => quadro.TextoLcd == "MENU\nALERTA"), Is.False);

        foreach (QuadroDeDisplayM4 quadro in EtapaUnica.Quadros)
        {
            Assert.That(quadro.Instrucao, Does.Not.Contain("desabilit").IgnoreCase);
            Assert.That(quadro.Instrucao, Does.Not.Contain("desligar o alerta").IgnoreCase);
        }
    }

    #endregion

    #region MARK: LED e mecanismo sem evidencia normativa

    [Test]
    public void PerfilDeA21_NaoInventaLedVermelhoSemEvidenciaNormativa()
    {
        Assert.That(EtapaUnica.Quadros.All(quadro => !quadro.LedPiscando), Is.True);
        Assert.That(EtapaUnica.Quadros.All(quadro => quadro.Leds == EstadoLedsM4.Desligado), Is.True);
    }

    [Test]
    public void PerfilDeA21_NaoAfirmaMecanismoDeAtivacaoConfirmado()
    {
        Assert.That(perfil.MecanismoDeAtivacaoConfirmado, Is.False);
    }

    #endregion

    #region MARK: Navegacao determinista

    [Test]
    public void Navegacao_ComecaNoPrimeiroQuadroDaEtapaUnica()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);

        Assert.That(navegacao.IndiceDaEtapaOficial, Is.EqualTo(0));
        Assert.That(navegacao.IndiceDoQuadro, Is.EqualTo(0));
        Assert.That(navegacao.QuadroAtual.TextoLcd, Is.EqualTo(CodigoA21));
    }

    [Test]
    public void ProximoQuadro_PercorreOsQuadrosSemSairDaEtapaOficial()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);

        for (int i = 1; i < EtapaUnica.Quantidade; i++)
        {
            Assert.That(navegacao.ProximoQuadro(), Is.True);
            Assert.That(navegacao.IndiceDoQuadro, Is.EqualTo(i));
            Assert.That(navegacao.IndiceDaEtapaOficial, Is.EqualTo(0));
        }

        Assert.That(navegacao.ProximoQuadro(), Is.False);
        Assert.That(navegacao.EstaNoUltimoQuadro, Is.True);
    }

    [Test]
    public void Repetir_ReiniciaNoPrimeiroQuadro()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);
        navegacao.ProximoQuadro();
        navegacao.ProximoQuadro();

        navegacao.Repetir();

        Assert.That(navegacao.IndiceDoQuadro, Is.EqualTo(0));
        Assert.That(navegacao.QuadroAtual.TextoLcd, Is.EqualTo(CodigoA21));
    }

    [Test]
    public void AvancarEVoltar_NaoSaemDaUnicaEtapaOficial()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);

        Assert.That(navegacao.Avancar(), Is.False);
        Assert.That(navegacao.Voltar(), Is.False);
        Assert.That(navegacao.IndiceDaEtapaOficial, Is.EqualTo(0));
    }

    [Test]
    public void Reiniciar_VoltaAoEstadoInicial()
    {
        var navegacao = new NavegacaoDeQuadrosDeAlerta(perfil);
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
            tutorial = AcaoVerificarTemperatura,
            animacao = string.Empty,
            textoDisplay = "residual",
            alerta = "alerta residual",
            leds = QuadroDeDisplayM4.LedAberto,
            textoAngulo = "90",
            alertaTempoExcedido = "estourou",
            progressoSegundos = 5f,
            progressoEstoura = true,
        };

        EtapaUnica.Primeiro.Aplicar(etapa);

        Assert.That(etapa.textoDisplay, Is.EqualTo(CodigoA21));
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
    public void EtapasDeA21_ExpandemAAcaoOficialEmPassosPraticos()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a21, EtapasGuiadasDeAlerta.Criar(a21));

        Assert.That(a21.Acoes, Has.Count.EqualTo(1));
        Assert.That(etapas, Has.Length.EqualTo(QuantidadeDeQuadros));
        Assert.That(etapas.Select(etapa => etapa.textoDisplay), Is.EqualTo(
            EtapaUnica.Quadros.Select(quadro => quadro.TextoLcd)));
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.tutorial)), Is.True);
        Assert.That(etapas.All(etapa => string.IsNullOrEmpty(etapa.animacao)), Is.True);
        Assert.That(etapas.All(TelaM4.EtapaTemConteudo), Is.True);
        Assert.That(etapas[1].progressoSegundos, Is.EqualTo(6f));
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
    public void ModeloDeA21_ResolveOPrefabBaseadoEmM4SmartTeste()
    {
        Assert.That(TipoModeloDeAlertaDisplay, Is.Not.Null);
        Assert.That(UsaM4SmartTeste(CodigoA21), Is.True);
        Assert.That(ResolverModelo("A24"), Is.Null);

        GameObject prefab = ResolverModelo(CodigoA21);

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
