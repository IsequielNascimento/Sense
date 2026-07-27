using NUnit.Framework;

public class IdentidadeDoCenarioTests
{
    #region MARK - Precedência

    [Test]
    public void Resolver_PrefereOPrimeiroCandidatoPreenchido()
    {
        Assert.That(IdentidadeDoCenario.Resolver("bluetooth_pairing", "A3"), Is.EqualTo("bluetooth_pairing"));
    }

    [Test]
    public void Resolver_CaiParaOLegadoQuandoOPreferidoEstaVazio()
    {
        Assert.That(IdentidadeDoCenario.Resolver(null, "A3"), Is.EqualTo("A3"));
        Assert.That(IdentidadeDoCenario.Resolver("", "A3"), Is.EqualTo("A3"));
        Assert.That(IdentidadeDoCenario.Resolver("   ", "A3"), Is.EqualTo("A3"));
    }

    [Test]
    public void Resolver_PercorreVariosNiveisDeFallback()
    {
        Assert.That(IdentidadeDoCenario.Resolver(null, null, "A8"), Is.EqualTo("A8"));
    }

    [Test]
    public void Resolver_DevolveVazioQuandoNaoHaCandidato()
    {
        Assert.That(IdentidadeDoCenario.Resolver(), Is.EqualTo(string.Empty));
        Assert.That(IdentidadeDoCenario.Resolver(null), Is.EqualTo(string.Empty));
        Assert.That(IdentidadeDoCenario.Resolver(null, ""), Is.EqualTo(string.Empty));
    }

    [Test]
    public void Resolver_RemoveEspacosAoRedor()
    {
        Assert.That(IdentidadeDoCenario.Resolver("  A5  "), Is.EqualTo("A5"));
    }

    #endregion

    #region MARK - Independência das identidades

    [Test]
    public void TrocarAImagem_NaoMudaOCenarioNemORecurso()
    {
        var problema = new Problema
        {
            scenarioId = "low_pressure_inspection",
            resourceKey = "A6",
            imageKey = "ilustracao_antiga"
        };

        problema.imageKey = "ilustracao_nova";

        Assert.That(problema.ScenarioId, Is.EqualTo("low_pressure_inspection"));
        Assert.That(problema.ResourceKey, Is.EqualTo("A6"));
        Assert.That(problema.ImageKey, Is.EqualTo("ilustracao_nova"));
    }

    [Test]
    public void TrocarOCenario_NaoMudaOAssetVisual()
    {
        var problema = new Problema
        {
            scenarioId = "low_supply_voltage",
            resourceKey = "A7",
            imageKey = "A7"
        };

        problema.scenarioId = "alimentacao_baixa";

        Assert.That(problema.ImageKey, Is.EqualTo("A7"));
        Assert.That(problema.ResourceKey, Is.EqualTo("A7"));
    }

    #endregion

    #region MARK - Compatibilidade com o formato legado

    [Test]
    public void FormatoLegado_ContinuaResolvendoPelasTresIdentidades()
    {
        var problema = new Problema { imagem = "A1" };

        Assert.That(problema.ScenarioId, Is.EqualTo("A1"));
        Assert.That(problema.ImageKey, Is.EqualTo("A1"));
        Assert.That(problema.ResourceKey, Is.EqualTo("A1"));
    }

    [Test]
    public void FormatoMisto_DeixaOCampoNovoVencer()
    {
        var problema = new Problema { imagem = "A4", resourceKey = "folga_mecanica" };

        Assert.That(problema.ResourceKey, Is.EqualTo("folga_mecanica"));
        Assert.That(problema.ImageKey, Is.EqualTo("A4"));
    }

    #endregion
}
