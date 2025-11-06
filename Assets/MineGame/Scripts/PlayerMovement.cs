using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Joystick joystick;
    [SerializeField] private Animator animator;
    
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("Stop Settings")]
    [SerializeField] private float stopSpeedThreshold = 0.3f;
    [SerializeField] private float stopInertiaDuration = 0.07f;
    [SerializeField] private float stopInertiaDistance = 0.5f;
    
    [Header("Physics")]
    [SerializeField] private float drag = 5f;
    
    private Rigidbody _rb;
    private Vector3 _currentVelocity;
    private bool _isMoving;
    private bool _isStopping;
    private float _stopTimer;
    private Vector3 _stopInertiaDirection;
    private bool _canMove;
    private bool _wasJoystickActive;
    
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
            Debug.LogError("Rigidbody is null");
        }
        
        if (joystick == null)
        {
            Debug.LogError("Joystick is null");
        }
        
        if (animator == null)
        {
            Debug.LogError("Animator is null");
        }
        
        _canMove = true;
        _wasJoystickActive = false;
    }
    
    private void FixedUpdate()
    {
        CheckCurrentAnimationState();
        
        if (_isStopping)
        {
            HandleStopInertia();
        }
        else if (_canMove)
        {
            HandleMovement();
            HandleRotation();
        }
        else
        {
            if (_rb != null)
            {
                _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
            }
            else
            {
                Debug.LogError("Rigidbody is null when stopping movement");
            }
        }
    }
    
    private void Update()
    {
        UpdateAnimation();
    }
    
    private void CheckCurrentAnimationState()
    {
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            _canMove = stateInfo.IsName("Blend Tree");
        }
        else
        {
            Debug.LogError("Animator is null in CheckCurrentAnimationState");
        }
    }
    
    private bool IsJoystickActive()
    {
        if (joystick != null)
        {
            return Input.touchCount > 0 || Input.GetMouseButton(0);
        }
        else
        {
            Debug.LogError("Joystick is null in IsJoystickActive");
            return false;
        }
    }
    
    private void HandleMovement()
    {
        if (joystick != null && _rb != null)
        {
            float horizontal = joystick.Horizontal;
            float vertical = joystick.Vertical;
            
            Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
            float inputMagnitude = new Vector2(horizontal, vertical).magnitude;
            inputMagnitude = Mathf.Clamp01(inputMagnitude);
            
            float currentMaxSpeed = Mathf.Lerp(walkSpeed, runSpeed, inputMagnitude);
            
            Vector3 targetVelocity = moveDirection * currentMaxSpeed;
            
            float currentAcceleration = moveDirection.magnitude > 0.1f ? acceleration : deceleration;
            
            _currentVelocity = Vector3.Lerp(_currentVelocity, targetVelocity, currentAcceleration * Time.fixedDeltaTime);
            
            Vector3 newVelocity = new Vector3(_currentVelocity.x, _rb.velocity.y, _currentVelocity.z);
            _rb.velocity = newVelocity;
        }
        else
        {
            if (joystick == null)
            {
                Debug.LogError("Joystick is null in HandleMovement");
            }
            else
            {
                Debug.LogError("Rigidbody is null in HandleMovement");
            }
        }
    }
    
    private void HandleStopInertia()
    {
        if (_rb != null)
        {
            _stopTimer += Time.fixedDeltaTime;
            
            float progress = _stopTimer / stopInertiaDuration;
            
            if (progress < 1f)
            {
                float speedMultiplier = 1f - progress;
                float inertiaSpeed = (stopInertiaDistance / stopInertiaDuration) * speedMultiplier;
                
                Vector3 inertiaVelocity = _stopInertiaDirection * inertiaSpeed;
                Vector3 newVelocity = new Vector3(inertiaVelocity.x, _rb.velocity.y, inertiaVelocity.z);
                _rb.velocity = newVelocity;
            }
            else
            {
                _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
                _currentVelocity = Vector3.zero;
                _isStopping = false;
                
                if (animator != null)
                {
                    animator.SetBool("IsStopping", false);
                }
                else
                {
                    Debug.LogError("Animator is null when finishing stop");
                }
            }
        }
        else
        {
            Debug.LogError("Rigidbody is null in HandleStopInertia");
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
                Debug.LogError("Joystick is null in HandleRotation");
            }
            else
            {
                Debug.LogError("Rigidbody is null in HandleRotation");
            }
        }
    }
    
    private void UpdateAnimation()
    {
        if (animator != null && joystick != null)
        {
            bool isJoystickActive = IsJoystickActive();
            
            float horizontal = joystick.Horizontal;
            float vertical = joystick.Vertical;
            
            float inputMagnitude = new Vector2(horizontal, vertical).magnitude;
            inputMagnitude = Mathf.Clamp01(inputMagnitude);
            
            bool wasMoving = _isMoving;
            _isMoving = inputMagnitude > 0.1f;
            
            if (_isStopping)
            {
                animator.SetFloat("Speed", 0f);
                
                if (_isMoving && _canMove)
                {
                    _isStopping = false;
                    _stopTimer = 0f;
                    animator.SetBool("IsStopping", false);
                }
            }
            else
            {
                animator.SetFloat("Speed", inputMagnitude);
                
                if (_wasJoystickActive && !isJoystickActive)
                {
                    float currentSpeed = _currentVelocity.magnitude;
                    
                    if (currentSpeed > stopSpeedThreshold)
                    {
                        _isStopping = true;
                        _stopTimer = 0f;
                        
                        if (_currentVelocity.magnitude > 0.1f)
                        {
                            _stopInertiaDirection = _currentVelocity.normalized;
                        }
                        else
                        {
                            _stopInertiaDirection = transform.forward;
                        }
                        
                        animator.SetBool("IsStopping", true);
                        animator.SetFloat("Speed", 0f);
                    }
                }
            }
            
            _wasJoystickActive = isJoystickActive;
        }
        else
        {
            if (animator == null)
            {
                Debug.LogError("Animator is null in UpdateAnimation");
            }
            else
            {
                Debug.LogError("Joystick is null in UpdateAnimation");
            }
        }
    }
}