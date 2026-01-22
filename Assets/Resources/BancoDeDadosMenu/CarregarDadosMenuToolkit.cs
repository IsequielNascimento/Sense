using UnityEngine;

public class CarregarDadosMenuToolkit : MonoBehaviour
{
    // Acesso estático aos dados carregados
    public static MenuTextosToolkit DadosMenu { get; private set; }

    public static void Carregar(string idioma)
    {
        // Carrega o arquivo da pasta Resources/BancoDeDadosMenu
        TextAsset arquivo = Resources.Load<TextAsset>($"BancoDeDadosMenu/banco_menu_{idioma}");

        if (arquivo != null)
        {
            WrapperToolkit wrapper = JsonUtility.FromJson<WrapperToolkit>(arquivo.text);
            DadosMenu = wrapper.menu;
        }
        else
        {
            Debug.LogError($"[Toolkit] Erro: Arquivo banco_menu_{idioma}.json não encontrado em Resources!");
            DadosMenu = new MenuTextosToolkit(); 
        }
    }

    [System.Serializable]
    private class WrapperToolkit
    {
        public MenuTextosToolkit menu;
    }

    [System.Serializable]
    public class MenuTextosToolkit
    {
        public string botao_iniciar;
        public string botao_problemas;
        public string botao_gemeo; // Novo campo adicionado
        public string subtitulo;
    }
}