using UnityEngine;
using System.Collections.Generic;

public class CarregarBancoDeDados : MonoBehaviour
{
    public static List<Problema> Problemas { get; set; }

    public static void Carregar()
    {
        string idioma = "pt";
        if (IdiomaManager.Instance != null)
        {
            idioma = IdiomaManager.Instance.ObterIdioma();
        }
        else
        {
            idioma = PlayerPrefs.GetString("idioma", "pt");
        }

        TextAsset arquivo = Resources.Load<TextAsset>($"banco_de_dados_{idioma}");

        if (arquivo != null)
        {
            Wrapper wrapper = JsonUtility.FromJson<Wrapper>(arquivo.text);
            Problemas = new List<Problema>(wrapper.problemas);
        }
        else
        {
            Debug.LogError($"Arquivo banco_de_dados_{idioma}.json não encontrado em Resources!");
            Problemas = new List<Problema>();
        }
    }

    [System.Serializable]
    private class Wrapper
    {
        public Problema[] problemas;
    }
}


