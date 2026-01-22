// Arquivo: EtapaProblema.cs

[System.Serializable]
public class EtapaProblema
{
    public string tutorial;
    public string animacao;
    
    // CAMPOS ADICIONADOS:
    public string telaDisplay; // Nome do sprite que aparecerá no display do sensor
    public string vfx;         // Identificador do efeito visual (ex: "alerta", "destaque_peca_X", "nenhum")
}