
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SemaforoMinigame : MonoBehaviour
{
    [Header("Gerenciador da Entrega")]
    [SerializeField]
    private DeliveryScreenManager gerenciadorEntrega;

    [Header("Imagem unica do semaforo")]
    [SerializeField]
    private Image imagemSemaforo;

    [Header("Sprites dos quatro estados")]
    [SerializeField] private Sprite spriteApagado;
    [SerializeField] private Sprite spriteVermelho;
    [SerializeField] private Sprite spriteAmarelo;
    [SerializeField] private Sprite spriteVerde;

    [Header("Interface")]
    [SerializeField] private TMP_Text textoInstrucao;
    [SerializeField] private TMP_Text textoAviso;
    [SerializeField] private Button botaoAvancar;

    [Header("Dificuldade")]
    [SerializeField]
    private float tempoMaximoDesafio = 13f;

    [SerializeField]
    private float tempoVerdeMinimo = 0.45f;

    [SerializeField]
    private float tempoVerdeMaximo = 0.85f;

    [SerializeField]
    private float duracaoAviso = 1.7f;

    [Header("Duracao da luz amarela")]
    [SerializeField]
    private float duracaoAmarelo = 0.55f;

    [Header("Diagnostico do som")]
    [SerializeField]
    private bool mostrarLogsDeAudio = true;

    private enum EstadoSinal
    {
        Apagado,
        Vermelho,
        Amarelo,
        Verde
    }

    private EstadoSinal estadoAtual;

    private float tempoDecorrido;
    private bool desafioEncerrado;

    private Coroutine rotinaLuzes;
    private bool avisouGerenciadorAusente;

    private void OnEnable()
    {
        tempoDecorrido = 0f;
        desafioEncerrado = false;
        avisouGerenciadorAusente = false;

        estadoAtual = EstadoSinal.Apagado;

        if (textoInstrucao != null)
        {
            textoInstrucao.text =
                "Avance quando o sinal estiver VERDE!";
        }

        if (textoAviso != null)
        {
            textoAviso.text = "";
        }

        if (botaoAvancar != null)
        {
            botaoAvancar.interactable = true;
        }

        MostrarEstado(
            EstadoSinal.Apagado,
            spriteApagado
        );

        rotinaLuzes = StartCoroutine(
            AlternarLuzes()
        );
    }

    private void Update()
    {
        if (desafioEncerrado)
            return;

        tempoDecorrido += Time.deltaTime;

        if (tempoDecorrido >= tempoMaximoDesafio)
        {
            EncerrarDesafio(
                false,
                "Tempo esgotado! A entrega atrasou alguns minutos."
            );
        }
    }

    private IEnumerator AlternarLuzes()
    {
        while (!desafioEncerrado)
        {
            MostrarEstado(
                EstadoSinal.Apagado,
                spriteApagado
            );

            yield return new WaitForSeconds(
                Random.Range(0.6f, 1.8f)
            );

            if (desafioEncerrado)
                yield break;

            MostrarEstado(
                EstadoSinal.Vermelho,
                spriteVermelho
            );

            yield return new WaitForSeconds(
                Random.Range(0.6f, 1.2f)
            );

            if (desafioEncerrado)
                yield break;

            MostrarEstado(
                EstadoSinal.Amarelo,
                spriteAmarelo
            );

            yield return new WaitForSeconds(
                Mathf.Max(0.1f, duracaoAmarelo)
            );

            if (desafioEncerrado)
                yield break;

            MostrarEstado(
                EstadoSinal.Verde,
                spriteVerde
            );

            float minimo = Mathf.Max(
                0.1f,
                Mathf.Min(
                    tempoVerdeMinimo,
                    tempoVerdeMaximo
                )
            );

            float maximo = Mathf.Max(
                minimo,
                Mathf.Max(
                    tempoVerdeMinimo,
                    tempoVerdeMaximo
                )
            );

            yield return new WaitForSeconds(
                Random.Range(minimo, maximo)
            );
        }
    }

    private void MostrarEstado(
        EstadoSinal novoEstado,
        Sprite novoSprite)
    {
        bool mudouDeEstado =
            novoEstado != estadoAtual;

        estadoAtual = novoEstado;

        if (imagemSemaforo != null)
        {
            imagemSemaforo.sprite = novoSprite;
            imagemSemaforo.color = Color.white;
        }

        // Cada luz acesa solicita o som
        // exatamente quando o sprite muda.
        if (mudouDeEstado &&
            novoEstado != EstadoSinal.Apagado)
        {
            TocarSomDaLuz();
        }
    }

    private void TocarSomDaLuz()
    {
        LogiVeryEfeitosSonoros sons =
            LogiVeryEfeitosSonoros.Instancia;

        if (sons == null)
        {
            if (!avisouGerenciadorAusente)
            {
                Debug.LogError(
                    "Semaforo: LogiVeryEfeitosSonoros " +
                    "nao encontrado! Confira o AudioManager.",
                    this
                );

                avisouGerenciadorAusente = true;
            }

            return;
        }

        // Agora usa o sistema centralizado:
        // AudioManager > Semaforo.
        sons.TocarLuzSemaforo();

        if (mostrarLogsDeAudio)
        {
            Debug.Log(
                "Semaforo: som solicitado para a luz " +
                estadoAtual,
                this
            );
        }
    }

    // Permite testar o audio sem esperar
    // o semaforo mudar de cor.
    [ContextMenu("TESTAR SOM DO SEMAFORO (Play Mode)")]
    private void TestarSomDoSemaforo()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Entre no Play Mode para testar.",
                this
            );

            return;
        }

        TocarSomDaLuz();
    }

    public void TentarAvancar()
    {
        if (desafioEncerrado)
            return;

        if (estadoAtual == EstadoSinal.Verde)
        {
            EncerrarDesafio(
                true,
                "Muito bem! Sua rota foi otimizada!"
            );
        }
        else
        {
            EncerrarDesafio(
                false,
                "Momento errado! A entrega atrasou alguns minutos."
            );
        }
    }

    private void EncerrarDesafio(
        bool sucesso,
        string mensagem)
    {
        if (desafioEncerrado)
            return;

        desafioEncerrado = true;

        if (rotinaLuzes != null)
        {
            StopCoroutine(rotinaLuzes);
            rotinaLuzes = null;
        }

        if (botaoAvancar != null)
            botaoAvancar.interactable = false;

        if (textoAviso != null)
            textoAviso.text = mensagem;

        StartCoroutine(
            FecharDepoisDoAviso(sucesso)
        );
    }

    private IEnumerator FecharDepoisDoAviso(
        bool sucesso)
    {
        yield return new WaitForSeconds(
            duracaoAviso
        );

        if (gerenciadorEntrega != null)
        {
            gerenciadorEntrega.ConcluirSemaforo(
                sucesso
            );
        }
        else
        {
            Debug.LogError(
                "Semaforo: gerenciador nao configurado!",
                this
            );
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        rotinaLuzes = null;

        // Evita que um bipe continue tocando
        // depois que o minigame for fechado.
        LogiVeryEfeitosSonoros sons =
            LogiVeryEfeitosSonoros.Instancia;

        if (sons == null)
            return;

        Transform objetoSom =
            sons.transform.Find("Semaforo");

        if (objetoSom == null)
            return;

        AudioSource fonte =
            objetoSom.GetComponent<AudioSource>();

        if (fonte != null)
            fonte.Stop();
    }
}
