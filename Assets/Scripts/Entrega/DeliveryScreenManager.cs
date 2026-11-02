
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Events;

public class DeliveryScreenManager : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject painelEntrega;
    [SerializeField] private GameObject painelDecisao;
    [SerializeField] private GameObject painelLabirinto;

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI textoLabirinto;

    [Header("Painéis de finalização")]
    [SerializeField] private GameObject painelAnimacaoCliente;
    [SerializeField] private GameObject painelAvaliacaoCliente;

    [Header("Tempos provisórios")]
    [SerializeField] private float tempoAntesDaAnimacao = 5f;
    [SerializeField] private float tempoAnimacaoCliente = 5f;

    [Header("Continuar Avaliação")]
    [SerializeField] private UnityEvent aoContinuarAvaliacao;

    [Header("Gerenciador de Avaliação")]
    [SerializeField] private CustomerEvaluationManager evaluationManager;

    [Header("Reset do Labirinto")]
    [SerializeField] private MazePlayerManager mazePlayer;
    [SerializeField] private MazeDestination mazeDestination;

    private bool eventoIniciado;
    private bool eventoJaAconteceu;

    // Estado do imprevisto atual
    private bool problemaPendente;
    private bool problemaResolvido;

    private void Update()
    {        
        // Atalho temporário para testes
        if (Keyboard.current != null &&
            Keyboard.current.tKey.wasPressedThisFrame)
        {
            MostrarDecisao();
        }

        // Inicia o sorteio quando a entrega fica ativa
        if (painelEntrega != null &&
            painelEntrega.activeInHierarchy &&
            !eventoIniciado &&
            !eventoJaAconteceu)
        {
            eventoIniciado = true;
            StartCoroutine(AguardarEventoNegativo());
        }

        // Atalho temporário: K conclui o labirinto
        if (Keyboard.current != null &&
            painelLabirinto != null &&
            painelLabirinto.activeInHierarchy &&
            Keyboard.current.kKey.wasPressedThisFrame)
        {
            ConcluirLabirinto();
        }
    }

    private IEnumerator AguardarEventoNegativo()
    {
        float espera = Random.Range(5f, 12f);
        yield return new WaitForSeconds(espera);

        if (painelEntrega != null &&
            painelEntrega.activeInHierarchy &&
            !eventoJaAconteceu)
        {
            MostrarDecisao();
        }
        else
        {
            eventoIniciado = false;
        }
    }

    public void MostrarDecisao()
    {
        if (eventoJaAconteceu)
            return;

        eventoJaAconteceu = true;
        problemaPendente = true;
        problemaResolvido = false;

        painelDecisao.SetActive(true);

        Debug.Log("Evento negativo iniciado: problema de trânsito!");
    }

    public void AceitarLabirinto()
    {
        // Primeiro fecha a decisão
        painelDecisao.SetActive(false);

        // Ativa o labirinto para inicializar seus componentes
        painelLabirinto.SetActive(true);

        // Agora reinicia o destino
        if (mazeDestination != null)
            mazeDestination.ResetDestination();

        // Agora reinicia o jogador
        if (mazePlayer != null)
            mazePlayer.ResetPlayer();

        Physics2D.SyncTransforms();

        Debug.Log("Labirinto aberto e jogador reiniciado.");
    }

    public void RecusarLabirinto()
    {
        painelDecisao.SetActive(false);
        problemaPendente = true;
        problemaResolvido = false;
        
        Debug.Log("O jogador recusou o desafio. O problema continua pendente.");

        StartCoroutine(FluxoPosLabirinto());

    }

    public void ConcluirLabirinto()
    {
        painelLabirinto.SetActive(false);
        problemaPendente = false;
        problemaResolvido = true;

        textoLabirinto.text = "Labirinto Concluído!";
        textoLabirinto.gameObject.SetActive(true);

        StartCoroutine(OcultarMensagem());

        Debug.Log("Problema resolvido! O jogador pode continuar a entrega.");

        StartCoroutine(FluxoPosLabirinto());
    }

    private IEnumerator OcultarMensagem()
    {
        yield return new WaitForSeconds(3f);

        textoLabirinto.gameObject.SetActive(false);
    }

    private IEnumerator FluxoPosLabirinto()
    {
       // aguardar antes de executar animação do cliente
       yield return new WaitForSeconds(tempoAntesDaAnimacao);
       //abrindo painel de animação do cliente
       painelAnimacaoCliente.SetActive(true);
       // tempo de animação do cliente
       yield return new WaitForSeconds(tempoAnimacaoCliente);
       //abrindo painel de avaliação do cliente
       painelAvaliacaoCliente.SetActive(true);

       //gera e exibe a avaliação do cliente
       evaluationManager.ShowRandomNote(); 

       Debug.Log ("Avaliação do Cliente aberta!");
    }

    public void ContinuarAvaliacao()
    {
        painelAnimacaoCliente.SetActive(false);
        painelAvaliacaoCliente.SetActive(false);

        aoContinuarAvaliacao?.Invoke();

        Debug.Log("Avaliação do cliente foi concluída. Continuando o fluxo do jogo.");
    }

    public void ResetDelivery()
    {
        StopAllCoroutines();

        eventoIniciado = false;
        eventoJaAconteceu = false;

        problemaPendente = false;
        problemaResolvido = false;

        if (painelEntrega != null)
            painelEntrega.SetActive(false);

        if (painelDecisao != null)
            painelDecisao.SetActive(false);

        if (mazePlayer != null)
            mazePlayer.ResetPlayer();

        if (mazeDestination != null)
            mazeDestination.ResetDestination();

        if (painelLabirinto != null)
            painelLabirinto.SetActive(false);

        if (painelAnimacaoCliente != null)
            painelAnimacaoCliente.SetActive(false);

        if (painelAvaliacaoCliente != null)
            painelAvaliacaoCliente.SetActive(false);

        if (textoLabirinto != null)
            textoLabirinto.gameObject.SetActive(false);

        Debug.Log("Entrega reiniciada.");
    }
}
    