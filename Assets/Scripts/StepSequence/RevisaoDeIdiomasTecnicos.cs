using System;
using System.Collections.Generic;

public enum EstadoDeRevisao
{
    Aprovado,
    PendenteRevisao,
}

public static class RevisaoDeIdiomasTecnicos
{
    #region MARK: Registro

    public const string IdiomaAprovado = "pt";

    private static readonly Dictionary<string, EstadoDeRevisao> Estados = new Dictionary<string, EstadoDeRevisao>(StringComparer.Ordinal)
    {
        { "pt", EstadoDeRevisao.Aprovado },
        { "en", EstadoDeRevisao.PendenteRevisao },
        { "es", EstadoDeRevisao.PendenteRevisao },
        { "fr", EstadoDeRevisao.PendenteRevisao },
    };

    #endregion

    #region MARK: Consulta

    public static EstadoDeRevisao ObterEstado(string idioma)
    {
        string chave = Normalizar(idioma);

        return Estados.TryGetValue(chave, out EstadoDeRevisao estado) ? estado : EstadoDeRevisao.PendenteRevisao;
    }

    public static string ResolverIdiomaTecnico(string idioma)
    {
        return ObterEstado(idioma) == EstadoDeRevisao.Aprovado ? Normalizar(idioma) : IdiomaAprovado;
    }

    private static string Normalizar(string idioma)
    {
        return string.IsNullOrWhiteSpace(idioma) ? IdiomaAprovado : idioma.Trim().ToLowerInvariant();
    }

    #endregion
}
