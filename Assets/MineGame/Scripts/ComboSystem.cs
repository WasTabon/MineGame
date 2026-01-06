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

    [Header("Combo Settings")]
    [SerializeField] private float comboResetTime = 30f;

    [Header("Animation Settings")]
    [SerializeField] private float punchScale = 0.3f;
    [SerializeField] private float punchDuration = 0.3f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private int _currentCombo = 0;
    private float _comboTimer = 0f;
    private Coroutine _comboTimerCoroutine;
    private CanvasGroup _panelCanvasGroup;

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

        if (_comboTimerCoroutine != null)
        {
            StopCoroutine(_comboTimerCoroutine);
        }
        _comboTimerCoroutine = StartCoroutine(ComboTimerRoutine());

        UpdateUI();
        AnimateCombo();
    }

    public void ResetCombo()
    {
        if (_currentCombo > 1)
        {
            HideUI();
        }

        _currentCombo = 0;

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
            yield return null;
        }

        ResetCombo();
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

    private void AnimateCombo()
    {
        if (_panelCanvasGroup == null) return;

        if (_currentCombo == 1)
        {
            _panelCanvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
        }

        if (comboText != null)
        {
            comboText.transform.DOKill();
            comboText.transform.localScale = Vector3.one;
            comboText.transform.DOPunchScale(Vector3.one * punchScale, punchDuration, 1, 0.5f);
        }

        if (multiplierText != null && _currentCombo > 1)
        {
            multiplierText.transform.DOKill();
            multiplierText.transform.localScale = Vector3.one;
            multiplierText.transform.DOPunchScale(Vector3.one * punchScale, punchDuration, 1, 0.5f);
        }
    }

    private void HideUI()
    {
        if (_panelCanvasGroup != null)
        {
            _panelCanvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad);
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
        if (comboText != null) comboText.transform.DOKill();
        if (multiplierText != null) multiplierText.transform.DOKill();
        if (_panelCanvasGroup != null) _panelCanvasGroup.DOKill();
    }
}
