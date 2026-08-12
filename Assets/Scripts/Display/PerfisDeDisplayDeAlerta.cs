using System;
using System.Collections.Generic;

public static class PerfisDeDisplayDeAlerta
{
    #region MARK: Textos comprovados pelo manual

    public const string CodigoA8 = "A8";
    public const string NomeA8 = "MODO SEGURO";
    public const string MenuModoSeguro = "C16";

    #endregion

    #region MARK: Registro central

    private static readonly Dictionary<string, PerfilDeDisplayDeAlerta> PorCodigo = Registrar();

    private static Dictionary<string, PerfilDeDisplayDeAlerta> Registrar()
    {
        var registro = new Dictionary<string, PerfilDeDisplayDeAlerta>(StringComparer.Ordinal);

        Adicionar(registro, CriarPerfilDeModoSeguro());

        return registro;
    }

    private static void Adicionar(
        Dictionary<string, PerfilDeDisplayDeAlerta> registro,
        PerfilDeDisplayDeAlerta perfil)
    {
        registro[perfil.Codigo] = perfil;
    }

    public static PerfilDeDisplayDeAlerta Obter(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return null;

        return PorCodigo.TryGetValue(codigo.Trim(), out PerfilDeDisplayDeAlerta perfil) ? perfil : null;
    }

    public static bool Existe(string codigo)
    {
        return Obter(codigo) != null;
    }

    public static IReadOnlyCollection<string> CodigosComPerfil => PorCodigo.Keys;

    #endregion

    #region MARK: A8 MODO SEGURO, paginas 11, 53 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeModoSeguro()
    {
        var acaoUnica = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(CodigoA8, EstadoLedsM4.Alerta, ledPiscando: true),
            new QuadroDeDisplayM4(NomeA8, EstadoLedsM4.Alerta, ledPiscando: true),
            new QuadroDeDisplayM4(MenuModoSeguro, EstadoLedsM4.Alerta, ledPiscando: true),
            new QuadroDeDisplayM4(NomeA8, EstadoLedsM4.Desligado));

        return new PerfilDeDisplayDeAlerta(
            CodigoA8,
            new[] { acaoUnica },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion
}
