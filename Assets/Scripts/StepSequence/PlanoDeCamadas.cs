public static class PlanoDeCamadas
{
    #region MARK - Constantes

    public const int PrimeiraCamadaDeProblema = 1;
    public const int CamadaInexistente = -1;

    #endregion

    #region MARK - Resolução de pesos

    public static bool EhCamadaDeProblema(int indiceDaCamada)
    {
        return indiceDaCamada >= PrimeiraCamadaDeProblema;
    }

    public static float PesoDaCamadaDeProblema(int indiceDaCamada, int camadaComEstado)
    {
        if (!EhCamadaDeProblema(indiceDaCamada)) return 0f;
        if (camadaComEstado == CamadaInexistente) return 0f;

        return indiceDaCamada == camadaComEstado ? 1f : 0f;
    }

    public static int CamadaComEstado(int indiceDaCamada, bool estadoExiste)
    {
        return estadoExiste ? indiceDaCamada : CamadaInexistente;
    }

    #endregion
}
