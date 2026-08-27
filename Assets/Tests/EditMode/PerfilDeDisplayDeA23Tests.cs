using System.Linq;
using NUnit.Framework;

public class PerfilDeDisplayDeA23Tests
{
    #region MARK: Fixture

    private const string CodigoA23 = "A23";
    private const string CodigoA24 = "A24";
    private const string AcaoVerificarOsFios = "Verificar os fios da solenoide";
    private const int LimiteDeCaracteresDaInstrucao = 120;

    private AlertaOficial a23;
    private PerfilDeDisplayDeAlerta perfil;

    [SetUp]
    public void Carregar()
    {
        a23 = CatalogoDeAlertas.Obter(CodigoA23, "pt");
        perfil = PerfisDeDisplayDeAlerta.Obter(CodigoA23);
    }

    private SequenciaDeQuadrosM4 EtapaFios => perfil.EtapaOficial(0);

    #endregion

    #region MARK: Fidelidade ao catalogo oficial

    [Test]
    public void CatalogoDeA23_MantemAAcaoUnicaDaPagina76()
    {
        Assert.That(a23, Is.Not.Null);
        Assert.That(a23.Nome, Is.EqualTo("SOLENOIDE CURTO"));
        Assert.That(a23.Padrao, Is.EqualTo("habilitado"));
        Assert.That(a23.Acoes.Count, Is.EqualTo(1));
        Assert.That(a23.Acoes[0], Is.EqualTo(AcaoVerificarOsFios));
        Assert.That(a23.Locais.Count, Is.EqualTo(1));
        Assert.That(a23.Locais[0], Is.EqualTo("em campo"));
    }

    [Test]
    public void PerfilDeA23_ExpandeAAcaoUnicaSemInventarUmaSegunda()
    {
        Assert.That(perfil, Is.Not.Null);
        Assert.That(perfil.Codigo, Is.EqualTo(CodigoA23));
        Assert.That(perfil.QuantidadeDeEtapasOficiais, Is.EqualTo(1));
        Assert.That(perfil.CorrespondeAoCatalogo(a23), Is.True);
    }

    [Test]
    public void RegistroDePerfis_A23NaoSelecionaPerfilDeOutroAlerta()
    {
        Assert.That(PerfisDeDisplayDeAlerta.Obter("A10"), Is.Null);
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA23));
        Assert.That(PerfisDeDisplayDeAlerta.CodigosComPerfil, Does.Contain(CodigoA24));
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
            Assert.That(quadro.TextoLcd, Is.EqualTo(CodigoA23));
            Assert.That(quadro.Instrucao, Is.Not.Null.And.Not.Empty);
            Assert.That(quadro.Instrucao, Is.EqualTo(quadro.Instrucao.Trim()));
        }
    }

    #endregion

    #region MARK: A fronteira com o A24, o defeito oposto, paginas 11 e 76

    [Test]
    public void PrimeiroQuadro_DizQueOCircuitoDaBobinaEstaFechadoEmCurto()
    {
        Assert.That(EtapaFios.Primeiro.Instrucao, Does.StartWith("Confirme o alerta A23"));
        Assert.That(EtapaFios.Primeiro.Instrucao, Does.Contain("curto circuito"));
        Assert.That(EtapaFios.Primeiro.Instrucao, Does.Contain("caminho fechado"));
    }

    [Test]
    public void SegundoQuadro_NomeiaOA24ComoODefeitoOposto()
    {
        Assert.That(EtapaFios.Em(1).Instrucao, Does.Contain(CodigoA24));
        Assert.That(EtapaFios.Em(1).Instrucao, Does.Contain("oposto"));
    }

    [Test]
    public void ForaDoQuadroDeFronteira_NenhumPassoMandaVerificarContinuidade()
    {
        var foraDaFronteira = EtapaFios.Quadros
            .Where((quadro, indice) => indice != 1)
            .Select(quadro => quadro.Instrucao)
            .ToList();

        Assert.That(foraDaFronteira, Has.None.Contains("continuidade"),
            "continuidade é a verificação do A24, o circuito aberto.");
    }

    [Test]
    public void AVerificacaoDoA23_ProcuraSinaisDeCurto()
    {
        Assert.That(EtapaFios.Em(5).Instrucao, Does.Contain("isolamento derretido"));
        Assert.That(EtapaFios.Em(5).Instrucao, Does.Contain("queima"));
        Assert.That(EtapaFios.Em(6).Instrucao, Does.Contain("fiapo de cobre"));
        Assert.That(EtapaFios.Ultimo.Instrucao, Does.Contain("sem caminho fechado"));
    }

    #endregion

    #region MARK: Seguranca da instalacao eletrica, paginas 18 e 19

    [Test]
    public void EtapaFios_AvisaSobreEnergiaAntesDeMandarTocarNaFiacao()
    {
        Assert.That(EtapaFios.Quantidade, Is.EqualTo(10));
        Assert.That(EtapaFios.Em(2).Instrucao, Does.StartWith("PERIGO"));
        Assert.That(EtapaFios.Em(2).Instrucao, Does.Contain("religamento"));
        Assert.That(EtapaFios.Em(3).Instrucao, Does.Contain("energia residual"));
        Assert.That(EtapaFios.Em(4).Instrucao, Does.Contain("ar comprimido"));
    }

    #endregion

    #region MARK: Nota normativa dos alertas 19 a 25, pagina 11

    [Test]
    public void UmQuadro_ExplicaQueDesligarOAlertaNaoApagaAIndicacaoFisica()
    {
        Assert.That(EtapaFios.Em(8).Instrucao, Does.Contain("notificação no aplicativo"));
        Assert.That(EtapaFios.Em(8).Instrucao, Does.Contain("indicação física"));
    }

    #endregion

    #region MARK: Destaque piscante no lugar da verificacao fisica

    [Test]
    public void OperadorEmCampo_VeOndeOlharEmCadaVerificacao()
    {
        Assert.That(EtapaFios.Em(5).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaquePneumatica));
        Assert.That(EtapaFios.Em(6).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaqueModuloEletronico));
        Assert.That(EtapaFios.Em(7).Vfx, Is.EqualTo(PerfisDeDisplayDeAlerta.DestaquePneumatica));
    }

    [Test]
    public void QuadrosSemVerificacaoFisica_NaoAcendemDestaqueNoModelo()
    {
        foreach (int indice in new[] { 0, 1, 2, 3, 4, 8, 9 })
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
        Assert.That(ultimo.Instrucao, Does.Contain("A23 é eliminado"));
    }

    [Test]
    public void EnquantoOAlertaEstaAtivo_OLedVermelhoPisca()
    {
        var ativos = EtapaFios.Quadros.Where(quadro => quadro != EtapaFios.Ultimo).ToList();

        Assert.That(ativos.All(quadro => quadro.Leds == EstadoLedsM4.Alerta), Is.True);
        Assert.That(ativos.All(quadro => quadro.LedPiscando), Is.True);
    }

    [Test]
    public void AnimacoesDeA23_FicamVaziasPorqueNenhumPassoUsaOsBotoes()
    {
        Assert.That(
            EtapaFios.Quadros.Select(quadro => quadro.Animacao),
            Is.EqualTo(new string[] { null, null, null, null, null, null, null, null, null, null }));

        Assert.That(perfil.Layer, Is.Null, "o A23 roda na Base Layer.");
    }

    [Test]
    public void EtapasDeA23_ExpandemAAcaoOficialEmDezPassosGuiados()
    {
        Etapa[] etapas = EtapasComDisplayDeAlerta.Aplicar(a23, EtapasGuiadasDeAlerta.Criar(a23));

        Assert.That(etapas, Has.Length.EqualTo(10));
        Assert.That(etapas[0].tutorial, Does.StartWith("Confirme o alerta A23"));
        Assert.That(etapas[5].vfx, Is.EqualTo("DestaquePneumatica"));
        Assert.That(etapas[6].vfx, Is.EqualTo("DestaqueModuloEletronico"));
        Assert.That(etapas.All(TelaM4.EtapaTemConteudo), Is.True);
    }

    #endregion
}
