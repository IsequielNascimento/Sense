using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnPlane_Adaptado : MonoBehaviour
{
    [Header("AR Placement")]
    [SerializeField] private GameObject placedPrefab;
    [SerializeField] private Camera arCamera;

    [Header("Conexão com UI Toolkit")]
    [SerializeField] private UIController uiController; 

    private ARRaycastManager raycastManager;
    private GameObject spawnedObject;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    
    // CORREÇÃO CRÍTICA: Agora usamos um Array para capturar TODOS os Animators do Prefab (Pai e Filhos)
    private Animator[] animators; 
    
    private GerenciadorVisual gerenciadorVisual;
    private bool objectPlaced = false;

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
            Pose hitPose = hits[0].pose;

            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
                spawnedObject.SetActive(true); 

                // CAPTURA TODOS OS ANIMATORS (O do 'Animação APK' e o do 'M4_Smart_Final_Animado')
                animators = spawnedObject.GetComponentsInChildren<Animator>();
                gerenciadorVisual = spawnedObject.GetComponentInChildren<GerenciadorVisual>();

                if (animators == null || animators.Length == 0)
                {
                    Debug.LogError("[PlaceOnPlane] CRÍTICO: Nenhum Animator encontrado no prefab instanciado!");
                    return;
                }

                // Reinicia todos os Animators encontrados para garantir um estado limpo
                foreach (var anim in animators)
                {
                    anim.Rebind();
                    anim.Update(0f); 
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
                spawnedObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            }
        }
    }

    public void PlayAnimation(string animName, string camadaAlvo, string telaDisplay, string vfx)
    {
        if (animators != null && animators.Length > 0)
        {
            if (string.IsNullOrEmpty(camadaAlvo)) camadaAlvo = "Base Layer";
            int hashDaAnimacao = Animator.StringToHash(animName);
            bool tocouEmPeloMenosUm = false;

            // DISPARA A ANIMAÇÃO EM TODOS OS ANIMATORS QUE A POSSUÍREM
            foreach (var anim in animators)
            {
                int layerIndex = anim.GetLayerIndex(camadaAlvo);
                if (layerIndex != -1 && anim.HasState(layerIndex, hashDaAnimacao))
                {
                    anim.speed = 1f;

                    // Ajusta o peso das camadas apenas no Animator atual do loop
                    for (int i = 1; i < anim.layerCount; i++)
                    {
                        anim.SetLayerWeight(i, (i == layerIndex) ? 1f : 0f);
                    }

                    // Dá o Play!
                    anim.Play(hashDaAnimacao, layerIndex, 0f);
                    tocouEmPeloMenosUm = true;
                }
            }

            if (tocouEmPeloMenosUm)
            {
                Debug.Log($"[AR Toolkit] Sucesso: Animação '{animName}' tocada na camada '{camadaAlvo}'.");
            }
            else
            {
                Debug.LogError($"[AR Toolkit] ESTADO NÃO ENCONTRADO: O estado '{animName}' NÃO EXISTE na camada '{camadaAlvo}' em nenhum dos Animators do Prefab.");
            }
        }

        // Lógica do Visual (Display / Efeitos)
        if (gerenciadorVisual != null)
        {
            // Chamadas para o seu script GerenciadorVisual (Descomente e ajuste os nomes das funções)
            // gerenciadorVisual.MudarSpriteDoSensor(telaDisplay);
            // gerenciadorVisual.AtivarEfeito(vfx);
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