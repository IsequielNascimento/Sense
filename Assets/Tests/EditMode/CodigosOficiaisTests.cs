using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class CodigosOficiaisTests
{
    #region MARK - Catálogo do manual

    [Test]
    public void A10_NaoExisteNoManual()
    {
        Assert.That(CodigosOficiais.EhValido("A10"), Is.False);
        Assert.That(CodigosOficiais.Alertas.Count, Is.EqualTo(24));
    }

    [TestCase("A1")]
    [TestCase("A9")]
    [TestCase("A11")]
    [TestCase("A25")]
    [TestCase("C1")]
    [TestCase("C23")]
    public void CodigosDaTabela3_SaoValidos(string codigo)
    {
        Assert.That(CodigosOficiais.EhValido(codigo), Is.True);
    }

    [TestCase("A26")]
    [TestCase("C24")]
    [TestCase("A0")]
    [TestCase("")]
    [TestCase(null)]
    public void CodigosForaDaTabela_SaoInvalidos(string codigo)
    {
        Assert.That(CodigosOficiais.EhValido(codigo), Is.False);
    }

    [Test]
    public void EhAlerta_DistingueAlertasDeConfiguracoes()
    {
        Assert.That(CodigosOficiais.EhAlerta("A18"), Is.True);
        Assert.That(CodigosOficiais.EhAlerta("C18"), Is.False);
    }

    [Test]
    public void EhConfiguracao_DistingueConfiguracoesDeAlertas()
    {
        Assert.That(CodigosOficiais.EhConfiguracao("C18"), Is.True);
        Assert.That(CodigosOficiais.EhConfiguracao("A18"), Is.False);
    }

    [TestCase("A26")]
    [TestCase("C24")]
    [TestCase("")]
    [TestCase(null)]
    public void EhConfiguracao_ForaDaTabela_EInvalido(string codigo)
    {
        Assert.That(CodigosOficiais.EhConfiguracao(codigo), Is.False);
    }

    #endregion

    #region MARK - Códigos gravados na estrutura

    private const string PastaEstrutural = "Assets/Resources/BancoDeDadosProblemas/estrutura";

    private static readonly string[] Cenarios =
    {
        "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8"
    };

    private static readonly Regex PadraoDeCodigo = new Regex(@"\b[AC]\d{1,2}\b");

    [TestCaseSource(nameof(Cenarios))]
    public void TodoCodigoRelacionado_ExisteNoManual(string cenario)
    {
        EstruturaCenario estrutura = Carregar(cenario);

        Assert.That(estrutura.relatedCodes, Is.Not.Empty,
            $"{cenario} nao declara nenhum codigo relacionado.");

        foreach (string codigo in estrutura.relatedCodes)
        {
            Assert.That(CodigosOficiais.EhValido(codigo), Is.True,
                $"{cenario} referencia codigo inexistente '{codigo}'.");
        }
    }

    [TestCaseSource(nameof(Cenarios))]
    public void PrimaryCode_VazioOuAlertaValidoContidoNosRelacionados(string cenario)
    {
        EstruturaCenario estrutura = Carregar(cenario);

        if (string.IsNullOrEmpty(estrutura.primaryCode)) return;

        Assert.That(CodigosOficiais.EhAlerta(estrutura.primaryCode), Is.True,
            $"{cenario}: primaryCode '{estrutura.primaryCode}' nao e um alerta oficial.");
        Assert.That(estrutura.relatedCodes, Contains.Item(estrutura.primaryCode),
            $"{cenario}: primaryCode fora de relatedCodes.");
    }

    [TestCaseSource(nameof(Cenarios))]
    public void CodigosMostradosNoDisplay_EstaoNosRelacionados(string cenario)
    {
        EstruturaCenario estrutura = Carregar(cenario);
        var relacionados = new HashSet<string>(estrutura.relatedCodes, StringComparer.Ordinal);

        foreach (EtapaEstrutural etapa in estrutura.etapas)
        {
            if (string.IsNullOrEmpty(etapa.textoDisplay)) continue;

            foreach (Match ocorrencia in PadraoDeCodigo.Matches(etapa.textoDisplay))
            {
                Assert.That(relacionados, Contains.Item(ocorrencia.Value),
                    $"{cenario}: display mostra '{ocorrencia.Value}' sem declara-lo em relatedCodes.");
            }
        }
    }

    [Test]
    public void ColisoesDoPlaceholder_ForamResolvidasPeloManual()
    {
        var esperado = new Dictionary<string, string>
        {
            { "A6", "A18" },
            { "A7", "A20" },
            { "A8", "A11" },
        };

        foreach (KeyValuePair<string, string> par in esperado)
        {
            EstruturaCenario estrutura = Carregar(par.Key);

            Assert.That(estrutura.primaryCode, Is.EqualTo(par.Value),
                $"{par.Key}: o codigo oficial deve ser {par.Value}, nao o placeholder.");
        }
    }

    #endregion

    #region MARK - Carregamento

    private static EstruturaCenario Carregar(string cenario)
    {
        string caminho = $"{PastaEstrutural}/{cenario}.json";
        TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(caminho);

        Assert.That(asset, Is.Not.Null, $"Arquivo nao encontrado: {caminho}");

        return JsonUtility.FromJson<EstruturaCenario>(asset.text);
    }

    #endregion
}
