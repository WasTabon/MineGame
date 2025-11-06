using UnityEngine;

public class FootIK : MonoBehaviour
{
    [Header("IK Settings")]
    [SerializeField] private bool enableLeftFootIK = true;
    [SerializeField] private bool enableRightFootIK = true;
    [SerializeField] private float raycastDistance = 1.5f;
    [SerializeField] private float footOffset = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float ikWeight = 1f;
    
    [Header("Body Adjustment")]
    [SerializeField] private bool adjustBodyHeight = true;
    [SerializeField] private float bodyOffsetSmooth = 5f;
    
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    
    private Animator _animator;
    private float _leftFootHeight;
    private float _rightFootHeight;
    private float _bodyOffset;
    private bool _isIdle;
    
    private void Start()
    {
        _animator = GetComponent<Animator>();
        
        if (_animator == null)
        {
            Debug.LogError("Animator is null");
        }
    }
    
    private void Update()
    {
        CheckIfIdle();
    }
    
    private void CheckIfIdle()
    {
        if (_animator != null)
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            _isIdle = stateInfo.IsName("Idle");
        }
        else
        {
            Debug.LogError("Animator is null in CheckIfIdle");
        }
    }
    
    private void OnAnimatorIK(int layerIndex)
    {
        if (_animator != null)
        {
            if (_isIdle)
            {
                _leftFootHeight = 0f;
                _rightFootHeight = 0f;
                
                if (enableLeftFootIK)
                {
                    _leftFootHeight = SetFootIK(AvatarIKGoal.LeftFoot, HumanBodyBones.LeftFoot);
                }
                else
                {
                    _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
                    _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);
                }
                
                if (enableRightFootIK)
                {
                    _rightFootHeight = SetFootIK(AvatarIKGoal.RightFoot, HumanBodyBones.RightFoot);
                }
                else
                {
                    _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
                    _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);
                }
                
                if (adjustBodyHeight)
                {
                    AdjustBodyHeight();
                }
            }
            else
            {
                _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
                _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);
                _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
                _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);
            }
        }
        else
        {
            Debug.LogError("Animator is null in OnAnimatorIK");
        }
    }
    
    private float SetFootIK(AvatarIKGoal foot, HumanBodyBones footBone)
    {
        Transform footTransform = _animator.GetBoneTransform(footBone);
        float heightOffset = 0f;
        
        if (footTransform != null)
        {
            Vector3 rayStart = footTransform.position + Vector3.up * 0.5f;
            RaycastHit hit;
            
            if (Physics.Raycast(rayStart, Vector3.down, out hit, raycastDistance, groundLayer))
            {
                _animator.SetIKPositionWeight(foot, ikWeight);
                _animator.SetIKRotationWeight(foot, ikWeight);
                
                Vector3 targetPosition = hit.point;
                targetPosition.y += footOffset;
                
                heightOffset = targetPosition.y - footTransform.position.y;
                
                _animator.SetIKPosition(foot, targetPosition);
                
                Quaternion targetRotation = Quaternion.LookRotation(transform.forward, hit.normal);
                _animator.SetIKRotation(foot, targetRotation);
            }
            else
            {
                _animator.SetIKPositionWeight(foot, 0f);
                _animator.SetIKRotationWeight(foot, 0f);
            }
        }
        else
        {
            Debug.LogError($"Foot transform is null for {footBone}");
        }
        
        return heightOffset;
    }
    
    private void AdjustBodyHeight()
    {
        float targetOffset = Mathf.Min(_leftFootHeight, _rightFootHeight);
        
        if (targetOffset < 0f)
        {
            _bodyOffset = Mathf.Lerp(_bodyOffset, targetOffset, bodyOffsetSmooth * Time.deltaTime);
            
            Vector3 bodyPosition = _animator.bodyPosition;
            bodyPosition.y += _bodyOffset;
            _animator.bodyPosition = bodyPosition;
        }
        else
        {
            _bodyOffset = Mathf.Lerp(_bodyOffset, 0f, bodyOffsetSmooth * Time.deltaTime);
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos || _animator == null)
        {
            return;
        }
        else
        {
            DrawFootGizmo(HumanBodyBones.LeftFoot, Color.green);
            DrawFootGizmo(HumanBodyBones.RightFoot, Color.red);
        }
    }
    
    private void DrawFootGizmo(HumanBodyBones footBone, Color color)
    {
        Transform footTransform = _animator.GetBoneTransform(footBone);
        
        if (footTransform != null)
        {
            Vector3 rayStart = footTransform.position + Vector3.up * 0.5f;
            Vector3 rayEnd = rayStart + Vector3.down * raycastDistance;
            
            Gizmos.color = color;
            Gizmos.DrawLine(rayStart, rayEnd);
            Gizmos.DrawWireSphere(rayStart, 0.05f);
            
            RaycastHit hit;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, raycastDistance, groundLayer))
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(hit.point, 0.08f);
                
                Vector3 targetPosition = hit.point;
                targetPosition.y += footOffset;
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(targetPosition, 0.06f);
            }
            else
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(rayEnd, 0.05f);
            }
        }
        else
        {
            Debug.LogError($"Foot transform is null for {footBone} in DrawFootGizmo");
        }
    }
}