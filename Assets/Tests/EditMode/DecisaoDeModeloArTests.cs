using NUnit.Framework;

public class DecisaoDeModeloArTests
{
    #region MARK: A montagem nao herda a selecao de um alerta

    [Test]
    public void NaMontagem_OModeloVemSempreDoPrefabDaCena()
    {
        Assert.That(
            DecisaoDeModeloAr.Escolher(origemEhMontagem: true, temModeloDoCenario: true, temCodigoDeAlerta: true),
            Is.EqualTo(FonteDoModeloAr.PrefabDaCena));
    }

    [Test]
    public void SelecaoQueSobrouDeUmAlerta_NaoTrocaOModeloDaMontagem()
    {
        FonteDoModeloAr comSobra =
            DecisaoDeModeloAr.Escolher(origemEhMontagem: true, temModeloDoCenario: true, temCodigoDeAlerta: false);
        FonteDoModeloAr semSobra =
            DecisaoDeModeloAr.Escolher(origemEhMontagem: true, temModeloDoCenario: false, temCodigoDeAlerta: false);

        Assert.That(comSobra, Is.EqualTo(semSobra));
    }

    #endregion

    #region MARK: Fora da montagem vale a precedencia do cenario

    [Test]
    public void ForaDaMontagem_OCenarioMapeadoVenceOModeloDeAlerta()
    {
        Assert.That(
            DecisaoDeModeloAr.Escolher(origemEhMontagem: false, temModeloDoCenario: true, temCodigoDeAlerta: true),
            Is.EqualTo(FonteDoModeloAr.ModeloDoCenario));
    }

    [Test]
    public void ForaDaMontagem_SemCenarioMapeado_UsaOModeloDeAlerta()
    {
        Assert.That(
            DecisaoDeModeloAr.Escolher(origemEhMontagem: false, temModeloDoCenario: false, temCodigoDeAlerta: true),
            Is.EqualTo(FonteDoModeloAr.ModeloDeAlerta));
    }

    [Test]
    public void ForaDaMontagem_SemCenarioNemCodigo_CaiNoPrefabDaCena()
    {
        Assert.That(
            DecisaoDeModeloAr.Escolher(origemEhMontagem: false, temModeloDoCenario: false, temCodigoDeAlerta: false),
            Is.EqualTo(FonteDoModeloAr.PrefabDaCena));
    }

    #endregion
}
