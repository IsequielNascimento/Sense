using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void LoadScenes(string cena)
    {
        SceneManager.LoadScene(cena);
    }
}
