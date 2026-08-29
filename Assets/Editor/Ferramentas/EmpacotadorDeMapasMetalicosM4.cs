// Rodar após reexportar M4SMARTTeste.fbx: monta o mapa metallic/smoothness que o importador descarta.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EmpacotadorDeMapasMetalicosM4
{
    #region MARK - Contrato

    public const string PastaDeMateriais = "Assets/Prefab/Teste/Materials";
    public const string PastaDeTexturas = "Assets/Prefab/Teste/textures";
    public const string SufixoDoMapa = "_MetallicSmoothness";

    public readonly struct MapaDeMetal
    {
        public MapaDeMetal(string material, string metallic, string roughness = null)
        {
            Material = material;
            Metallic = metallic;
            Roughness = roughness;
        }

        public string Material { get; }
        public string Metallic { get; }
        public string Roughness { get; }
    }

    public static readonly MapaDeMetal[] Mapas =
    {
        new MapaDeMetal("Old Metal", "Used Metal_metallic.jpg"),
        new MapaDeMetal("BlackMetal", "Used Metal_metallic.jpg"),
        new MapaDeMetal("CleanMetal", "clean chrome_Metallic.jpg"),
        new MapaDeMetal("Dark Steel", "Steel Grey_old_Metallic.jpeg", "Steel Grey_old_Roughness.jpeg"),
    };

    #endregion

    #region MARK - Execução

    [MenuItem("Sense/Alertas/Empacotar mapas metalicos do M4")]
    public static void Empacotar()
    {
        int aplicados = 0;

        foreach (MapaDeMetal mapa in Mapas)
        {
            if (Aplicar(mapa)) aplicados++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[MapasMetalicos] {aplicados}/{Mapas.Length} material(is) com mapa metallic/smoothness.");
    }

    static bool Aplicar(MapaDeMetal mapa)
    {
        string caminhoDoMaterial = $"{PastaDeMateriais}/{mapa.Material}.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(caminhoDoMaterial);

        if (material == null)
        {
            Debug.LogError($"[MapasMetalicos] Material não encontrado: {caminhoDoMaterial}");
            return false;
        }

        Texture2D metallic = CarregarLegivel($"{PastaDeTexturas}/{mapa.Metallic}");
        if (metallic == null) return false;

        Texture2D roughness = mapa.Roughness == null
            ? null
            : CarregarLegivel($"{PastaDeTexturas}/{mapa.Roughness}");

        if (mapa.Roughness != null && roughness == null) return false;

        string destino = $"{PastaDeTexturas}/{mapa.Material}{SufixoDoMapa}.png";
        Texture2D empacotado = Empacotar(metallic, roughness, destino);
        if (empacotado == null) return false;

        float suavidade = material.HasFloat("_Smoothness") ? material.GetFloat("_Smoothness") : 0.5f;

        material.SetTexture("_MetallicGlossMap", empacotado);
        material.SetFloat("_Metallic", 1f);
        material.SetFloat("_SmoothnessTextureChannel", 0f);
        material.SetFloat("_GlossMapScale", suavidade);
        material.EnableKeyword("_METALLICSPECGLOSSMAP");

        EditorUtility.SetDirty(material);
        Debug.Log($"[MapasMetalicos] {mapa.Material}: {Path.GetFileName(destino)} (glossScale {suavidade:F2}).");
        return true;
    }

    static Texture2D Empacotar(Texture2D metallic, Texture2D roughness, string destino)
    {
        int largura = metallic.width;
        int altura = metallic.height;

        Color[] canalMetallic = metallic.GetPixels();
        Color[] canalRoughness = roughness == null
            ? null
            : Redimensionar(roughness, largura, altura);

        var pixels = new Color[canalMetallic.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            float m = canalMetallic[i].r;
            float suavidade = canalRoughness == null ? 1f : 1f - canalRoughness[i].r;
            pixels[i] = new Color(m, m, m, suavidade);
        }

        var mapa = new Texture2D(largura, altura, TextureFormat.RGBA32, true, true);
        mapa.SetPixels(pixels);
        mapa.Apply();

        File.WriteAllBytes(destino, mapa.EncodeToPNG());
        Object.DestroyImmediate(mapa);
        AssetDatabase.ImportAsset(destino, ImportAssetOptions.ForceUpdate);

        var importador = AssetImporter.GetAtPath(destino) as TextureImporter;
        if (importador != null)
        {
            importador.sRGBTexture = false;
            importador.alphaSource = TextureImporterAlphaSource.FromInput;
            importador.alphaIsTransparency = false;
            importador.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Texture2D>(destino);
    }

    static Color[] Redimensionar(Texture2D origem, int largura, int altura)
    {
        if (origem.width == largura && origem.height == altura) return origem.GetPixels();

        var pixels = new Color[largura * altura];

        for (int y = 0; y < altura; y++)
        {
            for (int x = 0; x < largura; x++)
            {
                pixels[y * largura + x] = origem.GetPixelBilinear((x + 0.5f) / largura, (y + 0.5f) / altura);
            }
        }

        return pixels;
    }

    static Texture2D CarregarLegivel(string caminho)
    {
        var importador = AssetImporter.GetAtPath(caminho) as TextureImporter;

        if (importador == null)
        {
            Debug.LogError($"[MapasMetalicos] Textura não encontrada: {caminho}");
            return null;
        }

        if (!importador.isReadable)
        {
            importador.isReadable = true;
            importador.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Texture2D>(caminho);
    }

    #endregion
}
