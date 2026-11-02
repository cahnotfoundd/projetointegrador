using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private CustomerEvaluationManager evaluationManager;

    [Header("Daily Report")]
    [SerializeField] private TextMeshProUGUI textoDia;
    [SerializeField] private TextMeshProUGUI textoAvaliacoesPositivas;
    [SerializeField] private TextMeshProUGUI textoOtimizacaoRotas;
    [SerializeField] private TextMeshProUGUI textoAvisos;

    [Header("Game Rules")]
    [SerializeField] private int entregasPorDia = 3;
    [SerializeField] private int metaAvaliacoesPositivas = 10;

    [Header("Reset dos Sistemas")]
    [SerializeField] private PackagingManager packagingManager;
    [SerializeField] private RouteDrawingManager routeDrawingManager;
    [SerializeField] private MountingManager mountingManager;
    [SerializeField] private DeliveryScreenManager deliveryScreenManager;

    private int SelectedOrderIndex = -1;

    public int selectedOrderIndex => SelectedOrderIndex;

    private int entregasDoDia;
    private int diaAtual = 1;
    private int totalAvaliacoesPositivas;
    private int avaliacoesPositivasDia;

    private void Start()
    {
        ShowOrderPad();

        if (buttonsOrder != null)
        {
            for (int i = 0; i < buttonsOrder.Length; i++)
            {
                int index = i;

                if (buttonsOrder[i] != null)
                {
                    buttonsOrder[i].onClick.AddListener(
                        () => SelectOrder(index));
                }
            }
        }
    }

    private void SelectOrder(int index)
    {
        SelectedOrderIndex = index;

        if (panelOrder != null)
            panelOrder.SetActive(false);

        if (panelMounting != null)
            panelMounting.SetActive(true);

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

    // Chamado quando o jogador clica em Continuar na avaliação
    public void RegistrarAvaliacao()
    {
        if (evaluationManager == null)
        {
            Debug.LogError("CustomerEvaluationManager não foi conectado!");
            return;
        }

        int nota = evaluationManager.deliveryNote;

        entregasDoDia++;

        if (evaluationManager.positiveNote)
        {
            totalAvaliacoesPositivas++;
            avaliacoesPositivasDia++;
        }

        Debug.Log("Entregas do dia: " + entregasDoDia);
        Debug.Log("Avaliações positivas acumuladas: " + totalAvaliacoesPositivas);

        // A vitória tem prioridade sobre o relatório
        if (totalAvaliacoesPositivas >= metaAvaliacoesPositivas)
        {
            MostrarVitoria();
            return;
        }

        // Verifica se as três entregas do dia foram concluídas
        if (entregasDoDia >= entregasPorDia)
        {
            MostrarRelatorio();
            return;
        }

        // Se ainda faltam entregas, reinicia as etapas e volta ao início
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

        Debug.Log("Relatório do dia " + diaAtual + " aberto.");
    }

    private void AtualizarRelatorio()
    {
        if (textoDia != null)
            textoDia.text = "Dia " + diaAtual;

        if (textoAvaliacoesPositivas != null)
            textoAvaliacoesPositivas.text =
                "Entregas bem avaliadas do dia: " + avaliacoesPositivasDia;

        if (textoOtimizacaoRotas != null)
            textoOtimizacaoRotas.text =
                "Otimização de Rotas:\n- Resumo das entregas concluídas.";

        if (textoAvisos != null)
            textoAvisos.text = "Avisos: Nenhum.";
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

        // Reinicia a animação dos papéis dos pedidos
        if (buttonsOrder != null)
        {
            foreach (Button button in buttonsOrder)
            {
                if (button == null) continue;

                AnimationButton animationButton =
                    button.GetComponent<AnimationButton>();

                if (animationButton != null)
                    animationButton.ResetAnimation();
            }
        }

        Debug.Log("Todas as etapas foram reiniciadas.");
    }

    // Chamado pelo botão Começar novo dia
    public void ComecarProximoDia()
    {
        diaAtual++;
        entregasDoDia = 0;
        avaliacoesPositivasDia = 0;

        ShowOrderPad();
        ReiniciarEtapas();

        Debug.Log("Começando o dia " + diaAtual);
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

        Debug.Log("VITÓRIA! Meta de avaliações positivas alcançada.");
    }
}