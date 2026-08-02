using System.Collections.Generic;
using System.Text;

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

    public const string RotuloOQueE = "O que é?";
    public const string RotuloQuandoOcorre = "Quando ocorre?";
    public const string RotuloOQueFazer = "O que fazer?";
    public const string RotuloOnde = "Onde";
    public const string RotuloPadrao = "Padrão";
    public const string RotuloNota = "Nota";

    #endregion

    #region MARK: Formatacao

    public static DetalheDeAlertaFormatado Formatar(AlertaOficial alerta)
    {
        if (alerta == null) return new DetalheDeAlertaFormatado(string.Empty, string.Empty, string.Empty);

        string titulo = $"{alerta.Codigo} - {alerta.Nome}";
        string descricao = FormatarDescricao(alerta);
        string solucao = FormatarSolucao(alerta);

        return new DetalheDeAlertaFormatado(titulo, descricao, solucao);
    }

    private static string FormatarDescricao(AlertaOficial alerta)
    {
        var construtor = new StringBuilder();

        AdicionarBloco(construtor, RotuloOQueE, alerta.OQueE);
        AdicionarBloco(construtor, RotuloQuandoOcorre, alerta.QuandoOcorre);

        return construtor.ToString();
    }

    private static string FormatarSolucao(AlertaOficial alerta)
    {
        var construtor = new StringBuilder();

        AdicionarBloco(construtor, RotuloOQueFazer, Numerar(alerta.Acoes));
        AdicionarBloco(construtor, RotuloOnde, string.Join("\n", alerta.Locais));

        if (!string.IsNullOrEmpty(alerta.Padrao)) AdicionarBloco(construtor, RotuloPadrao, alerta.Padrao);
        if (!string.IsNullOrEmpty(alerta.Nota)) AdicionarBloco(construtor, RotuloNota, alerta.Nota);

        return construtor.ToString();
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
