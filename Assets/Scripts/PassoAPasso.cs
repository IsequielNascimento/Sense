[System.Serializable]
public class PassoAPasso
{
    public string idProblema;
    public string tutorialInicial;
    public string popupInicialTexto1;
    public string popupInicialTexto2;
    public Etapa[] etapas;
    public string layer;
}

[System.Serializable]
public class Etapa
{
    public string animacao;
    public string tutorial;
    
    // AS DUAS LINHAS MÁGICAS QUE FALTAVAM:
    public string telaDisplay; 
    public string vfx;         
}