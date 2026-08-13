using UnityEditor;
using UnityEngine;

public static class CriadorDePrefabDeAlertaA11
{
    #region MARK - Contrato

    public const string PrefabOrigemA8 = "Assets/Resources/M4Problem1/M4SMARTTesteProblema1.prefab";
    public const string PastaPaiDestino = "Assets/Resources";
    public const string NomeDaPastaDestino = "M4Problem11";
    public const string PastaDestinoA11 = PastaPaiDestino + "/" + NomeDaPastaDestino;
    public const string PrefabDestinoA11 = PastaDestinoA11 + "/M4SMARTTesteProblema11.prefab";

    #endregion

    #region MARK - Execução

    [MenuItem("Sense/Alertas/Criar prefab M4 do A11")]
    public static void CriarOuAtualizarPrefabA11()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDestinoA11) != null)
        {
            Debug.Log($"[CriadorDePrefabDeAlertaA11] Prefab já existe em {PrefabDestinoA11}.");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabOrigemA8) == null)
        {
            Debug.LogError($"[CriadorDePrefabDeAlertaA11] Prefab de origem não encontrado: {PrefabOrigemA8}.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(PastaDestinoA11))
        {
            AssetDatabase.CreateFolder(PastaPaiDestino, NomeDaPastaDestino);
        }

        bool copiado = AssetDatabase.CopyAsset(PrefabOrigemA8, PrefabDestinoA11);
        if (!copiado)
        {
            Debug.LogError($"[CriadorDePrefabDeAlertaA11] Falha ao copiar {PrefabOrigemA8} para {PrefabDestinoA11}.");
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[CriadorDePrefabDeAlertaA11] Prefab criado em {PrefabDestinoA11}.");
    }

    #endregion
}
