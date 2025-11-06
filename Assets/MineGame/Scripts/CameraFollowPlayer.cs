using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    
    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float positionDamping = 0.1f;
    
    private Vector3 _offset;
    private Vector3 _velocity;
    
    private void Start()
    {
        if (player != null)
        {
            _offset = transform.position - player.position;
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
            Vector3 targetPosition = new Vector3(
                player.position.x + _offset.x,
                transform.position.y,
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