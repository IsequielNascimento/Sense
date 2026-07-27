[System.Serializable]
public class Problema
{
    #region MARK - Conteúdo traduzido

    public string titulo;
    public string descricao;
    public string descricao2;
    public string solucao;
    public string solucao2;
    public string botaopasso;

    #endregion

    #region MARK - Identidade

    public string scenarioId;
    public string imageKey;
    public string resourceKey;

    public string imagem;

    public string ScenarioId => IdentidadeDoCenario.Resolver(scenarioId, imagem);

    public string ImageKey => IdentidadeDoCenario.Resolver(imageKey, imagem);

    public string ResourceKey => IdentidadeDoCenario.Resolver(resourceKey, scenarioId, imagem);

    #endregion
}
