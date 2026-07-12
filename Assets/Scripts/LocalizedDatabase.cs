using System;
using UnityEngine;

public static class LocalizedDatabase
{
    public const string ProblemasPath = "banco_de_dados_{language}";
    public const string MenuPath = "BancoDeDadosMenu/banco_menu_{language}";
    public const string MontagemPath = "BancoDeDadosMontagem/Montagem/banco_montagem_{language}";
    public const string Montagem2Path = "BancoDeDadosMontagem2/banco_montagem2_{language}";

    public static string CurrentLanguage => ResolveCurrentLanguage();

    public static ArExperienceData LoadArExperience(string source, string problemId = null)
    {
        DadosMontagem uiText = Load<DadosMontagem>(MontagemPath);
        bool isAssembly = string.IsNullOrWhiteSpace(source) || source == "montagem";

        if (isAssembly)
        {
            return new ArExperienceData(uiText, CreateAssemblySteps(uiText));
        }

        string resolvedProblemId = string.IsNullOrWhiteSpace(problemId) ? source : problemId;
        string path = $"BancoDeDadosProblemas/{{language}}/{resolvedProblemId}";
        PassoAPasso problem = Load<PassoAPasso>(path);

        if (problem.etapas == null || problem.etapas.Length == 0)
        {
            Debug.LogError($"[LocalizedDatabase] O problema '{resolvedProblemId}' nao contem etapas.");
            return new ArExperienceData(uiText, new StepSequenceData());
        }

        int count = problem.etapas.Length;
        var steps = new string[count];
        var animations = new string[count];
        var displays = new string[count];
        var vfx = new string[count];

        for (int i = 0; i < count; i++)
        {
            Etapa stage = problem.etapas[i];
            steps[i] = stage?.tutorial ?? string.Empty;
            animations[i] = stage?.animacao ?? string.Empty;
            displays[i] = stage?.telaDisplay ?? string.Empty;
            vfx[i] = stage?.vfx ?? string.Empty;
        }

        var sequence = new StepSequenceData(
            steps,
            animations,
            displays,
            vfx,
            string.IsNullOrWhiteSpace(problem.layer) ? "Base Layer" : problem.layer);

        DevelopmentLog.Log($"[LocalizedDatabase] Problema '{resolvedProblemId}' carregado com {count} etapas.");
        return new ArExperienceData(uiText, sequence);
    }

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

    private static StepSequenceData CreateAssemblySteps(DadosMontagem data)
    {
        if (data?.passos == null || data.passos.Length == 0)
        {
            return new StepSequenceData();
        }

        int count = data.passos.Length;
        var steps = new string[count];
        var animations = new string[count];
        var displays = new string[count];
        var vfx = new string[count];

        for (int i = 0; i < count; i++)
        {
            PassoMontagem step = data.passos[i];
            steps[i] = step?.tutorial ?? string.Empty;
            animations[i] = $"animacao_{step?.numero}";
            displays[i] = string.Empty;
            vfx[i] = string.Empty;
        }

        DevelopmentLog.Log($"[LocalizedDatabase] Montagem padrao carregada com {count} etapas.");
        return new StepSequenceData(steps, animations, displays, vfx, "Base Layer");
    }
}

public sealed class ArExperienceData
{
    public ArExperienceData(DadosMontagem uiText, StepSequenceData sequence)
    {
        UiText = uiText ?? new DadosMontagem();
        Sequence = sequence ?? new StepSequenceData();
    }

    public DadosMontagem UiText { get; }
    public StepSequenceData Sequence { get; }
}

public sealed class StepSequenceData
{
    public StepSequenceData(
        string[] steps = null,
        string[] animations = null,
        string[] displays = null,
        string[] vfx = null,
        string layer = "Base Layer")
    {
        Steps = steps ?? Array.Empty<string>();
        Animations = animations ?? Array.Empty<string>();
        Displays = displays ?? Array.Empty<string>();
        Vfx = vfx ?? Array.Empty<string>();
        Layer = string.IsNullOrWhiteSpace(layer) ? "Base Layer" : layer;
    }

    public string[] Steps { get; }
    public string[] Animations { get; }
    public string[] Displays { get; }
    public string[] Vfx { get; }
    public string Layer { get; }
}
