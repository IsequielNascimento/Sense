using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

// Coloque-o no XR Origin.
[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnPlane_Adaptado : MonoBehaviour
{
    [Header("AR Placement")]
    [SerializeField] private GameObject placedPrefab;
    [SerializeField] private Camera arCamera;

    [Header("Conexão com UI Toolkit")]
    [Tooltip("Arraste aqui o GameObject 'Controlador_UI' que tem o UIController.cs")]
    [SerializeField]
    private UIController uiController; 

    // --- Variáveis privadas ---
    private ARRaycastManager raycastManager;
    private GameObject spawnedObject;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private Animator animator;
    private bool objectPlaced = false;

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();

        if (arCamera == null)
            arCamera = Camera.main;

        if (uiController == null)
            Debug.LogError("[PlaceOnPlane] O 'Ui Controller' não foi arrastado no Inspector!");
    }

    void Update()
    {
        Vector2 screenPosition;

        // --- Lógica de Toque/Mouse ---
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
            Pose hitPose = hits[0].pose;

            if (spawnedObject == null)
            {
                // --- PRIMEIRA COLOCAÇÃO DO OBJETO ---
                spawnedObject = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
                spawnedObject.SetActive(true); 

                animator = spawnedObject.GetComponentInChildren<Animator>();
                if (animator == null)
                {
                    Debug.LogError("CRÍTICO: Animator não encontrado no prefab instanciado!");
                    return;
                }

                animator.Rebind();
                animator.Update(0f);
                Debug.Log("[PlaceOnPlane] Prefab AR colocado e Animator reinicializado.");

                // --- ADAPTAÇÃO ---
                if (uiController != null && !objectPlaced)
                {
                    Debug.Log("[PlaceOnPlane] Objeto colocado. Chamando uiController.IniciarPassos().");
                    uiController.IniciarPassos(); // Inicia a UI de Montagem
                }
                
                objectPlaced = true; 
                SetARPlanesActive(false);
            }
            else
            {
                // --- REPOSICIONAMENTO DO OBJETO ---
                spawnedObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            }
        }
    }

    // --- FUNÇÃO DE ANIMAÇÃO (será chamada pelo UIController) ---
    public void PlayAnimation(string animName)
    {
        if (animator == null)
        {
            Debug.LogWarning("[PlaceOnPlane] Animator não está disponível para tocar a animação.");
            return;
        }

        string camadaAlvo = "Base Layer";
        int layerIndex = animator.GetLayerIndex(camadaAlvo);
        if (layerIndex == -1)
        {
            Debug.LogError($"[PlaceOnPlane] Layer '{camadaAlvo}' não existe no Animator Controller.");
            return;
        }

        for (int i = 0; i < animator.layerCount; i++)
            animator.SetLayerWeight(i, (i == layerIndex) ? 1f : 0f);

        Debug.Log($"[PlaceOnPlane] Tocando animação '{animName}' no layer '{camadaAlvo}'");
        animator.Play(animName, layerIndex, 0f);
        animator.Update(Time.deltaTime);
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