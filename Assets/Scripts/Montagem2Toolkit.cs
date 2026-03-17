using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class Montagem2Toolkit : MonoBehaviour
{
    public UIDocument uiDocument;

    // --- Elementos da UI ---
    // Note que _btnIniciar e _btnVoltar agora são VisualElement
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

        // 1. Buscando os Elementos (Agora buscando como VisualElement)
        _lblTitulo = root.Q<Label>("Titulo");
        _lblSubtitulo = root.Q<Label>("subtitulo");
        
        _btnVoltar = root.Q<VisualElement>("Botao-voltar");
        _btnIniciar = root.Q<VisualElement>("botao"); 

        var grupoAviso = root.Q<VisualElement>("Aviso");
        if (grupoAviso != null)
            _lblAvisoTexto = grupoAviso.Q<Label>("texto");

        if (_btnIniciar != null)
            _lblTextoBotaoIniciar = _btnIniciar.Q<Label>("texto");


        // 2. Configurando Cliques (Usando RegisterCallback para VisualElements)
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


        // 3. Atualizar Textos
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
            if (_lblAvisoTexto != null) _lblAvisoTexto.text = dados.botao_aviso;

            if (_lblTextoBotaoIniciar != null) 
                _lblTextoBotaoIniciar.text = dados.botao_iniciar;
        }
    }

    void VoltarParaMenu()
    {
        SceneManager.LoadScene("Main-Menu");
    }

    void IrParaAR()
    {
        Debug.Log("[Montagem2] Botão clicado! Carregando Banco de Dados e Cena AR...");

        // Pega o idioma atual
        string idioma = IdiomaManager.Instance.ObterIdioma();

        // Carrega o banco de dados da montagem (sua lógica do NavegarParaMontagemPadrao.cs)
        CarregarBancoDeDadosMontagem.Carregar(idioma);

        // Muda para a cena AR
        SceneManager.LoadScene("AR_Cena_UIToolkit");
    }
}