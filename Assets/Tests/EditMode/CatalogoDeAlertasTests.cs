using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class CatalogoDeAlertasTests
{
    #region MARK: Fixture

    private const string PastaDeAssets = "Assets/Resources/CatalogoDeAlertas";

    private const string VerificarArComprimido = "Verificar o fornecimento de ar comprimido";
    private const string VerificarAvarias = "Verificar possíveis avarias no conjunto válvula / atuador";
    private const string GeradorDeArComprimido = "gerador de ar comprimido";
    private const string EmCampo = "em campo";

    private static readonly string[] Funcionais =
    {
        "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A9"
    };

    private static readonly string[] Operacionais =
    {
        "A11", "A12", "A13", "A14", "A15", "A16", "A17", "A18",
        "A19", "A20", "A21", "A22", "A23", "A24", "A25"
    };

    private IReadOnlyList<AlertaOficial> catalogo;

    [SetUp]
    public void CarregarCatalogo()
    {
        catalogo = CatalogoDeAlertas.Carregar();
    }

    #endregion

    #region MARK: Transcricao da Tabela 10

    public sealed class RegistroDoManual
    {
        public string Codigo;
        public string Nome;
        public string OQueE;
        public string QuandoOcorre;
        public string[] Acoes;
        public string[] Locais;
        public string Padrao;

        public override string ToString() => Codigo;
    }

    private static IEnumerable<RegistroDoManual> RegistrosDoManual()
    {
        yield return new RegistroDoManual
        {
            Codigo = "A1",
            Nome = "ÂNGULO MÍNIMO",
            OQueE = "a válvula não atingiu o ângulo de operação mínimo necessário",
            QuandoOcorre = "alerta devido ao ângulo formado entre o ponto de abertura e o ponto de fechamento inferior a 30°",
            Acoes = new[] { VerificarArComprimido, VerificarAvarias },
            Locais = new[] { GeradorDeArComprimido, EmCampo },
            Padrao = "sempre ligado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A2",
            Nome = "TEMPO LIMITE",
            OQueE = "o tempo para realização da calibração automática é monitorado",
            QuandoOcorre = "alerta por tempo excedente para calibração da válvula de acordo com o configurado no menu",
            Acoes = new[] { "Aumentar tempo limite", VerificarAvarias },
            Locais = new[] { "menu tempo max. cal", EmCampo },
            Padrao = "sempre ligado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A3",
            Nome = "FALHA NA AUTO CALIBRAÇÃO",
            OQueE = "o deslocamento do eixo é monitorado durante o processo de calibração e armazena o tempo e as posições de aberto e fechado",
            QuandoOcorre = "alerta devido ao armazenamento dos pontos de aberto e fechado não serem iguais nos ciclos de calibração",
            Acoes = new[] { VerificarArComprimido, VerificarAvarias },
            Locais = new[] { GeradorDeArComprimido, EmCampo },
            Padrao = "sempre ligado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A4",
            Nome = "FORA DE FAIXA",
            OQueE = "a pressão de alimentação está superior a utilizada no processo de calibração",
            QuandoOcorre = "alerta devido ao avanço do ponto de abertura ou fechamento da válvula aprendido na calibração em 5°",
            Acoes = new[] { "Verificar fornecimento de ar comprimido" },
            Locais = new[] { GeradorDeArComprimido },
            Padrao = "sempre ligado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A5",
            Nome = "SEM MOVIMENTO",
            OQueE = "o deslocamento do eixo é monitorado durante o processo de calibração",
            QuandoOcorre = "alerta devido a falta de movimento da válvula durante a calibração",
            Acoes = new[] { VerificarArComprimido, VerificarAvarias },
            Locais = new[] { GeradorDeArComprimido, EmCampo },
            Padrao = "sempre ligado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A6",
            Nome = "FALHA DE COMANDO",
            OQueE = "o comando para solenoide é monitorado",
            QuandoOcorre = "alerta quando a válvula não executa o comando da solenoide",
            Acoes = new[]
            {
                "Verifica se a solenoide está com defeito",
                "Verificar a falta sinal elétrico para solenoide",
            },
            Locais = new[] { "solenoide", EmCampo },
            Padrao = "sempre ligado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A7",
            Nome = "MUDANÇA INESPERADA",
            OQueE = "a posição da válvula é continuamente monitorada",
            QuandoOcorre = "alerta devido a uma mudança de posição não esperada, independente do comando da solenoide",
            Acoes = new[]
            {
                "Verificar acionamento manual da válvula",
                "Verificar conexões pneumáticas",
            },
            Locais = new[] { EmCampo },
            Padrao = "sempre ligado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A8",
            Nome = "MODO SEGURO",
            OQueE = "monitor não esta no modo segurado podendo ser acessado indevidamente",
            QuandoOcorre = "quando o monitor não está no modo seguro",
            Acoes = new[] { "Colocar o monitor no modo seguro" },
            Locais = new[] { "menu modo seguro" },
            Padrao = "sempre ligado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A9",
            Nome = "MÁX. PRESSÃO NA LINHA",
            OQueE = "a pressão da linha é monitorada",
            QuandoOcorre = "alerta devido a pressão máxima da linha ultrapassar 9 bar",
            Acoes = new[] { VerificarArComprimido },
            Locais = new[] { GeradorDeArComprimido },
            Padrao = "sempre ligado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A11",
            Nome = "CONTADO PARCIAL",
            OQueE = "contador aumenta toda vez que a válvula atingir a posição aberta",
            QuandoOcorre = "quando a contagem parcial é atingida",
            Acoes = new[]
            {
                "Aumentar número de ciclos ou resetar o contador",
                "Desligar alerta",
            },
            Locais = new[] { EmCampo, "menu contador parcial" },
            Padrao = "desabilitado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A12",
            Nome = "CONTADOR TOTAL",
            OQueE = "contador aumenta toda vez que a válvula atingir a posição aberta",
            QuandoOcorre = "quando a contagem total é atingida",
            Acoes = new[] { "Resetar o contador", "Desligar o alerta" },
            Locais = new[] { EmCampo, "menu contador total" },
            Padrao = "desabilitado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A13",
            Nome = "ALERTA DIAS",
            OQueE = "monitor acumula número de dias desde que foi energizado",
            QuandoOcorre = "quando o dia ajustado é atingido",
            Acoes = new[] { "Resetar alerta", "Desligar alerta" },
            Locais = new[] { EmCampo, "menu alerta dias" },
            Padrao = "desabilitado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A14",
            Nome = "ALERTA DATA",
            OQueE = "alerta para uma data especifica",
            QuandoOcorre = "quando a data ajustada é atingida",
            Acoes = new[] { "Definir nova data", "Desligar alerta" },
            Locais = new[] { EmCampo, "menu alerta data" },
            Padrao = "desabilitado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A15",
            Nome = "TEMPO ABERTURA",
            OQueE = "monitor aprende o tempo de abertura da válvula durante o processo de calibração",
            QuandoOcorre = "quando o tempo de abertura é ultrapassado",
            Acoes = new[] { VerificarArComprimido, VerificarAvarias, "Desligar o alerta" },
            Locais = new[] { GeradorDeArComprimido, EmCampo, "menu tempo abertura" },
            Padrao = "desabilitado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A16",
            Nome = "TEMPO FECHAMENTO",
            OQueE = "monitor aprende o tempo de fechamento da válvula durante o processo de calibração",
            QuandoOcorre = "quando o tempo de fechamento é ultrapassado",
            Acoes = new[] { VerificarArComprimido, VerificarAvarias, "Desligar o alerta" },
            Locais = new[] { GeradorDeArComprimido, EmCampo, "menu tempo fechamento" },
            Padrao = "desabilitado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A17",
            Nome = "ALTA PRESSÃO",
            OQueE = "a pressão da linha é monitorada",
            QuandoOcorre = "quando a pressão de entrada está acima da faixa especificada",
            Acoes = new[] { "Verificar fornecimento de ar comprimido", "Desligar alerta" },
            Locais = new[] { GeradorDeArComprimido, "menu alta pressão" },
            Padrao = "20%",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A18",
            Nome = "BAIXA PRESSÃO",
            OQueE = "a pressão da linha é monitorada",
            QuandoOcorre = "quando a pressão de entrada está abaixo da faixa especificada",
            Acoes = new[] { "Verificar fornecimento de ar comprimido", "Desligar alerta" },
            Locais = new[] { GeradorDeArComprimido, "menu baixa pressão" },
            Padrao = "20%",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A19",
            Nome = "ALIMENTAÇÃO ALTA",
            OQueE = "a tensão de alimentação é monitorada",
            QuandoOcorre = "tensão acima 32Vcc para rede AS-Interface e 10% para PNP e DeviceNet",
            Acoes = new[] { "Verificar fonte de alimentação" },
            Locais = new[] { "painel de controle" },
            Padrao = "habilitado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A20",
            Nome = "ALIMENTAÇÃO BAIXA",
            OQueE = "a tensão de alimentação é monitorada",
            QuandoOcorre = "tensão abaixo de 27V para AS-Interface e 22,8V DeviceNet e PNP",
            Acoes = new[] { "Verificar fonte de alimentação" },
            Locais = new[] { "painel de controle" },
            Padrao = "habilitado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A21",
            Nome = "TEMPERATURA ALTA",
            OQueE = "a temperatura interna é monitorada",
            QuandoOcorre = "quando a temperatura do monitor está acima de 70°",
            Acoes = new[] { "Verificar temperatura do processo" },
            Locais = new[] { EmCampo },
            Padrao = "habilitado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A22",
            Nome = "TEMPERATURA BAIXA",
            OQueE = "a temperatura interna é monitorada",
            QuandoOcorre = "quando a temperatura do monitor está abaixo de -20°C",
            Acoes = new[] { "Verificar temperatura do processo" },
            Locais = new[] { EmCampo },
            Padrao = "habilitado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A23",
            Nome = "SOLENOIDE CURTO",
            OQueE = "a bobina solenoide é monitorada",
            QuandoOcorre = "quando a bobina da solenoide esta em curto circuito",
            Acoes = new[] { "Verificar os fios da solenoide" },
            Locais = new[] { EmCampo },
            Padrao = "habilitado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A24",
            Nome = "SOLENOIDE ABERTA",
            OQueE = "monitora a quebra do cabo da bobina solenoide",
            QuandoOcorre = "quando a bobina da solenoide ou seu cabo está quebrado",
            Acoes = new[] { "Verificar os fios da solenoide" },
            Locais = new[] { EmCampo },
            Padrao = "habilitado",
        };

        yield return new RegistroDoManual
        {
            Codigo = "A25",
            Nome = "SAÍDA CURTO",
            OQueE = "as saídas PNP são monitoradas",
            QuandoOcorre = "quando qualquer uma das saídas PNP ou suas cargas estiverem em curto",
            Acoes = new[] { "Verificar conexão das saídas" },
            Locais = new[] { EmCampo, "painel de controle" },
            Padrao = "habilitado",
        };
    }

    [TestCaseSource(nameof(RegistrosDoManual))]
    public void TodoAlerta_TranscreveALiteralidadeDaTabela10(RegistroDoManual esperado)
    {
        AlertaOficial alerta = Buscar(esperado.Codigo);

        Assert.That(alerta.Nome, Is.EqualTo(esperado.Nome), $"{esperado.Codigo}: nome.");
        Assert.That(alerta.OQueE, Is.EqualTo(esperado.OQueE), $"{esperado.Codigo}: 'O que e'.");
        Assert.That(alerta.QuandoOcorre, Is.EqualTo(esperado.QuandoOcorre), $"{esperado.Codigo}: 'Quando ocorre'.");
        Assert.That(alerta.Acoes, Is.EqualTo(esperado.Acoes), $"{esperado.Codigo}: 'O que fazer'.");
        Assert.That(alerta.Locais, Is.EqualTo(esperado.Locais), $"{esperado.Codigo}: 'Onde'.");
        Assert.That(alerta.Padrao, Is.EqualTo(esperado.Padrao), $"{esperado.Codigo}: 'Padrao'.");
    }

    [Test]
    public void ATranscricaoCobreOsVinteEQuatroCodigosOficiais()
    {
        Assert.That(RegistrosDoManual().Select(registro => registro.Codigo),
            Is.EqualTo(CodigosOficiais.Alertas.ToArray()));
    }

    #endregion

    #region MARK: Conjunto oficial

    [Test]
    public void Catalogo_TemVinteEQuatroRegistros()
    {
        Assert.That(catalogo.Count, Is.EqualTo(24));
    }

    [Test]
    public void Catalogo_SegueAOrdemEOConjuntoDoManual()
    {
        string[] esperado = Funcionais.Concat(Operacionais).ToArray();

        Assert.That(catalogo.Select(alerta => alerta.Codigo), Is.EqualTo(esperado));
        Assert.That(esperado, Is.EqualTo(CodigosOficiais.Alertas.ToArray()));
    }

    [Test]
    public void Catalogo_NaoContemA10()
    {
        Assert.That(catalogo.Any(alerta => alerta.Codigo == "A10"), Is.False);
        Assert.That(CatalogoDeAlertas.Obter("A10"), Is.Null);
    }

    [Test]
    public void ArquivosDeDados_NaoDeclaramA10()
    {
        Assert.That(LerAsset("estrutura"), Does.Not.Contain("\"A10\""));
        Assert.That(LerAsset("pt"), Does.Not.Contain("\"A10\""));
    }

    [Test]
    public void Estrutura_NaoDeclaraCodigoForaDoManualNemRepetido()
    {
        AlertaEstrutural[] itens = CatalogoDeAlertas.CarregarEstrutura().alertas;

        Assert.That(itens.Length, Is.EqualTo(24));
        Assert.That(itens.Select(item => item.codigo).Distinct().Count(), Is.EqualTo(24));

        foreach (AlertaEstrutural item in itens)
        {
            Assert.That(CodigosOficiais.EhAlerta(item.codigo), Is.True,
                $"estrutura declara codigo inexistente '{item.codigo}'.");
        }
    }

    [Test]
    public void Textos_NaoDeclaramCodigoForaDoManualNemRepetido()
    {
        TextoDeAlerta[] itens = CatalogoDeAlertas.CarregarTextos("pt").alertas;

        Assert.That(itens.Length, Is.EqualTo(24));
        Assert.That(itens.Select(item => item.codigo).Distinct().Count(), Is.EqualTo(24));

        foreach (TextoDeAlerta item in itens)
        {
            Assert.That(CodigosOficiais.EhAlerta(item.codigo), Is.True,
                $"pt declara codigo inexistente '{item.codigo}'.");
        }
    }

    #endregion

    #region MARK: Campos obrigatorios

    [Test]
    public void TodoAlerta_TemNomeEDescricoesPreenchidos()
    {
        foreach (AlertaOficial alerta in catalogo)
        {
            Assert.That(alerta.Nome, Is.Not.Empty, $"{alerta.Codigo} sem nome.");
            Assert.That(alerta.OQueE, Is.Not.Empty, $"{alerta.Codigo} sem 'O que e'.");
            Assert.That(alerta.QuandoOcorre, Is.Not.Empty, $"{alerta.Codigo} sem 'Quando ocorre'.");
            Assert.That(alerta.Locais, Is.Not.Empty, $"{alerta.Codigo} sem 'Onde'.");
            Assert.That(alerta.Padrao, Is.Not.Empty, $"{alerta.Codigo} sem 'Padrao'.");
        }
    }

    [Test]
    public void TodoAlerta_TemCategoriaConhecida()
    {
        foreach (string codigo in Funcionais)
        {
            Assert.That(Buscar(codigo).EhFuncional, Is.True, $"{codigo} deveria ser funcional.");
        }

        foreach (string codigo in Operacionais)
        {
            Assert.That(Buscar(codigo).EhOperacional, Is.True, $"{codigo} deveria ser operacional.");
        }

        Assert.That(catalogo.Count(alerta => alerta.EhFuncional), Is.EqualTo(9));
        Assert.That(catalogo.Count(alerta => alerta.EhOperacional), Is.EqualTo(15));
    }

    #endregion

    #region MARK: Listas ordenadas

    [Test]
    public void TodoAlerta_TemAcoesELocaisNaQuantidadeDeclarada()
    {
        foreach (AlertaOficial alerta in catalogo)
        {
            Assert.That(alerta.Acoes, Is.Not.Empty, $"{alerta.Codigo} sem acao corretiva.");
            Assert.That(alerta.Acoes.Count, Is.EqualTo(alerta.QuantidadeDeAcoesEsperada),
                $"{alerta.Codigo}: quantidade de acoes divergente da estrutura.");
            Assert.That(alerta.Locais.Count, Is.EqualTo(alerta.QuantidadeDeLocaisEsperada),
                $"{alerta.Codigo}: quantidade de locais divergente da estrutura.");
        }
    }

    [Test]
    public void NenhumaAcao_ECarregaNumeracaoOuVemVazia()
    {
        foreach (AlertaOficial alerta in catalogo)
        {
            foreach (string acao in alerta.Acoes)
            {
                Assert.That(acao, Is.Not.Empty, $"{alerta.Codigo} tem acao vazia.");
                Assert.That(char.IsDigit(acao[0]), Is.False,
                    $"{alerta.Codigo}: a ordem deve vir da lista, nao de numero no texto ('{acao}').");
            }
        }
    }

    [Test]
    public void NenhumLocal_CarregaSeparadorEditorialOuVemVazio()
    {
        foreach (AlertaOficial alerta in catalogo)
        {
            foreach (string local in alerta.Locais)
            {
                Assert.That(local, Is.Not.Empty, $"{alerta.Codigo} tem local vazio.");
                Assert.That(local, Does.Not.Contain(";"),
                    $"{alerta.Codigo}: cada linha da coluna 'Onde' e um item da lista ('{local}').");
                Assert.That(local.Trim(), Is.EqualTo(local),
                    $"{alerta.Codigo}: local com espaco de borda ('{local}').");
            }
        }
    }

    #endregion

    #region MARK: Padroes de fabrica

    [TestCaseSource(nameof(Funcionais))]
    public void AlertasFuncionais_SaoSempreLigados(string codigo)
    {
        Assert.That(Buscar(codigo).Padrao, Is.EqualTo("sempre ligado"));
    }

    [TestCase("A11")]
    [TestCase("A12")]
    [TestCase("A13")]
    [TestCase("A14")]
    [TestCase("A15")]
    [TestCase("A16")]
    public void ContadoresEAlertasDeTempo_SaoDesabilitadosDeFabrica(string codigo)
    {
        Assert.That(Buscar(codigo).Padrao, Is.EqualTo("desabilitado"));
    }

    [TestCase("A17")]
    [TestCase("A18")]
    public void AlertasDePressao_TemPadraoPercentual(string codigo)
    {
        Assert.That(Buscar(codigo).Padrao, Is.EqualTo("20%"));
    }

    [TestCase("A19")]
    [TestCase("A20")]
    [TestCase("A21")]
    [TestCase("A22")]
    [TestCase("A23")]
    [TestCase("A24")]
    [TestCase("A25")]
    public void AlertasDeDiagnostico_SaoHabilitadosDeFabrica(string codigo)
    {
        Assert.That(Buscar(codigo).Padrao, Is.EqualTo("habilitado"));
    }

    #endregion

    #region MARK: Rastreabilidade

    [Test]
    public void TodoAlerta_ApontaParaAsTabelasDoManual()
    {
        foreach (AlertaOficial alerta in catalogo)
        {
            Assert.That(alerta.PaginaTabelaCodigos, Is.EqualTo(11), $"{alerta.Codigo} sem Tabela 3.");
            Assert.That(alerta.PaginaTabelaResolucao, Is.EqualTo(76), $"{alerta.Codigo} sem Tabela 10.");
        }
    }

    #endregion

    #region MARK: Separacao de dados e traducao

    [Test]
    public void Estrutura_NaoGuardaTextoTraduzivel()
    {
        string conteudo = LerAsset("estrutura");

        foreach (string campo in new[] { "nome", "oQueE", "quandoOcorre", "acoes", "locais", "padrao" })
        {
            Assert.That(conteudo, Does.Not.Contain($"\"{campo}\""),
                $"estrutura.json nao deve guardar o campo traduzivel '{campo}'.");
        }
    }

    [Test]
    public void ArquivoDeIdioma_NaoRepeteEstrutura()
    {
        foreach (string idioma in IdiomasPresentes())
        {
            string conteudo = LerAsset(idioma);

            foreach (string campo in new[] { "categoria", "quantidadeDe", "paginaTabela" })
            {
                Assert.That(conteudo, Does.Not.Contain($"\"{campo}"),
                    $"{idioma}.json duplica o campo estrutural '{campo}'.");
            }
        }
    }

    [Test]
    public void PaginasDoManual_FicamEmUmLugarSo()
    {
        EstruturaDoCatalogoDeAlertas estrutura = CatalogoDeAlertas.CarregarEstrutura();

        Assert.That(estrutura.paginaTabelaCodigos, Is.EqualTo(11));
        Assert.That(estrutura.paginaTabelaResolucao, Is.EqualTo(76));
        Assert.That(LerAsset("estrutura").Split(new[] { "\"paginaTabelaCodigos\"" }, System.StringSplitOptions.None).Length,
            Is.EqualTo(2));
    }

    #endregion

    #region MARK: Nota normativa

    private const string NotaEsperada =
        "Ao desabilitar os alertas de 19 a 25, apenas as notificações no aplicativo serão desabilitadas. A indicação física no monitor, continua funcionando normalmente.";

    private static readonly string[] AlertasComNota =
    {
        "A19", "A20", "A21", "A22", "A23", "A24", "A25"
    };

    [TestCaseSource(nameof(AlertasComNota))]
    public void AlertasDeDiagnostico_RecebemANotaLiteral(string codigo)
    {
        Assert.That(Buscar(codigo).Nota, Is.EqualTo(NotaEsperada));
    }

    [Test]
    public void AlertasQueNaoSaoDeDiagnostico_NaoRecebemNota()
    {
        foreach (string codigo in Funcionais.Concat(Operacionais.Except(AlertasComNota)))
        {
            Assert.That(Buscar(codigo).Nota, Is.Empty, $"{codigo} nao deveria ter nota.");
        }
    }

    [Test]
    public void NotaNormativa_EUmCampoGlobalUnicoEmPt()
    {
        string conteudo = LerAsset("pt");

        Assert.That(conteudo.Split(new[] { "\"notaAlertasDiagnostico\"" }, System.StringSplitOptions.None).Length,
            Is.EqualTo(2), "pt.json deve declarar a nota uma unica vez.");
    }

    [Test]
    public void NotaNormativa_NaoFicaNaEstruturaNemNosRegistrosDoIdioma()
    {
        Assert.That(LerAsset("estrutura"), Does.Not.Contain("notaAlertasDiagnostico"));
    }

    #endregion

    #region MARK: Avisos do manual

    private const string TextoDoAvisoA9 =
        "Alertamos para o risco de danos ao equipamento caso a pressão da linha exceda 9 bar (130,5 psi).”";

    private static readonly string[] AlertasBloqueadosParaAvisos =
    {
        "A6", "A19", "A20", "A23", "A24", "A25"
    };

    [Test]
    public void A9_RecebeExatamenteUmAvisoImportanteDaPagina5ComOTextoLiteral()
    {
        AlertaOficial a9 = Buscar("A9");

        Assert.That(a9.Avisos.Count, Is.EqualTo(1));
        Assert.That(a9.Avisos[0].Nivel, Is.EqualTo(NivelDeAviso.Importante));
        Assert.That(a9.Avisos[0].Pagina, Is.EqualTo(5));
        Assert.That(a9.Avisos[0].Texto, Is.EqualTo(TextoDoAvisoA9));
    }

    [Test]
    public void OsOutrosVinteETresAlertas_NaoRecebemAviso()
    {
        foreach (AlertaOficial alerta in catalogo)
        {
            if (alerta.Codigo == "A9") continue;

            Assert.That(alerta.Avisos, Is.Empty, $"{alerta.Codigo} nao deveria ter aviso.");
        }
    }

    [TestCaseSource(nameof(AlertasBloqueadosParaAvisos))]
    public void AlertasBloqueados_NaoRecebemAvisoPorInferencia(string codigo)
    {
        Assert.That(Buscar(codigo).Avisos, Is.Empty, $"{codigo} nao pode receber aviso sem aprovacao tecnica.");
    }

    #endregion

    #region MARK: Idiomas tecnicos pendentes de revisao

    private static readonly string[] IdiomasTecnicos = { "pt", "en", "es", "fr" };

    private static readonly string[] IdiomasPendentes = { "en", "es", "fr" };

    [TestCaseSource(nameof(IdiomasPendentes))]
    public void CarregarComIdiomaPendente_ResolveParaOMesmoConteudoDoPt(string idioma)
    {
        IReadOnlyList<AlertaOficial> catalogoPt = CatalogoDeAlertas.Carregar("pt");
        IReadOnlyList<AlertaOficial> catalogoPendente = CatalogoDeAlertas.Carregar(idioma);

        Assert.That(catalogoPendente.Select(alerta => alerta.Codigo),
            Is.EqualTo(catalogoPt.Select(alerta => alerta.Codigo)));

        for (int i = 0; i < catalogoPt.Count; i++)
        {
            AlertaOficial esperado = catalogoPt[i];
            AlertaOficial obtido = catalogoPendente[i];

            Assert.That(obtido.Categoria, Is.EqualTo(esperado.Categoria), $"{esperado.Codigo}: categoria.");
            Assert.That(obtido.Nome, Is.EqualTo(esperado.Nome), $"{esperado.Codigo}: nome.");
            Assert.That(obtido.OQueE, Is.EqualTo(esperado.OQueE), $"{esperado.Codigo}: 'O que e'.");
            Assert.That(obtido.QuandoOcorre, Is.EqualTo(esperado.QuandoOcorre), $"{esperado.Codigo}: 'Quando ocorre'.");
            Assert.That(obtido.Acoes, Is.EqualTo(esperado.Acoes), $"{esperado.Codigo}: 'O que fazer'.");
            Assert.That(obtido.Locais, Is.EqualTo(esperado.Locais), $"{esperado.Codigo}: 'Onde'.");
            Assert.That(obtido.QuantidadeDeAcoesEsperada, Is.EqualTo(esperado.QuantidadeDeAcoesEsperada), $"{esperado.Codigo}: quantidade de acoes esperada.");
            Assert.That(obtido.QuantidadeDeLocaisEsperada, Is.EqualTo(esperado.QuantidadeDeLocaisEsperada), $"{esperado.Codigo}: quantidade de locais esperada.");
            Assert.That(obtido.Padrao, Is.EqualTo(esperado.Padrao), $"{esperado.Codigo}: 'Padrao'.");
            Assert.That(obtido.Nota, Is.EqualTo(esperado.Nota), $"{esperado.Codigo}: nota.");
            Assert.That(obtido.PaginaTabelaCodigos, Is.EqualTo(esperado.PaginaTabelaCodigos), $"{esperado.Codigo}: pagina tabela codigos.");
            Assert.That(obtido.PaginaTabelaResolucao, Is.EqualTo(esperado.PaginaTabelaResolucao), $"{esperado.Codigo}: pagina tabela resolucao.");

            Assert.That(obtido.Avisos.Count, Is.EqualTo(esperado.Avisos.Count), $"{esperado.Codigo}: quantidade de avisos.");

            for (int j = 0; j < esperado.Avisos.Count; j++)
            {
                AvisoOficial avisoEsperado = esperado.Avisos[j];
                AvisoOficial avisoObtido = obtido.Avisos[j];

                Assert.That(avisoObtido.Id, Is.EqualTo(avisoEsperado.Id), $"{esperado.Codigo}: aviso[{j}].Id.");
                Assert.That(avisoObtido.Nivel, Is.EqualTo(avisoEsperado.Nivel), $"{esperado.Codigo}: aviso[{j}].Nivel.");
                Assert.That(avisoObtido.Pagina, Is.EqualTo(avisoEsperado.Pagina), $"{esperado.Codigo}: aviso[{j}].Pagina.");
                Assert.That(avisoObtido.Texto, Is.EqualTo(avisoEsperado.Texto), $"{esperado.Codigo}: aviso[{j}].Texto.");
            }
        }
    }

    [TestCaseSource(nameof(IdiomasTecnicos))]
    public void Carregar_RetornaVinteEQuatroCodigosParaTodoIdiomaTecnico(string idioma)
    {
        Assert.That(CatalogoDeAlertas.Carregar(idioma).Count, Is.EqualTo(24));
    }

    [Test]
    public void ArquivosDeIdiomaPendente_NaoExistemEmCatalogoDeAlertas()
    {
        foreach (string idioma in IdiomasPendentes)
        {
            string caminho = $"{PastaDeAssets}/{idioma}.json";
            Assert.That(AssetDatabase.LoadAssetAtPath<TextAsset>(caminho), Is.Null,
                $"{caminho} nao deveria existir; {idioma} esta pendente de revisao.");
        }
    }

    [Test]
    public void UnicoIdiomaPresenteEmCatalogoDeAlertas_EPt()
    {
        Assert.That(IdiomasPresentes().ToArray(), Is.EqualTo(new[] { "pt" }));
    }

    #endregion

    #region MARK: Auxiliares

    private AlertaOficial Buscar(string codigo)
    {
        AlertaOficial alerta = catalogo.FirstOrDefault(item => item.Codigo == codigo);

        Assert.That(alerta, Is.Not.Null, $"Alerta '{codigo}' ausente do catalogo.");

        return alerta;
    }

    private static IEnumerable<string> IdiomasPresentes()
    {
        return AssetDatabase.FindAssets("t:TextAsset", new[] { PastaDeAssets })
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(caminho => System.IO.Path.GetFileNameWithoutExtension(caminho))
            .Where(nome => nome != CatalogoDeAlertas.ArquivoDeEstrutura);
    }

    private static string LerAsset(string nome)
    {
        string caminho = $"{PastaDeAssets}/{nome}.json";
        TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(caminho);

        Assert.That(asset, Is.Not.Null, $"Arquivo nao encontrado: {caminho}");

        return asset.text;
    }

    #endregion
}
