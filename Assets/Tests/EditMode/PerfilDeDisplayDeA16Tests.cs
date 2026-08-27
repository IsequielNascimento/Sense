using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class PerfilDeDisplayDeA16Tests
{
    #region MARK: Fixture

    private const string CodigoA16 = "A16";
    private const string CodigoA15 = "A15";
    private const string AcaoVerificarArComprimido = "Verificar o fornecimento de ar comprimido";
    private const string AcaoVerificarAvarias = "Verificar possíveis avarias no conjunto válvula / atuador";
    private const string AcaoDesligarOAlerta = "Desligar o alerta";
    private const int QuantidadeDeQuadrosVerificarArComprimido = 11;
    private const int QuantidadeDeQuadrosVerificarAvarias = 11;
    private const int QuantidadeDeQuadrosDesligarOAlerta = 7;
    private const int LimiteDeCaracteresDaInstrucao = 120;
    private const string ModelPath = "Assets/Prefab/Teste/M4SMARTTeste.fbx";
    private const string PrefabPath = "Assets/Resources/M4ProblemA16/M4SMARTTesteProblemaA16.prefab";

    private AlertaOficial a16;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a16 = CatalogoDeAlertas.Obter(CodigoA16, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA16);
    }

    private SequenciaDeQuadrosM4 EtapaVerificarArComprimido => perfil.EtapaOficial(0);
    private SequenciaDeQuadrosM4 EtapaVerificarAvarias => perfil.EtapaOficial(1);
    private SequenciaDeQuadrosM4 EtapaDesligarOAlerta => perfil.EtapaOficial(2);

    private SequenciaDeQuadrosM4[] TodasAsEtapas =>
        new[] { EtapaVerificarArComprimido, EtapaVerificarAvarias, EtapaDesligarOAlerta };

    private static SequenciaDeQuadrosM4 EtapaDeA15(int indice)
    {
        return PerfisDeDisplayDeAlerta.Obter(CodigoA15).EtapaOficial(indice);
    }

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA16_MantemAsTresAcoesDaPagina76()
    {
        Assert.That(a16, Is.Not.Null);
        Assert.That(a16.Nome, Is.EqualTo("TEMPO FECHAMENTO"));
        Assert.That(a16.Padrao, Is.EqualTo("desabilitado"));
        Assert.That(a16.OQueE, Is.EqualTo(
            "monitor aprende o tempo de fechamento da válvula durante o processo de calibração"));
        Assert.That(a16.QuandoOcorre, Is.EqualTo("quando o tempo de fechamento é ultrapassado"));
        Assert.That(a16.Acoes.Count, Is.EqualTo(3));
        Assert.That(a16.Acoes[0], Is.EqualTo(AcaoVerificarArComprimido));
        Assert.That(a16.Acoes[1], Is.EqualTo(AcaoVerificarAvarias));
        Assert.That(a16.Acoes[2], Is.EqualTo(AcaoDesligarOAlerta));
        Assert.That(a16.Locais.Count, Is.EqualTo(3));
        Assert.That(a16.Locais[0], Is.EqualTo("gerador de ar comprimido"));
        Assert.That(a16.Locais[1], Is.EqualTo("em campo"));
        Assert.That(a16.Locais[2], Is.EqualTo("menu tempo fechamento"));
    }

    [Test]
    public void PerfilDeA16_TemExatamenteTresEtapasOficiais()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA16));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(3));
        Assert.That(perfil.CorrespondeAoCatalogo(a16), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A16NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A10"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA16));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA15));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(24));
    }

    #endregion

    #region MARK: Reuso do motor de tempo de curso do A15

    [Test]
    public void A16ReutilizaAFundacaoDeA15SemDuplicarOMotor()
    {
        Assert.That(
            perfil.QuantidadeDeEtapasOficiais,
            Is.EqualTo(PerfisDeDisplayDeAlerta.Obter(CodigoA15).QuantidadeDeEtapasOficiais));

        for (int indiceDaEtapa = 0; indiceDaEtapa < perfil.QuantidadeDeEtapasOficiais; indiceDaEtapa++)
        {
            SequenciaDeQuadrosM4 etapaDeA16 = perfil.EtapaOficial(indiceDaEtapa);
            SequenciaDeQuadrosM4 etapaDeA15 = EtapaDeA15(indiceDaEtapa);

            Assert.That(etapaDeA16.Quantidade, Is.EqualTo(etapaDeA15.Quantidade), $"etapa {indiceDaEtapa}");

            for (int i = 1; i < etapaDeA16.Quantidade; i++)
            {
                Assert.That(etapaDeA16.Em(i).Instrucao, Is.EqualTo(etapaDeA15.Em(i).Instrucao),
                    $"etapa {indiceDaEtapa}, quadro {i}");
                Assert.That(etapaDeA16.Em(i).Vfx, Is.EqualTo(etapaDeA15.Em(i).Vfx),
                    $"etapa {indiceDaEtapa}, quadro {i}");
            }
        }
    }

    [Test]
    public void CadaAlerta_MostraOProprioCodigoNoDisplayMesmoCompartilhandoOMotor()
    {
        for (int indiceDaEtapa = 0; indiceDaEtapa < perfil.QuantidadeDeEtapasOficiais; indiceDaEtapa++)
        {
            SequenciaDeQuadrosM4 etapaDeA16 = perfil.EtapaOficial(indiceDaEtapa);
            SequenciaDeQuadrosM4 etapaDeA15 = EtapaDeA15(indiceDaEtapa);

            for (int i = 0; i < etapaDeA16.Quantidade; i++)
            {
                string lcdDeA15 = etapaDeA15.Em(i).TextoLcd;

                Assert.That(
                    etapaDeA16.Em(i).TextoLcd,
                    Is.EqualTo(lcdDeA15.Replace(CodigoA15, CodigoA16)),
                    $"etapa {indiceDaEtapa}, quadro {i}");
            }
        }
    }

    [Test]
    public void DiagnosticoInicialDeCadaSequencia_DistingueA16DeA15()
    {
        for (int indiceDaEtapa = 0; indiceDaEtapa < perfil.QuantidadeDeEtapasOficiais; indiceDaEtapa++)
        {
            Assert.That(
                perfil.EtapaOficial(indiceDaEtapa).Primeiro.Instrucao,
                Is.Not.EqualTo(EtapaDeA15(indiceDaEtapa).Primeiro.Instrucao),
                $"etapa {indiceDaEtapa}");
        }

        Assert.That(EtapaVerificarArComprimido.Primeiro.Instrucao, Does.StartWith("Confirme o alerta A16"));
        Assert.That(EtapaVerificarArComprimido.Primeiro.Instrucao, Does.Contain("fechou por completo"));
        Assert.That(EtapaVerificarArComprimido.Primeiro.Instrucao, Does.Contain("aprendido na calibração"));
    }

    #endregion

    #region MARK: Fronteira contra os outros alertas de curso

    [Test]
    public void NenhumQuadroDeA16_InvadeODiagnosticoDeA1A2A3OuA5()
    {
        foreach (QuadroDeDisplayM4 quadro in TodasAsEtapas.SelectMany(etapa => etapa.Quadros))
        {
            Assert.That(quadro.Instrucao, Does.Not.Contain("30°"),
                $"o quadro '{quadro.TextoLcd}' descreve o curso curto do A1.");
            Assert.That(quadro.Instrucao, Does.Not.Contain("C6"),
                $"o quadro '{quadro.TextoLcd}' descreve o tempo máximo de calibração do A2.");
            Assert.That(quadro.Instrucao, Does.Not.Contain("TEMPO MAX CAL"),
                $"o quadro '{quadro.TextoLcd}' descreve o tempo máximo de calibração do A2.");
            Assert.That(quadro.Instrucao, Does.Not.Contain("pontos"),
                $"o quadro '{quadro.TextoLcd}' descreve os pontos divergentes do A3.");
            Assert.That(quadro.Instrucao, Does.Not.Contain("movimento"),
                $"o quadro '{quadro.TextoLcd}' descreve a ausência de movimento do A5.");
        }
    }

    [Test]
    public void DiagnosticoInicial_NaoRepeteOInicioDeNenhumOutroAlertaDeCurso()
    {
        string inicialDeA16 = EtapaVerificarArComprimido.Primeiro.Instrucao;

        foreach (string outro in new[] { "A1", "A2", "A3", "A5" })
        {
            Assert.That(
                PerfisDeDisplayDeAlerta.Obter(outro).EtapasOficiais
                    .SelectMany(etapa => etapa.Quadros)
                    .Select(quadro => quadro.Instrucao),
                Has.None.EqualTo(inicialDeA16),
                $"o diagnóstico do A16 repete um quadro do {outro}.");
        }
    }

    [Test]
    public void NenhumQuadroDeA16_CitaOCenarioDeAbertura()
    {
        foreach (QuadroDeDisplayM4 quadro in TodasAsEtapas.SelectMany(etapa => etapa.Quadros))
        {
            Assert.That(quadro.Instrucao, Does.Not.Contain("abertura"),
                $"o quadro '{quadro.TextoLcd}' descreve o cenário do A15.");
            Assert.That(quadro.Instrucao, Does.Not.Contain("abriu"),
                $"o quadro '{quadro.TextoLcd}' descreve o cenário do A15.");
        }
    }

    #endregion

    #region MARK: Desligamento pelo menu de alertas

    [Test]
    public void AberturaDoDesligamento_DizOPercentualSobreOTempoDeFechamento()
    {
        QuadroDeDisplayM4 abertura = EtapaDesligarOAlerta.Primeiro;

        Assert.That(abertura.TextoLcd, Is.EqualTo(CodigoA16));
        Assert.That(abertura.Instrucao, Does.Contain("tempo de fechamento"));
        Assert.That(abertura.Instrucao, Does.Contain("20%, 30%, 40% ou 50%"));
        Assert.That(abertura.Instrucao, Does.Contain("aprendido na calibração"));
    }

    [Test]
    public void EtapaDesligarOAlerta_PercorreMenuConfigMenuAlertaEOMenuDoProprioAlerta()
    {
        Assert.That(EtapaDesligarOAlerta.Em(1).TextoLcd, Is.EqualTo(PerfisDeDisplayDeAlerta.Menu));
        Assert.That(EtapaDesligarOAlerta.Em(1).ProgressoSegundos, Is.EqualTo(6f));
        Assert.That(EtapaDesligarOAlerta.Em(2).TextoLcd, Is.EqualTo(PerfisDeDisplayDeAlerta.MenuConfig));
        Assert.That(EtapaDesligarOAlerta.Em(3).TextoLcd, Is.EqualTo(PerfisDeDisplayDeAlerta.MenuAlerta));
        Assert.That(EtapaDesligarOAlerta.Em(4).TextoLcd, Is.EqualTo(PerfisDeDisplayDeAlerta.MenuTempoFechamento));
        Assert.That(EtapaDesligarOAlerta.Em(5).TextoLcd, Is.EqualTo(PerfisDeDisplayDeAlerta.Desabilitar));
        Assert.That(EtapaDesligarOAlerta.Ultimo.TextoLcd, Is.EqualTo(PerfisDeDisplayDeAlerta.MenuTempoFechamento));
        Assert.That(EtapaDesligarOAlerta.Ultimo.Instrucao, Does.Contain("não avisa mais"));
    }

    #endregion

    #region MARK: Quadros do LCD

    [Test]
    public void CadaEtapa_TemAQuantidadeDeQuadrosPrevistaSemTextoVazio()
    {
        Assert.That(EtapaVerificarArComprimido.Quantidade, Is.EqualTo(QuantidadeDeQuadrosVerificarArComprimido));
        Assert.That(EtapaVerificarAvarias.Quantidade, Is.EqualTo(QuantidadeDeQuadrosVerificarAvarias));
        Assert.That(EtapaDesligarOAlerta.Quantidade, Is.EqualTo(QuantidadeDeQuadrosDesligarOAlerta));

        foreach (QuadroDeDisplayM4 quadro in TodasAsEtapas.SelectMany(etapa => etapa.Quadros))
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
        foreach (QuadroDeDisplayM4 quadro in TodasAsEtapas.SelectMany(etapa => etapa.Quadros))
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
            TodasAsEtapas.SelectMany(etapa => etapa.Quadros).Select(quadro => quadro.Instrucao),
            Has.None.EqualTo(AcaoVerificarArComprimido));
        Assert.That(
            TodasAsEtapas.SelectMany(etapa => etapa.Quadros).Select(quadro => quadro.Instrucao),
            Has.None.EqualTo(AcaoVerificarAvarias));
        Assert.That(
            TodasAsEtapas.SelectMany(etapa => etapa.Quadros).Select(quadro => quadro.Instrucao),
            Has.None.EqualTo(AcaoDesligarOAlerta));
    }

    #endregion

    #region MARK: LED e destaque das pecas

    [Test]
    public void EnquantoOAlertaEstaAtivo_OLedVermelhoPisca()
    {
        foreach (SequenciaDeQuadrosM4 etapa in TodasAsEtapas)
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
        Assert.That(EtapaVerificarArComprimido.Em(7).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueMangueiras));
        Assert.That(EtapaVerificarArComprimido.Em(9).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaquePneumatica));
    }

    [Test]
    public void OConjuntoEmCampo_AcendeOAtuadorEOCopo()
    {
        Assert.That(EtapaVerificarAvarias.Em(0).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueAtuadorCopo));
        Assert.That(EtapaVerificarAvarias.Em(1).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueAtuadorCopo));
        Assert.That(EtapaVerificarAvarias.Em(2).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueAtuadorCopo));
        Assert.That(EtapaVerificarAvarias.Em(4).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueMangueiras));
    }

    [Test]
    public void QuadrosDeMenu_NaoAcendemDestaqueNoModelo()
    {
        for (int i = 5; i < EtapaVerificarAvarias.Quantidade; i++)
        {
            Assert.That(EtapaVerificarAvarias.Em(i).Vfx, Is.Null, $"quadro {i}");
        }

        foreach (QuadroDeDisplayM4 quadro in EtapaDesligarOAlerta.Quadros)
        {
            Assert.That(quadro.Vfx, Is.Null, $"o quadro '{quadro.TextoLcd}' é de menu.");
        }
    }

    #endregion

    #region MARK: Mecanismo sem evidencia normativa

    [Test]
    public void PerfilDeA16_NaoAfirmaMecanismoDeAtivacaoConfirmado()
    {
        Assert.That(perfil.MecanismoDeAtivacaoConfirmado, Is.False);
    }

    #endregion

    #region MARK: Animacao do botao citado em cada passo

    [Test]
    public void AnimacoesDeA16_SeguemOsBotoesCitadosEmCadaPasso()
    {
        Assert.That(
            EtapaVerificarArComprimido.Quadros.Select(quadro => quadro.Animacao),
            Is.EqualTo(new string[QuantidadeDeQuadrosVerificarArComprimido]));

        Assert.That(
            EtapaVerificarAvarias.Quadros.Select(quadro => quadro.Animacao),
            Is.EqualTo(new[]
            {
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                AnimacaoDeBotaoM4.B3,
                AnimacaoDeBotaoM4.B2,
                AnimacaoDeBotaoM4.B1,
                null,
            }));

        Assert.That(
            EtapaDesligarOAlerta.Quadros.Select(quadro => quadro.Animacao),
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
    public void EtapasDeA16_ExpandemAsTresAcoesOficiaisEmPassosPraticos()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a16, EtapasGuiadasDeAlerta.Criar(a16));

        Assert.That(etapas, Has.Length.EqualTo(
            QuantidadeDeQuadrosVerificarArComprimido
            + QuantidadeDeQuadrosVerificarAvarias
            + QuantidadeDeQuadrosDesligarOAlerta));
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.tutorial)), Is.True);
        Assert.That(etapas.All(etapa => !string.IsNullOrWhiteSpace(etapa.textoDisplay)), Is.True);
        Assert.That(
            etapas[QuantidadeDeQuadrosVerificarArComprimido + 7].progressoSegundos,
            Is.EqualTo(6f));
        Assert.That(
            etapas[QuantidadeDeQuadrosVerificarArComprimido + QuantidadeDeQuadrosVerificarAvarias + 1]
                .progressoSegundos,
            Is.EqualTo(6f));
    }

    #endregion

    #region MARK: Modelo visual obrigatorio

    private static readonly Type TipoModeloDeAlertaDisplay =
        Type.GetType("ModeloDeAlertaDisplay, Assembly-CSharp");

    private static GameObject ResolverModelo(string codigo)
    {
        return TipoModeloDeAlertaDisplay
            .GetMethod("Resolver")
            .Invoke(null, new object[] { codigo }) as GameObject;
    }

    [Test]
    public void ModeloDeA16_ResolveOPrefabBaseadoEmM4SmartTeste()
    {
        Assert.That(TipoModeloDeAlertaDisplay, Is.Not.Null);

        GameObject prefab = ResolverModelo(CodigoA16);

        Assert.That(prefab, Is.Not.Null);
        Assert.That(AssetDatabase.GetAssetPath(prefab), Is.EqualTo(PrefabPath));
        Assert.That(AssetDatabase.GetDependencies(PrefabPath), Does.Contain(ModelPath));
        Assert.That(prefab.GetComponentsInChildren<Transform>(true)
            .Any(item => item.name == "DisplayDynamic"), Is.True);
        Assert.That(prefab.GetComponentInChildren<ControladorLedsM4>(true), Is.Not.Null);
    }

    #endregion
}
