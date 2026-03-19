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
        if (displaySpriteRenderer == null) return;

        // Se o JSON pedir "nenhum", o script desliga o ecrã virtual
        if (string.IsNullOrEmpty(nomeTela) || nomeTela.ToLower() == "nenhum")
        {
            displaySpriteRenderer.gameObject.SetActive(false);
            return;
        }

        foreach (var tela in telasDisponiveis)
        {
            if (tela.Nome.ToLower() == nomeTela.ToLower())
            {
                displaySpriteRenderer.sprite = tela.Sprite;
                
                // O Pulo do Gato: Garante que o objeto Square seja LIGADO na cena!
                displaySpriteRenderer.gameObject.SetActive(true); 
                
                Debug.Log($"[Gerenciador Visual] Display atualizado para: {nomeTela}");
                return;
            }
        }
        
        Debug.LogWarning($"[Gerenciador Visual] Tela '{nomeTela}' não encontrada na lista 'Telas Disponiveis'.");
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
                if (!string.IsNullOrEmpty(nomeEfeito) && 
                    nomeEfeito.ToLower() != "nenhum" && 
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