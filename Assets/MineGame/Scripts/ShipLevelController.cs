using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class ShipLevelController : MonoBehaviour
{
    [SerializeField] private Transform movingObject;
    [SerializeField] private Transform[] targetPositions;
    [SerializeField] private PlanetLevel[] planetLevels;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    
    private int currentIndex = 0;
    private bool isMoving = false;

    private void Start()
    {
        UpdateButtonState();
    }

    public void MoveNext()
    {
        if (isMoving || targetPositions.Length == 0) return;

        currentIndex = (currentIndex + 1) % targetPositions.Length;
        MoveToCurrentTarget();
    }

    public void MovePrevious()
    {
        if (isMoving || targetPositions.Length == 0) return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = targetPositions.Length - 1;
        
        MoveToCurrentTarget();
    }

    private void MoveToCurrentTarget()
    {
        isMoving = true;
        
        Vector3 targetPosition = new Vector3(
            targetPositions[currentIndex].position.x,
            targetPositions[currentIndex].position.y,
            movingObject.position.z
        );

        movingObject.DOMove(targetPosition, 1f).OnComplete(() => 
        {
            isMoving = false;
            UpdateButtonState();
        });
    }

    private void UpdateButtonState()
    {
        if (planetLevels.Length == 0 || currentIndex >= planetLevels.Length) return;

        PlanetLevel currentPlanet = planetLevels[currentIndex];
        
        if (currentPlanet.IsBought())
        {
            actionButtonText.text = "PLAY";
        }
        else
        {
            actionButtonText.text = "PURCHASE";
        }
    }

    public void OnActionButtonClick()
    {
        if (planetLevels.Length == 0 || currentIndex >= planetLevels.Length) return;

        PlanetLevel currentPlanet = planetLevels[currentIndex];
        
        if (currentPlanet.IsBought())
        {
            UIController.Instance.ShowLevelPanel(currentPlanet.GetLevelNumber());
        }
        else
        {
            UIController.Instance.ShowBuyLevelPanel(currentPlanet);
        }
    }
}