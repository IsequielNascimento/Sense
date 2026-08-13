using UnityEditor;
using UnityEngine;

public static class CriadorDePrefabDeAlertaA13
{
    #region MARK - Contrato

    public const string PrefabOrigemA8 = "Assets/Resources/M4Problem1/M4SMARTTesteProblema1.prefab";
    public const string PastaPaiDestino = "Assets/Resources";
    public const string NomeDaPastaDestino = "M4Problem13";
    public const string PastaDestinoA13 = PastaPaiDestino + "/" + NomeDaPastaDestino;
    public const string PrefabDestinoA13 = PastaDestinoA13 + "/M4SMARTTesteProblema13.prefab";

    #endregion

    #region MARK - Execução

    [MenuItem("Sense/Alertas/Criar prefab M4 do A13")]
    public static void CriarOuAtualizarPrefabA13()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDestinoA13) != null)
        {
            Debug.Log($"[CriadorDePrefabDeAlertaA13] Prefab já existe em {PrefabDestinoA13}.");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabOrigemA8) == null)
        {
            Debug.LogError($"[CriadorDePrefabDeAlertaA13] Prefab de origem não encontrado: {PrefabOrigemA8}.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(PastaDestinoA13))
        {
            AssetDatabase.CreateFolder(PastaPaiDestino, NomeDaPastaDestino);
        }

        bool copiado = AssetDatabase.CopyAsset(PrefabOrigemA8, PrefabDestinoA13);
        if (!copiado)
        {
            Debug.LogError($"[CriadorDePrefabDeAlertaA13] Falha ao copiar {PrefabOrigemA8} para {PrefabDestinoA13}.");
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[CriadorDePrefabDeAlertaA13] Prefab criado em {PrefabDestinoA13}.");
    }

    #endregion
}
