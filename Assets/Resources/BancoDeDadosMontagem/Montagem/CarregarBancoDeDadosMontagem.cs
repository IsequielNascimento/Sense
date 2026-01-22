using UnityEngine;

// ===================================================================
// ===== CLASSES DE DADOS MOVIDAS PARA FORA DA CLASSE PRINCIPAL =====
//
// Isto corrige o erro CS0246 e permite que o UIController.cs
// enxergue a classe "DadosMontagem".
// ===================================================================

[System.Serializable]
public class DadosMontagem
{
    public PassoMontagem[] passos;

    // textos gerais
    public string proximo;
    public string finalizar;
    public string popupFinalTitulo;
    public string popupFinalBotaoMenu;
    public string popupFinalBotaoRecomecar;

    // textos do popup inicial
    public string popupInicialTexto1;
    public string popupInicialTexto2;
    public string popupInicialBotao;

    // topo/botões
    public string sair;
    public string textoDe;

    // texto de orientação que fica depois do popup
    public string tutorialInicial;
}

[System.Serializable]
public class PassoMontagem
{
    public string tutorial;
    public string numero;
}


// ===================================================================
// ===== SUA CLASSE PRINCIPAL (SEM ALTERAÇÕES NA LÓGICA) =====
// ===================================================================
public class CarregarBancoDeDadosMontagem : MonoBehaviour
{
    public static DadosMontagem Dados { get; private set; }

    /// <summary>
    /// Carrega explicitamente o banco para um idioma (pt, en, es, fr).
    /// </summary>
    public static void Carregar(string idioma)
    {
        TextAsset arquivo = Resources.Load<TextAsset>($"BancoDeDadosMontagem/Montagem/banco_montagem_{idioma}");
        if (arquivo != null)
        {
            Dados = JsonUtility.FromJson<DadosMontagem>(arquivo.text);
        }
        else
        {
            Debug.LogError($"Arquivo banco_montagem_{idioma}.json não encontrado em Resources/BancoDeDadosMontagem/Montagem/.");
            Dados = new DadosMontagem(); // evita null
        }
    }

    /// <summary>
    /// Garante que o banco esteja carregado. Se ainda não estiver, tenta
    /// descobrir o idioma atual e carrega. Usa IdiomaManager (se existir)
    /// ou PlayerPrefs ("idioma"), caindo em 'pt' como padrão.
    /// </summary>
    public static void GarantirBancoCarregado()
    {
        if (Dados != null) return;

        string idioma = "pt";
        // Usa IdiomaManager se existir
        // if (IdiomaManager.Instance != null)
        // {
        //     idioma = IdiomaManager.Instance.ObterIdioma();
        // }
        // else
        // {
            // fallback por PlayerPrefs
            idioma = PlayerPrefs.GetString("idioma", "pt");
        // }

        Carregar(idioma);
    }
    
    // As classes DadosMontagem e PassoMontagem
    // foram removidas daqui e colocadas acima,
    // no escopo global do arquivo.
}