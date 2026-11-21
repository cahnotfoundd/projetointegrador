
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SomHoverPapel : MonoBehaviour, IPointerEnterHandler
{
    private Button botao;

    private void Awake()
    {
        botao = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Nao toca se o botao estiver desativado.
        if (botao != null && !botao.IsActive())
            return;

        if (botao != null && !botao.interactable)
            return;

        // Toca uma vez quando o mouse entra no papel.
        LogiVeryEfeitosSonoros.Instancia?.TocarPapeis();
    }
}
