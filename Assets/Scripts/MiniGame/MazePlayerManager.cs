using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class MazePlayerManager : MonoBehaviour
{
    [SerializeField] private float velocidade = 3f;

    private Rigidbody2D rb;
    private Vector2 direcao;
    private Vector2 posicaoInicial;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        posicaoInicial = rb.position;
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            direcao = Vector2.zero;
            return;
        }

        direcao = Vector2.zero;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            direcao.y = 1;

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            direcao.y = -1;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            direcao.x = -1;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            direcao.x = 1;

        direcao = direcao.normalized;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + direcao * velocidade * Time.fixedDeltaTime);
    }

    public void ResetPlayer()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D não encontrado no Player!");
            return;
        }

        direcao = Vector2.zero;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.position = posicaoInicial;
        transform.position = posicaoInicial;

        Physics2D.SyncTransforms();

        Debug.Log("Jogador voltou à posição inicial: " + posicaoInicial);
    }
}