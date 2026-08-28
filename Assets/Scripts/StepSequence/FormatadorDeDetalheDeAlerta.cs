using System;
using System.Collections.Generic;
using System.Text;

public readonly struct RotulosDeDetalhe
{
    public RotulosDeDetalhe(string oQueE, string quandoOcorre, string oQueFazer, string onde, string padrao, string nota, string origemManual)
    {
        OQueE = oQueE;
        QuandoOcorre = quandoOcorre;
        OQueFazer = oQueFazer;
        Onde = onde;
        Padrao = padrao;
        Nota = nota;
        OrigemManual = origemManual;
    }

    public string OQueE { get; }
    public string QuandoOcorre { get; }
    public string OQueFazer { get; }
    public string Onde { get; }
    public string Padrao { get; }
    public string Nota { get; }
    public string OrigemManual { get; }
}

public readonly struct DetalheDeAlertaFormatado
{
    public DetalheDeAlertaFormatado(string titulo, string descricao, string solucao)
    {
        Titulo = titulo;
        Descricao = descricao;
        Solucao = solucao;
    }

    public string Titulo { get; }
    public string Descricao { get; }
    public string Solucao { get; }
}

public static class FormatadorDeDetalheDeAlerta
{
    #region MARK: Rotulos

    public const string IdiomaPadrao = "pt";

    public const string RotuloOQueE = "O que é?";
    public const string RotuloQuandoOcorre = "Quando ocorre?";
    public const string RotuloOQueFazer = "O que fazer?";
    public const string RotuloOnde = "Onde";
    public const string RotuloPadrao = "Padrão";
    public const string RotuloNota = "Nota";
    public const string OrigemManual = "Manual";

    private static readonly Dictionary<string, RotulosDeDetalhe> RotulosPorIdioma =
        new Dictionary<string, RotulosDeDetalhe>(StringComparer.OrdinalIgnoreCase)
        {
            ["pt"] = new RotulosDeDetalhe(RotuloOQueE, RotuloQuandoOcorre, RotuloOQueFazer, RotuloOnde, RotuloPadrao, RotuloNota, OrigemManual),
            ["en"] = new RotulosDeDetalhe("What is it?", "When does it occur?", "What to do?", "Where", "Default", "Note", "Manual"),
            ["es"] = new RotulosDeDetalhe("¿Qué es?", "¿Cuándo ocurre?", "¿Qué hacer?", "Dónde", "Predeterminado", "Nota", "Manual"),
            ["fr"] = new RotulosDeDetalhe("Qu'est-ce que c'est ?", "Quand cela se produit-il ?", "Que faire ?", "Où", "Par défaut", "Note", "Manuel"),
        };

    public static RotulosDeDetalhe Rotulos(string idioma)
    {
        string chave = string.IsNullOrWhiteSpace(idioma) ? IdiomaPadrao : idioma.Trim();

        return RotulosPorIdioma.TryGetValue(chave, out RotulosDeDetalhe rotulos) ? rotulos : RotulosPorIdioma[IdiomaPadrao];
    }

    #endregion

    #region MARK: Formatacao

    public static DetalheDeAlertaFormatado Formatar(AlertaOficial alerta, string idioma = IdiomaPadrao)
    {
        if (alerta == null) return new DetalheDeAlertaFormatado(string.Empty, string.Empty, string.Empty);

        RotulosDeDetalhe rotulos = Rotulos(idioma);

        string titulo = $"{alerta.Codigo} - {alerta.Nome}";
        string descricao = FormatarDescricao(alerta, rotulos);
        string solucao = FormatarSolucao(alerta, rotulos, idioma);

        return new DetalheDeAlertaFormatado(titulo, descricao, solucao);
    }

    private static string FormatarDescricao(AlertaOficial alerta, RotulosDeDetalhe rotulos)
    {
        var construtor = new StringBuilder();

        AdicionarBloco(construtor, rotulos.OQueE, alerta.OQueE);
        AdicionarBloco(construtor, rotulos.QuandoOcorre, alerta.QuandoOcorre);

        return construtor.ToString();
    }

    private static string FormatarSolucao(AlertaOficial alerta, RotulosDeDetalhe rotulos, string idioma)
    {
        var construtor = new StringBuilder();

        AdicionarBloco(construtor, rotulos.OQueFazer, Numerar(alerta.Acoes));
        AdicionarBloco(construtor, rotulos.Onde, string.Join("\n", alerta.Locais));

        if (!string.IsNullOrEmpty(alerta.Padrao)) AdicionarBloco(construtor, rotulos.Padrao, alerta.Padrao);
        if (!string.IsNullOrEmpty(alerta.Nota)) AdicionarBloco(construtor, rotulos.Nota, alerta.Nota);

        foreach (AvisoOficial aviso in alerta.Avisos)
        {
            AdicionarBloco(construtor, aviso.Nivel.RotuloExibicao(idioma), FormatarAviso(aviso, rotulos));
        }

        return construtor.ToString();
    }

    private static string FormatarAviso(AvisoOficial aviso, RotulosDeDetalhe rotulos)
    {
        return $"{aviso.Texto}\n{rotulos.OrigemManual}, p. {aviso.Pagina}";
    }

    private static string Numerar(IReadOnlyList<string> itens)
    {
        var construtor = new StringBuilder();

        for (int indice = 0; indice < itens.Count; indice++)
        {
            if (indice > 0) construtor.Append('\n');
            construtor.Append(indice + 1).Append(" - ").Append(itens[indice]);
        }

        return construtor.ToString();
    }

    private static void AdicionarBloco(StringBuilder construtor, string rotulo, string conteudo)
    {
        if (construtor.Length > 0) construtor.Append("\n\n");
        construtor.Append("<b>").Append(rotulo).Append("</b>\n").Append(conteudo);
    }

    #endregion
}
