using System;

public static class EtapasGuiadasDeAlerta
{
    #region MARK: Conversao das acoes oficiais

    public static Etapa[] Criar(AlertaOficial alerta)
    {
        if (alerta == null || alerta.Acoes == null || alerta.Acoes.Count == 0)
        {
            return Array.Empty<Etapa>();
        }

        var etapas = new Etapa[alerta.Acoes.Count];

        for (int i = 0; i < etapas.Length; i++)
        {
            etapas[i] = new Etapa
            {
                tutorial = alerta.Acoes[i] ?? string.Empty,
                animacao = string.Empty,
            };
        }

        return etapas;
    }

    public static string[] Tutoriais(Etapa[] etapas)
    {
        if (etapas == null || etapas.Length == 0) return Array.Empty<string>();

        var tutoriais = new string[etapas.Length];

        for (int i = 0; i < etapas.Length; i++)
        {
            tutoriais[i] = etapas[i]?.tutorial ?? string.Empty;
        }

        return tutoriais;
    }

    #endregion
}
