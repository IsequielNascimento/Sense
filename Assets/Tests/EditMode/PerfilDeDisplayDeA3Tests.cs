using System.Linq;
using NUnit.Framework;

public class PerfilDeDisplayDeA3Tests
{
    #region MARK: Fixture

    private const string CodigoA3 = "A3";
    private const string AcaoVerificarArComprimido = "Verificar o fornecimento de ar comprimido";
    private const string AcaoVerificarAvarias = "Verificar possíveis avarias no conjunto válvula / atuador";
    private const int LimiteDeCaracteresDaInstrucao = 120;

    private AlertaOficial a3;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a3 = CatalogoDeAlertas.Obter(CodigoA3, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA3);
    }

    private SequenciaDeQuadrosM4 EtapaArComprimido => perfil.EtapaOficial(0);
    private SequenciaDeQuadrosM4 EtapaAvarias => perfil.EtapaOficial(1);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA3_MantemAsDuasAcoesDaPagina76()
    {
        Assert.That(a3, Is.Not.Null);
        Assert.That(a3.Nome, Is.EqualTo("FALHA NA AUTO CALIBRAÇÃO"));
        Assert.That(a3.Acoes.Count, Is.EqualTo(2));
        Assert.That(a3.Acoes[0], Is.EqualTo(AcaoVerificarArComprimido));
        Assert.That(a3.Acoes[1], Is.EqualTo(AcaoVerificarAvarias));
    }

    [Test]
    public void OndeOficialDoPrimeiroPasso_EOGeradorDeArComprimido()
    {
        Assert.That(a3.Locais.Count, Is.EqualTo(2));
        Assert.That(a3.Locais[0], Is.EqualTo("gerador de ar comprimido"));
        Assert.That(a3.Locais[1], Is.EqualTo("em campo"));
    }

    [Test]
    public void PerfilDeA3_ExpandeAsDuasAcoesOficiaisSemInventarUmaTerceira()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA3));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(2));
        Assert.That(perfil.CorrespondeAoCatalogo(a3), Is.True);
    }

    #endregion

    #region MARK: Passo a passo auto explicativo, no padrao do A2

    [Test]
    public void NenhumQuadro_RepeteOTextoResumidoDaTabelaDeResolucao()
    {
        var instrucoes = perfil.EtapasOficiais
            .SelectMany(etapa => etapa.Quadros)
            .Select(quadro => quadro.Instrucao)
            .ToList();

        Assert.That(instrucoes, Has.None.EqualTo(AcaoVerificarArComprimido));
        Assert.That(instrucoes, Has.None.EqualTo(AcaoVerificarAvarias));
    }

    [Test]
    public void Instrucoes_CabemNaCaixaDeTextoDoPassoAPasso()
    {
        foreach (QuadroDeDisplayM4 quadro in perfil.EtapasOficiais.SelectMany(etapa => etapa.Quadros))
        {
            Assert.That(
                quadro.Instrucao.Length,
                Is.LessThanOrEqualTo(LimiteDeCaracteresDaInstrucao),
                $"Instrução longa demais no quadro '{quadro.TextoLcd}'.");
        }
    }

    [Test]
    public void EtapaArComprimido_GuiaAVerificacaoNoGeradorEmSeteQuadros()
    {
        Assert.That(EtapaArComprimido.Quantidade, Is.EqualTo(7));
        Assert.That(EtapaArComprimido.Primeiro.TextoLcd, Is.EqualTo(CodigoA3));
        Assert.That(EtapaArComprimido.Primeiro.Instrucao, Does.StartWith("Confirme o alerta A3"));

        Assert.That(
            EtapaArComprimido.Quadros.Count(quadro => quadro.Vfx == "DestaquePneumatica"),
            Is.EqualTo(1),
            "o bloco pneumático é destacado onde o texto fala da entrada de ar.");
    }

    [Test]
    public void OQuadroQueCitaAsMangueiras_DestacaAsMangueiras()
    {
        QuadroDeDisplayM4 quadro = EtapaArComprimido.Quadros
            .Single(item => item.Vfx == "DestaqueMangueiras");

        Assert.That(quadro.Instrucao, Does.Contain("mangueiras"));
    }

    [Test]
    public void EtapaArComprimido_NaoDisparaAnimacaoDeBotaoPorqueEUmaVerificacaoEmCampo()
    {
        Assert.That(
            EtapaArComprimido.Quadros.Where(quadro => !string.IsNullOrEmpty(quadro.Animacao)),
            Is.Empty);
    }

    [Test]
    public void EtapaAvarias_InspecionaOConjuntoERefazACalibracaoEmOitoQuadros()
    {
        Assert.That(EtapaAvarias.Quantidade, Is.EqualTo(8));
        Assert.That(
            EtapaAvarias.Quadros.Count(quadro => quadro.Vfx == "DestaqueAtuadorCopo"),
            Is.EqualTo(3));
    }

    #endregion

    #region MARK: Procedimento de auto calibracao da pagina 49

    [Test]
    public void RefazerACalibracao_SegueOsBotoesEOsTemposDoManual()
    {
        var quadros = EtapaAvarias.Quadros;

        Assert.That(quadros[4].TextoLcd, Is.EqualTo("FAST\nSETUP"));
        Assert.That(quadros[4].Animacao, Is.EqualTo("B3Button"));
        Assert.That(quadros[4].ProgressoSegundos, Is.EqualTo(6f));

        Assert.That(quadros[5].TextoLcd, Is.EqualTo("CERTO"));
        Assert.That(quadros[5].Animacao, Is.EqualTo("B2Button"));

        Assert.That(quadros[6].TextoLcd, Is.EqualTo("ABORT"));
        Assert.That(quadros[6].Animacao, Is.EqualTo("B1Button"));
        Assert.That(quadros[6].ProgressoSegundos, Is.EqualTo(3f));
    }

    [Test]
    public void OUltimoQuadro_ConfirmaAResolucaoComOsLedsDesligados()
    {
        QuadroDeDisplayM4 ultimo = EtapaAvarias.Quadros.Last();

        Assert.That(ultimo.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(ultimo.LedPiscando, Is.False);
        Assert.That(ultimo.Instrucao, Does.Contain("alerta A3 é eliminado"));
    }

    #endregion

    #region MARK: Composicao sobre as etapas guiadas

    [Test]
    public void EtapasDeA3_ExpandemAsDuasAcoesOficiaisEmQuinzePassosGuiados()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a3, EtapasGuiadasDeAlerta.Criar(a3));

        Assert.That(etapas, Has.Length.EqualTo(15));
        Assert.That(etapas[0].tutorial, Does.StartWith("Confirme o alerta A3"));
        Assert.That(etapas[1].vfx, Is.EqualTo("DestaquePneumatica"));
        Assert.That(etapas[7].vfx, Is.EqualTo("DestaqueAtuadorCopo"));
        Assert.That(etapas.All(TelaM4.EtapaTemConteudo), Is.True);
    }

    #endregion
}
