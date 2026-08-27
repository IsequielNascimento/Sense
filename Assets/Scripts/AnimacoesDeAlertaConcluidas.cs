using System;
using System.Collections.Generic;

public static class AnimacoesDeAlertaConcluidas
{
    #region MARK: Registro das animacoes ja finalizadas

    public const string CodigoA8 = "A8";
    public const string CodigoA11 = "A11";
    public const string CodigoA12 = "A12";
    public const string CodigoA13 = "A13";
    public const string CodigoA14 = "A14";
    public const string CodigoA19 = "A19";
    public const string CodigoA20 = "A20";
    public const string CodigoA21 = "A21";
    public const string CodigoA22 = "A22";
    public const string CodigoA1 = "A1";
    public const string CodigoA2 = "A2";
    public const string CodigoA3 = "A3";
    public const string CodigoA4 = "A4";
    public const string CodigoA5 = "A5";
    public const string CodigoA6 = "A6";
    public const string CodigoA9 = "A9";
    public const string CodigoA23 = "A23";
    public const string CodigoA24 = "A24";
    public const string CodigoA25 = "A25";
    public const string CodigoA17 = "A17";
    public const string CodigoA18 = "A18";
    public const string CodigoA15 = "A15";
    public const string CodigoA16 = "A16";
    public const string CodigoA7 = "A7";

    private static readonly HashSet<string> Concluidas =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            CodigoA8,
            CodigoA11,
            CodigoA12,
            CodigoA13,
            CodigoA14,
            CodigoA19,
            CodigoA20,
            CodigoA21,
            CodigoA22,
            CodigoA1,
            CodigoA2,
            CodigoA3,
            CodigoA4,
            CodigoA5,
            CodigoA6,
            CodigoA9,
            CodigoA23,
            CodigoA24,
            CodigoA25,
            CodigoA17,
            CodigoA18,
            CodigoA15,
            CodigoA16,
            CodigoA7,
        };

    #endregion

    #region MARK: Consulta

    public static bool EstaConcluida(string codigoOficial)
    {
        return !string.IsNullOrWhiteSpace(codigoOficial) && Concluidas.Contains(codigoOficial.Trim());
    }

    public static IReadOnlyCollection<string> Codigos => Concluidas;

    #endregion
}
