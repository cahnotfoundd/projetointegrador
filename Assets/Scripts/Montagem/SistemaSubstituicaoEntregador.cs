
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SistemaSubstituicaoEntregador : MonoBehaviour
{
    [Header("Sistemas")]
    [SerializeField] private GeradorPedidoAleatorio gerador;
    [SerializeField] private CronometroPreparacao cronometro;
    [SerializeField] private MountingManager montagem;
    [SerializeField] private PackagingManager embalagem;
    [SerializeField] private GameplayManager gameplayManager;

    [Header("Paineis principais")]
    [SerializeField] private GameObject panelMontagem;
    [SerializeField] private GameObject panelEmbalagem;

    [Header("Telas da substituicao")]
    [SerializeField] private GameObject telaTempoEsgotado;
    [SerializeField] private GameObject telaNovoEntregador;
    [SerializeField] private GameObject telaPedidoPerdido;

    [Header("Elementos visuais")]
    [SerializeField] private TMP_Text textoDesistencia;
    [SerializeField] private TMP_Text textoContagem;
    [SerializeField] private Image fichaNovoEntregador;
    [SerializeField] private TMP_Text textoDesistenciaFinal;

    [Header("Tempo de procura")]
    [Min(1)] [SerializeField] private int esperaMinima = 1;
    [Min(1)] [SerializeField] private int esperaMaxima = 10;

    private int substituicoes;
    private bool aguardando;
    private bool pedidoPerdido;
    private Coroutine busca;

    private void Start()
    {
        OcultarTelas();
    }

    private void OnEnable()
    {
        if (cronometro != null)
            cronometro.AoEsgotarTempo += TempoAcabou;
    }

    private void OnDisable()
    {
        if (cronometro != null)
            cronometro.AoEsgotarTempo -= TempoAcabou;

        if (busca != null)
        {
            StopCoroutine(busca);
            busca = null;
        }
    }

    // Reinicia o controle para um pedido novo.
    public void PrepararNovoPedido()
    {
        if (busca != null)
        {
            StopCoroutine(busca);
            busca = null;
        }

        substituicoes = 0;
        aguardando = false;
        pedidoPerdido = false;

        OcultarTelas();
    }

    private void TempoAcabou()
    {
        if (aguardando || pedidoPerdido || gerador == null)
            return;

        // Primeira desistência: procurar substituto.
        if (substituicoes == 0)
        {
            aguardando = true;

            if (textoDesistencia != null)
                textoDesistencia.text =
                    gerador.EntregadorAtual.nome +
                    " desistiu da entrega.";

            telaTempoEsgotado.SetActive(true);
            busca = StartCoroutine(ProcurarEntregador());
        }
        else
        {
            // Segunda desistência: pedido perdido.
            MostrarPedidoPerdido();
        }
    }

    private IEnumerator ProcurarEntregador()
    {
        int minimo = Mathf.Max(
            1, Mathf.Min(esperaMinima, esperaMaxima));

        int maximo = Mathf.Max(
            minimo, Mathf.Max(esperaMinima, esperaMaxima));

        int espera = Random.Range(minimo, maximo + 1);

        
        for (int i = 1; i <= espera; i++)
        {
            if (textoContagem != null)
                textoContagem.text = $"{i:00}s";

            yield return new WaitForSecondsRealtime(1f);
        }


        telaTempoEsgotado.SetActive(false);
        busca = null;

        if (!gerador.SortearEntregadorSubstituto(
            out GeradorPedidoAleatorio.Entregador novo))
        {
            MostrarPedidoPerdido();
            yield break;
        }

        if (fichaNovoEntregador != null)
        {
            fichaNovoEntregador.sprite = novo.imagemCompleta;
            fichaNovoEntregador.enabled =
                novo.imagemCompleta != null;
        }

        telaNovoEntregador.SetActive(true);
    }

    // CONTINUAR da tela do novo entregador.
    
    public void ContinuarComNovoEntregador()
    {
        if (!aguardando ||
            telaNovoEntregador == null ||
            !telaNovoEntregador.activeSelf)
            return;

        if (gerador == null || gerador.EntregadorAtual == null)
        {
            Debug.LogError("Novo entregador não encontrado!");
            return;
        }

        // Registra que o pedido já recebeu um substituto.
        substituicoes = 1;
        aguardando = false;

        // PRIMEIRO: atualiza a foto e reinicia o tempo.
        // Usa o mesmo cronômetro do pedido original.
        gerador.ReiniciarCronometroDoEntregadorAtual();

        // Depois, fecha a tela do novo entregador.
        telaNovoEntregador.SetActive(false);

        // Reinicia os produtos do mesmo pedido.
        if (montagem != null)
            montagem.ResetMounting();

        // Reinicia a embalagem.
        if (embalagem != null)
            embalagem.ResetPackaging();

        // Retorna à montagem.
        if (panelEmbalagem != null)
            panelEmbalagem.SetActive(false);

        if (panelMontagem != null)
            panelMontagem.SetActive(true);
    }


    private void MostrarPedidoPerdido()
    {
        aguardando = false;
        pedidoPerdido = true;

        if (telaTempoEsgotado != null)
            telaTempoEsgotado.SetActive(false);

        if (telaNovoEntregador != null)
            telaNovoEntregador.SetActive(false);

        if (textoDesistenciaFinal != null &&
            gerador != null &&
            gerador.EntregadorAtual != null)
        {
            textoDesistenciaFinal.text =
                gerador.EntregadorAtual.nome +
                " desistiu da entrega.";
        }

        if (telaPedidoPerdido != null)
            telaPedidoPerdido.SetActive(true);
    }

    // CONTINUAR da tela do pedido perdido.
    public void ContinuarDepoisDaFalha()
    {
        if (!pedidoPerdido)
            return;

        // Impede contar a mesma falha duas vezes.
        pedidoPerdido = false;

        if (cronometro != null)
            cronometro.Parar();

        OcultarTelas();

        // O GameplayManager escolhe o destino.
        if (gameplayManager != null)
            gameplayManager.RegistrarPedidoPerdido();
        else
            Debug.LogError("GameplayManager não atribuído!");
    }

    private void OcultarTelas()
    {
        if (telaTempoEsgotado != null)
            telaTempoEsgotado.SetActive(false);

        if (telaNovoEntregador != null)
            telaNovoEntregador.SetActive(false);

        if (telaPedidoPerdido != null)
            telaPedidoPerdido.SetActive(false);
    }
}
