using UnityEngine;

public readonly struct TelaM4Layout
{
    public readonly Vector2 Dimensoes;
    public readonly float ConteudoRotacaoZ;

    public readonly Vector3 TextoPrincipalPosicao;
    public readonly Vector2 TextoPrincipalTamanho;

    public readonly Vector3 BarraContainerPosicao;
    public readonly float BarraLargura;
    public readonly float BarraAltura;
    public readonly float BarraSegmentoLargura;
    public readonly float BarraEspacamento;

    public readonly Vector3 LedPosicao;
    public readonly float LedDiametro;

    public readonly Vector3 AlertaContainerPosicao;
    public readonly Vector2 AlertaFundoTamanho;
    public readonly Vector2 AlertaTextoTamanho;

    public readonly Vector3 AnguloContainerPosicao;
    public readonly Vector2 AnguloFundoTamanho;
    public readonly Vector2 AnguloTextoTamanho;

    public TelaM4Layout(Vector2 dimensoes)
    {
        Dimensoes = dimensoes;
        float largura = dimensoes.x;
        float altura = dimensoes.y;
        ConteudoRotacaoZ = 90f;
        Quaternion rotacaoConteudo = Quaternion.Euler(0f, 0f, ConteudoRotacaoZ);

        TextoPrincipalPosicao = new Vector3(-largura * 0.15f, -altura * 0.007f, 0f);
        TextoPrincipalTamanho = new Vector2(largura * 0.9f, altura * 0.5f);

        BarraLargura = largura * 0.58f;
        BarraAltura = altura * 0.1f;
        BarraEspacamento = BarraLargura * 0.015f;
        BarraSegmentoLargura = (BarraLargura - BarraEspacamento * (TelaM4.QuantidadeSegmentosBarra - 1)) /
            TelaM4.QuantidadeSegmentosBarra;
        BarraContainerPosicao = TextoPrincipalPosicao + rotacaoConteudo * new Vector3(0f, -altura * 0.32f, 0f);

        LedPosicao = TextoPrincipalPosicao + rotacaoConteudo * new Vector3(largura * 0.42f, altura * 0.38f, 0f);
        LedDiametro = altura * 0.12f;

        AlertaContainerPosicao = TextoPrincipalPosicao;
        AlertaFundoTamanho = new Vector2(largura * 0.94f, altura * 0.42f);
        AlertaTextoTamanho = new Vector2(largura * 0.88f, altura * 0.38f);

        AnguloContainerPosicao = TextoPrincipalPosicao + rotacaoConteudo * new Vector3(0f, -altura * 0.75f, 0f);
        AnguloFundoTamanho = new Vector2(largura * 0.96f, altura * 0.32f);
        AnguloTextoTamanho = new Vector2(largura * 0.9f, altura * 0.28f);
    }
}
