using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance;
     
    private void Awake()
    {
        Instance = this;
    }
    
    
}
