using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARInitializer : MonoBehaviour
{
    [Header("Dependencias da cena")]
    [SerializeField] private ARSession arSession;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private UIController uiController;
    [SerializeField] private PlaceOnPlane_Adaptado exibidorAR;
    [SerializeField] private ARCameraManager arCameraManager;
    [SerializeField] private ARPlaneManager arPlaneManager;
    [SerializeField] private ARRaycastManager arRaycastManager;

    public float initializationTimeout = 10f;

    private VisualElement arErrorPanel;
    private Label arErrorTitle;
    private Label arErrorMessage;
    private Button arErrorBackButton;
    private bool arErrorButtonRegistered;
    private void Start()
    {
        ResolveDependencies();

        CacheErrorOverlay();

        if (arSession == null)
        {
            Debug.LogError("ARSession nao encontrado na cena. Iniciando modo 3D.");
            IniciarModoSemAR();
            return;
        }

        arSession.enabled = false;
        SetARManagersActive(false);
        StartCoroutine(InitializeARSessionManually());
    }

    private IEnumerator InitializeARSessionManually()
    {
        yield return null;

        Debug.Log("Iniciando verificacao de suporte ao AR...");

        if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
        {
            yield return ARSession.CheckAvailability();
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            Debug.Log("Dispositivo nao suporta AR. Iniciando modo 3D. Estado: " + ARSession.state);
            IniciarModoSemAR();
            yield break;
        }

        if (ARSession.state == ARSessionState.NeedsInstall)
        {
            yield return ARSession.Install();
        }

        if (ARSession.state == ARSessionState.Unsupported || ARSession.state == ARSessionState.NeedsInstall)
        {
            Debug.Log("Provedor AR indisponivel apos tentativa de instalacao. Iniciando modo 3D. Estado: " + ARSession.state);
            IniciarModoSemAR();
            yield break;
        }

        if (!HasCameraPermission())
        {
            yield return RequestCameraPermission();
        }

        if (!HasCameraPermission())
        {
            Debug.LogError("Permissao da camera negada. A sessao AR nao pode ser iniciada.");
            ShowARError();
            yield break;
        }

        if (ARSession.state == ARSessionState.Ready ||
            ARSession.state == ARSessionState.SessionInitializing ||
            ARSession.state == ARSessionState.SessionTracking)
        {
            Debug.Log("Suporte AR encontrado. Ativando ARSession.");
            arSession.enabled = true;

            if (uiController != null)
            {
                uiController.MostrarPopupInicial();
            }

            float startTime = Time.realtimeSinceStartup;
            while (ARSession.state != ARSessionState.SessionTracking)
            {
                if (ARSession.state == ARSessionState.Unsupported ||
                    ARSession.state == ARSessionState.NeedsInstall ||
                    Time.realtimeSinceStartup - startTime > initializationTimeout)
                {
                    Debug.Log("Falha ou timeout na inicializacao do ARSession. Iniciando modo 3D. Estado: " + ARSession.state);
                    IniciarModoSemAR();
                    yield break;
                }

                yield return null;
            }

            Debug.Log("ARSession inicializado e rastreando com sucesso!");
            SetARManagersActive(true);
            yield break;
        }

        Debug.Log("Estado inesperado do ARSession. Iniciando modo 3D. Estado: " + ARSession.state);
        IniciarModoSemAR();
    }

    private void IniciarModoSemAR()
    {
        if (arSession != null)
        {
            arSession.enabled = false;
        }

        SetARManagersActive(false);

        if (exibidorAR != null)
        {
            exibidorAR.enabled = false;
        }

        Camera camera = Camera.main;
        if (camera != null)
        {
            var background = camera.GetComponent<ARCameraBackground>();
            if (background != null)
            {
                background.enabled = false;
            }

            var poseDriver = camera.GetComponent<TrackedPoseDriver>();
            if (poseDriver != null)
            {
                poseDriver.enabled = false;
            }

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.19f, 0.19f, 0.19f);
        }

        GameObject prefab = exibidorAR != null ? exibidorAR.PrefabDoModelo : null;
        if (prefab == null || uiController == null || camera == null)
        {
            Debug.LogError("Modo 3D indisponivel: prefab, UIController ou camera nao encontrados.");
            return;
        }

        // Nesta cena o campo placedPrefab aponta para uma instancia ja ativa na
        // hierarquia. O visualizador cria sua propria instancia; ocultar a
        // original evita que os dois modelos recebam as mesmas animacoes.
        if (prefab.scene.IsValid())
        {
            prefab.SetActive(false);
        }

        Visualizador3D visualizador = gameObject.AddComponent<Visualizador3D>();
        visualizador.Configurar(prefab, uiController, camera);
        uiController.exibidor = visualizador;
        uiController.PularPopupInicial();

        Debug.Log("[ARInitializer] Modo 3D sem AR iniciado.");
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

    private void ShowARError()
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

        DadosMontagem dados = LocalizedDatabase.Load<DadosMontagem>(LocalizedDatabase.MontagemPath);

        SetText(arErrorTitle, FirstFilled(dados?.arErroTitulo, "Nao foi possivel iniciar a realidade aumentada"));
        SetText(arErrorMessage, FirstFilled(dados?.arErroPermissaoCamera, "Permita o acesso a camera nas configuracoes do sistema para usar a realidade aumentada."));
        SetText(arErrorBackButton, FirstFilled(dados?.arErroBotaoMenu, dados?.popupFinalBotaoMenu, "Voltar ao menu"));

        HideElement("PainelPopup");
        HideElement("PainelPopupFinal");
        HideElement("Tutorial");
        HideElement("GrupoMontagem");

        arErrorPanel.style.display = DisplayStyle.Flex;
        arErrorPanel.style.opacity = 1;
        arErrorPanel.BringToFront();
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
        SceneManager.LoadScene(Scenes.MainMenu);
    }

    private void SetARManagersActive(bool active)
    {
        if (arCameraManager != null)
        {
            arCameraManager.enabled = active;
        }

        if (arPlaneManager != null)
        {
            arPlaneManager.enabled = active;
        }

        if (arRaycastManager != null)
        {
            arRaycastManager.enabled = active;
        }
    }

    private void ResolveDependencies()
    {
        arSession = ResolveAnyActive(arSession, nameof(arSession));
        uiDocument = ResolveFirstActive(uiDocument, nameof(uiDocument));

        if (uiController == null && uiDocument != null)
        {
            uiController = uiDocument.GetComponent<UIController>();
        }

        uiController = ResolveFirstActive(uiController, nameof(uiController));
        exibidorAR = ResolveAnyActive(exibidorAR, nameof(exibidorAR));
        arCameraManager = ResolveAnyActive(arCameraManager, nameof(arCameraManager));
        arPlaneManager = ResolveAnyActive(arPlaneManager, nameof(arPlaneManager));
        arRaycastManager = ResolveAnyActive(arRaycastManager, nameof(arRaycastManager));
    }

    private T ResolveAnyActive<T>(T reference, string fieldName) where T : Object
    {
        if (reference != null)
        {
            return reference;
        }

        T fallback = FindAnyObjectByType<T>(FindObjectsInactive.Exclude);
        return ValidateFallback(fallback, fieldName);
    }

    private T ResolveFirstActive<T>(T reference, string fieldName) where T : Object
    {
        if (reference != null)
        {
            return reference;
        }

        T fallback = FindFirstObjectByType<T>(FindObjectsInactive.Exclude);
        return ValidateFallback(fallback, fieldName);
    }

    private T ValidateFallback<T>(T fallback, string fieldName) where T : Object
    {
        if (fallback != null)
        {
            Debug.LogWarning($"[ARInitializer] Referencia '{fieldName}' nao atribuida no Inspector; usando fallback ativo '{fallback.name}'.", this);
            return fallback;
        }

        Debug.LogError($"[ARInitializer] Referencia obrigatoria '{fieldName}' nao atribuida e nenhum objeto ativo compativel foi encontrado.", this);
        return null;
    }
}
