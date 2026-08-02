using UnityEngine;

public enum TipoDeSelecaoAr
{
    Nenhuma,
    CenarioLegado,
    AlertaOficial
}

public class ProblemaSelecionadoAR : MonoBehaviour
{
    public static ProblemaSelecionadoAR Instance { get; private set; }

    public string idProblema;
    public string scenarioId;
    public string codigoOficial;
    public PassoAPasso passoAPasso;

    public string ChaveDoRecurso => IdentidadeDoCenario.Resolver(idProblema, scenarioId);

    public string CodigoOficial => IdentidadeDoCenario.Resolver(codigoOficial);

    public TipoDeSelecaoAr Tipo
    {
        get
        {
            if (IdentidadeDoCenario.EstaDefinida(ChaveDoRecurso)) return TipoDeSelecaoAr.CenarioLegado;
            if (IdentidadeDoCenario.EstaDefinida(CodigoOficial)) return TipoDeSelecaoAr.AlertaOficial;

            return TipoDeSelecaoAr.Nenhuma;
        }
    }

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
        codigoOficial = string.Empty;
    }

    public void SelecionarAlertaOficial(string codigo)
    {
        idProblema = string.Empty;
        scenarioId = string.Empty;
        codigoOficial = codigo;
    }

    public static ProblemaSelecionadoAR ObterOuCriar()
    {
        if (Instance != null) return Instance;

        new GameObject(nameof(ProblemaSelecionadoAR)).AddComponent<ProblemaSelecionadoAR>();

        return Instance;
    }
}
