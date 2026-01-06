using UnityEngine;

public class FloatingRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 30f;
    
    [Header("Vertical Movement")]
    [SerializeField] private float verticalAmplitude = 0.5f;
    [SerializeField] private float verticalSpeed = 1f;
    
    [Header("Horizontal Movement")]
    [SerializeField] private float horizontalAmplitude = 0.3f;
    [SerializeField] private float horizontalSpeed = 0.8f;
    
    private Vector3 startPosition;
    private float timeOffset;

    private void Start()
    {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        float verticalOffset = Mathf.Sin((Time.time + timeOffset) * verticalSpeed) * verticalAmplitude;
        float horizontalOffset = Mathf.Sin((Time.time + timeOffset) * horizontalSpeed) * horizontalAmplitude;
        
        transform.position = startPosition + new Vector3(horizontalOffset, verticalOffset, 0f);
    }
}