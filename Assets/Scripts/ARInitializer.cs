using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;

public class ARInitializer : MonoBehaviour
{
    // O ARSession que será inicializado.
    // Arraste o componente ARSession da sua cena para este campo no Inspector.
    public ARSession arSession;

    void Start()
    {
        // Se você não arrastar o ARSession, tentamos encontrá-lo na cena.
        if (arSession == null)
        {
            arSession = FindObjectOfType<ARSession>();
        }

        if (arSession != null)
        {
            // 1. Desativa o ARSession para garantir que a inicialização não seja automática.
            arSession.enabled = false;
            
            // 2. Inicia a rotina de inicialização manual.
            StartCoroutine(InitializeARSessionManually());
        }
        else
        {
            Debug.LogError("ARSession não encontrado na cena. Verifique se o componente está presente e se foi arrastado para o script ARInitializer no Inspector.");
        }
    }

    private IEnumerator InitializeARSessionManually()
    {
        Debug.Log("Iniciando verificação de suporte ao AR...");

        // Espera até que o estado do ARSession seja conhecido.
        // O estado inicial é geralmente 'None' ou 'CheckingAvailability'.
        while (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
        {
            // O Unity precisa de um quadro para iniciar a verificação de disponibilidade.
            yield return null;
        }

        // Verifica o estado final de disponibilidade.
        if (ARSession.state == ARSessionState.Unsupported)
        {
            Debug.LogError("Dispositivo não suporta AR. Estado: " + ARSession.state);
            // Implemente aqui a lógica para mostrar uma mensagem de erro ao usuário.
            yield break;
        }

        // Se o suporte for encontrado, ativamos o ARSession.
        if (ARSession.state == ARSessionState.Ready)
        {
            Debug.Log("Suporte AR encontrado. Ativando ARSession.");
            // 3. Ativa o ARSession para iniciar a sessão de AR.
            arSession.enabled = true;
            
            // 4. Espera até que a sessão esteja rastreando (tracking).
            while (ARSession.state != ARSessionState.SessionTracking)
            {
                yield return null;
            }

            Debug.Log("ARSession inicializado e rastreando com sucesso!");
            // 5. Opcional: Ativar outros managers de AR (como ARPlaneManager, ARRaycastManager)
            // que estavam desativados no editor.
            SetARManagersActive(true);
        }
        else
        {
            Debug.LogError("Falha na inicialização do ARSession. Estado: " + ARSession.state);
        }
    }

    private void SetARManagersActive(bool active)
    {
        // Este é um método auxiliar para ativar/desativar outros componentes de AR.
        // Certifique-se de que estes componentes estejam DESATIVADOS (enabled=false) no editor.

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
        
        // Adicione outros managers que você usa (e que estão desativados inicialmente)
        // Exemplo:
        // var arRaycastManager = FindObjectOfType<ARRaycastManager>();
        // if (arRaycastManager != null)
        // {
        //     arRaycastManager.enabled = active;
        // }
    }
}
