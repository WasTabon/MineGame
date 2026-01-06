using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class EnemyRadar : MonoBehaviour
{
    public static EnemyRadar Instance;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RectTransform radarCanvas;
    [SerializeField] private GameObject arrowPrefab;

    [Header("Radar Settings")]
    [SerializeField] private float edgeOffset = 100f;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private float minDistance = 5f;

    [Header("Arrow Size")]
    [SerializeField] private float minArrowScale = 0.5f;
    [SerializeField] private float maxArrowScale = 1.5f;

    [Header("Arrow Colors")]
    [SerializeField] private Color farColor = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private Color midColor = new Color(1f, 0.8f, 0.2f, 0.8f);
    [SerializeField] private Color closeColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color dangerColor = new Color(1f, 0f, 0f, 1f);

    [Header("Animation")]
    [SerializeField] private float pulseSpeed = 0.5f;
    [SerializeField] private float pulseIntensity = 1.2f;
    [SerializeField] private float dangerDistance = 10f;

    private Dictionary<Enemy, RadarArrow> _arrows = new Dictionary<Enemy, RadarArrow>();
    private List<Enemy> _enemies = new List<Enemy>();

    private class RadarArrow
    {
        public GameObject gameObject;
        public RectTransform rectTransform;
        public Image image;
        public Tween pulseTween;
        public bool isPulsing;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        RefreshEnemyList();
    }

    private void Update()
    {
        if (player == null || mainCamera == null) return;

        UpdateAllArrows();
    }

    public void RefreshEnemyList()
    {
        _enemies.Clear();
        Enemy[] foundEnemies = FindObjectsOfType<Enemy>();
        _enemies.AddRange(foundEnemies);

        foreach (var arrow in _arrows.Values)
        {
            if (arrow.pulseTween != null) arrow.pulseTween.Kill();
            if (arrow.gameObject != null) Destroy(arrow.gameObject);
        }
        _arrows.Clear();

        foreach (var enemy in _enemies)
        {
            CreateArrowForEnemy(enemy);
        }
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (!_enemies.Contains(enemy))
        {
            _enemies.Add(enemy);
            CreateArrowForEnemy(enemy);
        }
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        if (_enemies.Contains(enemy))
        {
            _enemies.Remove(enemy);

            if (_arrows.TryGetValue(enemy, out RadarArrow arrow))
            {
                if (arrow.pulseTween != null) arrow.pulseTween.Kill();
                if (arrow.gameObject != null) Destroy(arrow.gameObject);
                _arrows.Remove(enemy);
            }
        }
    }

    private void CreateArrowForEnemy(Enemy enemy)
    {
        if (arrowPrefab == null || radarCanvas == null) return;

        GameObject arrowObj = Instantiate(arrowPrefab, radarCanvas);
        
        RadarArrow arrow = new RadarArrow
        {
            gameObject = arrowObj,
            rectTransform = arrowObj.GetComponent<RectTransform>(),
            image = arrowObj.GetComponent<Image>(),
            isPulsing = false
        };

        if (arrow.image == null)
        {
            arrow.image = arrowObj.GetComponentInChildren<Image>();
        }

        _arrows[enemy] = arrow;
    }

    private void UpdateAllArrows()
    {
        List<Enemy> enemiesToRemove = new List<Enemy>();

        foreach (var kvp in _arrows)
        {
            Enemy enemy = kvp.Key;
            RadarArrow arrow = kvp.Value;

            if (enemy == null || arrow.gameObject == null)
            {
                enemiesToRemove.Add(enemy);
                continue;
            }

            UpdateArrow(enemy, arrow);
        }

        foreach (var enemy in enemiesToRemove)
        {
            if (_arrows.TryGetValue(enemy, out RadarArrow arrow))
            {
                if (arrow.pulseTween != null) arrow.pulseTween.Kill();
                if (arrow.gameObject != null) Destroy(arrow.gameObject);
            }
            _arrows.Remove(enemy);
            _enemies.Remove(enemy);
        }
    }

    private void UpdateArrow(Enemy enemy, RadarArrow arrow)
    {
        Vector3 enemyPosition = enemy.transform.position;
        Vector3 playerPosition = player.position;

        float distance = Vector3.Distance(
            new Vector3(enemyPosition.x, 0, enemyPosition.z),
            new Vector3(playerPosition.x, 0, playerPosition.z)
        );

        Vector3 screenPos = mainCamera.WorldToScreenPoint(enemyPosition);

        bool isOnScreen = screenPos.z > 0 &&
                          screenPos.x > 0 && screenPos.x < Screen.width &&
                          screenPos.y > 0 && screenPos.y < Screen.height;

        Vector3 directionToEnemy = enemyPosition - playerPosition;
        directionToEnemy.y = 0;

        if (isOnScreen)
        {
            arrow.gameObject.SetActive(false);
        }
        else
        {
            arrow.gameObject.SetActive(true);
            PositionArrowOnEdge(arrow, directionToEnemy, screenPos);
        }

        UpdateArrowAppearance(arrow, distance);
    }

    private void PositionArrowOnEdge(RadarArrow arrow, Vector3 direction, Vector3 screenPos)
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);

        if (screenPos.z < 0)
        {
            screenPos.x = Screen.width - screenPos.x;
            screenPos.y = Screen.height - screenPos.y;
        }

        Vector3 directionFromCenter = (screenPos - screenCenter).normalized;

        float angle = Mathf.Atan2(directionFromCenter.y, directionFromCenter.x);

        float halfWidth = (Screen.width / 2f) - edgeOffset;
        float halfHeight = (Screen.height / 2f) - edgeOffset;

        float absX = Mathf.Abs(Mathf.Cos(angle));
        float absY = Mathf.Abs(Mathf.Sin(angle));

        float edgeX, edgeY;

        if (absX * halfHeight > absY * halfWidth)
        {
            edgeX = Mathf.Sign(Mathf.Cos(angle)) * halfWidth;
            edgeY = Mathf.Tan(angle) * edgeX;
        }
        else
        {
            edgeY = Mathf.Sign(Mathf.Sin(angle)) * halfHeight;
            edgeX = edgeY / Mathf.Tan(angle);
        }

        Vector2 edgePosition = new Vector2(
            screenCenter.x + edgeX,
            screenCenter.y + edgeY
        );

        arrow.rectTransform.position = edgePosition;

        float rotationAngle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
        arrow.rectTransform.rotation = Quaternion.Euler(0, 0, rotationAngle - 90f);
    }

    private void UpdateArrowAppearance(RadarArrow arrow, float distance)
    {
        float normalizedDistance = Mathf.InverseLerp(minDistance, maxDistance, distance);
        normalizedDistance = Mathf.Clamp01(normalizedDistance);

        float scale = Mathf.Lerp(maxArrowScale, minArrowScale, normalizedDistance);
        
        if (!arrow.isPulsing)
        {
            arrow.rectTransform.localScale = Vector3.one * scale;
        }

        Color targetColor;
        if (distance <= dangerDistance)
        {
            targetColor = dangerColor;
        }
        else if (normalizedDistance < 0.33f)
        {
            targetColor = Color.Lerp(dangerColor, closeColor, normalizedDistance / 0.33f);
        }
        else if (normalizedDistance < 0.66f)
        {
            targetColor = Color.Lerp(closeColor, midColor, (normalizedDistance - 0.33f) / 0.33f);
        }
        else
        {
            targetColor = Color.Lerp(midColor, farColor, (normalizedDistance - 0.66f) / 0.34f);
        }

        if (arrow.image != null)
        {
            arrow.image.color = targetColor;
        }

        bool shouldPulse = distance <= dangerDistance;

        if (shouldPulse && !arrow.isPulsing)
        {
            StartPulse(arrow, scale);
        }
        else if (!shouldPulse && arrow.isPulsing)
        {
            StopPulse(arrow, scale);
        }
    }

    private void StartPulse(RadarArrow arrow, float baseScale)
    {
        arrow.isPulsing = true;

        arrow.pulseTween = arrow.rectTransform
            .DOScale(baseScale * pulseIntensity, pulseSpeed)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopPulse(RadarArrow arrow, float baseScale)
    {
        arrow.isPulsing = false;

        if (arrow.pulseTween != null)
        {
            arrow.pulseTween.Kill();
            arrow.pulseTween = null;
        }

        arrow.rectTransform.DOScale(baseScale, 0.2f).SetEase(Ease.OutQuad);
    }

    private void OnDestroy()
    {
        foreach (var arrow in _arrows.Values)
        {
            if (arrow.pulseTween != null) arrow.pulseTween.Kill();
            if (arrow.rectTransform != null) arrow.rectTransform.DOKill();
        }
    }
}
