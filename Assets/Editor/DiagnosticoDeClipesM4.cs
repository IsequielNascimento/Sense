using System.Linq;
using UnityEditor;
using UnityEngine;

public static class DiagnosticoDeClipesM4
{
    public const string CaminhoFbx = "Assets/Prefab/Teste/M4SMARTTeste.fbx";

    public const string CaminhoPrefabA8 = "Assets/Resources/M4Problem1/M4SMARTTesteProblema1.prefab";

    [MenuItem("Sense/Diagnostico/Comparar escala do FBX e do prefab")]
    public static void CompararEscalas()
    {
        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoFbx);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoPrefabA8);

        if (fbx != null)
        {
            Debug.Log($"[DiagEscala] FBX root '{fbx.name}' scale={fbx.transform.localScale} pos={fbx.transform.localPosition}");

            var bounds = CalcularBounds(fbx);
            Debug.Log($"[DiagEscala] FBX bounds size={bounds.size} center={bounds.center}");

            Transform chave = fbx.transform.Find("CHAVE_S");
            if (chave != null) Debug.Log($"[DiagEscala] FBX CHAVE_S scale={chave.localScale} pos={chave.localPosition}");
        }

        if (prefab != null)
        {
            Debug.Log($"[DiagEscala] Prefab root scale={prefab.transform.localScale} pos={prefab.transform.localPosition}");

            var bounds = CalcularBounds(prefab);
            Debug.Log($"[DiagEscala] Prefab bounds size={bounds.size} center={bounds.center}");
        }
    }

    static Bounds CalcularBounds(GameObject alvo)
    {
        var renderers = alvo.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return new Bounds();

        Bounds b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return b;
    }

    [MenuItem("Sense/Diagnostico/Listar curvas dos clipes do FBX")]
    public static void Listar()
    {
        var clipes = AssetDatabase
            .LoadAllAssetRepresentationsAtPath(CaminhoFbx)
            .OfType<AnimationClip>()
            .ToArray();

        Debug.Log($"[DiagClipes] {clipes.Length} clipes expostos em {CaminhoFbx}");

        foreach (AnimationClip clip in clipes)
        {
            var bindings = AnimationUtility.GetCurveBindings(clip);
            var paths = bindings.Select(b => string.IsNullOrEmpty(b.path) ? "<ROOT>" : b.path).Distinct().ToArray();

            Debug.Log(
                $"[DiagClipes] clip='{clip.name}' duracao={clip.length:F2}s curvas={bindings.Length} " +
                $"objetosAnimados={paths.Length}");

            foreach (var b in bindings)
            {
                var curva = AnimationUtility.GetEditorCurve(clip, b);
                if (curva == null || curva.keys.Length == 0) continue;

                float min = curva.keys.Min(k => k.value);
                float max = curva.keys.Max(k => k.value);
                string caminho = string.IsNullOrEmpty(b.path) ? "<ROOT>" : b.path;

                Debug.Log(
                    $"[DiagClipes]    '{caminho}'.{b.propertyName} keys={curva.keys.Length} " +
                    $"min={min:F4} max={max:F4} primeiro={curva.keys[0].value:F4}");
            }
        }
    }
}
