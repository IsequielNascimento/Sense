using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class M4ProblemA2IntegrationTests
{
    #region MARK - Recursos do problema A2

    private const string ModelPath = "Assets/Prefab/Teste/M4SMARTTeste.fbx";
    private const string PrefabPathA2 = "Assets/Resources/M4ProblemA2/M4SMARTTesteProblemaA2.prefab";
    private const string LegacyFbxPath = "Assets/Prefab/M4 Prefabs/M4_Smart_Final.fbx";
    private const string LegacyAnimadoPath = "Assets/Prefab/M4 Prefabs/M4_Smart_Final_Animado.prefab";

    [Test]
    public void PrefabDeA2DependeDiretamenteDoFbxCanonicoENaoDoModeloLegado()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPathA2);
        Assert.That(prefab, Is.Not.Null);

        string[] dependencias = AssetDatabase.GetDependencies(PrefabPathA2, true);
        Assert.That(dependencias, Does.Contain(ModelPath));
        Assert.That(dependencias, Does.Not.Contain(LegacyFbxPath));
        Assert.That(dependencias, Does.Not.Contain(LegacyAnimadoPath));
    }

    [Test]
    public void PrefabDeA2ContemOutlinesDeAtuadorECopoDesativadosPorPadrao()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPathA2);

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
    public void GerenciadorVisualDoA2_RegistraOsDoisOutlinesSobOMesmoNomeDeEfeito()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPathA2);
        Type tipoGerenciador = Type.GetType("GerenciadorVisual, Sense.Runtime");
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
    public void ExperienciaDeAlertaOficialDoA2_UsaABaseLayerPadrao()
    {
        Type tipoLocalizedDatabase = Type.GetType("LocalizedDatabase, Sense.Runtime");
        Assert.That(tipoLocalizedDatabase, Is.Not.Null);

        MethodInfo metodo = tipoLocalizedDatabase.GetMethod(
            "LoadArExperienceParaAlertaOficial", BindingFlags.Public | BindingFlags.Static);
        object experiencia = metodo.Invoke(null, new object[] { "A2" });

        object sequence = experiencia.GetType().GetProperty("Sequence").GetValue(experiencia);
        string layer = (string)sequence.GetType().GetProperty("Layer").GetValue(sequence);
        Etapa[] etapas = (Etapa[])sequence.GetType().GetProperty("Etapas").GetValue(sequence);

        int quadrosDoPerfil = PerfisDeDisplayDeAlerta.Obter("A2")
            .EtapasOficiais.Sum(etapa => etapa.Quantidade);

        Assert.That(layer, Is.EqualTo("Base Layer"));
        Assert.That(etapas, Has.Length.EqualTo(quadrosDoPerfil));
        Assert.That(etapas.Select(etapa => etapa.vfx), Does.Contain("DestaqueAtuadorCopo"));
    }

    #endregion
}
