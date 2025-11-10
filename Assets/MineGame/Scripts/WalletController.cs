using UnityEngine;
using System;

public class WalletController : MonoBehaviour
{
    public static WalletController Instance;

    public event Action<int, int> OnMoneyChanged;

    public int Money
    {
        get => _money;

        set
        {
            int previousMoney = _money;
            _money = value;
            PlayerPrefs.SetInt("money", _money);
            PlayerPrefs.Save();
            
            OnMoneyChanged?.Invoke(previousMoney, _money);
        }
    }
    
    private int _money;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple WalletController instances found");
        }
        
        _money = PlayerPrefs.GetInt("money", 100);
    }

    [ContextMenu("Add Money")]
    public void AddMoney()
    {
        Money += 5000;
    }
}