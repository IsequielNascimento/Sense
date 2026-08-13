using UnityEditor;
using UnityEngine;

public static class CriadorDePrefabDeAlertaA12
{
    #region MARK - Contrato

    public const string PrefabOrigemA8 = "Assets/Resources/M4Problem1/M4SMARTTesteProblema1.prefab";
    public const string PastaPaiDestino = "Assets/Resources";
    public const string NomeDaPastaDestino = "M4Problem12";
    public const string PastaDestinoA12 = PastaPaiDestino + "/" + NomeDaPastaDestino;
    public const string PrefabDestinoA12 = PastaDestinoA12 + "/M4SMARTTesteProblema12.prefab";

    #endregion

    #region MARK - Execução

    [MenuItem("Sense/Alertas/Criar prefab M4 do A12")]
    public static void CriarOuAtualizarPrefabA12()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDestinoA12) != null)
        {
            Debug.Log($"[CriadorDePrefabDeAlertaA12] Prefab já existe em {PrefabDestinoA12}.");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabOrigemA8) == null)
        {
            Debug.LogError($"[CriadorDePrefabDeAlertaA12] Prefab de origem não encontrado: {PrefabOrigemA8}.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(PastaDestinoA12))
        {
            AssetDatabase.CreateFolder(PastaPaiDestino, NomeDaPastaDestino);
        }

        bool copiado = AssetDatabase.CopyAsset(PrefabOrigemA8, PrefabDestinoA12);
        if (!copiado)
        {
            Debug.LogError($"[CriadorDePrefabDeAlertaA12] Falha ao copiar {PrefabOrigemA8} para {PrefabDestinoA12}.");
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[CriadorDePrefabDeAlertaA12] Prefab criado em {PrefabDestinoA12}.");
    }

    #endregion
}
