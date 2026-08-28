// Rodar após mexer nas cenas de AR: garante o ARAnchorManager que PlaceOnPlane_Adaptado exige.
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public static class GarantirAncoraNasCenasDeAr
{
    #region MARK - Contrato

    public static readonly string[] CenasDeAr =
    {
        "Assets/Scenes/AR_Cena_UIToolkit.unity",
    };

    #endregion

    #region MARK - Execução

    [MenuItem("Sense/AR/Garantir ARAnchorManager nas cenas")]
    public static void Garantir()
    {
        foreach (string caminho in CenasDeAr)
        {
            Scene cena = EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);

            PlaceOnPlane_Adaptado colocador = Object.FindObjectsByType<PlaceOnPlane_Adaptado>(
                FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();

            if (colocador == null)
            {
                Debug.LogError($"[AncoraAR] {caminho}: nenhum PlaceOnPlane_Adaptado na cena.");
                continue;
            }

            if (colocador.GetComponent<ARAnchorManager>() != null)
            {
                Debug.Log($"[AncoraAR] {caminho}: ARAnchorManager ja presente em '{colocador.name}'.");
                continue;
            }

            colocador.gameObject.AddComponent<ARAnchorManager>();
            EditorSceneManager.MarkSceneDirty(cena);
            EditorSceneManager.SaveScene(cena);
            Debug.Log($"[AncoraAR] {caminho}: ARAnchorManager adicionado em '{colocador.name}'.");
        }
    }

    #endregion
}
