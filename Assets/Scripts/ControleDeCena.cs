using UnityEngine;

public class ControleDeCena : MonoBehaviour
{
    public static ControleDeCena Instance { get; private set; }

    public string origemDaCena { get; private set; } = "";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void DefinirOrigem(string origem)
    {
        origemDaCena = origem;
    }
} 
