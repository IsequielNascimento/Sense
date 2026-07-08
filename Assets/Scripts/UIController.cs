using UnityEngine;
using UnityEngine.UIElements; 
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UIController : MonoBehaviour
{
    [Header("Conexão AR")]
    public PlaceOnPlane_Adaptado placeOnPlane; 

    private string[] passosTutoriais;
    private string[] animacoes;
    private string[] telasDisplay;
    private string[] vfxs;
    
    private string layerAtual = "Base Layer"; 
    private int passoAtual = -1;
    private bool passosIniciados = false;
    private DadosMontagem dados;
    private VisualElement root;
    
    // Elementos do UI Toolkit
    private VisualElement painelPopup; 
    private VisualElement blurBackground;
    private Label labelPopup1;
    private Label labelPopup2; 
    private Button botaoOK; 

    private VisualElement grupoMontagem; 
    private VisualElement tutorialUI; 
    private Label textoTutorial; 
    private Button botaoSair; 
    private Button botaoProximo; 
    private Button botaoVoltar; 
    private Button botaoReplay; 
    
    private RadialProgress preenchimentoProgresso; 
    private Label textoNumeroProgresso; 
    private Label textoTotalPassos;     

    private VisualElement painelPopupFinal; 
    private VisualElement iconeCorreto; 
    private Label labelTituloFinal; 
    private Button botaoVoltarMenu; 
    private Button botaoRecomecar;  

    public float duracaoFade = 0.5f;

    void Awake()
    {
        CarregarBancoDeDadosMontagem.GarantirBancoCarregado();
        dados = CarregarBancoDeDadosMontagem.Dados;
        CarregarPassosDoProblemaOuMontagem(); 
    }

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;
        root = uiDocument.rootVisualElement;

        blurBackground = root.Q<VisualElement>("FundoBlur");
        painelPopup = root.Q<VisualElement>("PainelPopup");
        labelPopup1 = root.Q<Label>("LabelTexto1");
        labelPopup2 = root.Q<Label>("LabelTexto2");
        botaoOK = root.Q<Button>("OK");

        grupoMontagem = root.Q<VisualElement>("GrupoMontagem");
        tutorialUI = root.Q<VisualElement>("Tutorial"); 
        textoTutorial = root.Q<Label>("TextoTutorial");
        
        // --- TESTE DE DIAGNÓSTICO DO BOTÃO SAIR ---
        botaoSair = root.Q<Button>("Sair");
        
        if (botaoSair == null) 
        { 
            Debug.LogError("🚨 ALERTA: O Unity NÃO ACHOU o botão Sair! Verifique se o nome no UI Builder é exatamente 'Sair' e se você salvou o arquivo .uxml."); 
        } 
        else 
        { 
            Debug.Log("✅ SUCESSO: O botão Sair foi encontrado no UI Builder e conectado ao script!"); 
        }
        // ------------------------------------------

        botaoProximo = root.Q<Button>("Proximo");
        botaoVoltar = root.Q<Button>("Voltar");
        botaoReplay = root.Q<Button>("Replay");

        preenchimentoProgresso = root.Q<RadialProgress>("PreenchimentoProgresso");
        textoNumeroProgresso = root.Q<Label>("TextoNumeroProgresso");
        textoTotalPassos = root.Q<Label>("NumeroTotal"); 
        painelPopupFinal = root.Q<VisualElement>("PainelPopupFinal");
        iconeCorreto = root.Q<VisualElement>("Correto");
        labelTituloFinal = root.Q<Label>("Parabens"); 
        botaoVoltarMenu = root.Q<Button>("BotaoVoltar"); 
        botaoRecomecar = root.Q<Button>("Recomecar"); 

        RegistrarCallback(botaoOK, OnBotaoOkClicado);
        RegistrarCallback(botaoSair, OnBotaoSairClicado);
        RegistrarCallback(botaoProximo, AvancarPasso);
        RegistrarCallback(botaoVoltar, VoltarPasso);
        RegistrarCallback(botaoReplay, RepetirPasso);
        RegistrarCallback(botaoVoltarMenu, VoltarMenu);
        RegistrarCallback(botaoRecomecar, RecomecarMontagem);
    }

    void Start()
    {
        if (root == null) return;
        if (placeOnPlane == null) placeOnPlane = Object.FindFirstObjectByType<PlaceOnPlane_Adaptado>();
        
        ConfigurarTextosIniciais(); 
        
        MostrarElemento(grupoMontagem, false);
        MostrarElemento(tutorialUI, false); 
        MostrarElemento(painelPopupFinal, false);
        
        StartCoroutine(Fade(painelPopup, 0, 1));
        StartCoroutine(Fade(blurBackground, 0, 1));
    }

    private void ConfigurarTextosIniciais()
    {
        if (dados == null) return;
        SetText(labelPopup1, dados.popupInicialTexto1);
        SetText(labelPopup2, dados.popupInicialTexto2);
        SetText(botaoOK, dados.popupInicialBotao);
        SetText(botaoSair, dados.sair);
        if (textoTotalPassos != null && passosTutoriais != null) 
            textoTotalPassos.text = $"{dados.textoDe} {passosTutoriais.Length}";
        SetText(labelTituloFinal, dados.popupFinalTitulo);
        SetText(botaoVoltarMenu, dados.popupFinalBotaoMenu);
        SetText(botaoRecomecar, dados.popupFinalBotaoRecomecar);
    }

    private void OnBotaoOkClicado(ClickEvent evt)
    {
        StartCoroutine(Fade(painelPopup, 1, 0));
        StartCoroutine(Fade(blurBackground, 1, 0));
        MostrarElemento(tutorialUI, true);
        MostrarElemento(grupoMontagem, false); 
        tutorialUI.style.left = 150; 
        tutorialUI.style.right = 0;
    }

    public void IniciarPassos()
    {
        if (passosTutoriais == null || passosTutoriais.Length == 0) return;
        
        passoAtual = 0;
        passosIniciados = true;
        
        AtualizarPasso();

        MostrarElemento(grupoMontagem, true);
        MostrarElemento(tutorialUI, true);
        tutorialUI.style.left = new Length(25, LengthUnit.Percent); 
    }

    private void AtualizarPasso()
    {
        if (passoAtual < 0 || passoAtual >= passosTutoriais.Length) return;

        SetText(textoTutorial, passosTutoriais[passoAtual]);
        SetText(textoNumeroProgresso, (passoAtual + 1).ToString());

        if (preenchimentoProgresso != null)
        {
            float porcentagem = (float)(passoAtual + 1) / passosTutoriais.Length;
            preenchimentoProgresso.Progress = porcentagem; 
        }

        bool ultimo = passoAtual == passosTutoriais.Length - 1;
        if (botaoProximo != null) botaoProximo.text = ultimo ? (dados != null ? dados.finalizar : "Finalizar") : (dados != null ? dados.proximo : "Próximo");
        
        if (placeOnPlane != null && animacoes != null && passoAtual < animacoes.Length)
        {
            string telaAtual = (telasDisplay != null && passoAtual < telasDisplay.Length) ? telasDisplay[passoAtual] : "";
            string vfxAtual = (vfxs != null && passoAtual < vfxs.Length) ? vfxs[passoAtual] : "";
            
            placeOnPlane.PlayAnimation(animacoes[passoAtual], layerAtual, telaAtual, vfxAtual);
        }
    }

    private void CarregarPassosDoProblemaOuMontagem()
    {
        string origem = ControleDeCena.Instance != null ? ControleDeCena.Instance.origemDaCena : "montagem";
        string idioma = IdiomaManager.Instance != null
            ? IdiomaManager.Instance.ObterIdioma()
            : PlayerPrefs.GetString("idioma", "pt");

        if (origem == "montagem" || string.IsNullOrEmpty(origem))
        {
            layerAtual = "Base Layer";
            if (dados != null && dados.passos != null && dados.passos.Length > 0)
            {
                passosTutoriais = dados.passos.Select(p => p.tutorial).ToArray();
                animacoes = dados.passos.Select(p => $"animacao_{p.numero}").ToArray();
                Debug.Log($"[AR Toolkit] Montagem Padrão carregada com {animacoes.Length} animações.");
            }
            else
            {
                passosTutoriais = new string[0];
                animacoes = new string[0];
            }
            telasDisplay = new string[0];
            vfxs = new string[0];
        }
        else
        {
            string idDoProblema = origem;
            if (ProblemaSelecionadoAR.Instance != null && !string.IsNullOrEmpty(ProblemaSelecionadoAR.Instance.idProblema))
            {
                idDoProblema = ProblemaSelecionadoAR.Instance.idProblema;
            }

            string caminhoDoJson = $"BancoDeDadosProblemas/{idioma}/{idDoProblema}";
            TextAsset json = Resources.Load<TextAsset>(caminhoDoJson);

            if (json == null && idioma != "pt")
            {
                Debug.LogWarning($"[AR Toolkit] Tradução ausente em Resources/{caminhoDoJson}. Usando fallback pt.");
                caminhoDoJson = $"BancoDeDadosProblemas/pt/{idDoProblema}";
                json = Resources.Load<TextAsset>(caminhoDoJson);
            }

            if (json != null)
            {
                PassoAPasso dadosProblema = JsonUtility.FromJson<PassoAPasso>(json.text);
                if (dadosProblema != null && dadosProblema.etapas != null && dadosProblema.etapas.Length > 0)
                {
                    passosTutoriais = dadosProblema.etapas.Select(e => e.tutorial).ToArray();
                    animacoes = dadosProblema.etapas.Select(e => e.animacao).ToArray();
                    telasDisplay = dadosProblema.etapas.Select(e => e.telaDisplay).ToArray();
                    vfxs = dadosProblema.etapas.Select(e => e.vfx).ToArray();
                    layerAtual = !string.IsNullOrEmpty(dadosProblema.layer) ? dadosProblema.layer : "Base Layer";

                    Debug.Log($"[AR Toolkit] JSON Lido com sucesso. Primeira tela encontrada: '{telasDisplay[0]}', Primeiro VFX: '{vfxs[0]}'");
                }
            }
            else
            {
                Debug.LogError($"[AR Toolkit] Erro: Arquivo JSON não encontrado em Resources/{caminhoDoJson}.");
                passosTutoriais = new string[0];
                animacoes = new string[0];
                telasDisplay = new string[0];
                vfxs = new string[0];
            }
        }
    }

    public void AvancarPasso(ClickEvent evt = null) 
    {
        if (!passosIniciados) return;
        if (passoAtual >= passosTutoriais.Length - 1) { MostrarPopupFinal(); return; }
        passoAtual++;
        AtualizarPasso();
    }

    public void VoltarPasso(ClickEvent evt)
    {
        if (!passosIniciados || passoAtual <= 0) return;
        passoAtual--;
        AtualizarPasso();
    }

    public void RepetirPasso(ClickEvent evt)
    {
        if (!passosIniciados) return;
        AtualizarPasso(); 
    }

    private void MostrarPopupFinal()
    {
        MostrarElemento(grupoMontagem, false); 
        MostrarElemento(tutorialUI, false);
        StartCoroutine(Fade(painelPopupFinal, 0, 1));
        StartCoroutine(Fade(blurBackground, 0, 1)); 
    }

    public void VoltarMenu(ClickEvent evt) { SceneManager.LoadScene(0); }

    public void RecomecarMontagem(ClickEvent evt)
    {
        StartCoroutine(Fade(painelPopupFinal, 1, 0));
        StartCoroutine(Fade(blurBackground, 1, 0, () => {
            MostrarElemento(grupoMontagem, true); 
            MostrarElemento(tutorialUI, true);
            tutorialUI.style.left = new Length(25, LengthUnit.Percent);
            ReiniciarMontagem();
        }));
    }

    private void ReiniciarMontagem()
    {
        if (passosTutoriais == null || passosTutoriais.Length == 0) return;
        passoAtual = 0;
        passosIniciados = true;
        AtualizarPasso();
    }

    private void OnBotaoSairClicado(ClickEvent evt) 
    { 
        Debug.Log("👆 CLICK REGISTRADO: O botão SAIR foi clicado e vai tentar carregar a cena 0.");
        VoltarMenu(evt); 
    }

    private void MostrarElemento(VisualElement elemento, bool mostrar)
    {
        if (elemento == null) return;
        elemento.style.display = mostrar ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void SetText(Label l, string s) { if (l != null) l.text = s; }
    private void SetText(Button b, string s) { if (b != null) b.text = s; }
    private void RegistrarCallback(Button b, EventCallback<ClickEvent> callback) { if (b != null) b.RegisterCallback(callback); }

    private IEnumerator Fade(VisualElement elemento, float de, float para, System.Action aoConcluir = null)
    {
        if (elemento == null) { aoConcluir?.Invoke(); yield break; }
        if (para > 0) MostrarElemento(elemento, true);
        elemento.style.opacity = de;
        float tempo = 0;
        while (tempo < duracaoFade) {
            float alpha = Mathf.Lerp(de, para, tempo / duracaoFade);
            elemento.style.opacity = alpha;
            tempo += Time.deltaTime;
            yield return null;
        }
        elemento.style.opacity = para;
        if (para == 0) MostrarElemento(elemento, false);
        aoConcluir?.Invoke();
    }
}