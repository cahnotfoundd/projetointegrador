
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

    // Toca o som correspondente ao produto.
    // Chamado quando o mouse e pressionado ou solto.
    public void TocarSomProduto(ProdutoArrastavel produto)
    {
        if (produto == null)
            return;

        LogiVeryEfeitosSonoros audio =
            LogiVeryEfeitosSonoros.Instancia;

        if (audio == null)
            return;

        ProdutoArrastavel.TipoSomProduto tipo = produto.TipoSom;

        // Identifica automaticamente pelo nome, se necessario.
        if (tipo == ProdutoArrastavel.TipoSomProduto.Automatico)
        {
            tipo = IdentificarTipoPeloNome(produto.gameObject.name);
        }

        switch (tipo)
        {
            case ProdutoArrastavel.TipoSomProduto.Adesivo:
                audio.TocarAdesivo();
                break;

            case ProdutoArrastavel.TipoSomProduto.CadernoAgenda:
                audio.TocarCadernoAgenda();
                break;

            case ProdutoArrastavel.TipoSomProduto.KitMarcadores:
                audio.TocarKitMarcadores();
                break;

            case ProdutoArrastavel.TipoSomProduto.PostIt:
                audio.TocarPostIt();
                break;

            case ProdutoArrastavel.TipoSomProduto.Estojo:
                audio.TocarEstojo();
                break;

            default:
                Debug.LogWarning(
                    "Som nao identificado para o produto: " +
                    produto.gameObject.name +
                    ". Configure o campo Tipo Som no prefab.",
                    produto
                );
                break;
        }
    }

    private ProdutoArrastavel.TipoSomProduto
        IdentificarTipoPeloNome(string nomeObjeto)
    {
        string nome = nomeObjeto.ToLowerInvariant()
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("_", "");

        if (nome.Contains("adesivo"))
            return ProdutoArrastavel.TipoSomProduto.Adesivo;

        if (nome.Contains("caderno") || nome.Contains("agenda"))
            return ProdutoArrastavel.TipoSomProduto.CadernoAgenda;

        if (nome.Contains("marcador"))
            return ProdutoArrastavel.TipoSomProduto.KitMarcadores;

        if (nome.Contains("postit"))
            return ProdutoArrastavel.TipoSomProduto.PostIt;

        if (nome.Contains("estojo"))
            return ProdutoArrastavel.TipoSomProduto.Estojo;

        return ProdutoArrastavel.TipoSomProduto.Automatico;
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
