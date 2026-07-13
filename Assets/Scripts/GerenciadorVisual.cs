using System;
using System.Collections.Generic;
using UnityEngine;

public class GerenciadorVisual : MonoBehaviour
{
    [System.Serializable]
    public struct TelaSetup
    {
        public string Nome;
        public Sprite Sprite;

        [TextArea]
        public string Texto;
    }

    [System.Serializable]
    public struct VFXSetup
    {
        public string Nome;
        public GameObject VfxObject;
    }

    [Header("Referências do Display")]
    [Tooltip("Fallback legado: objeto Square que usa SpriteRenderer.")]
    public SpriteRenderer displaySpriteRenderer;

    [Tooltip("Display de texto 3D. Se vazio, será localizado automaticamente no prefab.")]
    [SerializeField] private DisplayM4 displayTexto;

    [Tooltip("Canvas antigo que exibe uma textura completa. É ocultado quando o texto dinâmico é usado.")]
    [SerializeField] private GameObject canvasDisplayLegado;

    public List<TelaSetup> telasDisponiveis;

    [Header("Referências de VFX")]
    public List<VFXSetup> efeitosDisponiveis;

    /// <summary>
    /// Aplica o estado do display baseado no identificador vindo do JSON.
    /// O texto dinâmico tem prioridade; o Sprite é mantido como fallback.
    /// </summary>
    public void MudarSpriteDoSensor(string nomeTela)
    {
        ResolverReferenciasDoDisplay();
        DevelopmentLog.Log($"[GerenciadorVisual] Aplicando estado de display '{nomeTela}'.");

        if (string.IsNullOrWhiteSpace(nomeTela) ||
            string.Equals(nomeTela, "nenhum", StringComparison.OrdinalIgnoreCase))
        {
            DesativarDisplays();
            DevelopmentLog.Log("[GerenciadorVisual] Estado vazio ou 'nenhum'; display desativado.");
            return;
        }

        if (telasDisponiveis == null || telasDisponiveis.Count == 0)
        {
            Debug.LogError("[GerenciadorVisual] Lista 'Telas Disponiveis' não configurada.");
            return;
        }

        foreach (var tela in telasDisponiveis)
        {
            if (!string.IsNullOrEmpty(tela.Nome) &&
                string.Equals(tela.Nome, nomeTela, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(tela.Texto) &&
                    displayTexto != null &&
                    displayTexto.EstaConfigurado)
                {
                    DesativarDisplaysLegados();
                    displayTexto.Mostrar(tela.Texto);
                    DevelopmentLog.Log($"[GerenciadorVisual] Texto do estado '{nomeTela}' ativado.");
                    return;
                }

                if (displaySpriteRenderer != null && tela.Sprite != null)
                {
                    displayTexto?.Limpar();
                    displaySpriteRenderer.sprite = tela.Sprite;
                    displaySpriteRenderer.gameObject.SetActive(true);
                    displaySpriteRenderer.enabled = true;
                    DevelopmentLog.Log($"[GerenciadorVisual] Sprite legado '{nomeTela}' ativado como fallback.");
                    return;
                }

                Debug.LogWarning(
                    $"[GerenciadorVisual] Estado '{nomeTela}' encontrado, mas não possui texto dinâmico utilizável nem Sprite de fallback.",
                    this);
                return;
            }
        }
        
        Debug.LogWarning($"[GerenciadorVisual] Estado '{nomeTela}' não encontrado em 'Telas Disponiveis'.");
    }

    private void ResolverReferenciasDoDisplay()
    {
        Transform raiz = transform.root;

        if (displayTexto == null || !displayTexto.EstaConfigurado)
        {
            displayTexto = DisplayM4.LocalizarOuCriar(raiz);
        }

        if (canvasDisplayLegado != null) return;

        foreach (Transform candidato in raiz.GetComponentsInChildren<Transform>(true))
        {
            if (candidato.name == "Canvas Display")
            {
                canvasDisplayLegado = candidato.gameObject;
                break;
            }
        }
    }

    private void DesativarDisplaysLegados()
    {
        if (displaySpriteRenderer != null)
        {
            displaySpriteRenderer.gameObject.SetActive(false);
        }

        if (canvasDisplayLegado != null)
        {
            canvasDisplayLegado.SetActive(false);
        }
    }

    private void DesativarDisplays()
    {
        displayTexto?.Limpar();
        DesativarDisplaysLegados();
    }

    /// <summary>
    /// Ativa o GameObject/ParticleSystem correspondente e desativa os demais.
    /// </summary>
    public void AtivarEfeito(string nomeEfeito)
    {
        if (efeitosDisponiveis == null || efeitosDisponiveis.Count == 0) return;

        bool efeitoEncontrado = false;

        foreach (var efeito in efeitosDisponiveis)
        {
            if (efeito.VfxObject != null)
            {
                // BLINDAGEM ADICIONADA: Verifica se o campo Nome do VFX no Inspector não está vazio
                if (!string.IsNullOrEmpty(nomeEfeito) && 
                    nomeEfeito.ToLower() != "nenhum" && 
                    !string.IsNullOrEmpty(efeito.Nome) && 
                    efeito.Nome.ToLower() == nomeEfeito.ToLower())
                {
                    efeito.VfxObject.SetActive(true);
                    efeitoEncontrado = true;
                    DevelopmentLog.Log($"[GerenciadorVisual] VFX '{nomeEfeito}' ativado.");
                }
                else
                {
                    efeito.VfxObject.SetActive(false);
                }
            }
        }

        if (!efeitoEncontrado && !string.IsNullOrEmpty(nomeEfeito) && nomeEfeito.ToLower() != "nenhum")
        {
            Debug.LogWarning($"[GerenciadorVisual] VFX '{nomeEfeito}' não encontrado em 'Efeitos Disponiveis'.");
        }
    }
}
