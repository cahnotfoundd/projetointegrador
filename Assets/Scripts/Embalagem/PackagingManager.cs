
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
    [SerializeField] private CronometroPreparacao cronometroPreparacao;

    private bool isPackagePlaced;
    private bool isLabelPlaced;

    public bool PackagePlaced => isPackagePlaced;
    public bool LabelPlaced => isLabelPlaced;

    private void Start()
    {
        if (closedBox != null)
            closedBox.SetActive(true);

        if (packagedBox != null)
            packagedBox.SetActive(false);

        if (labeledBox != null)
            labeledBox.SetActive(false);

        if (proceedButton != null)
            proceedButton.interactable = false;
    }

    public bool ItsOnBox(
        Vector2 screenPosition,
        Camera eventCamera)
    {
        if (boxArea == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
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

        if (packaging != null)
            packaging.SetActive(false);

        if (closedBox != null)
            closedBox.SetActive(false);

        if (packagedBox != null)
            packagedBox.SetActive(true);

        if (labeledBox != null)
            labeledBox.SetActive(false);

        // AUDIO: embrulho colocado corretamente.
        LogiVeryEfeitosSonoros.Instancia?
            .TocarEmbalagem();

        UpdateProgress();
    }

    public void TryPutLabel()
    {
        if (!isPackagePlaced || isLabelPlaced)
            return;

        isLabelPlaced = true;

        if (label != null)
            label.SetActive(false);

        if (closedBox != null)
            closedBox.SetActive(false);

        if (packagedBox != null)
            packagedBox.SetActive(false);

        if (labeledBox != null)
            labeledBox.SetActive(true);

        // AUDIO: etiqueta aplicada corretamente.
        LogiVeryEfeitosSonoros.Instancia?
            .TocarEtiqueta();

        UpdateProgress();
    }

    private void UpdateProgress()
    {
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

    public void ResetPackaging()
    {
        isPackagePlaced = false;
        isLabelPlaced = false;

        if (packaging != null)
            packaging.SetActive(true);

        if (label != null)
            label.SetActive(true);

        if (closedBox != null)
            closedBox.SetActive(true);

        if (packagedBox != null)
            packagedBox.SetActive(false);

        if (labeledBox != null)
            labeledBox.SetActive(false);

        if (packagingDrag != null)
            packagingDrag.ResetPackagingItem();

        if (labelDrag != null)
            labelDrag.ResetPackagingItem();

        if (proceedButton != null)
            proceedButton.interactable = false;

        Debug.Log("Embalagem reiniciada.");
    }
}
