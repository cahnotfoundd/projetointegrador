
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropPackaging : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [Header("References")]
    [SerializeField]
    private PackagingManager packagingManager;

    [SerializeField]
    private bool isSticker;

    private RectTransform rectTransform;
    private RectTransform originalParent;
    private Vector2 originalPosition;
    private Canvas canvas;

    private bool applied;
    private bool referenciasInicializadas;

    private void Awake()
    {
        PrepararReferencias();
    }

    // Garante que os componentes sejam encontrados
    // mesmo quando o objeto ainda nao executou Awake.
    private bool PrepararReferencias()
    {
        if (referenciasInicializadas)
            return rectTransform != null;

        rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            Debug.LogError(
                "DragDropPackaging: falta um RectTransform!",
                this
            );

            return false;
        }

        originalParent =
            rectTransform.parent as RectTransform;

        originalPosition =
            rectTransform.anchoredPosition;

        // Procura o Canvas mesmo se o objeto
        // ou algum dos seus pais estiver desativado.
        canvas = GetComponentInParent<Canvas>(true);

        if (canvas == null)
        {
            Debug.LogError(
                "DragDropPackaging: Canvas nao encontrado.",
                this
            );
        }

        referenciasInicializadas = true;

        return true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!PrepararReferencias() || applied)
            return;

        rectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!PrepararReferencias() ||
            applied ||
            canvas == null)
        {
            return;
        }

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
            rectTransform.anchoredPosition =
                localPoint;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!PrepararReferencias() ||
            applied ||
            packagingManager == null)
        {
            return;
        }

        bool inside = packagingManager.ItsOnBox(
            eventData.position,
            eventData.pressEventCamera
        );

        if (!inside)
        {
            VoltarPosicaoOriginal();
            return;
        }

        if (isSticker)
        {
            if (packagingManager.LabelPlaced)
            {
                VoltarPosicaoOriginal();
                return;
            }

            if (packagingManager.PackagePlaced)
            {
                applied = true;
                packagingManager.TryPutLabel();
            }
            else
            {
                // Nao permite aplicar a etiqueta
                // antes de colocar a embalagem.
                VoltarPosicaoOriginal();
            }
        }
        else
        {
            if (packagingManager.PackagePlaced)
            {
                VoltarPosicaoOriginal();
                return;
            }

            applied = true;
            packagingManager.TryPlacePackage();
        }
    }

    private void VoltarPosicaoOriginal()
    {
        if (!PrepararReferencias())
            return;

        if (originalParent != null)
        {
            rectTransform.SetParent(
                originalParent,
                false
            );
        }

        rectTransform.anchoredPosition =
            originalPosition;
    }

    // Chamado pelo PackagingManager durante
    // reinicios e restauracao do salvamento.
    public void ResetPackagingItem()
    {
        // CORRECAO:
        // As referencias precisam existir antes
        // de tentar acessar rectTransform.
        if (!PrepararReferencias())
        {
            Debug.LogError(
                "Nao foi possivel reiniciar o item " +
                "da embalagem.",
                this
            );

            return;
        }

        applied = false;

        VoltarPosicaoOriginal();

        Debug.Log(
            "Item de embalagem reiniciado: " +
            gameObject.name
        );
    }
}
