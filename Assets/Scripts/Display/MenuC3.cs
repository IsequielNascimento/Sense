using System;
using System.Collections.Generic;

public sealed class MenuC3
{
    #region MARK - Rótulos padrão (ASCII maiúsculo, compatível com a fonte do LCD)

    public static readonly IReadOnlyDictionary<ModoC3, string> RotulosPadrao = new Dictionary<ModoC3, string>
    {
        { ModoC3.Position, "POSICAO" },
        { ModoC3.Angle, "ANGULO" },
        { ModoC3.Percent, "PERCENT" },
        { ModoC3.Temperature, "TEMP" },
        { ModoC3.PartialCount, "PARCIAL" },
        { ModoC3.TotalCount, "TOTAL" },
        { ModoC3.LinePressure, "PRESSAO" },
        { ModoC3.WorkedDays, "DIAS" },
        { ModoC3.NetworkAddress, "ENDERECO" },
    };

    #endregion

    #region MARK - Estado

    private readonly IReadOnlyList<ModoC3> modosDisponiveis;
    private readonly IReadOnlyDictionary<ModoC3, string> rotulos;

    private int indiceDestaque;
    private ModoC3 modoConfirmado;
    private bool ativo;
    private string textoValorRun = string.Empty;

    #endregion

    #region MARK - Construção

    public MenuC3(bool deviceNetHabilitado, IReadOnlyDictionary<ModoC3, string> rotulosPersonalizados = null)
    {
        modosDisponiveis = ConstruirModosDisponiveis(deviceNetHabilitado);
        rotulos = rotulosPersonalizados ?? RotulosPadrao;
        modoConfirmado = modosDisponiveis[0];
        indiceDestaque = 0;
    }

    private static IReadOnlyList<ModoC3> ConstruirModosDisponiveis(bool deviceNetHabilitado)
    {
        var modos = new List<ModoC3>
        {
            ModoC3.Position,
            ModoC3.Angle,
            ModoC3.Percent,
            ModoC3.Temperature,
            ModoC3.PartialCount,
            ModoC3.TotalCount,
            ModoC3.LinePressure,
            ModoC3.WorkedDays,
        };

        if (deviceNetHabilitado) modos.Add(ModoC3.NetworkAddress);

        return modos;
    }

    #endregion

    #region MARK - Consulta de estado

    public IReadOnlyList<ModoC3> ModosDisponiveis => modosDisponiveis;
    public ModoC3 ModoDestacado => modosDisponiveis[indiceDestaque];
    public ModoC3 ModoConfirmado => modoConfirmado;
    public bool MenuAtivo => ativo;

    public string TextoDisplayAtual => ativo ? TextoMenu() : textoValorRun;

    private string TextoMenu()
    {
        if (rotulos.TryGetValue(ModoDestacado, out string valor)) return "C3\n" + valor;
        if (RotulosPadrao.TryGetValue(ModoDestacado, out string padrao)) return "C3\n" + padrao;
        return "C3\n" + ModoDestacado;
    }

    #endregion

    #region MARK - Navegação (B1/B2/B3)

    public void Entrar()
    {
        if (ativo) return;
        ativo = true;
        indiceDestaque = IndiceDoModo(modoConfirmado);
    }

    public void ProximoB3()
    {
        if (!ativo) return;
        indiceDestaque = (indiceDestaque + 1) % modosDisponiveis.Count;
    }

    public void AnteriorB1()
    {
        if (!ativo) return;
        indiceDestaque = (indiceDestaque - 1 + modosDisponiveis.Count) % modosDisponiveis.Count;
    }

    public void ConfirmarB2()
    {
        if (!ativo) return;
        if (ModoDestacado != modoConfirmado) textoValorRun = string.Empty;
        modoConfirmado = ModoDestacado;
        ativo = false;
    }

    public void CancelarSair()
    {
        if (!ativo) return;
        ativo = false;
    }

    private int IndiceDoModo(ModoC3 modo)
    {
        for (int i = 0; i < modosDisponiveis.Count; i++)
        {
            if (modosDisponiveis[i] == modo) return i;
        }
        return 0;
    }

    #endregion

    #region MARK - Valor RUN

    public void AtualizarValorRun<T>(T valor, Func<T, string> formatador)
    {
        textoValorRun = formatador(valor);
    }

    #endregion
}
