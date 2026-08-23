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
    public const string CodigoA14 = "A14";

    private static readonly IReadOnlyDictionary<string, string> RecursoPorCodigo =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { CodigoA8, "M4Problem1/M4SMARTTesteProblema1" },
            { CodigoA11, "M4Problem11/M4SMARTTesteProblema11" },
            { CodigoA12, "M4Problem12/M4SMARTTesteProblema12" },
            { CodigoA13, "M4Problem13/M4SMARTTesteProblema13" },
            { CodigoA14, "M4Problem14/M4SMARTTesteProblema14" },
            { "A1", "M4ProblemA1/M4SMARTTesteProblemaA1" },
            { "A2", "M4ProblemA2/M4SMARTTesteProblemaA2" },
            { "A3", "M4ProblemA3/M4SMARTTesteProblemaA3" },
            { "A4", "M4ProblemA4/M4SMARTTesteProblemaA4" },
            { "A5", "M4ProblemA5/M4SMARTTesteProblemaA5" },
            { "A6", "M4ProblemA6/M4SMARTTesteProblemaA6" },
            { "A7", "M4ProblemA7/M4SMARTTesteProblemaA7" },
            { "A9", "M4ProblemA9/M4SMARTTesteProblemaA9" },
            { "A15", "M4ProblemA15/M4SMARTTesteProblemaA15" },
            { "A16", "M4ProblemA16/M4SMARTTesteProblemaA16" },
            { "A17", "M4ProblemA17/M4SMARTTesteProblemaA17" },
            { "A18", "M4ProblemA18/M4SMARTTesteProblemaA18" },
            { "A19", "M4ProblemA19/M4SMARTTesteProblemaA19" },
            { "A20", "M4ProblemA20/M4SMARTTesteProblemaA20" },
            { "A21", "M4ProblemA21/M4SMARTTesteProblemaA21" },
            { "A22", "M4ProblemA22/M4SMARTTesteProblemaA22" },
            { "A23", "M4ProblemA23/M4SMARTTesteProblemaA23" },
            { "A24", "M4ProblemA24/M4SMARTTesteProblemaA24" },
            { "A25", "M4ProblemA25/M4SMARTTesteProblemaA25" },
        };

    #endregion

    #region MARK: Resolucao

    public static IEnumerable<string> TodosOsRecursos => RecursoPorCodigo.Values;

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
