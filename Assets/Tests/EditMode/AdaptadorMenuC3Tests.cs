using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class AdaptadorMenuC3Tests
{
    private GameObject ancoraGO;
    private GameObject adaptadorGO;

    #region MARK - Configuração do adaptador sem depender do momento do Awake

    private AdaptadorMenuC3 CriarAdaptador(bool deviceNetHabilitado = false, bool iniciarNoMenu = false)
    {
        ancoraGO = new GameObject("Ancora");

        adaptadorGO = new GameObject("Adaptador");
        adaptadorGO.SetActive(false);

        var adaptador = adaptadorGO.AddComponent<AdaptadorMenuC3>();

        var so = new SerializedObject(adaptador);
        so.FindProperty("ancoraDisplay").objectReferenceValue = ancoraGO.transform;
        so.FindProperty("dimensoesDisplay").vector2Value = new Vector2(0.1f, 0.06f);
        so.FindProperty("deviceNetHabilitado").boolValue = deviceNetHabilitado;
        so.FindProperty("iniciarNoMenu").boolValue = iniciarNoMenu;
        so.ApplyModifiedPropertiesWithoutUndo();

        adaptador.Inicializar();
        return adaptador;
    }

    [TearDown]
    public void Limpar()
    {
        if (adaptadorGO != null) Object.DestroyImmediate(adaptadorGO);
        if (ancoraGO != null) Object.DestroyImmediate(ancoraGO);
    }

    #endregion

    #region MARK - Criação e reaproveitamento da TelaM4

    [Test]
    public void Awake_CriaTelaM4SobAAncoraQuandoNaoExiste()
    {
        CriarAdaptador();

        Assert.That(ancoraGO.GetComponentInChildren<TelaM4>(), Is.Not.Null);
    }

    [Test]
    public void Awake_ReaproveitaTelaM4ExistenteEmVezDeCriarOutra()
    {
        var telaExistente = TelaM4.CriarEm(new GameObject("Ancora2").transform, new Vector2(0.1f, 0.06f), 0, 0);
        ancoraGO = telaExistente.transform.parent.gameObject;

        adaptadorGO = new GameObject("Adaptador");
        adaptadorGO.SetActive(false);
        var adaptador = adaptadorGO.AddComponent<AdaptadorMenuC3>();
        var so = new SerializedObject(adaptador);
        so.FindProperty("ancoraDisplay").objectReferenceValue = ancoraGO.transform;
        so.ApplyModifiedPropertiesWithoutUndo();
        adaptador.Inicializar();

        Assert.That(ancoraGO.GetComponentsInChildren<TelaM4>().Length, Is.EqualTo(1));
    }

    #endregion

    #region MARK - Comandos públicos refletem no texto do display

    [Test]
    public void Awake_ComIniciarNoMenuAtivo_ComecaMostrandoC3EORotuloInicial()
    {
        var adaptador = CriarAdaptador(iniciarNoMenu: true);

        Assert.That(adaptador.MenuAtivo, Is.True);
        Assert.That(adaptador.TextoDisplayAtual, Is.EqualTo("C3\nPOSICAO"));
    }

    [Test]
    public void EntrarMenu_MostraORotuloDoModoDestacado()
    {
        var adaptador = CriarAdaptador();

        adaptador.EntrarMenu();

        Assert.That(adaptador.TextoDisplayAtual, Is.EqualTo("C3\nPOSICAO"));
    }

    [Test]
    public void ProximoB3EAnteriorB1_NavegamOsModos()
    {
        var adaptador = CriarAdaptador();

        adaptador.EntrarMenu();
        adaptador.ProximoB3();
        Assert.That(adaptador.TextoDisplayAtual, Is.EqualTo("C3\nANGULO"));

        adaptador.AnteriorB1();
        Assert.That(adaptador.TextoDisplayAtual, Is.EqualTo("C3\nPOSICAO"));
    }

    [Test]
    public void ConfirmarB2_SaiDoMenuEMostraValorRunDoNovoModo()
    {
        var adaptador = CriarAdaptador();
        adaptador.AtualizarValorRun(10f, v => v.ToString());

        adaptador.EntrarMenu();
        adaptador.ProximoB3();
        adaptador.ConfirmarB2();

        Assert.That(adaptador.MenuAtivo, Is.False);
        Assert.That(adaptador.ModoConfirmado, Is.EqualTo(ModoC3.Angle));
        Assert.That(adaptador.TextoDisplayAtual, Is.EqualTo(string.Empty));
    }

    [Test]
    public void CancelarSair_MantemOModoConfirmadoAnterior()
    {
        var adaptador = CriarAdaptador();

        adaptador.EntrarMenu();
        adaptador.ProximoB3();
        adaptador.CancelarSair();

        Assert.That(adaptador.ModoConfirmado, Is.EqualTo(ModoC3.Position));
    }

    [Test]
    public void AtualizarValorRun_ForaDoMenu_AtualizaOTextoExibido()
    {
        var adaptador = CriarAdaptador();

        adaptador.AtualizarValorRun(7.5f, v => v.ToString("F1", System.Globalization.CultureInfo.InvariantCulture));

        Assert.That(adaptador.TextoDisplayAtual, Is.EqualTo("7.5"));
    }

    #endregion
}
