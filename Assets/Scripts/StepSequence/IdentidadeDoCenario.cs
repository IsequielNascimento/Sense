public static class IdentidadeDoCenario
{
    #region MARK - Resolução

    public static string Resolver(params string[] candidatos)
    {
        if (candidatos == null) return string.Empty;

        foreach (string candidato in candidatos)
        {
            if (string.IsNullOrWhiteSpace(candidato)) continue;

            return candidato.Trim();
        }

        return string.Empty;
    }

    public static bool EstaDefinida(string identificador)
    {
        return !string.IsNullOrWhiteSpace(identificador);
    }

    #endregion
}
