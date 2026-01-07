using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StealthMeter : MonoBehaviour
{
    public static StealthMeter Instance;

    [Header("UI References")]
    [SerializeField] private GameObject meterPanel;
    [SerializeField] private Image meterFill;
    [SerializeField] private Image alertIcon;
    [SerializeField] private RectTransform meterRect;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Detection Settings")]
    [SerializeField] private float detectionTime = 2f;
    [SerializeField] private float decayRate = 0.5f;

    [Header("Colors")]
    [SerializeField] private Color safeColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Color warningColor = new Color(1f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color dangerColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color detectedColor = new Color(1f, 0f, 0f, 1f);

    [Header("Animation")]
    [SerializeField] private float showDuration = 0.2f;
    [SerializeField] private float hideDuration = 0.3f;
    [SerializeField] private float pulseSpeed = 0.2f;
    [SerializeField] private float shakeIntensity = 5f;

    private float _currentDetection = 0f;
    private bool _isBeingDetected = false;
    private bool _isVisible = false;
    private bool _isDetected = false;
    private Tween _pulseTween;
    private Tween _iconPulseTween;

    public System.Action OnPlayerDetected;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (meterPanel != null)
        {
            meterPanel.SetActive(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (meterFill != null)
        {
            meterFill.fillAmount = 0f;
        }

        if (alertIcon != null)
        {
            alertIcon.color = safeColor;
        }
    }

    private void Update()
    {
        if (_isDetected) return;

        if (_isBeingDetected)
        {
            IncreaseDetection();
        }
        else if (_currentDetection > 0f)
        {
            DecreaseDetection();
        }
    }

    public void SetDetecting(bool detecting)
    {
        if (_isDetected) return;

        bool wasDetecting = _isBeingDetected;
        _isBeingDetected = detecting;

        if (detecting && !wasDetecting)
        {
            ShowMeter();
        }
    }

    private void IncreaseDetection()
    {
        _currentDetection += Time.deltaTime / detectionTime;
        _currentDetection = Mathf.Clamp01(_currentDetection);

        UpdateMeterUI();

        if (_currentDetection >= 1f)
        {
            TriggerDetection();
        }
    }

    private void DecreaseDetection()
    {
        _currentDetection -= Time.deltaTime * decayRate;
        _currentDetection = Mathf.Max(0f, _currentDetection);

        UpdateMeterUI();

        if (_currentDetection <= 0f && _isVisible)
        {
            HideMeter();
        }
    }

    private void UpdateMeterUI()
    {
        if (meterFill != null)
        {
            meterFill.fillAmount = _currentDetection;

            Color targetColor;
            if (_currentDetection < 0.5f)
            {
                targetColor = Color.Lerp(safeColor, warningColor, _currentDetection * 2f);
            }
            else
            {
                targetColor = Color.Lerp(warningColor, dangerColor, (_currentDetection - 0.5f) * 2f);
            }
            meterFill.color = targetColor;
        }

        if (alertIcon != null)
        {
            Color iconColor;
            if (_currentDetection < 0.5f)
            {
                iconColor = Color.Lerp(safeColor, warningColor, _currentDetection * 2f);
            }
            else
            {
                iconColor = Color.Lerp(warningColor, dangerColor, (_currentDetection - 0.5f) * 2f);
            }
            alertIcon.color = iconColor;
        }

        if (_currentDetection > 0.7f && _pulseTween == null)
        {
            StartPulse();
        }
        else if (_currentDetection <= 0.7f && _pulseTween != null)
        {
            StopPulse();
        }
    }

    private void TriggerDetection()
    {
        _isDetected = true;
        _isBeingDetected = false;

        StopPulse();

        if (meterFill != null)
        {
            meterFill.color = detectedColor;
        }

        if (alertIcon != null)
        {
            alertIcon.color = detectedColor;
            alertIcon.transform.DOKill();
            alertIcon.transform.DOScale(1.5f, 0.15f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                alertIcon.transform.DOScale(1f, 0.1f).SetEase(Ease.InQuad);
            });
        }

        if (meterRect != null)
        {
            meterRect.DOShakePosition(0.3f, shakeIntensity * 2f, 20, 90f);
        }

        OnPlayerDetected?.Invoke();

        DOVirtual.DelayedCall(0.5f, () =>
        {
            HideMeter();
        });
    }

    public void ResetDetection()
    {
        _isDetected = false;
        _isBeingDetected = false;
        _currentDetection = 0f;

        StopPulse();

        if (meterFill != null)
        {
            meterFill.fillAmount = 0f;
            meterFill.color = safeColor;
        }

        if (alertIcon != null)
        {
            alertIcon.color = safeColor;
        }

        HideMeter();
    }

    private void ShowMeter()
    {
        if (_isVisible) return;

        _isVisible = true;

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.DOFade(1f, showDuration).SetEase(Ease.OutQuad);
        }

        if (meterRect != null)
        {
            meterRect.DOKill();
            meterRect.localScale = Vector3.one * 0.5f;
            meterRect.DOScale(1f, showDuration).SetEase(Ease.OutBack);
        }
    }

    private void HideMeter()
    {
        if (!_isVisible) return;

        _isVisible = false;

        StopPulse();

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.DOFade(0f, hideDuration).SetEase(Ease.InQuad);
        }
    }

    private void StartPulse()
    {
        if (meterRect == null) return;

        _pulseTween = meterRect.DOScale(1.05f, pulseSpeed)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        if (alertIcon != null)
        {
            _iconPulseTween = alertIcon.DOFade(0.5f, pulseSpeed)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void StopPulse()
    {
        _pulseTween?.Kill();
        _pulseTween = null;

        _iconPulseTween?.Kill();
        _iconPulseTween = null;

        if (meterRect != null)
        {
            meterRect.localScale = Vector3.one;
        }

        if (alertIcon != null)
        {
            Color c = alertIcon.color;
            c.a = 1f;
            alertIcon.color = c;
        }
    }

    public float GetDetectionLevel()
    {
        return _currentDetection;
    }

    public bool IsDetected()
    {
        return _isDetected;
    }

    private void OnDestroy()
    {
        _pulseTween?.Kill();
        _iconPulseTween?.Kill();
        if (canvasGroup != null) canvasGroup.DOKill();
        if (meterRect != null) meterRect.DOKill();
        if (alertIcon != null) alertIcon.transform.DOKill();
    }
}
