using UnityEngine;

public static class EtapasComDisplayDeAlerta
{
    #region MARK: Composicao do display sobre as etapas oficiais

    public static Etapa[] Aplicar(AlertaOficial alerta, Etapa[] etapas)
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

        for (int i = 0; i < etapas.Length; i++)
        {
            perfil.EtapaOficial(i).Primeiro.Aplicar(etapas[i]);
        }

        return etapas;
    }

    #endregion
}
