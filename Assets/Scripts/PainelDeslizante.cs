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

    // O método Start() foi REMOVIDO propositalmente aqui para evitar 
    // que o painel se feche sozinho no primeiro clique!

    public void Abrir()
    {
        if (rt == null) rt = GetComponent<RectTransform>();

        // Garante que as âncoras ocupem a tela toda (evita conflitos de Layout)
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);

        // Se estiver a abrir, garante que ele começa o movimento de baixo
        if (!aberto)
        {
            rt.sizeDelta = Vector2.zero; 
            rt.anchoredPosition = new Vector2(0, -ObterAlturaDeslocamento());
        }

        // Liga o painel ANTES da animação para garantir que os textos atualizem
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
        // Inicia a animação descendo para fora da tela
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
        
        // Joga o painel instantaneamente para fora da tela
        rt.anchoredPosition = new Vector2(0, -ObterAlturaDeslocamento());
        
        aberto = false;
        if (botaoFechar != null) botaoFechar.SetActive(false);
        gameObject.SetActive(false); // Oculta completamente
    }

    private float ObterAlturaDeslocamento()
    {
        // Pega a altura real do painel para garantir que ele desce o suficiente
        float altura = rt.rect.height;
        
        if (altura <= 0)
        {
            RectTransform parentRT = rt.parent as RectTransform;
            altura = parentRT != null ? parentRT.rect.height : Screen.height;
        }
        
        // Margem de segurança caso o layout demore 1 frame a calcular
        return altura > 0 ? altura : 2000f; 
    }

    // Corrotina que mexe na POSIÇÃO (deslize real) e não nas Âncoras
    private IEnumerator AnimarPosicao(bool indoAberto)
    {
        float tempo = 0f;
        float altura = ObterAlturaDeslocamento();
        
        // Posição Aberta = Y no 0 (Centro da tela)
        // Posição Fechada = Y negativo igual à altura (Escondido lá embaixo)
        Vector2 posFechada = new Vector2(0, -altura);
        Vector2 posAberta = Vector2.zero;

        Vector2 posInicial = rt.anchoredPosition;
        Vector2 posDestino = indoAberto ? posAberta : posFechada;

        while (tempo < duracaoAnimacao)
        {
            float t = tempo / duracaoAnimacao;
            t = t * t * (3f - 2f * t); // Efeito de suavização (SmoothStep)

            // Move o painel fisicamente para cima ou para baixo
            rt.anchoredPosition = Vector2.Lerp(posInicial, posDestino, t);

            tempo += Time.deltaTime;
            yield return null;
        }

        rt.anchoredPosition = posDestino;
        
        // Só desativa o GameObject quando a animação de descer terminar por completo!
        if (!indoAberto)
        {
            gameObject.SetActive(false);
        }
    }
}