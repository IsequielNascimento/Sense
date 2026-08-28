using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class FormatadorDeDetalheDeAlertaTests
{
    #region MARK: Fixture

    private static readonly string[] ComNota =
    {
        "A19", "A20", "A21", "A22", "A23", "A24", "A25",
    };

    private static readonly string[] SemNota =
    {
        "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A9",
        "A11", "A12", "A13", "A14", "A15", "A16", "A17", "A18",
    };

    private const string NotaEsperada =
        "Ao desabilitar os alertas de 19 a 25, apenas as notificações no aplicativo serão desabilitadas. A indicação física no monitor, continua funcionando normalmente.";

    private IReadOnlyList<AlertaOficial> catalogo;

    [SetUp]
    public void CarregarCatalogo()
    {
        catalogo = CatalogoDeAlertas.Carregar();
    }

    private static IEnumerable<string> Codigos() => CodigosOficiais.Alertas;

    private AlertaOficial Buscar(string codigo)
    {
        AlertaOficial alerta = catalogo.FirstOrDefault(item => item.Codigo == codigo);

        Assert.That(alerta, Is.Not.Null, $"Alerta '{codigo}' ausente do catalogo.");

        return alerta;
    }

    #endregion

    #region MARK: Titulo

    [TestCaseSource(nameof(Codigos))]
    public void Formatar_TituloTrazCodigoENome(string codigo)
    {
        AlertaOficial alerta = Buscar(codigo);
        DetalheDeAlertaFormatado detalhe = FormatadorDeDetalheDeAlerta.Formatar(alerta);

        Assert.That(detalhe.Titulo, Is.EqualTo($"{alerta.Codigo} - {alerta.Nome}"));
    }

    #endregion

    #region MARK: Descricao

    [TestCaseSource(nameof(Codigos))]
    public void Formatar_DescricaoTrazOQueEEQuandoOcorreNaOrdem(string codigo)
    {
        AlertaOficial alerta = Buscar(codigo);
        DetalheDeAlertaFormatado detalhe = FormatadorDeDetalheDeAlerta.Formatar(alerta);

        Assert.That(detalhe.Descricao, Does.Contain(FormatadorDeDetalheDeAlerta.RotuloOQueE));
        Assert.That(detalhe.Descricao, Does.Contain(alerta.OQueE));
        Assert.That(detalhe.Descricao, Does.Contain(FormatadorDeDetalheDeAlerta.RotuloQuandoOcorre));
        Assert.That(detalhe.Descricao, Does.Contain(alerta.QuandoOcorre));

        int indiceOQueE = detalhe.Descricao.IndexOf(FormatadorDeDetalheDeAlerta.RotuloOQueE, StringComparison.Ordinal);
        int indiceQuandoOcorre = detalhe.Descricao.IndexOf(FormatadorDeDetalheDeAlerta.RotuloQuandoOcorre, StringComparison.Ordinal);

        Assert.That(indiceOQueE, Is.LessThan(indiceQuandoOcorre), $"{codigo}: rotulos fora de ordem.");
    }

    #endregion

    #region MARK: Solucao - acoes e locais

    [TestCaseSource(nameof(Codigos))]
    public void Formatar_SolucaoNumeraAsAcoesPreservandoOrdemEQuantidade(string codigo)
    {
        AlertaOficial alerta = Buscar(codigo);
        DetalheDeAlertaFormatado detalhe = FormatadorDeDetalheDeAlerta.Formatar(alerta);

        Assert.That(detalhe.Solucao, Does.Contain(FormatadorDeDetalheDeAlerta.RotuloOQueFazer));

        int posicaoAnterior = -1;

        for (int indice = 0; indice < alerta.Acoes.Count; indice++)
        {
            string esperado = $"{indice + 1} - {alerta.Acoes[indice]}";
            int posicao = detalhe.Solucao.IndexOf(esperado, StringComparison.Ordinal);

            Assert.That(posicao, Is.GreaterThan(-1), $"{codigo}: acao '{esperado}' ausente.");
            Assert.That(posicao, Is.GreaterThan(posicaoAnterior), $"{codigo}: acao fora de ordem.");

            posicaoAnterior = posicao;
        }
    }

    [TestCaseSource(nameof(Codigos))]
    public void Formatar_SolucaoPreservaOrdemEQuantidadeDosLocais(string codigo)
    {
        AlertaOficial alerta = Buscar(codigo);
        DetalheDeAlertaFormatado detalhe = FormatadorDeDetalheDeAlerta.Formatar(alerta);

        Assert.That(detalhe.Solucao, Does.Contain(FormatadorDeDetalheDeAlerta.RotuloOnde));

        int posicaoAnterior = -1;

        foreach (string local in alerta.Locais)
        {
            int posicao = detalhe.Solucao.IndexOf(local, StringComparison.Ordinal);

            Assert.That(posicao, Is.GreaterThan(-1), $"{codigo}: local '{local}' ausente.");
            Assert.That(posicao, Is.GreaterThan(posicaoAnterior), $"{codigo}: local fora de ordem.");

            posicaoAnterior = posicao;
        }
    }

    #endregion

    #region MARK: Solucao - padrao

    [TestCaseSource(nameof(Codigos))]
    public void Formatar_SolucaoTrazPadraoQuandoPreenchido(string codigo)
    {
        AlertaOficial alerta = Buscar(codigo);
        DetalheDeAlertaFormatado detalhe = FormatadorDeDetalheDeAlerta.Formatar(alerta);

        Assert.That(detalhe.Solucao, Does.Contain(FormatadorDeDetalheDeAlerta.RotuloPadrao));
        Assert.That(detalhe.Solucao, Does.Contain(alerta.Padrao));
    }

    [Test]
    public void Formatar_OmiteOPadraoQuandoVazio()
    {
        var estrutura = new AlertaEstrutural
        {
            codigo = "A1",
            categoria = AlertaOficial.CategoriaFuncional,
            quantidadeDeAcoes = 1,
            quantidadeDeLocais = 1,
        };

        var texto = new TextoDeAlerta
        {
            codigo = "A1",
            nome = "TESTE",
            oQueE = "descricao de teste",
            quandoOcorre = "ocorrencia de teste",
            acoes = new[] { "Acao unica" },
            locais = new[] { "em campo" },
            padrao = string.Empty,
        };

        var alerta = new AlertaOficial(estrutura, texto, 11, 76);
        DetalheDeAlertaFormatado detalhe = FormatadorDeDetalheDeAlerta.Formatar(alerta);

        Assert.That(detalhe.Solucao, Does.Not.Contain(FormatadorDeDetalheDeAlerta.RotuloPadrao));
    }

    #endregion

    #region MARK: Solucao - nota normativa

    [TestCaseSource(nameof(ComNota))]
    public void Formatar_AlertasDeDiagnostico_RecebemANotaLiteralNaSolucao(string codigo)
    {
        AlertaOficial alerta = Buscar(codigo);
        DetalheDeAlertaFormatado detalhe = FormatadorDeDetalheDeAlerta.Formatar(alerta);

        Assert.That(alerta.Nota, Is.EqualTo(NotaEsperada));
        Assert.That(detalhe.Solucao, Does.Contain(FormatadorDeDetalheDeAlerta.RotuloNota));
        Assert.That(detalhe.Solucao, Does.Contain(NotaEsperada));
    }

    [TestCaseSource(nameof(SemNota))]
    public void Formatar_AlertasQueNaoSaoDeDiagnostico_NaoRecebemNota(string codigo)
    {
        AlertaOficial alerta = Buscar(codigo);
        DetalheDeAlertaFormatado detalhe = FormatadorDeDetalheDeAlerta.Formatar(alerta);

        Assert.That(alerta.Nota, Is.Empty, $"{codigo} nao deveria ter nota.");
        Assert.That(detalhe.Solucao, Does.Not.Contain(FormatadorDeDetalheDeAlerta.RotuloNota));
    }

    #endregion

    #region MARK: Aviso do manual

    private const string TextoDoAvisoA9 =
        "Alertamos para o risco de danos ao equipamento caso a pressão da linha exceda 9 bar (130,5 psi).”";

    [Test]
    public void Formatar_A9_TrazNivelTextoEOrigemDoAvisoNaSolucao()
    {
        AlertaOficial alerta = Buscar("A9");
        DetalheDeAlertaFormatado detalhe = FormatadorDeDetalheDeAlerta.Formatar(alerta);

        Assert.That(detalhe.Solucao, Does.Contain("<b>IMPORTANTE</b>"));
        Assert.That(detalhe.Solucao, Does.Contain(TextoDoAvisoA9));
        Assert.That(detalhe.Solucao, Does.Contain("Manual, p. 5"));
    }

    [Test]
    public void Formatar_A9_NaoUsaRotuloDePerigoParaOAviso()
    {
        AlertaOficial alerta = Buscar("A9");
        DetalheDeAlertaFormatado detalhe = FormatadorDeDetalheDeAlerta.Formatar(alerta);

        Assert.That(detalhe.Solucao, Does.Not.Contain("PERIGO"));
    }

    [TestCaseSource(nameof(Codigos))]
    public void Formatar_ApenasA9_TrazBlocoDeAvisoNaSolucao(string codigo)
    {
        AlertaOficial alerta = Buscar(codigo);
        DetalheDeAlertaFormatado detalhe = FormatadorDeDetalheDeAlerta.Formatar(alerta);

        if (codigo == "A9")
        {
            Assert.That(detalhe.Solucao, Does.Contain("<b>IMPORTANTE</b>"));
        }
        else
        {
            Assert.That(detalhe.Solucao, Does.Not.Contain("<b>IMPORTANTE</b>"));
        }
    }

    #endregion

    #region MARK: Rotulos por idioma

    [TestCase("en", "What is it?", "When does it occur?", "What to do?", "Where")]
    [TestCase("es", "¿Qué es?", "¿Cuándo ocurre?", "¿Qué hacer?", "Dónde")]
    [TestCase("fr", "Qu'est-ce que c'est ?", "Quand cela se produit-il ?", "Que faire ?", "Où")]
    public void Formatar_ComIdiomaTraduzido_UsaOsRotulosDoIdioma(
        string idioma, string oQueE, string quandoOcorre, string oQueFazer, string onde)
    {
        AlertaOficial alerta = Buscar("A1");
        DetalheDeAlertaFormatado detalhe = FormatadorDeDetalheDeAlerta.Formatar(alerta, idioma);

        Assert.That(detalhe.Descricao, Does.Contain(oQueE));
        Assert.That(detalhe.Descricao, Does.Contain(quandoOcorre));
        Assert.That(detalhe.Solucao, Does.Contain(oQueFazer));
        Assert.That(detalhe.Solucao, Does.Contain(onde));

        Assert.That(detalhe.Descricao, Does.Not.Contain(FormatadorDeDetalheDeAlerta.RotuloOQueE));
        Assert.That(detalhe.Solucao, Does.Not.Contain(FormatadorDeDetalheDeAlerta.RotuloOQueFazer));
    }

    [TestCase("de")]
    [TestCase("")]
    [TestCase(null)]
    public void Formatar_ComIdiomaDesconhecido_CaiParaOsRotulosPt(string idioma)
    {
        AlertaOficial alerta = Buscar("A1");
        DetalheDeAlertaFormatado detalhe = FormatadorDeDetalheDeAlerta.Formatar(alerta, idioma);

        Assert.That(detalhe.Descricao, Does.Contain(FormatadorDeDetalheDeAlerta.RotuloOQueE));
        Assert.That(detalhe.Solucao, Does.Contain(FormatadorDeDetalheDeAlerta.RotuloOQueFazer));
    }

    [Test]
    public void Formatar_SemIdioma_MantemOsRotulosPt()
    {
        AlertaOficial alerta = Buscar("A1");
        DetalheDeAlertaFormatado detalhe = FormatadorDeDetalheDeAlerta.Formatar(alerta);

        Assert.That(detalhe.Descricao, Does.Contain(FormatadorDeDetalheDeAlerta.RotuloOQueE));
        Assert.That(detalhe.Solucao, Does.Contain(FormatadorDeDetalheDeAlerta.RotuloOnde));
    }

    [TestCase("pt", "IMPORTANTE")]
    [TestCase("en", "IMPORTANT")]
    [TestCase("es", "IMPORTANTE")]
    [TestCase("fr", "IMPORTANT")]
    public void Formatar_AvisoDoA9_UsaORotuloDeNivelDoIdioma(string idioma, string rotulo)
    {
        AlertaOficial alerta = Buscar("A9");
        DetalheDeAlertaFormatado detalhe = FormatadorDeDetalheDeAlerta.Formatar(alerta, idioma);

        Assert.That(detalhe.Solucao, Does.Contain($"<b>{rotulo}</b>"));
    }

    [TestCase("pt", "PERIGO")]
    [TestCase("en", "DANGER")]
    [TestCase("es", "PELIGRO")]
    [TestCase("fr", "DANGER")]
    [TestCase("de", "PERIGO")]
    public void RotuloExibicao_TraduzONivelDeAviso(string idioma, string esperado)
    {
        Assert.That(NivelDeAviso.Perigo.RotuloExibicao(idioma), Is.EqualTo(esperado));
    }

    #endregion
}
