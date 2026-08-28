public enum FonteDoModeloAr
{
    PrefabDaCena,
    ModeloDoCenario,
    ModeloDeAlerta,
}

public static class DecisaoDeModeloAr
{
    #region MARK: Precedencia da fonte do modelo

    public static FonteDoModeloAr Escolher(bool origemEhMontagem, bool temModeloDoCenario, bool temCodigoDeAlerta)
    {
        if (origemEhMontagem) return FonteDoModeloAr.PrefabDaCena;
        if (temModeloDoCenario) return FonteDoModeloAr.ModeloDoCenario;
        if (temCodigoDeAlerta) return FonteDoModeloAr.ModeloDeAlerta;

        return FonteDoModeloAr.PrefabDaCena;
    }

    #endregion
}
