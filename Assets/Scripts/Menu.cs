using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void LoadScenes(string cena)
    {
        if (cena == "ARMudanca")
        {
            ControleDeCena.Instance.DefinirOrigem("montagem");

            string idioma = IdiomaManager.Instance.ObterIdioma();
            CarregarBancoDeDadosMontagem.Carregar(idioma);
        }

        SceneManager.LoadScene(cena);
    }
}
