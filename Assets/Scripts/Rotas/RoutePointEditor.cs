using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoutePointEditor : MonoBehaviour
{
    [Header("Configuracao")]
    [SerializeField, Range(1, 3)]
    private int routeNumber = 1;

    [SerializeField]
    private RectTransform mapArea;

    [Header("Debug")]
    [SerializeField]
    private bool logClicksOutsideMap = false;

    private readonly List<Vector2> points = new List<Vector2>();

    private bool warnedNoMouse;
    private bool ignoreUntilRelease;

    private void Start()
    {
        if (mapArea == null)
            Debug.LogError("RoutePointEditor: 'mapArea' nao foi atribuido no Inspector.", this);
        else
            Debug.Log($"RoutePointEditor pronto. Tamanho do mapa: {mapArea.rect.size}", this);
    }

    private void OnEnable()
    {
        // Ao entrar na etapa do mapa, ignora o clique que acabou de trocar de tela.
        ignoreUntilRelease = true;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        // O clique que deu foco na janela Game nao deve contar.
        if (hasFocus)
            ignoreUntilRelease = true;
    }

    private void Update()
    {
        if (mapArea == null || !mapArea.gameObject.activeInHierarchy)
            return;

        if (!Application.isFocused)
            return;

        if (Mouse.current == null)
        {
            if (!warnedNoMouse)
            {
                Debug.LogError(
                    "Mouse.current e nulo. Verifique Project Settings > Player > " +
                    "Active Input Handling (use 'Input System Package' ou 'Both').", this);
                warnedNoMouse = true;
            }
            return;
        }

        // Espera o botao ser solto antes de voltar a aceitar cliques.
        if (ignoreUntilRelease)
        {
            if (!Mouse.current.leftButton.isPressed)
                ignoreUntilRelease = false;
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 screenPos = Mouse.current.position.ReadValue();

        // Ignora cliques fora da area da janela Game.
        if (screenPos.x < 0 || screenPos.y < 0 ||
            screenPos.x > Screen.width || screenPos.y > Screen.height)
            return;

        RegisterPoint(screenPos);
    }

    private void RegisterPoint(Vector2 screenPosition)
    {
        Canvas canvas = mapArea.GetComponentInParent<Canvas>();
        if (canvas != null)
            canvas = canvas.rootCanvas;

        Camera uiCamera = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapArea, screenPosition, uiCamera, out Vector2 localPosition))
        {
            Debug.LogWarning("Nao foi possivel converter a posicao do clique para o mapa.");
            return;
        }

        Rect rect = mapArea.rect;

        if (!rect.Contains(localPosition))
        {
            if (logClicksOutsideMap)
                Debug.Log($"Clique fora do mapa. Local: {localPosition} | Rect: {rect}");
            return;
        }

        float x = (localPosition.x - rect.xMin) / rect.width;
        float y = (localPosition.y - rect.yMin) / rect.height;
        Vector2 normalized = new Vector2(x, y);

        // Coordenada em pixels a partir do canto inferior esquerdo do mapa.
        Vector2 pixels = new Vector2(localPosition.x - rect.xMin, localPosition.y - rect.yMin);

        points.Add(normalized);

        Debug.Log(
            $"Ponto {points.Count} da rota {routeNumber}: " +
            $"normalizado = ({normalized.x:F4}, {normalized.y:F4}) | " +
            $"pixels = ({pixels.x:F1}, {pixels.y:F1})");
    }

    public void UndoLastPoint()
    {
        if (points.Count == 0)
            return;

        points.RemoveAt(points.Count - 1);
    }

    public void ClearPoints()
    {
        points.Clear();
    }
}