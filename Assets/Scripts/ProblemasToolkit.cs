using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class ProblemasToolkit : MonoBehaviour
{
    public UIDocument uiDocument;

    void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // Adapte os IDs "botao-problema-1", etc., conforme seu UXML dessa cena
        var btnProblema1 = root.Q<Button>("botao-problema-1");
        
        if (btnProblema1 != null)
        {
            // CORREÇÃO: Passando o nome exato do arquivo (A1) para o ControleDeCena
            btnProblema1.clicked += () => SelecionarProblema("A1");
        }
    }

    void SelecionarProblema(string tipoProblema)
    {
        // 1. Define qual problema foi escolhido
        if (ControleDeCena.Instance != null)
        {
            ControleDeCena.Instance.DefinirOrigem(tipoProblema);
        }
        else
        {
            Debug.LogWarning("[Unity AR] ControleDeCena não encontrado na cena atual.");
        }

        // 2. Manda para a cena intermediária (Montagem2) conforme solicitado
        SceneManager.LoadScene("Montagem2");
    }
}