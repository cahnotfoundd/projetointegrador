
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Events;

public class DeliveryScreenManager : MonoBehaviour
{
    [Header("Paineis principais")]
    [SerializeField] private GameObject painelEntrega;
    [SerializeField] private GameObject painelDecisao;
    [SerializeField] private GameObject painelLabirinto;
    [SerializeField] private GameObject painelSemaforo;

    [Header("Paineis de finalizacao")]
    [SerializeField] private GameObject painelFimViagem;
    [SerializeField] private GameObject painelAnimacaoCliente;
    [SerializeField] private GameObject painelAvaliacaoCliente;

    [Header("Tempos da finalizacao")]
    [SerializeField] private float tempoAntesDaAnimacao = 5f;
    [SerializeField] private float tempoAnimacaoCliente = 5f;

    [Header("Animacao do transito")]
    [SerializeField] private Animator animatorTransito;

    [Header("Animacao do cliente")]
    [SerializeField] private Animator animatorCliente;

    [Header("Continuar Avaliacao")]
    [SerializeField] private UnityEvent aoContinuarAvaliacao;

    [Header("Gerenciador de Avaliacao")]
    [SerializeField] private CustomerEvaluationManager evaluationManager;

    [Header("Reset do Labirinto")]
    [SerializeField] private MazePlayerManager mazePlayer;
    [SerializeField] private MazeDestination mazeDestination;

    [Header("Tempo invisivel da viagem")]
    [Min(25f)]
    [SerializeField] private float duracaoViagem = 60f;

    [Header("Notificacoes")]
    [SerializeField] private GameObject painelNotificacao;
    [SerializeField] private TMP_Text textoTituloNotificacao;
    [SerializeField] private TMP_Text textoMensagemNotificacao;

    [Header("Duracao das notificacoes")]
    [SerializeField] private float duracaoMinimaNotificacao = 2f;
    [SerializeField] private float duracaoMaximaNotificacao = 3f;

    // ESTADOS DA ENTREGA

    private bool viagemIniciada;
    private bool viagemAtiva;
    private bool viagemPausada;
    private bool viagemFinalizada;
    private bool transicaoClienteIniciada;

    private bool desafioEmAndamento;

    private bool houveMinigameNaViagem;
    private bool ultimoMinigameFoiLabirinto;

    private bool avisoDeEventoEmAndamento;

    private bool problemaPendente;
    private bool problemaResolvido;

    private float tempoDecorrido;
    private float velocidadeOriginalAnimacao = 1f;
    private float velocidadeOriginalAnimacaoCliente = 1f;

    // EVENTOS ALEATORIOS

    private readonly List<float> momentosEventos =
        new List<float>();

    private int proximoEvento;

    // PONTUACAO DA ENTREGA

    private int saldoAvaliacao;

    // NOTIFICACOES

    private bool notificacaoVisivel;
    private float tempoRestanteNotificacao;
    private int proximaNotificacao;

    private readonly List<int> mensagensSorteadas =
        new List<int>();

    private readonly List<float> momentosNotificacoes =
        new List<float>();

    private struct MensagemEntrega
    {
        public string titulo;
        public string mensagem;

        public MensagemEntrega(string titulo, string mensagem)
        {
            this.titulo = titulo;
            this.mensagem = mensagem;
        }
    }

    private static readonly MensagemEntrega[] bancoMensagens =
    {
        new MensagemEntrega(
            "CENTRAL LOGIVERY",
            "Tudo certo! Estamos acompanhando a entrega."
        ),

        new MensagemEntrega(
            "MENSAGEM DO ENTREGADOR",
            "Mais um trecho concluido! Seguimos viagem."
        ),

        new MensagemEntrega(
            "STATUS DA ENCOMENDA",
            "Pedido seguro e a caminho do destino!"
        ),

        new MensagemEntrega(
            "CENTRAL DE OPERACOES",
            "Monitoramento ativo. Tudo sob controle!"
        ),

        new MensagemEntrega(
            "TRANSITO URBANO",
            "O movimento esta intenso por aqui!"
        ),

        new MensagemEntrega(
            "MENSAGEM DO ENTREGADOR",
            "Central, estou avancando pela cidade!"
        ),

        new MensagemEntrega(
            "ATUALIZACAO DO PEDIDO",
            "O cliente esta acompanhando a entrega."
        ),

        new MensagemEntrega(
            "DICA LOGIVERY",
            "Uma boa rota faz toda a diferenca!"
        ),

        new MensagemEntrega(
            "CENTRAL LOGIVERY",
            "Mais uma entrega em andamento!"
        ),

        new MensagemEntrega(
            "STATUS DA VIAGEM",
            "Nenhuma ocorrencia importante no momento."
        ),

        new MensagemEntrega(
            "CENTRAL LOGIVERY",
            "Estamos cada vez mais proximos do destino!"
        )
    };

    private void Awake()
    {
        if (animatorTransito != null)
        {
            velocidadeOriginalAnimacao = animatorTransito.speed;
        }

        if (animatorCliente != null)
        {
            velocidadeOriginalAnimacaoCliente =
                animatorCliente.speed;
        }
    }

    private void Update()
    {
        if (painelEntrega == null)
            return;

        // Detecta a saida da tela de entrega.
        if (!painelEntrega.activeInHierarchy)
        {
            if (viagemIniciada)
            {
                StopAllCoroutines();
                LimparEstadoViagem();
            }

            return;
        }

        // Inicia quando a tela de entrega abre.
        if (!viagemIniciada)
        {
            IniciarViagem();
        }

        if (!viagemAtiva)
            return;

        // Atalho temporario T: abre o labirinto.
        if (Keyboard.current != null &&
            Keyboard.current.tKey.wasPressedThisFrame &&
            !desafioEmAndamento &&
            !avisoDeEventoEmAndamento &&
            !viagemPausada)
        {
            MostrarDecisao();
        }

        // Atalho temporario K: conclui o labirinto.
        if (Keyboard.current != null &&
            Keyboard.current.kKey.wasPressedThisFrame &&
            painelLabirinto != null &&
            painelLabirinto.activeInHierarchy)
        {
            ConcluirLabirinto();
        }

        // O tempo nao avanca durante os desafios.
        if (viagemPausada)
            return;

        float delta = Time.deltaTime;

        tempoDecorrido += delta;

        // Eventos importantes tem prioridade.
        if (!desafioEmAndamento &&
            !avisoDeEventoEmAndamento &&
            proximoEvento < momentosEventos.Count &&
            tempoDecorrido >= momentosEventos[proximoEvento])
        {
            SortearMinigame();
            return;
        }

        AtualizarNotificacoes(delta);

        if (tempoDecorrido >= duracaoViagem)
        {
            FinalizarViagem();
        }
    }

    public void IniciarViagem()
    {
        if (viagemIniciada ||
            painelEntrega == null ||
            !painelEntrega.activeInHierarchy)
        {
            return;
        }

        if (painelDecisao == null ||
            painelLabirinto == null ||
            painelSemaforo == null ||
            painelFimViagem == null ||
            painelNotificacao == null ||
            textoTituloNotificacao == null ||
            textoMensagemNotificacao == null)
        {
            Debug.LogError(
                "DeliveryScreenManager: faltam referencias!",
                this
            );

            return;
        }

        viagemIniciada = true;
        viagemAtiva = true;
        viagemPausada = false;
        viagemFinalizada = false;
        transicaoClienteIniciada = false;

        desafioEmAndamento = false;
        houveMinigameNaViagem = false;
        ultimoMinigameFoiLabirinto = false;
        avisoDeEventoEmAndamento = false;
        problemaPendente = false;
        problemaResolvido = false;

        tempoDecorrido = 0f;
        saldoAvaliacao = 0;

        proximoEvento = 0;
        proximaNotificacao = 0;

        notificacaoVisivel = false;
        tempoRestanteNotificacao = 0f;

        painelDecisao.SetActive(false);
        painelLabirinto.SetActive(false);
        painelSemaforo.SetActive(false);
        painelNotificacao.SetActive(false);
        painelFimViagem.SetActive(false);

        if (painelAnimacaoCliente != null)
            painelAnimacaoCliente.SetActive(false);

        if (painelAvaliacaoCliente != null)
            painelAvaliacaoCliente.SetActive(false);

        // Reativa a animacao do transito.
        if (animatorTransito != null)
        {
            animatorTransito.speed =
                velocidadeOriginalAnimacao;
        }

        PrepararEventos();
        PrepararNotificacoes();

        Debug.Log("Nova viagem LogiVery iniciada!");
    }

    // PREPARACAO DOS EVENTOS

    private void PrepararEventos()
    {
        momentosEventos.Clear();
        proximoEvento = 0;

        // Tres ou quatro desafios por viagem.
        int quantidade = Random.Range(3, 5);

        float intervalo = duracaoViagem / (quantidade + 1f);

        for (int i = 1; i <= quantidade; i++)
        {
            float momento = intervalo * i;

            momento += Random.Range(-1.5f, 1.5f);

            momento = Mathf.Clamp(
                momento,
                4f,
                duracaoViagem - 4f
            );

            momentosEventos.Add(momento);
        }

        Debug.Log("Eventos programados: " + quantidade);
    }

    private void SortearMinigame()
    {
        if (!viagemAtiva ||
            desafioEmAndamento ||
            avisoDeEventoEmAndamento)
        {
            return;
        }

        bool escolherLabirinto;

        if (!houveMinigameNaViagem)
        {
            // Primeiro desafio: aleatorio.
            escolherLabirinto = Random.value < 0.5f;
        }
        else
        {
            // Demais desafios: alternancia obrigatoria.
            escolherLabirinto = !ultimoMinigameFoiLabirinto;
        }

        StartCoroutine(
            AvisarEIniciarMinigame(escolherLabirinto)
        );
    }

    private IEnumerator AvisarEIniciarMinigame(bool labirinto)
    {
        avisoDeEventoEmAndamento = true;

        // Pausa o tempo invisivel da viagem.
        viagemPausada = true;

        OcultarNotificacao();

        if (textoTituloNotificacao != null)
        {
            textoTituloNotificacao.text = labirinto
                ? "ALERTA DE TRANSITO!"
                : "ATENCAO AO SEMAFORO!";
        }

        if (textoMensagemNotificacao != null)
        {
            textoMensagemNotificacao.text = labirinto
                ? "Acidente detectado a frente! " +
                  "Uma rota alternativa pode ser necessaria."
                : "Semaforo a frente! " +
                  "Aguarde o momento certo para avancar.";
        }

        if (painelNotificacao != null)
        {
            painelNotificacao.SetActive(true);

            // AUDIO: nova notificacao de alerta.
            LogiVeryEfeitosSonoros.Instancia?
                .TocarNotificacoes();
        }

        yield return new WaitForSeconds(2f);

        if (painelNotificacao != null)
        {
            painelNotificacao.SetActive(false);
        }

        if (!viagemAtiva ||
            painelEntrega == null ||
            !painelEntrega.activeInHierarchy)
        {
            avisoDeEventoEmAndamento = false;
            yield break;
        }

        avisoDeEventoEmAndamento = false;

        if (labirinto)
        {
            MostrarDecisao();
        }
        else
        {
            AbrirSemaforo();
        }
    }

    // LABIRINTO

    public void MostrarDecisao()
    {
        if (!viagemAtiva ||
            desafioEmAndamento ||
            painelDecisao == null)
        {
            return;
        }

        desafioEmAndamento = true;
        houveMinigameNaViagem = true;
        ultimoMinigameFoiLabirinto = true;

        proximoEvento = Mathf.Min(
            proximoEvento + 1,
            momentosEventos.Count
        );

        problemaPendente = true;
        problemaResolvido = false;

        PausarViagem();

        painelDecisao.SetActive(true);

        Debug.Log("Evento de labirinto iniciado!");
    }

    public void AceitarLabirinto()
    {
        if (!viagemAtiva ||
            !desafioEmAndamento ||
            painelDecisao == null ||
            !painelDecisao.activeInHierarchy)
        {
            return;
        }

        painelDecisao.SetActive(false);
        painelLabirinto.SetActive(true);

        if (mazeDestination != null)
            mazeDestination.ResetDestination();

        if (mazePlayer != null)
            mazePlayer.ResetPlayer();

        Physics2D.SyncTransforms();

        Debug.Log("Labirinto aberto e reiniciado!");
    }

    public void RecusarLabirinto()
    {
        if (!viagemAtiva ||
            !desafioEmAndamento ||
            painelDecisao == null ||
            !painelDecisao.activeInHierarchy)
        {
            return;
        }

        painelDecisao.SetActive(false);

        problemaPendente = true;
        problemaResolvido = false;

        saldoAvaliacao -= 1;

        Debug.Log("Labirinto recusado: -1 ponto.");

        EncerrarDesafio();
    }

    public void ConcluirLabirinto()
    {
        if (!viagemAtiva ||
            !desafioEmAndamento ||
            painelLabirinto == null ||
            !painelLabirinto.activeInHierarchy)
        {
            return;
        }

        painelLabirinto.SetActive(false);

        problemaPendente = false;
        problemaResolvido = true;

        saldoAvaliacao += 1;

        Debug.Log("Labirinto concluido: +1 ponto.");

        EncerrarDesafio();
    }

    // SEMAFORO

    public void AbrirSemaforo()
    {
        if (!viagemAtiva ||
            desafioEmAndamento ||
            painelSemaforo == null)
        {
            return;
        }

        desafioEmAndamento = true;
        houveMinigameNaViagem = true;
        ultimoMinigameFoiLabirinto = false;

        proximoEvento = Mathf.Min(
            proximoEvento + 1,
            momentosEventos.Count
        );

        problemaPendente = true;
        problemaResolvido = false;

        PausarViagem();

        painelSemaforo.SetActive(true);

        Debug.Log("Minigame Onda Verde iniciado!");
    }

    public void ConcluirSemaforo(bool sucesso)
    {
        if (!viagemAtiva ||
            !desafioEmAndamento ||
            painelSemaforo == null ||
            !painelSemaforo.activeInHierarchy)
        {
            return;
        }

        painelSemaforo.SetActive(false);

        problemaPendente = !sucesso;
        problemaResolvido = sucesso;

        if (sucesso)
        {
            saldoAvaliacao += 1;
            Debug.Log("Semaforo correto: +1 ponto.");
        }
        else
        {
            saldoAvaliacao -= 1;
            Debug.Log("Semaforo incorreto: -1 ponto.");
        }

        EncerrarDesafio();
    }

    // CONTROLE DOS DESAFIOS

    private void EncerrarDesafio()
    {
        desafioEmAndamento = false;
        RetomarViagem();

        Debug.Log(
            "Viagem retomada. Saldo: " + saldoAvaliacao
        );
    }

    public void PausarViagem()
    {
        if (!viagemAtiva)
            return;

        viagemPausada = true;

        if (painelNotificacao != null)
            painelNotificacao.SetActive(false);
    }

    public void RetomarViagem()
    {
        if (!viagemAtiva)
            return;

        viagemPausada = false;

        if (notificacaoVisivel &&
            painelNotificacao != null)
        {
            // Apenas reexibe, sem repetir o som.
            painelNotificacao.SetActive(true);
        }
    }

    // NOTIFICACOES

    private void PrepararNotificacoes()
    {
        mensagensSorteadas.Clear();
        momentosNotificacoes.Clear();

        proximaNotificacao = 0;
        notificacaoVisivel = false;
        tempoRestanteNotificacao = 0f;

        int ultimaMensagem = -1;

        float momento = 0.5f;

        while (momento < duracaoViagem - 1f)
        {
            int mensagem = Random.Range(
                0,
                bancoMensagens.Length
            );

            if (bancoMensagens.Length > 1)
            {
                while (mensagem == ultimaMensagem)
                {
                    mensagem = Random.Range(
                        0,
                        bancoMensagens.Length
                    );
                }
            }

            mensagensSorteadas.Add(mensagem);
            momentosNotificacoes.Add(momento);

            ultimaMensagem = mensagem;

            momento += Random.Range(3.8f, 4.8f);
        }
    }

    private void AtualizarNotificacoes(float delta)
    {
        if (notificacaoVisivel)
        {
            tempoRestanteNotificacao -= delta;

            if (tempoRestanteNotificacao <= 0f)
                OcultarNotificacao();
        }

        if (!notificacaoVisivel &&
            proximaNotificacao < momentosNotificacoes.Count &&
            tempoDecorrido >=
            momentosNotificacoes[proximaNotificacao])
        {
            MostrarNotificacao();
            proximaNotificacao++;
        }
    }

    private void MostrarNotificacao()
    {
        int indice = mensagensSorteadas[proximaNotificacao];

        MensagemEntrega mensagem = bancoMensagens[indice];

        textoTituloNotificacao.text = mensagem.titulo;
        textoMensagemNotificacao.text = mensagem.mensagem;

        float minimo = Mathf.Max(
            0.5f,
            Mathf.Min(
                duracaoMinimaNotificacao,
                duracaoMaximaNotificacao
            )
        );

        float maximo = Mathf.Max(
            minimo,
            Mathf.Max(
                duracaoMinimaNotificacao,
                duracaoMaximaNotificacao
            )
        );

        tempoRestanteNotificacao =
            Random.Range(minimo, maximo);

        notificacaoVisivel = true;
        painelNotificacao.SetActive(true);

        // AUDIO: somente quando uma nova mensagem aparece.
        LogiVeryEfeitosSonoros.Instancia?
            .TocarNotificacoes();
    }

    private void OcultarNotificacao()
    {
        notificacaoVisivel = false;
        tempoRestanteNotificacao = 0f;

        if (painelNotificacao != null)
            painelNotificacao.SetActive(false);
    }

    // RESULTADO DA ENTREGA

    public int CalcularNotaFinal(int notaBase)
    {
        int modificador = Mathf.Clamp(
            saldoAvaliacao,
            -2,
            2
        );

        return Mathf.Clamp(
            notaBase + modificador,
            1,
            5
        );
    }

    public int ObterSaldoAvaliacao()
    {
        return Mathf.Clamp(
            saldoAvaliacao,
            -2,
            2
        );
    }

    // CHEGADA AO DESTINO

    private void FinalizarViagem()
    {
        if (viagemFinalizada || desafioEmAndamento)
            return;

        viagemFinalizada = true;
        viagemAtiva = false;
        viagemPausada = false;

        OcultarNotificacao();

        // Congela a animacao do transito.
        if (animatorTransito != null)
        {
            animatorTransito.speed = 0f;
        }

        if (painelFimViagem != null)
            painelFimViagem.SetActive(true);

        Debug.Log(
            "Destino alcancado! Saldo da entrega: " +
            saldoAvaliacao
        );
    }

    public void ContinuarAposViagem()
    {
        if (!viagemFinalizada ||
            transicaoClienteIniciada)
        {
            return;
        }

        transicaoClienteIniciada = true;

        if (painelFimViagem != null)
            painelFimViagem.SetActive(false);

        StartCoroutine(FluxoPosLabirinto());
    }

    private IEnumerator FluxoPosLabirinto()
    {
        // Preserva a espera antes da animacao.
        yield return new WaitForSeconds(
            tempoAntesDaAnimacao
        );

        if (painelAnimacaoCliente != null)
        {
            painelAnimacaoCliente.SetActive(true);
        }

        // Reinicia a animacao desde o primeiro frame.
        if (animatorCliente != null)
        {
            animatorCliente.Rebind();

            animatorCliente.speed =
                velocidadeOriginalAnimacaoCliente;

            animatorCliente.Play(0, 0, 0f);
            animatorCliente.Update(0f);
        }

        // AUDIO: toca somente quando a animacao comeca.
        // Nao toca quando PanelEntrega e aberto.
        if (painelAnimacaoCliente != null &&
            painelAnimacaoCliente.activeInHierarchy)
        {
            LogiVeryEfeitosSonoros.Instancia?
                .TocarAnimacaoCliente();
        }

        yield return new WaitForSeconds(
            tempoAnimacaoCliente
        );

        // Congela no ultimo frame.
        if (animatorCliente != null)
        {
            animatorCliente.Play(0, 0, 0.999f);
            animatorCliente.Update(0f);
            animatorCliente.speed = 0f;
        }

        // Abre a avaliacao com a cena congelada.
        if (painelAvaliacaoCliente != null)
        {
            painelAvaliacaoCliente.SetActive(true);
        }

        // Preserva a influencia dos minigames.
        if (evaluationManager != null)
        {
            evaluationManager.ShowRandomNote(
                ObterSaldoAvaliacao()
            );
        }

        Debug.Log("Avaliacao do cliente aberta!");
    }

    public void ContinuarAvaliacao()
    {
        if (painelAnimacaoCliente != null)
            painelAnimacaoCliente.SetActive(false);

        if (painelAvaliacaoCliente != null)
            painelAvaliacaoCliente.SetActive(false);

        aoContinuarAvaliacao?.Invoke();

        Debug.Log("Avaliacao concluida!");
    }

    // REINICIO

    private void LimparEstadoViagem()
    {
        viagemIniciada = false;
        viagemAtiva = false;
        viagemPausada = false;
        viagemFinalizada = false;
        transicaoClienteIniciada = false;

        desafioEmAndamento = false;

        problemaPendente = false;
        problemaResolvido = false;

        tempoDecorrido = 0f;
        saldoAvaliacao = 0;

        proximoEvento = 0;
        proximaNotificacao = 0;

        notificacaoVisivel = false;
        tempoRestanteNotificacao = 0f;

        momentosEventos.Clear();
        mensagensSorteadas.Clear();
        momentosNotificacoes.Clear();

        if (painelNotificacao != null)
            painelNotificacao.SetActive(false);

        if (animatorTransito != null)
        {
            animatorTransito.speed =
                velocidadeOriginalAnimacao;
        }

        if (animatorCliente != null)
        {
            animatorCliente.speed =
                velocidadeOriginalAnimacaoCliente;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        LimparEstadoViagem();
    }

    public void ResetDelivery()
    {
        StopAllCoroutines();

        LimparEstadoViagem();

        if (painelEntrega != null)
            painelEntrega.SetActive(false);

        if (painelDecisao != null)
            painelDecisao.SetActive(false);

        if (painelLabirinto != null)
            painelLabirinto.SetActive(false);

        if (painelSemaforo != null)
            painelSemaforo.SetActive(false);

        if (painelFimViagem != null)
            painelFimViagem.SetActive(false);

        if (mazePlayer != null)
            mazePlayer.ResetPlayer();

        if (mazeDestination != null)
            mazeDestination.ResetDestination();

        if (painelAnimacaoCliente != null)
            painelAnimacaoCliente.SetActive(false);

        if (painelAvaliacaoCliente != null)
            painelAvaliacaoCliente.SetActive(false);

        Debug.Log("Entrega reiniciada!");
    }
}
