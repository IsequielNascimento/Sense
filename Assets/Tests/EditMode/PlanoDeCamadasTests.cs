using NUnit.Framework;

public class PlanoDeCamadasTests
{
    #region MARK - Regressão do peso residual

    [Test]
    public void EstadoInexistente_ZeraTodasAsCamadasDeProblema()
    {
        int camadaAnterior = 3;
        int camadaComEstado = PlanoDeCamadas.CamadaComEstado(camadaAnterior, false);

        for (int i = PlanoDeCamadas.PrimeiraCamadaDeProblema; i < 9; i++)
        {
            Assert.That(
                PlanoDeCamadas.PesoDaCamadaDeProblema(i, camadaComEstado),
                Is.EqualTo(0f),
                $"A camada {i} nao pode manter peso quando o novo estado nao existe.");
        }
    }

    [Test]
    public void CamadaNaoEncontrada_ZeraTodasAsCamadasDeProblema()
    {
        int camadaComEstado = PlanoDeCamadas.CamadaComEstado(PlanoDeCamadas.CamadaInexistente, false);

        for (int i = PlanoDeCamadas.PrimeiraCamadaDeProblema; i < 9; i++)
        {
            Assert.That(PlanoDeCamadas.PesoDaCamadaDeProblema(i, camadaComEstado), Is.EqualTo(0f));
        }
    }

    [Test]
    public void TrocaDeProblema_AtivaSomenteACamadaDoNovoEstado()
    {
        int camadaComEstado = PlanoDeCamadas.CamadaComEstado(5, true);

        for (int i = PlanoDeCamadas.PrimeiraCamadaDeProblema; i < 9; i++)
        {
            float esperado = i == 5 ? 1f : 0f;
            Assert.That(PlanoDeCamadas.PesoDaCamadaDeProblema(i, camadaComEstado), Is.EqualTo(esperado));
        }
    }

    #endregion

    #region MARK - Limites

    [Test]
    public void BaseLayer_NuncaEhTratadaComoCamadaDeProblema()
    {
        Assert.That(PlanoDeCamadas.EhCamadaDeProblema(0), Is.False);
        Assert.That(PlanoDeCamadas.PesoDaCamadaDeProblema(0, 0), Is.EqualTo(0f));
    }

    [Test]
    public void CamadaComEstado_PreservaIndiceQuandoEstadoExiste()
    {
        Assert.That(PlanoDeCamadas.CamadaComEstado(4, true), Is.EqualTo(4));
        Assert.That(PlanoDeCamadas.CamadaComEstado(4, false), Is.EqualTo(PlanoDeCamadas.CamadaInexistente));
    }

    #endregion
}
