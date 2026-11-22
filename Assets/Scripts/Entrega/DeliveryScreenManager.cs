
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
    [SerializeField]
    private float tempoAntesDaAnimacao = 5f;

    [SerializeField]
    private float tempoAnimacaoCliente = 5f;

    [Header("Animacao do transito")]
    [SerializeField] private Animator animatorTransito;

    [Header("Animacao do cliente")]
    [SerializeField] private Animator animatorCliente;

    [Header("Continuar Avaliacao")]
    [SerializeField]
    private UnityEvent aoContinuarAvaliacao;

    [Header("Gerenciador de Avaliacao")]
    [SerializeField]
    private CustomerEvaluationManager evaluationManager;

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
    [SerializeField]
    private float duracaoMinimaNotificacao = 2f;

    [SerializeField]
    private float duracaoMaximaNotificacao = 3f;

    private bool viagemIniciada;
    private bool viagemAtiva;
    private bool viagemPausada;
    private bool viagemFinalizada;
    private bool transicaoClienteIniciada;

    private bool desafioEmAndamento;
    private bool houveMinigameNaViagem;
    private bool ultimoMinigameFoiLabirinto;

    private bool avisoDeEventoEmAndamento;
    private bool avisoEventoLabirinto;
    private float tempoRestanteAvisoEvento;

    private bool problemaPendente;
    private bool problemaResolvido;

    private float tempoDecorrido;
    private float velocidadeOriginalAnimacao = 1f;
    private float velocidadeOriginalAnimacaoCliente = 1f;

    private readonly List<float> momentosEventos =
        new List<float>();

    private int proximoEvento;
    private int saldoAvaliacao;

    private bool notificacaoVisivel;
    private float tempoRestanteNotificacao;
    private int proximaNotificacao;

    private readonly List<int> mensagensSorteadas =
        new List<int>();

    private readonly List<float> momentosNotificacoes =
        new List<float>();

    // 0 = viagem ou fim de viagem
    // 1 = aguardando animacao
    // 2 = animacao em andamento
    // 3 = avaliacao aberta
    private int faseFinalizacao;
    private float tempoRestanteFinalizacao;

    private struct MensagemEntrega
    {
        public string titulo;
        public string mensagem;

        public MensagemEntrega(
            string titulo,
            string mensagem)
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
            velocidadeOriginalAnimacao =
                animatorTransito.speed;
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

        if (!painelEntrega.activeInHierarchy)
        {
            if (viagemIniciada)
            {
                StopAllCoroutines();
                LimparEstadoViagem();
            }

            return;
        }

        if (!viagemIniciada)
            IniciarViagem();

        if (!viagemAtiva)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.tKey.wasPressedThisFrame &&
            !desafioEmAndamento &&
            !avisoDeEventoEmAndamento &&
            !viagemPausada)
        {
            MostrarDecisao();
        }

        if (Keyboard.current != null &&
            Keyboard.current.kKey.wasPressedThisFrame &&
            painelLabirinto != null &&
            painelLabirinto.activeInHierarchy)
        {
            ConcluirLabirinto();
        }

        if (viagemPausada)
            return;

        float delta = Time.deltaTime;

        tempoDecorrido += delta;

        if (!desafioEmAndamento &&
            !avisoDeEventoEmAndamento &&
            proximoEvento < momentosEventos.Count &&
            tempoDecorrido >=
            momentosEventos[proximoEvento])
        {
            SortearMinigame();
            return;
        }

        AtualizarNotificacoes(delta);

        if (tempoDecorrido >= duracaoViagem)
            FinalizarViagem();
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
        avisoEventoLabirinto = false;
        tempoRestanteAvisoEvento = 0f;

        problemaPendente = false;
        problemaResolvido = false;

        tempoDecorrido = 0f;
        saldoAvaliacao = 0;

        proximoEvento = 0;
        proximaNotificacao = 0;

        notificacaoVisivel = false;
        tempoRestanteNotificacao = 0f;

        faseFinalizacao = 0;
        tempoRestanteFinalizacao = 0f;

        painelDecisao.SetActive(false);
        painelLabirinto.SetActive(false);
        painelSemaforo.SetActive(false);
        painelNotificacao.SetActive(false);
        painelFimViagem.SetActive(false);

        if (painelAnimacaoCliente != null)
            painelAnimacaoCliente.SetActive(false);

        if (painelAvaliacaoCliente != null)
            painelAvaliacaoCliente.SetActive(false);

        if (animatorTransito != null)
        {
            animatorTransito.speed =
                velocidadeOriginalAnimacao;
        }

        PrepararEventos();
        PrepararNotificacoes();

        Debug.Log("Nova viagem LogiVery iniciada!");
    }

    private void PrepararEventos()
    {
        momentosEventos.Clear();
        proximoEvento = 0;

        int quantidade = Random.Range(3, 5);

        float intervalo =
            duracaoViagem / (quantidade + 1f);

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

        Debug.Log(
            "Eventos programados: " + quantidade
        );
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
            escolherLabirinto = Random.value < 0.5f;
        }
        else
        {
            escolherLabirinto =
                !ultimoMinigameFoiLabirinto;
        }

        tempoRestanteAvisoEvento = 2f;

        StartCoroutine(
            AvisarEIniciarMinigame(
                escolherLabirinto,
                false
            )
        );
    }

    private IEnumerator AvisarEIniciarMinigame(
        bool labirinto,
        bool restaurando)
    {
        avisoDeEventoEmAndamento = true;
        avisoEventoLabirinto = labirinto;
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

            if (!restaurando)
            {
                LogiVeryEfeitosSonoros.Instancia?
                    .TocarNotificacoes();
            }
        }

        while (tempoRestanteAvisoEvento > 0f)
        {
            tempoRestanteAvisoEvento = Mathf.Max(
                0f,
                tempoRestanteAvisoEvento - Time.deltaTime
            );

            yield return null;
        }

        if (painelNotificacao != null)
            painelNotificacao.SetActive(false);

        if (!viagemAtiva ||
            painelEntrega == null ||
            !painelEntrega.activeInHierarchy)
        {
            avisoDeEventoEmAndamento = false;
            yield break;
        }

        avisoDeEventoEmAndamento = false;

        if (labirinto)
            MostrarDecisao();
        else
            AbrirSemaforo();
    }

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

        Debug.Log(
            "Evento de labirinto iniciado!"
        );
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

        Debug.Log(
            "Labirinto aberto e reiniciado!"
        );
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

        Debug.Log(
            "Labirinto concluido: +1 ponto."
        );

        EncerrarDesafio();
    }

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

        Debug.Log(
            "Minigame Onda Verde iniciado!"
        );
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

            Debug.Log(
                "Semaforo correto: +1 ponto."
            );
        }
        else
        {
            saldoAvaliacao -= 1;

            Debug.Log(
                "Semaforo incorreto: -1 ponto."
            );
        }

        EncerrarDesafio();
    }

    private void EncerrarDesafio()
    {
        desafioEmAndamento = false;
        RetomarViagem();

        Debug.Log(
            "Viagem retomada. Saldo: " +
            saldoAvaliacao
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
            painelNotificacao.SetActive(true);
        }
    }

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
            proximaNotificacao <
            momentosNotificacoes.Count &&
            tempoDecorrido >=
            momentosNotificacoes[proximaNotificacao])
        {
            MostrarNotificacao();
            proximaNotificacao++;
        }
    }

    private void MostrarNotificacao()
    {
        int indice =
            mensagensSorteadas[proximaNotificacao];

        MensagemEntrega mensagem =
            bancoMensagens[indice];

        textoTituloNotificacao.text =
            mensagem.titulo;

        textoMensagemNotificacao.text =
            mensagem.mensagem;

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

    private void FinalizarViagem()
    {
        if (viagemFinalizada || desafioEmAndamento)
            return;

        viagemFinalizada = true;
        viagemAtiva = false;
        viagemPausada = false;

        OcultarNotificacao();

        if (animatorTransito != null)
            animatorTransito.speed = 0f;

        if (painelFimViagem != null)
            painelFimViagem.SetActive(true);

        Debug.Log(
            "Destino alcancado! Saldo: " +
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

        faseFinalizacao = 1;

        tempoRestanteFinalizacao = Mathf.Max(
            0f,
            tempoAntesDaAnimacao
        );

        StartCoroutine(
            FluxoPosLabirinto(false)
        );
    }

    private IEnumerator EsperarFase()
    {
        while (tempoRestanteFinalizacao > 0f)
        {
            tempoRestanteFinalizacao = Mathf.Max(
                0f,
                tempoRestanteFinalizacao -
                Time.deltaTime
            );

            yield return null;
        }
    }

    private IEnumerator FluxoPosLabirinto(
        bool restaurando)
    {
        if (faseFinalizacao <= 1)
        {
            faseFinalizacao = 1;

            yield return StartCoroutine(
                EsperarFase()
            );

            faseFinalizacao = 2;

            tempoRestanteFinalizacao = Mathf.Max(
                0f,
                tempoAnimacaoCliente
            );

            if (painelAnimacaoCliente != null)
                painelAnimacaoCliente.SetActive(true);

            if (animatorCliente != null)
            {
                animatorCliente.Rebind();

                animatorCliente.speed =
                    velocidadeOriginalAnimacaoCliente;

                animatorCliente.Play(0, 0, 0f);
                animatorCliente.Update(0f);
            }

            LogiVeryEfeitosSonoros.Instancia?
                .TocarAnimacaoCliente();
        }

        if (faseFinalizacao == 2)
        {
            // Ao restaurar, a animacao ja foi
            // posicionada no frame salvo.
            yield return StartCoroutine(
                EsperarFase()
            );

            if (animatorCliente != null)
            {
                animatorCliente.Play(
                    0, 0, 0.999f
                );

                animatorCliente.Update(0f);
                animatorCliente.speed = 0f;
            }

            faseFinalizacao = 3;

            if (painelAvaliacaoCliente != null)
                painelAvaliacaoCliente.SetActive(true);

            if (evaluationManager != null)
            {
                evaluationManager.ShowRandomNote(
                    ObterSaldoAvaliacao()
                );
            }

            Debug.Log(
                "Avaliacao do cliente aberta!"
            );
        }
    }

    public void ContinuarAvaliacao()
    {
        if (painelAnimacaoCliente != null)
            painelAnimacaoCliente.SetActive(false);

        if (painelAvaliacaoCliente != null)
            painelAvaliacaoCliente.SetActive(false);

        aoContinuarAvaliacao?.Invoke();

        Debug.Log(
            "Avaliacao concluida!"
        );
    }

    // ==========================================
    // SALVAMENTO DA ENTREGA
    // ==========================================

    private static bool EstaAberto(GameObject obj)
    {
        return obj != null && obj.activeSelf;
    }

    private SemaforoMinigame EncontrarSemaforo()
    {
        if (painelSemaforo == null)
            return null;

        return painelSemaforo
            .GetComponentInChildren<SemaforoMinigame>(
                true
            );
    }

    public void CapturarEstado(
        LogiVerySaveManager.Dados dados)
    {
        if (dados == null)
            return;

        LogiVerySaveManager.Entrega salvo =
            dados.entrega;

        salvo.iniciada = viagemIniciada;
        salvo.ativa = viagemAtiva;
        salvo.pausada = viagemPausada;
        salvo.finalizada = viagemFinalizada;

        salvo.transicaoClienteIniciada =
            transicaoClienteIniciada;

        salvo.desafioEmAndamento =
            desafioEmAndamento;

        salvo.houveMinigame =
            houveMinigameNaViagem;

        salvo.ultimoFoiLabirinto =
            ultimoMinigameFoiLabirinto;

        salvo.avisoDeEvento =
            avisoDeEventoEmAndamento;

        salvo.avisoEventoLabirinto =
            avisoEventoLabirinto;

        salvo.tempoRestanteAvisoEvento =
            tempoRestanteAvisoEvento;

        salvo.problemaPendente =
            problemaPendente;

        salvo.problemaResolvido =
            problemaResolvido;

        salvo.tempoDecorrido = tempoDecorrido;
        salvo.saldoAvaliacao = saldoAvaliacao;
        salvo.proximoEvento = proximoEvento;

        salvo.momentosEventos =
            new List<float>(momentosEventos);

        salvo.proximaNotificacao =
            proximaNotificacao;

        salvo.notificacaoVisivel =
            notificacaoVisivel;

        salvo.tempoRestanteNotificacao =
            tempoRestanteNotificacao;

        salvo.mensagensSorteadas =
            new List<int>(mensagensSorteadas);

        salvo.momentosNotificacoes =
            new List<float>(momentosNotificacoes);

        salvo.tituloNotificacao =
            textoTituloNotificacao != null
                ? textoTituloNotificacao.text
                : "";

        salvo.mensagemNotificacao =
            textoMensagemNotificacao != null
                ? textoMensagemNotificacao.text
                : "";

        salvo.painelDecisao =
            EstaAberto(painelDecisao);

        salvo.painelLabirinto =
            EstaAberto(painelLabirinto);

        salvo.painelSemaforo =
            EstaAberto(painelSemaforo);

        salvo.painelFimViagem =
            EstaAberto(painelFimViagem);

        salvo.painelAnimacaoCliente =
            EstaAberto(painelAnimacaoCliente);

        salvo.painelAvaliacaoCliente =
            EstaAberto(painelAvaliacaoCliente);

        salvo.faseFinalizacao =
            faseFinalizacao;

        salvo.tempoRestanteFinalizacao =
            tempoRestanteFinalizacao;

        if (animatorCliente != null &&
            animatorCliente.isActiveAndEnabled)
        {
            salvo.progressoAnimacaoCliente =
                Mathf.Repeat(
                    animatorCliente
                    .GetCurrentAnimatorStateInfo(0)
                    .normalizedTime,
                    1f
                );
        }

        if (mazePlayer != null)
        {
            Rigidbody2D rb =
                mazePlayer.GetComponent<Rigidbody2D>();

            dados.labirinto.posicaoJogador =
                rb != null
                    ? rb.position
                    : (Vector2)mazePlayer.transform.position;
        }

        SemaforoMinigame semaforo =
            EncontrarSemaforo();

        if (semaforo != null &&
            salvo.painelSemaforo)
        {
            semaforo.CapturarEstado(dados.semaforo);
        }

        if (evaluationManager != null)
        {
            dados.avaliacao.notaPedido =
                evaluationManager.orderNote;

            dados.avaliacao.notaEntrega =
                evaluationManager.deliveryNote;
        }
    }

    public void RestaurarEstado(
        LogiVerySaveManager.Dados dados)
    {
        if (dados == null || dados.entrega == null)
            return;

        StopAllCoroutines();

        LogiVerySaveManager.Entrega salvo =
            dados.entrega;

        viagemIniciada = salvo.iniciada;
        viagemAtiva = salvo.ativa;
        viagemPausada = salvo.pausada;
        viagemFinalizada = salvo.finalizada;

        transicaoClienteIniciada =
            salvo.transicaoClienteIniciada;

        desafioEmAndamento =
            salvo.desafioEmAndamento;

        houveMinigameNaViagem =
            salvo.houveMinigame;

        ultimoMinigameFoiLabirinto =
            salvo.ultimoFoiLabirinto;

        avisoDeEventoEmAndamento =
            salvo.avisoDeEvento;

        avisoEventoLabirinto =
            salvo.avisoEventoLabirinto;

        tempoRestanteAvisoEvento = Mathf.Max(
            0f,
            salvo.tempoRestanteAvisoEvento
        );

        problemaPendente = salvo.problemaPendente;
        problemaResolvido = salvo.problemaResolvido;

        tempoDecorrido = Mathf.Max(
            0f,
            salvo.tempoDecorrido
        );

        saldoAvaliacao = salvo.saldoAvaliacao;
        proximoEvento = salvo.proximoEvento;

        momentosEventos.Clear();

        if (salvo.momentosEventos != null)
        {
            momentosEventos.AddRange(
                salvo.momentosEventos
            );
        }

        proximaNotificacao =
            salvo.proximaNotificacao;

        notificacaoVisivel =
            salvo.notificacaoVisivel;

        tempoRestanteNotificacao =
            salvo.tempoRestanteNotificacao;

        mensagensSorteadas.Clear();

        if (salvo.mensagensSorteadas != null)
        {
            mensagensSorteadas.AddRange(
                salvo.mensagensSorteadas
            );
        }

        momentosNotificacoes.Clear();

        if (salvo.momentosNotificacoes != null)
        {
            momentosNotificacoes.AddRange(
                salvo.momentosNotificacoes
            );
        }

        faseFinalizacao =
            salvo.faseFinalizacao;

        tempoRestanteFinalizacao =
            Mathf.Max(
                0f,
                salvo.tempoRestanteFinalizacao
            );

        if (textoTituloNotificacao != null)
        {
            textoTituloNotificacao.text =
                salvo.tituloNotificacao ?? "";
        }

        if (textoMensagemNotificacao != null)
        {
            textoMensagemNotificacao.text =
                salvo.mensagemNotificacao ?? "";
        }

        if (painelDecisao != null)
            painelDecisao.SetActive(
                salvo.painelDecisao
            );

        if (painelLabirinto != null)
            painelLabirinto.SetActive(
                salvo.painelLabirinto
            );

        if (painelSemaforo != null)
            painelSemaforo.SetActive(
                salvo.painelSemaforo
            );

        if (painelFimViagem != null)
            painelFimViagem.SetActive(
                salvo.painelFimViagem
            );

        if (painelAnimacaoCliente != null)
        {
            painelAnimacaoCliente.SetActive(
                salvo.painelAnimacaoCliente
            );
        }

        if (painelAvaliacaoCliente != null)
        {
            painelAvaliacaoCliente.SetActive(
                salvo.painelAvaliacaoCliente
            );
        }

        if (painelNotificacao != null)
        {
            painelNotificacao.SetActive(
                salvo.notificacaoVisivel ||
                salvo.avisoDeEvento
            );
        }

        if (animatorTransito != null)
        {
            animatorTransito.speed =
                viagemFinalizada
                    ? 0f
                    : velocidadeOriginalAnimacao;
        }

        // Restaura a posicao dentro do labirinto.
        if (salvo.painelLabirinto)
        {
            if (mazeDestination != null)
                mazeDestination.ResetDestination();

            if (mazePlayer != null &&
                dados.labirinto != null)
            {
                Rigidbody2D rb =
                    mazePlayer.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    rb.position =
                        dados.labirinto.posicaoJogador;
                }

                mazePlayer.transform.position =
                    dados.labirinto.posicaoJogador;

                Physics2D.SyncTransforms();
            }
        }

        if (salvo.painelSemaforo)
        {
            SemaforoMinigame semaforo =
                EncontrarSemaforo();

            if (semaforo != null)
            {
                semaforo.RestaurarEstado(
                    dados.semaforo
                );
            }
        }

        // Recupera o aviso antes de um minigame.
        if (avisoDeEventoEmAndamento)
        {
            StartCoroutine(
                AvisarEIniciarMinigame(
                    avisoEventoLabirinto,
                    true
                )
            );
        }

        // Recupera a espera e a animacao do cliente.
        if (faseFinalizacao == 1 ||
            faseFinalizacao == 2)
        {
            if (faseFinalizacao == 2 &&
                animatorCliente != null &&
                painelAnimacaoCliente != null &&
                painelAnimacaoCliente.activeInHierarchy)
            {
                animatorCliente.speed =
                    velocidadeOriginalAnimacaoCliente;

                animatorCliente.Play(
                    0,
                    0,
                    Mathf.Clamp01(
                        salvo.progressoAnimacaoCliente
                    )
                );

                animatorCliente.Update(0f);
            }

            StartCoroutine(
                FluxoPosLabirinto(true)
            );
        }
        else if (faseFinalizacao == 3)
        {
            if (animatorCliente != null &&
                painelAnimacaoCliente != null &&
                painelAnimacaoCliente.activeInHierarchy)
            {
                animatorCliente.Play(
                    0, 0, 0.999f
                );

                animatorCliente.Update(0f);
                animatorCliente.speed = 0f;
            }

            if (evaluationManager != null)
            {
                evaluationManager.ShowRandomNote(
                    ObterSaldoAvaliacao()
                );
            }
        }

        Debug.Log(
            "LogiVery: viagem restaurada " +
            "no segundo " + tempoDecorrido
        );
    }

    private void LimparEstadoViagem()
    {
        viagemIniciada = false;
        viagemAtiva = false;
        viagemPausada = false;
        viagemFinalizada = false;
        transicaoClienteIniciada = false;

        desafioEmAndamento = false;
        houveMinigameNaViagem = false;
        ultimoMinigameFoiLabirinto = false;

        avisoDeEventoEmAndamento = false;
        avisoEventoLabirinto = false;
        tempoRestanteAvisoEvento = 0f;

        problemaPendente = false;
        problemaResolvido = false;

        tempoDecorrido = 0f;
        saldoAvaliacao = 0;

        proximoEvento = 0;
        proximaNotificacao = 0;

        notificacaoVisivel = false;
        tempoRestanteNotificacao = 0f;

        faseFinalizacao = 0;
        tempoRestanteFinalizacao = 0f;

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
