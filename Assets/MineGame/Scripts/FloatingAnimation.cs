using UnityEngine;
using DG.Tweening;

public class FloatingAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private float moveDistance = 20f;
    [SerializeField] private float duration = 2f;
    
    private Vector2 _startPosition;

    private void Start()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
        _startPosition = rectTransform.anchoredPosition;
        
        StartFloating();
    }

    private void StartFloating()
    {
        Sequence sequence = DOTween.Sequence();
        
        sequence.Append(rectTransform.DOAnchorPosY(_startPosition.y + moveDistance, duration).SetEase(Ease.InOutSine));
        sequence.Append(rectTransform.DOAnchorPosY(_startPosition.y - moveDistance, duration).SetEase(Ease.InOutSine));
        sequence.Append(rectTransform.DOAnchorPosY(_startPosition.y, duration).SetEase(Ease.InOutSine));
        
        sequence.SetLoops(-1);
    }

    private void OnDestroy()
    {
        rectTransform.DOKill();
    }
}