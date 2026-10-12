
using UnityEngine;
using UnityEngine.EventSystems;

public class ProdutoArrastavel : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    [SerializeField] private RectTransform boxArea;
    [SerializeField] private MountingManager mountingManager;

    [Header("State")]
    [SerializeField] private bool placed;

    private RectTransform rectTransform;
    private RectTransform originalParent;
    private Vector2 originalPosition;
    private Canvas canvas;

    public bool Placed => placed;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalParent = rectTransform.parent as RectTransform;
        originalPosition = rectTransform.anchoredPosition;
        canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("O Produto não está no Canvas.");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (placed) return;

        originalParent = rectTransform.parent as RectTransform;
        originalPosition = rectTransform.anchoredPosition;

        rectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (placed || canvas == null) return;

        RectTransform movementArea = rectTransform.parent as RectTransform;

        if (movementArea == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
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
        if (placed || boxArea == null) return;

        bool placedInBox = RectTransformUtility.RectangleContainsScreenPoint(
            boxArea,
            eventData.position,
            eventData.pressEventCamera);

        if (placedInBox)
        {
            PutInBox(eventData);
        }
        else
        {
            rectTransform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    private void PutInBox(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            boxArea,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPosition))
        {
            rectTransform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = originalPosition;
            return;
        }

        rectTransform.SetParent(boxArea, false);

        float halfWidth = boxArea.rect.width / 2f;
        float halfHeight = boxArea.rect.height / 2f;
        float productHalfWidth = rectTransform.rect.width / 2f;
        float productHalfHeight = rectTransform.rect.height / 2f;

        float limitX = Mathf.Max(0f, halfWidth - productHalfWidth);
        float limitY = Mathf.Max(0f, halfHeight - productHalfHeight);

        localPosition.x = Mathf.Clamp(localPosition.x, -limitX, limitX);
        localPosition.y = Mathf.Clamp(localPosition.y, -limitY, limitY);

        rectTransform.anchoredPosition = localPosition;
        placed = true;

        if (mountingManager != null)
        {
            mountingManager.CheckOrderCompletion();
        }
        else
        {
            Debug.LogError("MountingManager não está atribuído!", this);
        }
        
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (mountingManager != null)
        {
            mountingManager.CheckOrderCompletion();
        }

        Debug.Log("Produto colocado na área de destino: " + gameObject.name);
    }
}
