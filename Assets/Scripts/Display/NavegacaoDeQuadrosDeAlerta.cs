using System;

public sealed class NavegacaoDeQuadrosDeAlerta
{
    #region MARK: Construcao

    public NavegacaoDeQuadrosDeAlerta(PerfilDeDisplayDeAlerta perfil)
    {
        Perfil = perfil ?? throw new ArgumentNullException(nameof(perfil));
        Reiniciar();
    }

    #endregion

    #region MARK: Estado

    public PerfilDeDisplayDeAlerta Perfil { get; }

    public int IndiceDaEtapaOficial { get; private set; }
    public int IndiceDoQuadro { get; private set; }

    public SequenciaDeQuadrosM4 EtapaAtual => Perfil.EtapaOficial(IndiceDaEtapaOficial);

    public QuadroDeDisplayM4 QuadroAtual => EtapaAtual.Em(IndiceDoQuadro);

    public bool EstaNoPrimeiroQuadro => IndiceDoQuadro == 0;

    public bool EstaNoUltimoQuadro => IndiceDoQuadro == EtapaAtual.Quantidade - 1;

    #endregion

    #region MARK: Navegacao determinista

    public void Reiniciar()
    {
        IndiceDaEtapaOficial = 0;
        IndiceDoQuadro = 0;
    }

    public bool ProximoQuadro()
    {
        if (EstaNoUltimoQuadro) return false;

        IndiceDoQuadro++;
        return true;
    }

    public bool Avancar()
    {
        if (IndiceDaEtapaOficial >= Perfil.QuantidadeDeEtapasOficiais - 1) return false;

        IndiceDaEtapaOficial++;
        IndiceDoQuadro = 0;
        return true;
    }

    public bool Voltar()
    {
        if (IndiceDaEtapaOficial <= 0) return false;

        IndiceDaEtapaOficial--;
        IndiceDoQuadro = 0;
        return true;
    }

    public void Repetir()
    {
        IndiceDoQuadro = 0;
    }

    #endregion

    #region MARK: Aplicacao no display

    public void AplicarEm(Etapa etapa)
    {
        QuadroAtual.Aplicar(etapa);
    }

    #endregion
}
