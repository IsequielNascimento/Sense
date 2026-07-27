using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class EstruturaDeCenarioTests
{
    #region MARK - Contrato

    private const string PastaDeProblemas = "Assets/Resources/BancoDeDadosProblemas";

    private static readonly string[] Cenarios =
    {
        "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8"
    };

    private static readonly string[] Idiomas = { "pt", "en", "es", "fr" };

    [Serializable]
    private class EtapaJson
    {
        public string tutorial;
        public string animacao;
        public string telaDisplay;
        public string vfx;
        public string textoDisplay;
    }

    [Serializable]
    private class CenarioJson
    {
        public string layer;
        public EtapaJson[] etapas;
    }

    #endregion

    #region MARK - Existência e fidelidade

    [TestCaseSource(nameof(Cenarios))]
    public void TodoCenarioPossuiArquivoEstrutural(string cenario)
    {
        EstruturaCenario estrutura = CarregarEstrutura(cenario);

        Assert.That(estrutura, Is.Not.Null, $"Estrutura ausente para {cenario}.");
        Assert.That(estrutura.scenarioId, Is.EqualTo(cenario));
        Assert.That(estrutura.etapas, Is.Not.Empty);
    }

    [TestCaseSource(nameof(Cenarios))]
    public void EstruturaReproduzExatamenteOsDadosDoIdiomaBase(string cenario)
    {
        EstruturaCenario estrutura = CarregarEstrutura(cenario);
        CenarioJson original = CarregarTraducao("pt", cenario);

        Assert.That(estrutura.layer, Is.EqualTo(original.layer));
        Assert.That(estrutura.etapas.Length, Is.EqualTo(original.etapas.Length));

        for (int i = 0; i < original.etapas.Length; i++)
        {
            Assert.That(estrutura.etapas[i].animacao, Is.EqualTo(original.etapas[i].animacao),
                $"{cenario} etapa {i}: animacao divergente.");
            Assert.That(estrutura.etapas[i].telaDisplay, Is.EqualTo(original.etapas[i].telaDisplay),
                $"{cenario} etapa {i}: telaDisplay divergente.");
            Assert.That(estrutura.etapas[i].vfx, Is.EqualTo(original.etapas[i].vfx),
                $"{cenario} etapa {i}: vfx divergente.");
            Assert.That(NormalizarTexto(estrutura.etapas[i].textoDisplay),
                Is.EqualTo(NormalizarTexto(original.etapas[i].textoDisplay)),
                $"{cenario} etapa {i}: textoDisplay divergente.");
        }
    }

    #endregion

    #region MARK - Aceite: estrutura desacoplada dos idiomas

    [TestCaseSource(nameof(Cenarios))]
    public void MesclarComQualquerIdioma_ProduzOsMesmosDadosEstruturais(string cenario)
    {
        EstruturaCenario estrutura = CarregarEstrutura(cenario);

        foreach (string idioma in Idiomas)
        {
            CenarioJson traduzido = CarregarTraducao(idioma, cenario);
            Etapa[] etapasTraduzidas = ConverterParaEtapas(traduzido);

            MesclaDeCenario.Resultado resultado =
                MesclaDeCenario.Mesclar(estrutura, etapasTraduzidas, traduzido.layer);

            Assert.That(resultado.UsouEstrutura, Is.True);
            Assert.That(resultado.TemDivergencia, Is.False,
                $"{cenario}/{idioma}: {resultado.Divergencia}");

            for (int i = 0; i < resultado.Etapas.Length; i++)
            {
                Assert.That(resultado.Etapas[i].animacao, Is.EqualTo(estrutura.etapas[i].animacao),
                    $"{cenario}/{idioma} etapa {i}: a animacao deve vir da estrutura.");
                Assert.That(resultado.Etapas[i].tutorial,
                    Is.EqualTo(traduzido.etapas[i].tutorial ?? string.Empty),
                    $"{cenario}/{idioma} etapa {i}: o tutorial deve vir da traducao.");
            }
        }
    }

    #endregion

    #region MARK - Carregamento

    private static EstruturaCenario CarregarEstrutura(string cenario)
    {
        string caminho = $"{PastaDeProblemas}/estrutura/{cenario}.json";
        TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(caminho);

        Assert.That(asset, Is.Not.Null, $"Arquivo nao encontrado: {caminho}");

        return JsonUtility.FromJson<EstruturaCenario>(asset.text);
    }

    private static CenarioJson CarregarTraducao(string idioma, string cenario)
    {
        string caminho = $"{PastaDeProblemas}/{idioma}/{cenario}.json";
        TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(caminho);

        Assert.That(asset, Is.Not.Null, $"Arquivo nao encontrado: {caminho}");

        return JsonUtility.FromJson<CenarioJson>(asset.text);
    }

    private static Etapa[] ConverterParaEtapas(CenarioJson origem)
    {
        var etapas = new Etapa[origem.etapas.Length];

        for (int i = 0; i < origem.etapas.Length; i++)
        {
            etapas[i] = new Etapa
            {
                tutorial = origem.etapas[i].tutorial,
                animacao = origem.etapas[i].animacao,
                telaDisplay = origem.etapas[i].telaDisplay,
                vfx = origem.etapas[i].vfx,
                textoDisplay = origem.etapas[i].textoDisplay,
            };
        }

        return etapas;
    }

    private static string NormalizarTexto(string valor)
    {
        return valor ?? string.Empty;
    }

    #endregion
}
