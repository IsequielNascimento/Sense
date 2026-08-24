using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class AnimacoesDeAlertaConcluidasTests
{
    #region MARK: Acesso ao registro em Assembly-CSharp

    private static readonly Type TipoAnimacoesConcluidas =
        Type.GetType("AnimacoesDeAlertaConcluidas, Assembly-CSharp");

    private static bool EstaConcluida(string codigo)
    {
        return (bool)TipoAnimacoesConcluidas
            .GetMethod("EstaConcluida")
            .Invoke(null, new object[] { codigo });
    }

    private static IEnumerable<string> Registradas()
    {
        return ((IEnumerable)TipoAnimacoesConcluidas.GetProperty("Codigos").GetValue(null))
            .Cast<string>();
    }

    #endregion

    #region MARK: O card so fica cinza enquanto a animacao nao existe

    [Test]
    public void TodoAlertaComPerfilDeDisplay_ContaComoAnimacaoConcluida()
    {
        Assert.That(TipoAnimacoesConcluidas, Is.Not.Null);

        foreach (string codigo in PerfisDeDisplayDeAlerta.CodigosComPerfil)
        {
            Assert.That(
                EstaConcluida(codigo),
                Is.True,
                $"{codigo} tem perfil de display, então o card não pode ficar cinza.");
        }
    }

    [Test]
    public void RegistroDeConcluidas_AcompanhaExatamenteOsPerfisDeDisplay()
    {
        Assert.That(Registradas(), Is.EquivalentTo(PerfisDeDisplayDeAlerta.CodigosComPerfil));
    }

    [Test]
    public void AlertaSemPerfil_ContinuaPendente()
    {
        Assert.That(EstaConcluida("A24"), Is.False);
        Assert.That(EstaConcluida(null), Is.False);
        Assert.That(EstaConcluida("   "), Is.False);
    }

    #endregion
}
