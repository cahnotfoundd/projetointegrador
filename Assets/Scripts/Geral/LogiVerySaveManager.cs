
using System;
using System.Collections.Generic;
using UnityEngine;

public static class LogiVerySaveManager
{
    private const string Chave =
        "LogiVery_SalvamentoCompleto";

    [Serializable]
    public class ItemPedido
    {
        public int indiceProduto;
        public int quantidade;
    }

    [Serializable]
    public class Pedido
    {
        public string cliente;
        public int distancia;
        public int indiceEntregador;
        public List<ItemPedido> itens = new List<ItemPedido>();
    }

    [Serializable]
    public class ProdutoMontagem
    {
        public bool colocadoNaCaixa;
        public Vector2 posicao;
        public List<int> desenhosAtivos = new List<int>();
    }

    [Serializable]
    public class Cronometro
    {
        public float tempoRestante;
        public float intervaloSom;
        public bool contando;
    }

    [Serializable]
    public class Embalagem
    {
        public bool embrulhoAplicado;
        public bool etiquetaAplicada;
    }

    [Serializable]
    public class Rota
    {
        public int rotaSelecionada = -1;
        public bool tentativaIniciada;
        public bool concluida;

        public List<Vector2> pontosDesenhados =
            new List<Vector2>();

        public List<float> progressoRotas =
            new List<float>();

        public List<int> rotasCandidatas =
            new List<int>();
    }

    [Serializable]
    public class Entrega
    {
        public bool iniciada;
        public bool ativa;
        public bool pausada;
        public bool finalizada;
        public bool transicaoClienteIniciada;

        public bool desafioEmAndamento;
        public bool houveMinigame;
        public bool ultimoFoiLabirinto;

        public bool avisoDeEvento;
        public bool avisoEventoLabirinto;
        public float tempoRestanteAvisoEvento;

        public bool problemaPendente;
        public bool problemaResolvido;

        public float tempoDecorrido;
        public int saldoAvaliacao;
        public int proximoEvento;

        public List<float> momentosEventos =
            new List<float>();

        public int proximaNotificacao;
        public bool notificacaoVisivel;
        public float tempoRestanteNotificacao;

        public List<int> mensagensSorteadas =
            new List<int>();

        public List<float> momentosNotificacoes =
            new List<float>();

        public string tituloNotificacao;
        public string mensagemNotificacao;

        public bool painelDecisao;
        public bool painelLabirinto;
        public bool painelSemaforo;
        public bool painelFimViagem;
        public bool painelAnimacaoCliente;
        public bool painelAvaliacaoCliente;

        // 0 = viagem, 1 = espera antes da animacao,
        // 2 = animacao, 3 = avaliacao.
        public int faseFinalizacao;
        public float tempoRestanteFinalizacao;
        public float progressoAnimacaoCliente;
    }

    [Serializable]
    public class Labirinto
    {
        public Vector2 posicaoJogador;
        public bool destinoConcluido;
    }

    [Serializable]
    public class Semaforo
    {
        public int estadoAtual;
        public float tempoDecorrido;
        public bool desafioEncerrado;
        public float tempoRestanteLuz;
        public float tempoRestanteAviso;
        public bool sucessoEncerramento;
        public string textoAviso;
    }

    [Serializable]
    public class Substituicao
    {
        public int quantidadeSubstituicoes;
        public bool aguardando;
        public bool pedidoPerdido;
        public int indiceEntregadorAtual = -1;
        public float tempoRestanteBusca;
        public int tempoBuscaTotal;
        public bool novoEntregadorVisivel;
    }

    [Serializable]
    public class Avaliacao
    {
        public int notaPedido;
        public int notaEntrega;
    }

    [Serializable]
    public class Dados
    {
        public int versao = 1;

        public int diaAtual = 1;
        public int entregasDoDia;
        public int estrelasAcumuladas;
        public int estrelasDoDia;
        public int pedidosPerdidosDoDia;

        public string etapaAtual = "Pedidos";
        public int pedidoSelecionado = -1;

        public List<Pedido> pedidos =
            new List<Pedido>();

        public List<ProdutoMontagem> produtosMontagem =
            new List<ProdutoMontagem>();

        public Cronometro cronometro = new Cronometro();
        public Embalagem embalagem = new Embalagem();
        public Rota rota = new Rota();
        public Entrega entrega = new Entrega();
        public Labirinto labirinto = new Labirinto();
        public Semaforo semaforo = new Semaforo();
        public Substituicao substituicao =
            new Substituicao();
        public Avaliacao avaliacao = new Avaliacao();
    }

    public static bool ExisteSalvamento()
    {
        return TentarCarregar(out _);
    }

    public static bool Salvar(Dados dados)
    {
        if (!Validar(dados))
        {
            Debug.LogError(
                "LogiVery: dados incompletos. " +
                "O salvamento anterior foi preservado."
            );
            return false;
        }

        try
        {
            string json = JsonUtility.ToJson(dados);

            PlayerPrefs.SetString(Chave, json);
            PlayerPrefs.Save();

            Debug.Log(
                "LogiVery: partida salva na etapa " +
                dados.etapaAtual + "."
            );

            return true;
        }
        catch (Exception erro)
        {
            Debug.LogError(
                "LogiVery: erro ao salvar: " +
                erro.Message
            );
            return false;
        }
    }

    public static bool TentarCarregar(out Dados dados)
    {
        dados = null;

        if (!PlayerPrefs.HasKey(Chave))
            return false;

        try
        {
            string json = PlayerPrefs.GetString(
                Chave, ""
            );

            if (string.IsNullOrEmpty(json))
                return false;

            Dados carregados =
                JsonUtility.FromJson<Dados>(json);

            if (!Validar(carregados))
                return false;

            dados = carregados;
            return true;
        }
        catch (Exception erro)
        {
            Debug.LogWarning(
                "LogiVery: erro ao ler salvamento: " +
                erro.Message
            );
            return false;
        }
    }

    private static bool Validar(Dados dados)
    {
        if (dados == null ||
            dados.versao != 1 ||
            dados.diaAtual < 1 ||
            dados.entregasDoDia < 0 ||
            dados.estrelasAcumuladas < 0 ||
            string.IsNullOrEmpty(dados.etapaAtual) ||
            dados.pedidos == null ||
            dados.pedidos.Count != 3)
        {
            return false;
        }

        foreach (Pedido pedido in dados.pedidos)
        {
            if (pedido == null ||
                pedido.itens == null ||
                pedido.itens.Count != 2)
            {
                return false;
            }
        }

        return true;
    }

    public static void ApagarSalvamento()
    {
        PlayerPrefs.DeleteKey(Chave);
        PlayerPrefs.Save();

        Debug.Log(
            "LogiVery: partida anterior apagada."
        );
    }
}
