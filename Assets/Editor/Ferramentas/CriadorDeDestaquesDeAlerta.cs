// Rodar após regenerar os prefabs de alerta: recria os objetos _Outline de destaque de cada alerta.
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CriadorDeDestaquesDeAlerta
{
    #region MARK - Contrato

    public const string CaminhoMaterialDeOutline = "Assets/Materials/Destaque Outline.mat";
    public const string SufixoDeContorno = "_Outline";
    public const float EscalaDoContorno = 1.02f;

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<(string Efeito, string[] Pecas)>> DestaquesPorAlerta =
        new Dictionary<string, IReadOnlyList<(string, string[])>>
        {
            {
                "A1", new[]
                {
                    (PerfisDeDisplayDeAlerta.DestaquePneumatica, new[] { "PNEUMATICA" }),
                    (PerfisDeDisplayDeAlerta.DestaqueMangueiras, new[] { "Magueira" }),
                    (PerfisDeDisplayDeAlerta.DestaqueAtuadorCopo, new[] { "ATUADOR", "COPO" }),
                }
            },
            {
                "A2", new[]
                {
                    (PerfisDeDisplayDeAlerta.DestaquePneumatica, new[] { "PNEUMATICA" }),
                    (PerfisDeDisplayDeAlerta.DestaqueMangueiras, new[] { "Magueira" }),
                    (PerfisDeDisplayDeAlerta.DestaqueAtuadorCopo, new[] { "ATUADOR", "COPO" }),
                }
            },
            {
                "A3", new[]
                {
                    (PerfisDeDisplayDeAlerta.DestaquePneumatica, new[] { "PNEUMATICA" }),
                    (PerfisDeDisplayDeAlerta.DestaqueMangueiras, new[] { "Magueira" }),
                    (PerfisDeDisplayDeAlerta.DestaqueAtuadorCopo, new[] { "ATUADOR", "COPO" }),
                }
            },
            {
                "A4", new[]
                {
                    (PerfisDeDisplayDeAlerta.DestaquePneumatica, new[] { "PNEUMATICA" }),
                    (PerfisDeDisplayDeAlerta.DestaqueMangueiras, new[] { "Magueira" }),
                }
            },
            {
                "A5", new[]
                {
                    (PerfisDeDisplayDeAlerta.DestaquePneumatica, new[] { "PNEUMATICA" }),
                    (PerfisDeDisplayDeAlerta.DestaqueMangueiras, new[] { "Magueira" }),
                    (PerfisDeDisplayDeAlerta.DestaqueAtuadorCopo, new[] { "ATUADOR", "COPO" }),
                }
            },
            {
                "A6", new[]
                {
                    (PerfisDeDisplayDeAlerta.DestaquePneumatica, new[] { "PNEUMATICA" }),
                    (PerfisDeDisplayDeAlerta.DestaqueModuloEletronico, new[] { "MODULO_ELETRONICO" }),
                }
            },
            {
                "A7", new[]
                {
                    (PerfisDeDisplayDeAlerta.DestaquePneumatica, new[] { "PNEUMATICA" }),
                    (PerfisDeDisplayDeAlerta.DestaqueMangueiras, new[] { "Magueira" }),
                    (PerfisDeDisplayDeAlerta.DestaqueAtuadorCopo, new[] { "ATUADOR", "COPO" }),
                }
            },
            {
                "A9", new[]
                {
                    (PerfisDeDisplayDeAlerta.DestaquePneumatica, new[] { "PNEUMATICA" }),
                    (PerfisDeDisplayDeAlerta.DestaqueMangueiras, new[] { "Magueira" }),
                }
            },
            {
                "A23", new[]
                {
                    (PerfisDeDisplayDeAlerta.DestaquePneumatica, new[] { "PNEUMATICA" }),
                    (PerfisDeDisplayDeAlerta.DestaqueModuloEletronico, new[] { "MODULO_ELETRONICO" }),
                }
            },
            {
                "A24", new[]
                {
                    (PerfisDeDisplayDeAlerta.DestaquePneumatica, new[] { "PNEUMATICA" }),
                    (PerfisDeDisplayDeAlerta.DestaqueModuloEletronico, new[] { "MODULO_ELETRONICO" }),
                }
            },
            {
                "A25", new[]
                {
                    (PerfisDeDisplayDeAlerta.DestaqueModuloEletronico, new[] { "MODULO_ELETRONICO" }),
                }
            },
            {
                "A19", new[]
                {
                    (PerfisDeDisplayDeAlerta.DestaqueModuloEletronico, new[] { "MODULO_ELETRONICO" }),
                }
            },
            {
                "A20", new[]
                {
                    (PerfisDeDisplayDeAlerta.DestaqueModuloEletronico, new[] { "MODULO_ELETRONICO" }),
                }
            },
            {
                "A17", new[]
                {
                    (PerfisDeDisplayDeAlerta.DestaquePneumatica, new[] { "PNEUMATICA" }),
                    (PerfisDeDisplayDeAlerta.DestaqueMangueiras, new[] { "Magueira" }),
                }
            },
            {
                "A18", new[]
                {
                    (PerfisDeDisplayDeAlerta.DestaquePneumatica, new[] { "PNEUMATICA" }),
                    (PerfisDeDisplayDeAlerta.DestaqueMangueiras, new[] { "Magueira" }),
                }
            },
            {
                "A15", new[]
                {
                    (PerfisDeDisplayDeAlerta.DestaquePneumatica, new[] { "PNEUMATICA" }),
                    (PerfisDeDisplayDeAlerta.DestaqueMangueiras, new[] { "Magueira" }),
                    (PerfisDeDisplayDeAlerta.DestaqueAtuadorCopo, new[] { "ATUADOR", "COPO" }),
                }
            },
            {
                "A16", new[]
                {
                    (PerfisDeDisplayDeAlerta.DestaquePneumatica, new[] { "PNEUMATICA" }),
                    (PerfisDeDisplayDeAlerta.DestaqueMangueiras, new[] { "Magueira" }),
                    (PerfisDeDisplayDeAlerta.DestaqueAtuadorCopo, new[] { "ATUADOR", "COPO" }),
                }
            },
        };

    #endregion

    #region MARK - Execução

    [MenuItem("Sense/Alertas/Criar destaques piscantes")]
    public static void CriarTodos()
    {
        Material outline = AssetDatabase.LoadAssetAtPath<Material>(CaminhoMaterialDeOutline);

        if (outline == null)
        {
            Debug.LogError($"[CriadorDeDestaques] Material nao encontrado: {CaminhoMaterialDeOutline}");
            return;
        }

        int ok = 0;

        foreach (var alerta in DestaquesPorAlerta)
        {
            if (Aplicar(alerta.Key, alerta.Value, outline)) ok++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[CriadorDeDestaques] {ok}/{DestaquesPorAlerta.Count} prefabs com destaque piscante.");
    }

    private static bool Aplicar(
        string codigo,
        IReadOnlyList<(string Efeito, string[] Pecas)> destaques,
        Material outline)
    {
        string caminho = CaminhoDoPrefab(codigo);
        GameObject conteudo = PrefabUtility.LoadPrefabContents(caminho);

        if (conteudo == null)
        {
            Debug.LogError($"[CriadorDeDestaques] Prefab nao encontrado: {caminho}");
            return false;
        }

        try
        {
            var criados = new List<(string Efeito, GameObject Objeto)>();

            foreach ((string efeito, string[] pecas) in destaques)
            {
                foreach (string nomeDaPeca in pecas)
                {
                    List<Transform> encontradas = BuscarPecas(conteudo.transform, nomeDaPeca);

                    if (encontradas.Count == 0)
                    {
                        Debug.LogError($"[CriadorDeDestaques] {codigo}: peca '{nomeDaPeca}' nao existe no prefab.");
                        return false;
                    }

                    foreach (Transform peca in encontradas)
                    {
                        GameObject contorno = RecriarContorno(peca, outline);

                        if (contorno == null)
                        {
                            Debug.LogError($"[CriadorDeDestaques] {codigo}: peca '{peca.name}' sem malha.");
                            return false;
                        }

                        criados.Add((efeito, contorno));
                    }
                }
            }

            RegistrarEfeitos(conteudo, criados);
            PrefabUtility.SaveAsPrefabAsset(conteudo, caminho);
            return true;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(conteudo);
        }
    }

    private static string CaminhoDoPrefab(string codigo)
    {
        return $"Assets/Resources/M4Problem{codigo}/M4SMARTTesteProblema{codigo}.prefab";
    }

    #endregion

    #region MARK - Contorno de uma peça

    private static GameObject RecriarContorno(Transform peca, Material outline)
    {
        Mesh malha = MalhaDaPeca(peca);
        var rendererDaPeca = peca.GetComponent<Renderer>();

        if (malha == null || rendererDaPeca == null) return null;

        string nomeDoContorno = peca.name + SufixoDeContorno;
        Transform anterior = peca.Find(nomeDoContorno);

        if (anterior != null) Object.DestroyImmediate(anterior.gameObject);

        var contorno = new GameObject(nomeDoContorno);
        contorno.transform.SetParent(peca, false);
        contorno.transform.localPosition = Vector3.zero;
        contorno.transform.localRotation = Quaternion.identity;
        contorno.transform.localScale = Vector3.one * EscalaDoContorno;

        contorno.AddComponent<MeshFilter>().sharedMesh = malha;

        var rendererDoContorno = contorno.AddComponent<MeshRenderer>();
        rendererDoContorno.sharedMaterials = Enumerable
            .Repeat(outline, Mathf.Max(1, rendererDaPeca.sharedMaterials.Length))
            .ToArray();
        rendererDoContorno.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rendererDoContorno.receiveShadows = false;

        var piscante = contorno.AddComponent<DestaquePiscante>();
        var serializado = new SerializedObject(piscante);
        serializado.FindProperty("rendererDestaque").objectReferenceValue = rendererDoContorno;
        serializado.ApplyModifiedPropertiesWithoutUndo();

        contorno.SetActive(false);
        return contorno;
    }

    private static Mesh MalhaDaPeca(Transform peca)
    {
        var filtro = peca.GetComponent<MeshFilter>();

        if (filtro != null && filtro.sharedMesh != null) return filtro.sharedMesh;

        var pele = peca.GetComponent<SkinnedMeshRenderer>();

        return pele != null ? pele.sharedMesh : null;
    }

    private static List<Transform> BuscarPecas(Transform raiz, string nome)
    {
        return raiz
            .GetComponentsInChildren<Transform>(true)
            .Where(atual => EhAPeca(atual.name, nome))
            .OrderBy(atual => atual.name, System.StringComparer.Ordinal)
            .ToList();
    }

    private static bool EhAPeca(string nomeDoObjeto, string nomeDaPeca)
    {
        if (nomeDoObjeto.EndsWith(SufixoDeContorno, System.StringComparison.Ordinal)) return false;

        return nomeDoObjeto == nomeDaPeca
            || nomeDoObjeto.StartsWith(nomeDaPeca + ".", System.StringComparison.Ordinal);
    }

    #endregion

    #region MARK - Registro no GerenciadorVisual

    private static void RegistrarEfeitos(GameObject raiz, IReadOnlyList<(string Efeito, GameObject Objeto)> criados)
    {
        var gerenciador = raiz.GetComponentInChildren<GerenciadorVisual>(true);

        if (gerenciador == null)
        {
            Debug.LogError($"[CriadorDeDestaques] {raiz.name}: sem GerenciadorVisual.");
            return;
        }

        var efeitos = new List<GerenciadorVisual.VFXSetup>();

        if (gerenciador.efeitosDisponiveis != null)
        {
            efeitos.AddRange(gerenciador.efeitosDisponiveis.Where(efeito =>
                efeito.VfxObject != null && !efeito.VfxObject.name.EndsWith(SufixoDeContorno)));
        }

        foreach ((string efeito, GameObject objeto) in criados)
        {
            efeitos.Add(new GerenciadorVisual.VFXSetup { Nome = efeito, VfxObject = objeto });
        }

        gerenciador.efeitosDisponiveis = efeitos;
        EditorUtility.SetDirty(gerenciador);
    }

    #endregion
}
