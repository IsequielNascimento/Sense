using UnityEngine;
using TMPro;

public class GerenciarMontagem2 : MonoBehaviour
{
    public TMP_Text textoTitulo;
    public TMP_Text textoSubtitulo;
    public TMP_Text textoBotaoIniciar;
    public TMP_Text textoBotaoAviso;

    void Start()
 {
    DadosMontagem2 dados = LocalizedDatabase.Load<DadosMontagem2>(LocalizedDatabase.Montagem2Path);
    if (dados == null)
    {
        Debug.LogError("Dados de montagem não carregados. Verifique o JSON.");
        return;
    }

    textoTitulo.text = dados.titulo;
    textoSubtitulo.text = dados.subtitulo;
    textoBotaoIniciar.text = dados.botao_iniciar;
    textoBotaoAviso.text = dados.botao_aviso;
 }

}
