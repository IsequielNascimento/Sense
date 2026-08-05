using UnityEngine;
using System.Collections;

public class PainelDeslizante : MonoBehaviour
{
    private RectTransform rt;

    [Header("Configurações")]
    public float duracaoAnimacao = 0.3f;
    public GameObject botaoFechar;

    private bool aberto = false;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public void Abrir()
    {
        if (rt == null) rt = GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);

        if (!aberto)
        {
            rt.sizeDelta = Vector2.zero; 
            rt.anchoredPosition = new Vector2(0, -ObterAlturaDeslocamento());
        }

        gameObject.SetActive(true); 
        
        StopAllCoroutines();
        StartCoroutine(AnimarPosicao(true));
        
        aberto = true;
        if (botaoFechar != null) botaoFechar.SetActive(true);
    }

    public void Fechar()
    {
        if (!aberto) return;
        
        StopAllCoroutines();
        StartCoroutine(AnimarPosicao(false));
        
        aberto = false;
        if (botaoFechar != null) botaoFechar.SetActive(false);
    }

    public void FecharInstantaneamente()
    {
        if (rt == null) rt = GetComponent<RectTransform>();
        
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.sizeDelta = Vector2.zero;
        
        rt.anchoredPosition = new Vector2(0, -ObterAlturaDeslocamento());
        
        aberto = false;
        if (botaoFechar != null) botaoFechar.SetActive(false);
        gameObject.SetActive(false);
    }

    private float ObterAlturaDeslocamento()
    {
        float altura = rt.rect.height;
        
        if (altura <= 0)
        {
            RectTransform parentRT = rt.parent as RectTransform;
            altura = parentRT != null ? parentRT.rect.height : Screen.height;
        }
        
        return altura > 0 ? altura : 2000f; 
    }

    private IEnumerator AnimarPosicao(bool indoAberto)
    {
        float tempo = 0f;
        float altura = ObterAlturaDeslocamento();
        
        Vector2 posFechada = new Vector2(0, -altura);
        Vector2 posAberta = Vector2.zero;

        Vector2 posInicial = rt.anchoredPosition;
        Vector2 posDestino = indoAberto ? posAberta : posFechada;

        while (tempo < duracaoAnimacao)
        {
            float t = tempo / duracaoAnimacao;
            t = t * t * (3f - 2f * t);

            rt.anchoredPosition = Vector2.Lerp(posInicial, posDestino, t);

            tempo += Time.deltaTime;
            yield return null;
        }

        rt.anchoredPosition = posDestino;
        
        if (!indoAberto)
        {
            gameObject.SetActive(false);
        }
    }
}
