
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LogiVeryAspectoFixo16x9 : MonoBehaviour
{
    private const float ProporcaoDesejada = 16f / 9f;

    private Camera cameraAlvo;

    private int larguraAnterior = -1;
    private int alturaAnterior = -1;

    private void Awake()
    {
        cameraAlvo = GetComponent<Camera>();
        AjustarProporcao();
    }

    private void OnEnable()
    {
        if (cameraAlvo == null)
            cameraAlvo = GetComponent<Camera>();

        AjustarProporcao();
    }

    private void LateUpdate()
    {
        // Atualiza somente quando a resolucao mudar.
        if (Screen.width != larguraAnterior ||
            Screen.height != alturaAnterior)
        {
            AjustarProporcao();
        }
    }

    private void AjustarProporcao()
    {
        if (cameraAlvo == null)
            return;

        int largura = Screen.width;
        int altura = Screen.height;

        if (largura <= 0 || altura <= 0)
            return;

        larguraAnterior = largura;
        alturaAnterior = altura;

        float proporcaoAtual =
            (float)largura / altura;

        Rect areaCamera;

        if (Mathf.Abs(
            proporcaoAtual - ProporcaoDesejada
        ) < 0.001f)
        {
            // Tela 16:9: utiliza toda a area.
            areaCamera = new Rect(
                0f, 0f, 1f, 1f
            );
        }
        else if (proporcaoAtual > ProporcaoDesejada)
        {
            // Tela mais larga: centraliza horizontalmente.
            float larguraNormalizada =
                ProporcaoDesejada / proporcaoAtual;

            float margem =
                (1f - larguraNormalizada) / 2f;

            areaCamera = new Rect(
                margem,
                0f,
                larguraNormalizada,
                1f
            );
        }
        else
        {
            // Tela mais alta: centraliza verticalmente.
            float alturaNormalizada =
                proporcaoAtual / ProporcaoDesejada;

            float margem =
                (1f - alturaNormalizada) / 2f;

            areaCamera = new Rect(
                0f,
                margem,
                1f,
                alturaNormalizada
            );
        }

        cameraAlvo.rect = areaCamera;
    }

    private void OnDisable()
    {
        // Restaura a camera caso o componente
        // seja desativado.
        if (cameraAlvo != null)
        {
            cameraAlvo.rect = new Rect(
                0f, 0f, 1f, 1f
            );
        }

        larguraAnterior = -1;
        alturaAnterior = -1;
    }
}
