// Rodar sempre que M4SMARTTeste.fbx for reexportado: os prefabs de alerta são achatados, não Variants, e não herdam mudanças do FBX.
using System.Collections.Generic;
using System.Linq;
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
        CorrigirPoseDeRepouso(novo);
        DesligarFerramentas(novo);

        PrefabUtility.SaveAsPrefabAsset(novo, caminho);
        Object.DestroyImmediate(novo);
        return true;
    }

    static void DesligarFerramentas(GameObject novo)
    {
        foreach (string ferramenta in FerramentasDoM4.Todas)
        {
            Transform alvo = BuscarRecursivo(novo.transform, ferramenta);

            if (alvo == null)
            {
                Debug.LogWarning($"[Regenerador] Ferramenta '{ferramenta}' não existe no FBX.");
                continue;
            }

            alvo.gameObject.SetActive(false);
        }
    }

    static void CorrigirPoseDeRepouso(GameObject novo)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            ConfiguradorDeAnimacoesDoM4.CaminhoDoClip(AnimacaoDoCopoM4.Calibrado));

        if (clip == null)
        {
            Debug.LogWarning($"[Regenerador] Clip '{AnimacaoDoCopoM4.Calibrado}' ausente; pose de repouso não corrigida.");
            return;
        }

        foreach (var pose in PosesDoClip(clip))
        {
            Transform alvo = novo.transform.Find(pose.Key);

            if (alvo == null || alvo.localPosition != Vector3.zero) continue;
            if (pose.Value.Posicao == Vector3.zero) continue;

            alvo.localPosition = pose.Value.Posicao;
            if (pose.Value.Rotacao.HasValue) alvo.localRotation = pose.Value.Rotacao.Value;

            Debug.Log($"[Regenerador] '{pose.Key}': import veio na origem; pose de repouso de '{clip.name}' aplicada ({pose.Value.Posicao:F6}).");
        }
    }

    static Dictionary<string, (Vector3 Posicao, Quaternion? Rotacao)> PosesDoClip(AnimationClip clip)
    {
        var posicoes = new Dictionary<string, Vector3>();
        var rotacoes = new Dictionary<string, Quaternion>();

        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
        {
            AnimationCurve curva = AnimationUtility.GetEditorCurve(clip, binding);
            if (curva == null || curva.length == 0) continue;

            float valor = curva.keys[0].value;

            if (binding.propertyName.StartsWith("m_LocalPosition."))
            {
                Vector3 p = posicoes.TryGetValue(binding.path, out Vector3 atual) ? atual : Vector3.zero;
                DefinirComponente(ref p, binding.propertyName, valor);
                posicoes[binding.path] = p;
            }
            else if (binding.propertyName.StartsWith("m_LocalRotation."))
            {
                Quaternion q = rotacoes.TryGetValue(binding.path, out Quaternion atualQ) ? atualQ : new Quaternion(0f, 0f, 0f, 0f);
                DefinirComponente(ref q, binding.propertyName, valor);
                rotacoes[binding.path] = q;
            }
        }

        var poses = new Dictionary<string, (Vector3, Quaternion?)>();

        foreach (var item in posicoes)
        {
            Quaternion? rot = rotacoes.TryGetValue(item.Key, out Quaternion q) ? Normalizar(q) : (Quaternion?)null;
            poses[item.Key] = (item.Value, rot);
        }

        return poses;
    }

    static Quaternion? Normalizar(Quaternion q)
    {
        float tamanho = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);

        return tamanho < 0.0001f ? (Quaternion?)null : new Quaternion(q.x / tamanho, q.y / tamanho, q.z / tamanho, q.w / tamanho);
    }

    static void DefinirComponente(ref Vector3 alvo, string propriedade, float valor)
    {
        if (propriedade.EndsWith(".x")) alvo.x = valor;
        else if (propriedade.EndsWith(".y")) alvo.y = valor;
        else if (propriedade.EndsWith(".z")) alvo.z = valor;
    }

    static void DefinirComponente(ref Quaternion alvo, string propriedade, float valor)
    {
        if (propriedade.EndsWith(".x")) alvo.x = valor;
        else if (propriedade.EndsWith(".y")) alvo.y = valor;
        else if (propriedade.EndsWith(".z")) alvo.z = valor;
        else if (propriedade.EndsWith(".w")) alvo.w = valor;
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

            AplicarOverridesDoPrefab(rendererAntigo, rendererNovo, caminho);
        }
    }

    static void AplicarOverridesDoPrefab(Renderer antigo, Renderer novo, string caminho)
    {
        var materiaisAntigos = antigo.sharedMaterials;
        var materiaisNovos = novo.sharedMaterials;

        if (materiaisAntigos.Length != materiaisNovos.Length)
        {
            Debug.LogWarning(
                $"[Regenerador] '{caminho}': o prefab tem {materiaisAntigos.Length} slot(s) e o FBX tem " +
                $"{materiaisNovos.Length}; os overrides do prefab foram descartados por desalinhamento.");
            return;
        }

        var vindosDoFbx = new HashSet<Material>(materiaisNovos.Where(m => m != null));
        bool mudou = false;

        for (int i = 0; i < materiaisNovos.Length; i++)
        {
            Material doPrefab = materiaisAntigos[i];
            if (!EhMaterialDoProjeto(doPrefab)) continue;

            bool fbxSemMaterial = !EhMaterialDoProjeto(materiaisNovos[i]);
            bool overrideDeliberado = !vindosDoFbx.Contains(doPrefab);

            if (!fbxSemMaterial && !overrideDeliberado) continue;

            string motivo = fbxSemMaterial
                ? "o FBX nao trouxe material de projeto"
                : "o prefab usa um material que o FBX nao conhece";

            Debug.Log($"[Regenerador] '{caminho}'[{i}]: {motivo}; mantendo '{doPrefab.name}' do prefab antigo.");

            materiaisNovos[i] = doPrefab;
            mudou = true;
        }

        if (mudou) novo.sharedMaterials = materiaisNovos;
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
