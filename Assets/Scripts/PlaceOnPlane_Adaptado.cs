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
    private ARAnchorManager anchorManager;
    private ARPlaneManager planeManager;
    private ARAnchor ancoraAtual;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private bool objectPlaced = false;

    private float offsetAtual = 0f;
    private Pose lastHitPose;
    private bool hasHit = false;

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        anchorManager = GetComponent<ARAnchorManager>();
        planeManager = GetComponent<ARPlaneManager>();

        if (anchorManager == null)
            Debug.LogWarning("[PlaceOnPlane] Sem ARAnchorManager: o modelo vai flutuar conforme o tracking refina o plano.");

        if (arCamera == null) arCamera = Camera.main;
        if (uiController == null) Debug.LogError("[PlaceOnPlane] Referência 'UI Controller' não configurada.");
    }

    void Update()
    {
        Vector2 screenPosition;

#if UNITY_EDITOR
        if (Mouse.current == null) return;
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
                GameObject prefab = ResolverPrefabDoModelo();
                if (prefab == null)
                {
                    Debug.LogError("[PlaceOnPlane] Nenhum prefab configurado para o cenário selecionado.");
                    return;
                }

                spawnedObject = Instantiate(prefab, posicaoFinal, lastHitPose.rotation);
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
                AncorarNoPlano(hits[0].trackableId);
                SetARPlanesActive(false);
            }
            else
            {
                AncorarNoPlano(hits[0].trackableId);
            }

            PosicionarConformeAOrigem();
        }
    }

    protected override void AjustarPosicaoParaPasso(bool isMontagem)
    {
        offsetAtual = isMontagem ? alturaMontagem : alturaProblemas;

        if (hasHit) PosicionarConformeAOrigem();
    }

    private void AncorarNoPlano(TrackableId planoId)
    {
        if (ancoraAtual != null)
        {
            if (anchorManager != null) anchorManager.TryRemoveAnchor(ancoraAtual);
            ancoraAtual = null;
        }

        if (spawnedObject == null) return;
        if (anchorManager == null || !anchorManager.enabled || anchorManager.subsystem == null) return;
        if (planeManager == null) return;

        ARPlane plano = planeManager.GetPlane(planoId);
        if (plano == null) return;

        ancoraAtual = anchorManager.AttachAnchor(plano, lastHitPose);

        if (ancoraAtual != null)
        {
            spawnedObject.transform.SetParent(ancoraAtual.transform, false);
        }
    }

    private void PosicionarConformeAOrigem()
    {
        if (spawnedObject == null) return;

        if (ancoraAtual != null)
        {
            spawnedObject.transform.localPosition = new Vector3(0f, offsetAtual, 0f);
            spawnedObject.transform.localRotation = Quaternion.identity;
            return;
        }

        spawnedObject.transform.SetPositionAndRotation(
            lastHitPose.position + new Vector3(0f, offsetAtual, 0f), lastHitPose.rotation);
    }

    private void SetARPlanesActive(bool isActive)
    {
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
