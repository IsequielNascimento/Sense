using UnityEngine;

/// <summary>
/// Lógica comum de exibição do modelo M4, compartilhada entre o modo AR
/// (PlaceOnPlane_Adaptado) e o modo visualizador 3D (Visualizador3D):
/// referências ao prefab/UIController, inicialização dos Animators e do
/// GerenciadorVisual do modelo instanciado e reprodução das animações dos passos.
/// </summary>
public abstract class ExibidorDeModeloBase : MonoBehaviour
{
    [Header("Modelo")]
    [SerializeField] protected GameObject placedPrefab;

    [Header("Conexão com UI Toolkit")]
    [SerializeField] protected UIController uiController;

    protected GameObject spawnedObject;
    protected Animator[] animators;
    protected GerenciadorVisual gerenciadorVisual;

    /// <summary>
    /// Configura o modelo recém-instanciado (Animators, GerenciadorVisual, atuador).
    /// Chamar logo após atribuir spawnedObject via Instantiate.
    /// </summary>
    protected void ConfigurarModeloInstanciado()
    {
        animators = spawnedObject.GetComponentsInChildren<Animator>();
        gerenciadorVisual = spawnedObject.GetComponentInChildren<GerenciadorVisual>();

        ForcarAtivacaoAtuador("após Instantiate");

        if (animators == null || animators.Length == 0)
        {
            Debug.LogError($"[{GetType().Name}] CRÍTICO: Nenhum Animator encontrado no prefab instanciado!");
            return;
        }

        foreach (var anim in animators)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        ForcarAtivacaoAtuador("após Rebind/Update");
    }

    /// <summary>
    /// Ajuste de posição específico do modo antes de tocar a animação do passo.
    /// O AR reposiciona o modelo sobre o plano detectado; o visualizador não
    /// precisa ajustar nada (modelo fixo na origem).
    /// </summary>
    protected virtual void AjustarPosicaoParaPasso(bool isMontagem) { }

    public void PlayAnimation(string animName, string camadaAlvo, string telaDisplay, string vfx)
    {
        ForcarAtivacaoAtuador($"PlayAnimation('{animName}')");

        bool isMontagem = string.IsNullOrEmpty(camadaAlvo) || camadaAlvo == "Base Layer";

        AjustarPosicaoParaPasso(isMontagem);

        if (spawnedObject != null)
        {
            // Animator pai (Animação APK):
            // - Em Montagem: DESLIGADO. Os clipes animacao_X mexeriam em transforms via "path: Atuador"
            //   etc. mas com WriteDefaultValues=1 o estado Parado estava sobrescrevendo a posição/rotação
            //   ajustadas no prefab. O Atuador continua visível porque é forçado ativo programaticamente
            //   (ForcarAtivacaoAtuador), e os outros parts ficam nas posições do prefab (que você corrigiu).
            // - Em Problemas: LIGADO. Necessário para os clipes Ax_pY animarem a Chave Verde N
            //   (scale, posição, rotação) via "path: Chave Verde N" como filho direto da raiz.
            Animator animatorPai = spawnedObject.GetComponent<Animator>();
            if (animatorPai != null)
            {
                animatorPai.enabled = !isMontagem;
            }
        }

        // 1. LÓGICA DE ANIMAÇÃO
        if (animators != null && animators.Length > 0)
        {
            if (string.IsNullOrEmpty(camadaAlvo)) camadaAlvo = "Base Layer";
            int hashDaAnimacao = Animator.StringToHash(animName);
            bool tocouEmPeloMenosUm = false;

            foreach (var anim in animators)
            {
                if (!anim.enabled) continue;

                int layerIndex = anim.GetLayerIndex(camadaAlvo);
                if (layerIndex != -1 && anim.HasState(layerIndex, hashDaAnimacao))
                {
                    anim.speed = 1f;

                    for (int i = 1; i < anim.layerCount; i++)
                    {
                        anim.SetLayerWeight(i, (i == layerIndex) ? 1f : 0f);
                    }

                    anim.Play(hashDaAnimacao, layerIndex, 0f);
                    tocouEmPeloMenosUm = true;
                }
            }

            if (tocouEmPeloMenosUm)
            {
                Debug.Log($"[AR Toolkit] Sucesso: Animação '{animName}' tocada na camada '{camadaAlvo}'.");
            }
            else
            {
                Debug.LogWarning($"[AR Toolkit] AVISO: O estado '{animName}' não foi encontrado nos Animators ATIVOS para a camada '{camadaAlvo}'.");
            }
        }

        // 2. LÓGICA VISUAL
        if (gerenciadorVisual != null)
        {
            gerenciadorVisual.MudarSpriteDoSensor(telaDisplay);
            gerenciadorVisual.AtivarEfeito(vfx);
        }
    }

    protected void ForcarAtivacaoAtuador(string contexto)
    {
        if (spawnedObject == null) return;

        int ativados = 0;
        int jaAtivos = 0;
        foreach (var t in spawnedObject.GetComponentsInChildren<Transform>(true))
        {
            if (t.name != "Atuador") continue;
            if (!t.gameObject.activeSelf)
            {
                t.gameObject.SetActive(true);
                ativados++;
            }
            else
            {
                jaAtivos++;
            }
        }
        Debug.Log($"[{GetType().Name}] ForcarAtivacaoAtuador ({contexto}): {ativados} ativado(s), {jaAtivos} já ativo(s).");
    }
}
