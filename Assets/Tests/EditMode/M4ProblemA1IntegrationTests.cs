using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class M4ProblemA1IntegrationTests
{
    #region MARK - Recursos do problema A1

    private const string ModelPath = "Assets/Prefab/Teste/M4SMARTTeste.fbx";
    private const string PrefabPathA1 = "Assets/Resources/M4ProblemA1/M4SMARTTesteProblemaA1.prefab";
    private const string LegacyFbxPath = "Assets/Prefab/M4 Prefabs/M4_Smart_Final.fbx";
    private const string LegacyAnimadoPath = "Assets/Prefab/M4 Prefabs/M4_Smart_Final_Animado.prefab";
    private const string OutlineShaderPath = "Assets/Shaders/DestaqueOutline.shader";
    private const string OutlineMaterialPath = "Assets/Materials/Destaque Outline.mat";

    [Test]
    public void PrefabDeA1DependeDiretamenteDoFbxCanonicoENaoDoModeloLegado()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPathA1);
        Assert.That(prefab, Is.Not.Null);

        string[] dependencias = AssetDatabase.GetDependencies(PrefabPathA1, true);
        Assert.That(dependencias, Does.Contain(ModelPath));
        Assert.That(dependencias, Does.Not.Contain(LegacyFbxPath));
        Assert.That(dependencias, Does.Not.Contain(LegacyAnimadoPath));
    }

    [Test]
    public void PrefabDeA1TemAnimatorComEstadoProblema1NaLayerDedicada()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPathA1);
        Animator animator = prefab.GetComponentInChildren<Animator>(true);

        Assert.That(animator, Is.Not.Null);
        Assert.That(animator.runtimeAnimatorController, Is.Not.Null);

        var controller = animator.runtimeAnimatorController as AnimatorController;
        Assert.That(controller, Is.Not.Null);
        Assert.That(controller.layers.Select(layer => layer.name), Does.Contain("Problema 1"));

        AnimatorControllerLayer layerProblema1 = controller.layers.Single(layer => layer.name == "Problema 1");
        Assert.That(layerProblema1.stateMachine.states.Select(s => s.state.name), Does.Contain("PROBLEMA1"));

        AnimatorControllerLayer baseLayer = controller.layers.Single(layer => layer.name == "Base Layer");
        Assert.That(baseLayer.stateMachine.states.Select(s => s.state.name), Does.Not.Contain("B2Button"));
        Assert.That(baseLayer.stateMachine.defaultState.motion, Is.Null);
    }

    [Test]
    public void PrefabDeA1ContemOutlinesDeAtuadorECopoDesativadosPorPadrao()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPathA1);

        Transform atuador = prefab.GetComponentsInChildren<Transform>(true).Single(t => t.name == "ATUADOR");
        Transform copo = prefab.GetComponentsInChildren<Transform>(true).Single(t => t.name == "COPO");

        Transform outlineAtuador = atuador.Find("ATUADOR_Outline");
        Transform outlineCopo = copo.Find("COPO_Outline");

        Assert.That(outlineAtuador, Is.Not.Null);
        Assert.That(outlineAtuador.gameObject.activeSelf, Is.False);
        Assert.That(outlineAtuador.GetComponent<MeshFilter>().sharedMesh,
            Is.EqualTo(atuador.GetComponent<MeshFilter>().sharedMesh));

        Assert.That(outlineCopo, Is.Not.Null);
        Assert.That(outlineCopo.gameObject.activeSelf, Is.False);
        Assert.That(outlineCopo.GetComponent<MeshFilter>().sharedMesh,
            Is.EqualTo(copo.GetComponent<MeshFilter>().sharedMesh));
    }

    [Test]
    public void GerenciadorVisualDoA1_RegistraOsDoisOutlinesSobOMesmoNomeDeEfeito()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPathA1);
        Type tipoGerenciador = Type.GetType("GerenciadorVisual, Assembly-CSharp");
        Assert.That(tipoGerenciador, Is.Not.Null);

        Component gerenciador = prefab.GetComponentInChildren(tipoGerenciador, true);
        Assert.That(gerenciador, Is.Not.Null);

        var efeitos = (IEnumerable)tipoGerenciador.GetField("efeitosDisponiveis").GetValue(gerenciador);
        Assert.That(efeitos, Is.Not.Null);

        var nomes = new System.Collections.Generic.List<string>();
        var nomesDosObjetos = new System.Collections.Generic.List<string>();

        foreach (object efeito in efeitos)
        {
            Type tipoVfxSetup = efeito.GetType();
            nomes.Add((string)tipoVfxSetup.GetField("Nome").GetValue(efeito));
            var vfxObject = (GameObject)tipoVfxSetup.GetField("VfxObject").GetValue(efeito);
            nomesDosObjetos.Add(vfxObject != null ? vfxObject.name : null);
        }

        var doAtuadorCopo = nomes
            .Zip(nomesDosObjetos, (nome, objeto) => (Nome: nome, Objeto: objeto))
            .Where(item => item.Nome == "DestaqueAtuadorCopo")
            .Select(item => item.Objeto);

        Assert.That(doAtuadorCopo, Is.EquivalentTo(new[] { "ATUADOR_Outline", "COPO_Outline" }));
    }

    [Test]
    public void MaterialDeDestaque_UsaOShaderDeOutlineComCorConfigurada()
    {
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(OutlineShaderPath);
        Material material = AssetDatabase.LoadAssetAtPath<Material>(OutlineMaterialPath);

        Assert.That(shader, Is.Not.Null);
        Assert.That(material, Is.Not.Null);
        Assert.That(material.shader, Is.EqualTo(shader));
        Assert.That(material.HasColor("_Color"), Is.True);
    }

    [Test]
    public void ExperienciaDeAlertaOficialDoA1_UsaALayerProblema1()
    {
        Type tipoLocalizedDatabase = Type.GetType("LocalizedDatabase, Assembly-CSharp");
        Assert.That(tipoLocalizedDatabase, Is.Not.Null);

        MethodInfo metodo = tipoLocalizedDatabase.GetMethod(
            "LoadArExperienceParaAlertaOficial", BindingFlags.Public | BindingFlags.Static);
        object experiencia = metodo.Invoke(null, new object[] { "A1" });

        object sequence = experiencia.GetType().GetProperty("Sequence").GetValue(experiencia);
        string layer = (string)sequence.GetType().GetProperty("Layer").GetValue(sequence);
        Etapa[] etapas = (Etapa[])sequence.GetType().GetProperty("Etapas").GetValue(sequence);

        int quadrosDoPerfil = PerfisDeDisplayDeAlerta.Obter("A1")
            .EtapasOficiais.Sum(etapa => etapa.Quantidade);

        Assert.That(layer, Is.EqualTo("Problema 1"));
        Assert.That(etapas, Has.Length.EqualTo(quadrosDoPerfil));
        Assert.That(
            etapas.Select(etapa => etapa.animacao).Where(animacao => !string.IsNullOrEmpty(animacao)).Distinct(),
            Is.EquivalentTo(new[] { PerfisDeDisplayDeAlerta.AnimacaoProblema1 }));
        Assert.That(etapas.Select(etapa => etapa.vfx), Does.Contain("DestaqueAtuadorCopo"));
    }

    #endregion
}
