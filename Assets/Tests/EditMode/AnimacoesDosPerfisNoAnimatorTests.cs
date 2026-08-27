using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;

public class AnimacoesDosPerfisNoAnimatorTests
{
    #region MARK: Resolucao igual a de LocalizedDatabase e ExibidorDeModeloBase

    private static readonly Type TipoModeloDeAlertaDisplay =
        Type.GetType("ModeloDeAlertaDisplay, Assembly-CSharp");

    private static readonly Type TipoArConstants = Type.GetType("ArConstants, Assembly-CSharp");

    private static string CamadaPadrao =>
        (string)TipoArConstants.GetField("DefaultAnimatorLayer").GetRawConstantValue();

    private static GameObject PrefabDoAlerta(string codigo)
    {
        return (GameObject)TipoModeloDeAlertaDisplay
            .GetMethod("Resolver")
            .Invoke(null, new object[] { codigo });
    }

    private static IEnumerable<string> EstadosDaCamada(AnimatorController controller, string camada)
    {
        AnimatorControllerLayer alvo = controller.layers.FirstOrDefault(layer => layer.name == camada);

        if (alvo == null) return null;

        return alvo.stateMachine.states.Select(filho => filho.state.name).ToArray();
    }

    #endregion

    #region MARK: Toda animacao pedida por um perfil precisa existir onde o passo vai procurar

    [Test]
    public void TodaAnimacaoDeQuadro_ExisteNaCamadaQueOPerfilUsa()
    {
        Assert.That(TipoModeloDeAlertaDisplay, Is.Not.Null);
        Assert.That(TipoArConstants, Is.Not.Null);

        var falhas = new List<string>();

        foreach (string codigo in PerfisDeDisplayDeAlerta.CodigosComPerfil)
        {
            PerfilDeDisplayDeAlerta perfil = PerfisDeDisplayDeAlerta.Obter(codigo);
            string camada = perfil.Layer ?? CamadaPadrao;

            GameObject prefab = PrefabDoAlerta(codigo);

            if (prefab == null)
            {
                falhas.Add($"{codigo}: ModeloDeAlertaDisplay nao resolve nenhum prefab.");
                continue;
            }

            Animator animator = prefab.GetComponentInChildren<Animator>(true);
            var controller = animator != null ? animator.runtimeAnimatorController as AnimatorController : null;

            if (controller == null)
            {
                falhas.Add($"{codigo}: prefab '{prefab.name}' sem Animator com AnimatorController.");
                continue;
            }

            IEnumerable<string> estados = EstadosDaCamada(controller, camada);

            if (estados == null)
            {
                falhas.Add($"{codigo}: a camada '{camada}' nao existe em '{controller.name}'.");
                continue;
            }

            IEnumerable<string> pedidas = perfil.EtapasOficiais
                .SelectMany(etapa => etapa.Quadros)
                .Select(quadro => quadro.Animacao)
                .Where(animacao => !string.IsNullOrWhiteSpace(animacao))
                .Distinct();

            foreach (string animacao in pedidas)
            {
                if (estados.Contains(animacao)) continue;

                falhas.Add(
                    $"{codigo}: o perfil pede '{animacao}', mas a camada '{camada}' de " +
                    $"'{controller.name}' nao tem esse estado. O passo ficaria sem animacao em runtime.");
            }
        }

        Assert.That(falhas, Is.Empty, string.Join("\n", falhas));
    }

    #endregion
}
