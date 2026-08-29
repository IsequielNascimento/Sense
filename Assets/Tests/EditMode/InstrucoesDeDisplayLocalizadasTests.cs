using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class InstrucoesDeDisplayLocalizadasTests
{
    #region MARK: Instrucoes canonicas dos perfis

    private static readonly string[] IdiomasTraduzidos = { "en", "es", "fr" };

    private static IEnumerable<string> InstrucoesCanonicas()
    {
        foreach (string codigo in PerfisDeDisplayDeAlerta.CodigosComPerfil)
        {
            PerfilDeDisplayDeAlerta perfil = PerfisDeDisplayDeAlerta.Obter(codigo);

            for (int etapa = 0; etapa < perfil.QuantidadeDeEtapasOficiais; etapa++)
            {
                foreach (QuadroDeDisplayM4 quadro in perfil.EtapaOficial(etapa).Quadros)
                {
                    if (string.IsNullOrWhiteSpace(quadro.Instrucao)) continue;

                    yield return quadro.Instrucao;
                }
            }
        }
    }

    #endregion

    #region MARK: Cobertura das traducoes

    [TestCase("en")]
    [TestCase("es")]
    [TestCase("fr")]
    public void TodaInstrucaoDosPerfis_TemTraducao(string idioma)
    {
        IReadOnlyDictionary<string, string> tabela = InstrucoesDeDisplayLocalizadas.Tabela(idioma);

        Assert.That(tabela, Is.Not.Null, $"Tabela de '{idioma}' nao carregada.");

        string[] semTraducao = InstrucoesCanonicas()
            .Distinct()
            .Where(instrucao => !tabela.TryGetValue(instrucao, out string texto)
                                || string.IsNullOrWhiteSpace(texto))
            .ToArray();

        Assert.That(
            semTraducao,
            Is.Empty,
            $"Instrucoes sem traducao em '{idioma}':\n{string.Join("\n", semTraducao)}");
    }

    [TestCase("en")]
    [TestCase("es")]
    [TestCase("fr")]
    public void NenhumaTraducao_RepeteOTextoEmPortugues(string idioma)
    {
        IReadOnlyDictionary<string, string> tabela = InstrucoesDeDisplayLocalizadas.Tabela(idioma);

        string[] naoTraduzidas = tabela
            .Where(par => string.Equals(par.Key, par.Value))
            .Select(par => par.Key)
            .ToArray();

        Assert.That(naoTraduzidas, Is.Empty, $"Entradas ainda em portugues em '{idioma}'.");
    }

    [Test]
    public void PortuguesNaoTemTabela_PoisEOIdiomaCanonico()
    {
        Assert.That(InstrucoesDeDisplayLocalizadas.Tabela("pt"), Is.Null);
    }

    [Test]
    public void IdiomaSemRevisaoTecnica_CaiNoPortugues()
    {
        Assert.That(InstrucoesDeDisplayLocalizadas.Tabela("de"), Is.Null);
    }

    #endregion

    #region MARK: Traducao aplicada nas etapas guiadas

    [TestCase("en")]
    [TestCase("es")]
    [TestCase("fr")]
    public void Traduzir_SubstituiAInstrucaoCanonica(string idioma)
    {
        string canonica = InstrucoesCanonicas().First();

        Assert.That(
            InstrucoesDeDisplayLocalizadas.Traduzir(canonica, idioma),
            Is.Not.EqualTo(canonica));
    }

    [Test]
    public void Traduzir_EmPortugues_MantemOTextoOriginal()
    {
        string canonica = InstrucoesCanonicas().First();

        Assert.That(InstrucoesDeDisplayLocalizadas.Traduzir(canonica, "pt"), Is.EqualTo(canonica));
    }

    [Test]
    public void Traduzir_TextoDesconhecido_MantemOTextoOriginal()
    {
        Assert.That(
            InstrucoesDeDisplayLocalizadas.Traduzir("texto que nao existe no catalogo", "en"),
            Is.EqualTo("texto que nao existe no catalogo"));
    }

    [Test]
    public void EtapasComDisplayDeAlerta_TraduzemOsTutoriaisDosQuadros()
    {
        AlertaOficial a8 = CatalogoDeAlertas.Obter("A8", "en");

        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(
            a8, EtapasGuiadasDeAlerta.Criar(a8), "en");

        var canonicas = new HashSet<string>(InstrucoesCanonicas());

        Assert.That(etapas, Is.Not.Empty);
        Assert.That(
            etapas.Where(etapa => canonicas.Contains(etapa.tutorial)).ToArray(),
            Is.Empty,
            "Nenhum passo guiado pode continuar com a instrucao em portugues.");
    }

    [Test]
    public void EtapasComDisplayDeAlerta_SemIdioma_MantemOPortugues()
    {
        AlertaOficial a8 = CatalogoDeAlertas.Obter("A8");

        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a8, EtapasGuiadasDeAlerta.Criar(a8));

        Assert.That(
            etapas[0].tutorial,
            Is.EqualTo(PerfisDeDisplayDeAlerta.Obter("A8").EtapaOficial(0).Primeiro.Instrucao));
    }

    #endregion
}
