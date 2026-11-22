
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
    [SerializeField] private Image imagemSemaforo;

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

    private float tempoRestanteLuz;
    private float tempoRestanteAviso;
    private bool sucessoEncerramento;
    private string mensagemEncerramento = "";

    private Coroutine rotinaLuzes;
    private Coroutine rotinaAviso;
    private bool avisouGerenciadorAusente;

    private void OnEnable()
    {
        tempoDecorrido = 0f;
        desafioEncerrado = false;
        avisouGerenciadorAusente = false;
        sucessoEncerramento = false;
        mensagemEncerramento = "";
        tempoRestanteAviso = 0f;

        if (textoInstrucao != null)
        {
            textoInstrucao.text =
                "Avance quando o sinal estiver VERDE!";
        }

        if (textoAviso != null)
            textoAviso.text = "";

        if (botaoAvancar != null)
            botaoAvancar.interactable = true;

        estadoAtual = EstadoSinal.Apagado;

        MostrarEstado(
            EstadoSinal.Apagado,
            spriteApagado,
            false
        );

        tempoRestanteLuz =
            Random.Range(0.6f, 1.8f);

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
            while (tempoRestanteLuz > 0f &&
                   !desafioEncerrado)
            {
                tempoRestanteLuz = Mathf.Max(
                    0f,
                    tempoRestanteLuz - Time.deltaTime
                );

                yield return null;
            }

            if (desafioEncerrado)
                yield break;

            AvancarEstadoLuz();
        }

        rotinaLuzes = null;
    }

    private void AvancarEstadoLuz()
    {
        switch (estadoAtual)
        {
            case EstadoSinal.Apagado:
                MostrarEstado(
                    EstadoSinal.Vermelho,
                    spriteVermelho
                );

                tempoRestanteLuz =
                    Random.Range(0.6f, 1.2f);
                break;

            case EstadoSinal.Vermelho:
                MostrarEstado(
                    EstadoSinal.Amarelo,
                    spriteAmarelo
                );

                tempoRestanteLuz =
                    Mathf.Max(0.1f, duracaoAmarelo);
                break;

            case EstadoSinal.Amarelo:
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

                tempoRestanteLuz =
                    Random.Range(minimo, maximo);
                break;

            case EstadoSinal.Verde:
                MostrarEstado(
                    EstadoSinal.Apagado,
                    spriteApagado
                );

                tempoRestanteLuz =
                    Random.Range(0.6f, 1.8f);
                break;
        }
    }

    private void MostrarEstado(
        EstadoSinal novoEstado,
        Sprite novoSprite,
        bool tocarSom = true)
    {
        bool mudouDeEstado =
            novoEstado != estadoAtual;

        estadoAtual = novoEstado;

        if (imagemSemaforo != null)
        {
            imagemSemaforo.sprite = novoSprite;
            imagemSemaforo.color = Color.white;
        }

        if (tocarSom &&
            mudouDeEstado &&
            novoEstado != EstadoSinal.Apagado)
        {
            TocarSomDaLuz();
        }
    }

    private Sprite SpriteDoEstado(EstadoSinal estado)
    {
        switch (estado)
        {
            case EstadoSinal.Vermelho:
                return spriteVermelho;

            case EstadoSinal.Amarelo:
                return spriteAmarelo;

            case EstadoSinal.Verde:
                return spriteVerde;

            default:
                return spriteApagado;
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
                    "Semaforo: gerenciador de sons " +
                    "nao encontrado!",
                    this
                );

                avisouGerenciadorAusente = true;
            }

            return;
        }

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
        sucessoEncerramento = sucesso;
        mensagemEncerramento = mensagem;
        tempoRestanteAviso =
            Mathf.Max(0f, duracaoAviso);

        if (rotinaLuzes != null)
        {
            StopCoroutine(rotinaLuzes);
            rotinaLuzes = null;
        }

        if (botaoAvancar != null)
            botaoAvancar.interactable = false;

        if (textoAviso != null)
            textoAviso.text = mensagem;

        rotinaAviso = StartCoroutine(
            FecharDepoisDoAviso()
        );
    }

    private IEnumerator FecharDepoisDoAviso()
    {
        while (tempoRestanteAviso > 0f)
        {
            tempoRestanteAviso = Mathf.Max(
                0f,
                tempoRestanteAviso - Time.deltaTime
            );

            yield return null;
        }

        rotinaAviso = null;

        if (gerenciadorEntrega != null)
        {
            gerenciadorEntrega.ConcluirSemaforo(
                sucessoEncerramento
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

    public void CapturarEstado(
        LogiVerySaveManager.Semaforo dados)
    {
        if (dados == null)
            return;

        dados.estadoAtual = (int)estadoAtual;
        dados.tempoDecorrido = tempoDecorrido;
        dados.desafioEncerrado = desafioEncerrado;
        dados.tempoRestanteLuz = tempoRestanteLuz;
        dados.tempoRestanteAviso = tempoRestanteAviso;
        dados.sucessoEncerramento = sucessoEncerramento;
        dados.textoAviso = mensagemEncerramento;
    }

    public void RestaurarEstado(
        LogiVerySaveManager.Semaforo dados)
    {
        if (dados == null)
            return;

        StopAllCoroutines();

        rotinaLuzes = null;
        rotinaAviso = null;

        estadoAtual = (EstadoSinal)Mathf.Clamp(
            dados.estadoAtual,
            0,
            3
        );

        tempoDecorrido = Mathf.Max(
            0f,
            dados.tempoDecorrido
        );

        desafioEncerrado = dados.desafioEncerrado;

        tempoRestanteLuz = Mathf.Max(
            0f,
            dados.tempoRestanteLuz
        );

        tempoRestanteAviso = Mathf.Max(
            0f,
            dados.tempoRestanteAviso
        );

        sucessoEncerramento =
            dados.sucessoEncerramento;

        mensagemEncerramento =
            dados.textoAviso ?? "";

        MostrarEstado(
            estadoAtual,
            SpriteDoEstado(estadoAtual),
            false
        );

        if (textoInstrucao != null)
        {
            textoInstrucao.text =
                "Avance quando o sinal estiver VERDE!";
        }

        if (textoAviso != null)
        {
            textoAviso.text = desafioEncerrado
                ? mensagemEncerramento
                : "";
        }

        if (botaoAvancar != null)
        {
            botaoAvancar.interactable =
                !desafioEncerrado;
        }

        if (desafioEncerrado)
        {
            rotinaAviso = StartCoroutine(
                FecharDepoisDoAviso()
            );
        }
        else
        {
            rotinaLuzes = StartCoroutine(
                AlternarLuzes()
            );
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        rotinaLuzes = null;
        rotinaAviso = null;

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
