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
    public const string CodigoA21 = "A21";
    public const string CodigoA22 = "A22";
    public const string CodigoA1 = "A1";
    public const string CodigoA2 = "A2";
    public const string CodigoA3 = "A3";
    public const string CodigoA4 = "A4";
    public const string CodigoA5 = "A5";
    public const string CodigoA9 = "A9";

    private static readonly HashSet<string> Concluidas =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            CodigoA8,
            CodigoA11,
            CodigoA12,
            CodigoA13,
            CodigoA14,
            CodigoA21,
            CodigoA22,
            CodigoA1,
            CodigoA2,
            CodigoA3,
            CodigoA4,
            CodigoA5,
            CodigoA9,
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
