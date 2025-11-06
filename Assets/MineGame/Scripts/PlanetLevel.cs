using UnityEngine;

public class PlanetLevel : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int levelNumber;
    [SerializeField] private bool isBoughtByDefault = false;
    
    private bool _isBought;
    private string SaveKey => $"planet_level_{levelNumber}_bought";

    private void Start()
    {
        LoadPurchaseState();
    }

    private void LoadPurchaseState()
    {
        if (isBoughtByDefault)
        {
            _isBought = true;
        }
        else
        {
            _isBought = PlayerPrefs.GetInt(SaveKey, 0) == 1;
        }
    }

    private void OnMouseDown()
    {
        HandleClick();
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        HandleClick();
                    }
                }
                else
                {
                    Debug.Log("Raycast didn't hit anything");
                }
            }
        }
    }

    private void HandleClick()
    {
        if (_isBought)
        {
            UIController.Instance.ShowLevelPanel(levelNumber);
        }
        else
        {
            UIController.Instance.ShowBuyLevelPanel(this);
        }
    }

    public void BuyLevel()
    {
        if (WalletController.Instance.Money >= 1000)
        {
            WalletController.Instance.Money -= 1000;
            _isBought = true;
            PlayerPrefs.SetInt(SaveKey, 1);
            PlayerPrefs.Save();
            
            UIController.Instance.HideBuyLevelPanel();
            UIController.Instance.ShowLevelPanel(levelNumber);
        }
        else
        {
            Debug.Log("Not enough money to buy this level");
        }
    }

    public int GetLevelNumber()
    {
        return levelNumber;
    }
}