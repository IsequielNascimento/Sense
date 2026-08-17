using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class ConfiguradorDeAnimacaoB2
{
    #region MARK - Contrato

    public const string CaminhoFbx = "Assets/Prefab/Teste/M4SMARTTeste.fbx";
    public const string CaminhoController = "Assets/Resources/M4Problem1/M4SMARTTesteProblema1.controller";
    public const string TakeDoB2 = "CHAVE_S|B2BUTTON";
    public const string NomeDoEstado = "B2Button";
    public const string CamadaAlvo = "Base Layer";
    public const string CaminhoClipSaneado = "Assets/Animation/Clips/B2Button.anim";
    public const string PropriedadeDeEscala = "m_LocalScale";

    #endregion

    #region MARK - Execução

    [MenuItem("Sense/Alertas/Configurar animacao do botao B2")]
    public static void Configurar()
    {
        if (!ExporClipDoFbx()) return;

        CriarEstadoNoController();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static bool ExporClipDoFbx()
    {
        var importador = AssetImporter.GetAtPath(CaminhoFbx) as ModelImporter;
        if (importador == null)
        {
            Debug.LogError($"[AnimacaoB2] FBX não encontrado: {CaminhoFbx}");
            return false;
        }

        bool ajustouEscala = false;
        if (!importador.removeConstantScaleCurves)
        {
            importador.removeConstantScaleCurves = true;
            ajustouEscala = true;
            Debug.Log("[AnimacaoB2] removeConstantScaleCurves ativado: curvas de escala constantes (100) descartadas.");
        }

        var clips = importador.clipAnimations.ToList();
        if (clips.Any(c => c.name == NomeDoEstado))
        {
            Debug.Log($"[AnimacaoB2] Clip '{NomeDoEstado}' já estava exposto no import.");
            if (ajustouEscala) importador.SaveAndReimport();
            return true;
        }

        var take = importador.defaultClipAnimations.FirstOrDefault(c => c.takeName == TakeDoB2);
        if (take == null)
        {
            string existentes = string.Join(", ", importador.defaultClipAnimations.Select(c => c.takeName));
            Debug.LogError($"[AnimacaoB2] Take '{TakeDoB2}' não existe no FBX. Disponíveis: {existentes}");
            return false;
        }

        take.name = NomeDoEstado;
        clips.Add(take);
        importador.clipAnimations = clips.ToArray();
        importador.SaveAndReimport();

        Debug.Log($"[AnimacaoB2] Clip '{NomeDoEstado}' exposto a partir do take '{TakeDoB2}'.");
        return true;
    }

    static AnimationClip GerarClipSemCurvasDeEscala()
    {
        AnimationClip original = AssetDatabase
            .LoadAllAssetRepresentationsAtPath(CaminhoFbx)
            .OfType<AnimationClip>()
            .FirstOrDefault(c => c.name == NomeDoEstado);

        if (original == null)
        {
            Debug.LogError($"[AnimacaoB2] Clip '{NomeDoEstado}' não foi encontrado no FBX.");
            return null;
        }

        var saneado = new AnimationClip { name = NomeDoEstado, frameRate = original.frameRate };
        AnimationUtility.SetAnimationClipSettings(saneado, AnimationUtility.GetAnimationClipSettings(original));

        int descartadas = 0;

        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(original))
        {
            if (binding.propertyName.StartsWith(PropriedadeDeEscala))
            {
                descartadas++;
                continue;
            }

            AnimationUtility.SetEditorCurve(saneado, binding, AnimationUtility.GetEditorCurve(original, binding));
        }

        foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(original))
        {
            AnimationUtility.SetObjectReferenceCurve(
                saneado, binding, AnimationUtility.GetObjectReferenceCurve(original, binding));
        }

        var existente = AssetDatabase.LoadAssetAtPath<AnimationClip>(CaminhoClipSaneado);
        if (existente != null)
        {
            EditorUtility.CopySerialized(saneado, existente);
            EditorUtility.SetDirty(existente);
            Debug.Log($"[AnimacaoB2] Clip saneado atualizado ({descartadas} curvas de escala descartadas).");
            return existente;
        }

        AssetDatabase.CreateAsset(saneado, CaminhoClipSaneado);
        Debug.Log($"[AnimacaoB2] Clip saneado criado em {CaminhoClipSaneado} ({descartadas} curvas de escala descartadas).");
        return saneado;
    }

    static void CriarEstadoNoController()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CaminhoController);
        if (controller == null)
        {
            Debug.LogError($"[AnimacaoB2] Controller não encontrado: {CaminhoController}");
            return;
        }

        AnimatorControllerLayer camada = controller.layers.FirstOrDefault(l => l.name == CamadaAlvo);
        if (camada == null)
        {
            Debug.LogError($"[AnimacaoB2] Camada '{CamadaAlvo}' não existe em {CaminhoController}.");
            return;
        }

        AnimationClip clip = GerarClipSemCurvasDeEscala();
        if (clip == null) return;

        AnimatorStateMachine maquina = camada.stateMachine;
        AnimatorState existente = maquina.states
            .Select(s => s.state)
            .FirstOrDefault(s => s.name == NomeDoEstado);

        if (existente != null)
        {
            existente.motion = clip;
            EditorUtility.SetDirty(controller);
            Debug.Log($"[AnimacaoB2] Estado '{NomeDoEstado}' reapontado para o clip saneado.");
            return;
        }

        AnimatorState padraoAnterior = maquina.defaultState;

        AnimatorState estado = maquina.AddState(NomeDoEstado);
        estado.motion = clip;

        if (padraoAnterior != null) maquina.defaultState = padraoAnterior;

        EditorUtility.SetDirty(controller);
        Debug.Log($"[AnimacaoB2] Estado '{NomeDoEstado}' criado na camada '{CamadaAlvo}' de {CaminhoController}.");
    }

    #endregion
}
