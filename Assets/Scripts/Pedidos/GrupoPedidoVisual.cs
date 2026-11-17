
using System.Collections.Generic;
using UnityEngine;

public class GrupoPedidoVisual : MonoBehaviour
{
    [Header("Desenhos disponíveis dentro do grupo")]
    [SerializeField] private GameObject[] desenhos;

    public int QuantidadeMaxima =>
        desenhos == null ? 0 : desenhos.Length;

    public int QuantidadeRepresentada { get; private set; }

    public void ConfigurarQuantidade(int quantidade)
    {
        if (desenhos == null || desenhos.Length == 0)
        {
            Debug.LogError("Nenhum desenho foi cadastrado!", this);
            return;
        }

        List<GameObject> disponiveis = new List<GameObject>();

        foreach (GameObject desenho in desenhos)
        {
            if (desenho != null)
            {
                desenho.SetActive(false);
                disponiveis.Add(desenho);
            }
        }

        if (quantidade < 1 || quantidade > disponiveis.Count)
        {
            Debug.LogError(
                "Quantidade inválida para este grupo.", this);
            return;
        }

        // Embaralha os desenhos sem repetir.
        for (int i = 0; i < quantidade; i++)
        {
            int indice = Random.Range(i, disponiveis.Count);

            GameObject temporario = disponiveis[i];
            disponiveis[i] = disponiveis[indice];
            disponiveis[indice] = temporario;

            disponiveis[i].SetActive(true);
        }

        QuantidadeRepresentada = quantidade;
    }
}
