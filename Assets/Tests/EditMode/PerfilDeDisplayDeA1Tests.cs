using System.Linq;
using NUnit.Framework;

public class PerfilDeDisplayDeA1Tests
{
    #region MARK: Fixture

    private const string CodigoA1 = "A1";
    private const string AcaoVerificarArComprimido = "Verificar o fornecimento de ar comprimido";
    private const string AcaoVerificarAvarias = "Verificar possíveis avarias no conjunto válvula / atuador";

    private AlertaOficial a1;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a1 = CatalogoDeAlertas.Obter(CodigoA1, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA1);
    }

    private SequenciaDeQuadrosM4 EtapaArComprimido => perfil.EtapaOficial(0);
    private SequenciaDeQuadrosM4 EtapaAvarias => perfil.EtapaOficial(1);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA1_MantemAsDuasAcoesDaPagina11()
    {
        Assert.That(a1, Is.Not.Null);
        Assert.That(a1.Nome, Is.EqualTo("ÂNGULO MÍNIMO"));
        Assert.That(a1.Acoes.Count, Is.EqualTo(2));
        Assert.That(a1.Acoes[0], Is.EqualTo(AcaoVerificarArComprimido));
        Assert.That(a1.Acoes[1], Is.EqualTo(AcaoVerificarAvarias));
    }

    [Test]
    public void PerfilDeA1_TemExatamenteDuasEtapasOficiais()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA1));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(2));
        Assert.That(perfil.CorrespondeAoCatalogo(a1), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A1NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A24"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA1));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain("A2"));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(9));
    }

    #endregion

    #region MARK: Camada de animacao dedicada

    [Test]
    public void PerfilDeA1_UsaALayerProblema1EmVezDaBaseLayer()
    {
        Assert.That(perfil.Layer, Is.EqualTo("Problema 1"));
    }

    #endregion

    #region MARK: Quadros simples, um por passo oficial

    [Test]
    public void EtapaArComprimido_TemUmUnicoQuadroComAAnimacaoProblema1()
    {
        Assert.That(EtapaArComprimido.Quantidade, Is.EqualTo(1));

        QuadroDeDisplayM4 quadro = EtapaArComprimido.Primeiro;
        Assert.That(quadro.TextoLcd, Is.EqualTo(CodigoA1));
        Assert.That(quadro.Instrucao, Is.EqualTo(AcaoVerificarArComprimido));
        Assert.That(quadro.Animacao, Is.EqualTo("PROBLEMA1"));
        Assert.That(quadro.Vfx, Is.Null.Or.Empty);
    }

    [Test]
    public void EtapaAvarias_TemUmUnicoQuadroComOVfxDeDestaque()
    {
        Assert.That(EtapaAvarias.Quantidade, Is.EqualTo(1));

        QuadroDeDisplayM4 quadro = EtapaAvarias.Primeiro;
        Assert.That(quadro.TextoLcd, Is.EqualTo(CodigoA1));
        Assert.That(quadro.Instrucao, Is.EqualTo(AcaoVerificarAvarias));
        Assert.That(quadro.Vfx, Is.EqualTo("DestaqueAtuadorCopo"));
        Assert.That(quadro.Animacao, Is.Null.Or.Empty);
    }

    #endregion

    #region MARK: Composicao sobre as etapas guiadas

    [Test]
    public void EtapasDeA1_ExpandemAsDuasAcoesOficiaisMantendoUmQuadroCadaUma()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a1, EtapasGuiadasDeAlerta.Criar(a1));

        Assert.That(etapas, Has.Length.EqualTo(2));
        Assert.That(etapas[0].tutorial, Is.EqualTo(AcaoVerificarArComprimido));
        Assert.That(etapas[0].animacao, Is.EqualTo("PROBLEMA1"));
        Assert.That(etapas[0].vfx, Is.Null.Or.Empty);
        Assert.That(etapas[1].tutorial, Is.EqualTo(AcaoVerificarAvarias));
        Assert.That(etapas[1].vfx, Is.EqualTo("DestaqueAtuadorCopo"));
        Assert.That(etapas.All(TelaM4.EtapaTemConteudo), Is.True);
    }

    #endregion
}
