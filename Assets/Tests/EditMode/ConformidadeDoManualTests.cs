using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class ConformidadeDoManualTests
{
    #region MARK: Fixture

    private IReadOnlyList<AlertaOficial> catalogo;

    [SetUp]
    public void CarregarCatalogo()
    {
        catalogo = CatalogoDeAlertas.Carregar("pt");
    }

    #endregion

    #region MARK: Conjunto e ordem oficiais

    [Test]
    public void Catalogo_ContemExatamenteOsCodigosOficiaisNaOrdemSemA10()
    {
        Assert.That(catalogo.Select(alerta => alerta.Codigo), Is.EqualTo(CodigosOficiais.Alertas.ToArray()));
        Assert.That(catalogo.Count, Is.EqualTo(24));
        Assert.That(catalogo.Any(alerta => alerta.Codigo == "A10"), Is.False);
    }

    #endregion

    #region MARK: Campos obrigatorios e contagens estruturais

    [Test]
    public void TodoAlerta_TemCamposObrigatoriosEContagensEstruturais()
    {
        foreach (AlertaOficial alerta in catalogo)
        {
            Assert.That(alerta.Codigo, Is.Not.Empty);
            Assert.That(alerta.Categoria, Is.Not.Empty, $"{alerta.Codigo}: categoria.");
            Assert.That(alerta.Nome, Is.Not.Empty, $"{alerta.Codigo}: nome.");
            Assert.That(alerta.OQueE, Is.Not.Empty, $"{alerta.Codigo}: 'O que e'.");
            Assert.That(alerta.QuandoOcorre, Is.Not.Empty, $"{alerta.Codigo}: 'Quando ocorre'.");
            Assert.That(alerta.Acoes, Is.Not.Empty, $"{alerta.Codigo}: acoes.");
            Assert.That(alerta.Locais, Is.Not.Empty, $"{alerta.Codigo}: locais.");
            Assert.That(alerta.Padrao, Is.Not.Empty, $"{alerta.Codigo}: padrao.");
            Assert.That(alerta.PaginaTabelaCodigos, Is.EqualTo(11), $"{alerta.Codigo}: pagina tabela codigos.");
            Assert.That(alerta.PaginaTabelaResolucao, Is.EqualTo(76), $"{alerta.Codigo}: pagina tabela resolucao.");
            Assert.That(alerta.Acoes.Count, Is.EqualTo(alerta.QuantidadeDeAcoesEsperada), $"{alerta.Codigo}: contagem de acoes.");
            Assert.That(alerta.Locais.Count, Is.EqualTo(alerta.QuantidadeDeLocaisEsperada), $"{alerta.Codigo}: contagem de locais.");
        }
    }

    #endregion

    #region MARK: Busca individual por codigo e nome

    [Test]
    public void BuscaDeAlertas_EncontraCadaAlertaPorCodigoEPorNomeExatos()
    {
        foreach (AlertaOficial alerta in catalogo)
        {
            IReadOnlyList<AlertaOficial> porCodigo = BuscaDeAlertas.Buscar(catalogo, alerta.Codigo);
            Assert.That(porCodigo.Any(item => item.Codigo == alerta.Codigo), Is.True, $"{alerta.Codigo}: busca por codigo.");

            IReadOnlyList<AlertaOficial> porNome = BuscaDeAlertas.Buscar(catalogo, alerta.Nome);
            Assert.That(porNome.Any(item => item.Codigo == alerta.Codigo), Is.True, $"{alerta.Codigo}: busca por nome '{alerta.Nome}'.");
        }
    }

    #endregion

    #region MARK: Nomes placeholder banidos

    private static readonly string[] TitulosPlaceholder =
    {
        "Falha na Calibracao", "Chave Magnetica", "Pareamento Bluetooth", "Folga Mecanica",
        "Erro de Torque", "Vazamento de Ar", "Falha no Sinal", "Manutencao",
    };

    [Test]
    public void NenhumNomeOficial_ContemTituloPlaceholder()
    {
        foreach (AlertaOficial alerta in catalogo)
        {
            string nomeNormalizado = Normalizar(alerta.Nome);

            foreach (string placeholder in TitulosPlaceholder)
            {
                Assert.That(nomeNormalizado, Does.Not.Contain(Normalizar(placeholder)),
                    $"{alerta.Codigo}: nome '{alerta.Nome}' contem titulo placeholder '{placeholder}'.");
            }
        }
    }

    private static string Normalizar(string texto)
    {
        string decomposto = texto.Normalize(NormalizationForm.FormD);
        var construtor = new StringBuilder();

        foreach (char caractere in decomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
            {
                construtor.Append(caractere);
            }
        }

        return construtor.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }

    #endregion

    #region MARK: Nota normativa restrita a A19-A25

    [Test]
    public void ApenasA19AteA25_PossuemNotaNormativa()
    {
        foreach (AlertaOficial alerta in catalogo)
        {
            bool esperaNota = CodigoEntre(alerta.Codigo, 19, 25);

            if (esperaNota)
            {
                Assert.That(alerta.Nota, Is.Not.Empty, $"{alerta.Codigo}: deveria ter nota normativa.");
            }
            else
            {
                Assert.That(alerta.Nota, Is.Empty, $"{alerta.Codigo}: nao deveria ter nota normativa.");
            }
        }
    }

    private static bool CodigoEntre(string codigo, int minimo, int maximo)
    {
        int numero = int.Parse(codigo.Substring(1));
        return numero >= minimo && numero <= maximo;
    }

    #endregion

    #region MARK: Aviso restrito a A9

    private static readonly string[] AlertasSemAvisoPorInferencia = { "A6", "A19", "A20", "A23", "A24", "A25" };

    [Test]
    public void ApenasA9_PossuiAvisoImportanteNaPagina5()
    {
        foreach (AlertaOficial alerta in catalogo)
        {
            if (alerta.Codigo == "A9")
            {
                Assert.That(alerta.Avisos.Count, Is.EqualTo(1));
                Assert.That(alerta.Avisos[0].Nivel, Is.EqualTo(NivelDeAviso.Importante));
                Assert.That(alerta.Avisos[0].Pagina, Is.EqualTo(5));
            }
            else
            {
                Assert.That(alerta.Avisos, Is.Empty, $"{alerta.Codigo}: nao deveria ter aviso.");
            }
        }
    }

    [TestCaseSource(nameof(AlertasSemAvisoPorInferencia))]
    public void AlertasBloqueados_PermanecemSemAvisoPorInferencia(string codigo)
    {
        AlertaOficial alerta = catalogo.First(item => item.Codigo == codigo);
        Assert.That(alerta.Avisos, Is.Empty, $"{codigo}: nao pode receber aviso sem aprovacao tecnica.");
    }

    #endregion

    #region MARK: Cenarios legados fora do dominio Alerta

    private const string PastaEstrutural = "Assets/Resources/BancoDeDadosProblemas/estrutura";

    private static readonly string[] CenariosLegados =
    {
        "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8"
    };

    private static EstruturaCenario CarregarEstrutura(string cenario)
    {
        string caminho = $"{PastaEstrutural}/{cenario}.json";
        TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(caminho);

        Assert.That(asset, Is.Not.Null, $"Arquivo nao encontrado: {caminho}");

        return JsonUtility.FromJson<EstruturaCenario>(asset.text);
    }

    [Test]
    public void NenhumAlertaOficial_TemCodigoC()
    {
        Assert.That(catalogo.Any(alerta => alerta.Codigo.StartsWith("C")), Is.False);
        Assert.That(CodigosOficiais.Alertas.Any(codigo => codigo.StartsWith("C")), Is.False);
    }

    [TestCaseSource(nameof(CenariosLegados))]
    public void CenarioLegado_FicaForaDoDominioAlertaENaoEApresentavelComoAlertaPublico(string cenario)
    {
        EstruturaCenario estrutura = CarregarEstrutura(cenario);

        Assert.That(estrutura, Is.Not.Null, $"{cenario}: estrutura ausente.");
        Assert.That(estrutura.dominio, Is.Not.EqualTo(DominioDeCenario.Alerta), $"{cenario}: nao deveria pertencer ao dominio Alerta.");
        Assert.That(DominioDeCenario.PodeSerApresentadoComoAlertaPublico(estrutura), Is.False, $"{cenario}: nao pode ser apresentado como alerta publico.");
    }

    #endregion

    #region MARK: Assinatura tecnica identica entre idiomas

    private static readonly string[] IdiomasTecnicos = { "pt", "en", "es", "fr" };

    private static string AssinaturaTecnica(AlertaOficial alerta)
    {
        var construtor = new StringBuilder();

        construtor.Append(alerta.Codigo).Append('|');
        construtor.Append(alerta.Categoria).Append('|');
        construtor.Append(alerta.Nome).Append('|');
        construtor.Append(alerta.OQueE).Append('|');
        construtor.Append(alerta.QuandoOcorre).Append('|');
        construtor.Append(string.Join(",", alerta.Acoes)).Append('|');
        construtor.Append(string.Join(",", alerta.Locais)).Append('|');
        construtor.Append(alerta.Padrao).Append('|');
        construtor.Append(alerta.Nota).Append('|');
        construtor.Append(alerta.PaginaTabelaCodigos).Append('|');
        construtor.Append(alerta.PaginaTabelaResolucao).Append('|');

        foreach (AvisoOficial aviso in alerta.Avisos)
        {
            construtor.Append(aviso.Id).Append(':').Append(aviso.Nivel).Append(':').Append(aviso.Pagina).Append(':').Append(aviso.Texto).Append(';');
        }

        return construtor.ToString();
    }

    [TestCaseSource(nameof(IdiomasTecnicos))]
    public void TodoIdiomaTecnico_RetornaAMesmaAssinaturaTecnicaCompleta(string idioma)
    {
        IReadOnlyList<AlertaOficial> catalogoPt = CatalogoDeAlertas.Carregar("pt");
        IReadOnlyList<AlertaOficial> catalogoIdioma = CatalogoDeAlertas.Carregar(idioma);

        IEnumerable<string> assinaturasPt = catalogoPt.Select(AssinaturaTecnica);
        IEnumerable<string> assinaturasIdioma = catalogoIdioma.Select(AssinaturaTecnica);

        Assert.That(assinaturasIdioma, Is.EqualTo(assinaturasPt), $"idioma '{idioma}': assinatura tecnica divergente do pt.");
    }

    #endregion
}
