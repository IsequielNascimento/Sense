using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class RegistroDeVfxTests
{
    #region MARK - Contrato

    private const string CaminhoDoPrefab = "Assets/Prefab/M4 Prefabs/Animação APK.prefab";
    private const string NomeDoComponente = "GerenciadorVisual";

    private static readonly string[] ErrosOrtograficosConhecidos =
    {
        "multimentro_medindo",
        "animacao_valvula_ar"
    };

    private const int TotalDeEfeitosSemObjeto = 16;

    #endregion

    #region MARK - Integridade dos nomes

    [Test]
    public void RegistroNaoPossuiNomesDuplicados()
    {
        List<string> nomes = NomesDoRegistro();

        var duplicados = nomes
            .GroupBy(nome => nome, StringComparer.OrdinalIgnoreCase)
            .Where(grupo => grupo.Count() > 1)
            .Select(grupo => grupo.Key)
            .ToList();

        Assert.That(duplicados, Is.Empty, $"Nomes duplicados: {string.Join(", ", duplicados)}");
    }

    [TestCaseSource(nameof(ErrosOrtograficosConhecidos))]
    public void ErroOrtograficoConhecidoNaoReaparece(string nomeIncorreto)
    {
        Assert.That(NomesDoRegistro(), Does.Not.Contain(nomeIncorreto),
            $"O nome '{nomeIncorreto}' voltou ao registro.");
    }

    [Test]
    public void RegistroNaoPossuiEntradaSemNome()
    {
        Assert.That(NomesDoRegistro().Any(string.IsNullOrWhiteSpace), Is.False);
    }

    #endregion

    #region MARK - Objetos ligados

    [Test]
    public void QuantidadeDeEfeitosSemObjeto_NaoAumentou()
    {
        int semObjeto = ContarEfeitosSemObjeto();

        Assert.That(semObjeto, Is.LessThanOrEqualTo(TotalDeEfeitosSemObjeto),
            "Um novo VFX foi cadastrado sem objeto.");

        if (semObjeto < TotalDeEfeitosSemObjeto)
        {
            Assert.Fail(
                $"{TotalDeEfeitosSemObjeto - semObjeto} VFX foram ligados. " +
                $"Atualize TotalDeEfeitosSemObjeto para {semObjeto} e trate a lacuna restante.");
        }
    }

    #endregion

    #region MARK - Leitura do prefab

    private static SerializedProperty EfeitosDisponiveis()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoDoPrefab);
        Assert.That(prefab, Is.Not.Null, $"Prefab nao encontrado: {CaminhoDoPrefab}");

        Component alvo = prefab
            .GetComponentsInChildren<Component>(true)
            .FirstOrDefault(componente =>
                componente != null && componente.GetType().Name == NomeDoComponente);

        Assert.That(alvo, Is.Not.Null, $"Componente {NomeDoComponente} nao encontrado no prefab.");

        var propriedade = new SerializedObject(alvo).FindProperty("efeitosDisponiveis");
        Assert.That(propriedade, Is.Not.Null, "Campo 'efeitosDisponiveis' nao encontrado.");

        return propriedade;
    }

    private static List<string> NomesDoRegistro()
    {
        SerializedProperty lista = EfeitosDisponiveis();
        var nomes = new List<string>(lista.arraySize);

        for (int i = 0; i < lista.arraySize; i++)
        {
            nomes.Add(lista.GetArrayElementAtIndex(i).FindPropertyRelative("Nome").stringValue);
        }

        return nomes;
    }

    private static int ContarEfeitosSemObjeto()
    {
        SerializedProperty lista = EfeitosDisponiveis();
        int semObjeto = 0;

        for (int i = 0; i < lista.arraySize; i++)
        {
            SerializedProperty objeto = lista
                .GetArrayElementAtIndex(i)
                .FindPropertyRelative("VfxObject");

            if (objeto.objectReferenceValue == null) semObjeto++;
        }

        return semObjeto;
    }

    #endregion
}
