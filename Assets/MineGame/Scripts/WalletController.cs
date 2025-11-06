using UnityEngine;

public class WalletController : MonoBehaviour
{
    public static WalletController Instance;

    public int Money
    {
        get => _money;

        set
        {
            _money = value;
            PlayerPrefs.SetInt("money", _money);
            PlayerPrefs.Save();
        }
    }
    
    private int _money;
    
    private void Awake()
    {
        Instance = this;
        
        _money = PlayerPrefs.GetInt("money", 100);
    }

    [ContextMenu("Add Money")]
    public void AddMoney()
    {
        Money += 5000;
    }
}
