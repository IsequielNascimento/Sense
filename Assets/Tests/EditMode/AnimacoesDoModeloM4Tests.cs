using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AnimacoesDoModeloM4Tests
{
    #region MARK - Recursos compartilhados pelos alertas de display

    private const string CaminhoFbx = "Assets/Prefab/Teste/M4SMARTTeste.fbx";
    private const string CaminhoController = "Assets/Resources/M4Problem1/M4SMARTTesteProblema1.controller";
    private const string CaminhoControllerA1 = "Assets/Resources/M4ProblemA1/M4SMARTTesteProblemaA1.controller";
    private const string CamadaAlvo = "Base Layer";
    private const string PastaDeClips = "Assets/Animation/Clips";
    private const string PropriedadeDeEscala = "m_LocalScale";

    private static string[] Botoes => AnimacaoDeBotaoM4.Todas;

    private static string[] Expostos =>
        Botoes.Concat(new[] { PerfisDeDisplayDeAlerta.AnimacaoProblema1 }).ToArray();

    private static AnimatorState Estado(string caminhoDoController, string camadaAlvo, string nome)
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(caminhoDoController);
        Assert.That(controller, Is.Not.Null, caminhoDoController);

        AnimatorControllerLayer camada = controller.layers.Single(layer => layer.name == camadaAlvo);

        return camada.stateMachine.states
            .Select(filho => filho.state)
            .FirstOrDefault(estado => estado.name == nome);
    }

    private static void AssertClipeSaneado(AnimatorState estado, string nome, string contexto)
    {
        Assert.That(estado, Is.Not.Null, $"{contexto}: estado '{nome}' ausente.");
        Assert.That(estado.motion, Is.Not.Null, $"{contexto}: estado '{nome}' sem clipe.");
        Assert.That(
            AssetDatabase.GetAssetPath(estado.motion),
            Is.EqualTo($"{PastaDeClips}/{nome}.anim"),
            $"{contexto}: '{nome}' precisa usar o clipe saneado, não o sub-asset do FBX.");

        bool temEscala = AnimationUtility.GetCurveBindings((AnimationClip)estado.motion)
            .Any(binding => binding.propertyName.StartsWith(PropriedadeDeEscala));

        Assert.That(temEscala, Is.False, $"{contexto}: '{nome}' não pode animar {PropriedadeDeEscala}.");
    }

    #endregion

    #region MARK - Takes expostos no import do FBX

    [Test]
    public void FbxExpoeUmClipPorAnimacaoUsada()
    {
        var importador = AssetImporter.GetAtPath(CaminhoFbx) as ModelImporter;
        Assert.That(importador, Is.Not.Null);

        string[] takes = importador.defaultClipAnimations.Select(clip => clip.takeName).ToArray();
        string[] gerados = importador.clipAnimations.Select(clip => clip.name).ToArray();

        foreach (string nome in Expostos)
        {
            Assert.That(takes, Does.Contain($"CHAVE_S|{nome.ToUpperInvariant()}"), nome);
            Assert.That(gerados, Does.Contain(nome), $"'{nome}' não está na lista Clips do importer.");
        }
    }

    #endregion

    #region MARK - Botoes na Base Layer do controller compartilhado

    [Test]
    public void ControllerCompartilhado_TemUmEstadoPorBotaoComOClipeSaneado()
    {
        foreach (string botao in Botoes)
        {
            AssertClipeSaneado(Estado(CaminhoController, CamadaAlvo, botao), botao, "compartilhado");
        }
    }

    #endregion

    #region MARK - PROBLEMA1 nao pode cair no sub-asset reciclado do FBX

    [Test]
    public void EstadoProblema1_UsaOClipeSaneadoNosDoisControllers()
    {
        string nome = PerfisDeDisplayDeAlerta.AnimacaoProblema1;
        string camada = PerfisDeDisplayDeAlerta.LayerProblema1;

        AssertClipeSaneado(Estado(CaminhoController, camada, nome), nome, "compartilhado");
        AssertClipeSaneado(Estado(CaminhoControllerA1, camada, nome), nome, "A1");
    }

    #endregion

    #region MARK - Estado neutro para onde o modelo volta

    [Test]
    public void BaseLayer_TemOEstadoDeRepousoComoPadraoESemClipe()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CaminhoController);
        AnimatorControllerLayer camada = controller.layers.Single(layer => layer.name == CamadaAlvo);

        AnimatorState padrao = camada.stateMachine.defaultState;

        Assert.That(padrao, Is.Not.Null);
        Assert.That(padrao.name, Is.EqualTo(DecisaoDeEtapaAr.EstadoDeRepouso));
        Assert.That(padrao.motion, Is.Null);
    }

    [Test]
    public void EstadoDeRepousoDoA1_SegueOMesmoContrato()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CaminhoControllerA1);
        AnimatorControllerLayer camada = controller.layers.Single(layer => layer.name == CamadaAlvo);

        Assert.That(camada.stateMachine.defaultState.name, Is.EqualTo(DecisaoDeEtapaAr.EstadoDeRepouso));
        Assert.That(camada.stateMachine.defaultState.motion, Is.Null);
    }

    #endregion

    #region MARK - O A1 usa camada propria e nao herda os botoes

    [Test]
    public void ControllerDoA1_NaoRecebeOsEstadosDeBotao()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CaminhoControllerA1);
        Assert.That(controller, Is.Not.Null);

        string[] nomes = controller.layers
            .SelectMany(layer => layer.stateMachine.states)
            .Select(filho => filho.state.name)
            .ToArray();

        foreach (string botao in Botoes)
        {
            Assert.That(nomes, Does.Not.Contain(botao));
        }
    }

    #endregion
}
