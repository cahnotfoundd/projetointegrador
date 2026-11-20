
using System;
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

    // Avisa o sistema de substituição.
    public event Action AoEsgotarTempo;

    public void Iniciar(
        GeradorPedidoAleatorio.Entregador entregador)
    {
        if (entregador == null)
        {
            Debug.LogError("Entregador não encontrado!");
            return;
        }

        AtualizarFoto(fotoMontagem, entregador.fotoMontagem);
        AtualizarFoto(fotoEmbalagem, entregador.fotoMontagem);

        tempoRestante = Mathf.Max(
            1, entregador.tempoPreparacaoSegundos);

        contando = true;
        AtualizarTexto();
    }

    private void Update()
    {
        if (!contando)
            return;

        tempoRestante = Mathf.Max(
            0f, tempoRestante - Time.deltaTime);

        AtualizarTexto();

        if (tempoRestante <= 0f)
        {
            contando = false;
            AoEsgotarTempo?.Invoke();
        }
    }

    public void Parar()
    {
        contando = false;
    }

    private void AtualizarFoto(Image imagem, Sprite foto)
    {
        if (imagem == null)
            return;

        imagem.sprite = foto;
        imagem.enabled = foto != null;
    }

    private void AtualizarTexto()
    {
        int total = Mathf.CeilToInt(tempoRestante);

        string formatado =
            $"{total / 60:00}:{total % 60:00}";

        if (tempoMontagem != null)
            tempoMontagem.text = formatado;

        if (tempoEmbalagem != null)
            tempoEmbalagem.text = formatado;
    }
}
