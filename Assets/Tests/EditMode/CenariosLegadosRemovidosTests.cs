using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;

public class CenariosLegadosRemovidosTests
{
    #region MARK - Recursos removidos

    private const string PastaLegada = "Assets/Resources/BancoDeDadosProblemas";

    [Test]
    public void PastaDeCenariosLegados_NaoExiste()
    {
        Assert.That(AssetDatabase.IsValidFolder(PastaLegada), Is.False);
    }

    [TestCase("A1")]
    [TestCase("A2")]
    [TestCase("A3")]
    [TestCase("A4")]
    [TestCase("A5")]
    [TestCase("A6")]
    [TestCase("A7")]
    [TestCase("A8")]
    public void RecursoJsonLegado_NaoExiste(string codigo)
    {
        bool existe = AssetDatabase
            .FindAssets($"{codigo} t:TextAsset", new[] { "Assets/Resources" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Any(caminho => string.Equals(
                Path.GetFileName(caminho),
                $"{codigo}.json",
                StringComparison.OrdinalIgnoreCase));

        Assert.That(existe, Is.False, $"{codigo}.json voltou a ser adicionado aos Resources.");
    }

    #endregion
}
