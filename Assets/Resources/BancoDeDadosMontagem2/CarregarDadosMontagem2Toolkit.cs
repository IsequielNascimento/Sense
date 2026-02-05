using UnityEngine;

public class CarregarDadosMontagem2Toolkit : MonoBehaviour
{
    public static DadosMontagem2Toolkit Dados { get; private set; }

    public static void Carregar(string idioma)
    {
        // Caminho: Assets/Resources/BancoDeDadosMontagem2/banco_montagem2_pt.json
        TextAsset arquivo = Resources.Load<TextAsset>($"BancoDeDadosMontagem2/banco_montagem2_{idioma}");

        if (arquivo != null)
        {
            Dados = JsonUtility.FromJson<DadosMontagem2Toolkit>(arquivo.text);
        }
        else
        {
            Debug.LogError($"[Toolkit] Arquivo JSON não encontrado em Resources/BancoDeDadosMontagem2 para o idioma: {idioma}");
            Dados = new DadosMontagem2Toolkit();
        }
    }

    [System.Serializable]
    public class DadosMontagem2Toolkit
    {
        public string titulo;
        public string subtitulo;
        public string botao_iniciar;
        public string botao_aviso;
    }
}