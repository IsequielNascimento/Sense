using System;
using System.Collections.Generic;

#region MARK: Dados estruturais

[Serializable]
public class AlertaEstrutural
{
    public string codigo;
    public string categoria;
    public int quantidadeDeAcoes;
    public int quantidadeDeLocais;
    public string scenarioResourceKey;
}

[Serializable]
public class EstruturaDoCatalogoDeAlertas
{
    public int paginaTabelaCodigos;
    public int paginaTabelaResolucao;
    public AlertaEstrutural[] alertas = new AlertaEstrutural[0];
    public AvisoEstrutural[] avisos = new AvisoEstrutural[0];
}

#endregion

#region MARK: Conteudo por idioma

[Serializable]
public class TextoDeAlerta
{
    public string codigo;
    public string nome;
    public string oQueE;
    public string quandoOcorre;
    public string[] acoes = new string[0];
    public string[] locais = new string[0];
    public string padrao;
}

[Serializable]
public class TextosDoCatalogoDeAlertas
{
    public string notaAlertasDiagnostico = string.Empty;
    public TextoDeAlerta[] alertas = new TextoDeAlerta[0];
    public TextoDeAviso[] avisos = new TextoDeAviso[0];
}

#endregion

public sealed class AlertaOficial
{
    #region MARK: Construcao

    public AlertaOficial(
        AlertaEstrutural estrutura,
        TextoDeAlerta texto,
        int paginaTabelaCodigos,
        int paginaTabelaResolucao,
        string nota = "",
        IReadOnlyList<AvisoOficial> avisos = null)
    {
        if (estrutura == null) throw new ArgumentNullException(nameof(estrutura));

        Codigo = estrutura.codigo ?? string.Empty;
        Categoria = estrutura.categoria ?? string.Empty;
        QuantidadeDeAcoesEsperada = estrutura.quantidadeDeAcoes;
        QuantidadeDeLocaisEsperada = estrutura.quantidadeDeLocais;
        PaginaTabelaCodigos = paginaTabelaCodigos;
        PaginaTabelaResolucao = paginaTabelaResolucao;
        ScenarioResourceKey = IdentidadeDoCenario.Resolver(estrutura.scenarioResourceKey);

        Nome = texto?.nome ?? string.Empty;
        OQueE = texto?.oQueE ?? string.Empty;
        QuandoOcorre = texto?.quandoOcorre ?? string.Empty;
        Padrao = texto?.padrao ?? string.Empty;
        Nota = nota ?? string.Empty;
        Acoes = Array.AsReadOnly(texto?.acoes ?? Array.Empty<string>());
        Locais = Array.AsReadOnly(texto?.locais ?? Array.Empty<string>());
        Avisos = avisos ?? Array.Empty<AvisoOficial>();
    }

    #endregion

    #region MARK: Identidade e fonte

    public string Codigo { get; }
    public string Categoria { get; }
    public int PaginaTabelaCodigos { get; }
    public int PaginaTabelaResolucao { get; }

    public bool EhFuncional => string.Equals(Categoria, CategoriaFuncional, StringComparison.Ordinal);
    public bool EhOperacional => string.Equals(Categoria, CategoriaOperacional, StringComparison.Ordinal);

    public const string CategoriaFuncional = "Funcional";
    public const string CategoriaOperacional = "Operacional";

    #endregion

    #region MARK: Vinculo com cenario animado

    public string ScenarioResourceKey { get; }

    public bool PossuiCenarioAnimado => IdentidadeDoCenario.EstaDefinida(ScenarioResourceKey);

    #endregion

    #region MARK: Conteudo do manual

    public string Nome { get; }
    public string OQueE { get; }
    public string QuandoOcorre { get; }
    public IReadOnlyList<string> Acoes { get; }
    public IReadOnlyList<string> Locais { get; }
    public string Padrao { get; }
    public string Nota { get; }
    public IReadOnlyList<AvisoOficial> Avisos { get; }

    public int QuantidadeDeAcoesEsperada { get; }
    public int QuantidadeDeLocaisEsperada { get; }

    #endregion
}
