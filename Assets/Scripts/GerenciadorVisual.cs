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
    }

    [System.Serializable]
    public struct VFXSetup
    {
        public string Nome;
        public GameObject VfxObject;
    }

    [Header("Referências do Display")]
    public SpriteRenderer displaySpriteRenderer;
    public List<TelaSetup> telasDisponiveis;
    [SerializeField] private Transform ancoraDisplayDinamico;
    [SerializeField] private Vector2 dimensoesDisplayDinamico = new Vector2(0.038032f, 0.042522635f);
    [SerializeField] private string sortingLayerDisplayDinamico = "Default";
    [SerializeField] private int sortingOrderDisplayDinamico = 10;

    [Header("Referências de VFX")]
    public List<VFXSetup> efeitosDisponiveis;

    private TelaM4 telaM4;

    public void MudarSpriteDoSensor(string nomeTela)
    {
        if (displaySpriteRenderer == null)
        {
            if (ancoraDisplayDinamico != null) return;
            Debug.LogError("[GerenciadorVisual] 'displaySpriteRenderer' não configurado.", this);
            return;
        }

        if (string.IsNullOrEmpty(nomeTela) || nomeTela.ToLower() == "nenhum")
        {
            displaySpriteRenderer.gameObject.SetActive(false);
            return;
        }

        if (telasDisponiveis == null || telasDisponiveis.Count == 0)
        {
            Debug.LogError("[GerenciadorVisual] 'telasDisponiveis' está vazia.", this);
            return;
        }

        foreach (var tela in telasDisponiveis)
        {
            if (!string.IsNullOrEmpty(tela.Nome) && tela.Nome.ToLower() == nomeTela.ToLower())
            {
                displaySpriteRenderer.sprite = tela.Sprite;
                displaySpriteRenderer.gameObject.SetActive(true);
                displaySpriteRenderer.enabled = true;
                return;
            }
        }

        Debug.LogWarning($"[GerenciadorVisual] Tela '{nomeTela}' não encontrada em 'telasDisponiveis'.", this);
    }

    public void AplicarCamadasDinamicas(Etapa etapa)
    {
        if (displaySpriteRenderer == null && ancoraDisplayDinamico == null) return;

        bool temConteudo = TelaM4.EtapaTemConteudo(etapa);

        if (telaM4 == null)
        {
            if (!temConteudo) return;
            telaM4 = displaySpriteRenderer != null
                ? TelaM4.CriarEm(displaySpriteRenderer)
                : TelaM4.CriarEm(
                    ancoraDisplayDinamico,
                    dimensoesDisplayDinamico,
                    SortingLayer.NameToID(sortingLayerDisplayDinamico),
                    sortingOrderDisplayDinamico);
        }

        if (displaySpriteRenderer != null && temConteudo && !displaySpriteRenderer.gameObject.activeSelf)
        {
            displaySpriteRenderer.gameObject.SetActive(true);
            displaySpriteRenderer.enabled = false;
        }

        telaM4.Aplicar(etapa);
    }

    public void AtivarEfeito(string nomeEfeito)
    {
        if (efeitosDisponiveis == null || efeitosDisponiveis.Count == 0) return;

        bool efeitoEncontrado = false;

        foreach (var efeito in efeitosDisponiveis)
        {
            if (efeito.VfxObject == null) continue;

            bool ehEsteEfeito = !string.IsNullOrEmpty(nomeEfeito) &&
                nomeEfeito.ToLower() != "nenhum" &&
                !string.IsNullOrEmpty(efeito.Nome) &&
                efeito.Nome.ToLower() == nomeEfeito.ToLower();

            efeito.VfxObject.SetActive(ehEsteEfeito);
            if (ehEsteEfeito) efeitoEncontrado = true;
        }

        if (!efeitoEncontrado && !string.IsNullOrEmpty(nomeEfeito) && nomeEfeito.ToLower() != "nenhum")
        {
            Debug.LogWarning($"[GerenciadorVisual] VFX '{nomeEfeito}' não encontrado em 'efeitosDisponiveis'.", this);
        }
    }
}
