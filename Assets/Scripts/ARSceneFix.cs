using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;

public class ARSceneFix : MonoBehaviour
{
    IEnumerator Start()
    {
        if (LoaderUtility.GetActiveLoader() == null)
        {
            Debug.Log("Reiniciando Loader XR...");
            yield return LoaderUtility.Initialize();
        }
    }
}
