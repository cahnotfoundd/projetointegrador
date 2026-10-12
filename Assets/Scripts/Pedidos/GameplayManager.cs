using UnityEngine;
using UnityEngine.UI;

public class GameplayManager : MonoBehaviour
{

    [Header("Order Pad")] // comentário para organizar o inspector
    [SerializeField] private Button[] buttonsOrder; // referência ao objeto do papel de pedidos

    [Header("Panels")] // comentário para organizar o inspector
    [SerializeField] private GameObject panelOrder; // referência ao painel de pedidos
    [SerializeField] private GameObject panelMounting; // referência ao painel de montagem do pedido
    [SerializeField] private GameObject panelPackaging; // referência ao painel de embalagem do pedido
    [SerializeField] private GameObject panelRoutes; // referência ao painel de rotas do pedido
    [SerializeField] private GameObject panelDelivery; // referência ao painel de entrega do pedido
    [SerializeField] private GameObject panelResult; // referência ao painel de resultado do pedido

    private int SelectedOrderIndex; // índice do pedido selecionado
    public int selectedOrderIndex => SelectedOrderIndex; // propriedade para acessar o índice do pedido selecionado
    private void Start()
    {
        ShowOrderPad(); // chama o método para mostrar o papel de pedidos

        for (int i = 0; i < buttonsOrder.Length; i++)
        {
            int index = i; // cria uma variável local para armazenar o índice do botão
            buttonsOrder[i].onClick.AddListener(
                () => SelectOrder(index)); // adiciona um listener para o evento de clique do botão
        }
    }
    private void SelectOrder(int index)
    {
        panelOrder.SetActive(false); // desativa o painel de pedidos
        panelMounting.SetActive(true); // ativa o painel de montagem do pedido

        SelectedOrderIndex = index;
        Debug.Log("Pedido selecionado: " + (index + 1) ); // exibe no console o índice do pedido selecionado
        GoToMountingPanel(); // chama o método para ir para o painel de montagem do pedido
    }

    private void GoToMountingPanel()
    {
        if (SelectedOrderIndex < 0)
        {
            Debug.LogWarning("Escolha um pedido antes de prosseguir!"); // exibe um aviso no console caso nenhum pedido tenha sido selecionado
            return;
        }

        if (panelOrder != null)
            panelOrder.SetActive(false); // desativa o painel de pedidos

        if (panelMounting != null)
            panelMounting.SetActive(true); // ativa o painel de montagem do pedido

        Debug.Log ("Indo para o painel de montagem do pedido: " + (SelectedOrderIndex + 1)); // exibe no console que está indo para o painel de montagem do pedido
    }

    private void ShowOrderPad()
    {
        if (panelOrder != null)
            panelOrder.SetActive(true); // ativa o painel de pedidos

        if (panelMounting != null)
            panelMounting.SetActive(false); // desativa o painel de montagem do pedido

        if (panelPackaging != null)
            panelPackaging.SetActive(false); // desativa o painel de embalagem do pedido

        if (panelRoutes != null)
            panelRoutes.SetActive(false); // desativa o painel de rotas do pedido

        if (panelDelivery != null)
            panelDelivery.SetActive(false); // desativa o painel de entrega do pedido

        if (panelResult != null)
            panelResult.SetActive(false); // desativa o painel de resultado do pedido
    }

}
