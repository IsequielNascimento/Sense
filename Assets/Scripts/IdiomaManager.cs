using UnityEngine;

public class IdiomaManager : MonoBehaviour
{
    public static IdiomaManager Instance { get; private set; }
    public string idiomaAtual = "pt";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            idiomaAtual = PlayerPrefs.GetString("idioma", "pt");
        }
    }

    public void DefinirIdioma(string codigo)
    {
        idiomaAtual = codigo;
        PlayerPrefs.SetString("idioma", codigo);
        PlayerPrefs.Save();
        Debug.Log("Idioma GLOBAL atualizado: " + idiomaAtual);
    }

    public string ObterIdioma()
    {
        return idiomaAtual;
    }
}
