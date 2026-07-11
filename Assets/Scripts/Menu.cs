using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void LoadScenes(string cena)
    {
        if (cena == "ARMudanca")
        {
            ControleDeCena.Instance.DefinirOrigem("montagem");

        }

        SceneManager.LoadScene(cena);
    }
}
