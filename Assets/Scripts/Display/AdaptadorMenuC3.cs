using System;
using UnityEngine;

public class AdaptadorMenuC3 : MonoBehaviour
{
    #region MARK - Configuração serializada

    [SerializeField] private Transform ancoraDisplay;
    [SerializeField] private Vector2 dimensoesDisplay = new Vector2(0.1f, 0.06f);
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrderBase;
    [SerializeField] private bool deviceNetHabilitado;
    [SerializeField] private bool iniciarNoMenu = true;
    [SerializeField] private EstadoLedsM4 estadoLeds = EstadoLedsM4.Aberto;

    #endregion

    #region MARK - Estado

    private MenuC3 menu;
    private TelaM4 tela;

    #endregion

    public MenuC3 Menu => menu;
    public ModoC3 ModoConfirmado => menu.ModoConfirmado;
    public bool MenuAtivo => menu.MenuAtivo;
    public string TextoDisplayAtual => menu.TextoDisplayAtual;

    private void Awake()
    {
        Inicializar();
    }

    internal void Inicializar()
    {
        menu = new MenuC3(deviceNetHabilitado);
        if (iniciarNoMenu) menu.Entrar();
        ObterOuCriarTela();
        Renderizar();
    }

    private void ObterOuCriarTela()
    {
        if (ancoraDisplay == null)
        {
            Debug.LogError("[AdaptadorMenuC3] 'ancoraDisplay' não foi atribuída.");
            return;
        }

        tela = ancoraDisplay.GetComponentInChildren<TelaM4>(includeInactive: true);
        if (tela == null)
        {
            int layerId = SortingLayer.NameToID(sortingLayerName);
            tela = TelaM4.CriarEm(ancoraDisplay, dimensoesDisplay, layerId, sortingOrderBase);
        }
    }

    private void Renderizar()
    {
        if (tela == null) return;
        tela.Aplicar(new Etapa
        {
            textoDisplay = menu.TextoDisplayAtual,
            leds = estadoLeds.ToString()
        });
    }

    #region MARK - Comandos públicos (B1/B2/B3)

    public void EntrarMenu()
    {
        menu.Entrar();
        Renderizar();
    }

    public void ProximoB3()
    {
        menu.ProximoB3();
        Renderizar();
    }

    public void AnteriorB1()
    {
        menu.AnteriorB1();
        Renderizar();
    }

    public void ConfirmarB2()
    {
        menu.ConfirmarB2();
        Renderizar();
    }

    public void CancelarSair()
    {
        menu.CancelarSair();
        Renderizar();
    }

    public void AtualizarValorRun<T>(T valor, Func<T, string> formatador)
    {
        menu.AtualizarValorRun(valor, formatador);
        Renderizar();
    }

    public void AtualizarEstadoLeds(EstadoLedsM4 estado)
    {
        estadoLeds = estado;
        Renderizar();
    }

    #endregion
}
