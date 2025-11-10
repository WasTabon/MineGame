using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RobotFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    
    [Header("Follow Settings")]
    [SerializeField] private float followDistance = 3f;
    [SerializeField] private float stopDistance = 2f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float deceleration = 8f;
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    
    [Header("Physics")]
    [SerializeField] private float drag = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    
    private Rigidbody _rb;
    private Vector3 _currentVelocity;
    private Quaternion _targetRotation;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
        if (_rb != null)
        {
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.drag = drag;
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else
        {
            Debug.LogError("Rigidbody is null on robot");
        }
        
        if (player == null)
        {
            Debug.LogError("Player transform is null");
        }
        
        _targetRotation = transform.rotation;
    }
    
    private void FixedUpdate()
    {
        if (player != null && _rb != null)
        {
            FollowPlayer();
            UpdateRotation();
        }
        else
        {
            if (player == null)
            {
                Debug.LogError("Player is null in FixedUpdate");
            }
            else
            {
                Debug.LogError("Rigidbody is null in FixedUpdate");
            }
        }
    }
    
    private void FollowPlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0f;
        float distanceToPlayer = directionToPlayer.magnitude;
        
        Vector3 targetVelocity = Vector3.zero;
        
        if (distanceToPlayer > followDistance)
        {
            Vector3 moveDirection = directionToPlayer.normalized;
            targetVelocity = moveDirection * moveSpeed;
        }
        else if (distanceToPlayer < stopDistance)
        {
            targetVelocity = Vector3.zero;
        }
        else
        {
            targetVelocity = Vector3.zero;
        }
        
        float currentAcceleration = targetVelocity.magnitude > 0.1f ? acceleration : deceleration;
        
        _currentVelocity = Vector3.Lerp(_currentVelocity, targetVelocity, currentAcceleration * Time.fixedDeltaTime);
        
        Vector3 currentRbVelocity = _rb.velocity;
        _rb.velocity = new Vector3(_currentVelocity.x, currentRbVelocity.y, _currentVelocity.z);
    }
    
    private void UpdateRotation()
    {
        if (_currentVelocity.magnitude > 0.1f)
        {
            _targetRotation = Quaternion.LookRotation(_currentVelocity.normalized);
        }
        
        _rb.rotation = Quaternion.Slerp(_rb.rotation, _targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos || player == null)
        {
            return;
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, followDistance);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, stopDistance);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}