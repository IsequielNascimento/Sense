using UnityEngine;
using UnityEngine.UIElements; 
using UnityEngine.SceneManagement;

public class MenuToolkit : MonoBehaviour
{
    public UIDocument uiDocument;
    
    private Button _btnMontagem;
    private Button _btnProblemas;
    private Button _btnGemeo; 
    private Label _lblSubtitulo;

    private void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        _btnMontagem = root.Q<Button>("botao-montagem");
        _btnProblemas = root.Q<Button>("botao-problemas");
        _btnGemeo = root.Q<Button>("botao-gemeo");
        _lblSubtitulo = root.Q<Label>("subtitulo");

        if (_btnMontagem != null) 
            _btnMontagem.clicked += () => CarregarCena(Scenes.AssemblyWarning, OrigemCena.Montagem);

        if (_btnProblemas != null) 
            _btnProblemas.clicked += () => CarregarCena(Scenes.Problems, OrigemCena.Problema);

        if (_btnGemeo != null) 
            _btnGemeo.clicked += () => CarregarCena(Scenes.DigitalTwin, OrigemCena.Gemeo);

        AtualizarTextosUI();
    }

    private void CarregarCena(string nomeCena, OrigemCena origem)
    {
        ControleDeCena.Instance.DefinirOrigem(origem);
        SceneManager.LoadScene(nomeCena);
    }

    public void AtualizarTextosUI()
    {
        MenuTextos dados = LocalizedDatabase.Load<BancoMenu>(LocalizedDatabase.MenuPath).menu;
        if (dados != null) {
            if (_btnMontagem != null) _btnMontagem.text = dados.botao_iniciar;
            if (_btnProblemas != null) _btnProblemas.text = dados.botao_problemas;
            if (_btnGemeo != null) _btnGemeo.text = dados.botao_gemeo;
            if (_lblSubtitulo != null) _lblSubtitulo.text = dados.subtitulo;
        }
    }
}
