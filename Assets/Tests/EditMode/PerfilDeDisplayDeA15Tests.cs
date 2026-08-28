using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class PerfilDeDisplayDeA15Tests
{
    #region MARK: Fixture

    private const string CodigoA15 = "A15";
    private const string CodigoA16 = "A16";
    private const string AcaoVerificarArComprimido = "Verificar o fornecimento de ar comprimido";
    private const string AcaoVerificarAvarias = "Verificar possíveis avarias no conjunto válvula / atuador";
    private const string AcaoDesligarOAlerta = "Desligar o alerta";
    private const int QuantidadeDeQuadrosVerificarArComprimido = 11;
    private const int QuantidadeDeQuadrosVerificarAvarias = 11;
    private const int QuantidadeDeQuadrosDesligarOAlerta = 7;
    private const int LimiteDeCaracteresDaInstrucao = 120;
    private const string ModelPath = "Assets/Prefab/Teste/M4SMARTTeste.fbx";
    private const string PrefabPath = "Assets/Resources/M4ProblemA15/M4SMARTTesteProblemaA15.prefab";

    private AlertaOficial a15;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a15 = CatalogoDeAlertas.Obter(CodigoA15, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA15);
    }

    private SequenciaDeQuadrosM4 EtapaVerificarArComprimido => perfil.EtapaOficial(0);
    private SequenciaDeQuadrosM4 EtapaVerificarAvarias => perfil.EtapaOficial(1);
    private SequenciaDeQuadrosM4 EtapaDesligarOAlerta => perfil.EtapaOficial(2);

    private SequenciaDeQuadrosM4[] TodasAsEtapas =>
        new[] { EtapaVerificarArComprimido, EtapaVerificarAvarias, EtapaDesligarOAlerta };

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA15_MantemAsTresAcoesDaPagina76()
    {
        Assert.That(a15, Is.Not.Null);
        Assert.That(a15.Nome, Is.EqualTo("TEMPO ABERTURA"));
        Assert.That(a15.Padrao, Is.EqualTo("desabilitado"));
        Assert.That(a15.OQueE, Is.EqualTo(
            "monitor aprende o tempo de abertura da válvula durante o processo de calibração"));
        Assert.That(a15.QuandoOcorre, Is.EqualTo("quando o tempo de abertura é ultrapassado"));
        Assert.That(a15.Acoes.Count, Is.EqualTo(3));
        Assert.That(a15.Acoes[0], Is.EqualTo(AcaoVerificarArComprimido));
        Assert.That(a15.Acoes[1], Is.EqualTo(AcaoVerificarAvarias));
        Assert.That(a15.Acoes[2], Is.EqualTo(AcaoDesligarOAlerta));
        Assert.That(a15.Locais.Count, Is.EqualTo(3));
        Assert.That(a15.Locais[0], Is.EqualTo("gerador de ar comprimido"));
        Assert.That(a15.Locais[1], Is.EqualTo("em campo"));
        Assert.That(a15.Locais[2], Is.EqualTo("menu tempo abertura"));
    }

    [Test]
    public void PerfilDeA15_TemExatamenteTresEtapasOficiais()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA15));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(3));
        Assert.That(perfil.CorrespondeAoCatalogo(a15), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A15NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A10"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA15));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA16));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(24));
    }

    #endregion

    #region MARK: O tempo aprendido na calibracao e a referencia do alerta

    [Test]
    public void EstadoInicial_DizQueOCursoTerminouForaDoTempoAprendido()
    {
        QuadroDeDisplayM4 estadoInicial = EtapaVerificarArComprimido.Primeiro;

        Assert.That(estadoInicial.TextoLcd, Is.EqualTo(CodigoA15));
        Assert.That(estadoInicial.Instrucao, Does.StartWith("Confirme o alerta A15"));
        Assert.That(estadoInicial.Instrucao, Does.Contain("abriu por completo"));
        Assert.That(estadoInicial.Instrucao, Does.Contain("aprendido na calibração"));
    }

    [Test]
    public void OPercentual_EMedidoSobreOTempoAprendidoENaoSobreUmValorAbsoluto()
    {
        Assert.That(EtapaVerificarArComprimido.Em(1).Instrucao, Does.Contain("não tem tempo fixo"));
        Assert.That(EtapaVerificarArComprimido.Em(1).Instrucao, Does.Contain("auto calibração"));
        Assert.That(EtapaVerificarArComprimido.Em(2).Instrucao, Does.Contain("20%, 30%, 40% ou 50%"));
        Assert.That(EtapaVerificarArComprimido.Em(3).Instrucao, Does.Contain("defasada"));
    }

    [Test]
    public void FaixaDeTrabalhoELimiteAbsoluto_SaemDoManualSemValorInventado()
    {
        Assert.That(EtapaVerificarArComprimido.Em(5).Instrucao, Does.Contain("3 e 8 bar (45 a 120 psi)"));
        Assert.That(EtapaVerificarArComprimido.Em(5).Instrucao, Does.Contain("6 bar (87 psi)"));
        Assert.That(EtapaVerificarArComprimido.Em(6).Instrucao, Does.Contain("10 bar (150 psi)"));
    }

    [Test]
    public void ARecalibracao_ReaprendeOTempoPelaAutoCalibracaoDoManual()
    {
        Assert.That(EtapaVerificarAvarias.Em(5).Instrucao, Does.Contain("reaprender o tempo de curso"));
        Assert.That(EtapaVerificarAvarias.Em(6).Instrucao, Does.Contain("C5 AUTO CAL"));
        Assert.That(EtapaVerificarAvarias.Em(6).Instrucao, Does.Contain("3, 5 ou 10 ciclos"));
        Assert.That(EtapaVerificarAvarias.Em(7).TextoLcd, Is.EqualTo(PerfisDeDisplayDeAlerta.FastSetup));
        Assert.That(EtapaVerificarAvarias.Em(7).ProgressoSegundos, Is.EqualTo(6f));
        Assert.That(EtapaVerificarAvarias.Em(8).TextoLcd, Is.EqualTo(PerfisDeDisplayDeAlerta.Certo));
        Assert.That(EtapaVerificarAvarias.Em(9).TextoLcd, Is.EqualTo(PerfisDeDisplayDeAlerta.Abortar));
        Assert.That(EtapaVerificarAvarias.Em(9).ProgressoSegundos, Is.EqualTo(3f));
    }

    #endregion

    #region MARK: Fronteira contra os outros alertas de curso

    [Test]
    public void NenhumQuadroDeA15_InvadeODiagnosticoDeA1A2A3OuA5()
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
        string inicialDeA15 = EtapaVerificarArComprimido.Primeiro.Instrucao;

        foreach (string outro in new[] { "A1", "A2", "A3", "A5" })
        {
            Assert.That(
                PerfisDeDisplayDeAlerta.Obter(outro).EtapasOficiais
                    .SelectMany(etapa => etapa.Quadros)
                    .Select(quadro => quadro.Instrucao),
                Has.None.EqualTo(inicialDeA15),
                $"o diagnóstico do A15 repete um quadro do {outro}.");
        }
    }

    [Test]
    public void NenhumQuadroDeA15_CitaOCenarioDeFechamento()
    {
        foreach (QuadroDeDisplayM4 quadro in TodasAsEtapas.SelectMany(etapa => etapa.Quadros))
        {
            Assert.That(quadro.Instrucao, Does.Not.Contain("fecha"),
                $"o quadro '{quadro.TextoLcd}' descreve o cenário do A16.");
        }
    }

    #endregion

    #region MARK: Desligamento pelo menu de alertas

    [Test]
    public void AberturaDoDesligamento_DizOPercentualSobreOTempoDeAbertura()
    {
        QuadroDeDisplayM4 abertura = EtapaDesligarOAlerta.Primeiro;

        Assert.That(abertura.TextoLcd, Is.EqualTo(CodigoA15));
        Assert.That(abertura.Instrucao, Does.Contain("tempo de abertura"));
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
        Assert.That(EtapaDesligarOAlerta.Em(4).TextoLcd, Is.EqualTo(PerfisDeDisplayDeAlerta.MenuTempoAbertura));
        Assert.That(EtapaDesligarOAlerta.Em(5).TextoLcd, Is.EqualTo(PerfisDeDisplayDeAlerta.Desabilitar));
        Assert.That(EtapaDesligarOAlerta.Ultimo.TextoLcd, Is.EqualTo(PerfisDeDisplayDeAlerta.MenuTempoAbertura));
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
    public void PerfilDeA15_NaoAfirmaMecanismoDeAtivacaoConfirmado()
    {
        Assert.That(perfil.MecanismoDeAtivacaoConfirmado, Is.False);
    }

    #endregion

    #region MARK: Animacao do botao citado em cada passo

    [Test]
    public void AnimacoesDeA15_SeguemOsBotoesCitadosEmCadaPasso()
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
    public void EtapasDeA15_ExpandemAsTresAcoesOficiaisEmPassosPraticos()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a15, EtapasGuiadasDeAlerta.Criar(a15));

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
        Type.GetType("ModeloDeAlertaDisplay, Sense.Runtime");

    private static GameObject ResolverModelo(string codigo)
    {
        return TipoModeloDeAlertaDisplay
            .GetMethod("Resolver")
            .Invoke(null, new object[] { codigo }) as GameObject;
    }

    [Test]
    public void ModeloDeA15_ResolveOPrefabBaseadoEmM4SmartTeste()
    {
        Assert.That(TipoModeloDeAlertaDisplay, Is.Not.Null);

        GameObject prefab = ResolverModelo(CodigoA15);

        Assert.That(prefab, Is.Not.Null);
        Assert.That(AssetDatabase.GetAssetPath(prefab), Is.EqualTo(PrefabPath));
        Assert.That(AssetDatabase.GetDependencies(PrefabPath), Does.Contain(ModelPath));
        Assert.That(prefab.GetComponentsInChildren<Transform>(true)
            .Any(item => item.name == "DisplayDynamic"), Is.True);
        Assert.That(prefab.GetComponentInChildren<ControladorLedsM4>(true), Is.Not.Null);
    }

    #endregion
}
