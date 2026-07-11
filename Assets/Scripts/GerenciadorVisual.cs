using System.Collections.Generic;
using UnityEngine;

public class GerenciadorVisual : MonoBehaviour
{
    [System.Serializable]
    public struct TelaSetup
    {
        public string Nome;
        public Sprite Sprite;
    }

    [System.Serializable]
    public struct VFXSetup
    {
        public string Nome;
        public GameObject VfxObject;
    }

    [Header("Referências do Display")]
    [Tooltip("Arraste o objeto Square (que agora usa SpriteRenderer) aqui")]
    public SpriteRenderer displaySpriteRenderer; 
    public List<TelaSetup> telasDisponiveis;

    [Header("Referências de VFX")]
    public List<VFXSetup> efeitosDisponiveis;

    /// <summary>
    /// Altera a imagem do display baseado no nome vindo do JSON.
    /// </summary>
    public void MudarSpriteDoSensor(string nomeTela)
    {
        DevelopmentLog.Log($"[GerenciadorVisual] Aplicando sprite '{nomeTela}'.");

        if (displaySpriteRenderer == null)
        {
            Debug.LogError("[GerenciadorVisual] Referência 'Display Sprite Renderer' não configurada.");
            return;
        }

        if (string.IsNullOrEmpty(nomeTela) || nomeTela.ToLower() == "nenhum")
        {
            displaySpriteRenderer.gameObject.SetActive(false);
            DevelopmentLog.Log("[GerenciadorVisual] Sprite vazio ou 'nenhum'; display desativado.");
            return;
        }

        if (telasDisponiveis == null || telasDisponiveis.Count == 0)
        {
            Debug.LogError("[GerenciadorVisual] Lista 'Telas Disponiveis' não configurada.");
            return;
        }

        foreach (var tela in telasDisponiveis)
        {
            // BLINDAGEM ADICIONADA: Só tenta ler se o Nome no Inspector não estiver vazio!
            if (!string.IsNullOrEmpty(tela.Nome))
            {
                if (tela.Nome.ToLower() == nomeTela.ToLower())
                {
                    displaySpriteRenderer.sprite = tela.Sprite;
                    
                    // Força o objeto e o componente a ligarem
                    displaySpriteRenderer.gameObject.SetActive(true); 
                    displaySpriteRenderer.enabled = true;
                    
                    DevelopmentLog.Log($"[GerenciadorVisual] Sprite '{nomeTela}' ativado.");
                    return;
                }
            }
        }
        
        Debug.LogWarning($"[GerenciadorVisual] Sprite '{nomeTela}' não encontrado em 'Telas Disponiveis'.");
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
