using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Joystick joystick;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("Physics")]
    [SerializeField] private float drag = 5f;
    
    private Rigidbody _rb;
    private Vector3 _currentVelocity;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
        if (_rb != null)
        {
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _rb.drag = drag;
        }
        else
        {
            Debug.Log("Rigidbody is null");
        }
        
        if (joystick == null)
        {
            Debug.Log("Joystick is null");
        }
    }
    
    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }
    
    private void HandleMovement()
    {
        if (joystick != null && _rb != null)
        {
            float horizontal = joystick.Horizontal;
            float vertical = joystick.Vertical;
            
            Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
            
            Vector3 targetVelocity = moveDirection * moveSpeed;
            
            float currentAcceleration = moveDirection.magnitude > 0.1f ? acceleration : deceleration;
            
            _currentVelocity = Vector3.Lerp(_currentVelocity, targetVelocity, currentAcceleration * Time.fixedDeltaTime);
            
            Vector3 newVelocity = new Vector3(_currentVelocity.x, _rb.velocity.y, _currentVelocity.z);
            _rb.velocity = newVelocity;
        }
        else
        {
            if (joystick == null)
            {
                Debug.Log("Joystick is null in HandleMovement");
            }
            if (_rb == null)
            {
                Debug.Log("Rigidbody is null in HandleMovement");
            }
        }
    }
    
    private void HandleRotation()
    {
        if (joystick != null && _rb != null)
        {
            float horizontal = joystick.Horizontal;
            float vertical = joystick.Vertical;
            
            if (horizontal != 0 || vertical != 0)
            {
                Vector3 lookDirection = new Vector3(horizontal, 0f, vertical);
                
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                
                _rb.rotation = Quaternion.Slerp(_rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            if (joystick == null)
            {
                Debug.Log("Joystick is null in HandleRotation");
            }
            if (_rb == null)
            {
                Debug.Log("Rigidbody is null in HandleRotation");
            }
        }
    }
}