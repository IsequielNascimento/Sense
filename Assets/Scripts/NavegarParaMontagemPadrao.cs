using UnityEngine;
using UnityEngine.SceneManagement;

public class NavegarParaMontagemPadrao : MonoBehaviour
{
    public void CarregarMontagemPadrao()
    {
        ControleDeCena.Instance.DefinirOrigem("montagem");

        SceneManager.LoadScene("ARMudanca");
    }
}
