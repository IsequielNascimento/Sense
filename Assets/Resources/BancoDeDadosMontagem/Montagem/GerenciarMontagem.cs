using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class GerenciarMontagem : MonoBehaviour
{
    [Header("UI - Passos")]
    public TMP_Text textoTutorial;
    public TMP_Text textoNumero;
    public TMP_Text textoProximo;

    [Header("UI - Instrução Inicial")]
    public TMP_Text textoInstrucaoInicial;

    [Header("Progresso Circular")]
    public Image preenchimentoProgresso;
    public TMP_Text textoNumeroProgresso;
    public TMP_Text textoTotalPassos;

    [Header("Referências")]
    public ControladorPopupFinal popupFinal;

    private string[] passosTutoriais;
    private string[] animacoes;
    private int passoAtual = -1;
    private bool passosIniciados = false;
    private PlaceOnPlane placeOnPlane;
    private DadosMontagem dadosMontagem;

    void Start()
    {
        dadosMontagem = LocalizedDatabase.Load<DadosMontagem>(LocalizedDatabase.MontagemPath);
        CarregarPassosDoProblemaOuMontagem();

        if (passosTutoriais == null || passosTutoriais.Length == 0)
        {
            Debug.LogError("Passos não carregados corretamente. Verifique os arquivos JSON e os caminhos.");
            if (textoInstrucaoInicial != null) textoInstrucaoInicial.text = "Erro ao carregar dados.";
            return;
        }

        placeOnPlane = FindObjectOfType<PlaceOnPlane>();

        if (textoTotalPassos != null)
        {
            string deTraduzido = dadosMontagem?.textoDe ?? "de";
            textoTotalPassos.text = $"{deTraduzido} {passosTutoriais.Length}";
        }

        if (preenchimentoProgresso != null)
            preenchimentoProgresso.fillAmount = 0f;

        var botaoSair = GameObject.Find("Sair")?.GetComponentInChildren<TMP_Text>();
        if (botaoSair != null && dadosMontagem != null)
            botaoSair.text = dadosMontagem.sair;
    }

    private void CarregarPassosDoProblemaOuMontagem()
    {
        string origem = ControleDeCena.Instance?.origemDaCena ?? "montagem";
        string instrucaoInicial = "";

        if (origem == "montagem")
        {
            DadosMontagem dados = dadosMontagem;

            if (dados != null && dados.passos != null && dados.passos.Length > 0)
            {
                passosTutoriais = dados.passos.Select(p => p.tutorial).ToArray();
                animacoes = dados.passos.Select(p => $"animacao_{p.numero}").ToArray();
                instrucaoInicial = dados.tutorialInicial;
                Debug.Log("✅ Passos carregados do banco padrão de montagem.");
            }
        }
        else
        {
            string id = ProblemaSelecionadoAR.Instance?.idProblema;
            if (!string.IsNullOrEmpty(id))
            {
                string caminhoDoJson = $"BancoDeDadosProblemas/{{language}}/{id}";
                PassoAPasso dados = LocalizedDatabase.Load<PassoAPasso>(caminhoDoJson);

                if (dados.etapas != null && dados.etapas.Length > 0)
                {
                    passosTutoriais = dados.etapas.Select(e => e.tutorial).ToArray();
                    animacoes = dados.etapas.Select(e => e.animacao).ToArray();
                    ProblemaSelecionadoAR.Instance.passoAPasso = dados;
                    instrucaoInicial = dados.tutorialInicial;
                    Debug.Log($"✅ PassoAPasso carregado para problema '{id}'.");
                }
                else
                {
                    Debug.LogError($"❌ O problema '{id}' não contém etapas.");
                    instrucaoInicial = "Arquivo de problema não encontrado.";
                }
            }
        }

        if (textoInstrucaoInicial != null)
        {
            if (string.IsNullOrEmpty(instrucaoInicial))
            {
                textoInstrucaoInicial.text = dadosMontagem?.tutorialInicial ?? "Toque na superfície para posicionar o objeto.";
            }
            else
            {
                textoInstrucaoInicial.text = instrucaoInicial;
            }
        }
    }

    // O restante do script permanece inalterado
    public void IniciarPassos()
    {
        if (passosTutoriais == null || passosTutoriais.Length == 0) return;
        passoAtual = 0;
        passosIniciados = true;
        AtualizarPasso();
    }

    public void AvancarPasso()
    {
        if (!passosIniciados) return;
        if (passoAtual >= passosTutoriais.Length - 1)
        {
            popupFinal?.MostrarPopupFinal();
            return;
        }
        passoAtual++;
        AtualizarPasso();
    }

    public void VoltarPasso()
    {
        if (!passosIniciados || passoAtual <= 0) return;
        passoAtual--;
        AtualizarPasso();
    }

    public void RepetirPasso()
    {
        if (!passosIniciados) return;
        AtualizarPasso();

        if (placeOnPlane != null && passoAtual < animacoes.Length)
            placeOnPlane.PlayAnimation(animacoes[passoAtual]);
    }

    private void AtualizarPasso()
    {
        if (passoAtual < 0 || passoAtual >= passosTutoriais.Length) return;

        textoTutorial.text = passosTutoriais[passoAtual];
        textoNumero.text = (passoAtual + 1).ToString();
        textoNumeroProgresso.text = (passoAtual + 1).ToString();
        textoTotalPassos.text = $"{dadosMontagem?.textoDe ?? "de"} {passosTutoriais.Length}";

        preenchimentoProgresso.fillAmount = (float)(passoAtual + 1) / passosTutoriais.Length;

        bool ultimo = passoAtual == passosTutoriais.Length - 1;
        textoProximo.text = ultimo
            ? dadosMontagem?.finalizar ?? "Finalizar Montagem"
            : dadosMontagem?.proximo ?? "Próximo Passo";
        textoProximo.color = ultimo ? new Color32(245, 71, 3, 255) : Color.white;
        Button botao = textoProximo.GetComponentInParent<Button>();
        if (botao != null)
            botao.image.color = ultimo ? Color.white : new Color32(245, 71, 3, 255);

        if (placeOnPlane != null && passoAtual < animacoes.Length)
            placeOnPlane.PlayAnimation(animacoes[passoAtual]);
    }

    public void ReiniciarMontagem()
    {
        if (passosTutoriais == null || passosTutoriais.Length == 0) return;
        passoAtual = 0;
        passosIniciados = true;
        AtualizarPasso();

        if (placeOnPlane != null && animacoes.Length > 0)
            placeOnPlane.PlayAnimation(animacoes[0]);
    }

    public bool IsRunning() => passosIniciados;
}
