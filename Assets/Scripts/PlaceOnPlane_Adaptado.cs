using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnPlane_Adaptado : ExibidorDeModeloBase
{
    [Header("AR Placement")]
    [SerializeField] private Camera arCamera;

    [Header("Ajuste Automático de Altura")]
    [Tooltip("Altura extra para a Montagem Padrão (Deixe 0 se já estiver correto)")]
    [SerializeField] private float alturaMontagem = 0f;
    [Tooltip("Altura para os Problemas. Como a peça está 1 metro acima, use -1 para colar no chão.")]
    [SerializeField] private float alturaProblemas = -1f;

    private ARRaycastManager raycastManager;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private bool objectPlaced = false;

    // Variáveis de controle de posição
    private float offsetAtual = 0f;
    private Pose lastHitPose;
    private bool hasHit = false;

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        if (arCamera == null) arCamera = Camera.main;
        if (uiController == null) Debug.LogError("[PlaceOnPlane] O 'UI Controller' não foi arrastado no Inspector!");
    }

    void Update()
    {
        Vector2 screenPosition;

#if UNITY_EDITOR
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        screenPosition = Mouse.current.position.ReadValue();
#else
        if (Touchscreen.current == null || Touchscreen.current.touches.Count == 0) return;
        var touch = Touchscreen.current.touches[0];
        if (!touch.press.wasPressedThisFrame) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.touchId.ReadValue())) return;
        screenPosition = touch.position.ReadValue();
#endif

        if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            lastHitPose = hits[0].pose;
            hasHit = true;

            Vector3 posicaoFinal = lastHitPose.position + new Vector3(0, offsetAtual, 0);

            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(placedPrefab, posicaoFinal, lastHitPose.rotation);
                spawnedObject.SetActive(true);

                ConfigurarModeloInstanciado();

                if (animators == null || animators.Length == 0)
                {
                    return;
                }

                if (uiController != null && !objectPlaced)
                {
                    uiController.IniciarPassos();
                }

                objectPlaced = true;
                SetARPlanesActive(false);
            }
            else
            {
                spawnedObject.transform.SetPositionAndRotation(posicaoFinal, lastHitPose.rotation);
            }
        }
    }

    protected override void AjustarPosicaoParaPasso(bool isMontagem)
    {
        offsetAtual = isMontagem ? alturaMontagem : alturaProblemas;

        if (spawnedObject != null && hasHit)
        {
            spawnedObject.transform.position = lastHitPose.position + new Vector3(0, offsetAtual, 0);
        }
    }

    private void SetARPlanesActive(bool isActive)
    {
        ARPlaneManager planeManager = GetComponent<ARPlaneManager>();
        if (planeManager != null)
        {
            planeManager.enabled = isActive;
            foreach (ARPlane plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(isActive);
            }
        }
    }
}
