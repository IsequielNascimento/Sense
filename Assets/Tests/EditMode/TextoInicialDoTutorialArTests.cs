using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class TextoInicialDoTutorialArTests
{
    #region MARK: Dado localizado

    [System.Serializable]
    private class TutorialEspelho
    {
        public string tutorialInicial;
    }

    [TestCase("pt")]
    [TestCase("en")]
    [TestCase("es")]
    [TestCase("fr")]
    public void TodoIdioma_TrazOTutorialInicialDaMontagem(string idioma)
    {
        string caminho = $"Assets/Resources/BancoDeDadosMontagem/Montagem/banco_montagem_{idioma}.json";
        TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(caminho);

        Assert.That(asset, Is.Not.Null, $"Arquivo nao encontrado: {caminho}");

        TutorialEspelho dados = JsonUtility.FromJson<TutorialEspelho>(asset.text);

        Assert.That(dados.tutorialInicial, Is.Not.Empty, $"{idioma}: tutorialInicial ausente.");
    }

    #endregion

    #region MARK: Fiacao no UIController

    [Test]
    public void UIController_AlimentaOTextoTutorialComODadoLocalizado()
    {
        string caminho = Path.Combine(Application.dataPath, "Scripts", "UIController.cs");
        string codigo = File.ReadAllText(caminho);

        Assert.That(codigo, Does.Contain("SetText(textoTutorial, text.tutorialInicial)"),
            "O UIController deve alimentar o rotulo do tutorial com o texto localizado, nao com o literal do UXML.");
    }

    #endregion
}
