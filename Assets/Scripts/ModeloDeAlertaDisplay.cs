using System;
using System.Collections.Generic;
using UnityEngine;

public static class ModeloDeAlertaDisplay
{
    #region MARK: Mapeamento centralizado de codigo para recurso M4SMARTTeste

    public const string CodigoA8 = "A8";
    public const string CodigoA11 = "A11";
    public const string CodigoA12 = "A12";
    public const string CodigoA13 = "A13";

    private static readonly IReadOnlyDictionary<string, string> RecursoPorCodigo =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { CodigoA8, "M4Problem1/M4SMARTTesteProblema1" },
            { CodigoA11, "M4Problem11/M4SMARTTesteProblema11" },
            { CodigoA12, "M4Problem12/M4SMARTTesteProblema12" },
            { CodigoA13, "M4Problem13/M4SMARTTesteProblema13" },
        };

    #endregion

    #region MARK: Resolucao

    public static bool UsaM4SmartTeste(string codigoOficial)
    {
        return !string.IsNullOrEmpty(codigoOficial) && RecursoPorCodigo.ContainsKey(codigoOficial);
    }

    public static GameObject Resolver(string codigoOficial)
    {
        if (string.IsNullOrEmpty(codigoOficial)) return null;

        return RecursoPorCodigo.TryGetValue(codigoOficial, out string recurso)
            ? Resources.Load<GameObject>(recurso)
            : null;
    }

    #endregion
}
