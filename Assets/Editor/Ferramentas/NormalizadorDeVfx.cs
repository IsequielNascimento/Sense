// Rodar após renomear VFX no prefab: aplica os nomes canônicos que Etapa.vfx espera.
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class NormalizadorDeVfx
{
    #region MARK - Contrato

    public const string CaminhoDoPrefab = "Assets/Prefab/M4 Prefabs/Animação APK.prefab";

    public static readonly IReadOnlyDictionary<string, string> NomesCanonicos =
        new Dictionary<string, string>
        {
            { "animacao_valvula_ar", "destaque_valvula_ar" },
            { "multimentro_medindo", "multimetro_medindo" },
        };

    #endregion

    #region MARK - Execução

    [MenuItem("Sense/Normalizar nomes de VFX")]
    public static void Normalizar()
    {
        GameObject conteudo = PrefabUtility.LoadPrefabContents(CaminhoDoPrefab);

        if (conteudo == null)
        {
            Debug.LogError($"[NormalizadorDeVfx] Prefab nao encontrado: {CaminhoDoPrefab}");
            return;
        }

        try
        {
            int alterados = AplicarEm(conteudo);

            if (alterados > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(conteudo, CaminhoDoPrefab);
                AssetDatabase.SaveAssets();
            }

            Debug.Log($"[NormalizadorDeVfx] {alterados} nome(s) normalizado(s).");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(conteudo);
        }
    }

    private static int AplicarEm(GameObject raiz)
    {
        int alterados = 0;

        foreach (var gerenciador in raiz.GetComponentsInChildren<GerenciadorVisual>(true))
        {
            if (gerenciador.efeitosDisponiveis == null) continue;

            for (int i = 0; i < gerenciador.efeitosDisponiveis.Count; i++)
            {
                var efeito = gerenciador.efeitosDisponiveis[i];

                if (string.IsNullOrEmpty(efeito.Nome)) continue;
                if (!NomesCanonicos.TryGetValue(efeito.Nome, out string canonico)) continue;
                if (efeito.Nome == canonico) continue;

                efeito.Nome = canonico;
                gerenciador.efeitosDisponiveis[i] = efeito;
                EditorUtility.SetDirty(gerenciador);
                alterados++;
            }
        }

        return alterados;
    }

    #endregion
}
