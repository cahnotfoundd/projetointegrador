using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class RouteDrawingManager : MonoBehaviour
{
    [Serializable]
    public class RouteData
    {
        public string routeName;
        public Vector2[] points;
    }

    [Header("Mapa")]
    [SerializeField] private RectTransform mapArea;
    [SerializeField] private UILineGraphic routeLine;

    [Header("Rotas")]
    [SerializeField] private RouteData[] routes = new RouteData[3];

    [Header("Navegação")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private GameObject routesPanel;
    [SerializeField] private GameObject nextPanel;

    [Header("Tolerâncias")]
    [SerializeField, Range(0.01f, 0.2f)] private float startTolerance = 0.06f;
    [SerializeField, Range(0.005f, 0.1f)] private float drawingTolerance = 0.025f;
    [SerializeField] private float arrivalTolerance = 0.035f;
    [SerializeField, Range(0.001f, 0.02f)] private float routeSelectionMargin = 0.003f;

    private readonly List<Vector2> paintedPoints = new();
    private readonly List<int> candidateRoutes = new();

    private float[] routeProgress;
    private int selectedRoute = -1;
    private bool attemptStarted;
    private bool drawing;
    private bool routeCompleted;

    private Camera UICamera
    {
        get
        {
            if (mapArea == null) return null;

            Canvas canvas = mapArea.GetComponentInParent<Canvas>();
            if (canvas == null) return Camera.main;

            Canvas root = canvas.rootCanvas;
            if (root.renderMode == RenderMode.ScreenSpaceOverlay) return null;

            return root.worldCamera != null ? root.worldCamera : Camera.main;
        }
    }

    private void Start()
    {
        routeProgress = new float[routes == null ? 0 : routes.Length];

        if (routeLine != null)
            routeLine.SetPoints(paintedPoints);

        if (confirmButton != null)
            confirmButton.interactable = false;
    }

    private void Update()
    {
        if (routeCompleted || Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            BeginDrawing();

        if (drawing && Mouse.current.leftButton.isPressed)
            ContinueDrawing();

        // Soltar o mouse pausa o desenho, mas preserva rota e progresso.
        if (Mouse.current.leftButton.wasReleasedThisFrame)
            drawing = false;
    }

    private bool GetMapPosition(Vector2 screenPosition, out Vector2 normalized)
    {
        normalized = Vector2.zero;
        if (mapArea == null) return false;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapArea, screenPosition, UICamera, out Vector2 local))
            return false;

        Rect rect = mapArea.rect;
        if (rect.width <= 0f || rect.height <= 0f) return false;

        normalized = new Vector2(
            (local.x - rect.xMin) / rect.width,
            (local.y - rect.yMin) / rect.height
        );

        return normalized.x >= 0f && normalized.x <= 1f &&
               normalized.y >= 0f && normalized.y <= 1f;
    }

    private void BeginDrawing()
    {
        if (!GetMapPosition(Mouse.current.position.ReadValue(), out Vector2 cursor))
            return;

        if (attemptStarted)
        {
            // Retoma de onde parou; não exige clicar novamente na origem.
            drawing = true;
            return;
        }

        if (!IsNearAnyStart(cursor)) return;

        attemptStarted = true;
        drawing = true;
        selectedRoute = -1;
        candidateRoutes.Clear();
        paintedPoints.Clear();
        Array.Clear(routeProgress, 0, routeProgress.Length);

        for (int i = 0; i < routes.Length; i++)
        {
            if (IsValidRoute(i))
                candidateRoutes.Add(i);
        }

        if (routeLine != null)
            routeLine.SetPoints(paintedPoints);

        if (confirmButton != null)
            confirmButton.interactable = false;
    }

    private bool IsNearAnyStart(Vector2 cursor)
    {
        for (int i = 0; i < routes.Length; i++)
        {
            if (IsValidRoute(i) &&
                Vector2.Distance(cursor, routes[i].points[0]) <= startTolerance)
                return true;
        }

        return false;
    }

    private bool IsValidRoute(int index)
    {
        return routes != null && index >= 0 && index < routes.Length &&
               routes[index] != null && routes[index].points != null &&
               routes[index].points.Length >= 2;
    }

    private void ContinueDrawing()
    {
        if (!GetMapPosition(Mouse.current.position.ReadValue(), out Vector2 cursor))
            return;

        if (candidateRoutes.Count == 0) return;

        int bestRoute = -1;
        float bestDistance = float.MaxValue;
        float secondDistance = float.MaxValue;
        float bestProgress = 0f;

        // Mantém várias rotas candidatas enquanto o cursor percorre um trecho compartilhado.
        foreach (int routeIndex in candidateRoutes)
        {
            if (!TryProjectForward(
                    cursor, routes[routeIndex].points, routeProgress[routeIndex],
                    out float distance, out float projectedProgress))
                continue;

            if (distance <= drawingTolerance * 2f)
                routeProgress[routeIndex] = Mathf.Max(
                    routeProgress[routeIndex], projectedProgress);

            if (distance < bestDistance)
            {
                secondDistance = bestDistance;
                bestDistance = distance;
                bestRoute = routeIndex;
                bestProgress = projectedProgress;
            }
            else if (distance < secondDistance)
            {
                secondDistance = distance;
            }
        }

        if (bestRoute < 0 || bestDistance > drawingTolerance)
            return;

        // Se há empate, ainda não escolhe entre caminhos compartilhados.
        bool ambiguous = secondDistance <= drawingTolerance &&
                         secondDistance - bestDistance < routeSelectionMargin;

        if (!ambiguous)
        {
            selectedRoute = bestRoute;
            candidateRoutes.Clear();
            candidateRoutes.Add(selectedRoute);
            routeProgress[selectedRoute] = Mathf.Max(
                routeProgress[selectedRoute], bestProgress);
        }
        else
        {
            // Descarta somente trajetos que já ficaram claramente mais distantes.
            candidateRoutes.RemoveAll(index =>
                !TryProjectForward(
                    cursor, routes[index].points, routeProgress[index],
                    out float distance, out _) ||
                distance > bestDistance + routeSelectionMargin + drawingTolerance * 0.15f);
        }

        int lineRoute = selectedRoute >= 0 ? selectedRoute : bestRoute;
        UpdatePaintedLine(routes[lineRoute].points, routeProgress[lineRoute]);

        // A conclusão só ocorre quando uma rota específica foi identificada.
        if (selectedRoute >= 0)
        {
            Vector2[] path = routes[selectedRoute].points;
            float endDistance = Vector2.Distance(cursor, path[path.Length - 1]);

            if (routeProgress[selectedRoute] >= 0.999f &&
                endDistance <= arrivalTolerance)
            {
                routeProgress[selectedRoute] = 1f;
                routeCompleted = true;
                drawing = false;
                UpdatePaintedLine(path, 1f);

                if (confirmButton != null)
                    confirmButton.interactable = true;
            }
        }
    }

    private bool TryProjectForward(
        Vector2 cursor, Vector2[] path, float currentProgress,
        out float bestDistance, out float bestProgress)
    {
        bestDistance = float.MaxValue;
        bestProgress = currentProgress;

        float totalLength = GetPathLength(path);
        if (totalLength <= Mathf.Epsilon) return false;

        float minimumDistance = currentProgress * totalLength;
        float travelled = 0f;

        for (int i = 0; i < path.Length - 1; i++)
        {
            Vector2 a = path[i];
            Vector2 b = path[i + 1];
            float segmentLength = Vector2.Distance(a, b);

            if (segmentLength <= Mathf.Epsilon) continue;

            float segmentEnd = travelled + segmentLength;
            if (segmentEnd < minimumDistance)
            {
                travelled = segmentEnd;
                continue;
            }

            float minT = Mathf.Clamp01((minimumDistance - travelled) / segmentLength);
            float t = Mathf.Clamp01(
                Vector2.Dot(cursor - a, b - a) / (segmentLength * segmentLength));
            t = Mathf.Max(t, minT);

            Vector2 projected = Vector2.Lerp(a, b, t);
            float distance = Vector2.Distance(cursor, projected);
            float projectedProgress = (travelled + segmentLength * t) / totalLength;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestProgress = Mathf.Max(currentProgress, projectedProgress);
            }

            travelled = segmentEnd;
        }

        return bestDistance < float.MaxValue;
    }

    private void UpdatePaintedLine(Vector2[] path, float normalizedProgress)
    {
        paintedPoints.Clear();
        paintedPoints.Add(ToLocal(path[0]));

        float targetDistance = normalizedProgress * GetPathLength(path);
        float travelled = 0f;

        for (int i = 0; i < path.Length - 1; i++)
        {
            float segmentLength = Vector2.Distance(path[i], path[i + 1]);

            if (travelled + segmentLength <= targetDistance)
            {
                paintedPoints.Add(ToLocal(path[i + 1]));
            }
            else
            {
                float remaining = targetDistance - travelled;
                if (remaining > 0f && segmentLength > 0f)
                {
                    Vector2 point = Vector2.Lerp(
                        path[i], path[i + 1], Mathf.Clamp01(remaining / segmentLength));
                    paintedPoints.Add(ToLocal(point));
                }

                break;
            }

            travelled += segmentLength;
        }

        if (routeLine != null)
            routeLine.SetPoints(paintedPoints);
    }

    private float GetPathLength(Vector2[] path)
    {
        float length = 0f;
        for (int i = 0; i < path.Length - 1; i++)
            length += Vector2.Distance(path[i], path[i + 1]);
        return length;
    }

    private Vector2 ToLocal(Vector2 normalized)
    {
        Rect rect = mapArea.rect;
        return new Vector2(
            rect.xMin + normalized.x * rect.width,
            rect.yMin + normalized.y * rect.height
        );
    }

    public void ConfirmRoute()
    {
        if (!routeCompleted) return;

        if (routesPanel != null) routesPanel.SetActive(false);
        if (nextPanel != null) nextPanel.SetActive(true);
    }

    public void ResetRoute()
    {
        paintedPoints.Clear();
        candidateRoutes.Clear();

        if (routeProgress != null)
            Array.Clear(routeProgress, 0, routeProgress.Length);

        selectedRoute = -1;
        attemptStarted = false;
        drawing = false;
        routeCompleted = false;

        if (routeLine != null)
            routeLine.SetPoints(paintedPoints);

        if (confirmButton != null)
            confirmButton.interactable = false;

        Debug.Log("Rota reiniciada.");
    }
}
