using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ExibidorDeModeloBase : MonoBehaviour
{
    [Serializable]
    private sealed class ModeloPorCenario
    {
        public string cenario;
        public GameObject prefab;
    }

    #region MARK - Referências

    [Header("Modelo")]
    [SerializeField] protected GameObject placedPrefab;
    [SerializeField] private List<ModeloPorCenario> modelosPorCenario = new();

    [Header("Conexão com UI Toolkit")]
    [SerializeField] protected UIController uiController;

    public GameObject PrefabDoModelo => ResolverPrefabDoModelo();

    protected GameObject spawnedObject;
    protected Animator[] animators;
    protected GerenciadorVisual gerenciadorVisual;

    #endregion

    #region MARK - Seleção do modelo

    protected GameObject ResolverPrefabDoModelo()
    {
        ProblemaSelecionadoAR selecao = ProblemaSelecionadoAR.Instance;
        string cenario = IdentidadeDoCenario.Resolver(selecao?.scenarioId, selecao?.ChaveDoRecurso);

        if (!string.IsNullOrEmpty(cenario) && modelosPorCenario != null)
        {
            foreach (ModeloPorCenario modelo in modelosPorCenario)
            {
                if (modelo?.prefab != null &&
                    string.Equals(modelo.cenario, cenario, StringComparison.OrdinalIgnoreCase))
                {
                    return modelo.prefab;
                }
            }
        }

        return placedPrefab;
    }

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
        bool possuiAnimacao = DecisaoDeEtapaAr.PossuiAnimacao(animName);
        bool isMontagem = DecisaoDeEtapaAr.EhMontagem(animName, camadaAlvo, ArConstants.DefaultAnimatorLayer);

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
            int hashDaAnimacao = possuiAnimacao ? Animator.StringToHash(animName) : 0;
            bool tocouEmPeloMenosUm = false;

            foreach (var anim in animators)
            {
                if (!anim.enabled) continue;

                int layerIndex = anim.GetLayerIndex(camadaAlvo);
                bool estadoExiste = possuiAnimacao
                    && layerIndex != PlanoDeCamadas.CamadaInexistente
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

            if (possuiAnimacao)
            {
                if (tocouEmPeloMenosUm)
                {
                    DevelopmentLog.Log($"[ExibidorDeModeloBase] Animação '{animName}' iniciada na camada '{camadaAlvo}'.");
                }
                else
                {
                    Debug.LogWarning($"[ExibidorDeModeloBase] Estado '{animName}' não encontrado nos Animators ativos para a camada '{camadaAlvo}'.");
                }
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
