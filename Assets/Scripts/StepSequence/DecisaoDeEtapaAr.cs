using System;

public static class DecisaoDeEtapaAr
{
    #region MARK: Vocabulario de animacao

    public const string PrefixoDeAnimacaoDeMontagem = "animacao_";

    #endregion

    #region MARK: Classificacao da etapa

    public static bool PossuiAnimacao(string animacao)
    {
        return !string.IsNullOrWhiteSpace(animacao);
    }

    public static bool EhMontagem(string animacao, string camadaAlvo, string camadaPadrao)
    {
        if (!PossuiAnimacao(animacao)) return false;
        if (!EhAnimacaoDeMontagem(animacao)) return false;
        if (string.IsNullOrEmpty(camadaAlvo)) return true;

        return string.Equals(camadaAlvo, camadaPadrao, StringComparison.Ordinal);
    }

    public static bool EhAnimacaoDeMontagem(string animacao)
    {
        return PossuiAnimacao(animacao)
            && animacao.StartsWith(PrefixoDeAnimacaoDeMontagem, StringComparison.Ordinal);
    }

    #endregion
}
