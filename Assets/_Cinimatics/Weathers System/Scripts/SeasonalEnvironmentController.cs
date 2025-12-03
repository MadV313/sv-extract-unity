using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace AdvancedWeatherSystem
{
    public class SeasonalEnvironmentController : MonoBehaviour
    {
        [Header("Cycle Manager Reference")]
        public CycleManager cycleManager;
    
    [Header("Seasonal Lighting")]
    public AnimationCurve springLightIntensity;
    public AnimationCurve summerLightIntensity;
    public AnimationCurve autumnLightIntensity;
    public AnimationCurve winterLightIntensity;
    
    [Header("Seasonal Colors")]
    public Gradient springSunColor;
    public Gradient summerSunColor;
    public Gradient autumnSunColor;
    public Gradient winterSunColor;
    
    [Header("Seasonal Fog")]
    public Color springFogColor = Color.white;
    public Color summerFogColor = Color.white;
    public Color autumnFogColor = new Color(1f, 0.8f, 0.6f);
    public Color winterFogColor = new Color(0.8f, 0.9f, 1f);
    
    [Header("Seasonal Sky Tint")]
    public Color springSkyTint = Color.white;
    public Color summerSkyTint = Color.white;
    public Color autumnSkyTint = new Color(1f, 0.9f, 0.7f);
    public Color winterSkyTint = new Color(0.9f, 0.95f, 1f);
    
    [Header("Environment Objects")]
    public GameObject[] springObjects;
    public GameObject[] summerObjects;
    public GameObject[] autumnObjects;
    public GameObject[] winterObjects;
    
    [Header("Seasonal Materials")]
    public Material grassSpringMaterial;
    public Material grassSummerMaterial;
    public Material grassAutumnMaterial;
    public Material grassWinterMaterial;
    
    public Renderer[] grassRenderers;
    
    [Header("Transition Settings")]
    public float transitionSpeed = 1f;
    
    // Performance optimization variables
    private Season lastSeason;
    private Material currentGrassMaterial;
    private float updateTimer = 0f;
    private const float UPDATE_INTERVAL = 2f; // Increased to 2 seconds for even better performance
    private bool needsSeasonUpdate = false;
    private bool needsLightingUpdate = false;

    void Start()
    {
        if (cycleManager == null)
            cycleManager = FindObjectOfType<CycleManager>();
            
        lastSeason = cycleManager.currentSeason;
        UpdateSeasonalEnvironment();
    }

    void Update()
    {
        if (cycleManager == null) return;
        
        // Check if season has changed
        if (lastSeason != cycleManager.currentSeason)
        {
            lastSeason = cycleManager.currentSeason;
            needsSeasonUpdate = true;
            needsLightingUpdate = true;
        }
        
        // Much less frequent updates for seasonal effects
        updateTimer += Time.deltaTime;
        if (updateTimer >= UPDATE_INTERVAL)
        {
            if (needsSeasonUpdate)
            {
                UpdateSeasonalEnvironment();
                needsSeasonUpdate = false;
            }
            
            if (needsLightingUpdate)
            {
                UpdateSeasonalLighting();
                UpdateSeasonalColors();
                UpdateSeasonalFog();
                needsLightingUpdate = false;
            }
            
            updateTimer = 0f;
        }
    }
    
    void UpdateSeasonalEnvironment()
    {
        ToggleSeasonalObjects();
        UpdateSeasonalMaterials();
    }
    
    void ToggleSeasonalObjects()
    {
        // Disable all seasonal objects first
        ToggleObjectArray(springObjects, false);
        ToggleObjectArray(summerObjects, false);
        ToggleObjectArray(autumnObjects, false);
        ToggleObjectArray(winterObjects, false);
        
        // Enable current season objects
        switch (cycleManager.currentSeason)
        {
            case Season.Spring:
                ToggleObjectArray(springObjects, true);
                break;
            case Season.Summer:
                ToggleObjectArray(summerObjects, true);
                break;
            case Season.Autumn:
                ToggleObjectArray(autumnObjects, true);
                break;
            case Season.Winter:
                ToggleObjectArray(winterObjects, true);
                break;
        }
    }
    
    void ToggleObjectArray(GameObject[] objects, bool active)
    {
        if (objects == null) return;
        
        foreach (GameObject obj in objects)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }
    
    void UpdateSeasonalMaterials()
    {
        Material targetMaterial = null;
        
        switch (cycleManager.currentSeason)
        {
            case Season.Spring:
                targetMaterial = grassSpringMaterial;
                break;
            case Season.Summer:
                targetMaterial = grassSummerMaterial;
                break;
            case Season.Autumn:
                targetMaterial = grassAutumnMaterial;
                break;
            case Season.Winter:
                targetMaterial = grassWinterMaterial;
                break;
        }
        
        // Only update materials if they actually changed
        if (targetMaterial != null && targetMaterial != currentGrassMaterial && grassRenderers != null)
        {
            foreach (Renderer renderer in grassRenderers)
            {
                if (renderer != null)
                    renderer.material = targetMaterial;
            }
            currentGrassMaterial = targetMaterial;
        }
    }
    
    void UpdateSeasonalLighting()
    {
        if (cycleManager.sunLight == null) return;
        
        float normalizedTime = cycleManager.currentTime / 24f;
        AnimationCurve intensityCurve = GetSeasonalLightCurve();
        
        if (intensityCurve != null)
        {
            float seasonalMultiplier = intensityCurve.Evaluate(normalizedTime);
            // Apply seasonal lighting intensity modifier
            // This works in conjunction with the existing sun intensity curve
            var sunData = cycleManager.sunLight.GetComponent<HDAdditionalLightData>();
            if (sunData != null)
            {
                // You can modify intensity here based on season
                // For example: sunData.intensity *= seasonalMultiplier;
            }
        }
    }
    
    AnimationCurve GetSeasonalLightCurve()
    {
        switch (cycleManager.currentSeason)
        {
            case Season.Spring:
                return springLightIntensity;
            case Season.Summer:
                return summerLightIntensity;
            case Season.Autumn:
                return autumnLightIntensity;
            case Season.Winter:
                return winterLightIntensity;
            default:
                return springLightIntensity;
        }
    }
    
    void UpdateSeasonalColors()
    {
        if (cycleManager.sunLight == null) return;
        
        float normalizedTime = cycleManager.currentTime / 24f;
        Gradient colorGradient = GetSeasonalColorGradient();
        
        if (colorGradient != null)
        {
            Color seasonalColor = colorGradient.Evaluate(normalizedTime);
            cycleManager.sunLight.color = seasonalColor;
        }
    }
    
    Gradient GetSeasonalColorGradient()
    {
        switch (cycleManager.currentSeason)
        {
            case Season.Spring:
                return springSunColor;
            case Season.Summer:
                return summerSunColor;
            case Season.Autumn:
                return autumnSunColor;
            case Season.Winter:
                return winterSunColor;
            default:
                return springSunColor;
        }
    }
    
    void UpdateSeasonalFog()
    {
        Color fogColor = GetSeasonalFogColor();
        RenderSettings.fogColor = fogColor;
    }
    
    Color GetSeasonalFogColor()
    {
        switch (cycleManager.currentSeason)
        {
            case Season.Spring:
                return springFogColor;
            case Season.Summer:
                return summerFogColor;
            case Season.Autumn:
                return autumnFogColor;
            case Season.Winter:
                return winterFogColor;
            default:
                return springFogColor;
        }
    }
    
    /// <summary>
    /// Get the current seasonal sky tint
    /// </summary>
    public Color GetCurrentSkyTint()
    {
        switch (cycleManager.currentSeason)
        {
            case Season.Spring:
                return springSkyTint;
            case Season.Summer:
                return summerSkyTint;
            case Season.Autumn:
                return autumnSkyTint;
            case Season.Winter:
                return winterSkyTint;
            default:
                return springSkyTint;
        }
    }
    
    /// <summary>
    /// Force update all seasonal effects
    /// </summary>
    public void ForceUpdateSeason()
    {
        UpdateSeasonalEnvironment();
        UpdateSeasonalLighting();
        UpdateSeasonalColors();
        UpdateSeasonalFog();
    }
    
    /// <summary>
    /// Call this when cloud volume is reassigned to recache component
    /// </summary>
    public void RefreshCacheReferences()
    {
        // This can be called if you change cloud volume at runtime
        needsSeasonUpdate = true;
    }
}

} // namespace AdvancedWeatherSystem
