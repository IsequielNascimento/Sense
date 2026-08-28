using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class Montagem2Toolkit : MonoBehaviour
{
    public UIDocument uiDocument;

    private Label _lblTitulo;
    private Label _lblSubtitulo;
    private Label _lblAvisoTexto;
    private VisualElement _btnIniciar; 
    private Label _lblTextoBotaoIniciar; 
    private VisualElement _btnVoltar;    

    void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        _lblTitulo = root.Q<Label>("Titulo");
        _lblSubtitulo = root.Q<Label>("subtitulo");
        
        _btnVoltar = root.Q<VisualElement>("Botao-voltar");
        _btnIniciar = root.Q<VisualElement>("botao"); 

        var grupoAviso = root.Q<VisualElement>("Aviso");
        if (grupoAviso != null)
            _lblAvisoTexto = grupoAviso.Q<Label>("texto");

        if (_btnIniciar != null)
            _lblTextoBotaoIniciar = _btnIniciar.Q<Label>("texto");


        if (_btnVoltar != null)
        {
            _btnVoltar.RegisterCallback<ClickEvent>(evt => VoltarParaMenu());
        }

        if (_btnIniciar != null)
        {
            _btnIniciar.RegisterCallback<ClickEvent>(evt => IrParaAR());
            Debug.Log("[Montagem2] Botão iniciar encontrado e pronto para clique!");
        }
        else
        {
            Debug.LogError("[Montagem2] ERRO: Elemento '#botao' não encontrado!");
        }


        AtualizarTextos();
    }

    void AtualizarTextos()
    {
        DadosMontagem2 dados = LocalizedDatabase.Load<DadosMontagem2>(LocalizedDatabase.Montagem2Path);

        if (dados != null)
        {
            if (_lblTitulo != null) _lblTitulo.text = dados.titulo;
            if (_lblSubtitulo != null) _lblSubtitulo.text = dados.subtitulo;
            if (_lblAvisoTexto != null) _lblAvisoTexto.text = dados.botao_aviso;

            if (_lblTextoBotaoIniciar != null) 
                _lblTextoBotaoIniciar.text = dados.botao_iniciar;
        }
    }

    void VoltarParaMenu()
    {
        SceneManager.LoadScene(Scenes.MainMenu);
    }

    void IrParaAR()
    {
        Debug.Log("[Montagem2] Botão clicado! Carregando Banco de Dados e Cena AR...");

        ControleDeCena.Instance?.DefinirOrigem(OrigemCena.Montagem);

        SceneManager.LoadScene(Scenes.ArUiToolkit);
    }
}
