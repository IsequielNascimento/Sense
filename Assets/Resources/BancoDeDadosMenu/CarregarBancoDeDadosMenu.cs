using UnityEngine;

public class CarregarBancoDeDadosMenu : MonoBehaviour
{
    public static MenuTextos DadosMenu { get; private set; }

    public static void Carregar(string idioma)
    {
        TextAsset arquivo = Resources.Load<TextAsset>($"BancoDeDadosMenu/banco_menu_{idioma}");

        if (arquivo != null)
        {
            Wrapper wrapper = JsonUtility.FromJson<Wrapper>(arquivo.text);
            DadosMenu = wrapper.menu;
        }
        else
        {
            Debug.LogError($"Arquivo banco_menu_{idioma}.json não encontrado!");
            DadosMenu = new MenuTextos(); // vazio
        }
    }

    [System.Serializable]
    private class Wrapper
    {
        public MenuTextos menu;
    }

    [System.Serializable]
    public class MenuTextos
    {
        public string botao_iniciar;
        public string botao_problemas;
        public string subtitulo;
    }
}
