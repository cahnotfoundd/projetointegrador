
using UnityEngine;
using UnityEngine.UI;

public class PackagingManager : MonoBehaviour
{
    [Header("Box States")]
    [SerializeField] private GameObject closedBox;
    [SerializeField] private GameObject packagedBox;
    [SerializeField] private GameObject labeledBox;

    [Header("Itens")]
    [SerializeField] private GameObject packaging;
    [SerializeField] private GameObject label;
    [SerializeField] private RectTransform boxArea;

    [Header("Navigation")]
    [SerializeField] private Button proceedButton;
    [SerializeField] private GameObject packagingPanel;
    [SerializeField] private GameObject nextScenePanel;

    [Header("Itens Arrastaveis")]
    [SerializeField] private DragDropPackaging packagingDrag;
    [SerializeField] private DragDropPackaging labelDrag;

    [Header("Cronometro")]
    [SerializeField]
    private CronometroPreparacao cronometroPreparacao;

    private bool isPackagePlaced;
    private bool isLabelPlaced;

    public bool PackagePlaced => isPackagePlaced;
    public bool LabelPlaced => isLabelPlaced;

    private void Start()
    {
        AtualizarVisual();
    }

    public bool ItsOnBox(
        Vector2 screenPosition,
        Camera eventCamera)
    {
        if (boxArea == null)
            return false;

        return RectTransformUtility
            .RectangleContainsScreenPoint(
                boxArea,
                screenPosition,
                eventCamera
            );
    }

    public void TryPlacePackage()
    {
        if (isPackagePlaced)
            return;

        isPackagePlaced = true;

        AtualizarVisual();

        LogiVeryEfeitosSonoros.Instancia?
            .TocarEmbalagem();
    }

    public void TryPutLabel()
    {
        if (!isPackagePlaced || isLabelPlaced)
            return;

        isLabelPlaced = true;

        AtualizarVisual();

        LogiVeryEfeitosSonoros.Instancia?
            .TocarEtiqueta();
    }

    private void AtualizarVisual()
    {
        if (packaging != null)
            packaging.SetActive(!isPackagePlaced);

        if (label != null)
            label.SetActive(!isLabelPlaced);

        if (closedBox != null)
            closedBox.SetActive(!isPackagePlaced);

        if (packagedBox != null)
        {
            packagedBox.SetActive(
                isPackagePlaced && !isLabelPlaced
            );
        }

        if (labeledBox != null)
            labeledBox.SetActive(isLabelPlaced);

        if (proceedButton != null)
        {
            proceedButton.interactable =
                isPackagePlaced && isLabelPlaced;
        }
    }

    public void GoToNextPhase()
    {
        if (!isPackagePlaced || !isLabelPlaced)
            return;

        if (packagingPanel != null)
            packagingPanel.SetActive(false);

        if (nextScenePanel != null)
            nextScenePanel.SetActive(true);

        if (cronometroPreparacao != null)
            cronometroPreparacao.Parar();
    }

    public void CapturarEstado(
        LogiVerySaveManager.Embalagem dados)
    {
        if (dados == null)
            return;

        dados.embrulhoAplicado = isPackagePlaced;
        dados.etiquetaAplicada = isLabelPlaced;
    }

    public void RestaurarEstado(
        LogiVerySaveManager.Embalagem dados)
    {
        if (dados == null)
            return;

        // Reconstroi sem tocar sons nem mudar
        // de etapa automaticamente.
        if (packagingDrag != null)
            packagingDrag.ResetPackagingItem();

        if (labelDrag != null)
            labelDrag.ResetPackagingItem();

        isPackagePlaced = dados.embrulhoAplicado;

        isLabelPlaced =
            dados.embrulhoAplicado &&
            dados.etiquetaAplicada;

        AtualizarVisual();
    }

    public void ResetPackaging()
    {
        isPackagePlaced = false;
        isLabelPlaced = false;

        if (packagingDrag != null)
            packagingDrag.ResetPackagingItem();

        if (labelDrag != null)
            labelDrag.ResetPackagingItem();

        AtualizarVisual();

        Debug.Log("Embalagem reiniciada.");
    }
}
