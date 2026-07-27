using UnityEngine;

public abstract class ExibidorDeModeloBase : MonoBehaviour
{
    #region MARK - Referências

    [Header("Modelo")]
    [SerializeField] protected GameObject placedPrefab;

    [Header("Conexão com UI Toolkit")]
    [SerializeField] protected UIController uiController;

    public GameObject PrefabDoModelo => placedPrefab;

    protected GameObject spawnedObject;
    protected Animator[] animators;
    protected GerenciadorVisual gerenciadorVisual;

    #endregion

    #region MARK - Ciclo de vida do modelo

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

    protected virtual void AjustarPosicaoParaPasso(bool isMontagem) { }

    #endregion

    #region MARK - Reprodução de etapa

    public void PlayAnimation(Etapa etapa, string camadaAlvo)
    {
        etapa ??= new Etapa();
        string animName = etapa.animacao ?? string.Empty;
        bool isMontagem = string.IsNullOrEmpty(camadaAlvo) || camadaAlvo == ArConstants.DefaultAnimatorLayer;

        AjustarPosicaoParaPasso(isMontagem);

        if (spawnedObject != null)
        {
            Animator animatorPai = spawnedObject.GetComponent<Animator>();
            if (animatorPai != null)
            {
                animatorPai.enabled = !isMontagem;
            }
        }

        if (animators != null && animators.Length > 0)
        {
            if (string.IsNullOrEmpty(camadaAlvo)) camadaAlvo = ArConstants.DefaultAnimatorLayer;
            int hashDaAnimacao = Animator.StringToHash(animName);
            bool tocouEmPeloMenosUm = false;

            foreach (var anim in animators)
            {
                if (!anim.enabled) continue;

                int layerIndex = anim.GetLayerIndex(camadaAlvo);
                bool estadoExiste = layerIndex != PlanoDeCamadas.CamadaInexistente
                    && anim.HasState(layerIndex, hashDaAnimacao);
                int camadaComEstado = PlanoDeCamadas.CamadaComEstado(layerIndex, estadoExiste);

                for (int i = PlanoDeCamadas.PrimeiraCamadaDeProblema; i < anim.layerCount; i++)
                {
                    anim.SetLayerWeight(i, PlanoDeCamadas.PesoDaCamadaDeProblema(i, camadaComEstado));
                }

                if (!estadoExiste) continue;

                anim.speed = 1f;
                anim.Play(hashDaAnimacao, layerIndex, 0f);
                tocouEmPeloMenosUm = true;
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

        if (gerenciadorVisual != null)
        {
            gerenciadorVisual.MudarSpriteDoSensor(etapa.telaDisplay);
            gerenciadorVisual.AtivarEfeito(etapa.vfx);
            gerenciadorVisual.AplicarCamadasDinamicas(etapa);
        }
    }

    #endregion
}
