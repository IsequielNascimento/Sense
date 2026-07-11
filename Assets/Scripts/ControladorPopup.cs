using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ControladorPopup : MonoBehaviour
{
    private DadosMontagem dados;
    public CanvasGroup painelPopup;             // Painel laranja
    public CanvasGroup fundoBlur;               // Fundo com blur
    public GameObject textoOrientacaoInicial;   // Texto de orientação (visível depois do popup)

    public TMP_Text texto1;     // Texto 1 dentro do popup
    public TMP_Text texto2;     // Texto 2 dentro do popup
    public TMP_Text botaoOK;    // Texto do botão OK

    public float duracaoFade = 0.5f;

    void Start()
    {
        dados = LocalizedDatabase.Load<DadosMontagem>(LocalizedDatabase.MontagemPath);
        painelPopup.alpha = 0;
        fundoBlur.alpha = 0;

        painelPopup.gameObject.SetActive(true);
        fundoBlur.gameObject.SetActive(true);

        painelPopup.interactable = false;
        painelPopup.blocksRaycasts = false;
        fundoBlur.interactable = false;
        fundoBlur.blocksRaycasts = false;

        // ✅ Aplica os textos traduzidos
        if (dados != null)
        {
            if (texto1 != null)
                texto1.text = dados.popupInicialTexto1;

            if (texto2 != null)
                texto2.text = dados.popupInicialTexto2;

            if (botaoOK != null)
                botaoOK.text = dados.popupInicialBotao;
        }

        if (textoOrientacaoInicial != null)
            textoOrientacaoInicial.SetActive(false);

        StartCoroutine(FadeIn());
    }

    public void FecharPopup()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeIn()
    {
        float tempo = 0;
        while (tempo < duracaoFade)
        {
            float alpha = Mathf.Lerp(0, 1, tempo / duracaoFade);
            painelPopup.alpha = alpha;
            fundoBlur.alpha = alpha;
            tempo += Time.deltaTime;
            yield return null;
        }

        painelPopup.alpha = 1;
        fundoBlur.alpha = 1;

        painelPopup.interactable = true;
        painelPopup.blocksRaycasts = true;
        fundoBlur.interactable = true;
        fundoBlur.blocksRaycasts = true;
    }

    IEnumerator FadeOut()
    {
        painelPopup.interactable = false;
        painelPopup.blocksRaycasts = false;
        fundoBlur.interactable = false;
        fundoBlur.blocksRaycasts = false;

        float tempo = 0;
        while (tempo < duracaoFade)
        {
            float alpha = Mathf.Lerp(1, 0, tempo / duracaoFade);
            painelPopup.alpha = alpha;
            fundoBlur.alpha = alpha;
            tempo += Time.deltaTime;
            yield return null;
        }

        painelPopup.alpha = 0;
        fundoBlur.alpha = 0;

        painelPopup.gameObject.SetActive(false);
        fundoBlur.gameObject.SetActive(false);

        // ✅ Ativa o painel de orientação com tradução
        if (textoOrientacaoInicial != null)
        {
            TMP_Text txt = textoOrientacaoInicial.GetComponent<TMP_Text>();
            if (txt != null && dados != null)
                txt.text = dados.tutorialInicial;

            textoOrientacaoInicial.SetActive(true);
        }
    }
}
