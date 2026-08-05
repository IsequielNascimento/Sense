using NUnit.Framework;

public class MesclaDeCenarioTests
{
    #region MARK - Precedência da estrutura

    [Test]
    public void EstruturaVence_NosCamposEstruturais()
    {
        var estrutura = new EstruturaCenario
        {
            layer = "Problema 6",
            etapas = new[]
            {
                new EtapaEstrutural
                {
                    animacao = "A6_p1",
                    telaDisplay = "alerta_pressao",
                    vfx = "vazamento_ar",
                    textoDisplay = "A18\nPRESS"
                }
            }
        };

        var traducao = new[]
        {
            new Etapa
            {
                tutorial = "Verifique a linha de ar.",
                animacao = "valor_antigo",
                telaDisplay = "valor_antigo",
                vfx = "valor_antigo",
                textoDisplay = "valor_antigo"
            }
        };

        MesclaDeCenario.Resultado resultado = MesclaDeCenario.Mesclar(estrutura, traducao, "Problema Antiga");

        Assert.That(resultado.UsouEstrutura, Is.True);
        Assert.That(resultado.Layer, Is.EqualTo("Problema 6"));
        Assert.That(resultado.Etapas[0].animacao, Is.EqualTo("A6_p1"));
        Assert.That(resultado.Etapas[0].telaDisplay, Is.EqualTo("alerta_pressao"));
        Assert.That(resultado.Etapas[0].vfx, Is.EqualTo("vazamento_ar"));
        Assert.That(resultado.Etapas[0].textoDisplay, Is.EqualTo("A18\nPRESS"));
    }

    [Test]
    public void TraducaoVence_NoTutorial()
    {
        var estrutura = new EstruturaCenario
        {
            etapas = new[] { new EtapaEstrutural { animacao = "A1_p1" } }
        };

        var traducao = new[] { new Etapa { tutorial = "Aguarde o processamento." } };

        MesclaDeCenario.Resultado resultado = MesclaDeCenario.Mesclar(estrutura, traducao, null);

        Assert.That(resultado.Etapas[0].tutorial, Is.EqualTo("Aguarde o processamento."));
    }

    [Test]
    public void CampoEstruturalVazio_CaiParaATraducao()
    {
        var estrutura = new EstruturaCenario
        {
            etapas = new[] { new EtapaEstrutural { animacao = "A1_p1", vfx = null } }
        };

        var traducao = new[] { new Etapa { vfx = "setas_rotacao" } };

        MesclaDeCenario.Resultado resultado = MesclaDeCenario.Mesclar(estrutura, traducao, null);

        Assert.That(resultado.Etapas[0].vfx, Is.EqualTo("setas_rotacao"));
    }

    #endregion

    #region MARK - Compatibilidade e robustez

    [Test]
    public void SemEstrutura_UsaSomenteATraducao()
    {
        var traducao = new[]
        {
            new Etapa { tutorial = "Passo", animacao = "A2_p1", vfx = "nenhum" }
        };

        MesclaDeCenario.Resultado resultado = MesclaDeCenario.Mesclar(null, traducao, "Problema 2");

        Assert.That(resultado.UsouEstrutura, Is.False);
        Assert.That(resultado.Layer, Is.EqualTo("Problema 2"));
        Assert.That(resultado.Etapas[0].animacao, Is.EqualTo("A2_p1"));
        Assert.That(resultado.TemDivergencia, Is.False);
    }

    [Test]
    public void EstruturaVazia_NaoSubstituiATraducao()
    {
        var estrutura = new EstruturaCenario { etapas = new EtapaEstrutural[0] };
        var traducao = new[] { new Etapa { animacao = "A3_p1" } };

        MesclaDeCenario.Resultado resultado = MesclaDeCenario.Mesclar(estrutura, traducao, null);

        Assert.That(resultado.UsouEstrutura, Is.False);
        Assert.That(resultado.Etapas[0].animacao, Is.EqualTo("A3_p1"));
    }

    [Test]
    public void TopologiaDivergente_ERelatadaSemPerderEtapasDaEstrutura()
    {
        var estrutura = new EstruturaCenario
        {
            etapas = new[]
            {
                new EtapaEstrutural { animacao = "A4_p1" },
                new EtapaEstrutural { animacao = "A4_p2" }
            }
        };

        var traducao = new[] { new Etapa { tutorial = "Só um passo traduzido" } };

        MesclaDeCenario.Resultado resultado = MesclaDeCenario.Mesclar(estrutura, traducao, null);

        Assert.That(resultado.TemDivergencia, Is.True);
        Assert.That(resultado.Etapas.Length, Is.EqualTo(2));
        Assert.That(resultado.Etapas[1].animacao, Is.EqualTo("A4_p2"));
        Assert.That(resultado.Etapas[1].tutorial, Is.EqualTo(string.Empty));
    }

    [Test]
    public void EntradasNulas_NaoQuebram()
    {
        MesclaDeCenario.Resultado resultado = MesclaDeCenario.Mesclar(null, null, null);

        Assert.That(resultado.Etapas, Is.Empty);
        Assert.That(resultado.UsouEstrutura, Is.False);
    }

    #endregion
}
