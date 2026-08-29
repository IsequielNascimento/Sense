using System.Collections.Generic;
using UnityEngine;

public static class EtapasComDisplayDeAlerta
{
    #region MARK: Composicao do display sobre as etapas oficiais

    public static Etapa[] Aplicar(
        AlertaOficial alerta,
        Etapa[] etapas,
        string idioma = InstrucoesDeDisplayLocalizadas.IdiomaCanonico)
    {
        if (alerta == null || etapas == null || etapas.Length == 0) return etapas;

        PerfilDeDisplayDeAlerta perfil = PerfisDeDisplayDeAlerta.Obter(alerta.Codigo);

        if (perfil == null) return etapas;

        if (!perfil.CorrespondeAoCatalogo(alerta))
        {
            Debug.LogError(
                $"[EtapasComDisplayDeAlerta] Perfil de '{perfil.Codigo}' tem {perfil.QuantidadeDeEtapasOficiais} " +
                $"etapas oficiais e o catalogo tem {alerta.Acoes.Count} acoes.");
            return etapas;
        }

        if (etapas.Length != perfil.QuantidadeDeEtapasOficiais) return etapas;

        var passosGuiados = new List<Etapa>();

        for (int i = 0; i < etapas.Length; i++)
        {
            Etapa etapaOficial = etapas[i];
            SequenciaDeQuadrosM4 sequencia = perfil.EtapaOficial(i);

            foreach (QuadroDeDisplayM4 quadro in sequencia.Quadros)
            {
                Etapa passo = Copiar(etapaOficial);
                quadro.Aplicar(passo);
                passo.tutorial = InstrucoesDeDisplayLocalizadas.Traduzir(passo.tutorial, idioma);
                passosGuiados.Add(passo);
            }
        }

        return passosGuiados.ToArray();
    }

    private static Etapa Copiar(Etapa origem)
    {
        return new Etapa
        {
            tutorial = origem?.tutorial ?? string.Empty,
            animacao = origem?.animacao ?? string.Empty,
            telaDisplay = origem?.telaDisplay,
            vfx = origem?.vfx,
        };
    }

    #endregion
}
