using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class PerfilDeDisplayDeA18Tests
{
    #region MARK: Fixture

    private const string CodigoA18 = "A18";
    private const string CodigoA17 = "A17";
    private const string AcaoVerificarArComprimido = "Verificar fornecimento de ar comprimido";
    private const string AcaoDesligarAlerta = "Desligar alerta";
    private const int QuantidadeDeQuadrosVerificarArComprimido = 14;
    private const int QuantidadeDeQuadrosDesligarAlerta = 7;
    private const int LimiteDeCaracteresDaInstrucao = 120;
    private const string ModelPath = "Assets/Prefab/Teste/M4SMARTTeste.fbx";
    private const string PrefabPath = "Assets/Resources/M4ProblemA18/M4SMARTTesteProblemaA18.prefab";

    private AlertaOficial a18;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a18 = CatalogoDeAlertas.Obter(CodigoA18, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA18);
    }

    private SequenciaDeQuadrosM4 EtapaVerificarArComprimido => perfil.EtapaOficial(0);
    private SequenciaDeQuadrosM4 EtapaDesligarAlerta => perfil.EtapaOficial(1);

    private static SequenciaDeQuadrosM4 EtapaDeA17(int indice)
    {
        return PerfisDeDisplayDeAlerta.Obter(CodigoA17).EtapaOficial(indice);
    }

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA18_MantemAsDuasAcoesDaPagina76()
    {
        Assert.That(a18, Is.Not.Null);
        Assert.That(a18.Nome, Is.EqualTo("BAIXA PRESSÃO"));
        Assert.That(a18.Padrao, Is.EqualTo("20%"));
        Assert.That(a18.OQueE, Is.EqualTo("a pressão da linha é monitorada"));
        Assert.That(a18.EhOperacional, Is.True);
        Assert.That(a18.Acoes.Count, Is.EqualTo(2));
        Assert.That(a18.Acoes[0], Is.EqualTo(AcaoVerificarArComprimido));
        Assert.That(a18.Acoes[1], Is.EqualTo(AcaoDesligarAlerta));
        Assert.That(a18.Locais.Count, Is.EqualTo(2));
        Assert.That(a18.Locais[0], Is.EqualTo("gerador de ar comprimido"));
        Assert.That(a18.Locais[1], Is.EqualTo("menu baixa pressão"));
    }

    [Test]
    public void PerfilDeA18_TemExatamenteDuasEtapasOficiais()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA18));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(2));
        Assert.That(perfil.CorrespondeAoCatalogo(a18), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A18NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A10"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA18));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA17));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(24));
    }

    #endregion

    #region MARK: Reuso do motor de pressao do A17

    [Test]
    public void A18ReutilizaAFundacaoDeA17SemDuplicarOMotor()
    {
        Assert.That(perfil.QuantidadeDeEtapasOficiais,
            Is.EqualTo(PerfisDeDisplayDeAlerta.Obter(CodigoA17).QuantidadeDeEtapasOficiais));

        for (int indiceDaEtapa = 0; indiceDaEtapa < perfil.QuantidadeDeEtapasOficiais; indiceDaEtapa++)
        {
            SequenciaDeQuadrosM4 etapaDeA18 = perfil.EtapaOficial(indiceDaEtapa);
            SequenciaDeQuadrosM4 etapaDeA17 = EtapaDeA17(indiceDaEtapa);

            Assert.That(etapaDeA18.Quantidade, Is.EqualTo(etapaDeA17.Quantidade), $"etapa {indiceDaEtapa}");

            for (int i = 1; i < etapaDeA18.Quantidade; i++)
            {
                Assert.That(etapaDeA18.Em(i).Instrucao, Is.EqualTo(etapaDeA17.Em(i).Instrucao),
                    $"etapa {indiceDaEtapa}, quadro {i}");
                Assert.That(etapaDeA18.Em(i).Vfx, Is.EqualTo(etapaDeA17.Em(i).Vfx),
                    $"etapa {indiceDaEtapa}, quadro {i}");
            }
        }
    }

    [Test]
    public void CadaAlerta_MostraOProprioCodigoNoDisplayMesmoCompartilhandoOMotor()
    {
        for (int indiceDaEtapa = 0; indiceDaEtapa < perfil.QuantidadeDeEtapasOficiais; indiceDaEtapa++)
        {
            SequenciaDeQuadrosM4 etapaDeA18 = perfil.EtapaOficial(indiceDaEtapa);
            SequenciaDeQuadrosM4 etapaDeA17 = EtapaDeA17(indiceDaEtapa);

            for (int i = 0; i < etapaDeA18.Quantidade; i++)
            {
                string lcdDeA17 = etapaDeA17.Em(i).TextoLcd;

                Assert.That(
                    etapaDeA18.Em(i).TextoLcd,
                    Is.EqualTo(lcdDeA17.Replace(CodigoA17, CodigoA18)),
                    $"etapa {indiceDaEtapa}, quadro {i}: o display deve trazer o código do próprio alerta.");
            }
        }
    }

    [Test]
    public void DiagnosticoInicial_DistingueA18DeA17()
    {
        QuadroDeDisplayM4 inicialDeA18 = EtapaVerificarArComprimido.Primeiro;
        QuadroDeDisplayM4 inicialDeA17 = EtapaDeA17(0).Primeiro;

        Assert.That(inicialDeA18.TextoLcd, Is.EqualTo(CodigoA18));
        Assert.That(inicialDeA17.TextoLcd, Is.EqualTo(CodigoA17));
        Assert.That(inicialDeA18.Instrucao, Is.Not.EqualTo(inicialDeA17.Instrucao));
        Assert.That(inicialDeA18.Instrucao, Does.StartWith("Confirme o alerta A18"));
        Assert.That(inicialDeA18.Instrucao, Does.Contain("abaixo"));
        Assert.That(inicialDeA18.Instrucao, Does.Contain("C17"));
    }

    #endregion

    #region MARK: Fronteira entre baixa e alta pressao

    [Test]
    public void NenhumQuadroDeA18_CitaOCenarioDeAltaPressao()
    {
        foreach (SequenciaDeQuadrosM4 etapa in new[] { EtapaVerificarArComprimido, EtapaDesligarAlerta })
        {
            foreach (QuadroDeDisplayM4 quadro in etapa.Quadros)
            {
                Assert.That(quadro.Instrucao, Does.Not.Contain("acima"),
                    $"o quadro '{quadro.TextoLcd}' descreve o cenário do A17.");
            }
        }
    }

    [Test]
    public void AberturaDoDesligamento_DizAFaixaConfiguravelNoSentidoBaixo()
    {
        QuadroDeDisplayM4 abertura = EtapaDesligarAlerta.Primeiro;

        Assert.That(abertura.TextoLcd, Is.EqualTo(CodigoA18));
        Assert.That(abertura.Instrucao, Does.Contain("20%, 30%, 40% ou 50%"));
        Assert.That(abertura.Instrucao, Does.Contain("abaixo"));
        Assert.That(abertura.Instrucao, Does.Contain("C17"));
    }

    #endregion

    #region MARK: Encadeamento com a pressao da linha do C17

    [Test]
    public void PercentualDoAlerta_EMedidoSobreAPressaoDefinidaEmC17()
    {
        Assert.That(EtapaVerificarArComprimido.Em(1).Instrucao, Does.Contain("C17 PRESSÃO DA LINHA"));
        Assert.That(EtapaVerificarArComprimido.Em(2).Instrucao, Does.Contain("3 a 8 bar"));
        Assert.That(EtapaVerificarArComprimido.Em(2).Instrucao, Does.Contain("6 bar (87 psi)"));
        Assert.That(EtapaVerificarArComprimido.Em(3).Instrucao, Does.Contain("4,8 a 7,2 bar"));
        Assert.That(EtapaVerificarArComprimido.Em(10).TextoLcd,
            Is.EqualTo(PerfisDeDisplayDeAlerta.MenuPressaoDaLinha));
    }

    #endregion

    #region MARK: Desligamento pelo menu de alertas

    [Test]
    public void EtapaDesligarAlerta_PassaPeloMenuDeAlertasDoProprioCodigo()
    {
        Assert.That(EtapaDesligarAlerta.Em(1).TextoLcd, Is.EqualTo("MENU"));
        Assert.That(EtapaDesligarAlerta.Em(1).ProgressoSegundos, Is.EqualTo(6f));
        Assert.That(EtapaDesligarAlerta.Em(3).TextoLcd, Is.EqualTo("MENU\nALERTA"));
        Assert.That(EtapaDesligarAlerta.Em(4).TextoLcd, Is.EqualTo(PerfisDeDisplayDeAlerta.MenuPressaoBaixa));
        Assert.That(EtapaDesligarAlerta.Em(5).TextoLcd, Is.EqualTo("DESABI"));
        Assert.That(EtapaDesligarAlerta.Ultimo.TextoLcd, Is.EqualTo(PerfisDeDisplayDeAlerta.MenuPressaoBaixa));
        Assert.That(EtapaDesligarAlerta.Ultimo.Instrucao, Does.Contain("A9"));
    }

    #endregion

    #region MARK: Quadros do LCD

    [Test]
    public void EtapaVerificarArComprimido_TemQuatorzeQuadrosSemTextoVazio()
    {
        Assert.That(EtapaVerificarArComprimido.Quantidade, Is.EqualTo(QuantidadeDeQuadrosVerificarArComprimido));

        foreach (QuadroDeDisplayM4 quadro in EtapaVerificarArComprimido.Quadros)
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
        foreach (SequenciaDeQuadrosM4 etapa in new[] { EtapaVerificarArComprimido, EtapaDesligarAlerta })
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
    public void NenhumQuadro_RepeteOTextoResumidoDaTabelaDeResolucao()
    {
        Assert.That(
            EtapaVerificarArComprimido.Quadros.Select(quadro => quadro.Instrucao),
            Has.None.EqualTo(AcaoVerificarArComprimido));
        Assert.That(
            EtapaDesligarAlerta.Quadros.Select(quadro => quadro.Instrucao),
            Has.None.EqualTo(AcaoDesligarAlerta));
    }

    #endregion

    #region MARK: LED e destaque das pecas

    [Test]
    public void EnquantoOAlertaEstaAtivo_OLedVermelhoPisca()
    {
        foreach (SequenciaDeQuadrosM4 etapa in new[] { EtapaVerificarArComprimido, EtapaDesligarAlerta })
        {
            for (int i = 0; i < etapa.Quantidade - 1; i++)
            {
                Assert.That(etapa.Em(i).Leds, Is.EqualTo(EstadoLedsM4.Alerta), $"quadro {i}");
                Assert.That(etapa.Em(i).LedPiscando, Is.True, $"quadro {i}");
            }

            Assert.That(etapa.Ultimo.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
            Assert.That(etapa.Ultimo.LedPiscando, Is.False);
            Assert.That(etapa.Ultimo.Instrucao, Does.StartWith("Verifique a confirmação"));
        }
    }

    [Test]
    public void OGeradorDeArComprimido_AcendeAPneumaticaEAsMangueiras()
    {
        Assert.That(EtapaVerificarArComprimido.Em(4).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaquePneumatica));
        Assert.That(EtapaVerificarArComprimido.Em(5).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueMangueiras));
        Assert.That(EtapaVerificarArComprimido.Em(7).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaquePneumatica));
    }

    [Test]
    public void QuadrosDeMenu_NaoAcendemDestaqueNoModelo()
    {
        for (int i = 8; i < EtapaVerificarArComprimido.Quantidade; i++)
        {
            Assert.That(EtapaVerificarArComprimido.Em(i).Vfx, Is.Null, $"quadro {i}");
        }

        foreach (QuadroDeDisplayM4 quadro in EtapaDesligarAlerta.Quadros)
        {
            Assert.That(quadro.Vfx, Is.Null, $"o quadro '{quadro.TextoLcd}' é de menu.");
        }
    }

    #endregion

    #region MARK: Mecanismo sem evidencia normativa

    [Test]
    public void PerfilDeA18_NaoAfirmaMecanismoDeAtivacaoConfirmado()
    {
        Assert.That(perfil.MecanismoDeAtivacaoConfirmado, Is.False);
    }

    #endregion

    #region MARK: Animacao do botao citado em cada passo

    [Test]
    public void AnimacoesDeA18_SeguemOsBotoesCitadosEmCadaPasso()
    {
        Assert.That(
            EtapaVerificarArComprimido.Quadros.Select(quadro => quadro.Animacao),
            Is.EqualTo(new[]
            {
                null,
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

        Assert.That(
            EtapaDesligarAlerta.Quadros.Select(quadro => quadro.Animacao),
            Is.EqualTo(new[]
            {
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
    public void EtapasDeA18_ExpandemAsDuasAcoesOficiaisEmPassosPraticos()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a18, EtapasGuiadasDeAlerta.Criar(a18));

        Assert.That(etapas, Has.Length.EqualTo(
            QuantidadeDeQuadrosVerificarArComprimido + QuantidadeDeQuadrosDesligarAlerta));
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.tutorial)), Is.True);
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.textoDisplay)), Is.True);
        Assert.That(etapas[QuantidadeDeQuadrosVerificarArComprimido + 1].progressoSegundos, Is.EqualTo(6f));
    }

    #endregion

    #region MARK: Modelo visual obrigatorio

    private static readonly Type TipoModeloDeAlertaDisplay =
        Type.GetType("ModeloDeAlertaDisplay, Sense.Runtime");

    private static GameObject ResolverModelo(string codigo)
    {
        return TipoModeloDeAlertaDisplay
            .GetMethod("Resolver")
            .Invoke(null, new object[] { codigo }) as GameObject;
    }

    [Test]
    public void ModeloDeA18_ResolveOPrefabBaseadoEmM4SmartTeste()
    {
        Assert.That(TipoModeloDeAlertaDisplay, Is.Not.Null);

        GameObject prefab = ResolverModelo(CodigoA18);

        Assert.That(prefab, Is.Not.Null);
        Assert.That(AssetDatabase.GetAssetPath(prefab), Is.EqualTo(PrefabPath));
        Assert.That(AssetDatabase.GetDependencies(PrefabPath), Does.Contain(ModelPath));
        Assert.That(prefab.GetComponentsInChildren<Transform>(true)
            .Any(item => item.name == "DisplayDynamic"), Is.True);
        Assert.That(prefab.GetComponentInChildren<ControladorLedsM4>(true), Is.Not.Null);
    }

    #endregion
}
