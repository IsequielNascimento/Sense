using System;
using UnityEngine;

public static class LocalizedDatabase
{
    public const string ProblemasPath = "banco_de_dados_{language}";
    public const string MenuPath = "BancoDeDadosMenu/banco_menu_{language}";
    public const string MontagemPath = "BancoDeDadosMontagem/Montagem/banco_montagem_{language}";
    public const string Montagem2Path = "BancoDeDadosMontagem2/banco_montagem2_{language}";

    public static string CurrentLanguage => ResolveCurrentLanguage();

    public static T Load<T>(string resourcePathPattern) where T : class, new()
    {
        string idioma = ResolveCurrentLanguage();
        string caminho = ResolvePath(resourcePathPattern, idioma);
        TextAsset arquivo = Resources.Load<TextAsset>(caminho);

        if (arquivo == null && idioma != "pt")
        {
            string caminhoFallback = ResolvePath(resourcePathPattern, "pt");
            Debug.LogWarning($"[LocalizedDatabase] Recurso '{caminho}' não encontrado. Usando '{caminhoFallback}'.");
            caminho = caminhoFallback;
            arquivo = Resources.Load<TextAsset>(caminho);
        }

        if (arquivo == null)
        {
            Debug.LogError($"[LocalizedDatabase] Recurso JSON não encontrado em Resources/{caminho}.");
            return new T();
        }

        try
        {
            T dados = JsonUtility.FromJson<T>(arquivo.text);
            if (dados != null)
            {
                return dados;
            }

            Debug.LogError($"[LocalizedDatabase] O JSON em Resources/{caminho} resultou em dados nulos.");
        }
        catch (Exception exception)
        {
            Debug.LogError($"[LocalizedDatabase] Falha ao desserializar Resources/{caminho}: {exception.Message}");
        }

        return new T();
    }

    private static string ResolveCurrentLanguage()
    {
        string idioma = IdiomaManager.Instance != null
            ? IdiomaManager.Instance.ObterIdioma()
            : PlayerPrefs.GetString("idioma", "pt");

        return string.IsNullOrWhiteSpace(idioma) ? "pt" : idioma.Trim().ToLowerInvariant();
    }

    private static string ResolvePath(string pattern, string language)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("O padrão do caminho do recurso não pode ser vazio.", nameof(pattern));
        }

        return pattern
            .Replace("{language}", language)
            .Replace("{0}", language);
    }
}
