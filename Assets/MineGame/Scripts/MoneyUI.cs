using UnityEngine;
using TMPro;
using DG.Tweening;

public class MoneyUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private RectTransform moneyHandler;
    
    [Header("Animation Settings")]
    [SerializeField] private float punchScale = 0.15f;
    [SerializeField] private float punchDuration = 0.3f;
    [SerializeField] private float counterDuration = 0.5f;
    
    private int _displayedMoney;
    private Tween _counterTween;
    
    private void Start()
    {
        if (moneyText == null)
        {
            Debug.LogError("Money text is null");
        }
        
        if (moneyHandler == null)
        {
            Debug.LogError("Money handler is null");
        }
        
        if (WalletController.Instance == null)
        {
            Debug.LogError("WalletController Instance is null");
            return;
        }
        
        _displayedMoney = WalletController.Instance.Money;
        UpdateMoneyText(_displayedMoney);
        
        WalletController.Instance.OnMoneyChanged += OnMoneyChanged;
    }
    
    private void OnDestroy()
    {
        if (WalletController.Instance != null)
        {
            WalletController.Instance.OnMoneyChanged -= OnMoneyChanged;
        }
        
        _counterTween?.Kill();
    }
    
    private void OnMoneyChanged(int previousMoney, int newMoney)
    {
        int difference = newMoney - previousMoney;
        
        if (difference > 0)
        {
            AnimateMoneyIncrease(previousMoney, newMoney);
        }
        else
        {
            _displayedMoney = newMoney;
            UpdateMoneyText(newMoney);
        }
    }
    
    private void AnimateMoneyIncrease(int fromMoney, int toMoney)
    {
        _counterTween?.Kill();
        
        if (moneyHandler != null)
        {
            Sequence punchSequence = DOTween.Sequence();
            punchSequence.Append(moneyHandler.DOScale(Vector3.one * (1f + punchScale), punchDuration * 0.5f).SetEase(Ease.OutQuad));
            punchSequence.Append(moneyHandler.DOScale(Vector3.one, punchDuration * 0.5f).SetEase(Ease.InQuad));
        }
        else
        {
            Debug.LogError("Money handler is null in AnimateMoneyIncrease");
        }
        
        _counterTween = DOTween.To(
            () => _displayedMoney,
            x => 
            {
                _displayedMoney = x;
                UpdateMoneyText(_displayedMoney);
            },
            toMoney,
            counterDuration
        ).SetEase(Ease.OutQuad);
    }
    
    private void UpdateMoneyText(int money)
    {
        if (moneyText != null)
        {
            moneyText.text = money.ToString();
        }
        else
        {
            Debug.LogError("Money text is null in UpdateMoneyText");
        }
    }
}