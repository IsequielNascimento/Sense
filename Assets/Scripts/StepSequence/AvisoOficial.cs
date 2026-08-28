using System;

#region MARK: Niveis centralizados

public enum NivelDeAviso
{
    Perigo,
    Atencao,
    Cuidado,
    Importante,
}

public static class NivelDeAvisoExtensoes
{
    public static bool EhSituacaoPerigosa(this NivelDeAviso nivel)
    {
        return nivel != NivelDeAviso.Importante;
    }

    public static string RotuloExibicao(this NivelDeAviso nivel, string idioma = "pt")
    {
        string chave = string.IsNullOrWhiteSpace(idioma) ? "pt" : idioma.Trim().ToLowerInvariant();

        switch (chave)
        {
            case "en": return RotuloEm(nivel, "DANGER", "WARNING", "CAUTION", "IMPORTANT");
            case "es": return RotuloEm(nivel, "PELIGRO", "ATENCIÓN", "PRECAUCIÓN", "IMPORTANTE");
            case "fr": return RotuloEm(nivel, "DANGER", "AVERTISSEMENT", "ATTENTION", "IMPORTANT");
            default: return RotuloEm(nivel, "PERIGO", "ATENÇÃO", "CUIDADO", "IMPORTANTE");
        }
    }

    private static string RotuloEm(NivelDeAviso nivel, string perigo, string atencao, string cuidado, string importante)
    {
        switch (nivel)
        {
            case NivelDeAviso.Perigo: return perigo;
            case NivelDeAviso.Atencao: return atencao;
            case NivelDeAviso.Cuidado: return cuidado;
            case NivelDeAviso.Importante: return importante;
            default: throw new ArgumentOutOfRangeException(nameof(nivel), nivel, null);
        }
    }
}

#endregion

#region MARK: Dados estruturais

[Serializable]
public class AvisoEstrutural
{
    public string id;
    public string nivel;
    public int pagina;
    public string[] codigos = new string[0];
}

#endregion

#region MARK: Conteudo por idioma

[Serializable]
public class TextoDeAviso
{
    public string id;
    public string texto;
}

#endregion

public sealed class AvisoOficial
{
    public AvisoOficial(AvisoEstrutural estrutura, string texto)
    {
        if (estrutura == null) throw new ArgumentNullException(nameof(estrutura));

        Id = estrutura.id ?? string.Empty;
        Nivel = ConverterNivel(estrutura.nivel);
        Pagina = estrutura.pagina;
        Texto = texto ?? string.Empty;
    }

    public string Id { get; }
    public NivelDeAviso Nivel { get; }
    public int Pagina { get; }
    public string Texto { get; }

    private static NivelDeAviso ConverterNivel(string nivel)
    {
        switch (nivel)
        {
            case "PERIGO": return NivelDeAviso.Perigo;
            case "ATENÇÃO": return NivelDeAviso.Atencao;
            case "ATENCAO": return NivelDeAviso.Atencao;
            case "CUIDADO": return NivelDeAviso.Cuidado;
            case "IMPORTANTE": return NivelDeAviso.Importante;
            default: throw new ArgumentException($"Nivel de aviso desconhecido: '{nivel}'.", nameof(nivel));
        }
    }
}
