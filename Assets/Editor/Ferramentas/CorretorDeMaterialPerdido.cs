// Rodar quando os prefabs de Resources aparecerem com material perdido em PARAFUSO B.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CorretorDeMaterialPerdido
{
    #region MARK - Contrato

    public const string PastaDosPrefabs = "Assets/Resources";
    public const string MaterialDosParafusos = "Assets/Prefab/Teste/Materials/Old Metal.mat";

    public static readonly string[] PecasCorrigidas = { "PARAFUSO B" };

    #endregion

    #region MARK - Execução

    [MenuItem("Sense/Alertas/Corrigir material perdido dos prefabs")]
    public static void Corrigir()
    {
        Material substituto = AssetDatabase.LoadAssetAtPath<Material>(MaterialDosParafusos);

        if (substituto == null)
        {
            Debug.LogError($"[MaterialPerdido] Material nao encontrado: {MaterialDosParafusos}");
            return;
        }

        int corrigidos = 0;

        foreach (string caminho in CaminhosDosPrefabs())
        {
            GameObject conteudo = PrefabUtility.LoadPrefabContents(caminho);

            try
            {
                if (!CorrigirPecas(conteudo, substituto, Path.GetFileNameWithoutExtension(caminho))) continue;

                PrefabUtility.SaveAsPrefabAsset(conteudo, caminho);
                corrigidos++;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(conteudo);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[MaterialPerdido] {corrigidos} prefab(s) corrigido(s).");
    }

    private static bool CorrigirPecas(GameObject conteudo, Material substituto, string prefab)
    {
        bool mudou = false;

        foreach (Renderer renderizador in conteudo.GetComponentsInChildren<Renderer>(true))
        {
            if (!PecasCorrigidas.Contains(renderizador.name)) continue;

            Material[] materiais = renderizador.sharedMaterials;

            for (int i = 0; i < materiais.Length; i++)
            {
                if (!EstaPerdido(materiais[i])) continue;

                string antes = materiais[i] == null ? "<nulo>" : materiais[i].name;
                materiais[i] = substituto;
                mudou = true;
                Debug.Log($"[MaterialPerdido] {prefab}/{renderizador.name}[{i}]: '{antes}' -> '{substituto.name}'.");
            }

            if (mudou) renderizador.sharedMaterials = materiais;
        }

        return mudou;
    }

    private static bool EstaPerdido(Material material)
    {
        if (material == null) return true;

        string caminho = AssetDatabase.GetAssetPath(material);

        return string.IsNullOrEmpty(caminho) || !caminho.StartsWith("Assets/");
    }

    private static IEnumerable<string> CaminhosDosPrefabs()
    {
        return AssetDatabase.FindAssets("t:Prefab", new[] { PastaDosPrefabs })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(caminho => Path.GetFileName(caminho).StartsWith("M4SMARTTesteProblema"))
            .OrderBy(caminho => caminho);
    }

    #endregion
}
