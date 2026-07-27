[System.Serializable]
public class EtapaEstrutural
{
    public string animacao;
    public string telaDisplay;
    public string vfx;
    public string textoDisplay;
    public string alerta;
    public string leds;
    public float progressoSegundos;
    public bool progressoEstoura;
    public string alertaTempoExcedido;
    public string textoAngulo;
}

[System.Serializable]
public class EstruturaCenario
{
    public string scenarioId;
    public string layer;
    public EtapaEstrutural[] etapas = new EtapaEstrutural[0];
}
