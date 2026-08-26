using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public static class RegeneradorDePrefabsDeAlerta
{
    #region MARK - Contrato

    public const string CaminhoFbx = "Assets/Prefab/Teste/M4SMARTTeste.fbx";
    public const string NomeDisplayDinamico = "DisplayDynamic";
    public const string NomeObjetoLeds = "LEDS";

    static IEnumerable<string> Alvos => ModeloDeAlertaDisplay.TodosOsRecursos;

    #endregion

    #region MARK - Execução

    [MenuItem("Sense/Alertas/Regenerar prefabs a partir do FBX")]
    public static void RegenerarTodos()
    {
        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoFbx);
        if (fbx == null)
        {
            Debug.LogError($"[Regenerador] FBX não encontrado: {CaminhoFbx}");
            return;
        }

        int ok = 0;
        int total = 0;
        foreach (string alvo in Alvos)
        {
            total++;
            if (Regenerar(fbx, $"Assets/Resources/{alvo}.prefab")) ok++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Regenerador] {ok}/{total} prefabs regenerados a partir de {CaminhoFbx}.");
    }

    static bool Regenerar(GameObject fbx, string caminho)
    {
        var antigo = AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
        if (antigo == null)
        {
            Debug.LogError($"[Regenerador] Prefab não encontrado: {caminho}");
            return false;
        }

        var ancoraAntiga = antigo.transform.Find(NomeDisplayDinamico);
        if (ancoraAntiga == null)
        {
            Debug.LogError($"[Regenerador] {NomeDisplayDinamico} não encontrado em {caminho}.");
            return false;
        }

        string nome = System.IO.Path.GetFileNameWithoutExtension(caminho);
        var novo = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
        PrefabUtility.UnpackPrefabInstance(novo, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        novo.name = nome;

        CopiarComponentesExtras(antigo, novo);

        var ancoraNova = new GameObject(NomeDisplayDinamico).transform;
        ancoraNova.SetParent(novo.transform, false);
        ancoraNova.localPosition = ancoraAntiga.localPosition;
        ancoraNova.localRotation = ancoraAntiga.localRotation;
        ancoraNova.localScale = ancoraAntiga.localScale;

        CopiarComponentesExtras(ancoraAntiga.gameObject, ancoraNova.gameObject);
        ReligarReferencias(ancoraNova.gameObject, novo, ancoraNova);
        PreservarMateriais(antigo, novo);

        PrefabUtility.SaveAsPrefabAsset(novo, caminho);
        Object.DestroyImmediate(novo);
        return true;
    }

    static void PreservarMateriais(GameObject antigo, GameObject novo)
    {
        foreach (var rendererAntigo in antigo.GetComponentsInChildren<Renderer>(true))
        {
            string caminho = CaminhoRelativo(rendererAntigo.transform, antigo.transform);
            if (string.IsNullOrEmpty(caminho)) continue;

            Transform correspondente = novo.transform.Find(caminho);
            if (correspondente == null) continue;

            var rendererNovo = correspondente.GetComponent<Renderer>();
            if (rendererNovo == null) continue;

            string motivo = MotivoParaNaoPropagar(rendererAntigo, rendererNovo);

            if (motivo != null)
            {
                Debug.LogWarning(
                    $"[Regenerador] '{caminho}': {motivo}. " +
                    "Mantendo o material que veio do FBX em vez de propagar o do prefab antigo.");
                continue;
            }

            rendererNovo.sharedMaterials = rendererAntigo.sharedMaterials;
        }
    }

    static string MotivoParaNaoPropagar(Renderer antigo, Renderer novo)
    {
        var materiaisAntigos = antigo.sharedMaterials;

        if (materiaisAntigos.Length == 0) return "prefab antigo sem slot de material";

        if (materiaisAntigos.Length != novo.sharedMaterials.Length)
        {
            return $"prefab antigo tem {materiaisAntigos.Length} slot(s) e o FBX tem " +
                   $"{novo.sharedMaterials.Length}; o array antigo está desatualizado";
        }

        foreach (var material in materiaisAntigos)
        {
            if (material == null) return "prefab antigo com material perdido";

            if (!EhMaterialDoProjeto(material))
            {
                return $"prefab antigo usa '{material.name}', um material de fallback fora de Assets/";
            }
        }

        return null;
    }

    static bool EhMaterialDoProjeto(Material material)
    {
        string caminho = AssetDatabase.GetAssetPath(material);

        return !string.IsNullOrEmpty(caminho) && caminho.StartsWith("Assets/");
    }

    static string CaminhoRelativo(Transform alvo, Transform raiz)
    {
        var partes = new List<string>();

        for (Transform atual = alvo; atual != null && atual != raiz; atual = atual.parent)
        {
            partes.Insert(0, atual.name);
        }

        return string.Join("/", partes);
    }

    static void CopiarComponentesExtras(GameObject origem, GameObject destino)
    {
        foreach (var comp in origem.GetComponents<Component>())
        {
            if (comp is Transform) continue;

            ComponentUtility.CopyComponent(comp);
            ComponentUtility.PasteComponentAsNew(destino);
        }
    }

    static void ReligarReferencias(GameObject ancora, GameObject raiz, Transform ancoraTransform)
    {
        var leds = BuscarRecursivo(raiz.transform, NomeObjetoLeds);
        var rendererLeds = leds != null ? leds.GetComponent<Renderer>() : null;
        if (rendererLeds == null) Debug.LogWarning($"[Regenerador] Renderer de {NomeObjetoLeds} não encontrado em {raiz.name}.");

        foreach (var comp in ancora.GetComponents<Component>())
        {
            if (comp is Transform) continue;

            var so = new SerializedObject(comp);
            DefinirReferencia(so, "rendererLeds", rendererLeds);
            DefinirReferencia(so, "ancoraDisplayDinamico", ancoraTransform);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    static void DefinirReferencia(SerializedObject so, string campo, Object valor)
    {
        var prop = so.FindProperty(campo);
        if (prop != null && prop.propertyType == SerializedPropertyType.ObjectReference)
        {
            prop.objectReferenceValue = valor;
        }
    }

    static Transform BuscarRecursivo(Transform raiz, string nome)
    {
        if (raiz.name == nome) return raiz;

        foreach (Transform filho in raiz)
        {
            var achado = BuscarRecursivo(filho, nome);
            if (achado != null) return achado;
        }

        return null;
    }

    #endregion
}
