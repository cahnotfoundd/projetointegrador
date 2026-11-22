using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomerEvaluationManager : MonoBehaviour
{
    [Header("Estrelas da Avaliacao")]
    [SerializeField] private Image[] orderStars;
    [SerializeField] private Image[] deliveryStars;

    [Header("Textos da Avaliacao")]
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Cores")]
    [SerializeField] private Color positiveColor =
        new Color(1f, 0.72f, 0.15f);

    [SerializeField] private Color negativeColor =
        new Color(0.55f, 0.55f, 0.55f);

    public int orderNote { get; private set; }
    public int deliveryNote { get; private set; }

    public bool positiveNote => deliveryNote >= 3;

    // Mantem compatibilidade com chamadas antigas.
    public void ShowRandomNote()
    {
        ShowRandomNote(0);
    }

    public void ShowRandomNote(int modificadorEntrega)
    {
        orderNote = 5;
        int notaBase = 3;

        int modificadorAplicado = Mathf.Clamp(modificadorEntrega, -2, 2);
        deliveryNote = Mathf.Clamp(notaBase + modificadorAplicado, 0, 5);

        UpdateStars(orderStars, orderNote);
        UpdateStars(deliveryStars, deliveryNote);
        UpdateFeedback();

        Debug.Log("Nota-base: " + notaBase);
        Debug.Log("Modificador dos minigames: " + modificadorAplicado);
        Debug.Log("Nota do Pedido: " + orderNote);
        Debug.Log("Nota da Entrega: " + deliveryNote);
        Debug.Log("Avaliacao Positiva: " + positiveNote);
    }

    private void UpdateStars(Image[] stars, int note)
    {
        if (stars == null)
            return;

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null)
                continue;

            stars[i].color = i < note ? positiveColor : negativeColor;
        }
    }

    private void UpdateFeedback()
    {
        if (feedbackText == null)
            return;

        if (deliveryNote == 5)
        {
            feedbackText.text =
                "Excelente! O cliente esta muito satisfeito " +
                "com a entrega!";
        }
        else if (deliveryNote >= 3)
        {
            feedbackText.text =
                "Bom trabalho! O cliente esta satisfeito " +
                "com a entrega.";
        }
        else if (deliveryNote >= 1)
        {
            feedbackText.text =
                "O cliente nao esta totalmente satisfeito. " +
                "Tente melhorar na proxima vez.";
        }
        else
        {
            feedbackText.text =
                "O cliente esta insatisfeito com a entrega. " +
                "E importante analisar o que deu errado.";
        }

        feedbackText.text = feedbackText.text.ToUpperInvariant();
    }
}
