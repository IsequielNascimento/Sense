// Rodar após reexportar M4SMARTTeste.fbx: refaz os clips dos takes e descarta as curvas de m_LocalScale.
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class ConfiguradorDeAnimacoesDoM4
{
    #region MARK - Contrato

    public const string CaminhoFbx = "Assets/Prefab/Teste/M4SMARTTeste.fbx";
    public const string CaminhoController = "Assets/Resources/M4Problem1/M4SMARTTesteProblema1.controller";
    public const string CaminhoControllerA1 = "Assets/Resources/M4ProblemA1/M4SMARTTesteProblemaA1.controller";
    public const string CamadaAlvo = "Base Layer";
    public const string PastaDeClips = "Assets/Animation/Clips";
    public const string PropriedadeDeEscala = "m_LocalScale";
    public const string PrefixoDoTakeDeBotao = "CHAVE_S|CHAVE_S|";
    public const string PrefixoDoTakeDoCopo = "COPO|COPO|";

    public static string[] EstadosExpostos =>
        AnimacaoDeBotaoM4.Todas.Concat(AnimacaoDoCopoM4.Todas).ToArray();

    static readonly string[] ControllersComCopo = { CaminhoController, CaminhoControllerA1 };

    public static string TakeDoEstado(string estado)
    {
        string prefixo = AnimacaoDoCopoM4.Todas.Contains(estado)
            ? PrefixoDoTakeDoCopo
            : PrefixoDoTakeDeBotao;

        return $"{prefixo}{estado.ToUpperInvariant()}";
    }

    public static string CaminhoDoClip(string estado)
    {
        return $"{PastaDeClips}/{estado}.anim";
    }

    #endregion

    #region MARK - Execução

    [MenuItem("Sense/Alertas/Configurar animacoes do M4")]
    public static void Configurar()
    {
        if (!ExporClipsDoFbx()) return;

        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CaminhoController);
        if (controller == null)
        {
            Debug.LogError($"[AnimacoesM4] Controller não encontrado: {CaminhoController}");
            return;
        }

        AnimatorControllerLayer camada = controller.layers.FirstOrDefault(l => l.name == CamadaAlvo);
        if (camada == null)
        {
            Debug.LogError($"[AnimacoesM4] Camada '{CamadaAlvo}' não existe em {CaminhoController}.");
            return;
        }

        GarantirEstadoDeRepouso(controller, camada.stateMachine, CamadaAlvo);

        foreach (string estado in AnimacaoDeBotaoM4.Todas)
        {
            CriarEstadoNoController(controller, camada.stateMachine, estado);
        }

        ReapontarAnimacoesDoCopo();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void ReapontarAnimacoesDoCopo()
    {
        foreach (string caminho in ControllersComCopo)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(caminho);
            if (controller == null)
            {
                Debug.LogError($"[AnimacoesM4] Controller não encontrado: {caminho}");
                continue;
            }

            AnimatorControllerLayer camada = controller.layers
                .FirstOrDefault(l => l.name == PerfisDeDisplayDeAlerta.LayerCopo);

            if (camada == null)
            {
                Debug.LogError($"[AnimacoesM4] Camada '{PerfisDeDisplayDeAlerta.LayerCopo}' não existe em {caminho}.");
                continue;
            }

            GarantirEstadoDeRepouso(controller, camada.stateMachine, PerfisDeDisplayDeAlerta.LayerCopo);

            foreach (string estado in AnimacaoDoCopoM4.Todas)
            {
                CriarEstadoNoController(controller, camada.stateMachine, estado);
            }

            RemoverEstadosObsoletos(controller, camada.stateMachine, AnimacaoDoCopoM4.Todas);
        }
    }

    static void RemoverEstadosObsoletos(
        AnimatorController controller,
        AnimatorStateMachine maquina,
        IReadOnlyList<string> estadosValidos)
    {
        AnimatorState[] obsoletos = maquina.states
            .Select(s => s.state)
            .Where(s => s.name != DecisaoDeEtapaAr.EstadoDeRepouso && !estadosValidos.Contains(s.name))
            .ToArray();

        foreach (AnimatorState estado in obsoletos)
        {
            Debug.Log($"[AnimacoesM4] Estado obsoleto '{estado.name}' removido de '{PerfisDeDisplayDeAlerta.LayerCopo}'.");
            maquina.RemoveState(estado);
        }

        if (obsoletos.Length > 0) EditorUtility.SetDirty(controller);
    }

    static void GarantirEstadoDeRepouso(AnimatorController controller, AnimatorStateMachine maquina, string camada)
    {
        AnimatorState repouso = maquina.states
            .Select(s => s.state)
            .FirstOrDefault(s => s.name == DecisaoDeEtapaAr.EstadoDeRepouso);

        if (repouso == null)
        {
            repouso = maquina.AddState(DecisaoDeEtapaAr.EstadoDeRepouso);
            Debug.Log($"[AnimacoesM4] Estado '{DecisaoDeEtapaAr.EstadoDeRepouso}' criado em '{camada}'.");
        }

        repouso.motion = null;
        maquina.defaultState = repouso;
        EditorUtility.SetDirty(controller);
    }

    static bool ExporClipsDoFbx()
    {
        var importador = AssetImporter.GetAtPath(CaminhoFbx) as ModelImporter;
        if (importador == null)
        {
            Debug.LogError($"[AnimacoesM4] FBX não encontrado: {CaminhoFbx}");
            return false;
        }

        bool precisaReimportar = false;

        if (!importador.removeConstantScaleCurves)
        {
            importador.removeConstantScaleCurves = true;
            precisaReimportar = true;
            Debug.Log("[AnimacoesM4] removeConstantScaleCurves ativado: curvas de escala constantes (100) descartadas.");
        }

        var clips = importador.clipAnimations.ToList();
        var disponiveis = importador.defaultClipAnimations;

        foreach (string estado in EstadosExpostos)
        {
            string take = TakeDoEstado(estado);
            ModelImporterClipAnimation atual = clips.FirstOrDefault(c => c.name == estado);

            if (atual != null)
            {
                if (atual.takeName == take) continue;

                clips.Remove(atual);
                Debug.Log($"[AnimacoesM4] Clip '{estado}' vinha do take '{atual.takeName}'; refazendo a partir de '{take}'.");
            }

            ModelImporterClipAnimation origem = disponiveis.FirstOrDefault(c => c.takeName == take);

            if (origem == null)
            {
                string existentes = string.Join(", ", disponiveis.Select(c => c.takeName));
                Debug.LogError($"[AnimacoesM4] Take '{take}' não existe no FBX. Disponíveis: {existentes}");
                return false;
            }

            origem.name = estado;
            clips.Add(origem);
            precisaReimportar = true;
            Debug.Log($"[AnimacoesM4] Clip '{estado}' exposto a partir do take '{take}'.");
        }

        if (precisaReimportar)
        {
            importador.clipAnimations = clips.ToArray();
            importador.SaveAndReimport();
        }

        return true;
    }

    static AnimationClip GerarClipSemCurvasDeEscala(string estado)
    {
        AnimationClip original = AssetDatabase
            .LoadAllAssetRepresentationsAtPath(CaminhoFbx)
            .OfType<AnimationClip>()
            .FirstOrDefault(c => c.name == estado);

        if (original == null)
        {
            Debug.LogError($"[AnimacoesM4] Clip '{estado}' não foi encontrado no FBX.");
            return null;
        }

        var saneado = new AnimationClip { name = estado, frameRate = original.frameRate };
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

        string caminho = CaminhoDoClip(estado);
        var existente = AssetDatabase.LoadAssetAtPath<AnimationClip>(caminho);

        if (existente != null)
        {
            EditorUtility.CopySerialized(saneado, existente);
            EditorUtility.SetDirty(existente);
            Debug.Log($"[AnimacoesM4] Clip saneado '{estado}' atualizado ({descartadas} curvas de escala descartadas).");
            return existente;
        }

        AssetDatabase.CreateAsset(saneado, caminho);
        Debug.Log($"[AnimacoesM4] Clip saneado criado em {caminho} ({descartadas} curvas de escala descartadas).");
        return saneado;
    }

    static void CriarEstadoNoController(AnimatorController controller, AnimatorStateMachine maquina, string estado)
    {
        AnimationClip clip = GerarClipSemCurvasDeEscala(estado);
        if (clip == null) return;

        AnimatorState existente = maquina.states
            .Select(s => s.state)
            .FirstOrDefault(s => s.name == estado);

        if (existente != null)
        {
            existente.motion = clip;
            EditorUtility.SetDirty(controller);
            Debug.Log($"[AnimacoesM4] Estado '{estado}' reapontado para o clip saneado.");
            return;
        }

        AnimatorState padraoAnterior = maquina.defaultState;

        AnimatorState novo = maquina.AddState(estado);
        novo.motion = clip;

        if (padraoAnterior != null) maquina.defaultState = padraoAnterior;

        EditorUtility.SetDirty(controller);
        Debug.Log($"[AnimacoesM4] Estado '{estado}' criado em {AssetDatabase.GetAssetPath(controller)}.");
    }

    #endregion
}
