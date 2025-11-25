using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeController : MonoBehaviour
{
    public static UpgradeController Instance;

    [SerializeField] private Button healthButton;
    [SerializeField] private Button speedButton;
    [SerializeField] private Button robotButton;
    
    [SerializeField] private TextMeshProUGUI healthButtonText;
    [SerializeField] private TextMeshProUGUI speedButtonText;
    [SerializeField] private TextMeshProUGUI robotButtonText;
    
    [SerializeField] private GameObject purchasePanel;
    
    private const int UPGRADE_COST = 500;
    
    private bool _healthPurchased;
    private bool _speedPurchased;
    private bool _robotPurchased;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple UpgradeController instances found");
        }
        
        LoadPurchases();
        UpdateButtonStates();
    }

    private void OnEnable()
    {
        healthButton.onClick.AddListener(() => TryPurchase("Health"));
        speedButton.onClick.AddListener(() => TryPurchase("Speed"));
        robotButton.onClick.AddListener(() => TryPurchase("Robot"));
    }

    private void OnDisable()
    {
        healthButton.onClick.RemoveAllListeners();
        speedButton.onClick.RemoveAllListeners();
        robotButton.onClick.RemoveAllListeners();
    }

    private void LoadPurchases()
    {
        _healthPurchased = PlayerPrefs.GetInt("healthPurchased", 0) == 1;
        _speedPurchased = PlayerPrefs.GetInt("speedPurchased", 0) == 1;
        _robotPurchased = PlayerPrefs.GetInt("robotPurchased", 0) == 1;
    }

    private void UpdateButtonStates()
    {
        if (_healthPurchased)
        {
            healthButtonText.text = "PURCHASED";
            healthButton.interactable = false;
        }
        
        if (_speedPurchased)
        {
            speedButtonText.text = "PURCHASED";
            speedButton.interactable = false;
        }
        
        if (_robotPurchased)
        {
            robotButtonText.text = "PURCHASED";
            robotButton.interactable = false;
        }
    }

    private void TryPurchase(string upgradeType)
    {
        if (WalletController.Instance.Money < UPGRADE_COST)
        {
            Debug.Log("Not enough money");
            return;
        }

        switch (upgradeType)
        {
            case "Health":
                if (!_healthPurchased)
                {
                    CompletePurchase("Health");
                    _healthPurchased = true;
                    PlayerPrefs.SetInt("healthPurchased", 1);
                    healthButtonText.text = "PURCHASED";
                    healthButton.interactable = false;
                }
                break;
                
            case "Speed":
                if (!_speedPurchased)
                {
                    CompletePurchase("Speed");
                    _speedPurchased = true;
                    PlayerPrefs.SetInt("speedPurchased", 1);
                    speedButtonText.text = "PURCHASED";
                    speedButton.interactable = false;
                }
                break;
                
            case "Robot":
                if (!_robotPurchased)
                {
                    CompletePurchase("Robot");
                    _robotPurchased = true;
                    PlayerPrefs.SetInt("robotPurchased", 1);
                    robotButtonText.text = "PURCHASED";
                    robotButton.interactable = false;
                }
                break;
        }
        
        PlayerPrefs.Save();
    }

    private void CompletePurchase(string upgradeName)
    {
        WalletController.Instance.Money -= UPGRADE_COST;
        ShowPurchasePanel();
        Debug.Log($"{upgradeName} purchased!");
    }

    private void ShowPurchasePanel()
    {
        if (purchasePanel != null)
        {
            purchasePanel.SetActive(true);
        }
    }

    public bool IsHealthPurchased() => _healthPurchased;
    public bool IsSpeedPurchased() => _speedPurchased;
    public bool IsRobotPurchased() => _robotPurchased;
}