using UnityEngine;

public class MazeDestination : MonoBehaviour
{
    [SerializeField] private DeliveryScreenManager gerenciadorEntrega;

    private bool concluido;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (concluido)
            return;

        MazePlayerManager player =
            other.GetComponent<MazePlayerManager>();

        if (player != null)
        {
            concluido = true;

            if (gerenciadorEntrega != null)
                gerenciadorEntrega.ConcluirLabirinto();
            else
                Debug.LogError("DeliveryScreenManager não conectado!");
        }
    }

    public void ResetDestination()
    {
        concluido = false;

        Debug.Log("Destino liberado para nova tentativa.");
    }
}