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
            ControleDeCena.Instance.DefinirOrigem(OrigemCena.Problema);
        }
        else
        {
            Debug.LogWarning("[Unity AR] ControleDeCena não encontrado na cena atual.");
        }

        if (ProblemaSelecionadoAR.Instance == null)
        {
            var selecionado = new GameObject(nameof(ProblemaSelecionadoAR));
            selecionado.AddComponent<ProblemaSelecionadoAR>().idProblema = tipoProblema;
        }
        else
        {
            ProblemaSelecionadoAR.Instance.idProblema = tipoProblema;
        }

        // 2. Manda para a cena intermediária (Montagem2) conforme solicitado
        SceneManager.LoadScene(Scenes.AssemblyWarning);
    }
}
