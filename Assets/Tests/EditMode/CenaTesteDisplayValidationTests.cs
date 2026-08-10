using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class CenaTesteDisplayValidationTests
{
    private const string CaminhoCena = "Assets/Scenes/teste.unity";
    private const string GuidFbxOriginal = "4db4073cdc6432f439e7f363faaeba5a";

    private Scene cena;

    #region MARK - Ciclo de vida da cena carregada em modo aditivo

    [SetUp]
    public void AbrirCena()
    {
        cena = EditorSceneManager.OpenScene(CaminhoCena, OpenSceneMode.Additive);
    }

    [TearDown]
    public void FecharCena()
    {
        if (cena.IsValid()) EditorSceneManager.CloseScene(cena, true);
    }

    private GameObject EncontrarNaCena(string nome)
    {
        foreach (var raiz in cena.GetRootGameObjects())
        {
            if (raiz.name == nome) return raiz;
            var achado = raiz.transform.Find(nome);
            if (achado != null) return achado.gameObject;
        }
        return null;
    }

    #endregion

    #region MARK - Modelo e FBX de origem

    [Test]
    public void Cena_ContemOModeloM4SMARTTeste()
    {
        Assert.That(EncontrarNaCena("M4SMARTTeste"), Is.Not.Null);
    }

    [Test]
    public void Modelo_MantemAReferenciaDoFbxOriginal()
    {
        var modelo = EncontrarNaCena("M4SMARTTeste");
        var origem = PrefabUtility.GetCorrespondingObjectFromSource(modelo);
        var caminho = AssetDatabase.GetAssetPath(origem);
        var guid = AssetDatabase.AssetPathToGUID(caminho);

        Assert.That(guid, Is.EqualTo(GuidFbxOriginal));
    }

    #endregion

    #region MARK - Âncora DisplayDynamic e adaptadores

    [Test]
    public void DisplayDynamic_ExisteSobOModelo()
    {
        var modelo = EncontrarNaCena("M4SMARTTeste");
        var ancora = modelo.transform.Find("DisplayDynamic");

        Assert.That(ancora, Is.Not.Null);
    }

    [Test]
    public void DisplayDynamic_NaoContemCanvas()
    {
        var modelo = EncontrarNaCena("M4SMARTTeste");
        var ancora = modelo.transform.Find("DisplayDynamic");

        Assert.That(ancora.GetComponentsInChildren<Canvas>(true), Is.Empty);
    }

    [Test]
    public void AdaptadorMenuC3_ReferenciaAAncoraDisplayDynamic()
    {
        var modelo = EncontrarNaCena("M4SMARTTeste");
        var ancora = modelo.transform.Find("DisplayDynamic");
        var adaptador = ancora.GetComponent<AdaptadorMenuC3>();

        Assert.That(adaptador, Is.Not.Null);

        var so = new SerializedObject(adaptador);
        var referencia = so.FindProperty("ancoraDisplay").objectReferenceValue as Transform;

        Assert.That(referencia, Is.EqualTo(ancora));
    }

    [Test]
    public void AdaptadorMenuC3_ComecaConfiguradoParaIniciarNoMenu()
    {
        var modelo = EncontrarNaCena("M4SMARTTeste");
        var ancora = modelo.transform.Find("DisplayDynamic");
        var adaptador = ancora.GetComponent<AdaptadorMenuC3>();

        var so = new SerializedObject(adaptador);

        Assert.That(so.FindProperty("iniciarNoMenu").boolValue, Is.True);
    }

    [Test]
    public void AdaptadorMenuC3_ComecaNoPreviewDeCalibracaoEmCinquentaPorCento()
    {
        var modelo = EncontrarNaCena("M4SMARTTeste");
        var ancora = modelo.transform.Find("DisplayDynamic");
        var adaptador = ancora.GetComponent<AdaptadorMenuC3>();

        var so = new SerializedObject(adaptador);

        Assert.That(so.FindProperty("usarEstadoPreview").boolValue, Is.True);
        Assert.That(so.FindProperty("textoPreview").stringValue, Is.EqualTo("50.0%\nAUTO"));
        Assert.That(so.FindProperty("ledsPreview").stringValue, Is.EqualTo("vermelho"));
        Assert.That(so.FindProperty("progressoPreview").floatValue, Is.EqualTo(0.5f));
    }

    [Test]
    public void AdaptadorEntradaPreviewC3_EstaPresenteEReferenciaOAdaptador()
    {
        var modelo = EncontrarNaCena("M4SMARTTeste");
        var ancora = modelo.transform.Find("DisplayDynamic");
        var adaptador = ancora.GetComponent<AdaptadorMenuC3>();
        var entradaPreview = ancora.GetComponent<AdaptadorEntradaPreviewC3>();

        Assert.That(entradaPreview, Is.Not.Null);

        var so = new SerializedObject(entradaPreview);
        Assert.That(so.FindProperty("adaptador").objectReferenceValue, Is.EqualTo(adaptador));
    }

    [Test]
    public void ControladorLeds_ReferenciaOMeshDeLedsDoModelo()
    {
        var modelo = EncontrarNaCena("M4SMARTTeste");
        var ancora = modelo.transform.Find("DisplayDynamic");
        var controlador = ancora.GetComponent<ControladorLedsM4>();

        Assert.That(controlador, Is.Not.Null);
        Assert.That(controlador.RendererLeds, Is.EqualTo(modelo.transform.Find("LEDS").GetComponent<Renderer>()));
        Assert.That(controlador.EstadoAtual, Is.EqualTo(EstadoLedsM4.Aberto));
    }

    #endregion

    #region MARK - Pós-processamento dos LEDs

    [Test]
    public void Cena_UsaPerfilGlobalComBloomAtivo()
    {
        var volume = EncontrarNaCena("M4 Bloom").GetComponent<Volume>();

        Assert.That(volume.isGlobal, Is.True);
        Assert.That(volume.sharedProfile, Is.Not.Null);
        Assert.That(volume.sharedProfile.TryGet(out Bloom bloom), Is.True);
        Assert.That(bloom.active, Is.True);
        Assert.That(bloom.intensity.overrideState, Is.True);
        Assert.That(bloom.intensity.value, Is.GreaterThan(0f));
    }

    #endregion

    #region MARK - Recursos DSEG carregam

    [Test]
    public void RecursoFonteDsegCarregaDoResources()
    {
        Assert.That(Resources.Load<TMP_FontAsset>("M4Display/DSEG14Classic SDF"), Is.Not.Null);
    }

    [Test]
    public void RecursoMaterialLcdDsegCarregaDoResources()
    {
        Assert.That(Resources.Load<Material>("M4Display/DSEG14Classic M4 LCD"), Is.Not.Null);
    }

    #endregion
}
