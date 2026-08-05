using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class DominioDeCenarioTests
{
    #region MARK - Valores centralizados

    [TestCase(DominioDeCenario.Treinamento)]
    [TestCase(DominioDeCenario.Configuracao)]
    [TestCase(DominioDeCenario.Manutencao)]
    [TestCase(DominioDeCenario.Alerta)]
    public void ValoresCentralizados_SaoValidos(string dominio)
    {
        Assert.That(DominioDeCenario.EhValido(dominio), Is.True);
    }

    [TestCase("Alerta ")]
    [TestCase("alerta")]
    [TestCase("Diagnostico")]
    [TestCase("")]
    [TestCase(null)]
    public void ForaDaListaCentralizada_EInvalido(string dominio)
    {
        Assert.That(DominioDeCenario.EhValido(dominio), Is.False);
    }

    #endregion

    #region MARK - Elegibilidade para alerta publico

    [Test]
    public void SomenteDominioAlertaComPrimaryCodeOficial_PodeSerApresentado()
    {
        var estrutura = new EstruturaCenario
        {
            scenarioId = "X",
            dominio = DominioDeCenario.Alerta,
            primaryCode = "A18",
        };

        Assert.That(DominioDeCenario.PodeSerApresentadoComoAlertaPublico(estrutura), Is.True);
    }

    [Test]
    public void DominioDiferenteDeAlerta_NaoPodeSerApresentado()
    {
        var estrutura = new EstruturaCenario
        {
            scenarioId = "X",
            dominio = DominioDeCenario.Treinamento,
            primaryCode = "A18",
        };

        Assert.That(DominioDeCenario.PodeSerApresentadoComoAlertaPublico(estrutura), Is.False);
    }

    [Test]
    public void DominioAlertaSemPrimaryCodeOficial_NaoPodeSerApresentado()
    {
        var estrutura = new EstruturaCenario
        {
            scenarioId = "X",
            dominio = DominioDeCenario.Alerta,
            primaryCode = "C12",
        };

        Assert.That(DominioDeCenario.PodeSerApresentadoComoAlertaPublico(estrutura), Is.False);
    }

    [Test]
    public void EstruturaNula_NaoPodeSerApresentada()
    {
        Assert.That(DominioDeCenario.PodeSerApresentadoComoAlertaPublico(null), Is.False);
    }

    #endregion

    #region MARK - Aceite: nenhum cenario legado e alerta publico

    private const string PastaEstrutural = "Assets/Resources/BancoDeDadosProblemas/estrutura";

    private static readonly string[] Cenarios =
    {
        "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8"
    };

    [TestCaseSource(nameof(Cenarios))]
    public void NenhumCenarioLegado_PodeSerApresentadoComoAlertaPublico(string cenario)
    {
        EstruturaCenario estrutura = Carregar(cenario);

        Assert.That(DominioDeCenario.PodeSerApresentadoComoAlertaPublico(estrutura), Is.False,
            $"{cenario}: cenario legado nao pode ser exposto como alerta publico.");
    }

    #endregion

    #region MARK - Aceite: classificacao trava as colisoes resolvidas

    private static readonly Dictionary<string, string> DominioEsperado = new Dictionary<string, string>
    {
        { "A1", DominioDeCenario.Treinamento },
        { "A2", DominioDeCenario.Configuracao },
        { "A3", DominioDeCenario.Configuracao },
        { "A4", DominioDeCenario.Treinamento },
        { "A5", DominioDeCenario.Treinamento },
        { "A6", DominioDeCenario.Treinamento },
        { "A7", DominioDeCenario.Treinamento },
        { "A8", DominioDeCenario.Manutencao },
    };

    [TestCaseSource(nameof(Cenarios))]
    public void DominioGravado_CorrespondeAClassificacaoAprovada(string cenario)
    {
        EstruturaCenario estrutura = Carregar(cenario);

        Assert.That(estrutura.dominio, Is.EqualTo(DominioEsperado[cenario]));
    }

    [Test]
    public void A2EA3_SaoConfiguracaoRelacionadasAC12EC15()
    {
        EstruturaCenario a2 = Carregar("A2");
        EstruturaCenario a3 = Carregar("A3");

        Assert.That(a2.dominio, Is.EqualTo(DominioDeCenario.Configuracao));
        Assert.That(a2.relatedCodes, Contains.Item("C12"));
        Assert.That(a3.dominio, Is.EqualTo(DominioDeCenario.Configuracao));
        Assert.That(a3.relatedCodes, Contains.Item("C15"));
    }

    [Test]
    public void A6EA7_SaoTreinamentoComPrimaryCodeA18EA20()
    {
        EstruturaCenario a6 = Carregar("A6");
        EstruturaCenario a7 = Carregar("A7");

        Assert.That(a6.dominio, Is.EqualTo(DominioDeCenario.Treinamento));
        Assert.That(a6.primaryCode, Is.EqualTo("A18"));
        Assert.That(a7.dominio, Is.EqualTo(DominioDeCenario.Treinamento));
        Assert.That(a7.primaryCode, Is.EqualTo("A20"));
    }

    [Test]
    public void A8_EManutencaoComPrimaryCodeA11ERelacionadoAC18()
    {
        EstruturaCenario a8 = Carregar("A8");

        Assert.That(a8.dominio, Is.EqualTo(DominioDeCenario.Manutencao));
        Assert.That(a8.primaryCode, Is.EqualTo("A11"));
        Assert.That(a8.relatedCodes, Contains.Item("C18"));
    }

    [TestCaseSource(nameof(Cenarios))]
    public void ScenarioIdENomeDeArquivo_PermanecemInalterados(string cenario)
    {
        EstruturaCenario estrutura = Carregar(cenario);

        Assert.That(estrutura.scenarioId, Is.EqualTo(cenario));
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
