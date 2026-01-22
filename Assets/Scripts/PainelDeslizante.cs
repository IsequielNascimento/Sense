using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PainelDeslizante : MonoBehaviour
{
    private RectTransform rt;

    [Header("Configurações")]
    public float duracaoAnimacao = 0.4f;
    public GameObject botaoFechar;

    private bool aberto = false;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        // Garante que comece totalmente fechado (Altura 0%)
        FecharInstantaneamente();
    }

    public void Abrir()
    {
        if (rt == null) rt = GetComponent<RectTransform>();

        // 1. Configura a base para ficar fixa no chão
        // Min = (0,0) -> Canto inferior esquerdo fixo
        // Max.x = 1 -> Largura total
        rt.anchorMin = new Vector2(0, 0); 
        
        // Zera margens para que as âncoras controlem tudo
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        StopAllCoroutines();
        // Anima a âncora do topo de onde ela está até 1 (100% da altura)
        StartCoroutine(AnimarAncoras(1f));
        
        aberto = true;
        if (botaoFechar != null) botaoFechar.SetActive(true);
    }

    public void Fechar()
    {
        if (rt == null) rt = GetComponent<RectTransform>();

        StopAllCoroutines();
        // Anima a âncora do topo até 0 (0% da altura)
        StartCoroutine(AnimarAncoras(0f));
        
        aberto = false;
        if (botaoFechar != null) botaoFechar.SetActive(false);
    }

    public void FecharInstantaneamente()
    {
        if (rt == null) rt = GetComponent<RectTransform>();
        
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0); // Topo colado no chão (0%)
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        aberto = false;
        if (botaoFechar != null) botaoFechar.SetActive(false);
    }

    // Corrotina que mexe na ÂNCORA (0 a 1) e não em PIXELS
    private IEnumerator AnimarAncoras(float topoDestino)
    {
        float tempo = 0f;
        float topoInicial = rt.anchorMax.y; // Pega a posição atual da porcentagem (ex: 1 se aberto, 0.5 se metade)

        while (tempo < duracaoAnimacao)
        {
            float t = tempo / duracaoAnimacao;
            t = t * t * (3f - 2f * t); // SmoothStep

            // Interpola o valor Y da âncora Max
            float novoTopo = Mathf.Lerp(topoInicial, topoDestino, t);
            
            rt.anchorMax = new Vector2(1, novoTopo);
            
            // É crucial zerar o offset a cada frame para que a âncora mande no tamanho
            rt.offsetMin = Vector2.zero; 
            rt.offsetMax = Vector2.zero;

            tempo += Time.deltaTime;
            yield return null;
        }

        // Valor final exato
        rt.anchorMax = new Vector2(1, topoDestino);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}