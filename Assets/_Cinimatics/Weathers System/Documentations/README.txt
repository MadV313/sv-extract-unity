🌦️ Advanced Weather System for Unity

A comprehensive, performance-optimized weather system for Unity with realistic day/night cycles, seasonal changes, and dynamic weather effects including dramatic thunderstorm sky darkening.

![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-blue)
![HDRP](https://img.shields.io/badge/HDRP-Required-orange)
![License](https://img.shields.io/badge/License-Unity%20Asset%20Store-green)

✨ Features

🌅 Dynamic Day/Night Cycle
- Realistic Sun & Moon Movement: Configurable latitude/longitude positioning
- Automatic Shadow Management: Smart shadow switching between sun and moon
- Lunar Phases: 30-day lunar cycle with visual moon scaling
- Performance Optimized: 30 FPS lighting updates for smooth transitions

🌦️ Advanced Weather System
-  5 Weather Types : Clear, Rain, Snow, Lightning, Thunderstorm
-  Seasonal Weather Patterns : Different probabilities per season
-  Smart Weather Transitions : Configurable duration and timing
-  Particle Effects : Optimized rain, snow, and lightning particles

⛈️ Dramatic Thunderstorm Effects
-  Automatic Sky Darkening : Realistic storm atmosphere
-  Dynamic Cloud Density : Heavy storm clouds (95% density)
-  Smooth Transitions : Performance-optimized sky exposure changes
-  Authentic Storm Feel : Combined rain + lightning effects

📅 Seasonal System
-  4 Seasons : Spring, Summer, Autumn, Winter
-  365-Day Calendar : Realistic yearly progression
-  Season-Based Weather : Different weather patterns per season
-  Customizable Duration : Configurable days per season

⚡ Performance Optimizations
-  Cached Components : Reduced GetComponent calls
-  Delta Checking : Only update when values change significantly
-  Optimized Update Rates : Different frequencies for different systems
-  Low Performance Mode : Reduced particle rates for better FPS

🎮 Quick Start

1. Setup Requirements
- Unity 2021.3 or newer
- HDRP (High Definition Render Pipeline)
- Visual Effect Graph (for particles)

2. Basic Setup
    1. Import the Weather System package
    2. Add the `CycleManager` script to an empty GameObject
    3. Assign your Sun and Moon lights
    4. Create Volume components for clouds and sky
    5. Assign particle systems for weather effects

3. Essential Volumes Setup

 Cloud Volume
```
Create > Volume > Global Volume
Add Override > Lighting > Volumetric Clouds
Assign to "Cloud Volume" field in CycleManager
```

 Sky Volume
```
Create > Volume > Global Volume  
Add Override > Sky > Physically Based Sky
Assign to "Sky Volume" field in CycleManager
```

🔧 Configuration

 Time & Progression
-  Current Time : 0-24 hour format
-  Time Speed : Multiplier for time progression
-  Season Speed : Controls seasonal changes
-  Days Per Season : Default 91 days (365 total)

 Weather Settings
-  Weather Duration : Min/Max hours between changes
-  Seasonal Probabilities : Customize weather chances per season
-  Cloud Densities : Different values per weather type
-  Transition Speeds : Control change smoothness

 Lighting Configuration
-  Sun Settings : Latitude, longitude, curves for natural movement
-  Moon Settings : Independent positioning and lunar cycle
-  Sky Exposure : Normal (13.0) vs Storm (8.0) values

 Performance Settings
-  Disable Cloud Updates : For better performance
-  Low Particle Mode : Reduced emission rates
-  Simplified Weather : Disable complex calculations

📋 Inspector Reference

 ⏱️ Time Progression
| Field | Description | Range |
|-------|-------------|-------|
| Current Time | Time of day in 24h format | 0-24 |
| Time Speed | Speed multiplier | 0+ |

 🌦️ Weather System
| Field | Description | Default |
|-------|-------------|---------|
| Current Weather | Active weather type | Clear |
| Min Weather Duration | Minimum hours between changes | 2h |
| Max Weather Duration | Maximum hours between changes | 8h |

 ☁️ Cloud Volume
| Field | Description | Range |
|-------|-------------|-------|
| Clear Cloud Density | Density for clear weather | 0.1 |
| Rain Cloud Density | Density for rain | 0.8 |
| Snow Cloud Density | Density for snow | 0.7 |
| Lightning Cloud Density | Density for lightning | 0.6 |
| Thunderstorm Cloud Density | Density for storms | 0.95 |

### 🌌 Sky/Atmosphere Control
| Field | Description | Recommended |
|-------|-------------|-------------|
| Normal Sky Exposure | Clear weather brightness | 13.0 |
| Storm Sky Exposure | Thunderstorm darkness | 8.0 |
| Sky Transition Speed | Change speed | 2.0 |

🎯 Usage Examples

 Force Weather Change
```csharp
CycleManager weatherSystem = FindObjectOfType<CycleManager>();
weatherSystem.SetWeather(WeatherType.Thunderstorm);
```

 Change Season
```csharp
weatherSystem.SetSeason(Season.Winter);
```

 Get Current Progress
```csharp
float yearProgress = weatherSystem.GetYearProgress(); // 0-1
float seasonProgress = weatherSystem.GetSeasonProgress(); // 0-1
```

 Refresh Components
```csharp
weatherSystem.RefreshCachedComponents();
```

🏗️ Architecture

 Core Components
-  CycleManager : Main controller script
-  Weather Enums : Season and WeatherType definitions
-  Performance Cache : Optimized component references
-  Smooth Transitions : Interpolated value changes

 Update Frequencies
-  Time Progression : Every frame (essential)
-  Lighting Updates : 30 FPS (smooth but optimized)
-  Weather System : 10 FPS (responsive)
-  Cloud/Sky Updates : Only when needed

⚠️ Important Notes

 Sun Light Control
- Sun intensity and temperature are  manually controlled 
- Sun position, rotation, and shadows are  script-controlled 
- This allows for custom sun setups while maintaining automation

 Moon Light Control
- Fully automated including intensity and temperature
- Realistic lunar cycle with phase-based brightness
- 30-day lunar calendar system

 Performance Considerations
- Use "Low Particle Mode" on mobile devices
- Consider "Disable Cloud Updates" for very low-end hardware
- "Simplified Weather Intensity" reduces calculations

🔍 Troubleshooting

 Sky Not Darkening During Storms
1. Ensure Sky Volume is assigned
2. Check Physically Based Sky component exists
3. Verify exposure values are different enough
4. Call `RefreshCachedComponents()` if needed

 Particles Not Showing
1. Check particle systems are assigned
2. Verify Visual Effect Graph is installed
3. Ensure particle materials are assigned
4. Check "Low Particle Mode" setting

 Performance Issues
1. Enable "Low Particle Mode"
2. Consider "Disable Cloud Updates"
3. Reduce "Sky Transition Speed"
4. Check Update frequencies in profiler

📝 Version History

 v1.0.0
- Initial release with full weather system
- Day/night cycle implementation
- Seasonal system
- Performance optimizations
- Thunderstorm sky effects

📞 Support

For support, questions, or feature requests:
- Check the troubleshooting section above
- Review the configuration documentation
- Ensure all required components are properly assigned
- Author : aikpak24@gmail.com

📄 License

This asset is licensed under the Unity Asset Store End User License Agreement.

---

Created with ❤️ for the Unity community
