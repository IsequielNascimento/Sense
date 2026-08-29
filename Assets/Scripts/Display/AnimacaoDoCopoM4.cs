using System.Text.RegularExpressions;

public static class AnimacaoDoCopoM4
{
    #region MARK: Estados do Animator expostos a partir do FBX

    public const string Calibrando = "CALIBRANDO";
    public const string Calibrado = "OPEN";

    public static readonly string[] Todas = { Calibrando, Calibrado };

    #endregion

    #region MARK: Derivacao a partir da instrucao do passo

    private static readonly Regex MencaoDeCalibracao = new Regex(@"\bcalibra\w*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex PassoDeConfirmacao = new Regex(@"^\s*Verifique a confirma", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string Derivar(string instrucao)
    {
        if (string.IsNullOrWhiteSpace(instrucao)) return null;
        if (!MencaoDeCalibracao.IsMatch(instrucao)) return null;

        return PassoDeConfirmacao.IsMatch(instrucao) ? Calibrado : Calibrando;
    }

    #endregion
}
