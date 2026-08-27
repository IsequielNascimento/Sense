using System.Linq;
using NUnit.Framework;

public class PerfilDeDisplayDeA24Tests
{
    #region MARK: Fixture

    private const string CodigoA23 = "A23";
    private const string CodigoA24 = "A24";
    private const string AcaoVerificarOsFios = "Verificar os fios da solenoide";
    private const int LimiteDeCaracteresDaInstrucao = 120;

    private AlertaOficial a24;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a24 = CatalogoDeAlertas.Obter(CodigoA24, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA24);
    }

    private SequenciaDeQuadrosM4 EtapaFios => perfil.EtapaOficial(0);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA24_MantemAAcaoUnicaDaPagina76()
    {
        Assert.That(a24, Is.Not.Null);
        Assert.That(a24.Nome, Is.EqualTo("SOLENOIDE ABERTA"));
        Assert.That(a24.Padrao, Is.EqualTo("habilitado"));
        Assert.That(a24.Acoes.Count, Is.EqualTo(1));
        Assert.That(a24.Acoes[0], Is.EqualTo(AcaoVerificarOsFios));
        Assert.That(a24.Locais.Count, Is.EqualTo(1));
        Assert.That(a24.Locais[0], Is.EqualTo("em campo"));
    }

    [Test]
    public void PerfilDeA24_ExpandeAAcaoUnicaSemInventarUmaSegunda()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA24));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(1));
        Assert.That(perfil.CorrespondeAoCatalogo(a24), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A24NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A10"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA24));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA23));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Has.Count.EqualTo(24));
    }

    #endregion

    #region MARK: Passo a passo auto explicativo, no padrao do A5

    [Test]
    public void NenhumQuadro_RepeteOTextoResumidoDaTabelaDeResolucao()
    {
        Assert.That(
            EtapaFios.Quadros.Select(quadro => quadro.Instrucao),
            Has.None.EqualTo(AcaoVerificarOsFios),
            "esse é o texto curto que o A23 e o A24 dividem no catálogo.");
    }

    [Test]
    public void Instrucoes_CabemNaCaixaDeTextoDoPassoAPasso()
    {
        foreach (QuadroDeDisplayM4 quadro in EtapaFios.Quadros)
        {
            Assert.That(
                quadro.Instrucao.Length,
                Is.LessThanOrEqualTo(LimiteDeCaracteresDaInstrucao),
                $"Instrução longa demais no quadro '{quadro.TextoLcd}'.");
        }
    }

    [Test]
    public void TodoQuadro_TemTextoDeLcdEInstrucaoSemEspacoNasPontas()
    {
        foreach (QuadroDeDisplayM4 quadro in EtapaFios.Quadros)
        {
            Assert.That(quadro.TextoLcd, Is.EqualTo(CodigoA24));
            Assert.That(quadro.Instrucao, Is.Not.Null.And.Not.Empty);
            Assert.That(quadro.Instrucao, Is.EqualTo(quadro.Instrucao.Trim()));
        }
    }

    #endregion

    #region MARK: A fronteira com o A23, o defeito oposto, paginas 11 e 76

    [Test]
    public void PrimeiroQuadro_DizQueOCircuitoDaBobinaEstaAberto()
    {
        Assert.That(EtapaFios.Primeiro.Instrucao, Does.StartWith("Confirme o alerta A24"));
        Assert.That(EtapaFios.Primeiro.Instrucao, Does.Contain("rompido"));
        Assert.That(EtapaFios.Primeiro.Instrucao, Does.Contain("circuito ficou aberto"));
    }

    [Test]
    public void SegundoQuadro_NomeiaOA23ComoODefeitoOposto()
    {
        Assert.That(EtapaFios.Em(1).Instrucao, Does.Contain(CodigoA23));
        Assert.That(EtapaFios.Em(1).Instrucao, Does.Contain("oposto"));
    }

    [Test]
    public void ForaDoQuadroDeFronteira_NenhumPassoDiagnosticaCurto()
    {
        var foraDaFronteira = EtapaFios.Quadros
            .Where((quadro, indice) => indice != 1)
            .Select(quadro => quadro.Instrucao)
            .ToList();

        Assert.That(foraDaFronteira, Has.None.Contains("em curto"),
            "curto é o diagnóstico do A23, não o do A24.");
        Assert.That(foraDaFronteira, Has.None.Contains("isolamento derretido"),
            "isolamento derretido é o sinal que o A23 procura.");
    }

    [Test]
    public void AVerificacaoDoA24_ProcuraFaltaDeContinuidade()
    {
        Assert.That(EtapaFios.Em(4).Instrucao, Does.Contain("rompimento"));
        Assert.That(EtapaFios.Em(5).Instrucao, Does.Contain("continuidade"));
        Assert.That(EtapaFios.Em(6).Instrucao, Does.Contain("abre o circuito"));
        Assert.That(EtapaFios.Ultimo.Instrucao, Does.Contain("continuidade restabelecida"));
    }

    #endregion

    #region MARK: Seguranca e troca da bobina, paginas 18 e 19

    [Test]
    public void EtapaFios_AvisaSobreEnergiaEGuiaATrocaDaBobinaEmOnzeQuadros()
    {
        Assert.That(EtapaFios.Quantidade, Is.EqualTo(11));
        Assert.That(EtapaFios.Em(2).Instrucao, Does.StartWith("PERIGO"));
        Assert.That(EtapaFios.Em(3).Instrucao, Does.Contain("energia residual"));
        Assert.That(EtapaFios.Em(7).Instrucao, Does.Contain("dois parafusos"));
        Assert.That(EtapaFios.Em(8).Instrucao, Does.Contain("anel de vedação"));
    }

    #endregion

    #region MARK: Nota normativa dos alertas 19 a 25, pagina 11

    [Test]
    public void UmQuadro_ExplicaQueDesligarOAlertaNaoApagaAIndicacaoFisica()
    {
        Assert.That(EtapaFios.Em(9).Instrucao, Does.Contain("notificação no aplicativo"));
        Assert.That(EtapaFios.Em(9).Instrucao, Does.Contain("indicação física"));
    }

    #endregion

    #region MARK: Destaque piscante no lugar da verificacao fisica

    [Test]
    public void OperadorEmCampo_VeOndeOlharEmCadaVerificacao()
    {
        Assert.That(EtapaFios.Em(4).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaquePneumatica));
        Assert.That(EtapaFios.Em(5).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaquePneumatica));
        Assert.That(EtapaFios.Em(6).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueModuloEletronico));
        Assert.That(EtapaFios.Em(7).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaquePneumatica));
    }

    [Test]
    public void QuadrosSemVerificacaoFisica_NaoAcendemDestaqueNoModelo()
    {
        foreach (int indice in new[] { 0, 1, 2, 3, 8, 9, 10 })
        {
            Assert.That(EtapaFios.Em(indice).Vfx, Is.Null,
                $"o quadro {indice} não é de verificação física e não deve destacar peça.");
        }
    }

    #endregion

    #region MARK: Leds, animacao e composicao

    [Test]
    public void OUltimoQuadro_ConfirmaAResolucaoComOsLedsDesligados()
    {
        QuadroDeDisplayM4 ultimo = EtapaFios.Ultimo;

        Assert.That(ultimo.Leds, Is.EqualTo(EstadoLedsM4.Desligado));
        Assert.That(ultimo.LedPiscando, Is.False);
        Assert.That(ultimo.Instrucao, Does.StartWith("Verifique a confirmação"));
        Assert.That(ultimo.Instrucao, Does.Contain("A24 é eliminado"));
    }

    [Test]
    public void EnquantoOAlertaEstaAtivo_OLedVermelhoPisca()
    {
        var ativos = EtapaFios.Quadros.Where(quadro => quadro != EtapaFios.Ultimo).ToList();

        Assert.That(ativos.All(quadro => quadro.Leds == EstadoLedsM4.Alerta), Is.True);
        Assert.That(ativos.All(quadro => quadro.LedPiscando), Is.True);
    }

    [Test]
    public void AnimacoesDeA24_FicamVaziasPorqueNenhumPassoUsaOsBotoes()
    {
        Assert.That(
            EtapaFios.Quadros.Select(quadro => quadro.Animacao),
            Is.EqualTo(new string[] { null, null, null, null, null, null, null, null, null, null, null }));

        Assert.That(perfil.Layer, Is.Null, "o A24 roda na Base Layer.");
    }

    [Test]
    public void EtapasDeA24_ExpandemAAcaoOficialEmOnzePassosGuiados()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a24, EtapasGuiadasDeAlerta.Criar(a24));

        Assert.That(etapas, Has.Length.EqualTo(11));
        Assert.That(etapas[0].tutorial, Does.StartWith("Confirme o alerta A24"));
        Assert.That(etapas[4].vfx, Is.EqualTo("DestaquePneumatica"));
        Assert.That(etapas[6].vfx, Is.EqualTo("DestaqueModuloEletronico"));
        Assert.That(etapas.All(TelaM4.EtapaTemConteudo), Is.True);
    }

    #endregion

    #region MARK: O A24 nunca mostra o mesmo passo que o A23

    [Test]
    public void OsDoisAlertasDaSolenoide_NaoCompartilhamNenhumPassoDeDiagnostico()
    {
        var deA23 = PerfisDeDisplayDeAlerta.Obter(CodigoA23).EtapasOficiais
            .SelectMany(etapa => etapa.Quadros)
            .Select(quadro => quadro.Instrucao)
            .ToList();

        var repetidos = EtapaFios.Quadros
            .Select(quadro => quadro.Instrucao)
            .Intersect(deA23)
            .ToList();

        Assert.That(repetidos.All(texto => texto.StartsWith("PERIGO") || texto.Contains("energia residual")), Is.True,
            "só os avisos de segurança podem ser idênticos entre A23 e A24; o diagnóstico precisa diferenciá-los.");
    }

    #endregion
}
