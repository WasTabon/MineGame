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
    
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    
    private Animator _animator;
    
    private void Start()
    {
        _animator = GetComponent<Animator>();
        
        if (_animator == null)
        {
            Debug.Log("Animator is null");
        }
    }
    
    private void OnAnimatorIK(int layerIndex)
    {
        if (_animator != null)
        {
            if (enableLeftFootIK)
            {
                SetFootIK(AvatarIKGoal.LeftFoot, HumanBodyBones.LeftFoot);
            }
            else
            {
                _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
                _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);
            }
            
            if (enableRightFootIK)
            {
                SetFootIK(AvatarIKGoal.RightFoot, HumanBodyBones.RightFoot);
            }
            else
            {
                _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
                _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);
            }
        }
        else
        {
            Debug.Log("Animator is null in OnAnimatorIK");
        }
    }
    
    private void SetFootIK(AvatarIKGoal foot, HumanBodyBones footBone)
    {
        Transform footTransform = _animator.GetBoneTransform(footBone);
        
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
            Debug.Log($"Foot transform is null for {footBone}");
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
            Debug.Log($"Foot transform is null for {footBone} in DrawFootGizmo");
        }
    }
}