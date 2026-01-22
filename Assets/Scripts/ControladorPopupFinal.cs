using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ControladorPopupFinal : MonoBehaviour
{
    public CanvasGroup painelPopupFinal;
    public CanvasGroup fundoBlurFinal;
    public float duracaoFade = 0.5f;
    public GerenciarMontagem gerenciarMontagem;

    void Start()
    {
        painelPopupFinal.alpha = 0;
        fundoBlurFinal.alpha = 0;
        painelPopupFinal.gameObject.SetActive(false);
        fundoBlurFinal.gameObject.SetActive(false);
    }

    public void MostrarPopupFinal()
    {
        painelPopupFinal.gameObject.SetActive(true);
        fundoBlurFinal.gameObject.SetActive(true);

        if (CarregarBancoDeDadosMontagem.Dados != null)
        {
            var dados = CarregarBancoDeDadosMontagem.Dados;
            TMP_Text titulo = painelPopupFinal.transform.Find("Parabens")?.GetComponent<TMP_Text>();
            TMP_Text voltar = painelPopupFinal.transform.Find("Botãovoltar/TextoVoltar")?.GetComponent<TMP_Text>();
            TMP_Text recomecar = painelPopupFinal.transform.Find("Recomeçar")?.GetComponent<TMP_Text>();

            if (titulo) titulo.text = dados.popupFinalTitulo;
            if (voltar) voltar.text = dados.popupFinalBotaoMenu;
            if (recomecar) recomecar.text = dados.popupFinalBotaoRecomecar;
        }

        StartCoroutine(FadeIn());
    }

    public void VoltarMenu()
    {
    // Limpa os dados do problema selecionado, se houver
    if (ProblemaSelecionadoAR.Instance != null)
        Destroy(ProblemaSelecionadoAR.Instance.gameObject);

    SceneManager.LoadScene(0); // Ou a cena do menu principal
    }


    public void RecomeçarMontagem()
    {
        FecharPopupFinal();
        gerenciarMontagem.ReiniciarMontagem();
    }

    public void FecharPopupFinal()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeIn()
    {
        float t = 0;
        while (t < duracaoFade)
        {
            float a = Mathf.Lerp(0, 1, t / duracaoFade);
            painelPopupFinal.alpha = a;
            fundoBlurFinal.alpha = a;
            t += Time.deltaTime;
            yield return null;
        }

        painelPopupFinal.alpha = 1;
        fundoBlurFinal.alpha = 1;
        painelPopupFinal.interactable = true;
        painelPopupFinal.blocksRaycasts = true;
        fundoBlurFinal.interactable = true;
        fundoBlurFinal.blocksRaycasts = true;
    }

    IEnumerator FadeOut()
    {
        painelPopupFinal.interactable = false;
        painelPopupFinal.blocksRaycasts = false;
        fundoBlurFinal.interactable = false;
        fundoBlurFinal.blocksRaycasts = false;

        float t = 0;
        while (t < duracaoFade)
        {
            float a = Mathf.Lerp(1, 0, t / duracaoFade);
            painelPopupFinal.alpha = a;
            fundoBlurFinal.alpha = a;
            t += Time.deltaTime;
            yield return null;
        }

        painelPopupFinal.alpha = 0;
        fundoBlurFinal.alpha = 0;
        painelPopupFinal.gameObject.SetActive(false);
        fundoBlurFinal.gameObject.SetActive(false);
    }
}
