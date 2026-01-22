using UnityEngine;
using UnityEngine.SceneManagement;

public class NavegarParaMontagemPadrao : MonoBehaviour
{
    public void CarregarMontagemPadrao()
    {
        ControleDeCena.Instance.DefinirOrigem("montagem");

        string idioma = IdiomaManager.Instance.ObterIdioma();
        CarregarBancoDeDadosMontagem.Carregar(idioma);

        SceneManager.LoadScene("ARMudanca");
    }
}
