using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomerEvaluationManager : MonoBehaviour
{
    [Header("Estrelas da Avaliação")]
    [SerializeField] private SpriteRenderer[] orderStars;
    [SerializeField] private SpriteRenderer[] deliveryStars;

    [Header("Textos da Avaliação")]
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Cores")]
    [SerializeField] private Color positiveColor = new Color(1f, 0.72f, 0.15f); // cor amarela para avaliação positiva
    [SerializeField] private Color negativeColor = new Color(0.55f, 0.55f, 0.55f); // cor cinza para avaliação negativa

    public int orderNote { get; private set; } // propriedade para armazenar a avaliação do pedido
    public int deliveryNote { get; private set; } // propriedade para armazenar a avaliação

    public bool positiveNote => deliveryNote  >= 3;

    public void ShowRandomNote()
    {
        orderNote = 5;
        deliveryNote = Random.Range(0, 6);

        UpdateStars(orderStars, orderNote);
        UpdateStars(deliveryStars, deliveryNote);

        UpdateFeedback();

        Debug.Log("Nota do Pedido: " + orderNote);
        Debug.Log("Nota da Entrega: " + deliveryNote);
        Debug.Log("Avaliação Positiva: " + positiveNote);
    }

    private void UpdateStars(SpriteRenderer[] stars, int note)
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
            feedbackText.text = "Excelente! O cliente está muito satisfeito com a entrega!";
        }

        else if (deliveryNote >= 3)
        {
            feedbackText.text = "Bom trabalho! O cliente está satisfeito com a entrega.";
        }

        else if (deliveryNote >= 1)
        {
            feedbackText.text = "O cliente não está totalmente satisfeito. Tente melhorar na próxima vez.";
        }

        else
        {
            feedbackText.text = "O cliente está insatisfeito com a entrega. É importante analisar o que deu errado.";
        }
    }
}

