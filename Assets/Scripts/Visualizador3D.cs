using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Modo de exibição sem AR: instancia o modelo M4 na origem, diante de uma
/// câmera dedicada que orbita ao redor do modelo (1 dedo arrasta = órbita,
/// pinça = zoom; no editor: mouse arrasta = órbita, scroll = zoom).
/// Usa a mesma UI/fluxo de passos do modo AR via ExibidorDeModeloBase.
/// </summary>
public class Visualizador3D : ExibidorDeModeloBase
{
    [Header("Câmera e Enquadramento")]
    [SerializeField] private Camera cameraViewer;
    [SerializeField] private float distanciaInicial = 2.5f;
    [SerializeField] private float distanciaMinima = 0.8f;
    [SerializeField] private float distanciaMaxima = 6f;
    [SerializeField] private float pitchInicial = 5f;

    [Header("Gestos")]
    [Tooltip("Graus de rotação por pixel arrastado")]
    [SerializeField] private float velocidadeOrbita = 0.25f;
    [Tooltip("Metros de zoom por pixel de variação da pinça")]
    [SerializeField] private float velocidadeZoomPinca = 0.005f;
    [Tooltip("Metros de zoom por 'clique' de scroll no editor")]
    [SerializeField] private float velocidadeZoomScroll = 0.3f;

    private Vector3 pivo;
    private float yaw;
    private float pitch;
    private float distancia;

    private bool arrastando;
    private Vector2 ultimaPosicaoArrasto;
    private bool pincando;
    private float ultimaDistanciaPinca;
    private Coroutine recentralizacaoPendente;

    public void Configurar(GameObject prefab, UIController ui, Camera cam)
    {
        placedPrefab = prefab;
        uiController = ui;
        cameraViewer = cam;
    }

    void Start()
    {
        if (cameraViewer == null) cameraViewer = Camera.main;
        if (uiController == null) Debug.LogError("[Visualizador3D] O 'UI Controller' não foi arrastado no Inspector!");

        if (placedPrefab == null)
        {
            Debug.LogError("[Visualizador3D] 'Placed Prefab' não configurado.");
            enabled = false;
            return;
        }

        pitch = pitchInicial;
        distancia = distanciaInicial;

        spawnedObject = Instantiate(placedPrefab, Vector3.zero, Quaternion.identity);
        spawnedObject.SetActive(true);
        ConfigurarModeloInstanciado();

        pivo = CalcularCentroDoModelo();
        AtualizarCamera();

        StartCoroutine(IniciarPassosNoProximoFrame());
    }

    // Espera um frame para garantir que uiController.exibidor já foi atribuído.
    private IEnumerator IniciarPassosNoProximoFrame()
    {
        yield return null;
        if (uiController != null) uiController.IniciarPassos();

        yield return null;
        pivo = CalcularCentroDoModelo();
        AtualizarCamera();
    }

    protected override void AjustarPosicaoParaPasso(bool isMontagem)
    {
        if (recentralizacaoPendente != null)
        {
            StopCoroutine(recentralizacaoPendente);
        }

        recentralizacaoPendente = StartCoroutine(RecentrarAposAnimacao());
    }

    private IEnumerator RecentrarAposAnimacao()
    {
        // Aguarda o Animator aplicar o estado do passo antes de medir os bounds.
        yield return new WaitForEndOfFrame();

        pivo = CalcularCentroDoModelo();
        AtualizarCamera();
        recentralizacaoPendente = null;
    }

    // Partes do template ficam "estacionadas" a ~1e13 do modelo (truque da cena
    // para escondê-lo) até a primeira animação reposicioná-las; ignorá-las aqui.
    private const float distanciaMaximaDoCentro = 100f;

    private Vector3 CalcularCentroDoModelo()
    {
        Vector3 referencia = spawnedObject.transform.position;
        Bounds bounds = new Bounds(referencia, Vector3.zero);
        bool encontrou = false;

        foreach (var r in spawnedObject.GetComponentsInChildren<Renderer>())
        {
            if (Vector3.Distance(r.bounds.center, referencia) > distanciaMaximaDoCentro) continue;

            if (encontrou)
            {
                bounds.Encapsulate(r.bounds);
            }
            else
            {
                bounds = r.bounds;
                encontrou = true;
            }
        }

        return encontrou ? bounds.center : referencia;
    }

    void Update()
    {
#if UNITY_EDITOR
        AtualizarGestosEditor();
#else
        AtualizarGestosToque();
#endif
    }

#if UNITY_EDITOR
    private void AtualizarGestosEditor()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                arrastando = false;
            }
            else
            {
                arrastando = true;
                ultimaPosicaoArrasto = Mouse.current.position.ReadValue();
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame) arrastando = false;

        if (arrastando && Mouse.current.leftButton.isPressed)
        {
            Vector2 atual = Mouse.current.position.ReadValue();
            AplicarOrbita(atual - ultimaPosicaoArrasto);
            ultimaPosicaoArrasto = atual;
        }

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f &&
            (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
        {
            AplicarZoom(-Mathf.Sign(scroll) * velocidadeZoomScroll);
        }
    }
#else
    private void AtualizarGestosToque()
    {
        if (Touchscreen.current == null) return;

        TouchControl primeiro = null;
        TouchControl segundo = null;
        foreach (var toque in Touchscreen.current.touches)
        {
            if (!toque.press.isPressed) continue;
            if (primeiro == null) primeiro = toque;
            else { segundo = toque; break; }
        }

        if (primeiro != null && segundo == null)
        {
            Vector2 posicao = primeiro.position.ReadValue();

            if (pincando)
            {
                // Fim da pinça com um dedo ainda na tela: retoma a órbita a partir
                // daqui, sem exigir um novo toque.
                pincando = false;
                arrastando = true;
                ultimaPosicaoArrasto = posicao;
            }
            else if (primeiro.press.wasPressedThisFrame)
            {
                bool sobreUI = EventSystem.current != null &&
                               EventSystem.current.IsPointerOverGameObject(primeiro.touchId.ReadValue());
                arrastando = !sobreUI;
                ultimaPosicaoArrasto = posicao;
            }
            else if (arrastando)
            {
                AplicarOrbita(posicao - ultimaPosicaoArrasto);
                ultimaPosicaoArrasto = posicao;
            }
        }
        else if (primeiro != null && segundo != null)
        {
            arrastando = false;

            if (!pincando && EventSystem.current != null &&
                (EventSystem.current.IsPointerOverGameObject(primeiro.touchId.ReadValue()) ||
                 EventSystem.current.IsPointerOverGameObject(segundo.touchId.ReadValue())))
            {
                return; // pinça começando sobre a UI: não inicia o zoom
            }

            float distanciaAtualPinca = Vector2.Distance(primeiro.position.ReadValue(), segundo.position.ReadValue());

            if (pincando)
            {
                AplicarZoom((ultimaDistanciaPinca - distanciaAtualPinca) * velocidadeZoomPinca);
            }

            pincando = true;
            ultimaDistanciaPinca = distanciaAtualPinca;
        }
        else
        {
            arrastando = false;
            pincando = false;
        }
    }
#endif

    private void AplicarOrbita(Vector2 delta)
    {
        yaw += delta.x * velocidadeOrbita;
        pitch = Mathf.Clamp(pitch - delta.y * velocidadeOrbita, -80f, 80f);
        AtualizarCamera();
    }

    private void AplicarZoom(float deltaDistancia)
    {
        distancia = Mathf.Clamp(distancia + deltaDistancia, distanciaMinima, distanciaMaxima);
        AtualizarCamera();
    }

    private void AtualizarCamera()
    {
        if (cameraViewer == null) return;
        Quaternion rotacao = Quaternion.Euler(pitch, yaw, 0f);
        cameraViewer.transform.SetPositionAndRotation(pivo - rotacao * Vector3.forward * distancia, rotacao);
    }
}
