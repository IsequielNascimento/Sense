// Arquivo: PassoAPasso.cs

[System.Serializable]
public class PassoAPasso
{
    public string layer;
    public EtapaProblema[] etapas; // O Unity vai encontrar a definição de EtapaProblema em outro arquivo
    public string tutorialInicial;
}

// A DEFINIÇÃO DA CLASSE EtapaProblema FOI REMOVIDA DESTE ARQUIVO.
