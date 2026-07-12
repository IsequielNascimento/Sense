using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class ActuatorVisibilityAnimationTests
{
    private const string ClipFolder = "Assets/Prefab/M4 Prefabs";
    private const string ActuatorPath = "M4_Smart_Final_Animado/M4/Atuador";
    private const string ControllerPath = ClipFolder + "/Animação APK.controller";

    private static readonly string[] AlwaysVisibleProblemClips =
    {
        "A1_p1", "A1_p2", "A1_p3", "A1_p4",
        "A2_p1", "A2_p2", "A2_p3", "A2_p4",
        "A3_p1", "A3_p2"
    };

    [TestCaseSource(nameof(AlwaysVisibleProblemClips))]
    public void A1ToA3Clips_KeepActuatorVisible(string clipName)
    {
        AnimationCurve curve = LoadVisibilityCurve(clipName);

        Assert.That(curve.keys, Is.Not.Empty, $"{clipName} must explicitly own tutorial visibility.");
        Assert.That(curve.keys.All(key => Mathf.Approximately(key.value, 1f)), Is.True);
    }

    [TestCase("A4_p1")]
    [TestCase("A4_p2")]
    public void A4DisassemblyClips_KeepMountedActuatorHidden(string clipName)
    {
        AnimationCurve curve = LoadVisibilityCurve(clipName);

        Assert.That(curve.keys.All(key => Mathf.Approximately(key.value, 0f)), Is.True);
    }

    [Test]
    public void A4Reposition_RestoresActuatorAtEnd()
    {
        AnimationCurve curve = LoadVisibilityCurve("A4_p3");

        Assert.That(curve.keys.First().value, Is.EqualTo(0f));
        Assert.That(curve.keys.Last().time, Is.EqualTo(1f));
        Assert.That(curve.keys.Last().value, Is.EqualTo(1f));
    }

    [Test]
    public void A4Tightening_HoldsRestoredVisibilityWithoutWritingTransformDefaults()
    {
        AnimationCurve curve = LoadVisibilityCurve("A4_p4");
        AnimatorState state = FindState("Problema 4", "A4_p4");

        Assert.That(curve.keys.All(key => Mathf.Approximately(key.value, 1f)), Is.True);
        Assert.That(state.writeDefaultValues, Is.False,
            "The visibility-only hold state must preserve transforms produced by A4_p3.");
    }

    [Test]
    public void ProblemLayers_DoNotEvaluateBeforeAProblemIsSelected()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

        Assert.That(controller.layers.Where(layer => layer.name.StartsWith("Problema", StringComparison.Ordinal))
            .All(layer => Mathf.Approximately(layer.defaultWeight, 0f)), Is.True,
            "Rebind must not evaluate default problem states before PlayAnimation selects one layer.");
    }

    [Test]
    public void AssemblyClips_DoNotOwnApplicationVisibility()
    {
        for (int step = 1; step <= 13; step++)
        {
            AnimationClip clip = LoadClip($"animacao_{step}");
            bool ownsVisibility = AnimationUtility.GetCurveBindings(clip).Any(binding =>
                binding.path == ActuatorPath && binding.propertyName == "m_IsActive");

            Assert.That(ownsVisibility, Is.False,
                $"animacao_{step} must leave structural visibility to the prefab.");
        }
    }

    private static AnimationCurve LoadVisibilityCurve(string clipName)
    {
        AnimationClip clip = LoadClip(clipName);
        EditorCurveBinding binding = AnimationUtility.GetCurveBindings(clip).Single(candidate =>
            candidate.path == ActuatorPath && candidate.propertyName == "m_IsActive");

        return AnimationUtility.GetEditorCurve(clip, binding);
    }

    private static AnimationClip LoadClip(string clipName)
    {
        string path = $"{ClipFolder}/{clipName}.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        Assert.That(clip, Is.Not.Null, $"Missing animation clip: {path}");
        return clip;
    }

    private static AnimatorState FindState(string layerName, string stateName)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        Assert.That(controller, Is.Not.Null);

        AnimatorControllerLayer layer = controller.layers.Single(candidate => candidate.name == layerName);
        AnimatorState state = layer.stateMachine.states
            .Select(candidate => candidate.state)
            .SingleOrDefault(candidate => candidate.name == stateName);
        Assert.That(state, Is.Not.Null, $"Missing state {layerName}.{stateName}");
        return state;
    }
}
