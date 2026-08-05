using System;
using System.Collections.Generic;

public static class DominioDeCenario
{
    #region MARK: Valores centralizados

    public const string Treinamento = "Treinamento";
    public const string Configuracao = "Configuracao";
    public const string Manutencao = "Manutencao";
    public const string Alerta = "Alerta";

    private static readonly HashSet<string> Validos = new HashSet<string>(StringComparer.Ordinal)
    {
        Treinamento, Configuracao, Manutencao, Alerta,
    };

    public static bool EhValido(string dominio)
    {
        return !string.IsNullOrWhiteSpace(dominio) && Validos.Contains(dominio);
    }

    #endregion

    #region MARK: Elegibilidade para alerta publico

    public static bool PodeSerApresentadoComoAlertaPublico(EstruturaCenario estrutura)
    {
        if (estrutura == null) return false;

        return string.Equals(estrutura.dominio, Alerta, StringComparison.Ordinal)
            && CodigosOficiais.EhAlerta(estrutura.primaryCode);
    }

    #endregion
}
