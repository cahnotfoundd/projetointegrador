
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameplayManager : MonoBehaviour
{
    private const string ChaveSalvamento =
        "LogiVery_Progresso_v1";

    [Serializable]
    private class DadosSalvamento
    {
        public int versao = 1;
        public int diaAtual;
        public int entregasDoDia;
        public int totalAvaliacoesPositivas;
        public int avaliacoesPositivasDia;
        public int avaliacoesNegativasDoDia;
    }

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
    [SerializeField]
    private RouteDrawingManager routeDrawingManager;
    [SerializeField] private MountingManager mountingManager;
    [SerializeField]
    private DeliveryScreenManager deliveryScreenManager;

    [Header("Pedidos perdidos")]
    [SerializeField]
    private GeradorPedidoAleatorio geradorPedidoAleatorio;

    [Header("Menu de opcoes do Gameplay")]
    [SerializeField]
    private GameObject painelMenuOpcoes;

    [SerializeField]
    private string cenaMenuPrincipal = "MenuPrincipal";

    [SerializeField]
    private bool pausarJogoAoAbrirMenu = true;

    private bool menuOpcoesAberto;
    private float escalaTempoAntesMenu = 1f;

    private int SelectedOrderIndex = -1;

    public int selectedOrderIndex => SelectedOrderIndex;

    private int entregasDoDia;
    private int diaAtual = 1;
    private int totalAvaliacoesPositivas;
    private int avaliacoesPositivasDia;
    private int avaliacoesNegativasDoDia = 0;

    private void Start()
    {
        // Evita que uma pausa da cena anterior
        // deixe o Gameplay congelado.
        Time.timeScale = 1f;

        if (painelMenuOpcoes != null)
            painelMenuOpcoes.SetActive(false);

        ShowOrderPad();

        if (buttonsOrder != null)
        {
            for (int i = 0; i < buttonsOrder.Length; i++)
            {
                int index = i;

                if (buttonsOrder[i] != null)
                {
                    Button botao = buttonsOrder[i];

                    // Som dos papeis somente no hover.
                    if (botao.GetComponent<SomHoverPapel>() == null)
                    {
                        botao.gameObject.AddComponent<SomHoverPapel>();
                    }

                    botao.onClick.AddListener(
                        () => SelectOrder(index));
                }
            }
        }

        // Recupera um progresso salvo, se existir.
        RestaurarProgresso();
    }

    private void SelectOrder(int index)
    {
        SelectedOrderIndex = index;

        if (panelOrder != null)
            panelOrder.SetActive(false);

        if (panelMounting != null)
            panelMounting.SetActive(true);

        // O som de papel continua no hover.
        Debug.Log("Pedido selecionado: " + (index + 1));
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

    // Chamado ao continuar apos a avaliacao.
    public void RegistrarAvaliacao()
    {
        if (evaluationManager == null)
        {
            Debug.LogError(
                "CustomerEvaluationManager nao foi conectado!"
            );
            return;
        }

        int nota = evaluationManager.deliveryNote;

        entregasDoDia++;

        // Apenas 5 estrelas contam para a meta secreta.
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

        Debug.Log(
            "Entregas com 5 estrelas acumuladas: " +
            totalAvaliacoesPositivas + "/" +
            metaAvaliacoesPositivas
        );

        // Guarda o progresso apos a entrega.
        SalvarProgresso();

        if (totalAvaliacoesPositivas >=
            metaAvaliacoesPositivas)
        {
            MostrarVitoria();
            return;
        }

        if (entregasDoDia >= entregasPorDia)
        {
            MostrarRelatorio();
            return;
        }

        ShowOrderPad();
        ReiniciarEtapas();
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
            "Relatorio do dia " + diaAtual + " aberto."
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

        // Reinicia as animacoes dos papeis.
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

        Debug.Log("Todas as etapas foram reiniciadas.");
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

        Debug.Log("Comecando o dia " + diaAtual);
    }

    private void MostrarVitoria()
    {
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
            "VITORIA! Meta de avaliacoes positivas alcancada."
        );
    }

    public void RegistrarPedidoPerdido()
    {
        entregasDoDia++;
        avaliacoesNegativasDoDia++;

        Debug.Log(
            $"Pedido perdido! " +
            $"Pedidos do dia: {entregasDoDia}/{entregasPorDia}"
        );

        SalvarProgresso();

        if (entregasDoDia >= entregasPorDia)
        {
            MostrarRelatorio();
            return;
        }

        ReiniciarEtapas();
        ShowOrderPad();

        if (geradorPedidoAleatorio != null)
            geradorPedidoAleatorio.GerarNovoPedido();
    }

    // =========================================
    // MENU DE OPCOES DO GAMEPLAY
    // =========================================

    public void AbrirMenuOpcoes()
    {
        if (painelMenuOpcoes == null)
        {
            Debug.LogError(
                "GameplayManager: painel de opcoes nao atribuido!",
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

        if (menuOpcoesAberto && pausarJogoAoAbrirMenu)
            Time.timeScale = escalaTempoAntesMenu;

        menuOpcoesAberto = false;
    }

    public void SalvarEVoltarAoMenu()
    {
        // Salva antes de trocar de cena.
        SalvarProgresso();

        // Remove a pausa para nao congelar o menu.
        FecharMenuOpcoes();
        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(cenaMenuPrincipal))
        {
            Debug.LogError(
                "Nome da cena MenuPrincipal nao configurado!",
                this
            );
            return;
        }

        SceneManager.LoadScene(cenaMenuPrincipal);
    }

    // =========================================
    // SALVAMENTO
    // =========================================

    private void SalvarProgresso()
    {
        DadosSalvamento dados = new DadosSalvamento
        {
            versao = 1,
            diaAtual = diaAtual,
            entregasDoDia = entregasDoDia,
            totalAvaliacoesPositivas =
                totalAvaliacoesPositivas,
            avaliacoesPositivasDia =
                avaliacoesPositivasDia,
            avaliacoesNegativasDoDia =
                avaliacoesNegativasDoDia
        };

        string json = JsonUtility.ToJson(dados);

        PlayerPrefs.SetString(ChaveSalvamento, json);
        PlayerPrefs.Save();

        Debug.Log(
            "LogiVery: progresso salvo no dia " +
            diaAtual + "."
        );
    }

    private void RestaurarProgresso()
    {
        if (!TentarLerProgresso(out DadosSalvamento dados))
        {
            Debug.Log(
                "LogiVery: iniciando sem progresso salvo."
            );
            return;
        }

        diaAtual = Mathf.Max(1, dados.diaAtual);

        entregasDoDia = Mathf.Clamp(
            dados.entregasDoDia,
            0,
            entregasPorDia
        );

        totalAvaliacoesPositivas = Mathf.Max(
            0,
            dados.totalAvaliacoesPositivas
        );

        avaliacoesPositivasDia = Mathf.Clamp(
            dados.avaliacoesPositivasDia,
            0,
            entregasDoDia
        );

        avaliacoesNegativasDoDia = Mathf.Clamp(
            dados.avaliacoesNegativasDoDia,
            0,
            entregasDoDia
        );

        // Um pedido interrompido recomeça na
        // selecao, sem ser contado como perdido.
        ReiniciarEtapas();

        if (totalAvaliacoesPositivas >=
            metaAvaliacoesPositivas)
        {
            MostrarVitoria();
        }
        else if (entregasDoDia >= entregasPorDia)
        {
            MostrarRelatorio();
        }
        else
        {
            ShowOrderPad();
        }

        Debug.Log(
            "LogiVery: progresso restaurado. Dia " +
            diaAtual + ", pedidos concluidos no dia: " +
            entregasDoDia + "."
        );
    }

    private static bool TentarLerProgresso(
        out DadosSalvamento dados)
    {
        dados = null;

        if (!PlayerPrefs.HasKey(ChaveSalvamento))
            return false;

        string json = PlayerPrefs.GetString(
            ChaveSalvamento,
            ""
        );

        if (string.IsNullOrEmpty(json))
            return false;

        try
        {
            dados = JsonUtility.FromJson<DadosSalvamento>(
                json
            );

            if (dados == null ||
                dados.versao != 1 ||
                dados.diaAtual < 1 ||
                dados.entregasDoDia < 0 ||
                dados.totalAvaliacoesPositivas < 0 ||
                dados.avaliacoesPositivasDia < 0 ||
                dados.avaliacoesNegativasDoDia < 0)
            {
                dados = null;
                return false;
            }

            return true;
        }
        catch (Exception erro)
        {
            Debug.LogWarning(
                "LogiVery: erro ao ler salvamento: " +
                erro.Message
            );

            dados = null;
            return false;
        }
    }

    public static bool ExisteProgressoSalvo()
    {
        return TentarLerProgresso(
            out DadosSalvamento dados
        );
    }

    public static void ApagarProgressoSalvo()
    {
        PlayerPrefs.DeleteKey(ChaveSalvamento);
        PlayerPrefs.Save();

        Debug.Log(
            "LogiVery: progresso anterior apagado."
        );
    }

    private void OnDestroy()
    {
        if (menuOpcoesAberto && pausarJogoAoAbrirMenu)
        {
            Time.timeScale = escalaTempoAntesMenu;
        }
    }
}
