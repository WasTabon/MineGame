using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StaminaSystem : MonoBehaviour
{
    public static StaminaSystem Instance;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float runThreshold = 0.7f;

    [Header("UI")]
    [SerializeField] private Image staminaFill;
    [SerializeField] private RectTransform staminaBarRect;
    [SerializeField] private CanvasGroup staminaBarCanvasGroup;

    [Header("UI Animation")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float fadeOutDelay = 2f;

    [Header("Colors")]
    [SerializeField] private Color fullColor = new Color(0.2f, 0.8f, 1f);
    [SerializeField] private Color midColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color emptyColor = new Color(1f, 0.3f, 0.2f);

    [Header("Effects")]
    [SerializeField] private float exhaustedPulseSpeed = 0.3f;
    [SerializeField] private float exhaustedPulseIntensity = 0.3f;

    private float _currentStamina;
    private bool _isExhausted = false;
    private float _timeSinceLastDrain = 0f;
    private Tween _fadeOutTween;
    private Tween _pulseTween;
    private bool _barVisible = false;

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
        _currentStamina = maxStamina;

        if (staminaBarCanvasGroup != null)
        {
            staminaBarCanvasGroup.alpha = 0f;
        }

        UpdateStaminaUI();
    }

    private void Update()
    {
        _timeSinceLastDrain += Time.deltaTime;

        if (_timeSinceLastDrain >= fadeOutDelay && _barVisible && _currentStamina >= maxStamina)
        {
            HideStaminaBar();
        }
    }

    public float ProcessInput(float inputMagnitude)
    {
        bool isRunning = inputMagnitude > runThreshold;
        bool isWalking = inputMagnitude > 0.1f && !isRunning;
        bool isIdle = inputMagnitude <= 0.1f;

        if (isRunning && !_isExhausted)
        {
            DrainStamina(staminaDrainRate * Time.deltaTime);
        }
        else if (isWalking || isIdle)
        {
            RegenStamina(staminaRegenRate * Time.deltaTime);
        }

        if (_isExhausted)
        {
            return Mathf.Min(inputMagnitude, runThreshold - 0.1f);
        }

        return inputMagnitude;
    }

    private void DrainStamina(float amount)
    {
        _currentStamina -= amount;
        _currentStamina = Mathf.Max(0f, _currentStamina);
        _timeSinceLastDrain = 0f;

        ShowStaminaBar();
        UpdateStaminaUI();

        if (_currentStamina <= 0f && !_isExhausted)
        {
            SetExhausted(true);
        }
    }

    private void RegenStamina(float amount)
    {
        if (_currentStamina >= maxStamina) return;

        _currentStamina += amount;
        _currentStamina = Mathf.Min(maxStamina, _currentStamina);

        UpdateStaminaUI();

        if (_isExhausted && _currentStamina >= maxStamina * 0.3f)
        {
            SetExhausted(false);
        }
    }

    private void SetExhausted(bool exhausted)
    {
        _isExhausted = exhausted;

        if (exhausted)
        {
            StartExhaustedPulse();
            AnimateExhausted();
        }
        else
        {
            StopExhaustedPulse();
        }
    }

    private void UpdateStaminaUI()
    {
        if (staminaFill == null) return;

        float fillAmount = _currentStamina / maxStamina;
        staminaFill.fillAmount = fillAmount;

        Color targetColor;
        if (fillAmount > 0.5f)
        {
            targetColor = Color.Lerp(midColor, fullColor, (fillAmount - 0.5f) * 2f);
        }
        else
        {
            targetColor = Color.Lerp(emptyColor, midColor, fillAmount * 2f);
        }

        staminaFill.color = targetColor;
    }

    private void ShowStaminaBar()
    {
        if (_barVisible) return;
        if (staminaBarCanvasGroup == null) return;

        _barVisible = true;

        _fadeOutTween?.Kill();
        staminaBarCanvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
    }

    private void HideStaminaBar()
    {
        if (!_barVisible) return;
        if (staminaBarCanvasGroup == null) return;
        if (_isExhausted) return;

        _barVisible = false;

        _fadeOutTween?.Kill();
        _fadeOutTween = staminaBarCanvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad);
    }

    private void AnimateExhausted()
    {
        if (staminaBarRect != null)
        {
            staminaBarRect.DOKill();
            staminaBarRect.DOShakePosition(0.5f, 10f, 20, 90f);
        }
    }

    private void StartExhaustedPulse()
    {
        if (staminaFill == null) return;

        StopExhaustedPulse();

        _pulseTween = staminaFill.DOFade(1f - exhaustedPulseIntensity, exhaustedPulseSpeed)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopExhaustedPulse()
    {
        _pulseTween?.Kill();

        if (staminaFill != null)
        {
            Color c = staminaFill.color;
            c.a = 1f;
            staminaFill.color = c;
        }
    }

    public float GetCurrentStamina()
    {
        return _currentStamina;
    }

    public float GetMaxStamina()
    {
        return maxStamina;
    }

    public float GetStaminaPercent()
    {
        return _currentStamina / maxStamina;
    }

    public bool IsExhausted()
    {
        return _isExhausted;
    }

    private void OnDestroy()
    {
        _fadeOutTween?.Kill();
        _pulseTween?.Kill();
        if (staminaBarRect != null) staminaBarRect.DOKill();
        if (staminaBarCanvasGroup != null) staminaBarCanvasGroup.DOKill();
        if (staminaFill != null) staminaFill.DOKill();
    }
}
