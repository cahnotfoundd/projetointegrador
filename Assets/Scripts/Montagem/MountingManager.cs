using UnityEngine;
    using UnityEngine.UI;

        public class MountingManager : MonoBehaviour
        {
            [Header("References")]
            [SerializeField] private ProdutoArrastavel[] products;
            [SerializeField] private Button proceedButton;
            [SerializeField] private GameObject panelMontagem;
            [SerializeField] private GameObject panelEmbalagemEtiquetagem;

            
        private void Start()
        {
            if (proceedButton != null)
            {
                proceedButton.interactable = false;
            }

            CheckOrderCompletion();
        }


        public void CheckOrderCompletion()
        {
            bool allProductsPlaced = products != null && products.Length > 0;

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
            {
                proceedButton.interactable = allProductsPlaced;
            }
        }

        public void GoToPackaging()
        {
            if (products == null || products.Length == 0)
            {
                return;
            }

            foreach (ProdutoArrastavel product in products)
            {
                if (product == null || !product.Placed)
                {
                    return;
                }
            }

            if (panelMontagem != null)
            {
                panelMontagem.SetActive(false);
            }

            if (panelEmbalagemEtiquetagem != null)
            {
                panelEmbalagemEtiquetagem.SetActive(true);
            }
        }
    }