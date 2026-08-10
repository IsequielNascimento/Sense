using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public class M4Problem1IntegrationTests
{
    #region MARK - Recursos do problema 1

    private const string ModelPath = "Assets/Prefab/Teste/M4SMARTTeste.fbx";
    private const string PrefabPath = "Assets/Resources/M4Problem1/M4SMARTTesteProblema1.prefab";
    private const string ScenePath = "Assets/Scenes/AR_Cena_UIToolkit.unity";

    [Test]
    public void FbxForneceAnimacaoProblema1DoCopo()
    {
        AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(ModelPath)
            .OfType<AnimationClip>()
            .FirstOrDefault(asset => asset.name == "COPO|PROBLEMA1");

        Assert.That(clip, Is.Not.Null);
        Assert.That(AnimationUtility.GetCurveBindings(clip)
            .Any(binding => binding.path == "COPO"), Is.True);
    }

    [Test]
    public void PrefabUsaFbxDisplayDinamicoEEstadoProblema1()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.That(prefab, Is.Not.Null);
        Assert.That(AssetDatabase.GetDependencies(PrefabPath), Does.Contain(ModelPath));
        Assert.That(prefab.GetComponentsInChildren<MonoBehaviour>(true)
            .Any(component => component.GetType().Name == "GerenciadorVisual"), Is.True);
        Assert.That(prefab.GetComponentInChildren<ControladorLedsM4>(true), Is.Not.Null);

        Animator animator = prefab.GetComponent<Animator>();
        Assert.That(animator, Is.Not.Null);
        var controller = animator.runtimeAnimatorController as AnimatorController;
        Assert.That(controller, Is.Not.Null);
        AnimatorControllerLayer layer = controller.layers.Single(item => item.name == "Problema 1");
        AnimatorState state = layer.stateMachine.states.Single(item => item.state.name == "PROBLEMA1").state;
        Assert.That(state.motion.name, Is.EqualTo("COPO|PROBLEMA1"));
    }

    [Test]
    public void EtapasExibemCalibracaoEAlertaDeAnguloMinimo()
    {
        TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(
            "Assets/Resources/BancoDeDadosProblemas/estrutura/A1.json");
        EstruturaCenario estrutura = JsonUtility.FromJson<EstruturaCenario>(asset.text);

        Assert.That(estrutura.etapas[0].animacao, Is.EqualTo("PROBLEMA1"));
        Assert.That(estrutura.etapas[0].textoDisplay, Is.EqualTo("50.0%\nAUTO"));
        Assert.That(estrutura.etapas[0].progressoSegundos, Is.EqualTo(2.5f));
        Assert.That(estrutura.etapas[1].textoDisplay, Is.EqualTo("A1\nANG MIN"));
        Assert.That(estrutura.etapas[2].textoDisplay, Is.EqualTo("A1\nANG MIN"));
        Assert.That(estrutura.etapas[3].textoDisplay, Is.EqualTo("ABERTO"));
    }

    [Test]
    public void CenaSelecionaNovoPrefabSomenteParaA1()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        MonoBehaviour exibidor = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .First(component => component.GetType().Name == "PlaceOnPlane_Adaptado");
        var serializado = new SerializedObject(exibidor);
        SerializedProperty modelos = serializado.FindProperty("modelosPorCenario");

        Assert.That(scene.IsValid(), Is.True);
        Assert.That(modelos.arraySize, Is.EqualTo(1));
        Assert.That(modelos.GetArrayElementAtIndex(0).FindPropertyRelative("cenario").stringValue,
            Is.EqualTo("A1"));
        Assert.That(AssetDatabase.GetAssetPath(modelos.GetArrayElementAtIndex(0)
            .FindPropertyRelative("prefab").objectReferenceValue), Is.EqualTo(PrefabPath));
    }

    #endregion
}
