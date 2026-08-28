using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class DestaquesPiscantesDeAlertaTests
{
    #region MARK: Fixture

    private const string CaminhoMaterialDeOutline = "Assets/Materials/Destaque Outline.mat";

    private static readonly string[] AlertasComDestaque =
        { "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A9", "A15", "A16", "A17", "A18", "A19", "A20", "A23", "A24", "A25" };

    private static readonly Dictionary<string, string[]> PecasEsperadasPorAlerta = new Dictionary<string, string[]>
    {
        { "A1", new[] { "PNEUMATICA", "Magueira", "Magueira.001", "ATUADOR", "COPO" } },
        { "A2", new[] { "PNEUMATICA", "Magueira", "Magueira.001", "ATUADOR", "COPO" } },
        { "A3", new[] { "PNEUMATICA", "Magueira", "Magueira.001", "ATUADOR", "COPO" } },
        { "A4", new[] { "PNEUMATICA", "Magueira", "Magueira.001" } },
        { "A5", new[] { "PNEUMATICA", "Magueira", "Magueira.001", "ATUADOR", "COPO" } },
        { "A6", new[] { "PNEUMATICA", "MODULO_ELETRONICO" } },
        { "A7", new[] { "PNEUMATICA", "Magueira", "Magueira.001", "ATUADOR", "COPO" } },
        { "A9", new[] { "PNEUMATICA", "Magueira", "Magueira.001" } },
        { "A15", new[] { "PNEUMATICA", "Magueira", "Magueira.001", "ATUADOR", "COPO" } },
        { "A16", new[] { "PNEUMATICA", "Magueira", "Magueira.001", "ATUADOR", "COPO" } },
        { "A17", new[] { "PNEUMATICA", "Magueira", "Magueira.001" } },
        { "A18", new[] { "PNEUMATICA", "Magueira", "Magueira.001" } },
        { "A19", new[] { "MODULO_ELETRONICO" } },
        { "A20", new[] { "MODULO_ELETRONICO" } },
        { "A23", new[] { "PNEUMATICA", "MODULO_ELETRONICO" } },
        { "A24", new[] { "PNEUMATICA", "MODULO_ELETRONICO" } },
        { "A25", new[] { "MODULO_ELETRONICO" } },
    };

    private static GameObject Prefab(string codigo)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(
            $"Assets/Resources/M4ProblemA{codigo.Substring(1)}/M4SMARTTesteProblema{codigo}.prefab");
    }

    private static Transform Peca(GameObject prefab, string nome)
    {
        return prefab.GetComponentsInChildren<Transform>(true).SingleOrDefault(t => t.name == nome);
    }

    private static Mesh MalhaDaPeca(Transform peca)
    {
        var filtro = peca.GetComponent<MeshFilter>();

        if (filtro != null && filtro.sharedMesh != null) return filtro.sharedMesh;

        var pele = peca.GetComponent<SkinnedMeshRenderer>();

        return pele != null ? pele.sharedMesh : null;
    }

    private static IEnumerable<(string Nome, GameObject Objeto)> EfeitosRegistrados(GameObject prefab)
    {
        Type tipoGerenciador = Type.GetType("GerenciadorVisual, Sense.Runtime");
        Component gerenciador = prefab.GetComponentInChildren(tipoGerenciador, true);
        var efeitos = (IEnumerable)tipoGerenciador.GetField("efeitosDisponiveis").GetValue(gerenciador);

        foreach (object efeito in efeitos)
        {
            Type tipo = efeito.GetType();
            yield return (
                (string)tipo.GetField("Nome").GetValue(efeito),
                (GameObject)tipo.GetField("VfxObject").GetValue(efeito));
        }
    }

    #endregion

    #region MARK: Estrutura dos contornos

    [TestCaseSource(nameof(AlertasComDestaque))]
    public void CadaPecaDestacada_TemUmContornoDesativadoComAMalhaDaPropriaPeca(string codigo)
    {
        GameObject prefab = Prefab(codigo);
        Assert.That(prefab, Is.Not.Null, $"{codigo}: prefab nao encontrado.");

        Material outline = AssetDatabase.LoadAssetAtPath<Material>(CaminhoMaterialDeOutline);

        foreach (string nomeDaPeca in PecasEsperadasPorAlerta[codigo])
        {
            Transform peca = Peca(prefab, nomeDaPeca);
            Assert.That(peca, Is.Not.Null, $"{codigo}: peca '{nomeDaPeca}' nao encontrada.");

            Transform contorno = peca.Find($"{nomeDaPeca}_Outline");
            Assert.That(contorno, Is.Not.Null, $"{codigo}: contorno de '{nomeDaPeca}' nao encontrado.");
            Assert.That(contorno.gameObject.activeSelf, Is.False, $"{codigo}/{nomeDaPeca}: contorno deve comecar desativado.");

            Assert.That(
                contorno.GetComponent<MeshFilter>().sharedMesh,
                Is.EqualTo(MalhaDaPeca(peca)),
                $"{codigo}/{nomeDaPeca}: malha do contorno divergente.");

            Assert.That(
                contorno.GetComponent<MeshRenderer>().sharedMaterials.All(material => material == outline),
                Is.True,
                $"{codigo}/{nomeDaPeca}: contorno deve usar apenas o material de outline.");
        }
    }

    #endregion

    #region MARK: Pulso suave em todo contorno

    [TestCaseSource(nameof(AlertasComDestaque))]
    public void TodoContorno_PulsaPorMeioDoDestaquePiscante(string codigo)
    {
        GameObject prefab = Prefab(codigo);

        foreach (string nomeDaPeca in PecasEsperadasPorAlerta[codigo])
        {
            Transform contorno = Peca(prefab, nomeDaPeca).Find($"{nomeDaPeca}_Outline");
            var piscante = contorno.GetComponent<DestaquePiscante>();

            Assert.That(piscante, Is.Not.Null, $"{codigo}/{nomeDaPeca}: falta o DestaquePiscante.");
            Assert.That(piscante.PeriodoSegundos, Is.GreaterThan(0f), $"{codigo}/{nomeDaPeca}: periodo invalido.");
            Assert.That(piscante.RendererDestaque, Is.Not.Null, $"{codigo}/{nomeDaPeca}: renderer nao religado.");
        }
    }

    [Test]
    public void UmCicloDoPulso_VaiDoMaximoAoMinimoEVolta()
    {
        var objeto = new GameObject("contorno", typeof(MeshRenderer));

        try
        {
            var piscante = objeto.AddComponent<DestaquePiscante>();
            float periodo = piscante.PeriodoSegundos;

            Assert.That(piscante.AvancarTempo(0f),
                Is.EqualTo(RegrasDePulsoDeDestaque.IntensidadeMaxima).Within(0.001f));
            Assert.That(piscante.AvancarTempo(periodo / 2f),
                Is.EqualTo(RegrasDePulsoDeDestaque.IntensidadeMinima).Within(0.001f));
            Assert.That(piscante.AvancarTempo(periodo / 2f),
                Is.EqualTo(RegrasDePulsoDeDestaque.IntensidadeMaxima).Within(0.001f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(objeto);
        }
    }

    #endregion

    #region MARK: Ligacao entre o perfil de display e o prefab

    [TestCaseSource(nameof(AlertasComDestaque))]
    public void TodoVfxPedidoPeloPerfil_EstaRegistradoNoPrefabDoAlerta(string codigo)
    {
        PerfilDeDisplayDeAlerta perfil = PerfisDeDisplayDeAlerta.Obter(codigo);
        Assert.That(perfil, Is.Not.Null, $"{codigo}: perfil de display ausente.");

        var registrados = EfeitosRegistrados(Prefab(codigo)).ToList();

        IEnumerable<string> pedidos = perfil.EtapasOficiais
            .SelectMany(etapa => etapa.Quadros)
            .Select(quadro => quadro.Vfx)
            .Where(vfx => !string.IsNullOrEmpty(vfx))
            .Distinct();

        foreach (string vfx in pedidos)
        {
            Assert.That(
                registrados.Any(efeito => efeito.Nome == vfx),
                Is.True,
                $"{codigo}: o perfil pede o VFX '{vfx}', mas o prefab nao o registra.");
        }
    }

    [TestCaseSource(nameof(AlertasComDestaque))]
    public void NenhumEfeitoRegistrado_ApontaParaUmObjetoPerdido(string codigo)
    {
        var perdidos = EfeitosRegistrados(Prefab(codigo))
            .Select((efeito, indice) => (efeito.Nome, efeito.Objeto, indice))
            .Where(item => item.Objeto == null)
            .Select(item => $"[{item.indice}] {item.Nome}")
            .ToArray();

        Assert.That(perdidos, Is.Empty,
            $"{codigo}: entrada de VFX apontando para objeto inexistente, sobra de uma regeneracao do prefab. " +
            "Rode 'Sense/Alertas/Criar destaques piscantes' para reconstruir a lista.");
    }

    [Test]
    public void PrefabDeA3_RegistraOBlocoPneumaticoSeparadoDoAtuadorECopo()
    {
        var registrados = EfeitosRegistrados(Prefab("A3")).ToList();

        Assert.That(
            registrados.Where(efeito => efeito.Nome == "DestaquePneumatica").Select(efeito => efeito.Objeto.name),
            Is.EquivalentTo(new[] { "PNEUMATICA_Outline" }));

        Assert.That(
            registrados.Where(efeito => efeito.Nome == "DestaqueAtuadorCopo").Select(efeito => efeito.Objeto.name),
            Is.EquivalentTo(new[] { "ATUADOR_Outline", "COPO_Outline" }));
    }

    [Test]
    public void PrefabDeA3_RegistraAsDuasMangueirasSobOMesmoEfeito()
    {
        var registrados = EfeitosRegistrados(Prefab("A3")).ToList();

        Assert.That(
            registrados.Where(efeito => efeito.Nome == "DestaqueMangueiras").Select(efeito => efeito.Objeto.name),
            Is.EquivalentTo(new[] { "Magueira_Outline", "Magueira.001_Outline" }));
    }

    [Test]
    public void NenhumaPecaPertenceADoisEfeitosAoMesmoTempo()
    {
        foreach (string codigo in AlertasComDestaque)
        {
            var porObjeto = EfeitosRegistrados(Prefab(codigo))
                .GroupBy(efeito => efeito.Objeto)
                .Where(grupo => grupo.Select(efeito => efeito.Nome).Distinct().Count() > 1);

            Assert.That(porObjeto, Is.Empty,
                $"{codigo}: um contorno em dois efeitos seria desligado pelo efeito perdedor em AtivarEfeito.");
        }
    }

    #endregion
}
