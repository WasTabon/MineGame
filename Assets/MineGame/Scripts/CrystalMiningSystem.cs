using UnityEngine;
using DG.Tweening;
using System.Collections;

public class CrystalMiningSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private RobotFollower robot;
    [SerializeField] private Canvas miningCanvas;
    [SerializeField] private UnityEngine.UI.Image miningUIBackground;
    [SerializeField] private UnityEngine.UI.Image miningProgressRadial;
    
    [Header("Mining Settings")]
    [SerializeField] private float miningDuration = 5f;
    [SerializeField] private int crystalReward = 50;
    
    [Header("Animation Settings")]
    [SerializeField] private float uiShowDuration = 0.3f;
    [SerializeField] private float uiRotationSpeed = 2f;
    [SerializeField] private float crystalShrinkDuration = 0.5f;
    [SerializeField] private float crystalFlyDuration = 1f;
    [SerializeField] private float crystalFlyHeight = 3f;
    
    private Transform _currentCrystal;
    private Collider _currentCrystalTrigger;
    private bool _isMining;
    private float _miningTimer;
    private Coroutine _miningCoroutine;
    private Coroutine _uiRotationCoroutine;
    
    private void Start()
    {
        if (miningCanvas != null)
        {
            miningCanvas.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Mining Canvas is null");
        }
        
        if (miningUIBackground != null)
        {
            miningUIBackground.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Mining UI Background is null");
        }
        
        if (miningProgressRadial != null)
        {
            miningProgressRadial.fillAmount = 0f;
            miningProgressRadial.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Mining Progress Radial is null");
        }
        
        if (player == null)
        {
            Debug.LogError("Player is null");
        }
        
        if (robot == null)
        {
            Debug.LogError("Robot is null");
        }
    }
    
    private void Update()
    {
        if (_isMining && _currentCrystal != null && _currentCrystalTrigger != null && player != null)
        {
            if (!IsPlayerInTrigger())
            {
                StopMining();
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Crystal") && !_isMining && player != null && robot != null)
        {
            _currentCrystal = other.transform;
            _currentCrystalTrigger = other;
            StartMining();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Crystal") && other.transform == _currentCrystal)
        {
            StopMining();
        }
    }
    
    private void StartMining()
    {
        if (_currentCrystal == null || robot == null)
        {
            if (_currentCrystal == null)
            {
                Debug.LogError("Current crystal is null in StartMining");
            }
            else
            {
                Debug.LogError("Robot is null in StartMining");
            }
            return;
        }
        
        _isMining = true;
        
        Vector3 crystalPosition = _currentCrystal.position;
        Vector3 directionToCrystal = (crystalPosition - robot.transform.position).normalized;
        
        float triggerRadius = 0f;
        if (_currentCrystalTrigger is SphereCollider sphereCollider)
        {
            triggerRadius = sphereCollider.radius * _currentCrystal.localScale.x;
        }
        else if (_currentCrystalTrigger is BoxCollider boxCollider)
        {
            triggerRadius = Mathf.Max(boxCollider.size.x, boxCollider.size.z) * 0.5f * _currentCrystal.localScale.x;
        }
        else
        {
            triggerRadius = 2f;
        }
        
        float distanceToStand = triggerRadius * 0.5f;
        Vector3 targetPosition = crystalPosition - directionToCrystal * distanceToStand;
        
        robot.enabled = false;
        
        Rigidbody robotRb = robot.GetComponent<Rigidbody>();
        if (robotRb != null)
        {
            robot.transform.DOMove(targetPosition, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                if (robotRb != null)
                {
                    robotRb.velocity = Vector3.zero;
                }
                else
                {
                    Debug.LogError("Robot Rigidbody is null when stopping");
                }
                
                Vector3 lookDirection = (crystalPosition - robot.transform.position).normalized;
                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    robot.transform.DORotateQuaternion(targetRotation, 0.3f);
                }
                
                ShowMiningUI();
                _miningCoroutine = StartCoroutine(MiningProcess());
            });
        }
        else
        {
            Debug.LogError("Robot Rigidbody is null");
        }
    }
    
    private void StopMining()
    {
        _isMining = false;
        _miningTimer = 0f;
        
        if (_miningCoroutine != null)
        {
            StopCoroutine(_miningCoroutine);
            _miningCoroutine = null;
        }
        
        HideMiningUI();
        
        _currentCrystal = null;
        _currentCrystalTrigger = null;
        
        if (robot != null)
        {
            robot.enabled = true;
        }
        else
        {
            Debug.LogError("Robot is null in StopMining");
        }
    }
    
    private void ShowMiningUI()
    {
        if (miningCanvas != null && miningUIBackground != null)
        {
            miningCanvas.gameObject.SetActive(true);
            miningUIBackground.gameObject.SetActive(true);
            
            miningUIBackground.transform.localScale = Vector3.zero;
            miningUIBackground.transform.DOScale(Vector3.one, uiShowDuration).SetEase(Ease.OutBack);
            
            if (miningProgressRadial != null)
            {
                miningProgressRadial.gameObject.SetActive(true);
                miningProgressRadial.fillAmount = 0f;
            }
            else
            {
                Debug.LogError("Mining Progress Radial is null in ShowMiningUI");
            }
            
            _uiRotationCoroutine = StartCoroutine(RotateUIToPlayer());
        }
        else
        {
            if (miningCanvas == null)
            {
                Debug.LogError("Mining Canvas is null in ShowMiningUI");
            }
            else
            {
                Debug.LogError("Mining UI Background is null in ShowMiningUI");
            }
        }
    }
    
    private void HideMiningUI()
    {
        if (_uiRotationCoroutine != null)
        {
            StopCoroutine(_uiRotationCoroutine);
            _uiRotationCoroutine = null;
        }
        
        if (miningUIBackground != null)
        {
            miningUIBackground.transform.DOScale(Vector3.zero, uiShowDuration).SetEase(Ease.InBack).OnComplete(() =>
            {
                if (miningUIBackground != null)
                {
                    miningUIBackground.gameObject.SetActive(false);
                }
                else
                {
                    Debug.LogError("Mining UI Background is null in hide callback");
                }
                
                if (miningCanvas != null)
                {
                    miningCanvas.gameObject.SetActive(false);
                }
                else
                {
                    Debug.LogError("Mining Canvas is null in hide callback");
                }
            });
        }
        else
        {
            Debug.LogError("Mining UI Background is null in HideMiningUI");
        }
        
        if (miningProgressRadial != null)
        {
            miningProgressRadial.fillAmount = 0f;
            miningProgressRadial.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Mining Progress Radial is null in HideMiningUI");
        }
    }
    
    private IEnumerator RotateUIToPlayer()
    {
        while (miningUIBackground != null && miningUIBackground.gameObject.activeSelf && player != null)
        {
            Vector3 directionToPlayer = player.position - miningUIBackground.transform.position;
            directionToPlayer.y = 0f;
            
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                miningUIBackground.transform.rotation = Quaternion.Slerp(
                    miningUIBackground.transform.rotation,
                    targetRotation,
                    uiRotationSpeed * Time.deltaTime
                );
            }
            
            yield return null;
        }
    }
    
    private IEnumerator MiningProcess()
    {
        _miningTimer = 0f;
        
        while (_miningTimer < miningDuration)
        {
            _miningTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(_miningTimer / miningDuration);
            
            if (miningProgressRadial != null)
            {
                miningProgressRadial.fillAmount = progress;
            }
            else
            {
                Debug.LogError("Mining Progress Radial is null during mining");
            }
            
            yield return null;
        }
        
        CompleteMining();
    }
    
    private void CompleteMining()
    {
        HideMiningUI();
        
        if (_currentCrystal != null && robot != null)
        {
            Transform childCrystal = _currentCrystal.Find("FX_Crystal_Floating_02");
            
            Sequence shrinkSequence = DOTween.Sequence();
            
            shrinkSequence.Append(_currentCrystal.DOScale(Vector3.one * 0.1f, crystalShrinkDuration).SetEase(Ease.InBack));
            shrinkSequence.Join(_currentCrystal.DORotate(new Vector3(0f, 360f, 0f), crystalShrinkDuration, RotateMode.FastBeyond360));
            
            if (childCrystal != null)
            {
                shrinkSequence.Join(childCrystal.DOScale(Vector3.one * 0.1f, crystalShrinkDuration).SetEase(Ease.InBack));
            }
            else
            {
                Debug.LogError("Child crystal FX_Crystal_Floating_02 not found");
            }
            
            ParticleSystem[] particles = _currentCrystal.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles)
            {
                var main = ps.main;
                DOTween.To(() => main.startSize.constant, x => 
                {
                    var mainModule = ps.main;
                    var startSize = mainModule.startSize;
                    startSize.constant = x;
                    mainModule.startSize = startSize;
                }, 0f, crystalShrinkDuration);
            }
            
            shrinkSequence.OnComplete(() =>
            {
                if (_currentCrystal != null && robot != null)
                {
                    FlyToRobot();
                }
                else
                {
                    if (_currentCrystal == null)
                    {
                        Debug.LogError("Current crystal is null after shrink");
                    }
                    else
                    {
                        Debug.LogError("Robot is null after shrink");
                    }
                }
            });
        }
        else
        {
            if (_currentCrystal == null)
            {
                Debug.LogError("Current crystal is null in CompleteMining");
            }
            else
            {
                Debug.LogError("Robot is null in CompleteMining");
            }
        }
    }
    
    private void FlyToRobot()
    {
        if (_currentCrystal == null || robot == null)
        {
            if (_currentCrystal == null)
            {
                Debug.LogError("Current crystal is null in FlyToRobot");
            }
            else
            {
                Debug.LogError("Robot is null in FlyToRobot");
            }
            return;
        }
        
        Vector3 startPos = _currentCrystal.position;
        Vector3 endPos = robot.transform.position + Vector3.up * 1.5f;
        Vector3 midPos = (startPos + endPos) / 2f + Vector3.up * crystalFlyHeight;
        
        Sequence flySequence = DOTween.Sequence();
        
        Vector3[] path = new Vector3[] { startPos, midPos, endPos };
        flySequence.Append(_currentCrystal.DOPath(path, crystalFlyDuration, PathType.CatmullRom).SetEase(Ease.InOutQuad));
        
        flySequence.Join(_currentCrystal.DORotate(new Vector3(360f, 720f, 360f), crystalFlyDuration, RotateMode.FastBeyond360).SetEase(Ease.InOutQuad));
        
        flySequence.Join(_currentCrystal.DOScale(Vector3.zero, crystalFlyDuration * 0.3f).SetDelay(crystalFlyDuration * 0.7f).SetEase(Ease.InBack));
        
        flySequence.OnComplete(() =>
        {
            if (WalletController.Instance != null)
            {
                WalletController.Instance.Money += crystalReward;
            }
            else
            {
                Debug.LogError("WalletController Instance is null");
            }
            
            if (_currentCrystal != null)
            {
                Destroy(_currentCrystal.gameObject);
            }
            else
            {
                Debug.LogError("Current crystal is null in fly complete");
            }
            
            _currentCrystal = null;
            _currentCrystalTrigger = null;
            
            if (robot != null)
            {
                robot.enabled = true;
            }
            else
            {
                Debug.LogError("Robot is null in fly complete");
            }
        });
    }
    
    private bool IsPlayerInTrigger()
    {
        if (_currentCrystalTrigger == null || player == null)
        {
            if (_currentCrystalTrigger == null)
            {
                Debug.LogError("Current crystal trigger is null");
            }
            else
            {
                Debug.LogError("Player is null in IsPlayerInTrigger");
            }
            return false;
        }
        
        Bounds bounds = _currentCrystalTrigger.bounds;
        return bounds.Contains(player.position);
    }
}