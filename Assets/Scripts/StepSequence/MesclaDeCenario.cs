using System;

public static class MesclaDeCenario
{
    #region MARK - Resultado

    public sealed class Resultado
    {
        public Resultado(Etapa[] etapas, string layer, bool usouEstrutura, string divergencia)
        {
            Etapas = etapas ?? Array.Empty<Etapa>();
            Layer = layer ?? string.Empty;
            UsouEstrutura = usouEstrutura;
            Divergencia = divergencia ?? string.Empty;
        }

        public Etapa[] Etapas { get; }
        public string Layer { get; }
        public bool UsouEstrutura { get; }
        public string Divergencia { get; }

        public bool TemDivergencia => !string.IsNullOrEmpty(Divergencia);
    }

    #endregion

    #region MARK - Mesclagem

    public static Resultado Mesclar(EstruturaCenario estrutura, Etapa[] traducao, string layerTraduzido)
    {
        traducao ??= Array.Empty<Etapa>();

        if (estrutura?.etapas == null || estrutura.etapas.Length == 0)
        {
            return new Resultado(Normalizar(traducao), layerTraduzido, false, string.Empty);
        }

        string divergencia = string.Empty;
        if (estrutura.etapas.Length != traducao.Length)
        {
            divergencia =
                $"estrutura tem {estrutura.etapas.Length} etapas e a traducao tem {traducao.Length}";
        }

        int total = estrutura.etapas.Length;
        var etapas = new Etapa[total];

        for (int i = 0; i < total; i++)
        {
            EtapaEstrutural fonte = estrutura.etapas[i] ?? new EtapaEstrutural();
            Etapa texto = i < traducao.Length ? traducao[i] : null;

            etapas[i] = new Etapa
            {
                tutorial = texto?.tutorial ?? string.Empty,
                animacao = IdentidadeDoCenario.Resolver(fonte.animacao, texto?.animacao),
                telaDisplay = IdentidadeDoCenario.Resolver(fonte.telaDisplay, texto?.telaDisplay),
                vfx = IdentidadeDoCenario.Resolver(fonte.vfx, texto?.vfx),
                textoDisplay = PreferirEstrutura(fonte.textoDisplay, texto?.textoDisplay),
                alerta = IdentidadeDoCenario.Resolver(fonte.alerta, texto?.alerta),
                leds = IdentidadeDoCenario.Resolver(fonte.leds, texto?.leds),
                progressoSegundos = fonte.progressoSegundos > 0f
                    ? fonte.progressoSegundos
                    : texto?.progressoSegundos ?? 0f,
                progressoEstoura = fonte.progressoEstoura || (texto?.progressoEstoura ?? false),
                alertaTempoExcedido = IdentidadeDoCenario.Resolver(
                    fonte.alertaTempoExcedido, texto?.alertaTempoExcedido),
                textoAngulo = IdentidadeDoCenario.Resolver(fonte.textoAngulo, texto?.textoAngulo),
            };
        }

        string layer = IdentidadeDoCenario.Resolver(estrutura.layer, layerTraduzido);

        return new Resultado(etapas, layer, true, divergencia);
    }

    #endregion

    #region MARK - Auxiliares

    private static string PreferirEstrutura(string daEstrutura, string daTraducao)
    {
        return daEstrutura ?? daTraducao ?? string.Empty;
    }

    private static Etapa[] Normalizar(Etapa[] etapas)
    {
        for (int i = 0; i < etapas.Length; i++)
        {
            etapas[i] ??= new Etapa();
        }

        return etapas;
    }

    #endregion
}
