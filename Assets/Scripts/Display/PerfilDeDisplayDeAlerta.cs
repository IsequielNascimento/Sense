using System;
using System.Collections.Generic;

public sealed class PerfilDeDisplayDeAlerta
{
    #region MARK: Construcao

    public PerfilDeDisplayDeAlerta(
        string codigo,
        IReadOnlyList<SequenciaDeQuadrosM4> etapasOficiais,
        bool mecanismoDeAtivacaoConfirmado = true,
        string layer = null)
    {
        if (!CodigosOficiais.EhAlerta(codigo))
        {
            throw new ArgumentException($"'{codigo}' nao e um codigo de alerta oficial.", nameof(codigo));
        }

        if (etapasOficiais == null || etapasOficiais.Count == 0)
        {
            throw new ArgumentException("Um perfil precisa de ao menos uma etapa oficial.", nameof(etapasOficiais));
        }

        foreach (SequenciaDeQuadrosM4 etapa in etapasOficiais)
        {
            if (etapa == null) throw new ArgumentException("Etapa oficial nula no perfil.", nameof(etapasOficiais));
        }

        Codigo = codigo.Trim();
        EtapasOficiais = etapasOficiais;
        MecanismoDeAtivacaoConfirmado = mecanismoDeAtivacaoConfirmado;
        Layer = string.IsNullOrWhiteSpace(layer) ? null : layer;
    }

    #endregion

    #region MARK: Identidade e conteudo

    public string Codigo { get; }
    public IReadOnlyList<SequenciaDeQuadrosM4> EtapasOficiais { get; }

    public int QuantidadeDeEtapasOficiais => EtapasOficiais.Count;

    public bool MecanismoDeAtivacaoConfirmado { get; }
    public string Layer { get; }

    public SequenciaDeQuadrosM4 EtapaOficial(int indice)
    {
        if (indice < 0 || indice >= EtapasOficiais.Count) return null;

        return EtapasOficiais[indice];
    }

    #endregion

    #region MARK: Conformidade com o catalogo

    public bool CorrespondeAoCatalogo(AlertaOficial alerta)
    {
        return alerta != null
            && string.Equals(alerta.Codigo, Codigo, StringComparison.Ordinal)
            && alerta.Acoes.Count == QuantidadeDeEtapasOficiais;
    }

    #endregion
}
