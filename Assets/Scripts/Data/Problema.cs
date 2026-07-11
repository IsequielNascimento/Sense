[System.Serializable]
public class Problema
{
    public string titulo;
    public string descricao;
    public string descricao2;
    public string solucao;
    public string solucao2;
    public string imagem;
    public string botaopasso;

    public string id => imagem;
}
