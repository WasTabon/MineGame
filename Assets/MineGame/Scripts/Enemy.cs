using UnityEngine;
using DG.Tweening;
using System.Collections;
using Unity.Mathematics;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Enemy : MonoBehaviour
{
    public GameObject _particle;
    public AudioClip roarSound;
    public AudioClip hitSound;
    
    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waypointReachDistance = 0.5f;
    
    [Header("Chase")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private BoxCollider detectionTrigger;
    
    [Header("Attack")]
    [SerializeField] private float attackDistance = 2f;
    [SerializeField] private float attackPositionDistance = 2.3f;
    [SerializeField] private float playerRotationDuration = 0.1f;
    [SerializeField] private float cameraMoveDuration = 0.15f;
    
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CameraFollowPlayer cameraFollow;
    [SerializeField] private GameObject[] canvasObjectsToDisable;
    [SerializeField] private GameObject deathPanel;
    
    [Header("Audio & Effects")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float cameraShakeIntensity = 0.1f;
    [SerializeField] private float cameraShakeDuration = 0.1f;
    
    [Header("Gizmos")]
    [SerializeField] private Color pathColor = Color.green;
    [SerializeField] private Color pathBlockedColor = Color.red;
    [SerializeField] private Color attackRangeColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color detectionRangeColor = new Color(1f, 1f, 0f, 0.3f);
    
    private Rigidbody _rb;
    private BoxCollider _boxCollider;
    private Vector3 _currentVelocity;
    
    private enum State { Patrol, Chase, Attack, Attacking }
    private State _currentState = State.Patrol;
    
    private int _currentPatrolIndex = 0;
    private bool _playerInRange = false;
    private Vector3 _originalCameraPosition;
    private Quaternion _originalCameraRotation;
    
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();
        
        if (_rb != null)
        {
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
            _rb.drag = 0f;
        }
        else
        {
            Debug.LogError("Rigidbody is null");
        }
        
        if (_boxCollider == null)
        {
            Debug.LogError("BoxCollider is null");
        }
        
        if (detectionTrigger != null)
        {
            detectionTrigger.isTrigger = true;
        }
        else
        {
            Debug.LogError("Detection trigger is null");
        }

        if (!detectionTrigger.isTrigger)
        {
            Debug.LogError("Trigger is not trigger");
        }
        
        if (player == null)
        {
            Debug.LogError("Player is null");
        }
        
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement is null");
        }
        
        if (playerRb == null)
        {
            Debug.LogError("Player Rigidbody is null");
        }
        
        if (enemyAnimator == null)
        {
            Debug.LogError("Enemy Animator is null");
        }
        
        if (playerAnimator == null)
        {
            Debug.LogError("Player Animator is null");
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is null");
        }
        
        if (cameraFollow == null)
        {
            Debug.LogError("CameraFollowPlayer is null");
        }
        
        if (deathPanel == null)
        {
            Debug.LogError("Death Panel is null");
        }
        else
        {
            deathPanel.SetActive(false);
        }
        
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is null");
        }
        
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogError("Patrol points are not set");
        }
        
        if (mainCamera != null)
        {
            _originalCameraPosition = mainCamera.transform.position;
            _originalCameraRotation = mainCamera.transform.rotation;
        }
        else
        {
            Debug.LogError("Cannot save camera position, camera is null");
        }
    }
    
    void FixedUpdate()
    {
        switch (_currentState)
        {
            case State.Patrol:
                PatrolBehavior();
                CheckForPlayer();
                break;
            case State.Chase:
                ChaseBehavior();
                CheckAttackDistance();
                break;
            case State.Attack:
                break;
            case State.Attacking:
                break;
        }
    }
    
    void PatrolBehavior()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogError("No patrol points");
            return;
        }
        
        Transform targetPoint = patrolPoints[_currentPatrolIndex];
        
        if (targetPoint == null)
        {
            Debug.LogError($"Patrol point at index {_currentPatrolIndex} is null");
            return;
        }
        
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        direction.y = 0;
        
        MoveTowards(direction, patrolSpeed);
        RotateTowards(direction);
        
        float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), 
                                         new Vector3(targetPoint.position.x, 0, targetPoint.position.z));
        
        if (distance < waypointReachDistance)
        {
            _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }
    
    void CheckForPlayer()
    {
        if (_playerInRange && player != null)
        {
            _currentState = State.Chase;
            
            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool("Run", true);
            }
            else
            {
                Debug.LogError("Enemy animator is null when starting chase");
            }
        }
    }
    
    void ChaseBehavior()
    {
        if (player == null)
        {
            Debug.LogError("Player is null in ChaseBehavior");
            return;
        }
        
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        
        MoveTowards(direction, chaseSpeed);
        RotateTowards(direction);
    }
    
    void CheckAttackDistance()
    {
        if (player == null)
        {
            Debug.LogError("Player is null in CheckAttackDistance");
            return;
        }
        
        float distance = Vector3.Distance(transform.position, player.position);
        
        if (distance <= attackDistance)
        {
            StartAttack();
        }
    }
    
    void StartAttack()
    {
        _currentState = State.Attack;
        
        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
        }
        else
        {
            Debug.LogError("Rigidbody is null when starting attack");
        }
        
        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool("Run", false);
        }
        else
        {
            Debug.LogError("Enemy animator is null when stopping run");
        }
        
        foreach (GameObject obj in canvasObjectsToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
            else
            {
                Debug.LogError("Canvas object to disable is null");
            }
        }
        
        if (cameraFollow != null)
        {
            cameraFollow.enabled = false;
        }
        else
        {
            Debug.LogError("CameraFollowPlayer is null when disabling");
        }
        
        StartCoroutine(AttackSequence());
    }
    
    IEnumerator AttackSequence()
{
    if (player == null || playerRb == null || playerMovement == null)
    {
        Debug.LogError("Player references are null in AttackSequence");
        yield break;
    }
    
    playerMovement.enabled = false;
    playerRb.velocity = Vector3.zero;
    playerRb.isKinematic = true;
    
    Vector3 attackPosition = transform.position + transform.forward * attackPositionDistance;
    attackPosition.y = player.position.y;
    
    Vector3 directionToEnemy = (transform.position - player.position).normalized;
    Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);
    
    player.DOMove(attackPosition, playerRotationDuration).SetEase(Ease.OutQuad);
    player.DORotateQuaternion(targetRotation, playerRotationDuration).SetEase(Ease.OutQuad);
    
    yield return new WaitForSeconds(playerRotationDuration);
    
    if (mainCamera != null && player != null)
{
    Vector3 midPoint = (transform.position + player.position) / 2f;
    
    Vector3 sideDirection = Vector3.Cross(Vector3.up, (transform.position - player.position).normalized);
    
    float distanceBetween = Vector3.Distance(transform.position, player.position);
    
    Collider enemyCollider = GetComponent<Collider>();
    Collider playerCollider = player.GetComponent<Collider>();
    
    float maxHeight = 2f;
    
    if (enemyCollider != null && playerCollider != null)
    {
        float enemyHeight = enemyCollider.bounds.size.y;
        float playerHeight = playerCollider.bounds.size.y;
        maxHeight = Mathf.Max(enemyHeight, playerHeight);
    }
    else
    {
        Debug.LogError("Enemy or Player collider is null when calculating height");
    }
    
    float verticalFOV = mainCamera.fieldOfView * Mathf.Deg2Rad;
    
    float totalWidth = distanceBetween * 2f;
    
    float requiredDistanceForWidth = totalWidth / (2f * Mathf.Tan(verticalFOV * mainCamera.aspect / 2f));
    
    float requiredDistanceForHeight = (maxHeight * 1.5f) / (2f * Mathf.Tan(verticalFOV / 2f));
    
    float cameraDistance = Mathf.Max(requiredDistanceForWidth, requiredDistanceForHeight, distanceBetween * 2f);
    
    Vector3 cameraPosition = midPoint + sideDirection * cameraDistance;
    
    float groundY = Mathf.Min(transform.position.y, player.position.y);
    cameraPosition.y = groundY + (maxHeight * 0.6f);
    
    Vector3 lookAtPoint = midPoint;
    lookAtPoint.y = groundY + (maxHeight * 0.5f);
    
    Quaternion cameraRotation = Quaternion.LookRotation(lookAtPoint - cameraPosition);
    
    mainCamera.transform.DOMove(cameraPosition, cameraMoveDuration).SetEase(Ease.OutQuad);
    mainCamera.transform.DORotateQuaternion(cameraRotation, cameraMoveDuration).SetEase(Ease.OutQuad);
}
else
{
    if (mainCamera == null)
    {
        Debug.LogError("Main camera is null when moving camera");
    }
    else
    {
        Debug.LogError("Player is null when moving camera");
    }
}
    
    yield return new WaitForSeconds(cameraMoveDuration);
    
    if (playerAnimator != null)
    {
        playerAnimator.SetTrigger("Scared");
    }
    else
    {
        Debug.LogError("Player animator is null when triggering Scared");
    }
    
    if (enemyAnimator != null)
    {
        enemyAnimator.SetTrigger("Attack");
    }
    else
    {
        Debug.LogError("Enemy animator is null when triggering Attack");
    }
    
    _currentState = State.Attacking;
}

    public void PlayRoarSound()
    {
        MusicController.Instance.PlaySpecificSound(roarSound);
    }
    public void PlayHitSound()
    {
        MusicController.Instance.PlaySpecificSound(hitSound);
    }
    
    public void OnAttackAnimationEvent()
    {
        StartCoroutine(KillPlayerSequence());
    }
    
    IEnumerator KillPlayerSequence()
    {
        if (playerMovement != null)
        {
            Instantiate(_particle, playerMovement.gameObject.transform.position, quaternion.identity);
            playerMovement.Die();
        }
        else
        {
            Debug.LogError("PlayerMovement is null when calling Die");
        }
        
        yield return new WaitForSeconds(5f);
        
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            deathPanel.transform.localScale = Vector3.zero;
            deathPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
        else
        {
            Debug.LogError("Death panel is null when showing");
        }
    }
    
    public void PlayFootstepSound()
    {
        if (footstepSounds == null || footstepSounds.Length == 0)
        {
            Debug.LogError("No footstep sounds assigned");
            return;
        }
        
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is null when playing footstep");
            return;
        }
        
        AudioClip randomClip = footstepSounds[Random.Range(0, footstepSounds.Length)];
        
        if (randomClip != null)
        {
            audioSource.PlayOneShot(randomClip);
        }
        else
        {
            Debug.LogError("Random footstep clip is null");
        }
        
        if (mainCamera != null)
        {
            mainCamera.transform.DOShakePosition(cameraShakeDuration, cameraShakeIntensity, 10, 90f);
        }
        else
        {
            Debug.LogError("Main camera is null when shaking");
        }
    }
    
    void MoveTowards(Vector3 direction, float speed)
    {
        if (_rb == null)
        {
            Debug.LogError("Rigidbody is null in MoveTowards");
            return;
        }
        
        Vector3 targetVelocity = direction * speed;
        Vector3 newVelocity = new Vector3(targetVelocity.x, _rb.velocity.y, targetVelocity.z);
        _rb.velocity = newVelocity;
    }
    
    void RotateTowards(Vector3 direction)
    {
        if (direction.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
        }
    }
    
    void OnDrawGizmos()
    {
        DrawPatrolPath();
        DrawAttackRange();
        DrawDetectionRange();
    }
    
    void DrawPatrolPath()
    {
        if (patrolPoints == null || patrolPoints.Length < 2)
        {
            return;
        }
        
        BoxCollider col = GetComponent<BoxCollider>();
        
        if (col == null)
        {
            return;
        }
        
        float pathWidth = Mathf.Max(col.size.x * transform.localScale.x, col.size.z * transform.localScale.z);
        
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
            {
                continue;
            }
            
            Transform current = patrolPoints[i];
            Transform next = patrolPoints[(i + 1) % patrolPoints.Length];
            
            if (next == null)
            {
                continue;
            }
            
            Vector3 start = current.position;
            Vector3 end = next.position;
            
            bool isBlocked = CheckPathObstruction(start, end, pathWidth);
            
            Gizmos.color = isBlocked ? pathBlockedColor : pathColor;
            
            Vector3 direction = (end - start).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
            
            Vector3 p1 = start + perpendicular * (pathWidth / 2f);
            Vector3 p2 = start - perpendicular * (pathWidth / 2f);
            Vector3 p3 = end + perpendicular * (pathWidth / 2f);
            Vector3 p4 = end - perpendicular * (pathWidth / 2f);
            
            Gizmos.DrawLine(p1, p3);
            Gizmos.DrawLine(p2, p4);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p3, p4);
            
            Gizmos.DrawLine(start, end);
            
            Gizmos.DrawWireSphere(current.position, 0.3f);
        }
    }
    
    bool CheckPathObstruction(Vector3 start, Vector3 end, float width)
    {
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        
        BoxCollider col = GetComponent<BoxCollider>();
        
        if (col == null)
        {
            return false;
        }
        
        Vector3 boxSize = new Vector3(width, col.size.y * transform.localScale.y, 0.5f);
        
        int checkCount = Mathf.CeilToInt(distance / 0.5f);
        
        for (int i = 0; i <= checkCount; i++)
        {
            float t = i / (float)checkCount;
            Vector3 checkPosition = Vector3.Lerp(start, end, t);
            
            Collider[] hits = Physics.OverlapBox(checkPosition, boxSize / 2f, Quaternion.LookRotation(direction));
            
            foreach (Collider hit in hits)
            {
                if (hit.gameObject != gameObject && !hit.isTrigger)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    void DrawAttackRange()
    {
        Gizmos.color = attackRangeColor;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
    
    void DrawDetectionRange()
    {
        if (detectionTrigger != null)
        {
            Gizmos.color = detectionRangeColor;
            Gizmos.matrix = detectionTrigger.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(detectionTrigger.center, detectionTrigger.size);
        }
    }
}