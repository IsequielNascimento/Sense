using UnityEngine;

public static class ModeloDeAlertaDisplay
{
    public const string CodigoA8 = "A8";

    private const string RecursoM4SmartTeste = "M4Problem1/M4SMARTTesteProblema1";

    public static bool UsaM4SmartTeste(string codigoOficial)
    {
        return string.Equals(codigoOficial, CodigoA8, System.StringComparison.OrdinalIgnoreCase);
    }

    public static GameObject Resolver(string codigoOficial)
    {
        return UsaM4SmartTeste(codigoOficial)
            ? Resources.Load<GameObject>(RecursoM4SmartTeste)
            : null;
    }
}
