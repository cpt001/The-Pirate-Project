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
    /// Wind script, controls all wind handling
    /// </summary>
    public class WeatherMakerWindScript : MonoBehaviour
    {
        /// <summary>Whether the wind should anchor to each camera rendered.</summary>
        [Tooltip("Whether the wind should anchor to each camera rendered.")]
        public bool IsFirstPerson = true;

        /// <summary>Wind intensity (0 - 1). MaximumWindSpeed * WindIntensity = WindSpeed.</summary>
        [Tooltip("Wind intensity (0 - 1). MaximumWindSpeed * WindIntensity = WindSpeed.")]
        [Range(0.0f, 1.0f)]
        public float WindIntensity = 0.0f;

        /// <summary>
        /// External intensity multiplier
        /// </summary>
        [HideInInspector]
        [System.NonSerialized]
        public float ExternalIntensityMultiplier = 1.0f;

        /// <summary>The wind profile</summary>
        [Tooltip("The wind profile")]
        public WeatherMakerWindProfileScript WindProfile;

        /// <summary>
        /// The current wind velocity, not including turbulence and pulsing
        /// </summary>
        public Vector3 CurrentWindVelocity { get; private set; }

        /// <summary>
        /// Wind zone
        /// </summary>
        public WindZone WindZone { get; private set; }

        /// <summary>
        /// Wind audio source
        /// </summary>
        public WeatherMakerLoopingAudioSource AudioSourceWind { get; private set; }

        /// <summary>
        /// Allow notification of when the wind velocity changes
        /// </summary>
        public System.Action<Vector3> WindChanged { get; set; }

        /// <summary>
        /// Whether the wind direction is random. The wind direction is random if WindMaximumChangeRotation and WindChangeInterval are both greater than 0.
        /// </summary>
        public bool RandomWindDirection
        {
            get
            {
                return
                (
                    WindProfile.WindMaximumChangeRotation.Minimum > 0.0f &&
                    WindProfile.WindMaximumChangeRotation.Maximum >= WindProfile.WindMaximumChangeRotation.Minimum &&
                    WindProfile.WindChangeInterval.Maximum > 0.0f &&
                    WindProfile.WindChangeInterval.Maximum >= WindProfile.WindChangeInterval.Minimum
                );
            }
        }

        private float windNextChangeTime;
        private float lastWindIntensity;

        /// <summary>
        /// Set a new wind profile
        /// </summary>
        /// <param name="newProfile">New wind profile</param>
        /// <param name="transitionDelay">Transition delay</param>
        /// <param name="transitionDuration">Transition duration</param>
        public void SetWindProfileAnimated(WeatherMakerWindProfileScript newProfile, float transitionDelay, float transitionDuration)
        {
            WindProfile = newProfile;
            AnimateWindChange(newProfile, transitionDelay, transitionDuration);
        }

        private void Awake()
        {
            WindZone = GetComponent<WindZone>();
            if (!WeatherMakerScript.IsHeadlessMode)
            {
                AudioSourceWind = new WeatherMakerLoopingAudioSource(GetComponent<AudioSource>());
            }
        }

        private void AnimateWindChange(WeatherMakerWindProfileScript newProfile, float transitionDelay, float transitionDuration)
        {
            // update to new wind
            WeatherMakerWindProfileScript oldProfile = WindProfile;
            newProfile = newProfile ?? WindProfile;
            WindProfile = newProfile;

            // pick the next time to change
            windNextChangeTime = WindProfile.WindChangeInterval.Random();

            // previous values...
            float oldWindIntensity = WindIntensity;
            float oldTurbulence = WindZone.windTurbulence;
            float oldPulseMagnitude = WindZone.windPulseMagnitude;
            float oldPulseFrequency = WindZone.windPulseFrequency;
            Quaternion oldWindDirection = WindZone.transform.rotation;

            // animate to new values...
            float newWindIntensity = WindIntensity;
            if (WindProfile.AutoWindIntensity)
            {
                newWindIntensity = (WindProfile.WindSpeedRange.Maximum <= 0.0f ? 0.0f : WindProfile.WindSpeedRange.Random() / WindProfile.WindSpeedRange.Maximum);
            }
            float newTurbulence = WindProfile.WindTurbulenceRange.Random();
            float newPulseMagnitude = WindProfile.WindPulseMagnitudeRange.Random();
            float newPulseFrequency = WindProfile.WindPulseFrequencyRange.Random();
            Quaternion newWindDirection = oldWindDirection;

            // if random wind, pick a new direction from wind
            if (RandomWindDirection)
            {
                // 2D is set immediately
                switch (WeatherMakerScript.ResolveCameraMode())
                {
                    case CameraMode.OrthographicXY:
                    case CameraMode.OrthographicXZ:
                        int val = WeatherMakerRandomizer.Unity.Random(0, 2);
                        newWindDirection = Quaternion.Euler(0.0f, -90.0f + (180.0f * val), 0.0f);
                        break;

                    default:
                        // 3D is lerped over time
                        float xAxis = (WindProfile.AllowBlowUp ? WeatherMakerRandomizer.Unity.Random(-30.0f, 30.0f) : 0.0f);
                        newWindDirection = Quaternion.Euler(xAxis, WeatherMakerRandomizer.Unity.Random(0.0f, 360.0f), 0.0f);
                        break;
                }
            }

            FloatTween tween = TweenFactory.Tween("WeatherMakerWindScript_" + GetInstanceID(), 0.0f, 1.0f, transitionDuration, TweenScaleFunctions.Linear, (t) =>
            {
                WindIntensity = Mathf.Lerp(oldWindIntensity, newWindIntensity, t.CurrentValue);
                WindZone.windTurbulence = Mathf.Lerp(oldTurbulence, newTurbulence, t.CurrentValue);
                WindZone.windPulseMagnitude = Mathf.Lerp(oldPulseMagnitude, newPulseMagnitude, t.CurrentValue);
                WindZone.windPulseFrequency = Mathf.Lerp(oldPulseFrequency, newPulseFrequency, t.CurrentValue);
                WindZone.transform.rotation = Quaternion.Lerp(oldWindDirection, newWindDirection, t.CurrentValue);
            }, (t) =>
            {
                // if the old profile was a clone, clean it up
                if (oldProfile.name.IndexOf("(clone)", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    GameObject.Destroy(oldProfile);
                }
            });
            tween.Delay = transitionDelay;
        }

        private void UpdateWind()
        {
            if (WindProfile != null && (WindIntensity > 0.0f || WindProfile.AutoWindIntensity) && ExternalIntensityMultiplier > 0.0f &&
                WindProfile.WindSpeedRange.Maximum > 0.0f && WindProfile.WindMainMultiplier > 0.0f)
            {
                // check for wind change
                if (WindProfile.WindChangeInterval.Maximum > 0.0f && (windNextChangeTime -= Time.deltaTime) <= 0.0f)
                {
                    if (!WeatherMakerScript.IsHeadlessMode && AudioSourceWind != null)
                    {
                        AudioSourceWind.AudioSource.clip = (WindProfile.WindAudioClip == null ? AudioSourceWind.AudioSource.clip : WindProfile.WindAudioClip);
                    }
                    AnimateWindChange(null, 0.0f, WindProfile.WindChangeDuration.Random());
                }

                // determine wind intensity
                WindZone.windMain = WindProfile.WindSpeedRange.Maximum * WindIntensity * WindProfile.WindMainMultiplier * ExternalIntensityMultiplier;

                // update wind audio if wind intensity changed
                if (!WeatherMakerScript.IsHeadlessMode && WindZone.windMain != lastWindIntensity && AudioSourceWind != null)
                {
                    AudioSourceWind.Play(WindIntensity * WindProfile.WindSoundMultiplier);
                }
                lastWindIntensity = WindZone.windMain;

                Vector3 newVelocity = WindZone.transform.forward * WindZone.windMain;
                bool velocityChanged = newVelocity != CurrentWindVelocity;
                CurrentWindVelocity = newVelocity;
                if (velocityChanged && WindChanged != null)
                {
                    WindChanged(newVelocity);
                }
            }
            else
            {
                if (!WeatherMakerScript.IsHeadlessMode && AudioSourceWind != null)
                {
                    AudioSourceWind.Stop();
                }
                WindZone.windMain = WindZone.windTurbulence = WindZone.windPulseFrequency = WindZone.windPulseMagnitude = 0.0f;
                CurrentWindVelocity = Vector3.zero;
            }
            if (!WeatherMakerScript.IsHeadlessMode && AudioSourceWind != null)
            {
                AudioSourceWind.VolumeModifier = WeatherMakerAudioManagerScript.CachedWeatherVolumeModifier;
                AudioSourceWind.Update();
            }
        }

        private void LateUpdate()
        {
            UpdateWind();
        }

        private void OnEnable()
        {
            WeatherMakerScript.EnsureInstance(this, ref instance);
            if (!WeatherMakerScript.IsHeadlessMode && AudioSourceWind != null)
            {
                AudioSourceWind.Resume();
            }

            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreCull(CameraPreCull, this);
            }
        }

        private void OnDisable()
        {
        }

        private void OnDestroy()
        {
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPreCull(this);
            }
            WeatherMakerScript.ReleaseInstance(ref instance);
        }

        private void CameraPreCull(Camera camera)
        {
            if (IsFirstPerson && !WeatherMakerScript.ShouldIgnoreCamera(this, camera))
            {
                transform.position = camera.transform.position;
            }
        }

        private static WeatherMakerWindScript instance;
        /// <summary>
        /// Shared instance of wind script
        /// </summary>
        public static WeatherMakerWindScript Instance
        {
            get { return WeatherMakerScript.FindOrCreateInstance(ref instance); }
        }
    }
}