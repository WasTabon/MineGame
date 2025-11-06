using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float dragSpeed = 0.5f;
    [SerializeField] private float inertiaDecay = 0.95f;
    
    [Header("Boundaries")]
    [SerializeField] private bool useBoundaries = true;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -10f;
    [SerializeField] private float maxY = 10f;
    
    private Vector3 _velocity;
    private bool _isDragging;
    private Vector3 _lastMousePosition;
    
    private void Update()
    {
        HandleInput();
        ApplyInertia();
        ClampPosition();
    }
    
    private void HandleInput()
    {
        if (Input.touchCount > 0)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }
    }
    
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _lastMousePosition = Input.mousePosition;
            _isDragging = true;
            _velocity = Vector3.zero;
        }
        else if (Input.GetMouseButton(0) && _isDragging)
        {
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 difference = currentMousePosition - _lastMousePosition;
            
            Vector3 move = new Vector3(-difference.x * dragSpeed * Time.deltaTime, -difference.y * dragSpeed * Time.deltaTime, 0);
            
            _velocity = move;
            transform.position += move;
            
            _lastMousePosition = currentMousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
        }
        else
        {
            _isDragging = false;
        }
    }
    
    private void HandleTouchInput()
    {
        Touch touch = Input.GetTouch(0);
        
        if (touch.phase == TouchPhase.Began)
        {
            _lastMousePosition = touch.position;
            _isDragging = true;
            _velocity = Vector3.zero;
        }
        else if (touch.phase == TouchPhase.Moved && _isDragging)
        {
            Vector3 currentTouchPosition = touch.position;
            Vector3 difference = currentTouchPosition - _lastMousePosition;
            
            Vector3 move = new Vector3(-difference.x * dragSpeed * Time.deltaTime, -difference.y * dragSpeed * Time.deltaTime, 0);
            
            _velocity = move;
            transform.position += move;
            
            _lastMousePosition = currentTouchPosition;
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            _isDragging = false;
        }
    }
    
    private void ApplyInertia()
    {
        if (!_isDragging && _velocity.magnitude > 0.001f)
        {
            transform.position += _velocity;
            _velocity *= inertiaDecay;
        }
        else if (!_isDragging)
        {
            _velocity = Vector3.zero;
        }
    }
    
    private void ClampPosition()
    {
        if (useBoundaries)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
            
            if (pos.x == minX || pos.x == maxX || pos.y == minY || pos.y == maxY)
            {
                _velocity = Vector3.zero;
            }
        }
    }
}