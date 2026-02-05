using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class Montagem2Toolkit : MonoBehaviour
{
    public UIDocument uiDocument;

    // Elementos da UI
    private Label _lblTitulo;
    private Label _lblSubtitulo;
    private Label _lblAvisoTexto; // O texto dentro do quadro vermelho
    private Button _btnIniciar;   // O botão laranja
    private Label _lblTextoBotaoIniciar; // Caso o texto seja um objeto separado dentro do botão
    private Button _btnVoltar;    // A setinha lá em cima

    void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // --- 1. BUSCAR ELEMENTOS (Baseado no seu print do UI Builder) ---
        
        _lblTitulo = root.Q<Label>("Titulo");
        _lblSubtitulo = root.Q<Label>("subtitulo");
        _btnVoltar = root.Q<Button>("Botao-voltar");

        // O texto do aviso parece estar dentro de um grupo chamado "Aviso"
        var grupoAviso = root.Q<VisualElement>("Aviso");
        if (grupoAviso != null)
            _lblAvisoTexto = grupoAviso.Q<Label>("texto");

        // O botão iniciar chama-se "botao" e tem um label "texto" dentro
        _btnIniciar = root.Q<Button>("botao"); // Botão Laranja
        if (_btnIniciar != null)
            _lblTextoBotaoIniciar = _btnIniciar.Q<Label>("texto");


        // --- 2. CONFIGURAR CLIQUES ---

        // Botão Voltar -> Vai para o Menu
        if (_btnVoltar != null)
            _btnVoltar.clicked += () => SceneManager.LoadScene("Main-Menu");

        // Botão Iniciar -> Vai para o AR
        if (_btnIniciar != null)
            _btnIniciar.clicked += IrParaAR;


        // --- 3. APLICAR TEXTOS ---
        AtualizarTextos();
    }

    void AtualizarTextos()
    {
        string idioma = IdiomaManager.Instance.ObterIdioma();
        CarregarDadosMontagem2Toolkit.Carregar(idioma);
        var dados = CarregarDadosMontagem2Toolkit.Dados;

        if (dados != null)
        {
            if (_lblTitulo != null) _lblTitulo.text = dados.titulo;
            if (_lblSubtitulo != null) _lblSubtitulo.text = dados.subtitulo;
            
            // Texto do Aviso
            if (_lblAvisoTexto != null) _lblAvisoTexto.text = dados.botao_aviso;

            // Texto do Botão (Prioriza o Label interno se existir, senão usa o próprio botão)
            if (_lblTextoBotaoIniciar != null) 
                _lblTextoBotaoIniciar.text = dados.botao_iniciar;
            else if (_btnIniciar != null)
                _btnIniciar.text = dados.botao_iniciar;
        }
    }

    void IrParaAR()
    {
        // Se precisar carregar algo específico para a montagem antes de ir:
        // string idioma = IdiomaManager.Instance.ObterIdioma();
        // CarregarBancoDeDadosMontagem.Carregar(idioma);

        SceneManager.LoadScene("ARMudanca");
    }
}