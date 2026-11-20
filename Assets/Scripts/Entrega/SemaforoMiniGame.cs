
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SemaforoMinigame : MonoBehaviour
{
    [Header("Gerenciador da Entrega")]
    [SerializeField] private DeliveryScreenManager gerenciadorEntrega;

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
    [SerializeField] private float tempoMaximoDesafio = 13f;
    [SerializeField] private float tempoVerdeMinimo = 0.45f;
    [SerializeField] private float tempoVerdeMaximo = 0.85f;
    [SerializeField] private float duracaoAviso = 1.7f;

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

    private void OnEnable()
    {
        // Reinicia o desafio a cada abertura.
        tempoDecorrido = 0f;
        desafioEncerrado = false;

        if (textoInstrucao != null)
        {
            textoInstrucao.text =
                "Avance quando o sinal estiver VERDE!";
        }

        if (textoAviso != null)
            textoAviso.text = "";

        if (botaoAvancar != null)
            botaoAvancar.interactable = true;

        MostrarEstado(
            EstadoSinal.Apagado,
            spriteApagado
        );

        rotinaLuzes = StartCoroutine(AlternarLuzes());
    }

    private void Update()
    {
        if (desafioEncerrado)
            return;

        tempoDecorrido += Time.deltaTime;

        // Impede que o minigame fique aberto para sempre.
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
            // Semaforo apagado por um intervalo aleatorio.
            MostrarEstado(
                EstadoSinal.Apagado,
                spriteApagado
            );

            yield return new WaitForSeconds(
                Random.Range(0.6f, 1.8f)
            );

            // Sinal vermelho.
            MostrarEstado(
                EstadoSinal.Vermelho,
                spriteVermelho
            );

            yield return new WaitForSeconds(
                Random.Range(0.6f, 1.2f)
            );

            // Sinal amarelo.
            MostrarEstado(
                EstadoSinal.Amarelo,
                spriteAmarelo
            );

            yield return new WaitForSeconds(0.4f);

            // Verde aparece rapidamente.
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

            // O ciclo volta ao estado apagado.
        }
    }

    private void MostrarEstado(
        EstadoSinal novoEstado,
        Sprite novoSprite)
    {
        estadoAtual = novoEstado;

        if (imagemSemaforo != null)
        {
            imagemSemaforo.sprite = novoSprite;
            imagemSemaforo.color = Color.white;
        }
    }

    // Chamado pelo BotaoAvancar.
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

        // Congela o semaforo no sprite atual.
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

    private IEnumerator FecharDepoisDoAviso(bool sucesso)
    {
        yield return new WaitForSeconds(duracaoAviso);

        if (gerenciadorEntrega != null)
        {
            gerenciadorEntrega.ConcluirSemaforo(sucesso);
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
        if (rotinaLuzes != null)
        {
            StopCoroutine(rotinaLuzes);
            rotinaLuzes = null;
        }
    }
}
