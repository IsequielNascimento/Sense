using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public class M4Problem12IntegrationTests
{
    #region MARK - Recursos do problema 12

    private const string ModelPath = "Assets/Prefab/Teste/M4SMARTTeste.fbx";
    private const string PrefabPathA12 = "Assets/Resources/M4Problem12/M4SMARTTesteProblema12.prefab";
    private const string PrefabPathA8 = "Assets/Resources/M4Problem1/M4SMARTTesteProblema1.prefab";
    private const string PrefabPathA11 = "Assets/Resources/M4Problem11/M4SMARTTesteProblema11.prefab";
    private const string LegacyFbxPath = "Assets/Prefab/M4 Prefabs/M4_Smart_Final.fbx";
    private const string LegacyAnimadoPath = "Assets/Prefab/M4 Prefabs/M4_Smart_Final_Animado.prefab";
    private const string LedMaterialPath = "Assets/Materials/M4 LEDs Bloom.mat";
    private const string ScenePath = "Assets/Scenes/AR_Cena_UIToolkit.unity";

    [Test]
    public void PrefabDeA12DependeDiretamenteDoFbxCanonicoENaoDoModeloLegado()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPathA12);
        Assert.That(prefab, Is.Not.Null);

        string[] dependencias = AssetDatabase.GetDependencies(PrefabPathA12, true);
        Assert.That(dependencias, Does.Contain(ModelPath));
        Assert.That(dependencias, Does.Not.Contain(LegacyFbxPath));
        Assert.That(dependencias, Does.Not.Contain(LegacyAnimadoPath));
    }

    [Test]
    public void PrefabDeA12ContemDisplayDinamicoGerenciadorVisualEControladorDeLeds()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPathA12);

        Assert.That(prefab.GetComponentsInChildren<Transform>(true)
            .Any(item => item.name == "DisplayDynamic"), Is.True);
        Assert.That(prefab.GetComponentsInChildren<Transform>(true)
            .Any(item => item.name == "DISPLAY"), Is.True);
        Assert.That(prefab.GetComponentsInChildren<MonoBehaviour>(true)
            .Any(component => component.GetType().Name == "GerenciadorVisual"), Is.True);
        Assert.That(prefab.GetComponentInChildren<ControladorLedsM4>(true), Is.Not.Null);
    }

    [Test]
    public void PrefabDeA12UsaMaterialEmissivoNoRendererDeLeds()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPathA12);
        Renderer rendererLeds = prefab.GetComponentsInChildren<Renderer>(true)
            .Single(renderer => renderer.name == "LEDS");

        Assert.That(AssetDatabase.GetAssetPath(rendererLeds.sharedMaterial), Is.EqualTo(LedMaterialPath));
    }

    [Test]
    public void SelecaoA12ResolveM4SmartTesteComDisplayELedsNoFluxoAr()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        MonoBehaviour exibidor = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .First(component => component.GetType().Name == "PlaceOnPlane_Adaptado");
        Type tipoSelecao = Type.GetType("ProblemaSelecionadoAR, Sense.Runtime");
        var objetoSelecao = new GameObject("Selecao A12 Teste AR");
        Component selecao = objetoSelecao.AddComponent(tipoSelecao);
        PropertyInfo instancia = tipoSelecao.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);

        try
        {
            instancia.SetValue(null, selecao);
            tipoSelecao.GetMethod("SelecionarAlertaOficial").Invoke(selecao, new object[] { "A12" });
            var prefabResolvido = exibidor.GetType().BaseType
                .GetProperty("PrefabDoModelo")
                .GetValue(exibidor) as GameObject;

            Assert.That(AssetDatabase.GetAssetPath(prefabResolvido), Is.EqualTo(PrefabPathA12));
            Assert.That(AssetDatabase.GetAssetPath(prefabResolvido), Is.Not.EqualTo(PrefabPathA8));
            Assert.That(AssetDatabase.GetAssetPath(prefabResolvido), Is.Not.EqualTo(PrefabPathA11));
            Assert.That(AssetDatabase.GetDependencies(PrefabPathA12), Does.Contain(ModelPath));
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
    public void SelecaoA12ResolveOMesmoPrefabNoFluxoArENoVisualizador3D()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        MonoBehaviour exibidorAr = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .First(component => component.GetType().Name == "PlaceOnPlane_Adaptado");
        Type tipoSelecao = Type.GetType("ProblemaSelecionadoAR, Sense.Runtime");
        Type tipoVisualizador = Type.GetType("Visualizador3D, Sense.Runtime");
        var objetoSelecao = new GameObject("Selecao A12 Teste Comparativo");
        Component selecao = objetoSelecao.AddComponent(tipoSelecao);
        PropertyInfo instancia = tipoSelecao.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        GameObject objetoVisualizador = null;

        try
        {
            instancia.SetValue(null, selecao);
            tipoSelecao.GetMethod("SelecionarAlertaOficial").Invoke(selecao, new object[] { "A12" });

            var prefabResolvidoAr = exibidorAr.GetType().BaseType
                .GetProperty("PrefabDoModelo")
                .GetValue(exibidorAr) as GameObject;

            objetoVisualizador = new GameObject("Visualizador3D Teste A12");
            Component visualizador = objetoVisualizador.AddComponent(tipoVisualizador);
            var prefabResolvidoVisualizador = visualizador.GetType().BaseType
                .GetProperty("PrefabDoModelo")
                .GetValue(visualizador) as GameObject;

            Assert.That(AssetDatabase.GetAssetPath(prefabResolvidoAr), Is.EqualTo(PrefabPathA12));
            Assert.That(AssetDatabase.GetAssetPath(prefabResolvidoVisualizador), Is.EqualTo(PrefabPathA12));
            Assert.That(prefabResolvidoVisualizador, Is.SameAs(prefabResolvidoAr));
        }
        finally
        {
            instancia.SetValue(null, null);
            Object.DestroyImmediate(objetoSelecao);
            if (objetoVisualizador != null) Object.DestroyImmediate(objetoVisualizador);
        }
    }

    [Test]
    public void ModeloDeA12DesabilitaAnimacaoMecanicaLegada()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        MonoBehaviour exibidor = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .First(component => component.GetType().Name == "PlaceOnPlane_Adaptado");
        Type tipoSelecao = Type.GetType("ProblemaSelecionadoAR, Sense.Runtime");
        var objetoSelecao = new GameObject("Selecao A12 Teste Animacao");
        Component selecao = objetoSelecao.AddComponent(tipoSelecao);
        PropertyInfo instancia = tipoSelecao.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        GameObject modeloInstanciado = null;

        try
        {
            instancia.SetValue(null, selecao);
            tipoSelecao.GetMethod("SelecionarAlertaOficial").Invoke(selecao, new object[] { "A12" });

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
