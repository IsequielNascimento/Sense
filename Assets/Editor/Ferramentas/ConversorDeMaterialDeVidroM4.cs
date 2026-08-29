// Rodar após reimportar o FBX: a transparência não sobrevive à viagem Blender -> Unity,
// então os materiais translúcidos do M4 são configurados aqui.
using UnityEditor;
using UnityEngine;

public static class ConversorDeMaterialDeVidroM4
{
    #region MARK - Contrato

    public const string ShaderUrpLit = "Universal Render Pipeline/Lit";
    public const string PastaDeMateriais = "Assets/Prefab/Teste/Materials";

    public readonly struct MaterialTranslucido
    {
        public MaterialTranslucido(string arquivo, float alfa, float suavidade, float tom = 1f)
        {
            Arquivo = arquivo;
            Alfa = alfa;
            Suavidade = suavidade;
            Tom = tom;
        }

        public string Arquivo { get; }
        public float Alfa { get; }
        public float Suavidade { get; }
        public float Tom { get; }

        public string Caminho => $"{PastaDeMateriais}/{Arquivo}.mat";
    }

    public static readonly MaterialTranslucido[] Translucidos =
    {
        new MaterialTranslucido("M4GLASS", 0.2f, 0.95f),
        new MaterialTranslucido("Transparent plastic (Plastic)", 0.70980394f, 0.95f, 0.3018868f),
    };

    #endregion

    #region MARK - Execução

    [MenuItem("Sense/Alertas/Converter materiais translucidos para URP")]
    public static void Converter()
    {
        var shader = Shader.Find(ShaderUrpLit);
        if (shader == null)
        {
            Debug.LogError($"[ConversorDeVidro] Shader não encontrado: {ShaderUrpLit}");
            return;
        }

        int convertidos = 0;

        foreach (MaterialTranslucido alvo in Translucidos)
        {
            if (Aplicar(alvo, shader)) convertidos++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[ConversorDeVidro] {convertidos}/{Translucidos.Length} material(is) translúcido(s) configurado(s).");
    }

    static bool Aplicar(MaterialTranslucido alvo, Shader shader)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(alvo.Caminho);
        if (material == null)
        {
            Debug.LogError($"[ConversorDeVidro] Material não encontrado: {alvo.Caminho}");
            return false;
        }

        material.shader = shader;

        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
        material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0f);
        material.SetFloat("_AlphaClip", 0f);
        material.SetFloat("_Metallic", 0f);
        material.SetFloat("_Smoothness", alvo.Suavidade);

        material.SetColor("_BaseColor", new Color(alvo.Tom, alvo.Tom, alvo.Tom, alvo.Alfa));

        material.SetFloat("_BlendModePreserveSpecular", 1f);
        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHATEST_ON");
        material.SetShaderPassEnabled("ShadowCaster", false);
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        EditorUtility.SetDirty(material);
        Debug.Log($"[ConversorDeVidro] {alvo.Caminho}: transparente, alfa {alvo.Alfa}, tom {alvo.Tom}, smoothness {alvo.Suavidade}.");
        return true;
    }

    #endregion
}
