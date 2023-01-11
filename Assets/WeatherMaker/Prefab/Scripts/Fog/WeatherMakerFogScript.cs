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
using System.Collections;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Base fog script with type of profile
    /// </summary>
    /// <typeparam name="T">Type of fog profile</typeparam>
    public abstract class WeatherMakerFogScript<T> : MonoBehaviour where T : WeatherMakerFogProfileScript
    {
        /// <summary>Fog profile</summary>
        [Header("Fog profile and material")]
        [Tooltip("Fog profile")]
        public T FogProfile;

        /// <summary>Fog material</summary>
        [Tooltip("Fog material")]
        public Material FogMaterial;

        private bool initialized;

        /// <summary>
        /// Shortcut to fog profile density
        /// </summary>
        public float FogDensity
        {
            get { return FogProfile == null ? 0.0f : FogProfile.FogDensity; }
            set { if (FogProfile != null) { FogProfile.FogDensity = value; } }
        }

        /// <summary>
        /// Set a new fog density over a period of time - if set to 0, game object will be disabled at end of transition
        /// </summary>
        /// <param name="fromDensity">Start of new fog density</param>
        /// <param name="toDensity">End of new fog density</param>
        /// <param name="transitionDuration">How long to transition to the new fog density in seconds</param>
        public void TransitionFogDensity(float fromDensity, float toDensity, float transitionDuration)
        {
            if (!isActiveAndEnabled)
            {
                Debug.LogError("Fog script must be enabled to show fog");
                return;
            }

            FogProfile.FogDensity = fromDensity;
            TweenFactory.Tween("WeatherMakerFog_" + GetInstanceID(), fromDensity, toDensity, transitionDuration, TweenScaleFunctions.Linear, (v) =>
            {
                FogProfile.FogDensity = v.CurrentValue;
            }, (v) =>
            {
                FogProfile.FogDensity = v.CurrentValue;
            });
        }

        /// <summary>
        /// Create a new fog profile and begin animating to the new profile settings
        /// </summary>
        /// <param name="toProfile">New fog profile to transition to</param>
        /// <param name="transitionDelay">Transition delay in seconds</param>
        /// <param name="transitionDuration">Transition duration in seconds</param>
        /// <param name="newDensity">Density for the new profile or null to use profile density</param>
        /// <param name="tweenKey">Tween key</param>
        /// <returns>The newly created fog profile that is being transitioned to (a copy of toProfile)</returns>
        public virtual T ShowFogAnimated(T toProfile, float transitionDelay, float transitionDuration, float? newDensity = null, string tweenKey = null)
        {
            if (FogProfile == null)
            {
                Debug.LogError("Fog profile needs to be set before ShowFogAnimated can be called");
                return null;
            }

            T toProfileInstance = ScriptableObject.Instantiate(toProfile);
            if (newDensity != null)
            {
                toProfileInstance.FogDensity = newDensity.Value;
            }
            FogProfile.CopyStateTo(toProfileInstance);
            tweenKey = (tweenKey ?? string.Empty);

            // animate density, color and noise properties, just leave the rest at final values

            // cleanup old profile if it was temporary
            T oldProfile = FogProfile;
            FogProfile = toProfileInstance;

            // make copies of final value for animatable properties
            float newFogDensity = toProfileInstance.FogDensity;
            float newFogFactorMultiplier = toProfileInstance.FogFactorMultiplier;
            Color newFogColor = toProfileInstance.FogColor;
            Color newFogEmissionColor = toProfileInstance.FogEmissionColor;
            float newFogLightAbsorption = toProfileInstance.fogLightAbsorption;
            float newMaxFogFactor = toProfileInstance.MaxFogFactor;
            int newFogNoiseSampleCount = toProfileInstance.FogNoiseSampleCount;
            float newFogNoiseAdder = toProfileInstance.FogNoiseAdder;
            float newFogNoiseMultiplier = toProfileInstance.FogNoiseMultiplier;
            float newFogNoiseScale = toProfileInstance.FogNoiseScale;
            Vector3 newFogNoiseVelocity = toProfileInstance.FogNoiseVelocity;
            Vector4 newFogNoiseVelocityRotation = toProfileInstance.FogNoiseVelocityRotation;
            float newFogShadowBrightness = toProfileInstance.FogShadowBrightness;
            float newFogShadowDecay = toProfileInstance.FogShadowDecay;
            float newFogShadowDither = toProfileInstance.FogShadowDither;
            float newFogShadowMaxRayLength = toProfileInstance.FogShadowMaxRayLength;
            float newFogShadowMultiplier = toProfileInstance.FogShadowMultiplier;
            float newFogShadowPower = toProfileInstance.FogShadowPower;
            int newFogShadowSampleCount = toProfileInstance.FogShadowSampleCount;
            float newDitherLevel = toProfileInstance.DitherLevel;

            float oldFogDensity = oldProfile.FogDensity;
            float oldFogFactorMultiplier = oldProfile.FogFactorMultiplier;
            T transitionFromProfile = (oldFogDensity == 0.0f ? toProfileInstance : oldProfile);
            bool newProfileHasNoise = toProfileInstance.HasNoise;
            bool oldProfileHasNoise = oldProfile.HasNoise;
            bool bothProfileHaveNoise = newProfileHasNoise && oldProfileHasNoise;
            bool transitionShadow = (oldProfile.FogShadowSampleCount > 0 && toProfileInstance.FogShadowSampleCount > 0);

            // transition to a new profile without noise, copy old profile noise settings
            if (oldProfileHasNoise && !newProfileHasNoise)
            {
                toProfileInstance.FogNoiseVelocity = transitionFromProfile.FogNoiseVelocity;
                toProfileInstance.FogNoiseVelocityRotation = transitionFromProfile.FogNoiseVelocityRotation;
                toProfileInstance.FogNoiseAdder = transitionFromProfile.FogNoiseAdder;
                toProfileInstance.FogNoiseMultiplier = transitionFromProfile.FogNoiseMultiplier;
                toProfileInstance.FogNoiseSampleCount = transitionFromProfile.FogNoiseSampleCount;
                toProfileInstance.FogNoiseScale = transitionFromProfile.FogNoiseScale;
            }

            FloatTween tween = TweenFactory.Tween("WeatherMakerFogScript_" + GetInstanceID() + tweenKey, 0.0f, 1.0f, transitionDuration, TweenScaleFunctions.Linear, (ITween<float> c) =>
            {
                float progress = c.CurrentValue;
                toProfileInstance.FogDensity = Mathf.Lerp(oldFogDensity, newFogDensity, progress);
                toProfileInstance.FogFactorMultiplier = Mathf.Lerp(oldFogFactorMultiplier, newFogFactorMultiplier, progress);
                toProfileInstance.FogColor = Color.Lerp(transitionFromProfile.FogColor, newFogColor, progress);
                toProfileInstance.FogEmissionColor = Color.Lerp(transitionFromProfile.FogEmissionColor, newFogEmissionColor, progress);
                toProfileInstance.fogLightAbsorption = Mathf.Lerp(transitionFromProfile.fogLightAbsorption, newFogLightAbsorption, progress);
                toProfileInstance.MaxFogFactor = Mathf.Lerp(transitionFromProfile.MaxFogFactor, newMaxFogFactor, progress);
                toProfileInstance.DitherLevel = Mathf.Lerp(transitionFromProfile.DitherLevel, newDitherLevel, progress);

                // if both profiles have fog noise, animate noise
                if (bothProfileHaveNoise)
                {
                    toProfileInstance.FogNoiseVelocity = Vector3.Lerp(transitionFromProfile.FogNoiseVelocity, newFogNoiseVelocity, progress);
                    toProfileInstance.FogNoiseVelocityRotation = Vector4.Lerp(transitionFromProfile.FogNoiseVelocityRotation, newFogNoiseVelocityRotation, progress);
                    toProfileInstance.FogNoiseAdder = Mathf.Lerp(transitionFromProfile.FogNoiseAdder, newFogNoiseAdder, progress);
                    toProfileInstance.FogNoiseMultiplier = Mathf.Lerp(transitionFromProfile.FogNoiseMultiplier, newFogNoiseMultiplier, progress);
                    toProfileInstance.FogNoiseSampleCount = (int)Mathf.Lerp(transitionFromProfile.FogNoiseSampleCount, newFogNoiseSampleCount, progress);
                    toProfileInstance.FogNoiseScale = Mathf.Lerp(transitionFromProfile.FogNoiseScale, newFogNoiseScale, progress);
                }
                // if only new profile has noise, animate to noise from no noise
                else if (newProfileHasNoise)
                {
                    toProfileInstance.fogNoisePercent = progress;
                }
                // if only old profile has noise, animate from noise to no noise
                else if (oldProfileHasNoise)
                {
                    toProfileInstance.fogNoisePercent = 1.0f - progress;
                }

                if (transitionShadow)
                {
                    toProfileInstance.FogShadowBrightness = Mathf.Lerp(transitionFromProfile.FogShadowBrightness, newFogShadowBrightness, progress);
                    toProfileInstance.FogShadowDecay = Mathf.Lerp(transitionFromProfile.FogShadowDecay, newFogShadowDecay, progress);
                    toProfileInstance.FogShadowDither = Mathf.Lerp(transitionFromProfile.FogShadowDither, newFogShadowDither, progress);
                    toProfileInstance.FogShadowMaxRayLength = Mathf.Lerp(transitionFromProfile.FogShadowMaxRayLength, newFogShadowMaxRayLength, progress);
                    toProfileInstance.FogShadowMultiplier = Mathf.Lerp(transitionFromProfile.FogShadowMultiplier, newFogShadowMultiplier, progress);
                    toProfileInstance.FogShadowPower = Mathf.Lerp(transitionFromProfile.FogShadowPower, newFogShadowPower, progress);
                    toProfileInstance.FogShadowSampleCount = (int)Mathf.Lerp(transitionFromProfile.FogShadowSampleCount, newFogShadowSampleCount, progress);
                }
                // note do not animate the dither magic property!
            }, (ITween<float> c) =>
            {
                // if the old profile was a clone, clean it up
                if (oldProfile.name.IndexOf("clone", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    GameObject.Destroy(oldProfile);
                }

                // if new profile has no noise and old profile had noise, disable noise by setting noise multiplier to 0 (save performance).
                if (oldProfileHasNoise && !newProfileHasNoise)
                {
                    toProfileInstance.FogNoiseMultiplier = 0.0f;
                }
            });
            tween.Delay = transitionDelay;
            return toProfileInstance;
        }

        /// <summary>
        /// Awake
        /// </summary>
        protected virtual void Awake()
        {
        }

        /// <summary>
        /// Start
        /// </summary>
        protected virtual void Start()
        {
            
        }

        /// <summary>
        /// Update fog material from the current fog profile
        /// </summary>
        protected virtual void UpdateFogMaterialFromProfile()
        {
            FogProfile.UpdateMaterialProperties(FogMaterial, null, false);
            UpdateWind();
        }

        /// <summary>
        /// LateUpdate
        /// </summary>
        protected virtual void LateUpdate()
        {
            if (!initialized)
            {
                Initialize();
                OnInitialize();
                initialized = true;
            }
            UpdateFogMaterialFromProfile();
            if (FogProfile != null)
            {
                FogProfile.Update();
            }
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        protected virtual void OnDestroy()
        {
            // if the fog profile was a clone, clean it up
            if (FogProfile != null && FogProfile.name.IndexOf("clone", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                GameObject.Destroy(FogProfile);
            }
        }

        /// <summary>
        /// OnEnable
        /// </summary>
        protected virtual void OnEnable()
        {
        }

        /// <summary>
        /// OnDisable
        /// </summary>
        protected virtual void OnDisable()
        {
        }

        /// <summary>
        /// OnWillRenderObject
        /// </summary>
        protected virtual void OnWillRenderObject()
        {
        }

        /// <summary>
        /// OnBecameVisible
        /// </summary>
        protected virtual void OnBecameVisible()
        {
        }

        /// <summary>
        /// OnBecameInvisible
        /// </summary>
        protected virtual void OnBecameInvisible()
        {
        }

        /// <summary>
        /// OnInitialize
        /// </summary>
        protected virtual void OnInitialize()
        {

        }

        /// <summary>
        /// UpdateWind
        /// </summary>
        protected void UpdateWind()
        {
            if (FogProfile.WindEffectsFogNoiseVelocity)
            {
                WeatherMakerWindScript wind = WeatherMakerWindScript.Instance;
                if (wind != null)
                {
                    Vector3 fogNoiseVelocity = Vector3.zero;
                    if (wind.WindProfile != null)
                    {
                        fogNoiseVelocity += (wind.CurrentWindVelocity * wind.WindProfile.FogVelocityMultiplier * 100.0f);
                    }
                    FogProfile.FogNoiseVelocity = fogNoiseVelocity;
                }
            }
        }

        private void Initialize()
        {
            if (WeatherMakerScript.Instance == null)
            {
                return;
            }

            if (FogProfile == null)
            {
                FogProfile = WeatherMakerScript.Instance.LoadResource<T>("WeatherMakerFogProfile_Default");
            }

#if UNITY_EDITOR

            if (Application.isPlaying)
            {

#endif

                // clone fog material and profile
                if (FogMaterial != null)
                {
                    FogMaterial = new Material(FogMaterial);
                }
                FogProfile = ScriptableObject.Instantiate(FogProfile);

#if UNITY_EDITOR

            }

#endif

        }
    }
}
