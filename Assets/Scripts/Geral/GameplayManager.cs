
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameplayManager : MonoBehaviour
{
    [Header("Order Pad")]
    [SerializeField] private Button[] buttonsOrder;

    [Header("Gameplay Panels")]
    [SerializeField] private GameObject panelOrder;
    [SerializeField] private GameObject panelMounting;
    [SerializeField] private GameObject panelPackaging;
    [SerializeField] private GameObject panelRoutes;
    [SerializeField] private GameObject panelDelivery;

    [Header("Final Panels")]
    [SerializeField] private GameObject panelResult;
    [SerializeField] private GameObject panelVictory;

    [Header("Evaluation")]
    [SerializeField]
    private CustomerEvaluationManager evaluationManager;

    [Header("Daily Report")]
    [SerializeField] private TextMeshProUGUI textoDia;

    [SerializeField]
    private TextMeshProUGUI textoAvaliacoesPositivas;

    [SerializeField]
    private TextMeshProUGUI textoOtimizacaoRotas;

    [SerializeField]
    private TextMeshProUGUI textoAvisos;

    [Header("Game Rules")]
    [SerializeField] private int entregasPorDia = 3;

    [SerializeField]
    private int metaAvaliacoesPositivas = 10;

    [Header("Reset dos Sistemas")]
    [SerializeField] private PackagingManager packagingManager;
    [SerializeField] private RouteDrawingManager routeDrawingManager;
    [SerializeField] private MountingManager mountingManager;
    [SerializeField] private DeliveryScreenManager deliveryScreenManager;

    [Header("Pedidos perdidos")]
    [SerializeField]
    private GeradorPedidoAleatorio geradorPedidoAleatorio;

    [Header("Menu de opcoes do Gameplay")]
    [SerializeField] private GameObject painelMenuOpcoes;
    [SerializeField] private string cenaMenuPrincipal = "MenuPrincipal";
    [SerializeField] private bool pausarJogoAoAbrirMenu = true;

    private CronometroPreparacao cronometroPreparacao;
    private SistemaSubstituicaoEntregador sistemaSubstituicao;

    private bool menuOpcoesAberto;
    private float escalaTempoAntesMenu = 1f;

    private int SelectedOrderIndex = -1;

    public int selectedOrderIndex =>
        SelectedOrderIndex;

    private int entregasDoDia;
    private int diaAtual = 1;
    private int totalAvaliacoesPositivas;
    private int avaliacoesPositivasDia;
    private int avaliacoesNegativasDoDia;

    private IEnumerator Start()
    {
        Time.timeScale = 1f;

        LocalizarSistemas();

        if (painelMenuOpcoes != null)
            painelMenuOpcoes.SetActive(false);

        if (panelVictory != null)
            panelVictory.SetActive(false);

        ShowOrderPad();

        if (buttonsOrder != null)
        {
            for (int i = 0; i < buttonsOrder.Length; i++)
            {
                int index = i;

                if (buttonsOrder[i] == null)
                    continue;

                Button botao = buttonsOrder[i];

                if (botao.GetComponent<SomHoverPapel>() == null)
                {
                    botao.gameObject.AddComponent<
                        SomHoverPapel
                    >();
                }

                botao.onClick.AddListener(
                    () => SelectOrder(index)
                );
            }
        }

        // Aguarda a inicializacao dos outros sistemas.
        yield return null;

        if (LogiVerySaveManager.TentarCarregar(
            out LogiVerySaveManager.Dados dados))
        {
            if (!RestaurarProgresso(dados))
            {
                Debug.LogError(
                    "LogiVery: nao foi possivel restaurar " +
                    "a partida. O arquivo salvo foi mantido."
                );
            }
        }
        else
        {
            Debug.Log(
                "LogiVery: iniciando partida nova."
            );
        }
    }

    private void LocalizarSistemas()
    {
        // Compatibilidade com Unity 6.4.
        // FindAnyObjectByType substitui o metodo
        // obsoleto FindFirstObjectByType.

        if (geradorPedidoAleatorio == null)
        {
            geradorPedidoAleatorio =
                FindAnyObjectByType<
                    GeradorPedidoAleatorio
                >(FindObjectsInactive.Include);
        }

        cronometroPreparacao =
            FindAnyObjectByType<
                CronometroPreparacao
            >(FindObjectsInactive.Include);

        sistemaSubstituicao =
            FindAnyObjectByType<
                SistemaSubstituicaoEntregador
            >(FindObjectsInactive.Include);
    }

    private void SelectOrder(int index)
    {
        SelectedOrderIndex = index;

        if (panelOrder != null)
            panelOrder.SetActive(false);

        if (panelMounting != null)
            panelMounting.SetActive(true);

        Debug.Log(
            "Pedido selecionado: " + (index + 1)
        );
    }

    private void ShowOrderPad()
    {
        if (panelOrder != null)
            panelOrder.SetActive(true);

        if (panelMounting != null)
            panelMounting.SetActive(false);

        if (panelPackaging != null)
            panelPackaging.SetActive(false);

        if (panelRoutes != null)
            panelRoutes.SetActive(false);

        if (panelDelivery != null)
            panelDelivery.SetActive(false);

        if (panelResult != null)
            panelResult.SetActive(false);

        SelectedOrderIndex = -1;
    }

    public void RegistrarAvaliacao()
    {
        if (evaluationManager == null)
        {
            Debug.LogError(
                "CustomerEvaluationManager nao conectado!"
            );
            return;
        }

        int nota = evaluationManager.deliveryNote;

        entregasDoDia++;

        // Somente cinco estrelas contam.
        if (nota == 5)
        {
            totalAvaliacoesPositivas++;
            avaliacoesPositivasDia++;

            Debug.Log(
                "Entrega com 5 estrelas registrada!"
            );
        }

        Debug.Log(
            "Nota final da entrega: " + nota
        );

        Debug.Log(
            "Entregas do dia: " +
            entregasDoDia + "/" + entregasPorDia
        );

        // A meta acumulada permanece secreta.
        if (totalAvaliacoesPositivas >=
            metaAvaliacoesPositivas)
        {
            MostrarVitoria();
            return;
        }

        if (entregasDoDia >= entregasPorDia)
        {
            MostrarRelatorio();
        }
        else
        {
            ShowOrderPad();
            ReiniciarEtapas();
        }

        SalvarProgresso();
    }

    private void MostrarRelatorio()
    {
        ShowOrderPad();

        if (panelOrder != null)
            panelOrder.SetActive(false);

        if (panelResult != null)
            panelResult.SetActive(true);

        AtualizarRelatorio();

        Debug.Log(
            "Relatorio do dia " +
            diaAtual + " aberto."
        );
    }

    private void AtualizarRelatorio()
    {
        if (textoDia != null)
            textoDia.text = "Dia " + diaAtual;

        if (textoAvaliacoesPositivas != null)
        {
            textoAvaliacoesPositivas.text =
                "Entregas bem-sucedidas do dia: " +
                avaliacoesPositivasDia;
        }

        if (textoOtimizacaoRotas != null)
        {
            textoOtimizacaoRotas.text =
                "Otimização de Rotas:\n" +
                "- Resumo das entregas concluídas.";
        }

        if (textoAvisos != null)
        {
            textoAvisos.text =
                avaliacoesNegativasDoDia == 0
                    ? "Avisos: Nenhum pedido perdido."
                    : $"Pedidos não entregues: " +
                      $"{avaliacoesNegativasDoDia} " +
                      $"(avaliações negativas).";
        }
    }

    private void ReiniciarEtapas()
    {
        if (mountingManager != null)
            mountingManager.ResetMounting();

        if (packagingManager != null)
            packagingManager.ResetPackaging();

        if (routeDrawingManager != null)
            routeDrawingManager.ResetRoute();

        if (deliveryScreenManager != null)
            deliveryScreenManager.ResetDelivery();

        if (buttonsOrder != null)
        {
            foreach (Button button in buttonsOrder)
            {
                if (button == null)
                    continue;

                AnimationButton animationButton =
                    button.GetComponent<AnimationButton>();

                if (animationButton != null)
                    animationButton.ResetAnimation();
            }
        }

        Debug.Log(
            "Todas as etapas foram reiniciadas."
        );
    }

    public void ComecarProximoDia()
    {
        diaAtual++;
        entregasDoDia = 0;
        avaliacoesNegativasDoDia = 0;
        avaliacoesPositivasDia = 0;

        ShowOrderPad();
        ReiniciarEtapas();

        SalvarProgresso();

        Debug.Log(
            "Comecando o dia " + diaAtual
        );
    }

    private void MostrarVitoria()
    {
        // A partida foi concluida.
        // O progresso deixa de estar disponivel.
        ApagarProgressoSalvo();

        if (panelOrder != null)
            panelOrder.SetActive(false);

        if (panelMounting != null)
            panelMounting.SetActive(false);

        if (panelPackaging != null)
            panelPackaging.SetActive(false);

        if (panelRoutes != null)
            panelRoutes.SetActive(false);

        if (panelDelivery != null)
            panelDelivery.SetActive(false);

        if (panelResult != null)
            panelResult.SetActive(false);

        if (panelVictory != null)
            panelVictory.SetActive(true);

        LogiVeryEfeitosSonoros.Instancia?
            .TocarComemoracaoVitoria();

        Debug.Log(
            "VITORIA! Partida encerrada."
        );
    }

    public void RegistrarPedidoPerdido()
    {
        entregasDoDia++;
        avaliacoesNegativasDoDia++;

        Debug.Log(
            $"Pedido perdido! Pedidos do dia: " +
            $"{entregasDoDia}/{entregasPorDia}"
        );

        if (entregasDoDia >= entregasPorDia)
        {
            MostrarRelatorio();
        }
        else
        {
            ReiniciarEtapas();
            ShowOrderPad();

            if (geradorPedidoAleatorio != null)
            {
                geradorPedidoAleatorio
                    .GerarNovoPedido();
            }
        }

        SalvarProgresso();
    }

    // ==========================================
    // MENU DE OPCOES
    // ==========================================

    public void AbrirMenuOpcoes()
    {
        if (painelMenuOpcoes == null)
        {
            Debug.LogError(
                "GameplayManager: painel de opcoes " +
                "nao atribuido!",
                this
            );
            return;
        }

        if (menuOpcoesAberto)
            return;

        escalaTempoAntesMenu = Time.timeScale;
        menuOpcoesAberto = true;

        painelMenuOpcoes.SetActive(true);

        if (pausarJogoAoAbrirMenu)
            Time.timeScale = 0f;
    }

    public void FecharMenuOpcoes()
    {
        if (painelMenuOpcoes != null)
            painelMenuOpcoes.SetActive(false);

        if (menuOpcoesAberto &&
            pausarJogoAoAbrirMenu)
        {
            Time.timeScale = escalaTempoAntesMenu;
        }

        menuOpcoesAberto = false;
    }

    public void SalvarEVoltarAoMenu()
    {
        if (!SalvarProgresso())
        {
            Debug.LogError(
                "LogiVery: nao foi possivel salvar. " +
                "O jogador permanecera na Gameplay."
            );
            return;
        }

        FecharMenuOpcoes();
        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(cenaMenuPrincipal))
        {
            Debug.LogError(
                "Cena MenuPrincipal nao configurada!",
                this
            );
            return;
        }

        SceneManager.LoadScene(cenaMenuPrincipal);
    }

    // ==========================================
    // CAPTURA E RESTAURACAO
    // ==========================================

    private static bool EstaAberto(GameObject obj)
    {
        return obj != null && obj.activeSelf;
    }

    private string DeterminarEtapaAtual()
    {
        if (EstaAberto(panelVictory))
            return "Vitoria";

        if (EstaAberto(panelResult))
            return "Relatorio";

        if (EstaAberto(panelDelivery))
            return "Entrega";

        if (EstaAberto(panelRoutes))
            return "Rotas";

        if (EstaAberto(panelPackaging))
            return "Embalagem";

        if (EstaAberto(panelMounting))
            return "Montagem";

        return "Pedidos";
    }

    private bool SalvarProgresso()
    {
        LocalizarSistemas();

        if (geradorPedidoAleatorio == null)
        {
            Debug.LogError(
                "LogiVery: gerador nao encontrado."
            );
            return false;
        }

        var dados = new LogiVerySaveManager.Dados
        {
            diaAtual = diaAtual,
            entregasDoDia = entregasDoDia,
            estrelasAcumuladas =
                totalAvaliacoesPositivas,
            estrelasDoDia =
                avaliacoesPositivasDia,
            pedidosPerdidosDoDia =
                avaliacoesNegativasDoDia,
            etapaAtual = DeterminarEtapaAtual()
        };

        if (dados.etapaAtual == "Vitoria")
        {
            ApagarProgressoSalvo();
            return false;
        }

        if (!geradorPedidoAleatorio
            .CapturarPedidosParaSalvamento(dados))
        {
            return false;
        }

        bool etapaComPedido =
            dados.etapaAtual == "Montagem" ||
            dados.etapaAtual == "Embalagem" ||
            dados.etapaAtual == "Rotas" ||
            dados.etapaAtual == "Entrega";

        if (!etapaComPedido)
        {
            dados.pedidoSelecionado = -1;
        }
        else if (dados.pedidoSelecionado < 0)
        {
            Debug.LogError(
                "LogiVery: etapa ativa sem pedido " +
                "selecionado. Salvamento cancelado."
            );
            return false;
        }

        if (etapaComPedido)
        {
            foreach (GameObject obj in
                geradorPedidoAleatorio.ProdutosCriados)
            {
                if (obj == null)
                    continue;

                ProdutoArrastavel produto =
                    obj.GetComponent<ProdutoArrastavel>();

                if (produto != null)
                {
                    dados.produtosMontagem.Add(
                        produto.CapturarParaSalvamento()
                    );
                }
            }
        }

        if (cronometroPreparacao != null)
        {
            cronometroPreparacao.CapturarEstado(
                dados.cronometro
            );
        }

        if (packagingManager != null)
        {
            packagingManager.CapturarEstado(
                dados.embalagem
            );
        }

        if (routeDrawingManager != null)
        {
            routeDrawingManager.CapturarEstado(
                dados.rota
            );
        }

        if (sistemaSubstituicao != null)
        {
            sistemaSubstituicao.CapturarEstado(
                dados.substituicao
            );
        }

        if (deliveryScreenManager != null &&
            dados.etapaAtual == "Entrega")
        {
            deliveryScreenManager.CapturarEstado(dados);
        }

        if (evaluationManager != null)
        {
            dados.avaliacao.notaPedido =
                evaluationManager.orderNote;

            dados.avaliacao.notaEntrega =
                evaluationManager.deliveryNote;
        }

        return LogiVerySaveManager.Salvar(dados);
    }

    private bool RestaurarProgresso(
        LogiVerySaveManager.Dados dados)
    {
        LocalizarSistemas();

        if (dados == null ||
            geradorPedidoAleatorio == null)
        {
            return false;
        }

        // Limpa as etapas antes de recuperar os dados.
        ShowOrderPad();
        ReiniciarEtapas();

        // Restaura os mesmos tres pedidos.
        if (!geradorPedidoAleatorio
            .RestaurarPedidosDoSalvamento(dados))
        {
            return false;
        }

        diaAtual = Mathf.Max(1, dados.diaAtual);

        entregasDoDia = Mathf.Clamp(
            dados.entregasDoDia,
            0,
            entregasPorDia
        );

        totalAvaliacoesPositivas = Mathf.Max(
            0,
            dados.estrelasAcumuladas
        );

        avaliacoesPositivasDia = Mathf.Clamp(
            dados.estrelasDoDia,
            0,
            entregasDoDia
        );

        avaliacoesNegativasDoDia = Mathf.Clamp(
            dados.pedidosPerdidosDoDia,
            0,
            entregasDoDia
        );

        if (dados.etapaAtual == "Relatorio")
        {
            MostrarRelatorio();
            return true;
        }

        if (dados.etapaAtual == "Pedidos")
        {
            ShowOrderPad();
            return true;
        }

        if (!geradorPedidoAleatorio
            .RestaurarPedidoSelecionado(dados))
        {
            return false;
        }

        SelectedOrderIndex = dados.pedidoSelecionado;

        // Recupera as posicoes dos produtos.
        int indiceProduto = 0;

        foreach (GameObject obj in
            geradorPedidoAleatorio.ProdutosCriados)
        {
            if (obj == null)
                continue;

            if (dados.produtosMontagem != null &&
                indiceProduto <
                dados.produtosMontagem.Count)
            {
                ProdutoArrastavel produto =
                    obj.GetComponent<ProdutoArrastavel>();

                if (produto != null)
                {
                    produto.RestaurarDoSalvamento(
                        dados.produtosMontagem[
                            indiceProduto
                        ]
                    );
                }
            }

            indiceProduto++;
        }

        if (mountingManager != null)
            mountingManager.CheckOrderCompletion();

        if (packagingManager != null)
        {
            packagingManager.RestaurarEstado(
                dados.embalagem
            );
        }

        if (routeDrawingManager != null)
        {
            routeDrawingManager.RestaurarEstado(
                dados.rota
            );
        }

        if (cronometroPreparacao != null)
        {
            cronometroPreparacao.RestaurarEstado(
                dados.cronometro,
                geradorPedidoAleatorio.EntregadorAtual
            );
        }

        if (sistemaSubstituicao != null)
        {
            sistemaSubstituicao.RestaurarEstado(
                dados.substituicao
            );
        }

        if (panelOrder != null)
            panelOrder.SetActive(false);

        if (panelMounting != null)
        {
            panelMounting.SetActive(
                dados.etapaAtual == "Montagem"
            );
        }

        if (panelPackaging != null)
        {
            panelPackaging.SetActive(
                dados.etapaAtual == "Embalagem"
            );
        }

        if (panelRoutes != null)
        {
            panelRoutes.SetActive(
                dados.etapaAtual == "Rotas"
            );
        }

        if (panelDelivery != null)
        {
            panelDelivery.SetActive(
                dados.etapaAtual == "Entrega"
            );
        }

        if (dados.etapaAtual == "Entrega" &&
            deliveryScreenManager != null)
        {
            deliveryScreenManager.RestaurarEstado(
                dados
            );
        }

        Debug.Log(
            "LogiVery: partida restaurada. Dia " +
            diaAtual + " | Etapa: " +
            dados.etapaAtual
        );

        return true;
    }

    // ==========================================
    // ACESSO PELO MENU PRINCIPAL
    // ==========================================

    public static bool ExisteProgressoSalvo()
    {
        return LogiVerySaveManager
            .ExisteSalvamento();
    }

    public static void ApagarProgressoSalvo()
    {
        LogiVerySaveManager.ApagarSalvamento();

        // Remove somente a chave antiga de progresso.
        // Nao apaga configuracoes de musica ou SFX.
        PlayerPrefs.DeleteKey(
            "LogiVery_Progresso_v1"
        );

        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (menuOpcoesAberto &&
            pausarJogoAoAbrirMenu)
        {
            Time.timeScale = escalaTempoAntesMenu;
        }
    }
}
