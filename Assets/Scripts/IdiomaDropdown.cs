using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class IdiomaDropdown : MonoBehaviour
{
    [System.Serializable]
    public class IdiomaOpcao
    {
        public string codigo;              
        public Button botao;               
        public Sprite imagemBotaoSelecionado;  
    }

    [Header("Referências")]
    public Image imagemSelecionada;        
    public GameObject painelOpcoes;        
    public List<IdiomaOpcao> opcoes;       

    private string idiomaAtual;
    private GerenciarMenu gerenciarMenu;

    void Start()
    {
        painelOpcoes.SetActive(false);
        gerenciarMenu = FindObjectOfType<GerenciarMenu>();

        // 🟢 PEGA O IDIOMA SALVO GLOBALMENTE
        idiomaAtual = IdiomaManager.Instance.ObterIdioma();

        foreach (var opcao in opcoes)
        {
            string codigo = opcao.codigo;
            opcao.botao.onClick.AddListener(() => SelecionarIdioma(codigo));
        }

        // 🟢 Atualiza o visual do botão principal conforme o idioma atual
        AtualizarVisualBotaoSelecionado(idiomaAtual);

        // 🟢 Atualiza os textos do Menu com o idioma atual
        if (gerenciarMenu != null)
        {
            gerenciarMenu.TrocarIdioma(idiomaAtual);
        }
    }

    public void AlternarPainel()
    {
        painelOpcoes.SetActive(!painelOpcoes.activeSelf);
    }

    public void SelecionarIdioma(string codigo)
    {
        idiomaAtual = codigo;

        // 🟢 SALVA O IDIOMA GLOBALMENTE
        IdiomaManager.Instance.DefinirIdioma(idiomaAtual);

        AtualizarVisualBotaoSelecionado(idiomaAtual);

        painelOpcoes.SetActive(false);

        if (gerenciarMenu != null)
        {
            gerenciarMenu.TrocarIdioma(idiomaAtual);
        }
    }

    private void AtualizarVisualBotaoSelecionado(string codigo)
    {
        foreach (var opcao in opcoes)
        {
            if (opcao.codigo == codigo)
            {
                imagemSelecionada.sprite = opcao.imagemBotaoSelecionado;
            }
        }
    }
}
