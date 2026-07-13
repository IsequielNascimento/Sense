using UnityEngine;

public enum OrigemCena
{
    Montagem,
    Problema,
    Gemeo
}

public class ControleDeCena : MonoBehaviour
{
    public static ControleDeCena Instance { get; private set; }

    public OrigemCena OrigemDaCena { get; private set; } = OrigemCena.Montagem;

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

    public void DefinirOrigem(OrigemCena origem)
    {
        OrigemDaCena = origem;
    }
} 
