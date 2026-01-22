using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class Montagem2Toolkit : MonoBehaviour
{
    public UIDocument uiDocument;

    void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // Busque o botão laranja dessa tela. 
        // Verifique no UI Builder o nome dele. Vou supor "botao-iniciar".
        var btnAvancar = root.Q<Button>("botao-iniciar"); 
        
        // DICA: Se no seu UXML o nome for "botao-montagem", mude acima para "botao-montagem"

        if (btnAvancar != null)
        {
            btnAvancar.clicked += IrParaAR;
        }
    }

    void IrParaAR()
    {
        // Vai para a cena final. O "ControleDeCena" já sabe se veio de Montagem ou Problema.
        
        // Opcional: Carregar dados específicos aqui se necessário
        string idioma = IdiomaManager.Instance.ObterIdioma();
        // CarregarBancoDeDadosMontagem.Carregar(idioma); 

        SceneManager.LoadScene("ARMudanca");
    }
}