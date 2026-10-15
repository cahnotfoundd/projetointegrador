using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropPackaging : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    //variaveis
    [Header("References")]
    [SerializeField] private PackagingManager packagingManager;
    [SerializeField] private bool isSticker;

    private RectTransform rectTransform;
    private RectTransform originalParent;
    private Vector2 originalPosition;
    private Canvas canvas;
    private bool applied;


   private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError("DragDropPackaging: falta um RectTransform neste objeto.", this);
            enabled = false;
            return;
        }

        originalParent = rectTransform.parent as RectTransform;
        originalPosition = rectTransform.anchoredPosition;

        canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("DragDropPackaging: não encontrei um Canvas neste objeto ou nos pais.", this);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (applied)
            return;
        
        originalParent = rectTransform.parent as RectTransform;
        originalPosition = rectTransform.anchoredPosition;

        rectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (applied || canvas == null)
            return;
        
        RectTransform movementArea = rectTransform.parent as RectTransform;

        if (movementArea == null)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(movementArea, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (applied || packagingManager == null)
            return;
        
        bool inside = packagingManager.ItsOnBox(eventData.position, eventData.pressEventCamera);

        if (!inside)
        {
            rectTransform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = originalPosition;
            return;
        }

        if (isSticker)
        {
            if (packagingManager.LabelPlaced)
                    return;
            
            if(packagingManager.PackagePlaced)
            {
                applied = true;
                packagingManager.TryPutLabel();
            }
            else
            {
                // o adesivo fica em cima da caixa, mas não se aplica
                rectTransform.SetParent(originalParent, false);
                rectTransform.anchoredPosition = originalPosition;
            }
            
        }

        else
        {
            if (packagingManager.PackagePlaced)
            return;

            applied = true;
            packagingManager.TryPlacePackage();
        }
    }
}
