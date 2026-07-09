using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;


public class SeletorDeModo : MonoBehaviour
{
    public static bool ModoVisualizadorAtivo { get; private set; }

    [Header("Objetos exclusivos de cada modo (começam DESATIVADOS na cena)")]
    [SerializeField] private GameObject[] objetosModoAR;
    [SerializeField] private GameObject[] objetosModoVisualizador;

    [Header("Exibidor de cada modo")]
    [SerializeField] private PlaceOnPlane_Adaptado exibidorAR;
    [SerializeField] private Visualizador3D exibidorVisualizador;

    [Header("UI")]
    [SerializeField] private UIController uiController;

    [Tooltip("Tempo máximo (s) aguardando o resultado do ARSession.CheckAvailability")]
    [SerializeField] private float timeoutSegundos = 3f;

    IEnumerator Start()
    {
        yield return null;

        float inicio = Time.realtimeSinceStartup;
        StartCoroutine(ARSession.CheckAvailability());

        while ((ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
               && Time.realtimeSinceStartup - inicio < timeoutSegundos)
        {
            yield return null;
        }

        bool arDisponivel = ARSession.state == ARSessionState.Ready
            || ARSession.state == ARSessionState.SessionInitializing
            || ARSession.state == ARSessionState.SessionTracking;

        ModoVisualizadorAtivo = !arDisponivel;
        Debug.Log($"[SeletorDeModo] Modo {(arDisponivel ? "AR" : "Visualizador 3D")} ativado " +
                  $"(ARSession.state = {ARSession.state}, check em {Time.realtimeSinceStartup - inicio:F2}s).");

        if (arDisponivel)
        {
            AtivarObjetos(objetosModoAR);
            if (uiController != null)
            {
                uiController.exibidor = exibidorAR;
                uiController.MostrarPopupInicial();
            }
        }
        else
        {
            AtivarObjetos(objetosModoVisualizador);
            if (uiController != null)
            {
                uiController.exibidor = exibidorVisualizador;
                uiController.PularPopupInicial();
            }
        }
    }

    private void AtivarObjetos(GameObject[] objetos)
    {
        if (objetos == null) return;
        foreach (var go in objetos)
        {
            if (go != null) go.SetActive(true);
        }
    }
}
