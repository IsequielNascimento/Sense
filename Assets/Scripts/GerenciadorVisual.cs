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
        Debug.Log($"[Gerenciador Visual] TENTATIVA: Recebeu ordem do JSON para mostrar a tela -> '{nomeTela}'");

        if (displaySpriteRenderer == null)
        {
            Debug.LogError("[Gerenciador Visual] ERRO CRÍTICO: O campo 'Display Sprite Renderer' está VAZIO! O script não sabe quem é o Square.");
            return;
        }

        if (string.IsNullOrEmpty(nomeTela) || nomeTela.ToLower() == "nenhum")
        {
            displaySpriteRenderer.gameObject.SetActive(false);
            Debug.Log("[Gerenciador Visual] Comando vazio ou 'nenhum'. Desligando a tela virtual.");
            return;
        }

        if (telasDisponiveis == null || telasDisponiveis.Count == 0)
        {
            Debug.LogError("[Gerenciador Visual] ERRO CRÍTICO: A sua lista de 'Telas Disponiveis' (as 16 imagens) está VAZIA neste clone do Prefab!");
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
                    
                    Debug.Log($"[Gerenciador Visual] SUCESSO ABSOLUTO: A tela '{nomeTela}' foi encontrada na lista e ativada no modelo!");
                    return;
                }
            }
        }
        
        Debug.LogWarning($"[Gerenciador Visual] AVISO: A tela chamada '{nomeTela}' chegou do JSON, mas NÃO FOI ENCONTRADA na sua lista. Verifique se os nomes estão idênticos.");
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
                    Debug.Log($"[Gerenciador Visual] VFX Ativado: {nomeEfeito}");
                }
                else
                {
                    efeito.VfxObject.SetActive(false);
                }
            }
        }

        if (!efeitoEncontrado && !string.IsNullOrEmpty(nomeEfeito) && nomeEfeito.ToLower() != "nenhum")
        {
            Debug.LogWarning($"[Gerenciador Visual] Efeito VFX '{nomeEfeito}' não encontrado na lista.");
        }
    }
}