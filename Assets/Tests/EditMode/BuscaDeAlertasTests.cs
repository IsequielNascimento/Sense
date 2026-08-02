using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class BuscaDeAlertasTests
{
    #region MARK: Fixture

    private static AlertaOficial Criar(string codigo, string nome, string oQueE, string quandoOcorre, string[] acoes)
    {
        var estrutura = new AlertaEstrutural
        {
            codigo = codigo,
            categoria = AlertaOficial.CategoriaFuncional,
            quantidadeDeAcoes = acoes.Length,
            quantidadeDeLocais = 1,
        };

        var texto = new TextoDeAlerta
        {
            codigo = codigo,
            nome = nome,
            oQueE = oQueE,
            quandoOcorre = quandoOcorre,
            acoes = acoes,
            locais = new[] { "em campo" },
            padrao = "sempre ligado",
        };

        return new AlertaOficial(estrutura, texto, 11, 76);
    }

    private static readonly AlertaOficial A1 = Criar(
        "A1", "ANGULO MINIMO", "a valvula nao atingiu o angulo minimo", "alerta por angulo insuficiente",
        new[] { "Verificar fornecimento de ar comprimido" });

    private static readonly AlertaOficial A2 = Criar(
        "A2", "TEMPO LIMITE", "o tempo de calibracao e monitorado", "alerta por tempo excedente",
        new[] { "Aumentar tempo limite" });

    private static readonly AlertaOficial A9 = Criar(
        "A9", "MAX PRESSAO NA LINHA", "a pressao da linha e monitorada", "alerta por pressao maxima",
        new[] { "Verificar fornecimento de ar comprimido" });

    private static readonly IReadOnlyList<AlertaOficial> Catalogo = new List<AlertaOficial> { A1, A2, A9 };

    #endregion

    #region MARK: Termo vazio

    [Test]
    public void TermoVazio_RetornaTodoOCatalogoNaOrdemOriginal()
    {
        IReadOnlyList<AlertaOficial> resultado = BuscaDeAlertas.Buscar(Catalogo, string.Empty);

        Assert.That(resultado, Is.EqualTo(Catalogo));
    }

    [Test]
    public void TermoNulo_RetornaTodoOCatalogo()
    {
        IReadOnlyList<AlertaOficial> resultado = BuscaDeAlertas.Buscar(Catalogo, null);

        Assert.That(resultado, Is.EqualTo(Catalogo));
    }

    #endregion

    #region MARK: Busca por campo

    [Test]
    public void EncontraPorCodigo()
    {
        IReadOnlyList<AlertaOficial> resultado = BuscaDeAlertas.Buscar(Catalogo, "A2");

        Assert.That(resultado, Is.EqualTo(new[] { A2 }));
    }

    [Test]
    public void EncontraPorNome()
    {
        IReadOnlyList<AlertaOficial> resultado = BuscaDeAlertas.Buscar(Catalogo, "TEMPO LIMITE");

        Assert.That(resultado, Is.EqualTo(new[] { A2 }));
    }

    [Test]
    public void EncontraPorOQueE()
    {
        IReadOnlyList<AlertaOficial> resultado = BuscaDeAlertas.Buscar(Catalogo, "angulo minimo");

        Assert.That(resultado, Is.EqualTo(new[] { A1 }));
    }

    [Test]
    public void EncontraPorQuandoOcorre()
    {
        IReadOnlyList<AlertaOficial> resultado = BuscaDeAlertas.Buscar(Catalogo, "pressao maxima");

        Assert.That(resultado, Is.EqualTo(new[] { A9 }));
    }

    [Test]
    public void EncontraPorAcao()
    {
        IReadOnlyList<AlertaOficial> resultado = BuscaDeAlertas.Buscar(Catalogo, "ar comprimido");

        Assert.That(resultado.Select(alerta => alerta.Codigo), Is.EqualTo(new[] { "A1", "A9" }));
    }

    #endregion

    #region MARK: Prioridade

    [Test]
    public void PriorizaCodigoOuNomeQueComecamPeloTermoAntesDeOcorrenciasInternas()
    {
        IReadOnlyList<AlertaOficial> resultado = BuscaDeAlertas.Buscar(Catalogo, "a");

        Assert.That(resultado.Select(alerta => alerta.Codigo), Is.EqualTo(new[] { "A1", "A2", "A9" }));
    }

    [Test]
    public void ItemQueSoTemOcorrenciaInternaVemDepoisDosQueComecamPeloTermo()
    {
        AlertaOficial interno = Criar(
            "A3", "ANGULO", "o tempo de resposta e monitorado", "alerta interno",
            new[] { "Verificar configuracao" });
        IReadOnlyList<AlertaOficial> catalogo = new[] { interno, A2 };

        IReadOnlyList<AlertaOficial> resultado = BuscaDeAlertas.Buscar(catalogo, "tempo");

        Assert.That(resultado.Select(alerta => alerta.Codigo), Is.EqualTo(new[] { "A2", "A3" }));
    }

    #endregion

    #region MARK: Case insensitive

    [Test]
    public void BuscaEhCaseInsensitive()
    {
        IReadOnlyList<AlertaOficial> resultado = BuscaDeAlertas.Buscar(Catalogo, "temPO liMITE");

        Assert.That(resultado, Is.EqualTo(new[] { A2 }));
    }

    #endregion

    #region MARK: Sem resultado

    [Test]
    public void TermoSemCorrespondencia_RetornaListaVazia()
    {
        IReadOnlyList<AlertaOficial> resultado = BuscaDeAlertas.Buscar(Catalogo, "inexistente");

        Assert.That(resultado, Is.Empty);
    }

    #endregion

    #region MARK: Catalogo oficial nunca introduz A10

    [Test]
    public void NuncaIntroduzA10_MesmoBuscandoPorEle()
    {
        IReadOnlyList<AlertaOficial> catalogoOficial = CatalogoDeAlertas.Carregar();

        IReadOnlyList<AlertaOficial> resultado = BuscaDeAlertas.Buscar(catalogoOficial, "A10");

        Assert.That(resultado.Any(alerta => alerta.Codigo == "A10"), Is.False);
    }

    [Test]
    public void TermoVazioNoCatalogoOficial_RetornaOsVinteEQuatroItens()
    {
        IReadOnlyList<AlertaOficial> catalogoOficial = CatalogoDeAlertas.Carregar();

        IReadOnlyList<AlertaOficial> resultado = BuscaDeAlertas.Buscar(catalogoOficial, string.Empty);

        Assert.That(resultado.Count, Is.EqualTo(24));
    }

    #endregion
}
