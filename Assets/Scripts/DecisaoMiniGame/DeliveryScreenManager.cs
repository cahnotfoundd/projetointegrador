
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DeliveryScreenManager : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject painelEntrega;
    [SerializeField] private GameObject painelDecisao;
    [SerializeField] private GameObject painelLabirinto;

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI textoLabirinto;

    private bool eventoIniciado;
    private bool eventoJaAconteceu;

    // Estado do imprevisto atual
    private bool problemaPendente;
    private bool problemaResolvido;

    private void Update()
    {        
        // Atalho temporário para testes
        if (Keyboard.current != null &&
            Keyboard.current.tKey.wasPressedThisFrame)
        {
            MostrarDecisao();
        }

        // Inicia o sorteio quando a entrega fica ativa
        if (painelEntrega != null &&
            painelEntrega.activeInHierarchy &&
            !eventoIniciado &&
            !eventoJaAconteceu)
        {
            eventoIniciado = true;
            StartCoroutine(AguardarEventoNegativo());
        }

        // Atalho temporário: K conclui o labirinto
        if (Keyboard.current != null &&
            Keyboard.current.kKey.wasPressedThisFrame)
        {
            ConcluirLabirinto();
        }
        
    }

    private IEnumerator AguardarEventoNegativo()
    {
        float espera = Random.Range(5f, 12f);
        yield return new WaitForSeconds(espera);

        if (painelEntrega != null &&
            painelEntrega.activeInHierarchy &&
            !eventoJaAconteceu)
        {
            MostrarDecisao();
        }
        else
        {
            eventoIniciado = false;
        }
    }

    public void MostrarDecisao()
    {
        if (eventoJaAconteceu)
            return;

        eventoJaAconteceu = true;
        problemaPendente = true;
        problemaResolvido = false;

        painelDecisao.SetActive(true);

        Debug.Log("Evento negativo iniciado: problema de trânsito!");
    }

    public void AceitarLabirinto()
    {
        painelDecisao.SetActive(false);
        painelLabirinto.SetActive(true);

        Debug.Log("O jogador aceitou resolver o problema.");
    }

    public void RecusarLabirinto()
    {
        painelDecisao.SetActive(false);
        problemaPendente = true;
        problemaResolvido = false;
        
        Debug.Log("O jogador recusou o desafio. O problema continua pendente.");
    }

    public void ConcluirLabirinto()
    {
        painelLabirinto.SetActive(false);
        problemaPendente = false;
        problemaResolvido = true;

        textoLabirinto.text = "Labirinto Concluído!";
        textoLabirinto.gameObject.SetActive(true);

        StartCoroutine(OcultarMensagem());

        Debug.Log("Problema resolvido! O jogador pode continuar a entrega.");
    }

    private IEnumerator OcultarMensagem()
    {
        yield return new WaitForSeconds(3f);

        textoLabirinto.gameObject.SetActive(false);
    }
}
    