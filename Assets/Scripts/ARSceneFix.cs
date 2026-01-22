using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;

public class ARSceneFix : MonoBehaviour
{
    IEnumerator Start()
    {
        // Força a reinicialização do sistema de Loader XR
        if (LoaderUtility.GetActiveLoader() == null)
        {
            Debug.Log("Reiniciando Loader XR...");
            yield return LoaderUtility.Initialize();
        }
    }
}