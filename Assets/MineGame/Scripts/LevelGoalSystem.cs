using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class LevelGoalSystem : MonoBehaviour
{
    public static LevelGoalSystem Instance;

    [Header("Goal Settings")]
    [SerializeField] private int crystalsRequired = 3;

    [Header("Progress UI")]
    [SerializeField] private GameObject progressPanel;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image progressFill;

    [Header("Victory Panel")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryTitleText;
    [SerializeField] private TextMeshProUGUI victoryStatsText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button replayButton;

    [Header("Animation Settings")]
    [SerializeField] private float progressPunchScale = 0.2f;
    [SerializeField] private float progressPunchDuration = 0.3f;
    [SerializeField] private float victoryPanelDuration = 0.5f;
    [SerializeField] private float victoryDelay = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip crystalCollectSound;
    [SerializeField] private AudioClip victorySound;

    private int _crystalsCollected = 0;
    private bool _levelCompleted = false;
    private int _totalMoneyEarned = 0;

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
        _crystalsCollected = 0;
        _levelCompleted = false;
        _totalMoneyEarned = 0;

        UpdateProgressUI();

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (replayButton != null)
        {
            replayButton.onClick.AddListener(OnReplayClicked);
        }
    }

    public void OnCrystalCollected(int moneyEarned)
    {
        if (_levelCompleted) return;

        _crystalsCollected++;
        _totalMoneyEarned += moneyEarned;

        UpdateProgressUI();
        AnimateProgress();

        if (crystalCollectSound != null && MusicController.Instance != null)
        {
            MusicController.Instance.PlaySpecificSound(crystalCollectSound);
        }

        if (_crystalsCollected >= crystalsRequired)
        {
            CompleteLevel();
        }
    }

    private void UpdateProgressUI()
    {
        if (progressText != null)
        {
            progressText.text = $"{_crystalsCollected}/{crystalsRequired}";
        }

        if (progressFill != null)
        {
            float fillAmount = (float)_crystalsCollected / crystalsRequired;
            progressFill.DOFillAmount(fillAmount, 0.3f).SetEase(Ease.OutQuad);
        }
    }

    private void AnimateProgress()
    {
        if (progressPanel != null)
        {
            progressPanel.transform.DOKill();
            progressPanel.transform.localScale = Vector3.one;
            progressPanel.transform.DOPunchScale(Vector3.one * progressPunchScale, progressPunchDuration, 2, 0.5f);
        }

        if (progressText != null)
        {
            progressText.transform.DOKill();
            progressText.transform.localScale = Vector3.one;

            Sequence textSequence = DOTween.Sequence();
            textSequence.Append(progressText.transform.DOScale(1.3f, progressPunchDuration * 0.5f).SetEase(Ease.OutQuad));
            textSequence.Append(progressText.transform.DOScale(1f, progressPunchDuration * 0.5f).SetEase(Ease.InQuad));
        }

        if (_crystalsCollected == crystalsRequired - 1 && progressText != null)
        {
            progressText.DOColor(new Color(1f, 0.8f, 0.2f), 0.3f);
        }
    }

    private void CompleteLevel()
    {
        _levelCompleted = true;

        if (victorySound != null && MusicController.Instance != null)
        {
            MusicController.Instance.PlaySpecificSound(victorySound);
        }

        DOVirtual.DelayedCall(victoryDelay, ShowVictoryPanel);
    }

    private void ShowVictoryPanel()
    {
        if (victoryPanel == null) return;

        victoryPanel.SetActive(true);

        CanvasGroup canvasGroup = victoryPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = victoryPanel.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        victoryPanel.transform.localScale = Vector3.one * 0.5f;

        Sequence showSequence = DOTween.Sequence();

        showSequence.Append(canvasGroup.DOFade(1f, victoryPanelDuration).SetEase(Ease.OutQuad));
        showSequence.Join(victoryPanel.transform.DOScale(1f, victoryPanelDuration).SetEase(Ease.OutBack));

        if (victoryTitleText != null)
        {
            victoryTitleText.transform.localScale = Vector3.zero;
            showSequence.Append(victoryTitleText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        }

        if (victoryStatsText != null)
        {
            victoryStatsText.text = $"Crystals: {_crystalsCollected}\nMoney earned: {_totalMoneyEarned}";

            victoryStatsText.alpha = 0f;
            showSequence.Append(victoryStatsText.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));
        }

        if (continueButton != null)
        {
            continueButton.transform.localScale = Vector3.zero;
            showSequence.Append(continueButton.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
        }

        if (replayButton != null)
        {
            replayButton.transform.localScale = Vector3.zero;
            showSequence.Append(replayButton.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
        }

        Time.timeScale = 0f;
        showSequence.SetUpdate(true);
    }

    private void OnContinueClicked()
    {
        Time.timeScale = 1f;

        if (MusicController.Instance != null)
        {
            MusicController.Instance.PlayClickSound();
        }

        SceneManager.LoadScene("Levels");
    }

    private void OnReplayClicked()
    {
        Time.timeScale = 1f;

        if (MusicController.Instance != null)
        {
            MusicController.Instance.PlayClickSound();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public int GetCrystalsCollected()
    {
        return _crystalsCollected;
    }

    public int GetCrystalsRequired()
    {
        return crystalsRequired;
    }

    public bool IsLevelCompleted()
    {
        return _levelCompleted;
    }

    public float GetProgress()
    {
        return (float)_crystalsCollected / crystalsRequired;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;

        if (progressPanel != null) progressPanel.transform.DOKill();
        if (progressText != null) progressText.transform.DOKill();
        if (victoryPanel != null)
        {
            victoryPanel.transform.DOKill();
            CanvasGroup cg = victoryPanel.GetComponent<CanvasGroup>();
            if (cg != null) cg.DOKill();
        }
        if (victoryTitleText != null) victoryTitleText.transform.DOKill();
        if (victoryStatsText != null) victoryStatsText.DOKill();
        if (continueButton != null) continueButton.transform.DOKill();
        if (replayButton != null) replayButton.transform.DOKill();
    }
}
