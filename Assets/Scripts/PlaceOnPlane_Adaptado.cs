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

    [Header("Ajuste Automático de Altura")]
    [Tooltip("Altura extra para a Montagem Padrão (Deixe 0 se já estiver correto)")]
    [SerializeField] private float alturaMontagem = 0f;
    [Tooltip("Altura para os Problemas. Como a peça está 1 metro acima, use -1 para colar no chão.")]
    [SerializeField] private float alturaProblemas = -1f;

    [Header("Conexão com UI Toolkit")]
    [SerializeField] private UIController uiController; 

    private ARRaycastManager raycastManager;
    private GameObject spawnedObject;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    
    private Animator[] animators; 
    private GerenciadorVisual gerenciadorVisual;
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

            // Calcula a posição usando o offset que estiver ativo no momento
            Vector3 posicaoFinal = lastHitPose.position + new Vector3(0, offsetAtual, 0);

            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(placedPrefab, posicaoFinal, lastHitPose.rotation);
                spawnedObject.SetActive(true); 

                animators = spawnedObject.GetComponentsInChildren<Animator>();
                gerenciadorVisual = spawnedObject.GetComponentInChildren<GerenciadorVisual>();

                if (animators == null || animators.Length == 0)
                {
                    Debug.LogError("[PlaceOnPlane] CRÍTICO: Nenhum Animator encontrado no prefab instanciado!");
                    return;
                }

                foreach (var anim in animators)
                {
                    anim.Rebind();
                    anim.Update(0f); 
                }

                if (uiController != null && !objectPlaced)
                {
                    // IniciarPassos vai chamar o PlayAnimation, que ajustará a altura correta imediatamente!
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

    public void PlayAnimation(string animName, string camadaAlvo, string telaDisplay, string vfx)
    {
        // NOVO: Define se o objeto sobe ou desce dependendo de qual menu o usuário abriu!
        if (string.IsNullOrEmpty(camadaAlvo) || camadaAlvo == "Base Layer")
        {
            offsetAtual = alturaMontagem;
        }
        else
        {
            offsetAtual = alturaProblemas;
        }

        // Se o objeto já estiver no cenário, atualiza a altura dele na mesma hora
        if (spawnedObject != null && hasHit)
        {
            spawnedObject.transform.position = lastHitPose.position + new Vector3(0, offsetAtual, 0);
        }

        // 1. LÓGICA DE ANIMAÇÃO
        if (animators != null && animators.Length > 0)
        {
            if (string.IsNullOrEmpty(camadaAlvo)) camadaAlvo = "Base Layer";
            int hashDaAnimacao = Animator.StringToHash(animName);
            bool tocouEmPeloMenosUm = false;

            foreach (var anim in animators)
            {
                int layerIndex = anim.GetLayerIndex(camadaAlvo);
                if (layerIndex != -1 && anim.HasState(layerIndex, hashDaAnimacao))
                {
                    anim.speed = 1f;

                    for (int i = 1; i < anim.layerCount; i++)
                    {
                        anim.SetLayerWeight(i, (i == layerIndex) ? 1f : 0f);
                    }

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
                Debug.LogError($"[AR Toolkit] ESTADO NÃO ENCONTRADO: O estado '{animName}' NÃO EXISTE na camada '{camadaAlvo}'.");
            }
        }

        // 2. LÓGICA VISUAL
        if (gerenciadorVisual != null)
        {
            gerenciadorVisual.MudarSpriteDoSensor(telaDisplay);
            gerenciadorVisual.AtivarEfeito(vfx);
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