
using UnityEngine;
using UnityEngine.EventSystems;

public class ProdutoArrastavel : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    public enum TipoSomProduto
    {
        Automatico,
        Adesivo,
        CadernoAgenda,
        KitMarcadores,
        PostIt,
        Estojo
    }

    [Header("References")]
    [SerializeField] private RectTransform boxArea;
    [SerializeField] private MountingManager mountingManager;

    [Header("Som do produto")]
    [SerializeField]
    private TipoSomProduto tipoSom =
        TipoSomProduto.Automatico;

    [Header("State")]
    [SerializeField] private bool placed;

    private int quantidadeRepresentada = 1;

    public bool Placed => placed;
    public int QuantidadeRepresentada =>
        quantidadeRepresentada;
    public TipoSomProduto TipoSom => tipoSom;

    private RectTransform rectTransform;
    private RectTransform originalParent;
    private Vector2 originalPosition;
    private Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        originalParent =
            rectTransform.parent as RectTransform;

        originalPosition =
            rectTransform.anchoredPosition;

        canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError(
                "O Produto nao esta no Canvas.", this
            );
        }
    }

    public void ConfigurarParaPedido(
        RectTransform novaArea,
        MountingManager novoManager,
        int quantidadeRepresentada = 1)
    {
        rectTransform = GetComponent<RectTransform>();

        boxArea = novaArea;
        mountingManager = novoManager;

        originalParent =
            rectTransform.parent as RectTransform;

        originalPosition =
            rectTransform.anchoredPosition;

        canvas = GetComponentInParent<Canvas>();

        placed = false;

        this.quantidadeRepresentada =
            Mathf.Max(1, quantidadeRepresentada);

        CanvasGroup canvasGroup =
            GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }

    private void TocarSomInteracao()
    {
        if (mountingManager != null)
            mountingManager.TocarSomProduto(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (placed)
            return;

        originalParent =
            rectTransform.parent as RectTransform;

        originalPosition =
            rectTransform.anchoredPosition;

        TocarSomInteracao();
        rectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (placed || canvas == null)
            return;

        RectTransform movementArea =
            rectTransform.parent as RectTransform;

        if (movementArea == null)
            return;

        if (RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                movementArea,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (placed)
            return;

        TocarSomInteracao();

        if (boxArea == null)
            return;

        bool placedInBox =
            RectTransformUtility
            .RectangleContainsScreenPoint(
                boxArea,
                eventData.position,
                eventData.pressEventCamera
            );

        if (placedInBox)
        {
            PutInBox(eventData);
        }
        else
        {
            rectTransform.SetParent(
                originalParent, false
            );

            rectTransform.anchoredPosition =
                originalPosition;
        }
    }

    private void PutInBox(PointerEventData eventData)
    {
        if (!RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                boxArea,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPosition))
        {
            rectTransform.SetParent(
                originalParent, false
            );

            rectTransform.anchoredPosition =
                originalPosition;

            return;
        }

        rectTransform.SetParent(boxArea, false);

        float halfWidth = boxArea.rect.width / 2f;
        float halfHeight = boxArea.rect.height / 2f;

        float productHalfWidth =
            rectTransform.rect.width / 2f;

        float productHalfHeight =
            rectTransform.rect.height / 2f;

        float limitX = Mathf.Max(
            0f, halfWidth - productHalfWidth
        );

        float limitY = Mathf.Max(
            0f, halfHeight - productHalfHeight
        );

        localPosition.x = Mathf.Clamp(
            localPosition.x, -limitX, limitX
        );

        localPosition.y = Mathf.Clamp(
            localPosition.y, -limitY, limitY
        );

        rectTransform.anchoredPosition =
            localPosition;

        placed = true;

        AtualizarInteracao();

        if (mountingManager != null)
        {
            mountingManager.CheckOrderCompletion();
        }
        else
        {
            Debug.LogError(
                "MountingManager nao esta atribuido!",
                this
            );
        }

        Debug.Log(
            "Produto colocado: " + gameObject.name +
            " | Quantidade representada: " +
            quantidadeRepresentada
        );
    }

    private void AtualizarInteracao()
    {
        CanvasGroup canvasGroup =
            GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = !placed;
            canvasGroup.interactable = !placed;
        }
    }

    public LogiVerySaveManager.ProdutoMontagem
        CapturarParaSalvamento()
    {
        var dados =
            new LogiVerySaveManager.ProdutoMontagem();

        dados.colocadoNaCaixa = placed;
        dados.posicao =
            rectTransform.anchoredPosition;

        GrupoPedidoVisual grupo =
            GetComponent<GrupoPedidoVisual>();

        if (grupo != null)
        {
            dados.desenhosAtivos =
                grupo.CapturarDesenhosAtivos();
        }

        return dados;
    }

    public void RestaurarDoSalvamento(
        LogiVerySaveManager.ProdutoMontagem dados)
    {
        if (dados == null)
            return;

        ResetProduct();

        GrupoPedidoVisual grupo =
            GetComponent<GrupoPedidoVisual>();

        if (grupo != null &&
            dados.desenhosAtivos != null)
        {
            grupo.RestaurarDesenhosAtivos(
                dados.desenhosAtivos
            );
        }

        if (dados.colocadoNaCaixa && boxArea != null)
        {
            rectTransform.SetParent(boxArea, false);
            rectTransform.anchoredPosition =
                dados.posicao;

            placed = true;
        }
        else
        {
            rectTransform.anchoredPosition =
                dados.posicao;

            placed = false;
        }

        AtualizarInteracao();
    }

    public void ResetProduct()
    {
        placed = false;

        if (originalParent != null)
        {
            rectTransform.SetParent(
                originalParent, false
            );
        }

        rectTransform.anchoredPosition =
            originalPosition;

        AtualizarInteracao();

        Debug.Log(
            "Produto reiniciado: " + gameObject.name
        );
    }
}
