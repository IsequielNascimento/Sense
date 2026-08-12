using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class AmbienteSimuladoReentradaTests
{
    private const string CenaAR = "AR_Cena_UIToolkit";
    private const string CenaMenu = "Main-Menu";
    private const string PrefixoCenaAmbiente = "Simulated Environment Scene";
    private const float TempoLimite = 15f;

    private static readonly string[] ElementosObrigatorios = { "Tabletop", "Floor", "Wall" };

    [UnityTest]
    public IEnumerator AmbienteComMesaChaoEParedeEhRecriadoAoEntrarNovamente()
    {
        yield return CarregarCena(CenaAR);
        yield return EsperarAmbienteCompleto("primeira entrada");

        yield return CarregarCena(CenaMenu);
        Assert.That(LocalizarCenaDoAmbiente().IsValid(), Is.False,
            "O ambiente simulado anterior deveria ser removido ao voltar ao menu.");

        yield return CarregarCena(CenaAR);
        yield return EsperarAmbienteCompleto("segunda entrada");
    }

    [UnityTearDown]
    public IEnumerator VoltarAoMenu()
    {
        if (SceneManager.GetActiveScene().name != CenaMenu)
        {
            yield return CarregarCena(CenaMenu);
        }
    }

    private static IEnumerator CarregarCena(string nome)
    {
        AsyncOperation operacao = SceneManager.LoadSceneAsync(nome, LoadSceneMode.Single);
        Assert.That(operacao, Is.Not.Null, $"Nao foi possivel iniciar o carregamento da cena '{nome}'.");

        while (!operacao.isDone)
        {
            yield return null;
        }

        yield return null;
    }

    private static IEnumerator EsperarAmbienteCompleto(string entrada)
    {
        float limite = Time.realtimeSinceStartup + TempoLimite;
        while (Time.realtimeSinceStartup < limite && !AmbienteEstaCompleto())
        {
            yield return null;
        }

        Scene cenaAmbiente = LocalizarCenaDoAmbiente();
        Assert.That(cenaAmbiente.IsValid() && cenaAmbiente.isLoaded, Is.True,
            $"A cena do XR Simulation nao foi criada na {entrada}.");

        HashSet<string> nomes = NomesDosObjetos(cenaAmbiente);
        Assert.That(nomes, Is.SupersetOf(ElementosObrigatorios),
            $"Mesa, chao e parede precisam existir na {entrada}.");
    }

    private static bool AmbienteEstaCompleto()
    {
        Scene cena = LocalizarCenaDoAmbiente();
        return cena.IsValid() && cena.isLoaded && NomesDosObjetos(cena).IsSupersetOf(ElementosObrigatorios);
    }

    private static Scene LocalizarCenaDoAmbiente()
    {
        for (int indice = 0; indice < SceneManager.sceneCount; indice++)
        {
            Scene cena = SceneManager.GetSceneAt(indice);
            if (cena.name.StartsWith(PrefixoCenaAmbiente))
            {
                return cena;
            }
        }

        return default;
    }

    private static HashSet<string> NomesDosObjetos(Scene cena)
    {
        return cena.GetRootGameObjects()
            .SelectMany(raiz => raiz.GetComponentsInChildren<Transform>(true))
            .Select(transform => transform.name)
            .ToHashSet();
    }
}
