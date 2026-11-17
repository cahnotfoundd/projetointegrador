
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MountingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProdutoArrastavel[] products;
    [SerializeField] private Button proceedButton;
    [SerializeField] private GameObject panelMontagem;
    [SerializeField] private GameObject panelEmbalagemEtiquetagem;

    private void Start()
    {
        CheckOrderCompletion();
    }

    // Recebe os produtos do pedido selecionado.
    public void ConfigurarProdutos(List<GameObject> objetos)
    {
        List<ProdutoArrastavel> lista =
            new List<ProdutoArrastavel>();

        foreach (GameObject obj in objetos)
        {
            if (obj == null)
                continue;

            ProdutoArrastavel produto =
                obj.GetComponent<ProdutoArrastavel>();

            if (produto != null)
                lista.Add(produto);
        }

        products = lista.ToArray();

        CheckOrderCompletion();
    }

    public void CheckOrderCompletion()
    {
        bool allProductsPlaced =
            products != null && products.Length > 0;

        if (allProductsPlaced)
        {
            foreach (ProdutoArrastavel product in products)
            {
                if (product == null || !product.Placed)
                {
                    allProductsPlaced = false;
                    break;
                }
            }
        }

        if (proceedButton != null)
            proceedButton.interactable = allProductsPlaced;
    }

    public void GoToPackaging()
    {
        if (products == null || products.Length == 0)
            return;

        foreach (ProdutoArrastavel product in products)
        {
            if (product == null || !product.Placed)
                return;
        }

        if (panelMontagem != null)
            panelMontagem.SetActive(false);

        if (panelEmbalagemEtiquetagem != null)
            panelEmbalagemEtiquetagem.SetActive(true);
    }

    public void ResetMounting()
    {
        if (products != null)
        {
            foreach (ProdutoArrastavel product in products)
            {
                if (product != null)
                    product.ResetProduct();
            }
        }

        CheckOrderCompletion();

        Debug.Log("Montagem reiniciada.");
    }
}
