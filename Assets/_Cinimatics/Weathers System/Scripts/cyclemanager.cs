using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace AdvancedWeatherSystem
{
    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    public enum WeatherType
    {
        Clear,
        Rain,
        Snow,
        Lightning,
        Thunderstorm  // Rain + Lightning combo
    }

    public class CycleManager : MonoBehaviour
{
    [Header("⏱️ Time Progression")]
    [Tooltip("Current time of day in 24-hour format.")]
    [Range(0f, 24f)] public float currentTime;
    
    [Tooltip("Speed multiplier for time progression.")]
    public float timeSpeed = 1f;

    [Space]
    [Header("📅 Seasonal Cycle")]
    [Tooltip("Current day of the year (1-365).")]
    [Range(1, 365)] public int currentDay = 1;
    
    [Tooltip("Speed multiplier for seasonal progression.")]
    public float seasonSpeed = 1f;
    
    [Tooltip("Current season.")]
    public Season currentSeason = Season.Spring;
    
    [Tooltip("Days per season (total 365 days per year).")]
    public int daysPerSeason = 91;

    [Space]
    [Header("🌦️ Weather System")]
    [Tooltip("Current weather type.")]
    public WeatherType currentWeather = WeatherType.Clear;
    
    [Tooltip("Time until next weather change (in hours).")]
    public float weatherChangeTimer = 0f;
    
    [Tooltip("Minimum hours between weather changes.")]
    public float minWeatherDuration = 2f;
    
    [Tooltip("Maximum hours between weather changes.")]
    public float maxWeatherDuration = 8f;
    
    [Space]
    [Header("☔ Rain Particles")]
    public ParticleSystem rainParticles;
    public Transform rainParticlesParent;
    
    [Space]
    [Header("❄️ Snow Particles")]
    public ParticleSystem snowParticles;
    public Transform snowParticlesParent;
    
    [Space]
    [Header("⚡ Lightning Particles")]
    public ParticleSystem lightningParticles;
    public Transform lightningParticlesParent;
    
    [Space]
    [Header("☁️ Cloud Volume")]
    public Volume cloudVolume;
    public AnimationCurve cloudDensityCurve;
    
    [Tooltip("Cloud density settings for different weather types")]
    public float clearCloudDensity = 0.1f;
    public float rainCloudDensity = 0.8f;
    public float snowCloudDensity = 0.7f;
    public float lightningCloudDensity = 0.6f;
    public float thunderstormCloudDensity = 0.95f; // Very heavy clouds for thunderstorms
    
    [Tooltip("Cloud transition speed")]
    public float cloudTransitionSpeed = 1f;
    
    [Space]
    [Header("🌡️ Seasonal Weather Probabilities")]
    [Tooltip("Weather probabilities for Spring [Clear, Rain, Snow, Lightning, Thunderstorm]")]
    public Vector4 springWeatherChance = new Vector4(0.5f, 0.3f, 0.1f, 0.1f);
    public float springThunderstormChance = 0.0f;
    
    [Tooltip("Weather probabilities for Summer [Clear, Rain, Snow, Lightning, Thunderstorm]")]
    public Vector4 summerWeatherChance = new Vector4(0.4f, 0.2f, 0.0f, 0.1f);
    public float summerThunderstormChance = 0.3f; // High chance of rain+lightning combo
    
    [Tooltip("Weather probabilities for Autumn [Clear, Rain, Snow, Lightning, Thunderstorm]")]
    public Vector4 autumnWeatherChance = new Vector4(0.4f, 0.4f, 0.1f, 0.05f);
    public float autumnThunderstormChance = 0.05f;
    
    [Tooltip("Weather probabilities for Winter [Clear, Rain, Snow, Lightning, Thunderstorm]")]
    public Vector4 winterWeatherChance = new Vector4(0.3f, 0.1f, 0.5f, 0.1f);
    public float winterThunderstormChance = 0.0f;

    [Space]
    [Header("🕒 Current Time Display")]
    [Tooltip("Formatted time string for UI or display.")]
    public string currentTimeString;

    [Space]
    [Header("☀️ Sun Configuration")]
    public Light sunLight;
    
    [Tooltip("Sun latitude angle.")]
    [Range(0f, 90f)] public float sunLatitude = 20f;

    [Tooltip("Sun longitude angle.")]
    [Range(-180f, 180f)] public float sunLongitude = -90f;

    [Tooltip("Base intensity for sun.")]
    // public float sunIntensity = 1f;

    public AnimationCurve sunIntensityMultiplier;
    // public AnimationCurve sunTemperatureCurve;

    [Space]
    [Header("🌙 Moon Configuration")]
    public Light moonLight;
    
    [Tooltip("Moon latitude angle.")]
    [Range(0f, 90f)] public float moonLatitude = 40f;

    [Tooltip("Moon longitude angle.")]
    [Range(-180f, 180f)] public float moonLongitude = 90f;

    [Tooltip("Base intensity for moon.")]
    public float moonIntensity = 1f;

    public AnimationCurve moonIntensityMultiplier;
    public AnimationCurve moonTemperatureCurve;

    [Space]
    [Header("🌕 Lunar Cycle Settings")]
    [Range(1, 30)] public int lunarDay = 1;

    [Tooltip("Speed at which the lunar day changes.")]
    public float lunarCycleSpeed = 1f;

    [Tooltip("Reference to moon visual model.")]
    public Transform moonModel;

    [Space]
    [Header("🔁 Status Flags (Read-Only)")]
    public bool isDay = true;
    public bool sunActive = true;
    public bool moonActive = true;
    
    [Space]
    [Header("⚡ Performance Settings")]
    [Tooltip("Disable cloud volume updates for better performance")]
    public bool disableCloudUpdates = false;
    
    [Tooltip("Reduce particle emission rates for better performance")]
    public bool lowParticleMode = false;
    
    [Tooltip("Disable seasonal weather intensity variations")]
    public bool simplifiedWeatherIntensity = true;
    [Space]
    [Header("📊 Debug Info (Read-Only)")]
    public string currentSeasonString;
    public string currentWeatherString;
    public int daysInCurrentSeason;
    
    // Private variables for smooth transitions
    private float currentCloudDensity = 0.1f;
    private float targetCloudDensity = 0.1f;
    
    // Performance optimization variables
    private float updateTimer = 0f;
    private const float UPDATE_INTERVAL = 0.1f; // Reduced to 0.1 seconds for more responsive updates
    private float lightUpdateTimer = 0f;
    private const float LIGHT_UPDATE_INTERVAL = 0.03f; // ~30 FPS for smooth lighting
    private bool isCloudDensityDirty = false;
    private UnityEngine.Rendering.HighDefinition.VolumetricClouds cachedCloudsComponent;
    
    // Cache weather state to avoid unnecessary updates
    private WeatherType lastWeatherType;
    private bool particlesNeedUpdate = false;
    
    // Cache light components to avoid repeated GetComponent calls
    private HDAdditionalLightData cachedSunData;
    private HDAdditionalLightData cachedMoonData;
    
    // Cache previous values to avoid unnecessary updates
    private float lastSunIntensity = -1f;
    private float lastMoonIntensity = -1f;
    private float lastSunTemperature = -1f;
    private float lastMoonTemperature = -1f;
    private bool lastSunShadowState = true;
    private bool lastMoonShadowState = false;

    void Start()
    {
        UpdateTimeText();
        CheckShadowStatus();
        UpdateSeason();
        InitializeWeather();
        
        // Cache cloud component for performance
        CacheCloudComponent();
        
        // Cache light components for performance
        CacheLightComponents();
        
        // Initialize weather state tracking
        lastWeatherType = currentWeather;
        UpdateWeatherParticles();
    }

    void Update()
    {
        // Core time progression - keep this every frame
        currentTime += Time.deltaTime * timeSpeed;

        if (currentTime >= 24f)
        {
            currentTime = 0f;
            lunarDay = (lunarDay % 30) + 1;
            currentDay++;
            
            if (currentDay > 365)
            {
                currentDay = 1;
            }
            
            UpdateSeason();
        }

        // Lighting updates - less frequent but still smooth
        lightUpdateTimer += Time.deltaTime;
        if (lightUpdateTimer >= LIGHT_UPDATE_INTERVAL)
        {
            UpdateLight();
            CheckShadowStatus();
            UpdateMoonPhase();
            lightUpdateTimer = 0f;
        }
        
        // Heavy operations - update less frequently
        updateTimer += Time.deltaTime;
        if (updateTimer >= UPDATE_INTERVAL)
        {
            UpdateTimeText();
            UpdateWeatherSystem();
            UpdateDebugInfo();
            updateTimer = 0f;
        }
        
        // Cloud density - only if dirty or target changed significantly AND not disabled
        if (!disableCloudUpdates && (isCloudDensityDirty || Mathf.Abs(currentCloudDensity - targetCloudDensity) > 0.05f))
        {
            UpdateCloudVolume();
        }
        
        // Particles - only update when weather actually changes
        if (particlesNeedUpdate)
        {
            UpdateWeatherParticles();
            particlesNeedUpdate = false;
        }
    }

    private void OnValidate()
    {
        // Only update in editor and when playing to avoid performance issues
        if (Application.isPlaying)
        {
            CacheLightComponents(); // Refresh cache if components changed
            UpdateLight();
            CheckShadowStatus();
            UpdateMoonPhase();
            UpdateSeason();
            UpdateDebugInfo();
        }
    }

    void UpdateTimeText()
    {
        currentTimeString = Mathf.Floor(currentTime).ToString("00") + ":" + ((currentTime % 1) * 60).ToString("00");
    }

    void UpdateLight()
    {
        float sunRotation = currentTime / 24f * 360f;
        float moonRotation = (currentTime + 12f) / 24f * 360f;

        sunLight.transform.localRotation = Quaternion.Euler(sunLatitude - 90f, sunLongitude, 0) * Quaternion.Euler(0, sunRotation, 0);
        moonLight.transform.localRotation = Quaternion.Euler(moonLatitude - 90f, moonLongitude, 0) * Quaternion.Euler(0, moonRotation, 0);

        float normalizedTime = currentTime / 24f;
        float sunCurve = sunIntensityMultiplier.Evaluate(normalizedTime);
        float moonCurve = moonIntensityMultiplier.Evaluate(normalizedTime);

        // Sun intensity is no longer managed by the script - left for manual control
        // Keep moon intensity changes for realistic lunar cycle
        if (cachedMoonData != null)
        {
            float phaseMultiplier = Mathf.Sin((float)lunarDay / 30f * Mathf.PI);
            float newMoonIntensity = moonCurve * moonIntensity * phaseMultiplier;
            if (Mathf.Abs(newMoonIntensity - lastMoonIntensity) > 0.01f)
            {
                moonLight.intensity = newMoonIntensity;
                lastMoonIntensity = newMoonIntensity;
            }
        }

        // Sun temperature is no longer managed by the script - left for manual control
        // Keep moon temperature changes for realistic lunar cycle
        if (moonLight != null)
        {
            float newMoonTemp = moonTemperatureCurve.Evaluate(normalizedTime) * 10000f;
            if (Mathf.Abs(newMoonTemp - lastMoonTemperature) > 50f)
            {
                moonLight.colorTemperature = newMoonTemp;
                lastMoonTemperature = newMoonTemp;
            }
        }
    }

    void CheckShadowStatus()
    {
        float t = currentTime;

        isDay = t >= 6f && t <= 18f;
        
        // Only update shadows if state changed
        bool newSunShadowState = isDay;
        bool newMoonShadowState = !isDay;
        
        if (newSunShadowState != lastSunShadowState)
        {
            if (cachedSunData != null)
                cachedSunData.EnableShadows(newSunShadowState);
            lastSunShadowState = newSunShadowState;
        }
        
        if (newMoonShadowState != lastMoonShadowState)
        {
            if (cachedMoonData != null)
                cachedMoonData.EnableShadows(newMoonShadowState);
            lastMoonShadowState = newMoonShadowState;
        }

        bool newSunActive = t >= 5.7f && t <= 18.3f;
        bool newMoonActive = !(t >= 6.3f && t <= 17.7f);
        
        // Only update GameObject active state if changed
        if (sunActive != newSunActive)
        {
            sunActive = newSunActive;
            sunLight.gameObject.SetActive(sunActive);
        }
        
        if (moonActive != newMoonActive)
        {
            moonActive = newMoonActive;
            moonLight.gameObject.SetActive(moonActive);
        }
    }

    void UpdateMoonPhase()
    {
        float phase = (lunarDay - 1) / 29f;
        float scale = Mathf.Sin(phase * Mathf.PI);
        scale = Mathf.Max(scale, 0.1f);

        if (moonModel != null)
            moonModel.localScale = Vector3.one * scale;
    }

    // ======== SEASONAL SYSTEM ========
    void UpdateSeason()
    {
        int dayInYear = currentDay - 1; // 0-364
        int seasonIndex = dayInYear / daysPerSeason;
        seasonIndex = Mathf.Clamp(seasonIndex, 0, 3);
        currentSeason = (Season)seasonIndex;
        daysInCurrentSeason = (dayInYear % daysPerSeason) + 1;
    }

    // ======== WEATHER SYSTEM ========
    void InitializeWeather()
    {
        weatherChangeTimer = Random.Range(minWeatherDuration, maxWeatherDuration);
        ChangeWeather();
        // Initialize cloud density
        SetTargetCloudDensity();
        currentCloudDensity = targetCloudDensity;
    }

    void UpdateWeatherSystem()
    {
        weatherChangeTimer -= UPDATE_INTERVAL * timeSpeed; // Use update interval directly
        
        if (weatherChangeTimer <= 0f)
        {
            ChangeWeather();
            weatherChangeTimer = Random.Range(minWeatherDuration, maxWeatherDuration);
        }
    }

    void ChangeWeather()
    {
        WeatherType previousWeather = currentWeather;
        
        Vector4 weatherChances = GetSeasonalWeatherChances();
        float thunderstormChance = GetSeasonalThunderstormChance();
        
        // Total chance including thunderstorm
        float totalChance = weatherChances.x + weatherChances.y + weatherChances.z + weatherChances.w + thunderstormChance;
        float randomValue = Random.Range(0f, totalChance);
        
        if (randomValue <= weatherChances.x)
            currentWeather = WeatherType.Clear;
        else if (randomValue <= weatherChances.x + weatherChances.y)
            currentWeather = WeatherType.Rain;
        else if (randomValue <= weatherChances.x + weatherChances.y + weatherChances.z)
            currentWeather = WeatherType.Snow;
        else if (randomValue <= weatherChances.x + weatherChances.y + weatherChances.z + weatherChances.w)
            currentWeather = WeatherType.Lightning;
        else
            currentWeather = WeatherType.Thunderstorm; // Rain + Lightning combo
            
        // Only update if weather actually changed
        if (currentWeather != previousWeather)
        {
            SetTargetCloudDensity();
            particlesNeedUpdate = true;
            lastWeatherType = currentWeather;
        }
    }

    Vector4 GetSeasonalWeatherChances()
    {
        switch (currentSeason)
        {
            case Season.Spring:
                return springWeatherChance;
            case Season.Summer:
                return summerWeatherChance;
            case Season.Autumn:
                return autumnWeatherChance;
            case Season.Winter:
                return winterWeatherChance;
            default:
                return springWeatherChance;
        }
    }
    
    float GetSeasonalThunderstormChance()
    {
        switch (currentSeason)
        {
            case Season.Spring:
                return springThunderstormChance;
            case Season.Summer:
                return summerThunderstormChance;
            case Season.Autumn:
                return autumnThunderstormChance;
            case Season.Winter:
                return winterThunderstormChance;
            default:
                return springThunderstormChance;
        }
    }

    void UpdateWeatherParticles()
    {
        // Skip if weather hasn't changed
        if (currentWeather == lastWeatherType && !particlesNeedUpdate)
            return;
            
        // Get all emission modules once
        bool hasRain = rainParticles != null;
        bool hasSnow = snowParticles != null;
        bool hasLightning = lightningParticles != null;
        
        // Disable all particles first - more aggressive stopping
        if (hasRain)
        {
            if (currentWeather != WeatherType.Rain && currentWeather != WeatherType.Thunderstorm)
            {
                if (rainParticles.isPlaying)
                {
                    rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }
        
        if (hasSnow)
        {
            if (currentWeather != WeatherType.Snow)
            {
                if (snowParticles.isPlaying)
                {
                    snowParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }
        
        if (hasLightning)
        {
            if (currentWeather != WeatherType.Lightning && currentWeather != WeatherType.Thunderstorm)
            {
                if (lightningParticles.isPlaying)
                {
                    lightningParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }

        // Enable current weather particles with simplified intensity
        float baseIntensity = simplifiedWeatherIntensity ? 1f : GetWeatherIntensity();
        float particleMultiplier = lowParticleMode ? 0.5f : 1f;
        
        switch (currentWeather)
        {
            case WeatherType.Clear:
                // All particles already stopped above
                break;
                
            case WeatherType.Rain:
                if (hasRain && !rainParticles.isPlaying)
                {
                    rainParticles.Play();
                    var rainEmission = rainParticles.emission;
                    rainEmission.enabled = true;
                    rainEmission.rateOverTime = 80f * baseIntensity * particleMultiplier; // Reduced from 100f
                }
                break;
                
            case WeatherType.Snow:
                if (hasSnow && !snowParticles.isPlaying)
                {
                    snowParticles.Play();
                    var snowEmission = snowParticles.emission;
                    snowEmission.enabled = true;
                    snowEmission.rateOverTime = 40f * baseIntensity * particleMultiplier; // Reduced from 50f
                }
                break;
                
            case WeatherType.Lightning:
                if (hasLightning && !lightningParticles.isPlaying)
                {
                    lightningParticles.Play();
                    var lightningEmission = lightningParticles.emission;
                    lightningEmission.enabled = true;
                    lightningEmission.rateOverTime = 3f * particleMultiplier; // Simplified - no random
                }
                break;
                
            case WeatherType.Thunderstorm:
                if (hasRain && !rainParticles.isPlaying)
                {
                    rainParticles.Play();
                    var rainEmission = rainParticles.emission;
                    rainEmission.enabled = true;
                    rainEmission.rateOverTime = 120f * baseIntensity * particleMultiplier; // Reduced from 150f
                }
                
                if (hasLightning && !lightningParticles.isPlaying)
                {
                    lightningParticles.Play();
                    var lightningEmission = lightningParticles.emission;
                    lightningEmission.enabled = true;
                    lightningEmission.rateOverTime = 5f * particleMultiplier; // Simplified - no random
                }
                break;
        }
        
        lastWeatherType = currentWeather;
    }

    float GetWeatherIntensity()
    {
        // Simplified intensity calculation - remove time-based complexity
        float seasonalIntensity = 1f;
        switch (currentSeason)
        {
            case Season.Winter:
                seasonalIntensity = 1.2f; // Reduced from 1.3f
                break;
            case Season.Autumn:
                seasonalIntensity = 1.1f; // Reduced from 1.2f
                break;
        }
        
        return seasonalIntensity;
    }

    void UpdateCloudVolume()
    {
        // Update cloud density more aggressively - larger steps
        float previousDensity = currentCloudDensity;
        currentCloudDensity = Mathf.MoveTowards(currentCloudDensity, targetCloudDensity, cloudTransitionSpeed * Time.deltaTime * 2f);
        
        // Only update cloud volume if density changed significantly
        if (Mathf.Abs(currentCloudDensity - previousDensity) < 0.02f && !isCloudDensityDirty)
            return;
            
        if (cachedCloudsComponent == null) return;
        
        // Simplified cloud density calculation - remove curve evaluation
        cachedCloudsComponent.densityMultiplier.value = currentCloudDensity;
        
        isCloudDensityDirty = false;
    }
    
    void CacheCloudComponent()
    {
        if (cloudVolume != null && cloudVolume.profile != null)
        {
            cloudVolume.profile.TryGet<UnityEngine.Rendering.HighDefinition.VolumetricClouds>(out cachedCloudsComponent);
        }
    }
    
    void CacheLightComponents()
    {
        if (sunLight != null)
            cachedSunData = sunLight.GetComponent<HDAdditionalLightData>();
        if (moonLight != null)
            cachedMoonData = moonLight.GetComponent<HDAdditionalLightData>();
    }
    
    void SetTargetCloudDensity()
    {
        float newTarget;
        switch (currentWeather)
        {
            case WeatherType.Clear:
                newTarget = clearCloudDensity;
                break;
            case WeatherType.Rain:
                newTarget = rainCloudDensity;
                break;
            case WeatherType.Snow:
                newTarget = snowCloudDensity;
                break;
            case WeatherType.Lightning:
                newTarget = lightningCloudDensity;
                break;
            case WeatherType.Thunderstorm:
                newTarget = thunderstormCloudDensity;
                break;
            default:
                newTarget = clearCloudDensity;
                break;
        }
        
        if (Mathf.Abs(targetCloudDensity - newTarget) > 0.01f)
        {
            targetCloudDensity = newTarget;
            isCloudDensityDirty = true;
        }
    }

    void UpdateDebugInfo()
    {
        currentSeasonString = currentSeason.ToString();
        currentWeatherString = currentWeather.ToString();
    }

    // ======== PUBLIC UTILITY METHODS ========
    
    /// <summary>
    /// Force change to a specific weather type
    /// </summary>
    public void SetWeather(WeatherType weather)
    {
        if (currentWeather != weather)
        {
            currentWeather = weather;
            SetTargetCloudDensity();
            particlesNeedUpdate = true;
            weatherChangeTimer = Random.Range(minWeatherDuration, maxWeatherDuration);
        }
    }
    
    /// <summary>
    /// Force change to a specific season
    /// </summary>
    public void SetSeason(Season season)
    {
        currentSeason = season;
        currentDay = ((int)season * daysPerSeason) + 1;
        UpdateSeason();
    }
    
    /// <summary>
    /// Get the current progress through the year (0-1)
    /// </summary>
    public float GetYearProgress()
    {
        return (float)currentDay / 365f;
    }
    
    /// <summary>
    /// Get the current progress through the current season (0-1)
    /// </summary>
    public float GetSeasonProgress()
    {
        return (float)daysInCurrentSeason / daysPerSeason;
    }
    
    /// <summary>
    /// Manually refresh all cached components (useful for debugging)
    /// </summary>
    public void RefreshCachedComponents()
    {
        CacheCloudComponent();
        CacheLightComponents();
    }
}

} // namespace AdvancedWeatherSystem
