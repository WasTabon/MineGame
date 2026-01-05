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
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private float moveDuration = 3f;
    [SerializeField] private float rotateDuration = 0.5f;
    [SerializeField] private float buttonFadeDuration = 0.3f;
    
    private int currentIndex = 0;
    private bool isMoving = false;
    private Sequence currentSequence;
    
    private CanvasGroup actionButtonCanvasGroup;
    private CanvasGroup nextButtonCanvasGroup;
    private CanvasGroup prevButtonCanvasGroup;

    private void Start()
    {
        actionButtonCanvasGroup = GetOrAddCanvasGroup(actionButton.gameObject);
        nextButtonCanvasGroup = GetOrAddCanvasGroup(nextButton.gameObject);
        prevButtonCanvasGroup = GetOrAddCanvasGroup(prevButton.gameObject);
    
        MoveToFirst();
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        var canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = obj.AddComponent<CanvasGroup>();
        return canvasGroup;
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
        
        currentSequence?.Kill();
        
        HideButtons();

        Vector3 planetPosition = targetPositions[currentIndex].position;
        Vector3 targetPosition = new Vector3(
            planetPosition.x,
            planetPosition.y,
            planetPosition.z + 575f
        );

        Vector3 moveDirection = targetPosition - movingObject.position;
        
        Quaternion flyRotation = Quaternion.LookRotation(moveDirection);
        
        Quaternion lookAtPlanetRotation = Quaternion.LookRotation(planetPosition - targetPosition);

        currentSequence = DOTween.Sequence();
        
        currentSequence.Append(
            movingObject.DORotateQuaternion(flyRotation, rotateDuration)
                .SetEase(Ease.InOutSine)
        );
        
        currentSequence.Append(
            movingObject.DOMove(targetPosition, moveDuration)
                .SetEase(Ease.InOutQuad)
        );
        
        currentSequence.Append(
            movingObject.DORotateQuaternion(lookAtPlanetRotation, rotateDuration)
                .SetEase(Ease.InOutSine)
        );

        currentSequence.OnComplete(() =>
        {
            isMoving = false;
            UpdateButtonState();
            ShowButtons();
        });
    }

    private void HideButtons()
    {
        SetButtonsInteractable(false);
        
        actionButtonCanvasGroup.DOFade(0f, buttonFadeDuration).SetEase(Ease.OutQuad);
        nextButtonCanvasGroup.DOFade(0f, buttonFadeDuration).SetEase(Ease.OutQuad);
        prevButtonCanvasGroup.DOFade(0f, buttonFadeDuration).SetEase(Ease.OutQuad);
    }

    private void ShowButtons()
    {
        actionButtonCanvasGroup.DOFade(1f, buttonFadeDuration).SetEase(Ease.InQuad);
        nextButtonCanvasGroup.DOFade(1f, buttonFadeDuration).SetEase(Ease.InQuad);
        prevButtonCanvasGroup.DOFade(1f, buttonFadeDuration).SetEase(Ease.InQuad)
            .OnComplete(() => SetButtonsInteractable(true));
    }

    private void SetButtonsInteractable(bool interactable)
    {
        actionButton.interactable = interactable;
        nextButton.interactable = interactable;
        prevButton.interactable = interactable;
        
        actionButtonCanvasGroup.blocksRaycasts = interactable;
        nextButtonCanvasGroup.blocksRaycasts = interactable;
        prevButtonCanvasGroup.blocksRaycasts = interactable;
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
    
    public void MoveToFirst()
    {
        currentIndex = 0;
    
        Vector3 planetPosition = targetPositions[currentIndex].position;
        Vector3 targetPosition = new Vector3(
            planetPosition.x,
            planetPosition.y,
            planetPosition.z + 575f
        );
    
        Quaternion lookAtPlanetRotation = Quaternion.LookRotation(planetPosition - targetPosition);
    
        movingObject.position = targetPosition;
        movingObject.rotation = lookAtPlanetRotation;
    
        UpdateButtonState();
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

    private void OnDestroy()
    {
        currentSequence?.Kill();
    }
}