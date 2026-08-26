using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class MateriaisDosPrefabsDeAlertaTests
{
    #region MARK: Fixture

    private const string PastaDosPrefabs = "Assets/Resources";

    private static IEnumerable<string> CaminhosDosPrefabsDeAlerta()
    {
        return AssetDatabase
            .FindAssets("M4SMARTTesteProblema t:Prefab", new[] { PastaDosPrefabs })
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(caminho => caminho);
    }

    private static IEnumerable<string> RenderersSemMaterialDoProjeto(GameObject prefab)
    {
        foreach (Renderer renderizador in prefab.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materiais = renderizador.sharedMaterials;

            if (materiais.Length == 0)
            {
                yield return $"{renderizador.name} (sem slot)";
                continue;
            }

            foreach (Material material in materiais)
            {
                if (material == null)
                {
                    yield return $"{renderizador.name} (material perdido)";
                    break;
                }

                string caminho = AssetDatabase.GetAssetPath(material);

                if (string.IsNullOrEmpty(caminho) || !caminho.StartsWith("Assets/"))
                {
                    yield return $"{renderizador.name} (fallback '{material.name}' fora de Assets/)";
                    break;
                }
            }
        }
    }

    #endregion

    #region MARK: Peca sem material nao e desenhada pela camera

    [Test]
    public void NenhumPrefabDeAlerta_TemPecaComMaterialDeFallback()
    {
        var falhas = new List<string>();

        foreach (string caminho in CaminhosDosPrefabsDeAlerta())
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
            if (prefab == null) continue;

            string[] pecas = RenderersSemMaterialDoProjeto(prefab).ToArray();

            if (pecas.Length > 0)
            {
                falhas.Add($"{System.IO.Path.GetFileNameWithoutExtension(caminho)}: {string.Join(", ", pecas)}");
            }
        }

        Assert.That(falhas, Is.Empty,
            "peça com material perdido não é desenhada pela câmera:\n" + string.Join("\n", falhas));
    }

    [Test]
    public void ODiagnosticoEncontraOsPrefabsDeAlerta()
    {
        Assert.That(CaminhosDosPrefabsDeAlerta().Count(), Is.GreaterThanOrEqualTo(24));
    }

    #endregion
}
