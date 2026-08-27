using System;
using System.Collections.Generic;

public static class PerfisDeDisplayDeAlerta
{
    #region MARK: Textos comprovados pelo manual

    public const string CodigoA8 = "A8";
    public const string Menu = "MENU";
    public const string MenuConfig = "MENU\nCONFIG";
    public const string MenuModoSeguro = "C16\nMODO S";
    public const string Senha = "SENHA";
    public const string Habilitar = "HABILI";
    public const string ExemploDeSenha = "1234";
    public const string Sair = "SAIR";
    public const string CodigoA11 = "A11";
    public const string ResetContadorParcial = "C18";
    public const string CodigoA12 = "A12";
    public const string MenuAlerta = "MENU\nALERTA";
    public const string MenuContadorTotal = "A12\nCONTAD";
    public const string ResetContadorTotal = "C19\nRESET";
    public const string Sim = "SIM";
    public const string Desabilitar = "DESABI";
    public const string CodigoA13 = "A13";
    public const string MenuAlertaDias = "A13\nALERTA";
    public const string Limpar = "LIMPAR";
    public const string CodigoA1 = "A1";
    public const string VerificarArComprimido = "Verificar o fornecimento de ar comprimido";
    public const string VerificarAvariasNoConjunto = "Verificar possíveis avarias no conjunto válvula / atuador";
    public const string CodigoA2 = "A2";
    public const string CodigoA3 = "A3";
    public const string FastSetup = "FAST\nSETUP";
    public const string Certo = "CERTO";
    public const string Abortar = "ABORT";
    public const string CodigoA4 = "A4";
    public const string MenuPressaoDaLinha = "C17";
    public const string CodigoA5 = "A5";
    public const string CodigoA6 = "A6";
    public const string CodigoA7 = "A7";
    public const string CodigoA9 = "A9";
    public const string CodigoA23 = "A23";
    public const string CodigoA24 = "A24";
    public const string CodigoA25 = "A25";
    public const string CodigoA14 = "A14";
    public const string MenuAlertaData = "A14\nALERTA";
    public const string ExemploDeData = "31 12\n2023";
    public const string CodigoA19 = "A19";
    public const string CodigoA20 = "A20";
    public const string MenuDevice = "MENU\nDEVICE";
    public const string PartNumber = "d1";
    public const string CodigoA21 = "A21";
    public const string CodigoA22 = "A22";
    public const string MenuDisplay = "C3\nDISPLA";
    public const string Temperatura = "TEMPER";
    public const string MenuTempoMaxCal = "C6";
    public const string CodigoA17 = "A17";
    public const string CodigoA18 = "A18";
    public const string MenuPressaoAlta = "A17\nPRESSA";
    public const string MenuPressaoBaixa = "A18\nPRESSA";
    public const string CodigoA15 = "A15";
    public const string CodigoA16 = "A16";
    public const string MenuTempoAbertura = "A15\nTEMPO";
    public const string MenuTempoFechamento = "A16\nTEMPO";

    #endregion

    #region MARK: Animacoes do modelo M4

    public const string AnimacaoProblema1 = "PROBLEMA1";
    public const string LayerProblema1 = "Problema 1";

    #endregion

    #region MARK: VFX do modelo M4

    public const string DestaqueAtuadorCopo = "DestaqueAtuadorCopo";
    public const string DestaquePneumatica = "DestaquePneumatica";
    public const string DestaqueMangueiras = "DestaqueMangueiras";
    public const string DestaqueModuloEletronico = "DestaqueModuloEletronico";

    #endregion

    #region MARK: Registro central

    private static readonly Dictionary<string, PerfilDeDisplayDeAlerta> PorCodigo = Registrar();

    private static Dictionary<string, PerfilDeDisplayDeAlerta> Registrar()
    {
        var registro = new Dictionary<string, PerfilDeDisplayDeAlerta>(StringComparer.Ordinal);

        Adicionar(registro, CriarPerfilDeModoSeguro());
        Adicionar(registro, CriarPerfilDeContadorParcial());
        Adicionar(registro, CriarPerfilDeContadorTotal());
        Adicionar(registro, CriarPerfilDeAlertaDias());
        Adicionar(registro, CriarPerfilDeAlertaData());
        Adicionar(registro, CriarPerfilDeTemperaturaAlta());
        Adicionar(registro, CriarPerfilDeTemperaturaBaixa());
        Adicionar(registro, CriarPerfilDeAnguloMinimo());
        Adicionar(registro, CriarPerfilDeTempoLimite());
        Adicionar(registro, CriarPerfilDeFalhaNaAutoCalibracao());
        Adicionar(registro, CriarPerfilDeForaDeFaixa());
        Adicionar(registro, CriarPerfilDeSemMovimento());
        Adicionar(registro, CriarPerfilDeMaxPressaoNaLinha());
        Adicionar(registro, CriarPerfilDeFalhaDeComando());
        Adicionar(registro, CriarPerfilDeSolenoideCurto());
        Adicionar(registro, CriarPerfilDeSolenoideAberta());
        Adicionar(registro, CriarPerfilDeSaidaCurto());
        Adicionar(registro, CriarPerfilDeAlimentacaoAlta());
        Adicionar(registro, CriarPerfilDeAlimentacaoBaixa());
        Adicionar(registro, CriarPerfilDePressaoAlta());
        Adicionar(registro, CriarPerfilDePressaoBaixa());
        Adicionar(registro, CriarPerfilDeTempoDeAbertura());
        Adicionar(registro, CriarPerfilDeTempoDeFechamento());
        Adicionar(registro, CriarPerfilDeMudancaInesperada());

        return registro;
    }

    private static void Adicionar(
        Dictionary<string, PerfilDeDisplayDeAlerta> registro,
        PerfilDeDisplayDeAlerta perfil)
    {
        registro[perfil.Codigo] = perfil;
    }

    public static PerfilDeDisplayDeAlerta Obter(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return null;

        return PorCodigo.TryGetValue(codigo.Trim(), out PerfilDeDisplayDeAlerta perfil) ? perfil : null;
    }

    public static bool Existe(string codigo)
    {
        return Obter(codigo) != null;
    }

    public static IReadOnlyCollection<string> CodigosComPerfil => PorCodigo.Keys;

    #endregion

    #region MARK: A8 MODO SEGURO, paginas 11, 50, 51, 53, 56, 60 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeModoSeguro()
    {
        var acaoUnica = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA8,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confirme o alerta A8: o monitor não está no modo seguro. O LED vermelho deve piscar até a correção."),
            new QuadroDeDisplayM4(
                Menu,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Aproxime o polo Sul do chaveiro magnético do botão B2 por mais de 6 segundos para entrar no menu.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                MenuConfig,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Após o bargraph, o display mostra MENU CONFIG. Pressione B2 para acessar as configurações."),
            new QuadroDeDisplayM4(
                MenuModoSeguro,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Use B3 para avançar até C16 MODO SEGURO. B1 volta à opção anterior e B2 entra em C16."),
            new QuadroDeDisplayM4(
                Senha,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Em C16, selecione SENHA com B1 ou B3 e pressione B2."),
            new QuadroDeDisplayM4(
                Habilitar,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Use B3 para selecionar HABILITAR e pressione B2 para definir a senha."),
            new QuadroDeDisplayM4(
                ExemploDeSenha,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Escolha quatro dígitos: B1 decrementa, B3 incrementa e B2 seleciona. Mantenha B2 por mais de 3 segundos para confirmar. 1234 é apenas o exemplo do manual."),
            new QuadroDeDisplayM4(
                MenuModoSeguro,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: o modo seguro está habilitado, o alerta A8 foi eliminado e o LED vermelho parou de piscar."),
            new QuadroDeDisplayM4(
                Sair,
                EstadoLedsM4.Desligado,
                instrucao: "Em uma opção principal, mantenha B1 por mais de 3 segundos para sair. O display mostra SAIR e retorna ao modo RUN."));

        return new PerfilDeDisplayDeAlerta(
            CodigoA8,
            new[] { acaoUnica },
            mecanismoDeAtivacaoConfirmado: true);
    }

    #endregion

    #region MARK: A11 CONTADO PARCIAL, paginas 11, 54 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeContadorParcial()
    {
        var ajusteOuReset = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA11,
                EstadoLedsM4.Desligado,
                instrucao: "Confirme o alerta A11: o contador parcial atingiu o limite configurado. A11 vem desabilitado por padrão, então esse alerta indica que ele foi habilitado."),
            new QuadroDeDisplayM4(
                Menu,
                EstadoLedsM4.Desligado,
                instrucao: "Aproxime o polo Sul do chaveiro magnético do botão B2 por mais de 6 segundos para entrar no menu.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                MenuConfig,
                EstadoLedsM4.Desligado,
                instrucao: "Após o bargraph, o display mostra MENU CONFIG. Pressione B2 para acessar as configurações."),
            new QuadroDeDisplayM4(
                CodigoA11,
                EstadoLedsM4.Desligado,
                instrucao: "Use B3 para avançar até A11. B1 volta à opção anterior e B2 entra no menu do contador parcial, que configura o limite e também pode zerar o contador."),
            new QuadroDeDisplayM4(
                CodigoA11,
                EstadoLedsM4.Desligado,
                instrucao: "Alternativa 1: aumente o número de ciclos configurado como limite do contador parcial. B1 diminui e B3 aumenta o valor. Mantenha B2 por mais de 3 segundos para confirmar."),
            new QuadroDeDisplayM4(
                CodigoA11,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: o novo limite do contador parcial foi salvo e o alerta A11 foi eliminado. O contador total não é alterado."),
            new QuadroDeDisplayM4(
                ResetContadorParcial,
                EstadoLedsM4.Desligado,
                instrucao: "Alternativa 2: em vez de aumentar o limite, entre em C18 para resetar o contador parcial. Use B3 para avançar até C18 RESET CONTADOR PARCIAL. B1 volta à opção anterior e B2 entra no reset."),
            new QuadroDeDisplayM4(
                ResetContadorParcial,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: o contador parcial voltou a zero e o alerta A11 foi eliminado. O contador total não é alterado nem resetado."));

        var desligarAlerta = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA11,
                EstadoLedsM4.Desligado,
                instrucao: "Identifique A11 no menu de alertas. Esta ação apenas desliga o alerta, sem ajustar o limite e sem resetar o contador parcial."),
            new QuadroDeDisplayM4(
                Menu,
                EstadoLedsM4.Desligado,
                instrucao: "Aproxime o polo Sul do chaveiro magnético do botão B2 por mais de 6 segundos para entrar no menu.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                MenuConfig,
                EstadoLedsM4.Desligado,
                instrucao: "Após o bargraph, o display mostra MENU CONFIG. Pressione B2 para acessar as configurações."),
            new QuadroDeDisplayM4(
                CodigoA11,
                EstadoLedsM4.Desligado,
                instrucao: "Use B3 para avançar até A11 e B2 para selecionar a opção de desligar o alerta, mantendo o contador parcial como está."),
            new QuadroDeDisplayM4(
                CodigoA11,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: o alerta A11 foi desligado. Isso não reseta nem altera o contador parcial ou o total."));

        return new PerfilDeDisplayDeAlerta(
            CodigoA11,
            new[] { ajusteOuReset, desligarAlerta },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion

    #region MARK: A12 CONTADOR TOTAL, paginas 11, 54 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeContadorTotal()
    {
        var resetarContador = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA12,
                EstadoLedsM4.Desligado,
                instrucao: "Confirme o alerta A12: o contador total atingiu o limite configurado. O contador total mede a vida útil da válvula. A12 vem desabilitado por padrão, então esse alerta indica que ele foi habilitado."),
            new QuadroDeDisplayM4(
                Menu,
                EstadoLedsM4.Desligado,
                instrucao: "Aproxime o polo Sul do chaveiro magnético do botão B2 por mais de 6 segundos para entrar no menu.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                MenuConfig,
                EstadoLedsM4.Desligado,
                instrucao: "Após o bargraph, o display mostra MENU CONFIG. Pressione B2 para acessar as configurações."),
            new QuadroDeDisplayM4(
                ResetContadorTotal,
                EstadoLedsM4.Desligado,
                instrucao: "Use B3 para avançar até C19 RESET CONTADOR TOTAL. B1 volta à opção anterior e B2 entra no reset do contador total."),
            new QuadroDeDisplayM4(
                ResetContadorTotal,
                EstadoLedsM4.Desligado,
                instrucao: "IMPORTANTE: o contador total é o medidor de vida útil da válvula e deve ser zerado somente quando o monitor for instalado em uma válvula diferente. O reset não é uma rotina comum de manutenção."),
            new QuadroDeDisplayM4(
                Sim,
                EstadoLedsM4.Desligado,
                instrucao: "Selecione SIM com B1 ou B3 e pressione B2 para confirmar o reset do contador total."),
            new QuadroDeDisplayM4(
                ResetContadorTotal,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: o contador total voltou a zero e o alerta A12 foi eliminado. O contador parcial e as demais configurações não são alterados."));

        var desligarAlerta = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA12,
                EstadoLedsM4.Desligado,
                instrucao: "Identifique A12 no menu de alertas. Esta ação apenas desliga o alerta, sem resetar o contador total, que continua medindo a vida útil da válvula."),
            new QuadroDeDisplayM4(
                Menu,
                EstadoLedsM4.Desligado,
                instrucao: "Aproxime o polo Sul do chaveiro magnético do botão B2 por mais de 6 segundos para entrar no menu.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                MenuConfig,
                EstadoLedsM4.Desligado,
                instrucao: "Após o bargraph, o display mostra MENU CONFIG. Use B3 para avançar até MENU ALERTA."),
            new QuadroDeDisplayM4(
                MenuAlerta,
                EstadoLedsM4.Desligado,
                instrucao: "Em MENU ALERTA, pressione B2 para entrar no menu de alertas."),
            new QuadroDeDisplayM4(
                MenuContadorTotal,
                EstadoLedsM4.Desligado,
                instrucao: "Use B3 para avançar até A12 CONTADOR TOTAL e B2 para entrar na opção do alerta."),
            new QuadroDeDisplayM4(
                Desabilitar,
                EstadoLedsM4.Desligado,
                instrucao: "Selecione DESABILITAR com B1 ou B3 e pressione B2 para desligar o alerta A12."),
            new QuadroDeDisplayM4(
                MenuContadorTotal,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: o alerta A12 foi desligado. Isso não reseta nem altera o contador total ou o parcial."));

        return new PerfilDeDisplayDeAlerta(
            CodigoA12,
            new[] { resetarContador, desligarAlerta },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion

    #region MARK: A13 ALERTA DIAS, paginas 11, 54 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeAlertaDias()
    {
        var resetarAlerta = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA13,
                EstadoLedsM4.Desligado,
                instrucao: "Confirme o alerta A13: o número de dias trabalhados programado para a manutenção foi atingido. O monitor acumula os dias desde que foi energizado na válvula."),
            new QuadroDeDisplayM4(
                Menu,
                EstadoLedsM4.Desligado,
                instrucao: "Aproxime o polo Sul do chaveiro magnético do botão B2 por mais de 6 segundos para entrar no menu.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                MenuConfig,
                EstadoLedsM4.Desligado,
                instrucao: "Após o bargraph, o display mostra MENU CONFIG. Use B3 para avançar até MENU ALERTA."),
            new QuadroDeDisplayM4(
                MenuAlerta,
                EstadoLedsM4.Desligado,
                instrucao: "Em MENU ALERTA, pressione B2 para entrar no menu de alertas."),
            new QuadroDeDisplayM4(
                MenuAlertaDias,
                EstadoLedsM4.Desligado,
                instrucao: "Use B3 para avançar até A13 ALERTA DIAS e B2 para entrar na opção do alerta."),
            new QuadroDeDisplayM4(
                Limpar,
                EstadoLedsM4.Desligado,
                instrucao: "Selecione LIMPAR com B1 ou B3 e pressione B2 para resetar a contagem de dias trabalhados."),
            new QuadroDeDisplayM4(
                MenuAlertaDias,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: a contagem de dias voltou a zero e uma nova contagem foi iniciada. Os contadores de ciclos parcial e total não são alterados."));

        var desligarAlerta = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA13,
                EstadoLedsM4.Desligado,
                instrucao: "Identifique A13 no menu de alertas. Esta ação apenas desliga o alerta, sem resetar a contagem de dias trabalhados."),
            new QuadroDeDisplayM4(
                Menu,
                EstadoLedsM4.Desligado,
                instrucao: "Aproxime o polo Sul do chaveiro magnético do botão B2 por mais de 6 segundos para entrar no menu.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                MenuConfig,
                EstadoLedsM4.Desligado,
                instrucao: "Após o bargraph, o display mostra MENU CONFIG. Use B3 para avançar até MENU ALERTA."),
            new QuadroDeDisplayM4(
                MenuAlerta,
                EstadoLedsM4.Desligado,
                instrucao: "Em MENU ALERTA, pressione B2 para entrar no menu de alertas."),
            new QuadroDeDisplayM4(
                MenuAlertaDias,
                EstadoLedsM4.Desligado,
                instrucao: "Use B3 para avançar até A13 ALERTA DIAS e B2 para entrar na opção do alerta."),
            new QuadroDeDisplayM4(
                Desabilitar,
                EstadoLedsM4.Desligado,
                instrucao: "Selecione DESABILITAR com B1 ou B3 e pressione B2 para desligar o alerta A13."),
            new QuadroDeDisplayM4(
                MenuAlertaDias,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: o alerta A13 foi desligado. A contagem de dias trabalhados continua registrada e não é apagada."));

        return new PerfilDeDisplayDeAlerta(
            CodigoA13,
            new[] { resetarAlerta, desligarAlerta },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion

    #region MARK: A14 ALERTA DATA, paginas 11, 53, 54, 63 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeAlertaData()
    {
        var definirNovaData = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA14,
                EstadoLedsM4.Desligado,
                instrucao: "Alerta A14: a data ajustada foi atingida. Depende do relógio C13 ajustado."),
            new QuadroDeDisplayM4(
                Menu,
                EstadoLedsM4.Desligado,
                instrucao: "Encoste o polo Sul do chaveiro em B2 por 6 segundos.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                MenuConfig,
                EstadoLedsM4.Desligado,
                instrucao: "Avance com B3 até MENU ALERTA."),
            new QuadroDeDisplayM4(
                MenuAlerta,
                EstadoLedsM4.Desligado,
                instrucao: "Pressione B2 para entrar."),
            new QuadroDeDisplayM4(
                MenuAlertaData,
                EstadoLedsM4.Desligado,
                instrucao: "Avance com B3 até A14 e pressione B2."),
            new QuadroDeDisplayM4(
                Habilitar,
                EstadoLedsM4.Desligado,
                instrucao: "Selecione HABILITAR com B3 e pressione B2."),
            new QuadroDeDisplayM4(
                ExemploDeData,
                EstadoLedsM4.Desligado,
                instrucao: "Ajuste a data da manutenção. B1 diminui, B3 aumenta, B2 troca de dígito. Segure B2 por 3 segundos para confirmar."),
            new QuadroDeDisplayM4(
                MenuAlertaData,
                EstadoLedsM4.Desligado,
                instrucao: "Data salva e alerta A14 encerrado. Sair com B1 antes de confirmar não salva nada."));

        var desligarAlerta = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA14,
                EstadoLedsM4.Desligado,
                instrucao: "Alerta A14 ativo. Aqui você só desliga o alerta, sem definir nova data."),
            new QuadroDeDisplayM4(
                Menu,
                EstadoLedsM4.Desligado,
                instrucao: "Encoste o polo Sul do chaveiro em B2 por 6 segundos.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                MenuConfig,
                EstadoLedsM4.Desligado,
                instrucao: "Avance com B3 até MENU ALERTA."),
            new QuadroDeDisplayM4(
                MenuAlerta,
                EstadoLedsM4.Desligado,
                instrucao: "Pressione B2 para entrar."),
            new QuadroDeDisplayM4(
                MenuAlertaData,
                EstadoLedsM4.Desligado,
                instrucao: "Avance com B3 até A14 e pressione B2."),
            new QuadroDeDisplayM4(
                Desabilitar,
                EstadoLedsM4.Desligado,
                instrucao: "Selecione DESABILITAR com B3 e pressione B2."),
            new QuadroDeDisplayM4(
                MenuAlertaData,
                EstadoLedsM4.Desligado,
                instrucao: "Alerta A14 desligado. O relógio C13 não é alterado."));

        return new PerfilDeDisplayDeAlerta(
            CodigoA14,
            new[] { definirNovaData, desligarAlerta },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion

    #region MARK: Verificacao de temperatura compartilhada por A21 e A22

    private static PerfilDeDisplayDeAlerta CriarPerfilDeTemperatura(string codigo, string diagnostico)
    {
        var verificarTemperatura = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Desligado,
                instrucao: diagnostico),
            new QuadroDeDisplayM4(
                Menu,
                EstadoLedsM4.Desligado,
                instrucao: "Encoste o polo Sul do chaveiro em B2 por 6 segundos.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                MenuConfig,
                EstadoLedsM4.Desligado,
                instrucao: "Pressione B2 para entrar."),
            new QuadroDeDisplayM4(
                MenuDisplay,
                EstadoLedsM4.Desligado,
                instrucao: "Avance com B3 até C3 e pressione B2."),
            new QuadroDeDisplayM4(
                Temperatura,
                EstadoLedsM4.Desligado,
                instrucao: "Selecione TEMPERATURA com B3 e pressione B2."),
            new QuadroDeDisplayM4(
                Sair,
                EstadoLedsM4.Desligado,
                instrucao: "Segure B1 por 3 segundos para sair do menu."),
            new QuadroDeDisplayM4(
                Temperatura,
                EstadoLedsM4.Desligado,
                instrucao: "O display mostra a temperatura interna. Verifique a temperatura do processo em campo."));

        return new PerfilDeDisplayDeAlerta(
            codigo,
            new[] { verificarTemperatura },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion

    #region MARK: A21 TEMPERATURA ALTA, paginas 11, 55 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeTemperaturaAlta()
    {
        return CriarPerfilDeTemperatura(
            CodigoA21,
            "Alerta A21: temperatura do monitor acima de 70°.");
    }

    #endregion

    #region MARK: A22 TEMPERATURA BAIXA, paginas 11, 55 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeTemperaturaBaixa()
    {
        return CriarPerfilDeTemperatura(
            CodigoA22,
            "Alerta A22: temperatura do monitor abaixo de -20°C.");
    }

    #endregion

    #region MARK: A1 ANGULO MINIMO, paginas 11, 16, 17, 49 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeAnguloMinimo()
    {
        var verificarArComprimido = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA1,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confirme o alerta A1: a válvula se moveu, mas o ângulo entre o ponto de abertura e o de fechamento ficou abaixo de 30°.",
                animacao: AnimacaoProblema1),
            new QuadroDeDisplayM4(
                CodigoA1,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Verifique no gerador de ar comprimido, fora do monitor: o ar entra pela conexão P, a entrada de ar 1, e vai ao atuador.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA1,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confira a pressão da linha: o monitor opera entre 3 e 8 bar (45 a 120 psi) e sai de fábrica em 6 bar (87 psi)."),
            new QuadroDeDisplayM4(
                CodigoA1,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Respeite o limite: acima de 10 bar (150 psi) o monitor é danificado permanentemente, mesmo em falha do fornecimento."),
            new QuadroDeDisplayM4(
                CodigoA1,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Cheque mangueiras e engates: vazamento derruba a pressão e a válvula para antes do fim do curso. Despressurize antes.",
                vfx: DestaqueMangueiras),
            new QuadroDeDisplayM4(
                CodigoA1,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Use ar comprimido limpo e isento de óleo. Os orifícios pneumáticos internos são pequenos e entopem, encurtando o curso."),
            new QuadroDeDisplayM4(
                CodigoA1,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Se a pressão está no especificado e o curso segue curto, o problema não está na linha. Inspecione o conjunto."));

        var verificarAvariasNoConjunto = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA1,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Com ar suficiente e curso curto, procure obstrução no conjunto. Desconecte o monitor do ar antes de tocar nele.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                CodigoA1,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Inspecione o acoplamento NAMUR: o adaptador de eixo deve encaixar perfeitamente no eixo, de 11 mm a 38 mm de diâmetro.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                CodigoA1,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Procure desgaste nas partes móveis: diafragma, gaxetas e assentos. É o que o contador parcial acompanha.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                CodigoA1,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Em atuador simples ação, confira o tamponamento: NA tampona a saída de ar 4, NF a saída de ar 2. Use tampões metálicos."),
            new QuadroDeDisplayM4(
                CodigoA1,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confira o padrão dos orifícios pneumáticos, NPT ou BSP: conexão incompatível danifica a rosca e restringe a passagem."),
            new QuadroDeDisplayM4(
                CodigoA1,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Resolvida a avaria, refaça a auto calibração para o monitor reaprender os pontos de abertura e de fechamento."),
            new QuadroDeDisplayM4(
                CodigoA1,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: com o curso completo reaprendido, o ângulo supera 30° e o alerta A1 é eliminado."));

        return new PerfilDeDisplayDeAlerta(
            CodigoA1,
            new[] { verificarArComprimido, verificarAvariasNoConjunto },
            mecanismoDeAtivacaoConfirmado: false,
            layer: LayerProblema1);
    }

    #endregion

    #region MARK: A2 TEMPO LIMITE, paginas 11, 16, 49, 51 e 52

    private static PerfilDeDisplayDeAlerta CriarPerfilDeTempoLimite()
    {
        var aumentarTempoLimite = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA2,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confirme o alerta A2: a auto calibração levou mais tempo que o máximo configurado em C6 TEMPO MAX CAL."),
            new QuadroDeDisplayM4(
                Menu,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Aproxime o polo Sul do chaveiro magnético do botão B2 por mais de 6 segundos para entrar no menu.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                MenuConfig,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Após o bargraph, o display mostra MENU CONFIG. Pressione B2 para acessar as configurações."),
            new QuadroDeDisplayM4(
                MenuTempoMaxCal,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Use B3 para avançar até C6 TEMPO MAX CAL. B1 volta à opção anterior e B2 entra na opção."),
            new QuadroDeDisplayM4(
                MenuTempoMaxCal,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Ajuste o valor entre 10 e 120 segundos: B1 diminui, B3 aumenta. Mantenha B2 por mais de 3 segundos para confirmar."),
            new QuadroDeDisplayM4(
                MenuTempoMaxCal,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: o novo tempo máximo de calibração foi salvo e o alerta A2 foi eliminado."),
            new QuadroDeDisplayM4(
                Sair,
                EstadoLedsM4.Desligado,
                instrucao: "Em uma opção principal, mantenha B1 por mais de 3 segundos para sair. O display mostra SAIR e retorna ao modo RUN."));

        var verificarAvariasNoConjunto = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA2,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Com o curso completo mas lento, procure a avaria que freia o conjunto. Desconecte o monitor do ar antes de tocar nele.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                CodigoA2,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Cheque mangueiras e engates: dobra ou engate estreito restringe a passagem de ar e a válvula demora a completar o curso.",
                vfx: DestaqueMangueiras),
            new QuadroDeDisplayM4(
                CodigoA2,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confira a pressão disponível na entrada de ar: entre 3 e 8 bar. Pressão baixa move a válvula, porém devagar demais.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA2,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Inspecione o acoplamento NAMUR: o adaptador de eixo deve encaixar perfeitamente no eixo, de 11 mm a 38 mm de diâmetro.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                CodigoA2,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Procure atrito e emperramento parcial: diafragma, gaxetas e assentos. É o que o contador parcial acompanha.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                CodigoA2,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confira se o atuador não está subdimensionado para a pressão disponível: ele completa o curso, mas sempre com lentidão."),
            new QuadroDeDisplayM4(
                FastSetup,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Refaça a auto calibração: aproxime o polo Norte do chaveiro do botão B3 por 6 segundos. O display mostra FAST SETUP.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                Certo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "O monitor avisa que a válvula vai se mover e mostra CERTO. Aproxime o polo Sul do chaveiro do botão B2 para iniciar."),
            new QuadroDeDisplayM4(
                Certo,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: com o conjunto livre, a calibração termina dentro de C6 e o alerta A2 é eliminado."));

        return new PerfilDeDisplayDeAlerta(
            CodigoA2,
            new[] { aumentarTempoLimite, verificarAvariasNoConjunto },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion

    #region MARK: A3 FALHA NA AUTO CALIBRACAO, paginas 11 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeFalhaNaAutoCalibracao()
    {
        var verificarArComprimido = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA3,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confirme o alerta A3: na auto calibração o monitor gravou pontos de aberto e fechado diferentes entre si."),
            new QuadroDeDisplayM4(
                CodigoA3,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Verifique no gerador de ar comprimido, fora do monitor: o ar entra pela conexão P, a entrada de ar 1, e vai ao atuador.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA3,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confira a pressão da linha: o monitor opera entre 3 e 8 bar (45 a 120 psi) e sai de fábrica em 6 bar (87 psi)."),
            new QuadroDeDisplayM4(
                CodigoA3,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Respeite o limite: acima de 10 bar (150 psi) o monitor é danificado permanentemente, mesmo em falha do fornecimento."),
            new QuadroDeDisplayM4(
                CodigoA3,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Cheque mangueiras e engates da entrada de ar: vazamento derruba a pressão. Despressurize a linha antes de mexer.",
                vfx: DestaqueMangueiras),
            new QuadroDeDisplayM4(
                CodigoA3,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Use ar comprimido limpo e isento de óleo. Os orifícios pneumáticos internos são pequenos e entopem, travando o monitor."),
            new QuadroDeDisplayM4(
                CodigoA3,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Se o fornecimento de ar está no especificado, o problema não está na linha. Inspecione o conjunto válvula / atuador."));

        var verificarAvariasNoConjunto = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA3,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Antes de tocar no conjunto, desconecte o monitor do ar comprimido. Faça toda manutenção com a linha despressurizada.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                CodigoA3,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Inspecione o acoplamento NAMUR: o adaptador de eixo deve encaixar perfeitamente no eixo, de 11 mm a 38 mm de diâmetro.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                CodigoA3,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Procure desgaste nas partes móveis: diafragma, gaxetas e assentos. É o que o contador parcial acompanha.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                CodigoA3,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Em atuador simples ação, confira o tamponamento: NA tampona a saída de ar 4, NF a saída de ar 2. Use tampões metálicos."),
            new QuadroDeDisplayM4(
                FastSetup,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Refaça a auto calibração: aproxime o polo Norte do chaveiro do botão B3 por 6 segundos. O display mostra FAST SETUP.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                Certo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "O monitor avisa que a válvula vai se mover e mostra CERTO. Aproxime o polo Sul do chaveiro do botão B2 para iniciar."),
            new QuadroDeDisplayM4(
                Abortar,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Para interromper a calibração, aproxime o polo Norte do chaveiro do botão B1 por 3 segundos. O display mostra ABORT.",
                progressoSegundos: 3f),
            new QuadroDeDisplayM4(
                Certo,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: com os pontos iguais em todos os ciclos, a calibração conclui e o alerta A3 é eliminado."));

        return new PerfilDeDisplayDeAlerta(
            CodigoA3,
            new[] { verificarArComprimido, verificarAvariasNoConjunto },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion

    #region MARK: A4 FORA DE FAIXA, paginas 11, 16, 49, 53 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeForaDeFaixa()
    {
        var verificarArComprimido = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA4,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confirme o alerta A4: a pressão de alimentação subiu acima da usada na calibração e a válvula avança 5° além do ponto."),
            new QuadroDeDisplayM4(
                CodigoA4,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Verifique no gerador de ar comprimido, fora do monitor: o ar entra pela conexão P, a entrada de ar 1, e vai ao atuador.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA4,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Meça a pressão na entrada de ar: o monitor opera entre 3 e 8 bar (45 a 120 psi) e sai de fábrica em 6 bar (87 psi).",
                vfx: DestaqueMangueiras),
            new QuadroDeDisplayM4(
                CodigoA4,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Respeite o limite: acima de 10 bar (150 psi) o monitor é danificado permanentemente, mesmo em falha do fornecimento."),
            new QuadroDeDisplayM4(
                CodigoA4,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Primeira saída: regule o gerador de volta à pressão da calibração e o A4 é eliminado sem mexer no monitor.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                Menu,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Segunda saída, se a pressão nova veio para ficar: aproxime o polo Sul do chaveiro do botão B2 por mais de 6 segundos.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                MenuConfig,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Após o bargraph o display mostra MENU CONFIG. Pressione B2 para acessar as configurações."),
            new QuadroDeDisplayM4(
                MenuPressaoDaLinha,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Use B3 para avançar até C17 PRESSÃO DA LINHA. B1 volta à opção anterior e B2 entra na opção."),
            new QuadroDeDisplayM4(
                MenuPressaoDaLinha,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Informe a pressão que a linha passou a ter: B1 diminui e B3 aumenta, entre 3 e 8 bar. Mantenha B2 por 3 segundos."),
            new QuadroDeDisplayM4(
                FastSetup,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Refaça a auto calibração: aproxime o polo Norte do chaveiro do botão B3 por 6 segundos. O display mostra FAST SETUP.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                Certo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "O monitor avisa que a válvula vai se mover e mostra CERTO. Aproxime o polo Sul do chaveiro do botão B2 para iniciar."),
            new QuadroDeDisplayM4(
                Certo,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: com os pontos reaprendidos na pressão atual, a válvula fica na faixa e o A4 é eliminado."));

        return new PerfilDeDisplayDeAlerta(
            CodigoA4,
            new[] { verificarArComprimido },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion

    #region MARK: A5 SEM MOVIMENTO, paginas 11, 13, 16, 17, 49 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeSemMovimento()
    {
        var verificarArComprimido = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA5,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confirme o alerta A5: na calibração o monitor não registrou movimento nenhum. É ausência de movimento, não curso curto."),
            new QuadroDeDisplayM4(
                CodigoA5,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Verifique no gerador de ar comprimido, fora do monitor. O ar entra pela conexão P e sai pela saída 4 e pela saída 2.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA5,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confira a pressão na entrada de ar: o monitor opera entre 3 e 8 bar (45 a 120 psi) e sai de fábrica em 6 bar (87 psi)."),
            new QuadroDeDisplayM4(
                CodigoA5,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Respeite o limite: acima de 10 bar (150 psi) o monitor é danificado permanentemente, mesmo em falha do fornecimento."),
            new QuadroDeDisplayM4(
                CodigoA5,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Siga as mangueiras até as portas do atuador: desconectada, invertida ou dobrada, ela bloqueia o ar. Despressurize antes.",
                vfx: DestaqueMangueiras),
            new QuadroDeDisplayM4(
                CodigoA5,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Use ar comprimido limpo e isento de óleo. Os orifícios pneumáticos internos são pequenos e podem entupir o monitor."),
            new QuadroDeDisplayM4(
                CodigoA5,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Se a pressão chega às portas do atuador e a válvula não se move, o problema não está na linha. Inspecione o conjunto."));

        var verificarAvariasNoConjunto = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA5,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Com ar disponível e a válvula parada, procure travamento ou acoplamento solto. Desconecte o ar antes de tocar nela.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                CodigoA5,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Inspecione o acoplamento NAMUR: o adaptador de eixo deve encaixar perfeitamente no eixo, de 11 mm a 38 mm de diâmetro.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                CodigoA5,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Procure emperramento nas partes móveis: diafragma, gaxetas e assentos. É o que o contador parcial acompanha.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                CodigoA5,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Em atuador simples ação, confira o tamponamento: NA tampona a saída de ar 4, NF a saída de ar 2. Use tampões metálicos."),
            new QuadroDeDisplayM4(
                FastSetup,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Refaça a auto calibração: aproxime o polo Norte do chaveiro do botão B3 por 6 segundos. O display mostra FAST SETUP.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                Certo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "O monitor avisa que a válvula vai se mover e mostra CERTO. Aproxime o polo Sul do chaveiro do botão B2 para iniciar."),
            new QuadroDeDisplayM4(
                Abortar,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Para interromper a calibração, aproxime o polo Norte do chaveiro do botão B1 por 3 segundos. O display mostra ABORT.",
                progressoSegundos: 3f),
            new QuadroDeDisplayM4(
                Certo,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: com a válvula percorrendo o curso completo, a calibração conclui e o alerta A5 é eliminado."));

        return new PerfilDeDisplayDeAlerta(
            CodigoA5,
            new[] { verificarArComprimido, verificarAvariasNoConjunto },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion

    #region MARK: A9 MAX. PRESSAO NA LINHA, paginas 5, 11, 16, 51, 53, 54 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeMaxPressaoNaLinha()
    {
        var verificarArComprimido = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA9,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confirme o alerta A9: a pressão da linha ultrapassou 9 bar. O A9 é sempre ligado e não se desliga pelo menu."),
            new QuadroDeDisplayM4(
                CodigoA9,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "IMPORTANTE: há risco de danos ao equipamento se a pressão da linha exceder 9 bar (130,5 psi). Trate como risco imediato."),
            new QuadroDeDisplayM4(
                CodigoA9,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Respeite o limite: acima de 10 bar (150 psi) o monitor é danificado permanentemente, mesmo em falha do fornecimento."),
            new QuadroDeDisplayM4(
                CodigoA9,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Verifique no gerador de ar comprimido, fora do monitor: o ar entra pela conexão P, a entrada de ar 1, e vai ao atuador.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA9,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Desligue a pressão da linha antes de mexer em qualquer mangueira. Com a linha desligada, confira todas as conexões.",
                vfx: DestaqueMangueiras),
            new QuadroDeDisplayM4(
                CodigoA9,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Regule o gerador de volta para 3 a 8 bar (45 a 120 psi). O padrão é 6 bar (87 psi). Acima de 9 bar o A9 segue ativo.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA9,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "O A17 ALTA PRESSÃO avisa antes: dispara 20% acima da pressão ajustada, ou seja, 7,2 bar (104 psi) para 6 bar."),
            new QuadroDeDisplayM4(
                Menu,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confira qual pressão o monitor tem como referência: aproxime o polo Sul do chaveiro do botão B2 por mais de 6 segundos.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                MenuConfig,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Após o bargraph o display mostra MENU CONFIG. Pressione B2 para acessar as configurações."),
            new QuadroDeDisplayM4(
                MenuPressaoDaLinha,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Use B3 para avançar até C17 PRESSÃO DA LINHA. B1 volta à opção anterior e B2 entra na opção."),
            new QuadroDeDisplayM4(
                MenuPressaoDaLinha,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Deixe C17 igual à pressão do gerador: B1 diminui e B3 aumenta, entre 3 e 8 bar. Mantenha B2 por mais de 3 segundos."),
            new QuadroDeDisplayM4(
                Sair,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Em uma opção principal, mantenha B1 por mais de 3 segundos para sair. O display mostra SAIR e volta ao modo RUN."),
            new QuadroDeDisplayM4(
                CodigoA9,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: com a pressão da linha abaixo de 9 bar, o A9 é eliminado e o LED vermelho para de piscar."));

        return new PerfilDeDisplayDeAlerta(
            CodigoA9,
            new[] { verificarArComprimido },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion

    #region MARK: A6 FALHA DE COMANDO, paginas 11, 17, 18, 19, 20, 22, 23, 76 e 77

    private static PerfilDeDisplayDeAlerta CriarPerfilDeFalhaDeComando()
    {
        var verificarDefeitoNaSolenoide = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confirme o alerta A6: a válvula não executa o comando enviado à solenoide. O monitor manda o sinal e nada acontece."),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Falha de comando é a válvula não responder ao sinal do monitor. A causa pode ser elétrica ou pneumática."),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "PERIGO: desligue a fonte de energia e impeça o religamento antes de abrir qualquer compartimento do monitor."),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Mesmo desligado, os circuitos guardam energia residual. Aguarde antes de encostar em fios ou terminais."),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Desconecte o monitor do ar comprimido antes de qualquer manutenção na válvula.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Para chegar à bobina, afrouxe os quatro parafusos que prendem a válvula ao corpo e puxe a válvula para fora.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Desconecte os fios da solenoide, afrouxe os dois parafusos de fixação da bobina e remova a bobina do compartimento.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Na bobina de troca, confira o anel de vedação em seu lugar: sem ele a válvula vaza ao ser fechada."),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Recoloque a bobina, aperte os parafusos, refaça os fios e encaixe a válvula no invólucro com os quatro parafusos.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Com a bobina íntegra e a válvula ainda sem responder, o defeito está no sinal elétrico que a comanda."));

        var verificarSinalEletrico = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Com a bobina íntegra, o A6 aponta falta de sinal elétrico: o comando não chega até a solenoide.",
                vfx: DestaqueModuloEletronico),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "O comando vem da saída do controlador lógico e entra no monitor pelo terminal aparafusável interno.",
                vfx: DestaqueModuloEletronico),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "PERIGO: desligue a fonte e impeça o religamento antes de mexer na fiação, sob risco de choque e curto circuito."),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Na versão Reed, o comando da solenoide chega pelos terminais S1+ e S1- do borne interno, junto do GND."),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Na versão IO-Link, a solenoide é alimentada por V+ e V- do mesmo borne. Confira também o conector M12."),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Reaperte os fios do borne com uma chave de bornes adequada e confira as cores contra o diagrama do seu modelo.",
                vfx: DestaqueModuloEletronico),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "No prensa cabos, passe o cabo pela porca e pela borracha de vedação; cabo mal preso solta o fio do terminal."),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Na solenoide externa, gire a tampa do compartimento no sentido anti-horário para destravar e revise os bornes."),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "O módulo é encapsulado em resina e não deve ser aberto. Se a fiação está boa, substitua o módulo eletrônico.",
                vfx: DestaqueModuloEletronico),
            new QuadroDeDisplayM4(
                CodigoA6,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: com o comando chegando e a solenoide sã, a válvula responde e o alerta A6 é eliminado."));

        return new PerfilDeDisplayDeAlerta(
            CodigoA6,
            new[] { verificarDefeitoNaSolenoide, verificarSinalEletrico },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion

    #region MARK: A23 SOLENOIDE CURTO, paginas 11, 18, 19, 55 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeSolenoideCurto()
    {
        var verificarOsFios = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA23,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confirme o alerta A23: a bobina da solenoide está em curto circuito. Há caminho fechado onde não deveria haver."),
            new QuadroDeDisplayM4(
                CodigoA23,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Curto é o oposto de circuito aberto: aqui sobra corrente. Se faltasse continuidade, o alerta seria o A24."),
            new QuadroDeDisplayM4(
                CodigoA23,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "PERIGO: desligue a fonte e impeça o religamento antes de tocar na fiação, sob risco de choque e curto circuito."),
            new QuadroDeDisplayM4(
                CodigoA23,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Mesmo desligada a fonte, os circuitos guardam energia residual. Aguarde antes de manusear os fios."),
            new QuadroDeDisplayM4(
                CodigoA23,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Desconecte o monitor do ar comprimido antes de qualquer manutenção na válvula."),
            new QuadroDeDisplayM4(
                CodigoA23,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Procure sinais de curto nos fios da solenoide: isolamento derretido, marcas de queima e cobre à mostra.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA23,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "No borne, um fio solto ou um fiapo de cobre atravessando dois terminais fecha o curto. Reaperte com chave de bornes.",
                vfx: DestaqueModuloEletronico),
            new QuadroDeDisplayM4(
                CodigoA23,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Se o isolamento da bobina está queimado, troque a bobina: afrouxe os quatro parafusos e puxe a válvula do corpo.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA23,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Desabilitar o A23 no menu apaga só a notificação no aplicativo. A indicação física no monitor continua ativa."),
            new QuadroDeDisplayM4(
                CodigoA23,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: sem caminho fechado na bobina, o A23 é eliminado e o LED vermelho para de piscar."));

        return new PerfilDeDisplayDeAlerta(
            CodigoA23,
            new[] { verificarOsFios },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion

    #region MARK: A24 SOLENOIDE ABERTA, paginas 11, 18, 19, 55 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeSolenoideAberta()
    {
        var verificarOsFios = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA24,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confirme o alerta A24: a bobina da solenoide ou o cabo dela está rompido. O circuito ficou aberto."),
            new QuadroDeDisplayM4(
                CodigoA24,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Aberto é o oposto de curto: aqui falta continuidade. Se os fios estivessem fechados entre si, o alerta seria o A23."),
            new QuadroDeDisplayM4(
                CodigoA24,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "PERIGO: desligue a fonte e impeça o religamento antes de tocar na fiação, sob risco de choque e curto circuito."),
            new QuadroDeDisplayM4(
                CodigoA24,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Mesmo desligada a fonte, os circuitos guardam energia residual. Aguarde antes de manusear os fios."),
            new QuadroDeDisplayM4(
                CodigoA24,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Siga o cabo da solenoide de ponta a ponta: procure rompimento, esmagamento e dobra na saída do prensa cabos.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA24,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confira a continuidade entre os dois fios da bobina. Sem continuidade, a bobina ou o cabo está partido.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA24,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "No borne, um fio que saiu do terminal aparafusável abre o circuito. Reaperte com uma chave de bornes adequada.",
                vfx: DestaqueModuloEletronico),
            new QuadroDeDisplayM4(
                CodigoA24,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Cabo íntegro e bobina sem continuidade: troque a bobina, afrouxando os dois parafusos que a prendem.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA24,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Antes de fechar, confira o anel de vedação da nova bobina; sem ele a válvula passa a vazar ar."),
            new QuadroDeDisplayM4(
                CodigoA24,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Desabilitar o A24 no menu apaga só a notificação no aplicativo. A indicação física no monitor continua ativa."),
            new QuadroDeDisplayM4(
                CodigoA24,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: com a continuidade restabelecida, o A24 é eliminado e o LED vermelho para de piscar."));

        return new PerfilDeDisplayDeAlerta(
            CodigoA24,
            new[] { verificarOsFios },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion

    #region MARK: A25 SAIDA CURTO, paginas 11, 18, 23, 55 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeSaidaCurto()
    {
        var verificarAsSaidas = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA25,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confirme o alerta A25: uma das saídas PNP, ou a carga ligada a ela, está em curto.",
                vfx: DestaqueModuloEletronico),
            new QuadroDeDisplayM4(
                CodigoA25,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Este alerta existe apenas no modelo IO-Link. Os demais módulos não têm saída PNP para monitorar."),
            new QuadroDeDisplayM4(
                CodigoA25,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "A saída 1 indica a válvula aberta e a saída 2 indica a válvula fechada, ambas para o cartão de entrada do CLP."),
            new QuadroDeDisplayM4(
                CodigoA25,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "PERIGO: desligue a fonte e impeça o religamento antes de mexer nos fios das saídas, sob risco de choque."),
            new QuadroDeDisplayM4(
                CodigoA25,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Cada saída PNP chaveia no máximo 50 mA. Carga acima disso, ou em curto, derruba a saída e dispara o alerta."),
            new QuadroDeDisplayM4(
                CodigoA25,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "No painel de controle, confira a carga de cada saída: relé, sinaleiro ou entrada do CLP em curto."),
            new QuadroDeDisplayM4(
                CodigoA25,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "As saídas PNP enviam sinal positivo e devem ir a cartão de entrada SINK, que tem o negativo como comum."),
            new QuadroDeDisplayM4(
                CodigoA25,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Revise o conector M12 e o terminal aparafusável interno: pino torto, fiapo de cobre ou umidade fecham o curto.",
                vfx: DestaqueModuloEletronico),
            new QuadroDeDisplayM4(
                CodigoA25,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Desabilitar o A25 no menu apaga só a notificação no aplicativo. A indicação física no monitor continua ativa."),
            new QuadroDeDisplayM4(
                CodigoA25,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: com as saídas e suas cargas livres de curto, o A25 é eliminado e o LED para de piscar."));

        return new PerfilDeDisplayDeAlerta(
            CodigoA25,
            new[] { verificarAsSaidas },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion

    #region MARK: Verificacao da fonte compartilhada por A19 e A20, paginas 11, 12, 18, 21, 22, 23, 51, 55, 76 e 77

    private static PerfilDeDisplayDeAlerta CriarPerfilDeAlimentacao(string codigo, string diagnostico)
    {
        var verificarFonteDeAlimentacao = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: diagnostico),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "A faixa aceita depende da versão do módulo: AS-Interface, DeviceNet ou PNP com IO-Link."),
            new QuadroDeDisplayM4(
                Menu,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Encoste o polo Sul do chaveiro em B2 por 6 segundos.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                MenuConfig,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Avance com B3 até MENU DEVICE."),
            new QuadroDeDisplayM4(
                MenuDevice,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Pressione B2 para entrar. Este menu só mostra informações, não altera nenhum parâmetro."),
            new QuadroDeDisplayM4(
                PartNumber,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Selecione d1 PART NUMBER com B3 e pressione B2."),
            new QuadroDeDisplayM4(
                PartNumber,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "O part number no display diz qual módulo você tem e, com ele, qual faixa de tensão vale aqui."),
            new QuadroDeDisplayM4(
                Sair,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Segure B1 por 3 segundos para sair do menu.",
                progressoSegundos: 3f),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "PERIGO: desligue a fonte e impeça o religamento antes de abrir o compartimento elétrico."),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Mesmo desligada a fonte, os circuitos guardam energia residual. Aguarde antes de manusear os fios."),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "A alimentação entra pelo prensa cabos ou pelo conector M12 e termina no terminal aparafusável interno.",
                vfx: DestaqueModuloEletronico),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confira V+ e V- no borne: terminal frouxo, cabo esmagado ou umidade alteram a tensão que chega ao módulo.",
                vfx: DestaqueModuloEletronico),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "A conferência principal é fora do monitor: vá ao painel de controle e inspecione a fonte que o alimenta."),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Fonte mal dimensionada, ou dividida com outros equipamentos, tira a tensão da faixa aceita pelo módulo."),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Desabilitar os alertas de A19 a A25 apaga só a notificação no aplicativo. O monitor segue indicando."),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: com a tensão dentro da faixa do módulo, o alerta cai e o LED para de piscar."));

        return new PerfilDeDisplayDeAlerta(
            codigo,
            new[] { verificarFonteDeAlimentacao },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion

    #region MARK: A19 ALIMENTACAO ALTA, paginas 11, 12, 55 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeAlimentacaoAlta()
    {
        return CriarPerfilDeAlimentacao(
            CodigoA19,
            "Confirme o alerta A19: tensão acima de 32Vcc em AS-Interface, ou 10% acima em PNP e DeviceNet.");
    }

    #endregion

    #region MARK: A20 ALIMENTACAO BAIXA, paginas 11, 12, 55 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeAlimentacaoBaixa()
    {
        return CriarPerfilDeAlimentacao(
            CodigoA20,
            "Confirme o alerta A20: tensão abaixo de 27V em AS-Interface, ou de 22,8V em DeviceNet e PNP.");
    }

    #endregion

    #region MARK: Verificacao da pressao de entrada compartilhada por A17 e A18, paginas 11, 16, 17, 51, 53, 54, 55 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDePressaoDeEntrada(
        string codigo,
        string diagnostico,
        string aberturaDoDesligamento,
        string menuDoAlerta)
    {
        var verificarArComprimido = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: diagnostico),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "O percentual do alerta é medido sobre a pressão definida em C17 PRESSÃO DA LINHA, e não sobre um valor fixo."),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "C17 vai de 3 a 8 bar (45 a 120 psi) e sai de fábrica em 6 bar (87 psi). O padrão do alerta é 20% desse valor."),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Com C17 em 6 bar e o padrão de 20%, a faixa vigiada vai de 4,8 a 7,2 bar (70 a 104 psi)."),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "A verificação principal é fora do monitor: vá ao gerador de ar comprimido que alimenta a entrada de ar 1.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Desligue a pressão da linha antes de mexer nas mangueiras. Vazamento, estrangulamento ou engate solto mudam a pressão.",
                vfx: DestaqueMangueiras),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Limite absoluto: passar de 10 bar (150 psi) danifica o monitor permanentemente. O A9 vigia a linha em 9 bar."),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Regule o gerador para a mesma pressão ajustada em C17, sempre dentro de 3 a 8 bar (45 a 120 psi).",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                Menu,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Encoste o polo Sul do chaveiro em B2 por mais de 6 segundos para conferir a referência do monitor.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                MenuConfig,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Após o bargraph o display mostra MENU CONFIG. Pressione B2 para acessar as configurações."),
            new QuadroDeDisplayM4(
                MenuPressaoDaLinha,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Use B3 para avançar até C17 PRESSÃO DA LINHA. B1 volta à opção anterior e B2 entra na opção."),
            new QuadroDeDisplayM4(
                MenuPressaoDaLinha,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Deixe C17 igual à pressão real do gerador: B1 diminui e B3 aumenta. Mantenha B2 pressionado para gravar o valor."),
            new QuadroDeDisplayM4(
                Sair,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Segure B1 por 3 segundos para sair do menu.",
                progressoSegundos: 3f),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: com a pressão de entrada de volta à faixa de C17, o alerta cai e o LED para de piscar."));

        var desligarAlerta = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: aberturaDoDesligamento),
            new QuadroDeDisplayM4(
                Menu,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Encoste o polo Sul do chaveiro em B2 por mais de 6 segundos para entrar no menu.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                MenuConfig,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Após o bargraph o display mostra MENU CONFIG. Use B3 para avançar até MENU ALERTA."),
            new QuadroDeDisplayM4(
                MenuAlerta,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Em MENU ALERTA, pressione B2 para entrar na lista de alertas configuráveis."),
            new QuadroDeDisplayM4(
                menuDoAlerta,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Avance com B3 até o alerta de pressão indicado no display e pressione B2 para entrar."),
            new QuadroDeDisplayM4(
                Desabilitar,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Selecione DESABILITAR com B1 ou B3 e pressione B2 para desligar este alerta."),
            new QuadroDeDisplayM4(
                menuDoAlerta,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: o alerta foi desligado. Isso não muda a pressão de C17 nem o A9, que é sempre ligado."));

        return new PerfilDeDisplayDeAlerta(
            codigo,
            new[] { verificarArComprimido, desligarAlerta },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion

    #region MARK: A17 ALTA PRESSAO, paginas 11, 16, 53, 54 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDePressaoAlta()
    {
        return CriarPerfilDePressaoDeEntrada(
            CodigoA17,
            "Confirme o alerta A17: a pressão de entrada passou do limite alto ajustado sobre o valor de C17 PRESSÃO DA LINHA.",
            "No A17 o alerta dispara com a pressão de entrada 20%, 30%, 40% ou 50% acima da pressão definida em C17.",
            MenuPressaoAlta);
    }

    #endregion

    #region MARK: A18 BAIXA PRESSAO, paginas 11, 16, 53, 55 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDePressaoBaixa()
    {
        return CriarPerfilDePressaoDeEntrada(
            CodigoA18,
            "Confirme o alerta A18: a pressão de entrada caiu abaixo do limite ajustado sobre o valor de C17 PRESSÃO DA LINHA.",
            "No A18 o alerta dispara com a pressão de entrada 20%, 30%, 40% ou 50% abaixo da pressão definida em C17.",
            MenuPressaoBaixa);
    }

    #endregion

    #region MARK: Tempo de curso aprendido na calibracao compartilhado por A15 e A16, paginas 11, 12, 16, 49, 51, 52, 54 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeTempoDeCurso(
        string codigo,
        string diagnostico,
        string aberturaDoConjunto,
        string aberturaDoDesligamento,
        string menuDoAlerta)
    {
        var verificarArComprimido = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: diagnostico),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "O alerta não tem tempo fixo: a referência é o tempo de curso que o monitor aprendeu no ciclo de auto calibração."),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "O disparo ocorre quando esse tempo aprendido é excedido em 20%, 30%, 40% ou 50%, conforme o percentual configurado."),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Se a calibração foi feita em outras condições, a referência está defasada e o alerta acusa atraso mesmo sem avaria."),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Comece pelo gerador de ar comprimido, fora do monitor: o ar entra pela conexão P, a entrada de ar 1, e vai ao atuador.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confira a pressão da linha: o monitor opera entre 3 e 8 bar (45 a 120 psi) e sai de fábrica em 6 bar (87 psi)."),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Respeite o limite: acima de 10 bar (150 psi) o monitor é danificado permanentemente, mesmo em falha do fornecimento."),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Cheque mangueiras e engates: dobra, vazamento ou engate estreito reduz a vazão e atrasa o curso. Despressurize antes.",
                vfx: DestaqueMangueiras),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Use ar comprimido limpo e isento de óleo. Os orifícios pneumáticos internos são pequenos e entopem, freando o curso."),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Regule o gerador para a mesma pressão usada na calibração: com a vazão de volta, o curso cabe no tempo aprendido.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: com o fornecimento de ar restabelecido, o curso termina no prazo e o LED para de piscar."));

        var verificarAvariasNoConjunto = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: aberturaDoConjunto,
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Inspecione o acoplamento NAMUR: o adaptador de eixo deve encaixar perfeitamente no eixo, de 11 mm a 38 mm de diâmetro.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Procure desgaste nas partes móveis: diafragma, gaxetas e assentos. É o que o contador parcial acompanha.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Em atuador simples ação, confira o tamponamento: NA tampona a saída de ar 4, NF a saída de ar 2. Use tampões metálicos."),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Siga as mangueiras até as portas do atuador: engate solto ou mangueira dobrada rouba vazão e alonga o curso.",
                vfx: DestaqueMangueiras),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Resolvida a avaria, refaça a auto calibração para o monitor reaprender o tempo de curso nas condições atuais."),
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "C5 AUTO CAL define em quantos ciclos o monitor aprende posições, tempos e ângulos: 3, 5 ou 10 ciclos."),
            new QuadroDeDisplayM4(
                FastSetup,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Refaça a auto calibração: aproxime o polo Norte do chaveiro do botão B3 por 6 segundos. O display mostra FAST SETUP.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                Certo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "O monitor avisa que a válvula vai se mover e mostra CERTO. Aproxime o polo Sul do chaveiro do botão B2 para iniciar."),
            new QuadroDeDisplayM4(
                Abortar,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Para interromper a calibração, aproxime o polo Norte do chaveiro do botão B1 por 3 segundos. O display mostra ABORT.",
                progressoSegundos: 3f),
            new QuadroDeDisplayM4(
                Certo,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: com o conjunto livre e o tempo reaprendido, o curso cabe na referência e o alerta cai."));

        var desligarOAlerta = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                codigo,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: aberturaDoDesligamento),
            new QuadroDeDisplayM4(
                Menu,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Encoste o polo Sul do chaveiro em B2 por mais de 6 segundos para entrar no menu.",
                progressoSegundos: 6f),
            new QuadroDeDisplayM4(
                MenuConfig,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Após o bargraph o display mostra MENU CONFIG. Use B3 para avançar até MENU ALERTA."),
            new QuadroDeDisplayM4(
                MenuAlerta,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Em MENU ALERTA, pressione B2 para entrar na lista de alertas configuráveis."),
            new QuadroDeDisplayM4(
                menuDoAlerta,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Avance com B3 até o alerta de tempo indicado no display e pressione B2 para entrar."),
            new QuadroDeDisplayM4(
                Desabilitar,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Selecione DESABILITAR com B1 ou B3 e pressione B2 para desligar este alerta."),
            new QuadroDeDisplayM4(
                menuDoAlerta,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: o alerta foi desligado. O monitor segue medindo o curso, mas não avisa mais o atraso."));

        return new PerfilDeDisplayDeAlerta(
            codigo,
            new[] { verificarArComprimido, verificarAvariasNoConjunto, desligarOAlerta },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion

    #region MARK: A15 TEMPO ABERTURA, paginas 11, 12, 49, 52, 54 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeTempoDeAbertura()
    {
        return CriarPerfilDeTempoDeCurso(
            CodigoA15,
            "Confirme o alerta A15: a válvula abriu por completo, mas gastou mais tempo do que o aprendido na calibração.",
            "Com ar suficiente e a abertura ainda lenta, procure no conjunto o atrito que atrasa o curso. Desconecte o ar antes.",
            "No A15 o alerta dispara quando o tempo de abertura passa em 20%, 30%, 40% ou 50% do tempo aprendido na calibração.",
            MenuTempoAbertura);
    }

    #endregion

    #region MARK: A16 TEMPO FECHAMENTO, paginas 11, 12, 49, 52, 54 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeTempoDeFechamento()
    {
        return CriarPerfilDeTempoDeCurso(
            CodigoA16,
            "Confirme o alerta A16: a válvula fechou por completo, mas gastou mais tempo do que o aprendido na calibração.",
            "Com ar suficiente e o fechamento ainda lento, procure no conjunto o atrito que atrasa o curso. Desconecte o ar antes.",
            "No A16 o alerta dispara quando o tempo de fechamento passa em 20%, 30%, 40% ou 50% do tempo aprendido na calibração.",
            MenuTempoFechamento);
    }

    #endregion

    #region MARK: A7 MUDANCA INESPERADA, paginas 7, 11, 16, 17, 48 e 76

    private static PerfilDeDisplayDeAlerta CriarPerfilDeMudancaInesperada()
    {
        var verificarAcionamentoManual = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA7,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confirme o alerta A7: a válvula mudou de posição sem comando da solenoide. Não é falha de resposta, é movimento sozinho.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                CodigoA7,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "A posição é monitorada sem contato: o ímã do indicador local e o sensor Hall registram todo giro do eixo."),
            new QuadroDeDisplayM4(
                CodigoA7,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Vá ao corpo da válvula, em campo, e procure o acionador manual com trava. Ele move a válvula sem a solenoide.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                CodigoA7,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "O acionador manual é operado com chave de fenda: pressione e gire o botão. Veja se ele ficou pressionado ou travado.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                CodigoA7,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Se estiver travado, destrave o acionador. A válvula volta ao estado comandado e para de se mover por conta própria.",
                vfx: DestaqueAtuadorCopo),
            new QuadroDeDisplayM4(
                CodigoA7,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Forçar a solenoide pede B1 e confirmação em B2; o display mostra Forc Aberta. Sem essa mensagem, não houve comando."),
            new QuadroDeDisplayM4(
                CodigoA7,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "O manual avisa: forçada pelo acionador físico, a solenoide não gera mensagem no display. Por isso o A7 aparece sozinho."),
            new QuadroDeDisplayM4(
                CodigoA7,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "A solenoide também pode ser forçada pelo sistema de controle ou pelo software. Confirme com quem opera a rede."),
            new QuadroDeDisplayM4(
                CodigoA7,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Sem acionamento manual e sem comando forçado, o movimento veio pelo ar. Verifique as conexões pneumáticas."));

        var verificarConexoesPneumaticas = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA7,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Descartado o acionamento manual, o ar moveu a válvula sem comando. Procure a falha nas conexões pneumáticas.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA7,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confira a ligação das portas: P é a entrada de ar 1, a saída de ar 4 abre o atuador e a saída de ar 2 fecha.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA7,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Mangueiras trocadas entre as saídas 4 e 2 invertem o sentido: a válvula assume a posição contrária sem receber comando.",
                vfx: DestaqueMangueiras),
            new QuadroDeDisplayM4(
                CodigoA7,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Procure vazamento nos engates: ar chegando ao atuador com a solenoide desenergizada empurra a válvula sozinho.",
                vfx: DestaqueMangueiras),
            new QuadroDeDisplayM4(
                CodigoA7,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Desligue a linha antes de conectar ou desconectar qualquer mangueira da válvula do monitor.",
                vfx: DestaqueMangueiras),
            new QuadroDeDisplayM4(
                CodigoA7,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Em atuador simples ação, confira o tamponamento: NA tampona a saída de ar 4, NF a saída de ar 2. Use tampões metálicos.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA7,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confira o padrão dos orifícios pneumáticos, NPT ou BSP: conexão incompatível danifica a rosca e deixa vazar.",
                vfx: DestaquePneumatica),
            new QuadroDeDisplayM4(
                CodigoA7,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: sem acionador travado e com as conexões corretas, a válvula só se move sob comando e o A7 sai."));

        return new PerfilDeDisplayDeAlerta(
            CodigoA7,
            new[] { verificarAcionamentoManual, verificarConexoesPneumaticas },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion
}
