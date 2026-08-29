using System.Linq;

public static class FerramentasDoM4
{
    #region MARK: Objetos do FBX

    public const string ChaveMagnetica = "CHAVE_S";
    public const string ChavePhilips = "Chave Philips";

    public static readonly string[] Todas = { ChaveMagnetica, ChavePhilips };

    #endregion

    #region MARK: Qual ferramenta o passo mostra

    public static string ParaAnimacao(string animacao)
    {
        if (string.IsNullOrWhiteSpace(animacao)) return null;

        return AnimacaoDeBotaoM4.Todas.Contains(animacao) ? ChaveMagnetica : null;
    }

    public static bool DeveAparecer(string nomeDaFerramenta, string animacao)
    {
        return nomeDaFerramenta == ParaAnimacao(animacao);
    }

    #endregion
}
