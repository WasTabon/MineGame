using UnityEngine;

public class SnowEffect : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private int particleCount = 150;
    [SerializeField] private float windStrength = 8f;
    [SerializeField] private float turbulence = 3f;
    [SerializeField] private float spawnDistance = 10f;
    
    private ParticleSystem blizzardParticles;
    
    void Start()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main camera is null");
            return;
        }
        
        CreateBlizzard();
    }
    
    void CreateBlizzard()
    {
        GameObject blizzardObj = new GameObject("Blizzard");
        blizzardObj.transform.SetParent(mainCamera.transform);
        blizzardObj.transform.localPosition = new Vector3(0, 0, spawnDistance);
        blizzardObj.transform.localRotation = Quaternion.identity;
        
        blizzardParticles = blizzardObj.AddComponent<ParticleSystem>();
        
        var main = blizzardParticles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = particleCount;
        main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startColor = new Color(1f, 1f, 1f, 0.9f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0, 360f * Mathf.Deg2Rad);
        
        var emission = blizzardParticles.emission;
        emission.rateOverTime = particleCount / 2.5f;
        
        var shape = blizzardParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        Vector3 boxSize = CalculateBoxSize();
        shape.scale = boxSize;
        
        var velocityOverLifetime = blizzardParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-windStrength, windStrength);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-5f, -2f);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-4f, -1f);
        
        var noise = blizzardParticles.noise;
        noise.enabled = true;
        noise.strength = turbulence;
        noise.frequency = 0.5f;
        noise.scrollSpeed = 2f;
        noise.damping = false;
        
        var rotationOverLifetime = blizzardParticles.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-180f * Mathf.Deg2Rad, 180f * Mathf.Deg2Rad);
        
        var sizeOverLifetime = blizzardParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0f);
        sizeCurve.AddKey(0.1f, 1f);
        sizeCurve.AddKey(0.9f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        var colorOverLifetime = blizzardParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0f), 
                new GradientColorKey(Color.white, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0f, 0f), 
                new GradientAlphaKey(0.9f, 0.1f), 
                new GradientAlphaKey(0.9f, 0.9f), 
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);
        
        var renderer = blizzardParticles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateBlizzardMaterial();
        renderer.sortingFudge = 0;
    }
    
    Vector3 CalculateBoxSize()
    {
        float halfFOV = mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
        float height = 2f * spawnDistance * Mathf.Tan(halfFOV);
        float width = height * mainCamera.aspect;
        
        float margin = 2.5f;
        
        return new Vector3(
            width * margin,
            height * margin,
            spawnDistance * 1.2f
        );
    }
    
    Material CreateBlizzardMaterial()
    {
        Material mat = new Material(Shader.Find("Unlit/Transparent"));
        
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        
        Vector2 center = new Vector2(16, 16);
        
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / 16f;
                float alpha = Mathf.Clamp01(1f - dist);
                alpha = Mathf.Pow(alpha, 2f);
                colors[y * 32 + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        mat.mainTexture = texture;
        mat.SetColor("_Color", Color.white);
        
        return mat;
    }
    
    void OnValidate()
    {
        if (Application.isPlaying && blizzardParticles != null && mainCamera != null)
        {
            UpdateParticleSystemSize();
        }
    }
    
    void UpdateParticleSystemSize()
    {
        var shape = blizzardParticles.shape;
        Vector3 boxSize = CalculateBoxSize();
        shape.scale = boxSize;
    }
    
    public void SetWindStrength(float strength)
    {
        windStrength = strength;
        
        if (blizzardParticles == null)
        {
            Debug.LogError("Blizzard particles is null");
            return;
        }
        
        var velocityOverLifetime = blizzardParticles.velocityOverLifetime;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-windStrength, windStrength);
    }
    
    public void SetTurbulence(float turb)
    {
        turbulence = turb;
        
        if (blizzardParticles == null)
        {
            Debug.LogError("Blizzard particles is null");
            return;
        }
        
        var noise = blizzardParticles.noise;
        noise.strength = turbulence;
    }
}