using System.Text.RegularExpressions;

public static class AnimacaoDeBotaoM4
{
    #region MARK: Estados do Animator expostos a partir do FBX

    public const string B1 = "B1Button";
    public const string B2 = "B2Button";
    public const string B3 = "B3Button";
    public const string B12 = "B12Button";
    public const string B13 = "B13Button";
    public const string B23 = "B23Button";
    public const string B123 = "B123Button";

    public static readonly string[] Todas = { B1, B2, B3, B12, B13, B23, B123 };

    #endregion

    #region MARK: Derivacao a partir da instrucao do passo

    private static readonly Regex MencaoDeBotao = new Regex(@"\bB([123])\b", RegexOptions.Compiled);

    public static string Derivar(string instrucao)
    {
        if (string.IsNullOrWhiteSpace(instrucao)) return null;

        bool b1 = false;
        bool b2 = false;
        bool b3 = false;

        foreach (Match mencao in MencaoDeBotao.Matches(instrucao))
        {
            switch (mencao.Groups[1].Value)
            {
                case "1": b1 = true; break;
                case "2": b2 = true; break;
                case "3": b3 = true; break;
            }
        }

        if (b1 && b2 && b3) return B123;
        if (b1 && b2) return B12;
        if (b1 && b3) return B13;
        if (b2 && b3) return B23;

        if (b1) return B1;
        if (b2) return B2;
        if (b3) return B3;

        return null;
    }

    #endregion
}
