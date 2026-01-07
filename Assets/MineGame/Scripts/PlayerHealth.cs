using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invincibilityDuration = 1.5f;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackDuration = 0.3f;

    [Header("UI")]
    [SerializeField] private Image healthFill;
    [SerializeField] private RectTransform healthBarRect;

    [Header("Visual Feedback")]
    [SerializeField] private float blinkDuration = 0.1f;
    [SerializeField] private int blinkCount = 5;

    [Header("References")]
    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Renderer playerRenderer;

    [Header("Audio")]
    [SerializeField] private AudioClip hurtSound;

    private int _currentHealth;
    private bool _isInvincible = false;
    private bool _isKnockedBack = false;

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
        _currentHealth = maxHealth;
        UpdateHealthUI();

        if (playerRb == null)
        {
            playerRb = GetComponent<Rigidbody>();
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        if (playerRenderer == null)
        {
            playerRenderer = GetComponentInChildren<Renderer>();
        }
    }

    public void TakeDamage(Vector3 damageSourcePosition)
    {
        if (_isInvincible || _currentHealth <= 0) return;

        _currentHealth--;

        UpdateHealthUI();
        AnimateHealthBar();
        ApplyKnockback(damageSourcePosition);
        StartInvincibility();

        if (hurtSound != null && MusicController.Instance != null)
        {
            MusicController.Instance.PlaySpecificSound(hurtSound);
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthFill != null)
        {
            float fillAmount = (float)_currentHealth / maxHealth;
            healthFill.DOFillAmount(fillAmount, 0.3f).SetEase(Ease.OutQuad);

            //Color targetColor = Color.Lerp(Color.red, Color.green, fillAmount);
            //healthFill.DOColor(targetColor, 0.3f);
        }
    }

    private void AnimateHealthBar()
    {
        if (healthBarRect != null)
        {
            healthBarRect.DOKill();
            healthBarRect.localScale = Vector3.one;
            healthBarRect.DOShakeScale(0.3f, 0.3f, 10, 90f);
        }
    }

    private void ApplyKnockback(Vector3 sourcePosition)
    {
        if (playerRb == null || playerMovement == null) return;

        _isKnockedBack = true;
        playerMovement.enabled = false;

        Vector3 knockbackDirection = (transform.position - sourcePosition).normalized;
        knockbackDirection.y = 0;

        playerRb.velocity = Vector3.zero;
        playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);

        DOVirtual.DelayedCall(knockbackDuration, () =>
        {
            _isKnockedBack = false;
            if (playerMovement != null && _currentHealth > 0)
            {
                playerMovement.enabled = true;
            }
        });
    }

    private void StartInvincibility()
    {
        _isInvincible = true;

        if (playerRenderer != null)
        {
            StartCoroutine(BlinkCoroutine());
        }

        DOVirtual.DelayedCall(invincibilityDuration, () =>
        {
            _isInvincible = false;
        });
    }

    private System.Collections.IEnumerator BlinkCoroutine()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            if (playerRenderer != null)
            {
                playerRenderer.enabled = false;
            }
            yield return new WaitForSeconds(blinkDuration);

            if (playerRenderer != null)
            {
                playerRenderer.enabled = true;
            }
            yield return new WaitForSeconds(blinkDuration);
        }

        if (playerRenderer != null)
        {
            playerRenderer.enabled = true;
        }
    }

    private void Die()
    {
        if (playerMovement != null)
        {
            playerMovement.Die();
        }
    }

    public bool IsInvincible()
    {
        return _isInvincible;
    }

    public bool IsKnockedBack()
    {
        return _isKnockedBack;
    }

    public int GetCurrentHealth()
    {
        return _currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    private void OnDestroy()
    {
        if (healthFill != null) healthFill.DOKill();
        if (healthBarRect != null) healthBarRect.DOKill();
    }
}
