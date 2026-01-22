using UnityEngine;

public class ProblemaSelecionadoAR : MonoBehaviour
{
    public static ProblemaSelecionadoAR Instance { get; private set; }

    public string idProblema;
    public PassoAPasso passoAPasso;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}