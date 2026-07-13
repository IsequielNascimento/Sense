using UnityEngine;
using UnityEngine.SceneManagement;

public class NavegarParaMontagemPadrao : MonoBehaviour
{
    public void CarregarMontagemPadrao()
    {
        ControleDeCena.Instance.DefinirOrigem(OrigemCena.Montagem);

        SceneManager.LoadScene(Scenes.ArLegacy);
    }
}
