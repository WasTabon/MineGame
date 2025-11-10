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
    [SerializeField] private float robotStopDistance = 0.5f;
    
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
    private Coroutine _waitForRobotCoroutine;
    private Coroutine _uiRotationCoroutine;
    private Camera _mainCamera;
    
    private void Start()
    {
        _mainCamera = Camera.main;
        
        if (_mainCamera == null)
        {
            Debug.LogError("Main Camera is null");
        }
        
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
        robot.SetMiningTarget(_currentCrystal);
        
        _waitForRobotCoroutine = StartCoroutine(WaitForRobotToReachCrystal());
    }
    
    private IEnumerator WaitForRobotToReachCrystal()
    {
        if (robot == null || _currentCrystal == null)
        {
            if (robot == null)
            {
                Debug.LogError("Robot is null in WaitForRobotToReachCrystal start");
            }
            else
            {
                Debug.LogError("Current crystal is null in WaitForRobotToReachCrystal start");
            }
            yield break;
        }
        
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
        
        float targetDistance = triggerRadius - robotStopDistance;
        
        while (_isMining && _currentCrystal != null && robot != null)
        {
            float distance = Vector3.Distance(robot.transform.position, _currentCrystal.position);
            
            if (distance <= targetDistance)
            {
                break;
            }
            
            yield return null;
        }
        
        if (!_isMining)
        {
            yield break;
        }
        
        if (_currentCrystal == null)
        {
            Debug.LogError("Current crystal is null after waiting");
            yield break;
        }
        
        if (robot == null)
        {
            Debug.LogError("Robot is null after waiting");
            yield break;
        }
        
        Vector3 crystalPosition = _currentCrystal.position;
        Vector3 lookDirection = (crystalPosition - robot.transform.position).normalized;
        lookDirection.y = 0f;
        
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            robot.transform.DORotateQuaternion(targetRotation, 0.3f).OnComplete(() =>
            {
                if (_isMining && _currentCrystal != null)
                {
                    ShowMiningUI();
                    _miningCoroutine = StartCoroutine(MiningProcess());
                }
            });
        }
        else
        {
            if (_isMining && _currentCrystal != null)
            {
                ShowMiningUI();
                _miningCoroutine = StartCoroutine(MiningProcess());
            }
        }
    }
    
    private void StopMining()
    {
        _isMining = false;
        _miningTimer = 0f;
        
        if (_waitForRobotCoroutine != null)
        {
            StopCoroutine(_waitForRobotCoroutine);
            _waitForRobotCoroutine = null;
        }
        
        if (_miningCoroutine != null)
        {
            StopCoroutine(_miningCoroutine);
            _miningCoroutine = null;
        }
        
        HideMiningUI();
        
        if (robot != null)
        {
            robot.ClearMiningTarget();
        }
        else
        {
            Debug.LogError("Robot is null in StopMining");
        }
        
        _currentCrystal = null;
        _currentCrystalTrigger = null;
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
            
            _uiRotationCoroutine = StartCoroutine(RotateUIToCamera());
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
    
    private IEnumerator RotateUIToCamera()
    {
        while (miningUIBackground != null && miningUIBackground.gameObject.activeSelf && _mainCamera != null)
        {
            Vector3 directionToCamera = _mainCamera.transform.position - miningUIBackground.transform.position;
            directionToCamera.y = 0f;
            
            if (directionToCamera != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
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
        if (_currentCrystal == null)
        {
            Debug.LogError("Current crystal is null in CompleteMining");
            return;
        }
        
        if (robot == null)
        {
            Debug.LogError("Robot is null in CompleteMining");
            return;
        }
        
        HideMiningUI();
        
        Transform crystalToAnimate = _currentCrystal;
        Transform childCrystal = crystalToAnimate.Find("FX_Crystal_Floating_02");
        
        Sequence shrinkSequence = DOTween.Sequence();
        
        shrinkSequence.Append(crystalToAnimate.DOScale(Vector3.one * 0.1f, crystalShrinkDuration).SetEase(Ease.InBack));
        shrinkSequence.Join(crystalToAnimate.DORotate(new Vector3(0f, 360f, 0f), crystalShrinkDuration, RotateMode.FastBeyond360));
        
        if (childCrystal != null)
        {
            shrinkSequence.Join(childCrystal.DOScale(Vector3.one * 0.1f, crystalShrinkDuration).SetEase(Ease.InBack));
        }
        else
        {
            Debug.LogError("Child crystal FX_Crystal_Floating_02 not found");
        }
        
        ParticleSystem[] particles = crystalToAnimate.GetComponentsInChildren<ParticleSystem>();
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
            if (crystalToAnimate != null && robot != null)
            {
                FlyToRobot(crystalToAnimate);
            }
            else
            {
                if (crystalToAnimate == null)
                {
                    Debug.LogError("Crystal is null after shrink");
                }
                else
                {
                    Debug.LogError("Robot is null after shrink");
                }
            }
        });
    }
    
    private void FlyToRobot(Transform crystal)
    {
        if (crystal == null || robot == null)
        {
            if (crystal == null)
            {
                Debug.LogError("Crystal is null in FlyToRobot");
            }
            else
            {
                Debug.LogError("Robot is null in FlyToRobot");
            }
            return;
        }
        
        Vector3 startPos = crystal.position;
        Vector3 endPos = robot.transform.position + Vector3.up * 1.5f;
        Vector3 midPos = (startPos + endPos) / 2f + Vector3.up * crystalFlyHeight;
        
        Sequence flySequence = DOTween.Sequence();
        
        Vector3[] path = new Vector3[] { startPos, midPos, endPos };
        flySequence.Append(crystal.DOPath(path, crystalFlyDuration, PathType.CatmullRom).SetEase(Ease.InOutQuad));
        
        flySequence.Join(crystal.DORotate(new Vector3(360f, 720f, 360f), crystalFlyDuration, RotateMode.FastBeyond360).SetEase(Ease.InOutQuad));
        
        flySequence.Join(crystal.DOScale(Vector3.zero, crystalFlyDuration * 0.3f).SetDelay(crystalFlyDuration * 0.7f).SetEase(Ease.InBack));
        
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
            
            if (crystal != null)
            {
                Destroy(crystal.gameObject);
            }
            else
            {
                Debug.LogError("Crystal is null in fly complete");
            }
            
            _currentCrystal = null;
            _currentCrystalTrigger = null;
            
            if (robot != null)
            {
                robot.ClearMiningTarget();
            }
            else
            {
                Debug.LogError("Robot is null in fly complete");
            }
        });
    }
}