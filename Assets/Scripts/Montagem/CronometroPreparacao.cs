
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CronometroPreparacao : MonoBehaviour
{
    [Header("Interface da montagem")]
    [SerializeField] private Image fotoMontagem;
    [SerializeField] private TMP_Text tempoMontagem;

    [Header("Interface da embalagem")]
    [SerializeField] private Image fotoEmbalagem;
    [SerializeField] private TMP_Text tempoEmbalagem;

    private float tempoRestante;
    private bool contando;

    // Recebe o entregador da folha escolhida.
    public void Iniciar(
        GeradorPedidoAleatorio.Entregador entregador)
    {
        if (entregador == null)
        {
            Debug.LogError(
                "Nenhum entregador foi selecionado.");
            return;
        }

        AtualizarFoto(fotoMontagem, entregador.fotoMontagem);
        AtualizarFoto(fotoEmbalagem, entregador.fotoMontagem);

        // Recebe o tempo individual do entregador.
        tempoRestante =
            Mathf.Max(1, entregador.tempoPreparacaoSegundos);

        contando = true;
        AtualizarTexto();

        Debug.Log(
            $"Entregador: {entregador.nome} | " +
            $"Tempo inicial: {tempoRestante} segundos.");
    }

    private void Update()
    {
        if (!contando)
            return;

        // Diminui o tempo conforme o jogo passa.
        tempoRestante = Mathf.Max(
            0f, tempoRestante - Time.deltaTime);

        AtualizarTexto();

        // Por enquanto, apenas para em zero.
        if (tempoRestante <= 0f)
            contando = false;
    }

    
    private void AtualizarTexto()
    {
        int total = Mathf.CeilToInt(tempoRestante);
        int minutos = total / 60;
        int segundos = total % 60;

        string tempoFormatado = $"{minutos:00}:{segundos:00}";

        if (tempoMontagem != null)
            tempoMontagem.text = tempoFormatado;

        if (tempoEmbalagem != null)
            tempoEmbalagem.text = tempoFormatado;
    }


    private void AtualizarFoto(Image imagem, Sprite foto)
    {
        if (imagem == null)
            return;

        imagem.sprite = foto;
        imagem.enabled = foto != null;
    }
}
