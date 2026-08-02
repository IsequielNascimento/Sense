using System;
using System.Collections.Generic;

public static class BuscaDeAlertas
{
    #region MARK: Busca

    public static IReadOnlyList<AlertaOficial> Buscar(IReadOnlyList<AlertaOficial> catalogo, string termo)
    {
        if (catalogo == null) return Array.Empty<AlertaOficial>();
        if (string.IsNullOrWhiteSpace(termo)) return catalogo;

        string procurado = termo.Trim().ToLowerInvariant();

        var prioritarios = new List<AlertaOficial>();
        var demais = new List<AlertaOficial>();

        foreach (AlertaOficial alerta in catalogo)
        {
            if (!Corresponde(alerta, procurado)) continue;

            if (ComecaCom(alerta.Codigo, procurado) || ComecaCom(alerta.Nome, procurado))
            {
                prioritarios.Add(alerta);
            }
            else
            {
                demais.Add(alerta);
            }
        }

        prioritarios.AddRange(demais);
        return prioritarios;
    }

    #endregion

    #region MARK: Correspondencia

    private static bool Corresponde(AlertaOficial alerta, string procurado)
    {
        if (Contem(alerta.Codigo, procurado)) return true;
        if (Contem(alerta.Nome, procurado)) return true;
        if (Contem(alerta.OQueE, procurado)) return true;
        if (Contem(alerta.QuandoOcorre, procurado)) return true;

        foreach (string acao in alerta.Acoes)
        {
            if (Contem(acao, procurado)) return true;
        }

        return false;
    }

    private static bool Contem(string texto, string procurado)
    {
        return !string.IsNullOrEmpty(texto) && texto.ToLowerInvariant().Contains(procurado);
    }

    private static bool ComecaCom(string texto, string procurado)
    {
        return !string.IsNullOrEmpty(texto) && texto.ToLowerInvariant().StartsWith(procurado, StringComparison.Ordinal);
    }

    #endregion
}
