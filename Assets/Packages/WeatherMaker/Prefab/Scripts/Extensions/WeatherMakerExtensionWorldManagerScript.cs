//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 
// *** A NOTE ABOUT PIRACY ***
// 
// If you got this asset from a pirate site, please consider buying it from the Unity asset store at https://assetstore.unity.com/packages/slug/60955?aid=1011lGnL. This asset is only legally available from the Unity Asset Store.
// 
// I'm a single indie dev supporting my family by spending hundreds and thousands of hours on this and other assets. It's very offensive, rude and just plain evil to steal when I (and many others) put so much hard work into the software.
// 
// Thank you.
//
// *** END NOTE ABOUT PIRACY ***
//

using UnityEngine;

#if WORLDAPI_PRESENT

using WAPI;

#endif

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// World API extension script
    /// </summary>
    [AddComponentMenu("Weather Maker/Extensions/World Manager (WAPI)", 7)]
    public class WeatherMakerExtensionWorldManagerScript : WeatherMakerExtensionRainSnowSeasonScript

#if WORLDAPI_PRESENT

        <WorldManager>, IWorldApiChangeHandler

#else

        <UnityEngine.MonoBehaviour>

#endif

    {

#if WORLDAPI_PRESENT
        
        /// <summary>The minimum rain power.</summary>
        [Tooltip("The minimum rain power.")]
        [Range(0.0f, 1.0f)]
        public float MinRainPower = 0.0f;

        /// <summary>The minimum snow power.</summary>
        [Tooltip("The minimum snow power.")]
        [Range(0.0f, 1.0f)]
        public float MinSnowPower = 0.0f;

        /// <summary>How much cloud cover reduces specular highlights from directional light coming off the water. 0 for none, higher for more reduction.</summary>
        [Tooltip("How much cloud cover reduces specular highlights from directional light coming off the water. 0 for none, higher for more reduction.")]
        [Range(0.0f, 4.0f)]
        public float CloudCoverWaterSpecularPower = 2.0f;

        /// <summary>How much cloud cover reduces reflections coming off the water. 0 for none, higher for more reduction.</summary>
        [Tooltip("How much cloud cover reduces reflections coming off the water. 0 for none, higher for more reduction.")]
        [Range(0.0f, 4.0f)]
        public float CloudCoverWaterReflectionPower = 2.0f;

        /// <summary>Unit vector for cloud direction</summary>
        [Tooltip("Unit vector for cloud direction")]
        public Vector3 CloudDirection = Vector3.right;

        private void OnEnable()
        {
            ConnectToWorldAPI();
        }

        private void OnDisable()
        {
            DisconnectFromWorldAPI();
        }

        /// <summary>
        /// Start listening to world api messaged
        /// </summary>
        private void ConnectToWorldAPI()
        {
            WorldManager.Instance.AddListener(this);
        }

        /// <summary>
        /// Stop listening to world api messages
        /// </summary>
        private void DisconnectFromWorldAPI()
        {
            WorldManager.Instance.RemoveListener(this);
        }

        /// <summary>
        /// This method is called when the class has been added as a listener, and something has changed 
        /// one of the WAPI settings.
        /// 
        /// Use the HasChanged method to work out what was changed and respond accordingly. 
        /// 
        /// NOTE : As the majority of the World API values are properties, setting something 
        /// is as easy as reading its value, and setting a property will cause another
        /// OnWorldChanged event to be raised.
        /// 
        /// </summary>
        /// <param name="args"></param>
        public void OnWorldChanged(WorldChangeArgs args)
        {
            if (WeatherMakerScript.Instance == null)
            {
                return;
            }
            WorldManager m = args.manager;

            if (args.HasChanged(WorldConstants.WorldChangeEvents.GameTimeChanged))
            {
                WeatherMakerDayNightCycleManagerScript dayNight = WeatherMakerDayNightCycleManagerScript.Instance;
                if (dayNight != null && dayNight.DayNightProfile != null)
                {
                    dayNight.DateTime = m.GameTime.ToLocalTime();
                }
            }
            else if (args.HasChanged(WorldConstants.WorldChangeEvents.CloudsChanged))
            {
                WeatherMakerFullScreenCloudsScript s = WeatherMakerFullScreenCloudsScript.Instance;
                if (s != null && s.CloudProfile != null)
                {
                    s.CloudProfile.CloudHeight.LastValue = m.CloudMinHeight;
                    s.CloudProfile.CloudHeightTop.LastValue = m.CloudMaxHeight;
                    if (s.CloudProfile.CloudLayerVolumetric1 != null && WeatherMakerScript.Instance.PerformanceProfile.EnableVolumetricClouds)
                    {
                        s.CloudProfile.WeatherMapCloudCoverageVelocity = CloudDirection * m.CloudSpeed;
                        s.CloudProfile.CloudLayerVolumetric1.CloudCover = new RangeOfFloats { Minimum = m.CloudPower, Maximum = m.CloudPower };
                    }
                }
            }
            else if (args.HasChanged(WorldConstants.WorldChangeEvents.SeasonChanged))
            {
            }
            else if (args.HasChanged(WorldConstants.WorldChangeEvents.FogChanged))
            {
                WeatherMakerFullScreenFogScript s = WeatherMakerFullScreenFogScript.Instance;
                if (s != null && s.FogProfile != null)
                {
                    s.FogProfile.FogEndDepth = m.FogDistanceMax;
                    s.FogProfile.FogDensity = m.FogDistancePower;
                    s.FogProfile.FogHeight = m.FogHeightMax;
                    s.FogProfile.FogHeightFalloffPower = m.FogHeightPower;
                }
            }
            else if (args.HasChanged(WorldConstants.WorldChangeEvents.HailChanged))
            {
                WeatherMakerPrecipitationManagerScript p = WeatherMakerScript.Instance.PrecipitationManager as WeatherMakerPrecipitationManagerScript;
                if (p != null)
                {
                    p.HailScript.Intensity = m.HailPower;
                }
            }
            else if (args.HasChanged(WorldConstants.WorldChangeEvents.LatLngChanged))
            {
                WeatherMakerDayNightCycleManagerScript d = WeatherMakerDayNightCycleManagerScript.Instance;
                if (d != null)
                {
                    d.Latitude = m.Latitude;
                    d.Longitude = m.Longitude;
                }
            }
            else if (args.HasChanged(WorldConstants.WorldChangeEvents.RainChanged))
            {
                WeatherMakerPrecipitationManagerScript p = WeatherMakerScript.Instance.PrecipitationManager as WeatherMakerPrecipitationManagerScript;
                if (p != null)
                {
                    p.RainScript.Intensity = m.RainPower;
                }
            }
            else if (args.HasChanged(WorldConstants.WorldChangeEvents.SeaChanged))
            {
            }
            else if (args.HasChanged(WorldConstants.WorldChangeEvents.SnowChanged))
            {
                WeatherMakerPrecipitationManagerScript p = WeatherMakerScript.Instance.PrecipitationManager as WeatherMakerPrecipitationManagerScript;
                if (p != null)
                {
                    p.SnowScript.Intensity = m.SnowPower;
                }
            }
            else if (args.HasChanged(WorldConstants.WorldChangeEvents.TempAndHumidityChanged))
            {
            }
            else if (args.HasChanged(WorldConstants.WorldChangeEvents.ThunderChanged))
            {
                WeatherMakerThunderAndLightningManagerScript t = WeatherMakerThunderAndLightningManagerScript.Instance;
                if (t != null)
                {
                    t.ThunderAndLightningScript.LightningIntenseProbability = m.ThunderPower * 0.5f;
                    t.ThunderAndLightningScript.LightningIntervalTimeRange = new RangeOfFloats { Minimum = Mathf.Lerp(5.0f, 30.0f, m.ThunderPower), Maximum = Mathf.Lerp(10.0f, 120.0f, m.ThunderPower) };
                }
            }
            else if (args.HasChanged(WorldConstants.WorldChangeEvents.VolumeChanged))
            {
                WeatherMakerAudioManagerScript a = WeatherMakerAudioManagerScript.Instance;
                if (a != null)
                {
                    a.WeatherVolumeModifier = m.VolumeWeather;
                    a.AmbientVolumeModifier = m.VolumeEnvironment;
                }
            }
            else if (args.HasChanged(WorldConstants.WorldChangeEvents.WindChanged))
            {
                WeatherMakerWindManagerScript w = WeatherMakerWindManagerScript.Instance;
                if (w != null)
                {
                    w.WindScript.WindIntensity = m.WindSpeed;
                    w.WindScript.WindProfile.WindTurbulenceRange = new RangeOfFloats { Minimum = m.WindTurbulence, Maximum = m.WindTurbulence };
                    w.WindScript.WindProfile.WindMaximumChangeRotation = new RangeOfFloats();
                    w.WindScript.transform.rotation = Quaternion.AngleAxis(m.WindDirection, Vector3.up);
                }
            }
        }

        protected override void OnUpdateRain(float rain)
        {
            WorldManager.Instance.RainPowerTerrain = rain;
        }

        protected override void OnUpdateSnow(float snow)
        {
            WorldManager.Instance.SnowPowerTerrain = snow;
        }

        protected override void OnUpdateSeason(float season)
        {
            WorldManager.Instance.Season = season;
        }

#endif

    }
}

