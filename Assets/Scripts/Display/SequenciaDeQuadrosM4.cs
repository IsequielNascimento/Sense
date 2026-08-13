using System;
using System.Collections.Generic;

public sealed class SequenciaDeQuadrosM4
{
    #region MARK: Construcao

    public SequenciaDeQuadrosM4(params QuadroDeDisplayM4[] quadros)
    {
        if (quadros == null || quadros.Length == 0)
        {
            throw new ArgumentException("Uma etapa oficial precisa de ao menos um quadro.", nameof(quadros));
        }

        foreach (QuadroDeDisplayM4 quadro in quadros)
        {
            if (quadro == null) throw new ArgumentException("Quadro nulo na sequencia.", nameof(quadros));
        }

        Quadros = Array.AsReadOnly((QuadroDeDisplayM4[])quadros.Clone());
    }

    #endregion

    #region MARK: Quadros

    public IReadOnlyList<QuadroDeDisplayM4> Quadros { get; }

    public int Quantidade => Quadros.Count;

    public QuadroDeDisplayM4 Primeiro => Quadros[0];

    public QuadroDeDisplayM4 Ultimo => Quadros[Quadros.Count - 1];

    public QuadroDeDisplayM4 Em(int indice)
    {
        if (indice < 0 || indice >= Quadros.Count) return null;

        return Quadros[indice];
    }

    #endregion
}
