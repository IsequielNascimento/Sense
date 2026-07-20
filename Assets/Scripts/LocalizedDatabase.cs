using System;
using UnityEngine;

public static class LocalizedDatabase
{
    public const string ProblemasPath = "banco_de_dados_{language}";
    public const string MenuPath = "BancoDeDadosMenu/banco_menu_{language}";
    public const string MontagemPath = "BancoDeDadosMontagem/Montagem/banco_montagem_{language}";
    public const string Montagem2Path = "BancoDeDadosMontagem2/banco_montagem2_{language}";

    public static string CurrentLanguage => ResolveCurrentLanguage();

    public static ArExperienceData LoadArExperience(OrigemCena source, string problemId = null)
    {
        DadosMontagem uiText = Load<DadosMontagem>(MontagemPath);
        bool isAssembly = source == OrigemCena.Montagem;

        if (isAssembly)
        {
            return new ArExperienceData(uiText, CreateAssemblySteps(uiText));
        }

        string resolvedProblemId = problemId;
        if (string.IsNullOrWhiteSpace(resolvedProblemId))
        {
            Debug.LogError("[LocalizedDatabase] Nenhum problema foi selecionado para a experiência AR.");
            return new ArExperienceData(uiText, new StepSequenceData());
        }
        string path = $"BancoDeDadosProblemas/{{language}}/{resolvedProblemId}";
        PassoAPasso problem = Load<PassoAPasso>(path);

        if (problem.etapas == null || problem.etapas.Length == 0)
        {
            Debug.LogError($"[LocalizedDatabase] O problema '{resolvedProblemId}' nao contem etapas.");
            return new ArExperienceData(uiText, new StepSequenceData());
        }

        int count = problem.etapas.Length;
        var steps = new string[count];
        var etapas = new Etapa[count];

        for (int i = 0; i < count; i++)
        {
            Etapa stage = problem.etapas[i] ?? new Etapa();
            steps[i] = stage.tutorial ?? string.Empty;
            etapas[i] = stage;
        }

        var sequence = new StepSequenceData(
            steps,
            etapas,
            string.IsNullOrWhiteSpace(problem.layer) ? ArConstants.DefaultAnimatorLayer : problem.layer);

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
        var etapas = new Etapa[count];

        for (int i = 0; i < count; i++)
        {
            PassoMontagem step = data.passos[i];
            steps[i] = step?.tutorial ?? string.Empty;
            etapas[i] = new Etapa
            {
                tutorial = steps[i],
                animacao = ArConstants.AssemblyAnimationName(step?.numero),
            };
        }

        DevelopmentLog.Log($"[LocalizedDatabase] Montagem padrao carregada com {count} etapas.");
        return new StepSequenceData(steps, etapas, ArConstants.DefaultAnimatorLayer);
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
        Etapa[] etapas = null,
        string layer = ArConstants.DefaultAnimatorLayer)
    {
        Steps = steps ?? Array.Empty<string>();
        Etapas = etapas ?? Array.Empty<Etapa>();
        Layer = string.IsNullOrWhiteSpace(layer) ? ArConstants.DefaultAnimatorLayer : layer;
    }

    public string[] Steps { get; }
    public Etapa[] Etapas { get; }
    public string Layer { get; }
}
