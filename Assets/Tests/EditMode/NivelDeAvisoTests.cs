using System;
using NUnit.Framework;

public class NivelDeAvisoTests
{
    #region MARK: Reconhecimento dos quatro niveis

    [TestCase(NivelDeAviso.Perigo)]
    [TestCase(NivelDeAviso.Atencao)]
    [TestCase(NivelDeAviso.Cuidado)]
    [TestCase(NivelDeAviso.Importante)]
    public void OsQuatroNiveis_SaoReconhecidos(NivelDeAviso nivel)
    {
        Assert.That(System.Enum.IsDefined(typeof(NivelDeAviso), nivel), Is.True);
    }

    #endregion

    #region MARK: EhSituacaoPerigosa

    [TestCase(NivelDeAviso.Perigo, ExpectedResult = true)]
    [TestCase(NivelDeAviso.Atencao, ExpectedResult = true)]
    [TestCase(NivelDeAviso.Cuidado, ExpectedResult = true)]
    [TestCase(NivelDeAviso.Importante, ExpectedResult = false)]
    public bool EhSituacaoPerigosa_RetornaFalsoApenasParaImportante(NivelDeAviso nivel)
    {
        return nivel.EhSituacaoPerigosa();
    }

    #endregion

    #region MARK: AvisoOficial a partir de AvisoEstrutural

    [TestCase("PERIGO", NivelDeAviso.Perigo)]
    [TestCase("ATENÇÃO", NivelDeAviso.Atencao)]
    [TestCase("CUIDADO", NivelDeAviso.Cuidado)]
    [TestCase("IMPORTANTE", NivelDeAviso.Importante)]
    public void AvisoOficial_ReconheceOsQuatroRotulosDoManual(string rotulo, NivelDeAviso esperado)
    {
        var estrutura = new AvisoEstrutural { id = "A1", nivel = rotulo, pagina = 1 };

        var aviso = new AvisoOficial(estrutura, "texto");

        Assert.That(aviso.Nivel, Is.EqualTo(esperado));
    }

    [Test]
    public void AvisoOficial_NivelDesconhecido_Falha()
    {
        var estrutura = new AvisoEstrutural { id = "A1", nivel = "DESCONHECIDO", pagina = 1 };

        Assert.Throws<ArgumentException>(() => new AvisoOficial(estrutura, "texto"));
    }

    #endregion
}
