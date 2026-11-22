
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
    private float intervaloSom;

    public event Action AoEsgotarTempo;

    public void Iniciar(
        GeradorPedidoAleatorio.Entregador entregador)
    {
        if (entregador == null)
        {
            Debug.LogError(
                "Entregador nao encontrado!"
            );
            return;
        }

        AtualizarFoto(
            fotoMontagem,
            entregador.fotoMontagem
        );

        AtualizarFoto(
            fotoEmbalagem,
            entregador.fotoMontagem
        );

        tempoRestante = Mathf.Max(
            1f,
            entregador.tempoPreparacaoSegundos
        );

        contando = true;
        intervaloSom = 0f;

        AtualizarTexto();
    }

    private void Update()
    {
        if (!contando)
            return;

        float delta = Time.deltaTime;

        tempoRestante = Mathf.Max(
            0f,
            tempoRestante - delta
        );

        AtualizarTexto();

        if (tempoRestante > 0f)
        {
            intervaloSom += delta;

            if (intervaloSom >= 1f)
            {
                intervaloSom %= 1f;

                LogiVeryEfeitosSonoros.Instancia?
                    .TocarCronometro();
            }
        }

        if (tempoRestante <= 0f)
        {
            contando = false;
            intervaloSom = 0f;

            AoEsgotarTempo?.Invoke();
        }
    }

    public void Parar()
    {
        contando = false;
        intervaloSom = 0f;
    }

    public void CapturarEstado(
        LogiVerySaveManager.Cronometro dados)
    {
        if (dados == null)
            return;

        dados.tempoRestante = tempoRestante;
        dados.contando = contando;
        dados.intervaloSom = intervaloSom;
    }

    public void RestaurarEstado(
        LogiVerySaveManager.Cronometro dados,
        GeradorPedidoAleatorio.Entregador entregador)
    {
        if (dados == null)
            return;

        // Atualiza primeiro as fotos do entregador.
        // Nao deixa o tempo anterior ser utilizado.
        if (entregador != null)
        {
            AtualizarFoto(
                fotoMontagem,
                entregador.fotoMontagem
            );

            AtualizarFoto(
                fotoEmbalagem,
                entregador.fotoMontagem
            );
        }

        tempoRestante = Mathf.Max(
            0f,
            dados.tempoRestante
        );

        intervaloSom = Mathf.Clamp(
            dados.intervaloSom,
            0f,
            0.999f
        );

        contando =
            dados.contando && tempoRestante > 0f;

        AtualizarTexto();
    }

    private void AtualizarFoto(
        Image imagem,
        Sprite foto)
    {
        if (imagem == null)
            return;

        imagem.sprite = foto;
        imagem.enabled = foto != null;
    }

    private void AtualizarTexto()
    {
        int total = Mathf.CeilToInt(
            tempoRestante
        );

        string formatado =
            $"{total / 60:00}:{total % 60:00}";

        if (tempoMontagem != null)
            tempoMontagem.text = formatado;

        if (tempoEmbalagem != null)
            tempoEmbalagem.text = formatado;
    }
}
