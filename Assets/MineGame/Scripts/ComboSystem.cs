using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

public class ComboSystem : MonoBehaviour
{
    public static ComboSystem Instance;

    [Header("UI References")]
    [SerializeField] private GameObject comboPanel;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private RectTransform timerFillBar;

    [Header("Combo Settings")]
    [SerializeField] private float comboResetTime = 30f;

    [Header("Animation Settings")]
    [SerializeField] private float punchScale = 0.3f;
    [SerializeField] private float punchDuration = 0.3f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Scale Animation")]
    [SerializeField] private float minScale = 0.6f;
    [SerializeField] private float maxScale = 1.2f;

    [Header("Urgency Settings")]
    [SerializeField] private float urgencyThreshold = 10f;
    [SerializeField] private float shakeIntensity = 5f;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color midColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color highColor = new Color(1f, 0.4f, 0.1f);
    [SerializeField] private Color maxColor = new Color(1f, 0.2f, 0.5f);
    [SerializeField] private Color urgencyColor = new Color(1f, 0.3f, 0.3f);

    private int _currentCombo = 0;
    private float _comboTimer = 0f;
    private Coroutine _comboTimerCoroutine;
    private CanvasGroup _panelCanvasGroup;
    private Vector3 _originalPanelPosition;
    private Tween _pulseTween;
    private Tween _urgencyShakeTween;
    private Tween _urgencyBlinkTween;
    private Tween _colorTween;
    private bool _isUrgent = false;
    private float _timerBarOriginalWidth;

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
        if (comboPanel != null)
        {
            _panelCanvasGroup = comboPanel.GetComponent<CanvasGroup>();
            if (_panelCanvasGroup == null)
            {
                _panelCanvasGroup = comboPanel.AddComponent<CanvasGroup>();
            }
            _panelCanvasGroup.alpha = 0f;
            comboPanel.SetActive(true);

            _originalPanelPosition = comboPanel.transform.localPosition;
        }

        if (timerFillBar != null)
        {
            _timerBarOriginalWidth = timerFillBar.sizeDelta.x;
        }
    }

    public float GetMultiplier()
    {
        if (_currentCombo <= 1) return 1f;
        if (_currentCombo == 2) return 1.25f;
        if (_currentCombo == 3) return 1.5f;
        if (_currentCombo == 4) return 1.75f;
        if (_currentCombo == 5) return 2f;
        return 2.5f;
    }

    public int ApplyMultiplier(int baseReward)
    {
        float multiplier = GetMultiplier();
        return Mathf.RoundToInt(baseReward * multiplier);
    }

    public void AddCombo()
    {
        _currentCombo++;
        _isUrgent = false;

        StopUrgencyEffects();

        if (_comboTimerCoroutine != null)
        {
            StopCoroutine(_comboTimerCoroutine);
        }
        _comboTimerCoroutine = StartCoroutine(ComboTimerRoutine());

        UpdateUI();
        AnimateCombo();
        UpdateComboColor();
        StartPulseIfNeeded();
    }

    public void ResetCombo()
    {
        if (_currentCombo > 1)
        {
            HideUI();
        }

        _currentCombo = 0;
        _isUrgent = false;

        StopPulse();
        StopUrgencyEffects();

        if (_comboTimerCoroutine != null)
        {
            StopCoroutine(_comboTimerCoroutine);
            _comboTimerCoroutine = null;
        }
    }

    private IEnumerator ComboTimerRoutine()
    {
        _comboTimer = comboResetTime;

        while (_comboTimer > 0f)
        {
            _comboTimer -= Time.deltaTime;

            UpdateTimerVisuals();

            if (_comboTimer <= urgencyThreshold && !_isUrgent)
            {
                _isUrgent = true;
                StartUrgencyEffect();
            }

            yield return null;
        }

        ResetCombo();
    }

    private void UpdateTimerVisuals()
    {
        float progress = _comboTimer / comboResetTime;

        if (comboText != null && !_isUrgent)
        {
            float scale = Mathf.Lerp(minScale, maxScale, progress);
            comboText.transform.localScale = Vector3.one * scale;
        }

        if (timerFillBar != null)
        {
            Vector2 size = timerFillBar.sizeDelta;
            size.x = _timerBarOriginalWidth * progress;
            timerFillBar.sizeDelta = size;
        }
    }

    private void UpdateUI()
    {
        if (comboText != null)
        {
            comboText.text = $"{_currentCombo}";
        }

        if (multiplierText != null)
        {
            float multiplier = GetMultiplier();
            multiplierText.text = $"x{multiplier:F2}";
        }
    }

    private void UpdateComboColor()
    {
        Color targetColor = GetComboColor();

        _colorTween?.Kill();

        if (comboText != null)
        {
            _colorTween = comboText.DOColor(targetColor, 0.3f).SetEase(Ease.OutQuad);
        }

        if (multiplierText != null)
        {
            multiplierText.DOColor(targetColor, 0.3f).SetEase(Ease.OutQuad);
        }
    }

    private Color GetComboColor()
    {
        if (_currentCombo <= 1) return normalColor;
        if (_currentCombo == 2) return normalColor;
        if (_currentCombo == 3) return midColor;
        if (_currentCombo == 4) return highColor;
        if (_currentCombo == 5) return highColor;
        return maxColor;
    }

    private void AnimateCombo()
    {
        if (_panelCanvasGroup == null) return;

        if (_currentCombo == 1)
        {
            comboPanel.transform.localScale = Vector3.one * 0.5f;
            comboPanel.transform.DOScale(Vector3.one, fadeInDuration).SetEase(Ease.OutBack);
            _panelCanvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
        }

        if (comboText != null)
        {
            comboText.transform.DOKill();
            comboText.transform.localScale = Vector3.one * maxScale;

            Sequence textSequence = DOTween.Sequence();
            textSequence.Append(comboText.transform.DOPunchScale(Vector3.one * punchScale, punchDuration, 2, 0.5f));
            textSequence.Join(comboText.transform.DOPunchRotation(new Vector3(0, 0, 10f), punchDuration, 5, 0.5f));
        }

        if (multiplierText != null && _currentCombo > 1)
        {
            multiplierText.transform.DOKill();
            multiplierText.transform.localScale = Vector3.one;

            Sequence multSequence = DOTween.Sequence();
            multSequence.Append(multiplierText.transform.DOScale(1.3f, punchDuration * 0.5f).SetEase(Ease.OutQuad));
            multSequence.Append(multiplierText.transform.DOScale(1f, punchDuration * 0.5f).SetEase(Ease.InQuad));
        }

        if (_currentCombo >= 3 && comboPanel != null)
        {
            comboPanel.transform.DOPunchPosition(new Vector3(Random.Range(-10f, 10f), Random.Range(-5f, 5f), 0), 0.2f, 10, 0.5f);
        }
    }

    private void StartPulseIfNeeded()
    {
        StopPulse();

        if (_currentCombo >= 4 && multiplierText != null)
        {
            float pulseSpeed = _currentCombo >= 6 ? 0.3f : 0.5f;
            float pulseIntensity = _currentCombo >= 6 ? 1.15f : 1.1f;

            _pulseTween = multiplierText.transform
                .DOScale(pulseIntensity, pulseSpeed)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void StopPulse()
    {
        _pulseTween?.Kill();
        if (multiplierText != null)
        {
            multiplierText.transform.localScale = Vector3.one;
        }
    }

    private void StartUrgencyEffect()
    {
        if (comboText != null)
        {
            _urgencyBlinkTween = DOTween.Sequence()
                .Append(comboText.DOColor(urgencyColor, 0.15f))
                .Append(comboText.DOColor(GetComboColor(), 0.15f))
                .SetLoops(-1);
        }

        if (comboPanel != null)
        {
            _urgencyShakeTween = comboPanel.transform
                .DOShakePosition(0.5f, shakeIntensity, 20, 90, false, true)
                .SetLoops(-1, LoopType.Restart);
        }
    }

    private void StopUrgencyEffects()
    {
        _urgencyShakeTween?.Kill();
        _urgencyBlinkTween?.Kill();

        if (comboPanel != null)
        {
            comboPanel.transform.localPosition = _originalPanelPosition;
        }

        if (comboText != null)
        {
            comboText.color = GetComboColor();
        }
    }

    private void HideUI()
    {
        StopPulse();
        StopUrgencyEffects();

        if (_panelCanvasGroup != null && comboPanel != null)
        {
            Sequence hideSequence = DOTween.Sequence();

            hideSequence.Append(comboPanel.transform.DOScale(1.2f, fadeOutDuration * 0.3f).SetEase(Ease.OutQuad));
            hideSequence.Append(comboPanel.transform.DOScale(0f, fadeOutDuration * 0.7f).SetEase(Ease.InBack));
            hideSequence.Join(_panelCanvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad));

            hideSequence.OnComplete(() =>
            {
                if (comboPanel != null)
                {
                    comboPanel.transform.localScale = Vector3.one;
                }
            });
        }
    }

    public int GetCurrentCombo()
    {
        return _currentCombo;
    }

    public float GetTimeRemaining()
    {
        return _comboTimer;
    }

    private void OnDestroy()
    {
        DOTween.Kill(comboText?.transform);
        DOTween.Kill(multiplierText?.transform);
        DOTween.Kill(comboPanel?.transform);
        DOTween.Kill(_panelCanvasGroup);
        _pulseTween?.Kill();
        _urgencyShakeTween?.Kill();
        _urgencyBlinkTween?.Kill();
        _colorTween?.Kill();
    }
}