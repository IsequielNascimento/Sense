using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// Gera automaticamente o Font Asset e o Material Preset usados pelo LCD do M4.
/// A fonte DSEG14 imita o display alfanumérico de 14 segmentos do equipamento.
/// </summary>
public static class M4DisplayFontSetup
{
    private const string SourceFontPath =
        "Assets/TextMesh Pro/Fonts/DSEG/DSEG14Classic-Regular.ttf";

    private const string ResourcesFolder = "Assets/Resources/M4Display";
    private const string FontAssetPath = ResourcesFolder + "/DSEG14Classic SDF.asset";
    private const string MaterialPresetPath = ResourcesFolder + "/DSEG14Classic M4 LCD.mat";
    private const string PrefabM4Path = "Assets/Prefab/M4 Prefabs/Animação APK.prefab";

    private const string CaracteresDoDisplay =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 .,%_-+/";

    private static bool configurando;

    [InitializeOnLoadMethod]
    private static void AgendarConfiguracao()
    {
        EditorApplication.delayCall += GarantirAssets;
    }

    [MenuItem("Tools/M4/Regenerar fonte do display LCD")]
    public static void RegenerarAssets()
    {
        AssetDatabase.DeleteAsset(MaterialPresetPath);
        AssetDatabase.DeleteAsset(FontAssetPath);
        GarantirAssets();
    }

    private static void GarantirAssets()
    {
        if (configurando || EditorApplication.isCompiling || EditorApplication.isUpdating) return;

        Font fonteOriginal = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
        if (fonteOriginal == null) return;

        configurando = true;

        try
        {
            GarantirPastaResources();

            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (fontAsset == null)
            {
                fontAsset = CriarFontAsset(fonteOriginal);
            }

            Material materialPreset = AssetDatabase.LoadAssetAtPath<Material>(MaterialPresetPath);
            if (materialPreset == null)
            {
                materialPreset = CriarMaterialPreset(fontAsset);
            }

            AssetDatabase.SaveAssets();
            AplicarEstiloAoPrefab(fontAsset, materialPreset);
        }
        finally
        {
            configurando = false;
        }
    }

    private static TMP_FontAsset CriarFontAsset(Font fonteOriginal)
    {
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            fonteOriginal,
            90,
            9,
            GlyphRenderMode.SDFAA,
            1024,
            1024,
            AtlasPopulationMode.Dynamic,
            false);

        fontAsset.name = "DSEG14Classic SDF";

        SerializedObject serializedFontAsset = new SerializedObject(fontAsset);
        serializedFontAsset.FindProperty("m_ClearDynamicDataOnBuild").boolValue = false;
        serializedFontAsset.ApplyModifiedPropertiesWithoutUndo();

        Texture2D[] atlasTextures = fontAsset.atlasTextures;
        Material materialBase = fontAsset.material;

        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);

        foreach (Texture2D atlas in atlasTextures)
        {
            if (atlas != null && !AssetDatabase.Contains(atlas))
            {
                AssetDatabase.AddObjectToAsset(atlas, fontAsset);
            }
        }

        if (materialBase != null && !AssetDatabase.Contains(materialBase))
        {
            AssetDatabase.AddObjectToAsset(materialBase, fontAsset);
        }

        if (!fontAsset.TryAddCharacters(CaracteresDoDisplay, out string caracteresAusentes))
        {
            Debug.LogWarning(
                $"[M4DisplayFontSetup] A fonte LCD não contém: {caracteresAusentes}",
                fontAsset);
        }

        EditorUtility.SetDirty(fontAsset);
        return fontAsset;
    }

    private static Material CriarMaterialPreset(TMP_FontAsset fontAsset)
    {
        Material material = new Material(fontAsset.material)
        {
            name = "DSEG14Classic M4 LCD"
        };

        material.SetColor(ShaderUtilities.ID_FaceColor, new Color32(8, 8, 8, 255));
        material.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
        material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0f);
        material.SetFloat(ShaderUtilities.ID_OutlineSoftness, 0f);
        material.SetFloat(ShaderUtilities.ID_FaceDilate, 0f);
        material.SetFloat(ShaderUtilities.ID_Sharpness, 1f);

        AssetDatabase.CreateAsset(material, MaterialPresetPath);
        return material;
    }

    private static void AplicarEstiloAoPrefab(TMP_FontAsset fontAsset, Material materialPreset)
    {
        GameObject raiz = PrefabUtility.LoadPrefabContents(PrefabM4Path);

        try
        {
            bool alterado = false;

            foreach (TMP_Text texto in raiz.GetComponentsInChildren<TMP_Text>(true))
            {
                if (!PossuiAncestralComNome(texto.transform, "DisplayDynamic")) continue;

                if (texto.font != fontAsset)
                {
                    texto.font = fontAsset;
                    alterado = true;
                }

                if (texto.fontSharedMaterial != materialPreset)
                {
                    texto.fontSharedMaterial = materialPreset;
                    alterado = true;
                }

                Color32 corLCD = new Color32(8, 8, 8, 255);
                if (texto.color != (Color)corLCD)
                {
                    texto.color = corLCD;
                    alterado = true;
                }
            }

            if (alterado)
            {
                PrefabUtility.SaveAsPrefabAsset(raiz, PrefabM4Path);
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(raiz);
        }
    }

    private static bool PossuiAncestralComNome(Transform candidato, string nome)
    {
        for (Transform atual = candidato; atual != null; atual = atual.parent)
        {
            if (atual.name == nome) return true;
        }

        return false;
    }

    private static void GarantirPastaResources()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (!AssetDatabase.IsValidFolder(ResourcesFolder))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "M4Display");
        }
    }
}
