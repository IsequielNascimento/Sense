using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class EtapasGuiadasDeAlertaTests
{
    #region MARK: Fixture

    private const string AcaoOficialDeA24 = "Verificar os fios da solenoide";

    private IReadOnlyList<AlertaOficial> catalogo;

    [SetUp]
    public void CarregarCatalogo()
    {
        catalogo = CatalogoDeAlertas.Carregar("pt");
    }

    private static IEnumerable<string> CodigosOficiaisDoManual()
    {
        return CodigosOficiais.Alertas;
    }

    private AlertaOficial Buscar(string codigo)
    {
        AlertaOficial alerta = catalogo.FirstOrDefault(item => item.Codigo == codigo);

        Assert.That(alerta, Is.Not.Null, $"Alerta '{codigo}' ausente do catalogo.");

        return alerta;
    }

    #endregion

    #region MARK: Conversao das acoes oficiais

    [TestCaseSource(nameof(CodigosOficiaisDoManual))]
    public void TodoAlerta_GeraUmaEtapaPorAcaoOficialNaMesmaOrdem(string codigo)
    {
        AlertaOficial alerta = Buscar(codigo);

        Etapa[] etapas = EtapasGuiadasDeAlerta.Criar(alerta);

        Assert.That(etapas.Length, Is.EqualTo(alerta.Acoes.Count), $"{codigo}: quantidade de etapas.");

        for (int i = 0; i < etapas.Length; i++)
        {
            Assert.That(etapas[i].tutorial, Is.EqualTo(alerta.Acoes[i]), $"{codigo}: tutorial da etapa {i}.");
            Assert.That(etapas[i].animacao, Is.Empty, $"{codigo}: etapa {i} nao pode declarar animacao.");
        }
    }

    [Test]
    public void A24_GeraUmaUnicaEtapaComAAcaoOficialESemAnimacao()
    {
        AlertaOficial a24 = Buscar("A24");

        Etapa[] etapas = EtapasGuiadasDeAlerta.Criar(a24);

        Assert.That(etapas.Length, Is.EqualTo(1));
        Assert.That(etapas[0].tutorial, Is.EqualTo(AcaoOficialDeA24));
        Assert.That(etapas[0].tutorial, Is.EqualTo(a24.Acoes[0]));
        Assert.That(etapas[0].animacao, Is.Empty);
    }

    [Test]
    public void AlertaNulo_NaoGeraEtapa()
    {
        Assert.That(EtapasGuiadasDeAlerta.Criar(null), Is.Empty);
    }

    #endregion

    #region MARK: Tutoriais

    [Test]
    public void Tutoriais_PreservamAOrdemDasEtapas()
    {
        Etapa[] etapas =
        {
            new Etapa { tutorial = "primeira", animacao = string.Empty },
            new Etapa { tutorial = "segunda", animacao = string.Empty },
            new Etapa { tutorial = "terceira", animacao = string.Empty },
        };

        Assert.That(EtapasGuiadasDeAlerta.Tutoriais(etapas), Is.EqualTo(new[] { "primeira", "segunda", "terceira" }));
    }

    [TestCaseSource(nameof(CodigosOficiaisDoManual))]
    public void Tutoriais_RepetemAsAcoesOficiaisNaOrdem(string codigo)
    {
        AlertaOficial alerta = Buscar(codigo);

        string[] tutoriais = EtapasGuiadasDeAlerta.Tutoriais(EtapasGuiadasDeAlerta.Criar(alerta));

        Assert.That(tutoriais, Is.EqualTo(alerta.Acoes.ToArray()), $"{codigo}: tutoriais.");
    }

    [Test]
    public void Tutoriais_TratamArrayNuloEEtapaNula()
    {
        Assert.That(EtapasGuiadasDeAlerta.Tutoriais(null), Is.Empty);
        Assert.That(EtapasGuiadasDeAlerta.Tutoriais(new Etapa[0]), Is.Empty);
        Assert.That(EtapasGuiadasDeAlerta.Tutoriais(new Etapa[] { null }), Is.EqualTo(new[] { string.Empty }));
    }

    #endregion
}
