using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// Classe para facilitar a associação de nomes (string) com Sprites no Inspector
[System.Serializable]
public class TelaDisplay
{
    public string nome;
    public Sprite sprite;
}

// Classe para facilitar a associação de nomes (string) com GameObjects (VFX) no Inspector
[System.Serializable]
public class EfeitoVisual
{
    public string nome;
    public GameObject vfxObject;
}

public class GerenciadorVisual : MonoBehaviour
{
    [Header("Referências do Display")]
    public Image displayDoSensor; // Arraste aqui a UI Image do display do seu sensor
    public List<TelaDisplay> telasDisponiveis; // Cadastre aqui os sprites do display

    [Header("Referências de VFX")]
    public List<EfeitoVisual> efeitosDisponiveis; // Cadastre aqui os prefabs/objetos de VFX

    private Dictionary<string, Sprite> _dicionarioTelas;
    private Dictionary<string, GameObject> _dicionarioVFX;

    void Awake()
    {
        if (displayDoSensor == null)
        {
            displayDoSensor = GetComponentInChildren<Image>(); // Ou GetComponent<Image>() se estiver no mesmo GameObject
            if (displayDoSensor == null)
            {
                Debug.LogError("❌ CRÍTICO: Image do display do sensor não encontrada em GerenciadorVisual ou seus filhos!");
            }
        }
        // Converte as listas em dicionários para acesso rápido e eficiente
        _dicionarioTelas = new Dictionary<string, Sprite>();
        foreach (var tela in telasDisponiveis)
        {
            _dicionarioTelas[tela.nome] = tela.sprite;
        }

        _dicionarioVFX = new Dictionary<string, GameObject>();
        foreach (var efeito in efeitosDisponiveis)
        {
            _dicionarioVFX[efeito.nome] = efeito.vfxObject;
            // Garante que todos os efeitos comecem desativados
            if (efeito.vfxObject != null)
            {
                efeito.vfxObject.SetActive(false);
            }
        }
    }

    // Método principal que será chamado pelo GerenciarMontagem
    public void AtualizarVisuais(string nomeTela, string nomeVFX)
    {
        Debug.Log($"[GerenciadorVisual] AtualizarVisuais chamado com nomeTela: {nomeTela}, nomeVFX: {nomeVFX}");
        // 1. ATUALIZAR O DISPLAY
        if (_dicionarioTelas.ContainsKey(nomeTela))
        {
            displayDoSensor.sprite = _dicionarioTelas[nomeTela];
            displayDoSensor.gameObject.SetActive(true);
        }
        else
        {
            // Se o nome da tela for "nenhum" ou inválido, desativa o display
            displayDoSensor.gameObject.SetActive(false);
            Debug.LogWarning($"Tela de display com nome '{nomeTela}' não encontrada.");
        }

        // 2. ATUALIZAR O VFX
        // Primeiro, desativa todos os efeitos para garantir um estado limpo
        foreach (var efeito in _dicionarioVFX.Values)
        {
            if (efeito != null)
            {
                efeito.SetActive(false);
            }
        }

        // Depois, ativa apenas o efeito desejado (se não for "nenhum")
        if (nomeVFX != "nenhum" && _dicionarioVFX.ContainsKey(nomeVFX))
        {
            _dicionarioVFX[nomeVFX].SetActive(true);
        }
    }
}


