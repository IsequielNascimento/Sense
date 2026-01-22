using UnityEngine;
using TMPro;

public class GerenciarMenu : MonoBehaviour
{
    [Header("Referências UI Menu")]
    public TMP_Text textoBotaoIniciar;
    public TMP_Text textoBotaoProblema;
    public TMP_Text textoSubtitulo;
    

    private string idiomaAtual = "pt";

    private void Start()
    {
        TrocarIdioma(idiomaAtual);
    }

    public void TrocarIdioma(string novoIdioma)
    {
        idiomaAtual = novoIdioma;
        CarregarBancoDeDadosMenu.Carregar(idiomaAtual);

        var dados = CarregarBancoDeDadosMenu.DadosMenu;
        textoBotaoIniciar.text = dados.botao_iniciar;
        textoBotaoProblema.text = dados.botao_problemas;
        textoSubtitulo.text = dados.subtitulo;
    }
}
