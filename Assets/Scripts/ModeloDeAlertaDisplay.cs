using System;
using UnityEngine;

public static class ModeloDeAlertaDisplay
{
    public const string CodigoA8 = "A8";
    public const string CodigoA12 = "A12";
    public const string CodigoA13 = "A13";
    public const string CodigoA14 = "A14";

    private const string RecursoM4SmartTeste = "M4Problem1/M4SMARTTesteProblema1";

    private static readonly string[] CodigosComM4SmartTeste =
    {
        CodigoA8,
        CodigoA12,
        CodigoA13,
        CodigoA14,
    };

    public static bool UsaM4SmartTeste(string codigoOficial)
    {
        foreach (string codigo in CodigosComM4SmartTeste)
        {
            if (string.Equals(codigoOficial, codigo, StringComparison.OrdinalIgnoreCase)) return true;
        }

        return false;
    }

    public static GameObject Resolver(string codigoOficial)
    {
        return UsaM4SmartTeste(codigoOficial)
            ? Resources.Load<GameObject>(RecursoM4SmartTeste)
            : null;
    }
}
