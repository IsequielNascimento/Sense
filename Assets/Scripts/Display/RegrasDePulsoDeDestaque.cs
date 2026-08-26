using System;

public static class RegrasDePulsoDeDestaque
{
    public const float PeriodoPadraoSegundos = 1.2f;
    public const float IntensidadeMinima = 0.35f;
    public const float IntensidadeMaxima = 1f;

    public static float Intensidade(float tempoSegundos, float periodoSegundos)
    {
        if (periodoSegundos <= 0f) return IntensidadeMaxima;

        double onda = (1d + Math.Cos(2d * Math.PI * tempoSegundos / periodoSegundos)) / 2d;

        return (float)(IntensidadeMinima + onda * (IntensidadeMaxima - IntensidadeMinima));
    }

    public static float AvancarFase(float faseSegundos, float deltaSegundos, float periodoSegundos)
    {
        if (periodoSegundos <= 0f) return 0f;

        float fase = (faseSegundos + deltaSegundos) % periodoSegundos;

        return fase < 0f ? fase + periodoSegundos : fase;
    }
}
