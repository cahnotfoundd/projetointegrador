
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomerEvaluationManager : MonoBehaviour
{
    [Header("Estrelas da Avaliacao")]
    [SerializeField] private SpriteRenderer[] orderStars;
    [SerializeField] private SpriteRenderer[] deliveryStars;

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

    // NOVO: recebe o resultado dos minigames.
    public void ShowRandomNote(int modificadorEntrega)
    {
        // A nota do pedido continua fixa em 5.
        orderNote = 5;

        // Nota-base aleatoria, como no sistema original.
        int notaBase = 3;

        // Limita o impacto dos desafios.
        int modificadorAplicado = Mathf.Clamp(
            modificadorEntrega,
            -2,
            2
        );

        // Calcula a nota final entre 0 e 5.
        deliveryNote = Mathf.Clamp(
            notaBase + modificadorAplicado,
            0,
            5
        );

        // Atualiza as estrelas.
        UpdateStars(orderStars, orderNote);
        UpdateStars(deliveryStars, deliveryNote);

        // Atualiza o texto de feedback.
        UpdateFeedback();

        Debug.Log("Nota-base: " + notaBase);

        Debug.Log(
            "Modificador dos minigames: " +
            modificadorAplicado
        );

        Debug.Log("Nota do Pedido: " + orderNote);
        Debug.Log("Nota da Entrega: " + deliveryNote);
        Debug.Log("Avaliacao Positiva: " + positiveNote);
    }

    private void UpdateStars(SpriteRenderer[] stars, int note)
    {
        if (stars == null)
            return;

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null)
                continue;

            stars[i].color =
                i < note ? positiveColor : negativeColor;
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

        //texto em caixa alta
        feedbackText.text = feedbackText.text.ToUpperInvariant();
    }
}
