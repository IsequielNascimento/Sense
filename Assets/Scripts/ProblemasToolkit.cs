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

        // Exemplo: Supondo que você tenha botões para cada problema
        // Adapte os IDs "botao-problema-1", etc., conforme seu UXML dessa cena
        var btnProblema1 = root.Q<Button>("botao-problema-1");
        
        if (btnProblema1 != null)
            btnProblema1.clicked += () => SelecionarProblema("problema_valvula_travada");
    }

    void SelecionarProblema(string tipoProblema)
    {
        // 1. Define qual problema foi escolhido
        ControleDeCena.Instance.DefinirOrigem(tipoProblema);

        // 2. Manda para a cena intermediária (Montagem2) conforme solicitado
        SceneManager.LoadScene("Montagem2");
    }
}