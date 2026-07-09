using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARInitializer : MonoBehaviour
{
    private enum ARErrorType
    {
        Unsupported,
        Initialization,
        CameraPermission
    }

    public ARSession arSession;
    public UIDocument uiDocument;
    public string menuSceneName = "Main-Menu";
    public float initializationTimeout = 10f;

    private VisualElement arErrorPanel;
    private Label arErrorTitle;
    private Label arErrorMessage;
    private Button arErrorBackButton;
    private bool arErrorButtonRegistered;

    private void Start()
    {
        if (arSession == null)
        {
            arSession = FindObjectOfType<ARSession>();
        }

        if (uiDocument == null)
        {
            uiDocument = FindObjectOfType<UIDocument>();
        }

        CacheErrorOverlay();

        if (arSession == null)
        {
            Debug.LogError("ARSession nao encontrado na cena.");
            ShowARError(ARErrorType.Initialization);
            return;
        }

        arSession.enabled = false;
        SetARManagersActive(false);
        StartCoroutine(InitializeARSessionManually());
    }

    private IEnumerator InitializeARSessionManually()
    {
        Debug.Log("Iniciando verificacao de suporte ao AR...");

        if (!HasCameraPermission())
        {
            yield return RequestCameraPermission();
        }

        if (!HasCameraPermission())
        {
            Debug.LogError("Permissao da camera negada. A sessao AR nao pode ser iniciada.");
            ShowARError(ARErrorType.CameraPermission);
            yield break;
        }

        if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
        {
            yield return ARSession.CheckAvailability();
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            Debug.LogError("Dispositivo nao suporta AR. Estado: " + ARSession.state);
            ShowARError(ARErrorType.Unsupported);
            yield break;
        }

        if (ARSession.state == ARSessionState.NeedsInstall)
        {
            yield return ARSession.Install();
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            Debug.LogError("Dispositivo nao suporta AR apos tentativa de instalacao. Estado: " + ARSession.state);
            ShowARError(ARErrorType.Unsupported);
            yield break;
        }

        if (ARSession.state == ARSessionState.NeedsInstall)
        {
            Debug.LogError("Instalacao do provedor AR negada ou indisponivel. Estado: " + ARSession.state);
            ShowARError(ARErrorType.Initialization);
            yield break;
        }

        if (ARSession.state == ARSessionState.Ready ||
            ARSession.state == ARSessionState.SessionInitializing ||
            ARSession.state == ARSessionState.SessionTracking)
        {
            Debug.Log("Suporte AR encontrado. Ativando ARSession.");
            arSession.enabled = true;

            float startTime = Time.realtimeSinceStartup;
            while (ARSession.state != ARSessionState.SessionTracking)
            {
                if (ARSession.state == ARSessionState.Unsupported)
                {
                    Debug.LogError("ARSession ficou sem suporte durante a inicializacao. Estado: " + ARSession.state);
                    ShowARError(ARErrorType.Unsupported);
                    yield break;
                }

                if (ARSession.state == ARSessionState.NeedsInstall ||
                    Time.realtimeSinceStartup - startTime > initializationTimeout)
                {
                    Debug.LogError("Falha ou timeout na inicializacao do ARSession. Estado: " + ARSession.state);
                    ShowARError(ARErrorType.Initialization);
                    yield break;
                }

                yield return null;
            }

            Debug.Log("ARSession inicializado e rastreando com sucesso!");
            SetARManagersActive(true);
            yield break;
        }

        Debug.LogError("Falha na inicializacao do ARSession. Estado: " + ARSession.state);
        ShowARError(ARErrorType.Initialization);
    }

    private bool HasCameraPermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera);
#elif UNITY_IOS && !UNITY_EDITOR
        return Application.HasUserAuthorization(UserAuthorization.WebCam);
#else
        return true;
#endif
    }

    private IEnumerator RequestCameraPermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bool permissionAnswered = false;
        var callbacks = new UnityEngine.Android.PermissionCallbacks();
        callbacks.PermissionGranted += _ => permissionAnswered = true;
        callbacks.PermissionDenied += _ => permissionAnswered = true;
        callbacks.PermissionDeniedAndDontAskAgain += _ => permissionAnswered = true;

        UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera, callbacks);

        float startTime = Time.realtimeSinceStartup;
        while (!permissionAnswered && Time.realtimeSinceStartup - startTime < 30f)
        {
            yield return null;
        }
#elif UNITY_IOS && !UNITY_EDITOR
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
#else
        yield break;
#endif
    }

    private void CacheErrorOverlay()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null)
        {
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        arErrorPanel = root.Q<VisualElement>("PainelErroAR");
        arErrorTitle = root.Q<Label>("TituloErroAR");
        arErrorMessage = root.Q<Label>("MensagemErroAR");
        arErrorBackButton = root.Q<Button>("BotaoErroARVoltar");

        if (arErrorPanel != null)
        {
            arErrorPanel.style.display = DisplayStyle.None;
        }

        if (arErrorBackButton != null && !arErrorButtonRegistered)
        {
            arErrorBackButton.clicked += BackToMenu;
            arErrorButtonRegistered = true;
        }
    }

    private void ShowARError(ARErrorType errorType)
    {
        SetARManagersActive(false);
        if (arSession != null)
        {
            arSession.enabled = false;
        }

        CacheErrorOverlay();
        if (arErrorPanel == null)
        {
            Debug.LogError("PainelErroAR nao encontrado no UIDocument da cena.");
            return;
        }

        CarregarBancoDeDadosMontagem.GarantirBancoCarregado();
        DadosMontagem dados = CarregarBancoDeDadosMontagem.Dados;

        SetText(arErrorTitle, FirstFilled(dados?.arErroTitulo, "Nao foi possivel iniciar a realidade aumentada"));
        SetText(arErrorMessage, GetErrorMessage(errorType, dados));
        SetText(arErrorBackButton, FirstFilled(dados?.arErroBotaoMenu, dados?.popupFinalBotaoMenu, "Voltar ao menu"));

        HideElement("PainelPopup");
        HideElement("PainelPopupFinal");
        HideElement("Tutorial");
        HideElement("GrupoMontagem");

        arErrorPanel.style.display = DisplayStyle.Flex;
        arErrorPanel.style.opacity = 1;
        arErrorPanel.BringToFront();
    }

    private string GetErrorMessage(ARErrorType errorType, DadosMontagem dados)
    {
        switch (errorType)
        {
            case ARErrorType.Unsupported:
                return FirstFilled(dados?.arErroSemSuporte, "Este dispositivo nao oferece suporte a realidade aumentada.");
            case ARErrorType.CameraPermission:
                return FirstFilled(dados?.arErroPermissaoCamera, "Permita o acesso a camera nas configuracoes do sistema para usar a realidade aumentada.");
            default:
                return FirstFilled(dados?.arErroInicializacao, "Nao foi possivel iniciar a sessao de realidade aumentada. Tente novamente mais tarde.");
        }
    }

    private string FirstFilled(params string[] values)
    {
        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private void HideElement(string elementName)
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null)
        {
            return;
        }

        VisualElement element = uiDocument.rootVisualElement.Q<VisualElement>(elementName);
        if (element != null)
        {
            element.style.display = DisplayStyle.None;
        }
    }

    private void SetText(Label label, string text)
    {
        if (label != null)
        {
            label.text = text;
        }
    }

    private void SetText(Button button, string text)
    {
        if (button != null)
        {
            button.text = text;
        }
    }

    private void BackToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    private void SetARManagersActive(bool active)
    {
        var arCameraManager = FindObjectOfType<ARCameraManager>();
        if (arCameraManager != null)
        {
            arCameraManager.enabled = active;
        }

        var arPlaneManager = FindObjectOfType<ARPlaneManager>();
        if (arPlaneManager != null)
        {
            arPlaneManager.enabled = active;
        }

        var arRaycastManager = FindObjectOfType<ARRaycastManager>();
        if (arRaycastManager != null)
        {
            arRaycastManager.enabled = active;
        }
    }
}
