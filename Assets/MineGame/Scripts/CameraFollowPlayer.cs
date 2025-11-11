using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    
    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float positionDamping = 0.1f;
    [SerializeField] private float yDamping = 0.3f;
    [SerializeField] private float yChangeThreshold = 2f;
    
    private Vector3 _offset;
    private Vector3 _velocity;
    private float _lastSignificantPlayerY;
    
    private void Start()
    {
        if (player != null)
        {
            _offset = transform.position - player.position;
            _lastSignificantPlayerY = player.position.y;
        }
        else
        {
            Debug.Log("Player is null");
        }
    }
    
    private void LateUpdate()
    {
        FollowPlayer();
    }
    
    private void FollowPlayer()
    {
        if (player != null)
        {
            float playerYChange = Mathf.Abs(player.position.y - _lastSignificantPlayerY);
            
            if (playerYChange >= yChangeThreshold)
            {
                _lastSignificantPlayerY = player.position.y;
            }
            
            Vector3 targetPosition = new Vector3(
                player.position.x + _offset.x,
                _lastSignificantPlayerY + _offset.y,
                player.position.z + _offset.z
            );
            
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref _velocity,
                positionDamping,
                smoothSpeed
            );
        }
        else
        {
            Debug.Log("Player is null in FollowPlayer");
        }
    }
}