using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class FerramentasDoM4Tests
{
    #region MARK: Derivacao a partir da animacao do passo

    [Test]
    public void CadaAnimacaoDeBotao_MostraAChaveMagnetica()
    {
        foreach (string botao in AnimacaoDeBotaoM4.Todas)
        {
            Assert.That(FerramentasDoM4.ParaAnimacao(botao), Is.EqualTo(FerramentasDoM4.ChaveMagnetica), botao);
        }
    }

    [Test]
    public void PassoSemBotao_NaoMostraFerramentaNenhuma()
    {
        foreach (string animacao in new[] { null, "", "   ", AnimacaoDoCopoM4.Calibrando, AnimacaoDoCopoM4.Calibrado })
        {
            Assert.That(FerramentasDoM4.ParaAnimacao(animacao), Is.Null, animacao ?? "<nulo>");
        }
    }

    [Test]
    public void AChavePhilips_NaoApareceEmNenhumaAnimacaoConhecida()
    {
        string[] conhecidas = AnimacaoDeBotaoM4.Todas.Concat(AnimacaoDoCopoM4.Todas).ToArray();

        foreach (string animacao in conhecidas)
        {
            Assert.That(FerramentasDoM4.DeveAparecer(FerramentasDoM4.ChavePhilips, animacao), Is.False, animacao);
        }
    }

    #endregion

    #region MARK: Prefabs nascem com as ferramentas desligadas

    [Test]
    public void PrefabsDeAlerta_TemAsFerramentasDesligadasNoRepouso()
    {
        string[] prefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .ToArray();

        Assert.That(prefabs, Is.Not.Empty);

        foreach (string caminho in prefabs)
        {
            var raiz = AssetDatabase.LoadAssetAtPath<GameObject>(caminho);

            foreach (string ferramenta in FerramentasDoM4.Todas)
            {
                Transform alvo = raiz.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(t => t.name == ferramenta);

                Assert.That(alvo, Is.Not.Null, $"{caminho}: '{ferramenta}' não existe.");
                Assert.That(alvo.gameObject.activeSelf, Is.False, $"{caminho}: '{ferramenta}' deveria nascer escondida.");
            }
        }
    }

    #endregion
}
