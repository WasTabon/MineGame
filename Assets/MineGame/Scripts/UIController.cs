using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [Header("Panels")]
    [SerializeField] private GameObject buyLevelPanel;
    [SerializeField] private GameObject levelPanel;
    
    [Header("Buy Panel Buttons")]
    [SerializeField] private Button buyButton;
    [SerializeField] private Button closeBuyButton;
    
    [Header("Level Panel Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button closeLevelButton;
    
    private PlanetLevel _currentPlanet;
    private int _currentLevelNumber;
     
    private void Awake()
    {
        Instance = this;
        
        SetupButtons();
    }

    private void SetupButtons()
    {
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }
        else
        {
            Debug.Log("Buy button is null");
        }
        
        if (closeBuyButton != null)
        {
            closeBuyButton.onClick.AddListener(HideBuyLevelPanel);
        }
        else
        {
            Debug.Log("Close buy button is null");
        }
        
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        else
        {
            Debug.Log("Play button is null");
        }
        
        if (closeLevelButton != null)
        {
            closeLevelButton.onClick.AddListener(HideLevelPanel);
        }
        else
        {
            Debug.Log("Close level button is null");
        }
    }

    public void ShowBuyLevelPanel(PlanetLevel planet)
    {
        _currentPlanet = planet;
        
        if (buyLevelPanel != null)
        {
            buyLevelPanel.SetActive(true);
        }
        else
        {
            Debug.Log("Buy level panel is null");
        }
    }

    public void HideBuyLevelPanel()
    {
        if (buyLevelPanel != null)
        {
            buyLevelPanel.SetActive(false);
        }
        else
        {
            Debug.Log("Buy level panel is null");
        }
    }

    public void ShowLevelPanel(int levelNumber)
    {
        _currentLevelNumber = levelNumber;
        
        if (levelPanel != null)
        {
            levelPanel.SetActive(true);
        }
        else
        {
            Debug.Log("Level panel is null");
        }
    }

    public void HideLevelPanel()
    {
        if (levelPanel != null)
        {
            levelPanel.SetActive(false);
        }
        else
        {
            Debug.Log("Level panel is null");
        }
    }

    private void OnBuyButtonClicked()
    {
        if (_currentPlanet != null)
        {
            _currentPlanet.BuyLevel();
        }
        else
        {
            Debug.Log("Current planet is null");
        }
    }

    private void OnPlayButtonClicked()
    {
        string sceneName = $"Level {_currentLevelNumber}";
        SceneManager.LoadScene(sceneName);
    }
}