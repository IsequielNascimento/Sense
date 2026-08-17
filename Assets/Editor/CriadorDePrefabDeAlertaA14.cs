using UnityEditor;
using UnityEngine;

public static class CriadorDePrefabDeAlertaA14
{
    #region MARK - Contrato

    public const string PrefabOrigemA8 = "Assets/Resources/M4Problem1/M4SMARTTesteProblema1.prefab";
    public const string PastaPaiDestino = "Assets/Resources";
    public const string NomeDaPastaDestino = "M4Problem14";
    public const string PastaDestinoA14 = PastaPaiDestino + "/" + NomeDaPastaDestino;
    public const string PrefabDestinoA14 = PastaDestinoA14 + "/M4SMARTTesteProblema14.prefab";

    #endregion

    #region MARK - Execução

    [MenuItem("Sense/Alertas/Criar prefab M4 do A14")]
    public static void CriarOuAtualizarPrefabA14()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDestinoA14) != null)
        {
            Debug.Log($"[CriadorDePrefabDeAlertaA14] Prefab já existe em {PrefabDestinoA14}.");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabOrigemA8) == null)
        {
            Debug.LogError($"[CriadorDePrefabDeAlertaA14] Prefab de origem não encontrado: {PrefabOrigemA8}.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(PastaDestinoA14))
        {
            AssetDatabase.CreateFolder(PastaPaiDestino, NomeDaPastaDestino);
        }

        bool copiado = AssetDatabase.CopyAsset(PrefabOrigemA8, PrefabDestinoA14);
        if (!copiado)
        {
            Debug.LogError($"[CriadorDePrefabDeAlertaA14] Falha ao copiar {PrefabOrigemA8} para {PrefabDestinoA14}.");
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[CriadorDePrefabDeAlertaA14] Prefab criado em {PrefabDestinoA14}.");
    }

    #endregion
}
