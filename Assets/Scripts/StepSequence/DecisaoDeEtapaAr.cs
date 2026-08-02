using System;

public static class DecisaoDeEtapaAr
{
    #region MARK: Classificacao da etapa

    public static bool PossuiAnimacao(string animacao)
    {
        return !string.IsNullOrWhiteSpace(animacao);
    }

    public static bool EhMontagem(string animacao, string camadaAlvo, string camadaPadrao)
    {
        if (!PossuiAnimacao(animacao)) return false;
        if (string.IsNullOrEmpty(camadaAlvo)) return true;

        return string.Equals(camadaAlvo, camadaPadrao, StringComparison.Ordinal);
    }

    #endregion
}
