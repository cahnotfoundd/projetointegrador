using UnityEngine;

public class MazeDestination : MonoBehaviour
{
    [SerializeField] private DeliveryScreenManager gerenciadorEntrega;
    
    private bool concluido;

    private void OTriggerEnter2D(Collider2D other)
    {
        if (concluido)
        return;

        if (other.GetComponent<MazePlayerManager>() != null)
        {
            concluido = true;
            gerenciadorEntrega.ConcluirLabirinto();
        }
    }
}
