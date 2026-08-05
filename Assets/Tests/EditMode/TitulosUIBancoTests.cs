using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class TitulosUIBancoTests
{
    #region MARK - Compatibilidade com o JSON legado

    private const string CaminhoDoAsset = "Assets/Resources/banco_de_dados_pt.json";

    [System.Serializable]
    private class TitulosUIBancoEspelho
    {
        public TitulosUIEspelho titulos = new TitulosUIEspelho();
    }

    [System.Serializable]
    private class TitulosUIEspelho
    {
        public string titulo_cena;
        public string subtitulo;
        public string holder;
    }

    [Test]
    public void CarregaOsTresTextosGenericosDoMesmoJsonLegado()
    {
        TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(CaminhoDoAsset);
        Assert.That(asset, Is.Not.Null, $"Arquivo nao encontrado: {CaminhoDoAsset}");

        TitulosUIBancoEspelho banco = JsonUtility.FromJson<TitulosUIBancoEspelho>(asset.text);

        Assert.That(banco.titulos, Is.Not.Null);
        Assert.That(banco.titulos.titulo_cena, Is.EqualTo("Problemas"));
        Assert.That(banco.titulos.subtitulo, Is.EqualTo("Escolha a opção que corresponde ao seu problema"));
        Assert.That(banco.titulos.holder, Is.EqualTo("Pesquisar problema..."));
    }

    #endregion

    #region MARK - Aceite: GerenciarUI nao desserializa BancoProblemas

    [Test]
    public void GerenciarUI_NaoReferenciaBancoProblemas()
    {
        string caminho = Path.Combine(Application.dataPath, "Scripts", "GerenciarUI.cs");
        string codigo = File.ReadAllText(caminho);

        Assert.That(codigo, Does.Not.Contain("BancoProblemas"),
            "GerenciarUI nao deve mais desserializar BancoProblemas; use TitulosUIBanco.");
        Assert.That(codigo, Does.Contain("TitulosUIBanco"));
    }

    #endregion
}
