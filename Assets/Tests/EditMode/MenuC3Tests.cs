using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

public class MenuC3Tests
{
    #region MARK - Ordenação e DeviceNet

    [Test]
    public void ModosDisponiveis_SemDeviceNet_SeguemAOrdemDocumentadaSemEnderecoDeRede()
    {
        var menu = new MenuC3(deviceNetHabilitado: false);

        Assert.That(menu.ModosDisponiveis, Is.EqualTo(new[]
        {
            ModoC3.Position,
            ModoC3.Angle,
            ModoC3.Percent,
            ModoC3.Temperature,
            ModoC3.PartialCount,
            ModoC3.TotalCount,
            ModoC3.LinePressure,
            ModoC3.WorkedDays,
        }));
    }

    [Test]
    public void ModosDisponiveis_ComDeviceNet_AdicionaEnderecoDeRedeAoFinal()
    {
        var menu = new MenuC3(deviceNetHabilitado: true);

        Assert.That(menu.ModosDisponiveis[^1], Is.EqualTo(ModoC3.NetworkAddress));
        Assert.That(menu.ModosDisponiveis.Count, Is.EqualTo(9));
    }

    #endregion

    #region MARK - Navegação com wrap-around

    [Test]
    public void ProximoB3_NoUltimoModo_VoltaParaOPrimeiro()
    {
        var menu = new MenuC3(deviceNetHabilitado: false);
        menu.Entrar();

        for (int i = 0; i < menu.ModosDisponiveis.Count; i++) menu.ProximoB3();

        Assert.That(menu.ModoDestacado, Is.EqualTo(ModoC3.Position));
    }

    [Test]
    public void AnteriorB1_NoPrimeiroModo_VaiParaOUltimo()
    {
        var menu = new MenuC3(deviceNetHabilitado: false);
        menu.Entrar();

        menu.AnteriorB1();

        Assert.That(menu.ModoDestacado, Is.EqualTo(ModoC3.WorkedDays));
    }

    [Test]
    public void ProximoB3_AvancaSequencialmente()
    {
        var menu = new MenuC3(deviceNetHabilitado: false);
        menu.Entrar();

        menu.ProximoB3();

        Assert.That(menu.ModoDestacado, Is.EqualTo(ModoC3.Angle));
    }

    #endregion

    #region MARK - Confirmar e cancelar

    [Test]
    public void ConfirmarB2_AplicaSelecaoESaiDoMenu()
    {
        var menu = new MenuC3(deviceNetHabilitado: false);
        menu.Entrar();
        menu.ProximoB3();
        menu.ProximoB3();

        menu.ConfirmarB2();

        Assert.That(menu.ModoConfirmado, Is.EqualTo(ModoC3.Percent));
        Assert.That(menu.MenuAtivo, Is.False);
    }

    [Test]
    public void CancelarSair_SaiSemAlterarOModoConfirmado()
    {
        var menu = new MenuC3(deviceNetHabilitado: false);
        menu.Entrar();
        menu.ProximoB3();
        menu.ProximoB3();

        menu.CancelarSair();

        Assert.That(menu.ModoConfirmado, Is.EqualTo(ModoC3.Position));
        Assert.That(menu.MenuAtivo, Is.False);
    }

    [Test]
    public void Entrar_ComecaDestacandoOModoConfirmadoAtual()
    {
        var menu = new MenuC3(deviceNetHabilitado: false);
        menu.Entrar();
        menu.ProximoB3();
        menu.ConfirmarB2();

        menu.Entrar();

        Assert.That(menu.ModoDestacado, Is.EqualTo(ModoC3.Angle));
    }

    #endregion

    #region MARK - Robustez fora do menu

    [Test]
    public void Navegacao_ForaDoMenu_NaoAlteraDestaqueOuConfirmacao()
    {
        var menu = new MenuC3(deviceNetHabilitado: false);

        menu.ProximoB3();
        menu.AnteriorB1();
        menu.ConfirmarB2();
        menu.CancelarSair();

        Assert.That(menu.MenuAtivo, Is.False);
        Assert.That(menu.ModoDestacado, Is.EqualTo(ModoC3.Position));
        Assert.That(menu.ModoConfirmado, Is.EqualTo(ModoC3.Position));
    }

    [Test]
    public void Entrar_QuandoJaAtivo_NaoReiniciaODestaqueParaOModoConfirmado()
    {
        var menu = new MenuC3(deviceNetHabilitado: false);
        menu.Entrar();
        menu.ProximoB3();
        menu.ProximoB3();

        menu.Entrar();

        Assert.That(menu.ModoDestacado, Is.EqualTo(ModoC3.Percent));
    }

    #endregion

    #region MARK - Texto de exibição

    [Test]
    public void TextoDisplayAtual_NoMenu_MostraC3EORotuloDoModoDestacado()
    {
        var menu = new MenuC3(deviceNetHabilitado: false);
        menu.Entrar();
        menu.ProximoB3();

        Assert.That(menu.TextoDisplayAtual, Is.EqualTo("C3\nANGULO"));
    }

    [Test]
    public void TextoDisplayAtual_ComRotulosInjetados_UsaOsRotulosPersonalizados()
    {
        var rotulos = new Dictionary<ModoC3, string> { { ModoC3.Position, "POS" } };
        var menu = new MenuC3(deviceNetHabilitado: false, rotulosPersonalizados: rotulos);
        menu.Entrar();

        Assert.That(menu.TextoDisplayAtual, Is.EqualTo("C3\nPOS"));
    }

    [Test]
    public void TextoDisplayAtual_ForaDoMenu_MostraOValorRunAtualizado()
    {
        var menu = new MenuC3(deviceNetHabilitado: false);

        menu.AtualizarValorRun(42.5f, v => v.ToString("F1", CultureInfo.InvariantCulture));

        Assert.That(menu.TextoDisplayAtual, Is.EqualTo("42.5"));
    }

    [Test]
    public void TextoDisplayAtual_AoEntrarNoMenu_IgnoraOValorRunAteConfirmarOuCancelar()
    {
        var menu = new MenuC3(deviceNetHabilitado: false);
        menu.AtualizarValorRun("99", v => v);

        menu.Entrar();

        Assert.That(menu.TextoDisplayAtual, Is.EqualTo("C3\nPOSICAO"));

        menu.CancelarSair();

        Assert.That(menu.TextoDisplayAtual, Is.EqualTo("99"));
    }

    [Test]
    public void TextoDisplayAtual_ComRotulosPersonalizadosIncompletos_UsaRotuloPadraoParaModoFaltante()
    {
        var rotulos = new Dictionary<ModoC3, string> { { ModoC3.Position, "POS" } };
        var menu = new MenuC3(deviceNetHabilitado: false, rotulosPersonalizados: rotulos);
        menu.Entrar();
        menu.ProximoB3();

        Assert.That(menu.TextoDisplayAtual, Is.EqualTo("C3\n" + MenuC3.RotulosPadrao[ModoC3.Angle]));
    }

    [Test]
    public void TextoDisplayAtual_AoConfirmarModoDiferente_LimpaValorRunAntigo()
    {
        var menu = new MenuC3(deviceNetHabilitado: false);
        menu.AtualizarValorRun(42.5f, v => v.ToString("F1", CultureInfo.InvariantCulture));

        menu.Entrar();
        menu.ProximoB3();
        menu.ConfirmarB2();

        Assert.That(menu.TextoDisplayAtual, Is.EqualTo(string.Empty));
    }

    [Test]
    public void TextoDisplayAtual_AoReconfirmarOMesmoModo_MantemOValorRunAtual()
    {
        var menu = new MenuC3(deviceNetHabilitado: false);
        menu.AtualizarValorRun(42.5f, v => v.ToString("F1", CultureInfo.InvariantCulture));

        menu.Entrar();
        menu.ConfirmarB2();

        Assert.That(menu.TextoDisplayAtual, Is.EqualTo("42.5"));
    }

    #endregion
}
