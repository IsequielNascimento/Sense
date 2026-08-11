public enum EstadoLedsM4
{
    Desligado,
    Aberto,
    Fechado,
    Alerta
}

public enum IndicacaoVisualM4
{
    Padrao,
    Invertida
}

public enum CorLedsM4
{
    Desligado,
    Verde,
    Laranja,
    Vermelho
}

public static class RegrasLedsM4
{
    public static CorLedsM4 Resolver(EstadoLedsM4 estado, IndicacaoVisualM4 indicacaoVisual)
    {
        if (estado == EstadoLedsM4.Alerta) return CorLedsM4.Vermelho;
        if (estado == EstadoLedsM4.Desligado) return CorLedsM4.Desligado;

        bool invertida = indicacaoVisual == IndicacaoVisualM4.Invertida;
        bool verde = estado == EstadoLedsM4.Aberto ? !invertida : invertida;
        return verde ? CorLedsM4.Verde : CorLedsM4.Laranja;
    }
}
