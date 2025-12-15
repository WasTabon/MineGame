using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using TMPro;

public class SlideShowManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image slideImage;
    [SerializeField] private TextMeshProUGUI slideText;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private Button skipButton;

    [Header("Slides Data")]
    [SerializeField] private Sprite[] slides;
    [SerializeField] private string[] slideTexts;

    [Header("Timing Settings")]
    [SerializeField] private float slideDisplayDuration = 3f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private int currentSlideIndex = 0;
    private bool isSkipped = false;

    private int isFirst;

    private void Awake()
    {
        isFirst = PlayerPrefs.GetInt("cutscene", 0);
        if (isFirst == 0)
        {
            skipButton.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        skipButton.onClick.AddListener(Skip);
        StartSlideShow();
    }

    private void StartSlideShow()
    {
        slideImage.color = new Color(1, 1, 1, 0);
        slideText.color = new Color(slideText.color.r, slideText.color.g, slideText.color.b, 0);
        panelCanvasGroup.alpha = 1;
        
        StartCoroutine(PlaySlideShow());
    }

    private IEnumerator PlaySlideShow()
    {
        for (currentSlideIndex = 0; currentSlideIndex < slides.Length; currentSlideIndex++)
        {
            if (isSkipped) break;

            slideImage.sprite = slides[currentSlideIndex];
            slideText.text = slideTexts[currentSlideIndex];

            slideImage.DOFade(1, fadeInDuration);
            slideText.DOFade(1, fadeInDuration);

            yield return new WaitForSeconds(fadeInDuration + slideDisplayDuration);

            if (isSkipped) break;

            slideImage.DOFade(0, fadeOutDuration);
            slideText.DOFade(0, fadeOutDuration);

            yield return new WaitForSeconds(fadeOutDuration);
        }

        ClosePanel();
    }

    private void Skip()
    {
        isSkipped = true;
        DOTween.Kill(slideImage);
        DOTween.Kill(slideText);
        ClosePanel();
    }

    private void ClosePanel()
    {
        panelCanvasGroup.DOFade(0, 0.5f).OnComplete(() =>
        {
            PlayerPrefs.SetInt("cutscene", 1);
            PlayerPrefs.Save();
            gameObject.SetActive(false);
        });
        Invoke("Close", 0.5f);
    }

    private void Close()
    {
        panelCanvasGroup.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        skipButton.onClick.RemoveListener(Skip);
        DOTween.Kill(slideImage);
        DOTween.Kill(slideText);
        DOTween.Kill(panelCanvasGroup);
    }
}