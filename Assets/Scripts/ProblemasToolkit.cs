using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class ProblemasToolkit : MonoBehaviour
{
    #region MARK - Referências

    public const string PrefixoDoBotaoDeProblema = "botao-problema-";

    public UIDocument uiDocument;

    #endregion

    #region MARK - Ligação com os dados

    void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        if (root == null) return;

        BancoProblemas banco = LocalizedDatabase.Load<BancoProblemas>(LocalizedDatabase.ProblemasPath);
        Problema[] problemas = banco?.problemas ?? new Problema[0];

        for (int i = 0; i < problemas.Length; i++)
        {
            var botao = root.Q<Button>($"{PrefixoDoBotaoDeProblema}{i + 1}");
            if (botao == null) continue;

            Problema problema = problemas[i];
            if (!IdentidadeDoCenario.EstaDefinida(problema.ResourceKey))
            {
                Debug.LogWarning($"[ProblemasToolkit] Problema {i + 1} nao possui identificador de recurso.", this);
                continue;
            }

            botao.clicked += () => SelecionarProblema(problema);
        }
    }

    #endregion

    #region MARK - Seleção

    void SelecionarProblema(Problema problema)
    {
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
            selecionado.AddComponent<ProblemaSelecionadoAR>()
                .Selecionar(problema.ResourceKey, problema.ScenarioId);
        }
        else
        {
            ProblemaSelecionadoAR.Instance.Selecionar(problema.ResourceKey, problema.ScenarioId);
        }

        SceneManager.LoadScene(Scenes.AssemblyWarning);
    }

    #endregion
}
