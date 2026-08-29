using System;
using System.Collections.Generic;
using UnityEngine;

#region MARK: Conteudo por idioma

[Serializable]
public class InstrucaoTraduzida
{
    public string pt;
    public string texto;
}

[Serializable]
public class TraducoesDeInstrucoesDeDisplay
{
    public InstrucaoTraduzida[] entradas = new InstrucaoTraduzida[0];
}

#endregion

public static class InstrucoesDeDisplayLocalizadas
{
    #region MARK: Recursos

    public const string IdiomaCanonico = "pt";
    public const string PastaDeRecursos = "CatalogoDeAlertas";
    public const string PrefixoDoArquivo = "instrucoes_";

    #endregion

    #region MARK: Cache por idioma

    private static readonly Dictionary<string, Dictionary<string, string>> PorIdioma =
        new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

    public static void LimparCache()
    {
        PorIdioma.Clear();
    }

    public static string CaminhoDoRecurso(string idioma)
    {
        return $"{PastaDeRecursos}/{PrefixoDoArquivo}{Normalizar(idioma)}";
    }

    #endregion

    #region MARK: Traducao das instrucoes canonicas

    public static string Traduzir(string instrucao, string idioma)
    {
        if (string.IsNullOrWhiteSpace(instrucao)) return instrucao;

        IReadOnlyDictionary<string, string> tabela = Tabela(idioma);

        if (tabela == null) return instrucao;

        return tabela.TryGetValue(instrucao, out string traducao) && !string.IsNullOrWhiteSpace(traducao)
            ? traducao
            : instrucao;
    }

    public static IReadOnlyDictionary<string, string> Tabela(string idioma)
    {
        string chave = Normalizar(idioma);

        if (string.Equals(chave, IdiomaCanonico, StringComparison.Ordinal)) return null;

        if (PorIdioma.TryGetValue(chave, out Dictionary<string, string> cacheada)) return cacheada;

        Dictionary<string, string> tabela = Carregar(chave);
        PorIdioma[chave] = tabela;

        return tabela;
    }

    #endregion

    #region MARK: Carregamento

    private static Dictionary<string, string> Carregar(string idioma)
    {
        var tabela = new Dictionary<string, string>(StringComparer.Ordinal);
        string caminho = $"{PastaDeRecursos}/{PrefixoDoArquivo}{idioma}";
        TextAsset arquivo = Resources.Load<TextAsset>(caminho);

        if (arquivo == null)
        {
            Debug.LogWarning(
                $"[InstrucoesDeDisplayLocalizadas] Recurso nao encontrado em Resources/{caminho}. " +
                "As instrucoes guiadas continuam em portugues.");
            return tabela;
        }

        try
        {
            TraducoesDeInstrucoesDeDisplay dados =
                JsonUtility.FromJson<TraducoesDeInstrucoesDeDisplay>(arquivo.text);

            if (dados?.entradas == null) return tabela;

            foreach (InstrucaoTraduzida entrada in dados.entradas)
            {
                if (entrada == null || string.IsNullOrWhiteSpace(entrada.pt)) continue;

                tabela[entrada.pt] = entrada.texto;
            }
        }
        catch (Exception excecao)
        {
            Debug.LogError(
                $"[InstrucoesDeDisplayLocalizadas] Falha ao desserializar Resources/{caminho}: {excecao.Message}");
        }

        return tabela;
    }

    private static string Normalizar(string idioma)
    {
        return RevisaoDeIdiomasTecnicos.ResolverIdiomaTecnico(idioma);
    }

    #endregion
}
