using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

public class M4Problem1IntegrationTests
{
    #region MARK - Recursos do problema 1

    private const string ModelPath = "Assets/Prefab/Teste/M4SMARTTeste.fbx";
    private const string PrefabPath = "Assets/Resources/M4Problem1/M4SMARTTesteProblema1.prefab";
    private const string DefaultPrefabPath = "Assets/Prefab/M4 Prefabs/Animação APK.prefab";
    private const string ScenePath = "Assets/Scenes/AR_Cena_UIToolkit.unity";
    private const string LedMaterialPath = "Assets/Materials/M4 LEDs Bloom.mat";
    private const string BloomProfilePath = "Assets/Settings/M4DisplayBloomProfile.asset";

    [Test]
    public void FbxForneceAnimacaoProblema1DoCopo()
    {
        AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(ModelPath)
            .OfType<AnimationClip>()
            .FirstOrDefault(asset => asset.name == PerfisDeDisplayDeAlerta.AnimacaoProblema1);

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
        Assert.That(state.motion.name, Is.EqualTo(PerfisDeDisplayDeAlerta.AnimacaoProblema1));
    }

    [Test]
    public void PrefabUsaMaterialEmissivoNoRendererDeLeds()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Renderer rendererLeds = prefab.GetComponentsInChildren<Renderer>(true)
            .Single(renderer => renderer.name == "LEDS");

        Assert.That(AssetDatabase.GetAssetPath(rendererLeds.sharedMaterial), Is.EqualTo(LedMaterialPath));
    }

    [Test]
    public void CenaArHabilitaPosProcessamentoEBloomDosLeds()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Camera camera = Object.FindFirstObjectByType<Camera>();
        UniversalAdditionalCameraData dadosDaCamera = camera.GetComponent<UniversalAdditionalCameraData>();
        Volume volume = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None)
            .Single(item => item.name == "M4 Bloom");

        Assert.That(dadosDaCamera.renderPostProcessing, Is.True);
        Assert.That(volume.isGlobal, Is.True);
        Assert.That(AssetDatabase.GetAssetPath(volume.sharedProfile), Is.EqualTo(BloomProfilePath));
        Assert.That(volume.sharedProfile.TryGet(out Bloom bloom), Is.True);
        Assert.That(bloom.active, Is.True);
    }

    [Test]
    public void CenaMantemMapeamentoSerializadoSomenteParaA1()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        MonoBehaviour exibidor = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .First(component => component.GetType().Name == "PlaceOnPlane_Adaptado");
        var serializado = new SerializedObject(exibidor);
        SerializedProperty modelos = serializado.FindProperty("modelosPorCenario");

        Assert.That(scene.IsValid(), Is.True);
        Assert.That(AssetDatabase.GetAssetPath(serializado.FindProperty("placedPrefab").objectReferenceValue),
            Is.EqualTo(DefaultPrefabPath));
        Assert.That(scene.GetRootGameObjects().Any(root => root.name == "Animação APK"), Is.False);
        Assert.That(modelos.arraySize, Is.EqualTo(1));
        Assert.That(modelos.GetArrayElementAtIndex(0).FindPropertyRelative("cenario").stringValue,
            Is.EqualTo("A1"));
        Assert.That(AssetDatabase.GetAssetPath(modelos.GetArrayElementAtIndex(0)
            .FindPropertyRelative("prefab").objectReferenceValue), Is.EqualTo(PrefabPath));
    }

    [Test]
    public void SelecaoA1ResolvePrefabNovoEmRuntime()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        MonoBehaviour exibidor = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .First(component => component.GetType().Name == "PlaceOnPlane_Adaptado");
        Type tipoSelecao = Type.GetType("ProblemaSelecionadoAR, Assembly-CSharp");
        var objetoSelecao = new GameObject("Selecao A1 Teste");
        Component selecao = objetoSelecao.AddComponent(tipoSelecao);
        PropertyInfo instancia = tipoSelecao.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        instancia.SetValue(null, selecao);

        tipoSelecao.GetMethod("Selecionar").Invoke(selecao, new object[] { "A1", "A1" });
        var prefabResolvido = exibidor.GetType().BaseType
            .GetProperty("PrefabDoModelo")
            .GetValue(exibidor) as GameObject;

        Assert.That(AssetDatabase.GetAssetPath(prefabResolvido), Is.EqualTo(PrefabPath));
        instancia.SetValue(null, null);
        Object.DestroyImmediate(objetoSelecao);
    }

    [Test]
    public void SelecaoA8ResolveM4SmartTesteComDisplayELeds()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        MonoBehaviour exibidor = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .First(component => component.GetType().Name == "PlaceOnPlane_Adaptado");
        Type tipoSelecao = Type.GetType("ProblemaSelecionadoAR, Assembly-CSharp");
        var objetoSelecao = new GameObject("Selecao A8 Teste");
        Component selecao = objetoSelecao.AddComponent(tipoSelecao);
        PropertyInfo instancia = tipoSelecao.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);

        try
        {
            instancia.SetValue(null, selecao);
            tipoSelecao.GetMethod("SelecionarAlertaOficial").Invoke(selecao, new object[] { "A8" });
            var prefabResolvido = exibidor.GetType().BaseType
                .GetProperty("PrefabDoModelo")
                .GetValue(exibidor) as GameObject;

            Assert.That(AssetDatabase.GetAssetPath(prefabResolvido), Is.EqualTo(PrefabPath));
            Assert.That(AssetDatabase.GetDependencies(PrefabPath), Does.Contain(ModelPath));
            Assert.That(prefabResolvido.GetComponentsInChildren<Transform>(true)
                .Any(item => item.name == "DISPLAY"), Is.True);
            Assert.That(prefabResolvido.GetComponentsInChildren<Transform>(true)
                .Any(item => item.name == "DisplayDynamic"), Is.True);
            Assert.That(prefabResolvido.GetComponentInChildren<ControladorLedsM4>(true), Is.Not.Null);
        }
        finally
        {
            instancia.SetValue(null, null);
            Object.DestroyImmediate(objetoSelecao);
        }
    }

    [Test]
    public void ModeloDeA8DesabilitaAnimacaoMecanicaLegada()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        MonoBehaviour exibidor = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .First(component => component.GetType().Name == "PlaceOnPlane_Adaptado");
        Type tipoSelecao = Type.GetType("ProblemaSelecionadoAR, Assembly-CSharp");
        var objetoSelecao = new GameObject("Selecao A8 Teste");
        Component selecao = objetoSelecao.AddComponent(tipoSelecao);
        PropertyInfo instancia = tipoSelecao.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        GameObject modeloInstanciado = null;

        try
        {
            instancia.SetValue(null, selecao);
            tipoSelecao.GetMethod("SelecionarAlertaOficial").Invoke(selecao, new object[] { "A8" });

            Type tipoBase = exibidor.GetType().BaseType;
            var prefabResolvido = tipoBase.GetProperty("PrefabDoModelo").GetValue(exibidor) as GameObject;
            modeloInstanciado = PrefabUtility.InstantiatePrefab(prefabResolvido) as GameObject;
            tipoBase.GetField("spawnedObject", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(exibidor, modeloInstanciado);
            tipoBase.GetMethod("ConfigurarModeloInstanciado", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(exibidor, null);

            Animator[] animadores = modeloInstanciado.GetComponentsInChildren<Animator>(true);
            Assert.That(animadores, Is.Not.Empty);
            Assert.That(animadores.All(animator => !animator.enabled), Is.True);
        }
        finally
        {
            if (modeloInstanciado != null)
            {
                Object.DestroyImmediate(modeloInstanciado);
            }

            instancia.SetValue(null, null);
            Object.DestroyImmediate(objetoSelecao);
        }
    }

    #endregion
}
