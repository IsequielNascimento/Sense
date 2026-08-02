using System;
using System.Collections.Generic;
using UnityEngine;

public static class CatalogoDeAlertas
{
    #region MARK: Recursos

    public const string PastaDeRecursos = "CatalogoDeAlertas";
    public const string ArquivoDeEstrutura = "estrutura";
    public const string IdiomaPadrao = "pt";

    #endregion

    #region MARK: Nota normativa

    private static readonly HashSet<string> CodigosComNotaDeDiagnostico = new HashSet<string>(StringComparer.Ordinal)
    {
        "A19", "A20", "A21", "A22", "A23", "A24", "A25",
    };

    #endregion

    #region MARK: Consulta

    public static IReadOnlyList<AlertaOficial> Carregar(string idioma = IdiomaPadrao)
    {
        EstruturaDoCatalogoDeAlertas estrutura = CarregarEstrutura();
        TextosDoCatalogoDeAlertas textos = CarregarTextos(idioma);

        Dictionary<string, AlertaEstrutural> porCodigo = IndexarEstrutura(estrutura.alertas);
        Dictionary<string, TextoDeAlerta> porCodigoTraduzido = IndexarTextos(textos.alertas);

        var catalogo = new List<AlertaOficial>(CodigosOficiais.Alertas.Count);

        foreach (string codigo in CodigosOficiais.Alertas)
        {
            if (!porCodigo.TryGetValue(codigo, out AlertaEstrutural dados)) continue;

            porCodigoTraduzido.TryGetValue(codigo, out TextoDeAlerta texto);

            string nota = CodigosComNotaDeDiagnostico.Contains(codigo) ? textos.notaAlertasDiagnostico : string.Empty;

            catalogo.Add(new AlertaOficial(
                dados,
                texto,
                estrutura.paginaTabelaCodigos,
                estrutura.paginaTabelaResolucao,
                nota));
        }

        return catalogo;
    }

    public static AlertaOficial Obter(string codigo, string idioma = IdiomaPadrao)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return null;

        string procurado = codigo.Trim();

        foreach (AlertaOficial alerta in Carregar(idioma))
        {
            if (string.Equals(alerta.Codigo, procurado, StringComparison.Ordinal)) return alerta;
        }

        return null;
    }

    #endregion

    #region MARK: Carregamento

    public static EstruturaDoCatalogoDeAlertas CarregarEstrutura()
    {
        return Desserializar<EstruturaDoCatalogoDeAlertas>($"{PastaDeRecursos}/{ArquivoDeEstrutura}");
    }

    public static TextosDoCatalogoDeAlertas CarregarTextos(string idioma)
    {
        string escolhido = string.IsNullOrWhiteSpace(idioma)
            ? IdiomaPadrao
            : idioma.Trim().ToLowerInvariant();

        var textos = Desserializar<TextosDoCatalogoDeAlertas>($"{PastaDeRecursos}/{escolhido}");

        bool temConteudo = textos.alertas != null && textos.alertas.Length > 0;

        if (temConteudo || escolhido == IdiomaPadrao) return textos;

        Debug.LogWarning(
            $"[CatalogoDeAlertas] Sem conteudo para '{escolhido}'. Usando '{IdiomaPadrao}'.");

        return Desserializar<TextosDoCatalogoDeAlertas>($"{PastaDeRecursos}/{IdiomaPadrao}");
    }

    private static T Desserializar<T>(string caminho) where T : class, new()
    {
        TextAsset arquivo = Resources.Load<TextAsset>(caminho);

        if (arquivo == null)
        {
            Debug.LogError($"[CatalogoDeAlertas] Recurso nao encontrado em Resources/{caminho}.");
            return new T();
        }

        try
        {
            return JsonUtility.FromJson<T>(arquivo.text) ?? new T();
        }
        catch (Exception excecao)
        {
            Debug.LogError(
                $"[CatalogoDeAlertas] Falha ao desserializar Resources/{caminho}: {excecao.Message}");
            return new T();
        }
    }

    #endregion

    #region MARK: Indexacao

    private static Dictionary<string, AlertaEstrutural> IndexarEstrutura(AlertaEstrutural[] itens)
    {
        var indice = new Dictionary<string, AlertaEstrutural>(StringComparer.Ordinal);

        if (itens == null) return indice;

        foreach (AlertaEstrutural item in itens)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.codigo)) continue;

            indice[item.codigo.Trim()] = item;
        }

        return indice;
    }

    private static Dictionary<string, TextoDeAlerta> IndexarTextos(TextoDeAlerta[] itens)
    {
        var indice = new Dictionary<string, TextoDeAlerta>(StringComparer.Ordinal);

        if (itens == null) return indice;

        foreach (TextoDeAlerta item in itens)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.codigo)) continue;

            indice[item.codigo.Trim()] = item;
        }

        return indice;
    }

    #endregion
}
