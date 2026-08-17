using System;

public sealed class QuadroDeDisplayM4
{
    #region MARK: Vocabulario de LED aceito pela TelaM4

    public const string LedApagado = "nenhum";
    public const string LedAberto = "aberto";
    public const string LedFechado = "fechado";
    public const string LedVermelho = "vermelho";
    public const string LedVermelhoPiscando = "vermelho_piscando";

    #endregion

    #region MARK: Construcao

    public QuadroDeDisplayM4(
        string textoLcd,
        EstadoLedsM4 leds,
        bool ledPiscando = false,
        string instrucao = null,
        float progressoSegundos = 0f,
        string animacao = null)
    {
        if (string.IsNullOrWhiteSpace(textoLcd))
        {
            throw new ArgumentException("Um quadro de display precisa de texto no LCD.", nameof(textoLcd));
        }

        if (ledPiscando && leds != EstadoLedsM4.Alerta)
        {
            throw new ArgumentException("Somente o estado de alerta pisca no M4.", nameof(ledPiscando));
        }

        if (progressoSegundos < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(progressoSegundos));
        }

        TextoLcd = textoLcd;
        Leds = leds;
        LedPiscando = ledPiscando;
        Instrucao = instrucao;
        ProgressoSegundos = progressoSegundos;
        Animacao = animacao;
    }

    #endregion

    #region MARK: Conteudo

    public string TextoLcd { get; }
    public EstadoLedsM4 Leds { get; }
    public bool LedPiscando { get; }
    public string Instrucao { get; }
    public float ProgressoSegundos { get; }
    public string Animacao { get; }

    #endregion

    #region MARK: Traducao para a Etapa

    public string EstadoDeLedDaEtapa()
    {
        switch (Leds)
        {
            case EstadoLedsM4.Alerta: return LedPiscando ? LedVermelhoPiscando : LedVermelho;
            case EstadoLedsM4.Aberto: return LedAberto;
            case EstadoLedsM4.Fechado: return LedFechado;
            default: return LedApagado;
        }
    }

    public void Aplicar(Etapa etapa)
    {
        if (etapa == null) return;

        if (!string.IsNullOrWhiteSpace(Instrucao)) etapa.tutorial = Instrucao;
        etapa.animacao = Animacao ?? string.Empty;
        etapa.textoDisplay = TextoLcd;
        etapa.leds = EstadoDeLedDaEtapa();
        etapa.alerta = string.Empty;
        etapa.textoAngulo = string.Empty;
        etapa.alertaTempoExcedido = string.Empty;
        etapa.progressoSegundos = ProgressoSegundos;
        etapa.progressoEstoura = false;
    }

    #endregion
}
