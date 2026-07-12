using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    [Header("Conexao AR")]
    [FormerlySerializedAs("placeOnPlane")]
    public ExibidorDeModeloBase exibidor;

    public float duracaoFade = 0.5f;

    private DadosMontagem text;
    private StepSequenceData content;
    private StepSequence sequence;
    private bool popupInicialResolvido;
    private VisualElement root;

    private VisualElement painelPopup, blurBackground, grupoMontagem, tutorialUI, painelPopupFinal;
    private Label labelPopup1, labelPopup2, textoTutorial, textoNumeroProgresso, textoTotalPassos, labelTituloFinal;
    private Button botaoOK, botaoSair, botaoProximo, botaoVoltar, botaoReplay, botaoVoltarMenu, botaoRecomecar;
    private RadialProgress preenchimentoProgresso;

    private void Awake()
    {
        OrigemCena source = ControleDeCena.Instance?.OrigemDaCena ?? OrigemCena.Montagem;
        string problemId = ProblemaSelecionadoAR.Instance?.idProblema;
        ArExperienceData experience = LocalizedDatabase.LoadArExperience(source, problemId);

        text = experience.UiText;
        content = experience.Sequence;
        sequence = new StepSequence(content.Steps);
    }

    private void OnEnable()
    {
        UIDocument document = GetComponent<UIDocument>();
        if (document == null) return;

        root = document.rootVisualElement;
        QueryView();
        RegisterCallbacks();
    }

    private void Start()
    {
        if (root == null) return;
        if (exibidor == null) exibidor = FindFirstObjectByType<ExibidorDeModeloBase>();

        ConfigureText();
        Show(grupoMontagem, false);
        Show(tutorialUI, false);
        Show(painelPopupFinal, false);

        if (FindFirstObjectByType<ARInitializer>() == null) MostrarPopupInicial();
        else
        {
            Show(painelPopup, false);
            Show(blurBackground, false);
        }
    }

    public void MostrarPopupInicial()
    {
        if (popupInicialResolvido) return;

        StartCoroutine(Fade(painelPopup, 0, 1));
        StartCoroutine(Fade(blurBackground, 0, 1));
    }

    public void PularPopupInicial()
    {
        popupInicialResolvido = true;
        Show(painelPopup, false);
        Show(blurBackground, false);
    }

    public void IniciarPassos()
    {
        if (!sequence.Start()) return;

        RenderCurrentStep();
        Show(grupoMontagem, true);
        Show(tutorialUI, true);
        tutorialUI.style.left = new Length(25, LengthUnit.Percent);
    }

    public void AvancarPasso(ClickEvent evt = null)
    {
        StepAdvanceResult result = sequence.Advance();
        if (result == StepAdvanceResult.Completed)
        {
            ShowFinalPopup();
        }
        else if (result == StepAdvanceResult.Advanced)
        {
            RenderCurrentStep();
        }
    }

    public void VoltarPasso(ClickEvent evt = null)
    {
        if (sequence.Back())
        {
            RenderCurrentStep();
        }
    }

    public void RepetirPasso(ClickEvent evt = null)
    {
        if (sequence.IsStarted)
        {
            RenderCurrentStep();
        }
    }

    public void VoltarMenu(ClickEvent evt = null)
    {
        SceneManager.LoadScene(Scenes.MainMenu);
    }

    public void RecomecarMontagem(ClickEvent evt = null)
    {
        StartCoroutine(Fade(painelPopupFinal, 1, 0));
        StartCoroutine(Fade(blurBackground, 1, 0, RestartAndRender));
    }

    private void QueryView()
    {
        blurBackground = root.Q<VisualElement>("FundoBlur");
        painelPopup = root.Q<VisualElement>("PainelPopup");
        labelPopup1 = root.Q<Label>("LabelTexto1");
        labelPopup2 = root.Q<Label>("LabelTexto2");
        botaoOK = root.Q<Button>("OK");
        grupoMontagem = root.Q<VisualElement>("GrupoMontagem");
        tutorialUI = root.Q<VisualElement>("Tutorial");
        textoTutorial = root.Q<Label>("TextoTutorial");
        botaoSair = root.Q<Button>("Sair");
        botaoProximo = root.Q<Button>("Proximo");
        botaoVoltar = root.Q<Button>("Voltar");
        botaoReplay = root.Q<Button>("Replay");
        preenchimentoProgresso = root.Q<RadialProgress>("PreenchimentoProgresso");
        textoNumeroProgresso = root.Q<Label>("TextoNumeroProgresso");
        textoTotalPassos = root.Q<Label>("NumeroTotal");
        painelPopupFinal = root.Q<VisualElement>("PainelPopupFinal");
        labelTituloFinal = root.Q<Label>("Parabens");
        botaoVoltarMenu = root.Q<Button>("BotaoVoltar");
        botaoRecomecar = root.Q<Button>("Recomecar");

        if (botaoSair == null) Debug.LogError("[UIController] Elemento UXML 'Sair' nao encontrado.");
    }

    private void RegisterCallbacks()
    {
        Register(botaoOK, OnInitialPopupAccepted);
        Register(botaoSair, VoltarMenu);
        Register(botaoProximo, AvancarPasso);
        Register(botaoVoltar, VoltarPasso);
        Register(botaoReplay, RepetirPasso);
        Register(botaoVoltarMenu, VoltarMenu);
        Register(botaoRecomecar, RecomecarMontagem);
    }

    private void ConfigureText()
    {
        SetText(labelPopup1, text.popupInicialTexto1);
        SetText(labelPopup2, text.popupInicialTexto2);
        SetText(botaoOK, text.popupInicialBotao);
        SetText(botaoSair, text.sair);
        SetText(textoTotalPassos, $"{text.textoDe} {sequence.Count}");
        SetText(labelTituloFinal, text.popupFinalTitulo);
        SetText(botaoVoltarMenu, text.popupFinalBotaoMenu);
        SetText(botaoRecomecar, text.popupFinalBotaoRecomecar);
    }

    private void OnInitialPopupAccepted(ClickEvent evt)
    {
        popupInicialResolvido = true;
        StartCoroutine(Fade(painelPopup, 1, 0));
        StartCoroutine(Fade(blurBackground, 1, 0));
        Show(tutorialUI, true);
        Show(grupoMontagem, false);
        tutorialUI.style.left = 150;
        tutorialUI.style.right = 0;
    }

    private void RenderCurrentStep()
    {
        int index = sequence.CurrentIndex;
        SetText(textoTutorial, sequence.Current);
        SetText(textoNumeroProgresso, (index + 1).ToString());

        if (preenchimentoProgresso != null)
            preenchimentoProgresso.Progress = (float)(index + 1) / sequence.Count;

        SetText(botaoProximo, sequence.IsLast ? text.finalizar : text.proximo);
        if (exibidor != null)
        {
            exibidor.PlayAnimation(
                ValueAt(content.Animations, index),
                content.Layer,
                ValueAt(content.Displays, index),
                ValueAt(content.Vfx, index));
        }
    }

    private void ShowFinalPopup()
    {
        Show(grupoMontagem, false);
        Show(tutorialUI, false);
        StartCoroutine(Fade(painelPopupFinal, 0, 1));
        StartCoroutine(Fade(blurBackground, 0, 1));
    }

    private void RestartAndRender()
    {
        if (!sequence.Restart()) return;

        Show(grupoMontagem, true);
        Show(tutorialUI, true);
        tutorialUI.style.left = new Length(25, LengthUnit.Percent);
        RenderCurrentStep();
    }

    private static string ValueAt(string[] values, int index)
    {
        return values != null && index >= 0 && index < values.Length ? values[index] : string.Empty;
    }

    private static void Show(VisualElement element, bool visible)
    {
        if (element != null) element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private static void SetText(TextElement element, string value)
    {
        if (element != null) element.text = value ?? string.Empty;
    }

    private static void Register(Button button, EventCallback<ClickEvent> callback)
    {
        button?.RegisterCallback(callback);
    }

    private IEnumerator Fade(VisualElement element, float from, float to, Action completed = null)
    {
        if (element == null)
        {
            completed?.Invoke();
            yield break;
        }

        if (to > 0) Show(element, true);

        element.style.opacity = from;
        float elapsed = 0;
        while (elapsed < duracaoFade)
        {
            element.style.opacity = Mathf.Lerp(from, to, elapsed / duracaoFade);
            elapsed += Time.deltaTime;
            yield return null;
        }

        element.style.opacity = to;
        if (to == 0) Show(element, false);

        completed?.Invoke();
    }
}
