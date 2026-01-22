using UnityEngine;

public class IniciarTela : MonoBehaviour
{
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.fullScreen = true;
        Screen.brightness = 1.0f;
    }
}
