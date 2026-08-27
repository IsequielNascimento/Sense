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
    public const string CodigoA9 = "A9";
    public const string CodigoA14 = "A14";
    public const string MenuAlertaData = "A14\nALERTA";
    public const string ExemploDeData = "31 12\n2023";
    public const string CodigoA21 = "A21";
    public const string CodigoA22 = "A22";
    public const string MenuDisplay = "C3\nDISPLA";
    public const string Temperatura = "TEMPER";
    public const string MenuTempoMaxCal = "C6";

    #endregion

    #region MARK: Animacoes do modelo M4

    public const string AnimacaoProblema1 = "PROBLEMA1";
    public const string LayerProblema1 = "Problema 1";

    #endregion

    #region MARK: VFX do modelo M4

    public const string DestaqueAtuadorCopo = "DestaqueAtuadorCopo";
    public const string DestaquePneumatica = "DestaquePneumatica";
    public const string DestaqueMangueiras = "DestaqueMangueiras";

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

    #region MARK: A1 ANGULO MINIMO, pagina 11

    private static PerfilDeDisplayDeAlerta CriarPerfilDeAnguloMinimo()
    {
        var verificarArComprimido = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA1,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: VerificarArComprimido,
                animacao: AnimacaoProblema1));

        var verificarAvariasNoConjunto = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA1,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: VerificarAvariasNoConjunto,
                vfx: DestaqueAtuadorCopo));

        return new PerfilDeDisplayDeAlerta(
            CodigoA1,
            new[] { verificarArComprimido, verificarAvariasNoConjunto },
            mecanismoDeAtivacaoConfirmado: false,
            layer: LayerProblema1);
    }

    #endregion

    #region MARK: A2 TEMPO LIMITE, paginas 11 e 52

    private static PerfilDeDisplayDeAlerta CriarPerfilDeTempoLimite()
    {
        var aumentarTempoLimite = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA2,
                EstadoLedsM4.Alerta,
                ledPiscando: true,
                instrucao: "Confirme o alerta A2: o tempo máximo de calibração (C6) foi excedido. O monitor demorou mais que o configurado para mover a válvula durante a auto calibração."),
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
                instrucao: "Aumente o tempo máximo de calibração: B1 diminui e B3 aumenta o valor entre 10 e 120 segundos. Mantenha B2 por mais de 3 segundos para confirmar."),
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
                instrucao: VerificarAvariasNoConjunto,
                vfx: DestaqueAtuadorCopo));

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
}
