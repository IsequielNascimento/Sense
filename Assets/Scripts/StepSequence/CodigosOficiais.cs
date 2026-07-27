using System;
using System.Collections.Generic;

public static class CodigosOficiais
{
    #region MARK - Tabela 3 do manual, página 11

    public static readonly IReadOnlyList<string> Alertas = new[]
    {
        "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A9",
        "A11", "A12", "A13", "A14", "A15", "A16", "A17", "A18",
        "A19", "A20", "A21", "A22", "A23", "A24", "A25",
    };

    public static readonly IReadOnlyList<string> Configuracoes = new[]
    {
        "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10",
        "C11", "C12", "C13", "C14", "C15", "C16", "C17", "C18", "C19",
        "C20", "C21", "C22", "C23",
    };

    #endregion

    #region MARK - Consulta

    private static readonly HashSet<string> Todos = CriarIndice();

    public static bool EhValido(string codigo)
    {
        return !string.IsNullOrWhiteSpace(codigo) && Todos.Contains(codigo.Trim());
    }

    public static bool EhAlerta(string codigo)
    {
        return EhValido(codigo) && codigo.Trim().StartsWith("A", StringComparison.Ordinal);
    }

    private static HashSet<string> CriarIndice()
    {
        var indice = new HashSet<string>(StringComparer.Ordinal);

        foreach (string codigo in Alertas) indice.Add(codigo);
        foreach (string codigo in Configuracoes) indice.Add(codigo);

        return indice;
    }

    #endregion
}
