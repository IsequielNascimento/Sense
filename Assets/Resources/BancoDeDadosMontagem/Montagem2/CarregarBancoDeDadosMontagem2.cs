using UnityEngine;

public class CarregarBancoDeDadosMontagem2 : MonoBehaviour
{
    public static DadosMontagem2 Dados { get; private set; }

    public static void Carregar(string idioma)
    {
        TextAsset arquivo = Resources.Load<TextAsset>($"BancoDeDadosMontagem/Montagem2/banco_montagem2_{idioma}");

        if (arquivo != null)
        {
            Dados = JsonUtility.FromJson<DadosMontagem2>(arquivo.text);
        }
        else
        {
            Debug.LogError($"Arquivo banco_montagem2_{idioma}.json não encontrado!");
            Dados = new DadosMontagem2(); // evita null
        }
    }
}

[System.Serializable]
public class DadosMontagem2
{
    public string titulo;
    public string subtitulo;
    public string botao_iniciar;
    public string botao_aviso;
}
