using UnityEngine;

public class ProblemaSelecionadoAR : MonoBehaviour
{
    public static ProblemaSelecionadoAR Instance { get; private set; }

    public string idProblema;
    public string scenarioId;
    public PassoAPasso passoAPasso;

    public string ChaveDoRecurso => IdentidadeDoCenario.Resolver(idProblema, scenarioId);

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

    public void Selecionar(string chaveDoRecurso, string identificadorDoCenario)
    {
        idProblema = chaveDoRecurso;
        scenarioId = identificadorDoCenario;
    }
}
