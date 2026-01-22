using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PlaneColorChanger : MonoBehaviour
{
    // O código foi temporariamente desativado para permitir a compilação na Unity 6.
    // Isso NÃO afeta a detecção de planos ou a câmera AR, apenas a mudança de cor visual.
    
    /* 
    private ARPlaneManager planeManager;
    void Awake() { planeManager = GetComponent<ARPlaneManager>(); }
    void OnEnable() { if (planeManager != null) planeManager.trackablesChanged += OnTrackablesChanged; }
    void OnDisable() { if (planeManager != null) planeManager.trackablesChanged -= OnTrackablesChanged; }
    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARPlane> eventArgs) { }
    */
}
