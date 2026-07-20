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

    public GameObject PrefabDoModelo => placedPrefab;

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

        if (animators == null || animators.Length == 0)
        {
            Debug.LogError($"[{GetType().Name}] Nenhum Animator encontrado no prefab instanciado.");
            return;
        }

        foreach (var anim in animators)
        {
            anim.Rebind();
            anim.Update(0f);
        }

    }

    /// <summary>
    /// Ajuste de posição específico do modo antes de tocar a animação do passo.
    /// O AR reposiciona o modelo sobre o plano detectado; o visualizador não
    /// precisa ajustar nada (modelo fixo na origem).
    /// </summary>
    protected virtual void AjustarPosicaoParaPasso(bool isMontagem) { }

    public void PlayAnimation(Etapa etapa, string camadaAlvo)
    {
        etapa ??= new Etapa();
        string animName = etapa.animacao ?? string.Empty;
        bool isMontagem = string.IsNullOrEmpty(camadaAlvo) || camadaAlvo == ArConstants.DefaultAnimatorLayer;

        AjustarPosicaoParaPasso(isMontagem);

        if (spawnedObject != null)
        {
            // O Animator da raiz pertence às sequências de problemas. Na montagem, o Animator
            // do modelo aninhado é o único responsável pelos clips animacao_X. A visibilidade
            // tutorial é propriedade dos clips; a visibilidade estrutural vem do prefab.
            Animator animatorPai = spawnedObject.GetComponent<Animator>();
            if (animatorPai != null)
            {
                animatorPai.enabled = !isMontagem;
            }
        }

        // 1. LÓGICA DE ANIMAÇÃO
        if (animators != null && animators.Length > 0)
        {
            if (string.IsNullOrEmpty(camadaAlvo)) camadaAlvo = ArConstants.DefaultAnimatorLayer;
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
                DevelopmentLog.Log($"[ExibidorDeModeloBase] Animação '{animName}' iniciada na camada '{camadaAlvo}'.");
            }
            else
            {
                Debug.LogWarning($"[ExibidorDeModeloBase] Estado '{animName}' não encontrado nos Animators ativos para a camada '{camadaAlvo}'.");
            }
        }

        // 2. LÓGICA VISUAL
        if (gerenciadorVisual != null)
        {
            gerenciadorVisual.MudarSpriteDoSensor(etapa.telaDisplay);
            gerenciadorVisual.AtivarEfeito(etapa.vfx);
            gerenciadorVisual.AplicarCamadasDinamicas(etapa);
        }
    }

}
