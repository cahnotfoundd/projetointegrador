
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
    [Min(1)]
    [SerializeField] private int esperaMinima = 1;

    [Min(1)]
    [SerializeField] private int esperaMaxima = 10;

    private int substituicoes;
    private bool aguardando;
    private bool pedidoPerdido;
    private Coroutine busca;

    private float tempoRestanteBusca;
    private int tempoBuscaTotal;

    private void Start()
    {
        OcultarTelas();
    }

    private void OnEnable()
    {
        if (cronometro != null)
        {
            cronometro.AoEsgotarTempo +=
                TempoAcabou;
        }
    }

    private void OnDisable()
    {
        if (cronometro != null)
        {
            cronometro.AoEsgotarTempo -=
                TempoAcabou;
        }

        CancelarBusca();
    }

    private void CancelarBusca()
    {
        if (busca != null)
        {
            StopCoroutine(busca);
            busca = null;
        }
    }

    public void PrepararNovoPedido()
    {
        CancelarBusca();

        substituicoes = 0;
        aguardando = false;
        pedidoPerdido = false;

        tempoRestanteBusca = 0f;
        tempoBuscaTotal = 0;

        OcultarTelas();
    }

    private void TempoAcabou()
    {
        if (aguardando ||
            pedidoPerdido ||
            gerador == null ||
            gerador.EntregadorAtual == null)
        {
            return;
        }

        if (substituicoes == 0)
        {
            aguardando = true;

            if (textoDesistencia != null)
            {
                textoDesistencia.text =
                    gerador.EntregadorAtual.nome +
                    " desistiu da entrega.";
            }

            int minimo = Mathf.Max(
                1,
                Mathf.Min(esperaMinima, esperaMaxima)
            );

            int maximo = Mathf.Max(
                minimo,
                Mathf.Max(esperaMinima, esperaMaxima)
            );

            tempoBuscaTotal =
                Random.Range(minimo, maximo + 1);

            tempoRestanteBusca = tempoBuscaTotal;

            if (telaTempoEsgotado != null)
                telaTempoEsgotado.SetActive(true);

            CancelarBusca();

            busca = StartCoroutine(
                ProcurarEntregador()
            );
        }
        else
        {
            MostrarPedidoPerdido();
        }
    }

    private IEnumerator ProcurarEntregador()
    {
        while (tempoRestanteBusca > 0f)
        {
            // Quando o menu de opcoes pausa o jogo,
            // a busca tambem fica pausada.
            if (Time.timeScale > 0f)
            {
                tempoRestanteBusca = Mathf.Max(
                    0f,
                    tempoRestanteBusca -
                    Time.unscaledDeltaTime
                );
            }

            if (textoContagem != null)
            {
                int decorrido = Mathf.Clamp(
                    tempoBuscaTotal -
                    Mathf.CeilToInt(
                        tempoRestanteBusca
                    ) + 1,
                    1,
                    Mathf.Max(1, tempoBuscaTotal)
                );

                textoContagem.text =
                    $"{decorrido:00}s";
            }

            yield return null;
        }

        if (telaTempoEsgotado != null)
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
            fichaNovoEntregador.sprite =
                novo.imagemCompleta;

            fichaNovoEntregador.enabled =
                novo.imagemCompleta != null;
        }

        if (telaNovoEntregador != null)
            telaNovoEntregador.SetActive(true);
    }

    public void ContinuarComNovoEntregador()
    {
        if (!aguardando ||
            telaNovoEntregador == null ||
            !telaNovoEntregador.activeSelf)
        {
            return;
        }

        if (gerador == null ||
            gerador.EntregadorAtual == null)
        {
            Debug.LogError(
                "Novo entregador nao encontrado!"
            );
            return;
        }

        substituicoes = 1;
        aguardando = false;

        gerador.ReiniciarCronometroDoEntregadorAtual();

        telaNovoEntregador.SetActive(false);

        if (montagem != null)
            montagem.ResetMounting();

        if (embalagem != null)
            embalagem.ResetPackaging();

        if (panelEmbalagem != null)
            panelEmbalagem.SetActive(false);

        if (panelMontagem != null)
            panelMontagem.SetActive(true);
    }

    private void MostrarPedidoPerdido()
    {
        CancelarBusca();

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

    public void ContinuarDepoisDaFalha()
    {
        if (!pedidoPerdido)
            return;

        pedidoPerdido = false;

        if (cronometro != null)
            cronometro.Parar();

        OcultarTelas();

        if (gameplayManager != null)
        {
            gameplayManager.RegistrarPedidoPerdido();
        }
        else
        {
            Debug.LogError(
                "GameplayManager nao atribuido!"
            );
        }
    }

    public void CapturarEstado(
        LogiVerySaveManager.Substituicao dados)
    {
        if (dados == null)
            return;

        dados.quantidadeSubstituicoes = substituicoes;
        dados.aguardando = aguardando;
        dados.pedidoPerdido = pedidoPerdido;
        dados.tempoRestanteBusca = tempoRestanteBusca;
        dados.tempoBuscaTotal = tempoBuscaTotal;

        dados.novoEntregadorVisivel =
            telaNovoEntregador != null &&
            telaNovoEntregador.activeSelf;
    }

    public void RestaurarEstado(
        LogiVerySaveManager.Substituicao dados)
    {
        if (dados == null)
            return;

        CancelarBusca();
        OcultarTelas();

        substituicoes =
            Mathf.Max(0, dados.quantidadeSubstituicoes);

        aguardando = dados.aguardando;
        pedidoPerdido = dados.pedidoPerdido;

        tempoRestanteBusca =
            Mathf.Max(0f, dados.tempoRestanteBusca);

        tempoBuscaTotal = Mathf.Max(
            1, dados.tempoBuscaTotal
        );

        if (pedidoPerdido)
        {
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

            return;
        }

        if (!aguardando)
            return;

        if (dados.novoEntregadorVisivel)
        {
            if (gerador != null &&
                gerador.EntregadorAtual != null &&
                fichaNovoEntregador != null)
            {
                fichaNovoEntregador.sprite =
                    gerador.EntregadorAtual.imagemCompleta;

                fichaNovoEntregador.enabled =
                    fichaNovoEntregador.sprite != null;
            }

            if (telaNovoEntregador != null)
                telaNovoEntregador.SetActive(true);
        }
        else
        {
            if (textoDesistencia != null &&
                gerador != null &&
                gerador.EntregadorAtual != null)
            {
                textoDesistencia.text =
                    gerador.EntregadorAtual.nome +
                    " desistiu da entrega.";
            }

            if (telaTempoEsgotado != null)
                telaTempoEsgotado.SetActive(true);

            busca = StartCoroutine(
                ProcurarEntregador()
            );
        }
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
