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
    public const string CodigoA14 = "A14";
    public const string MenuAlertaData = "A14\nALERTA";
    public const string ExemploDeData = "31 12\n2023";

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
                instrucao: "Alternativa 1: aumente o número de ciclos configurado como limite do contador parcial."),
            new QuadroDeDisplayM4(
                CodigoA11,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: o novo limite do contador parcial foi salvo e o alerta A11 foi eliminado. O contador total não é alterado."),
            new QuadroDeDisplayM4(
                ResetContadorParcial,
                EstadoLedsM4.Desligado,
                instrucao: "Alternativa 2: em vez de aumentar o limite, entre em C18 para resetar o contador parcial."),
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
                instrucao: "Confirme o alerta A14: a data ajustada para a manutenção preventiva foi atingida. Para que esse alerta funcione, o relógio interno precisa estar corretamente ajustado em C13."),
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
                MenuAlertaData,
                EstadoLedsM4.Desligado,
                instrucao: "Use B3 para avançar até A14 ALERTA DATA e B2 para entrar na opção do alerta."),
            new QuadroDeDisplayM4(
                Habilitar,
                EstadoLedsM4.Desligado,
                instrucao: "Selecione HABILITAR com B1 ou B3 e pressione B2 para editar a data do alerta."),
            new QuadroDeDisplayM4(
                ExemploDeData,
                EstadoLedsM4.Desligado,
                instrucao: "Ajuste dia, mês e ano: B1 decrementa, B3 incrementa e B2 seleciona o dígito. Mantenha B2 por mais de 3 segundos para confirmar. 31 12 2023 é apenas o exemplo da Figura 108; a data real é escolhida pelo usuário."),
            new QuadroDeDisplayM4(
                MenuAlertaData,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: a nova data foi salva e o alerta A14 foi eliminado. Se você sair com B1 por mais de 3 segundos antes de confirmar, nenhuma alteração parcial é salva."));

        var desligarAlerta = new SequenciaDeQuadrosM4(
            new QuadroDeDisplayM4(
                CodigoA14,
                EstadoLedsM4.Desligado,
                instrucao: "Identifique A14 no menu de alertas. Esta ação apenas desliga o alerta, sem definir uma nova data."),
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
                MenuAlertaData,
                EstadoLedsM4.Desligado,
                instrucao: "Use B3 para avançar até A14 ALERTA DATA e B2 para entrar na opção do alerta."),
            new QuadroDeDisplayM4(
                Desabilitar,
                EstadoLedsM4.Desligado,
                instrucao: "Selecione DESABILITAR com B1 ou B3 e pressione B2 para desligar o alerta A14."),
            new QuadroDeDisplayM4(
                MenuAlertaData,
                EstadoLedsM4.Desligado,
                instrucao: "Verifique a confirmação: o alerta A14 foi desligado. O relógio C13 não é alterado e continua com a data e a hora ajustadas."));

        return new PerfilDeDisplayDeAlerta(
            CodigoA14,
            new[] { definirNovaData, desligarAlerta },
            mecanismoDeAtivacaoConfirmado: false);
    }

    #endregion
}
