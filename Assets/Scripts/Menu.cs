using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void LoadScenes(string cena)
    {
        if (cena == Scenes.ArLegacy)
        {
            ControleDeCena.Instance.DefinirOrigem(OrigemCena.Montagem);

        }

        SceneManager.LoadScene(cena);
    }
}
