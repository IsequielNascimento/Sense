using UnityEditor;
using UnityEngine;

public static class ConversorDeMaterialDeVidroM4
{
    #region MARK - Contrato

    public const string CaminhoMaterial = "Assets/Prefab/Teste/Materials/M4GLASS.mat";
    public const string ShaderUrpLit = "Universal Render Pipeline/Lit";

    #endregion

    #region MARK - Execução

    [MenuItem("Sense/Alertas/Converter M4GLASS para URP transparente")]
    public static void Converter()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(CaminhoMaterial);
        if (material == null)
        {
            Debug.LogError($"[ConversorDeVidro] Material não encontrado: {CaminhoMaterial}");
            return;
        }

        var shader = Shader.Find(ShaderUrpLit);
        if (shader == null)
        {
            Debug.LogError($"[ConversorDeVidro] Shader não encontrado: {ShaderUrpLit}");
            return;
        }

        material.shader = shader;

        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0f);
        material.SetFloat("_AlphaClip", 0f);
        material.SetFloat("_Metallic", 0f);
        material.SetFloat("_Smoothness", 0.95f);
        material.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.2f));

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHATEST_ON");
        material.SetShaderPassEnabled("ShadowCaster", false);
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();
        Debug.Log($"[ConversorDeVidro] {CaminhoMaterial} convertido para {ShaderUrpLit} transparente.");
    }

    #endregion
}
