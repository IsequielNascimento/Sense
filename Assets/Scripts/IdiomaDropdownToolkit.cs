using UnityEngine;
using UnityEngine.UIElements;

public class IdiomaDropdownToolkit : MonoBehaviour
{
    [Header("Dependências")]
    public UIDocument uiDocument;
    public MenuToolkit menuToolkitScript;

    [Header("Sprites das Bandeiras")] // Arraste os Sprites aqui! (Você já fez isso, ótimo!)
    public Sprite flagPT;
    public Sprite flagEN;
    public Sprite flagES;
    public Sprite flagFR;

    private VisualElement _painelOpcoes;      
    private VisualElement _iconeSelecionado;  // Vamos clicar AQUI para abrir

    private void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (menuToolkitScript == null) menuToolkitScript = GetComponent<MenuToolkit>();

        var root = uiDocument.rootVisualElement;

        // Buscando elementos
        _painelOpcoes = root.Q<VisualElement>("painelOpcoes");
        _iconeSelecionado = root.Q<VisualElement>("idiomaSelecionado");

        // --- CORREÇÃO AQUI: Clique no ícone, não no container todo ---
        if (_iconeSelecionado != null)
            _iconeSelecionado.RegisterCallback<ClickEvent>(evt => AlternarPainel());

        // Botões de Idioma
        var btnPT = root.Q<Button>("botaoPortugues");
        var btnEN = root.Q<Button>("botaoIngles");
        var btnES = root.Q<Button>("botaoEspanhol");
        var btnFR = root.Q<Button>("botaoFrances");

        if (btnPT != null) btnPT.clicked += () => SelecionarIdioma("pt", flagPT);
        if (btnEN != null) btnEN.clicked += () => SelecionarIdioma("en", flagEN);
        if (btnES != null) btnES.clicked += () => SelecionarIdioma("es", flagES);
        if (btnFR != null) btnFR.clicked += () => SelecionarIdioma("fr", flagFR);

        // Começa fechado
        if (_painelOpcoes != null) _painelOpcoes.style.display = DisplayStyle.None;
        
        // Carrega ícone inicial
        AtualizarIconeInicial(LocalizedDatabase.CurrentLanguage);
    }

    private void AlternarPainel()
    {
        if (_painelOpcoes == null) return;
        bool isVisible = _painelOpcoes.style.display == DisplayStyle.Flex;
        _painelOpcoes.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void SelecionarIdioma(string codigo, Sprite bandeira)
    {
        IdiomaManager.Instance.DefinirIdioma(codigo);

        if (_iconeSelecionado != null)
            _iconeSelecionado.style.backgroundImage = new StyleBackground(bandeira);

        if (menuToolkitScript != null)
            menuToolkitScript.AtualizarTextosUI();

        // Garante que fecha o painel
        if (_painelOpcoes != null) _painelOpcoes.style.display = DisplayStyle.None;
    }
    
    private void AtualizarIconeInicial(string codigo)
    {
        Sprite s = null;
        switch(codigo) {
            case "pt": s = flagPT; break;
            case "en": s = flagEN; break;
            case "es": s = flagES; break;
            case "fr": s = flagFR; break;
        }
        if (s != null && _iconeSelecionado != null) 
            _iconeSelecionado.style.backgroundImage = new StyleBackground(s);
    }
}
