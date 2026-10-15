using UnityEngine;
using UnityEngine.UI;

public class PackagingManager : MonoBehaviour
{
    //variaveis

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

    // guardando o estado dos itens
    private bool isPackagePlaced;
    private bool isLabelPlaced;

    // permitindo que outros scripts acessem o estado dos itens
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

    public bool ItsOnBox(Vector2 screenPosition, Camera eventCamera)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(boxArea, screenPosition, eventCamera);
    }

    public void TryPlacePackage()
    {
        if (isPackagePlaced)
            return;
        
        isPackagePlaced = true;

        packaging.gameObject.SetActive(false);

        closedBox.SetActive(false);
        packagedBox.SetActive(true);
        labeledBox.SetActive(false);

        UpdateProgress();

    }

    public void TryPutLabel()
    {
        if (!isPackagePlaced || isLabelPlaced)
            return;

        isLabelPlaced = true;

        label.gameObject.SetActive(false);

        closedBox.SetActive(false);
        packagedBox.SetActive(false);
        labeledBox.SetActive(true);

        UpdateProgress();

    }

    private void UpdateProgress()
    {
        if (proceedButton != null)
        {
            proceedButton.interactable = isPackagePlaced && isLabelPlaced;
        }
    }

    public void GoToNextPhase()
    {
        if (!isPackagePlaced || !isLabelPlaced)
            return;
        
        packagingPanel.SetActive(false);
        nextScenePanel.SetActive(true);
    }
}
