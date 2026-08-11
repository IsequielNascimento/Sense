using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class IntegridadeDeEtapasTests
{
    #region MARK - Contrato de dados

    private const string PastaDeProblemas = "Assets/Resources/BancoDeDadosProblemas";
    private static readonly string[] ControllersDeProblemas =
    {
        "Assets/Prefab/M4 Prefabs/Animação APK.controller",
        "Assets/Resources/M4Problem1/M4SMARTTesteProblema1.controller"
    };

    private static readonly string[] Idiomas = { "pt", "en", "es", "fr" };

    private static readonly string[] Cenarios =
    {
        "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8"
    };

    private static readonly string[] LacunasDeProducaoConhecidas =
    {
        "A3_p3",
        "A5_p1", "A5_p2", "A5_p3", "A5_p4",
        "A6_p1", "A6_p2", "A6_p3", "A6_p4",
        "A7_p1", "A7_p2", "A7_p3", "A7_p4",
        "A8_p1", "A8_p2", "A8_p3", "A8_p4"
    };

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

    #region MARK - Etapas e estados

    [TestCaseSource(nameof(Cenarios))]
    public void TodoCenarioPossuiEtapas(string cenario)
    {
        CenarioJson dados = CarregarCenario("pt", cenario);

        Assert.That(dados.etapas, Is.Not.Null, $"{cenario} nao possui array de etapas.");
        Assert.That(dados.etapas, Is.Not.Empty, $"{cenario} possui array de etapas vazio.");
        Assert.That(dados.layer, Is.Not.Null.And.Not.Empty, $"{cenario} nao declara camada.");
    }

    [Test]
    public void AnimacoesAusentes_CorrespondemExatamenteAsLacunasConhecidas()
    {
        HashSet<string> estados = EstadosDosControllers();

        List<string> ausentes = new List<string>();

        foreach (string cenario in Cenarios)
        {
            foreach (EtapaJson etapa in CarregarCenario("pt", cenario).etapas)
            {
                if (string.IsNullOrWhiteSpace(etapa.animacao) ||
                    etapa.animacao.Equals("nenhum", StringComparison.OrdinalIgnoreCase)) continue;
                if (estados.Contains(etapa.animacao)) continue;

                ausentes.Add(etapa.animacao);
            }
        }

        Assert.That(
            ausentes.OrderBy(nome => nome, StringComparer.Ordinal),
            Is.EqualTo(LacunasDeProducaoConhecidas.OrderBy(nome => nome, StringComparer.Ordinal)),
            "A lista de estados ausentes mudou. Nova quebra ou lacuna fechada sem atualizar o contrato.");
    }

    [TestCaseSource(nameof(Cenarios))]
    public void CamadaDeclaradaExisteNoController(string cenario)
    {
        AnimatorController controller = CarregarController();
        CenarioJson dados = CarregarCenario("pt", cenario);

        string[] camadas = controller.layers.Select(camada => camada.name).ToArray();

        Assert.That(camadas, Contains.Item(dados.layer), $"{cenario} referencia camada inexistente '{dados.layer}'.");
    }

    #endregion

    #region MARK - Topologia entre idiomas

    [TestCaseSource(nameof(Cenarios))]
    public void IdiomasCompartilhamMesmaTopologiaEstrutural(string cenario)
    {
        CenarioJson referencia = CarregarCenario("pt", cenario);

        foreach (string idioma in Idiomas.Where(item => item != "pt"))
        {
            CenarioJson comparado = CarregarCenario(idioma, cenario);

            Assert.That(comparado.etapas.Length, Is.EqualTo(referencia.etapas.Length),
                $"{cenario}/{idioma} possui quantidade de etapas diferente de pt.");
            Assert.That(comparado.layer, Is.EqualTo(referencia.layer),
                $"{cenario}/{idioma} declara camada diferente de pt.");

            for (int i = 0; i < referencia.etapas.Length; i++)
            {
                Assert.That(comparado.etapas[i].animacao, Is.EqualTo(referencia.etapas[i].animacao),
                    $"{cenario}/{idioma} etapa {i} diverge no campo estrutural 'animacao'.");
                Assert.That(comparado.etapas[i].vfx, Is.EqualTo(referencia.etapas[i].vfx),
                    $"{cenario}/{idioma} etapa {i} diverge no campo estrutural 'vfx'.");
                Assert.That(comparado.etapas[i].telaDisplay, Is.EqualTo(referencia.etapas[i].telaDisplay),
                    $"{cenario}/{idioma} etapa {i} diverge no campo estrutural 'telaDisplay'.");
            }
        }
    }

    #endregion

    #region MARK - Carregamento

    private static CenarioJson CarregarCenario(string idioma, string cenario)
    {
        string caminho = $"{PastaDeProblemas}/{idioma}/{cenario}.json";
        TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(caminho);

        Assert.That(asset, Is.Not.Null, $"Arquivo nao encontrado: {caminho}");

        return JsonUtility.FromJson<CenarioJson>(asset.text);
    }

    private static AnimatorController CarregarController()
    {
        AnimatorController controller =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllersDeProblemas[0]);

        Assert.That(controller, Is.Not.Null, $"Controller nao encontrado: {ControllersDeProblemas[0]}");

        return controller;
    }

    private static HashSet<string> EstadosDosControllers()
    {
        var estados = new HashSet<string>(StringComparer.Ordinal);

        foreach (string caminho in ControllersDeProblemas)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(caminho);
            Assert.That(controller, Is.Not.Null, $"Controller nao encontrado: {caminho}");
            estados.UnionWith(EstadosDoController(controller));
        }

        return estados;
    }

    private static HashSet<string> EstadosDoController(AnimatorController controller)
    {
        var estados = new HashSet<string>(StringComparer.Ordinal);

        foreach (AnimatorControllerLayer camada in controller.layers)
        {
            foreach (ChildAnimatorState filho in camada.stateMachine.states)
            {
                estados.Add(filho.state.name);
            }
        }

        return estados;
    }

    #endregion
}
