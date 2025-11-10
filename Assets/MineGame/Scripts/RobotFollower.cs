using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RobotFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] wheels;
    
    [Header("Follow Settings")]
    [SerializeField] private float followDistance = 3f;
    [SerializeField] private float stopDistance = 2f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float miningSpeed = 6f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float deceleration = 8f;
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    
    [Header("Wheel Settings")]
    [SerializeField] private float wheelRotationSpeed = 360f;
    [SerializeField] private Vector3 wheelRotationAxis = Vector3.right;
    
    [Header("Physics")]
    [SerializeField] private float drag = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    
    private Rigidbody _rb;
    private Vector3 _currentVelocity;
    private Quaternion _targetRotation;
    private Transform _miningTarget;
    private bool _isMining;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
        if (_rb == null)
        {
            Debug.LogError("Rigidbody is null on robot");
        }
        else
        {
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.drag = drag;
        }
        
        if (player == null)
        {
            Debug.LogError("Player transform is null");
        }
        
        if (wheels == null || wheels.Length == 0)
        {
            Debug.LogError("Wheels array is null or empty");
        }
        
        _targetRotation = transform.rotation;
    }
    
    private void FixedUpdate()
    {
        if (player == null)
        {
            Debug.LogError("Player is null in FixedUpdate");
            return;
        }
        
        if (_rb == null)
        {
            Debug.LogError("Rigidbody is null in FixedUpdate");
            return;
        }
        
        if (_isMining && _miningTarget != null)
        {
            FollowMiningTarget();
        }
        else
        {
            FollowPlayer();
        }
        
        UpdateRotation();
        UpdateWheelRotation();
    }
    
    public void SetMiningTarget(Transform target)
    {
        _miningTarget = target;
        _isMining = true;
    }
    
    public void ClearMiningTarget()
    {
        _miningTarget = null;
        _isMining = false;
    }
    
    private void FollowMiningTarget()
    {
        if (_miningTarget == null)
        {
            Debug.LogError("Mining target is null in FollowMiningTarget");
            return;
        }
        
        Vector3 directionToTarget = _miningTarget.position - transform.position;
        directionToTarget.y = 0f;
        float distanceToTarget = directionToTarget.magnitude;
        
        Vector3 targetVelocity = Vector3.zero;
        
        float targetDistance = 3f;
        
        if (distanceToTarget > targetDistance + 0.2f)
        {
            Vector3 moveDirection = directionToTarget.normalized;
            targetVelocity = moveDirection * miningSpeed;
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
    
    private void UpdateWheelRotation()
    {
        if (wheels == null || wheels.Length == 0)
        {
            return;
        }
        
        float currentSpeed = _currentVelocity.magnitude;
        
        if (currentSpeed > 0.1f)
        {
            float rotationAmount = wheelRotationSpeed * currentSpeed * Time.fixedDeltaTime;
            
            foreach (Transform wheel in wheels)
            {
                if (wheel == null)
                {
                    Debug.LogError("One of the wheels is null");
                    continue;
                }
                
                wheel.Rotate(wheelRotationAxis, rotationAmount, Space.Self);
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos)
        {
            return;
        }
        
        if (player == null)
        {
            return;
        }
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, followDistance);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, player.position);
    }
}