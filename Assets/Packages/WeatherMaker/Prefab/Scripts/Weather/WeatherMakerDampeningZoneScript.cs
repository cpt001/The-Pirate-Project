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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Dampening zone, reduces precipitation intensity, sound and light
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WeatherMakerDampeningZoneScript : MonoBehaviour
    {
        /// <summary>How much to reduce precipitation and weather intensity.</summary>
        [Tooltip("How much to reduce precipitation and weather intensity.")]
        [Range(0.0f, 1.0f)]
        public float IntensityDampening = 0.0f;

        /// <summary>How much to reduce sounds.</summary>
        [Tooltip("How much to reduce sounds.")]
        [Range(0.0f, 1.0f)]
        public float SoundDampening = 0.2f;

        /// <summary>How much to reduce light from lightning.</summary>
        [Tooltip("How much to reduce light from lightning.")]
        [Range(0.0f, 1.0f)]
        public float LightDampening = 0.0f;

        /// <summary>Transition duration in seconds to new dampening values.</summary>
        [Tooltip("Transition duration in seconds to new dampening values.")]
        [Range(0.0f, 10.0f)]
        public float TransitionDuration = 3.0f;

        private static int triggers;

        private void Start()
        {

#if UNITY_EDITOR

            Collider col = GetComponent<Collider>();
            if (col == null || !col.isTrigger)
            {
                Debug.LogError("WeatherMakerDampeningZoneScript only works with trigger colliders.");
            }

#endif

            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
        }

        private void OnEnable()
        {
            triggers = 0;
        }

        private void SceneManager_sceneUnloaded(UnityEngine.SceneManagement.Scene arg0)
        {
            triggers = 0;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (gameObject.activeInHierarchy && enabled && WeatherMakerScript.IsLocalPlayer(other.transform) && ++triggers == 1)
            {
                // if this is the first trigger entered, run it
                TweenFactory.Tween("WeatherMakerDampeningZoneScript", 0.0f, 1.0f, TransitionDuration, TweenScaleFunctions.Linear, (t) =>
                {
                    if (WeatherMakerAudioManagerScript.Instance != null)
                    {
                        float currentValue;
                        if (!WeatherMakerAudioManagerScript.Instance.VolumeModifierDictionary.TryGetValue("WeatherMakerDampeningZoneScript", out currentValue))
                        {
                            currentValue = 1.0f;
                        }
                        WeatherMakerAudioManagerScript.Instance.VolumeModifierDictionary["WeatherMakerDampeningZoneScript"] = Mathf.Lerp(currentValue, SoundDampening, t.CurrentValue);
                    }
                    if (WeatherMakerScript.Instance != null)
                    {
                        float currentValue;
                        if (!WeatherMakerScript.Instance.IntensityModifierDictionary.TryGetValue("WeatherMakerDampeningZoneScript", out currentValue))
                        {
                            currentValue = 1.0f;
                        }
                        WeatherMakerScript.Instance.IntensityModifierDictionary["WeatherMakerDampeningZoneScript"] = Mathf.Lerp(currentValue, IntensityDampening, t.CurrentValue);
                    }
                    if (WeatherMakerThunderAndLightningScript.Instance != null)
                    {
                        WeatherMakerThunderAndLightningScript.Instance.LightningBoltScript.LightParameters.LightIntensity =
                            Mathf.Lerp(WeatherMakerThunderAndLightningScript.Instance.LightningBoltScript.LightParameters.LightIntensity, LightDampening, t.CurrentValue);
                    }
                });
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // if this is the last trigger exited, run it
            if (gameObject.activeInHierarchy && enabled && WeatherMakerScript.IsLocalPlayer(other.transform) && --triggers == 0)
            {
                TweenFactory.Tween("WeatherMakerDampeningZoneScript", 0.0f, 1.0f, TransitionDuration, TweenScaleFunctions.Linear, (t) =>
                {
                    if (WeatherMakerAudioManagerScript.Instance != null)
                    {
                        float currentValue;
                        if (WeatherMakerAudioManagerScript.Instance.VolumeModifierDictionary.TryGetValue("WeatherMakerDampeningZoneScript", out currentValue))
                        {
                            WeatherMakerAudioManagerScript.Instance.VolumeModifierDictionary["WeatherMakerDampeningZoneScript"] = Mathf.Lerp(currentValue, 1.0f, t.CurrentValue);
                        }
                    }
                    if (WeatherMakerScript.Instance != null)
                    {
                        float currentValue;
                        if (WeatherMakerScript.Instance.IntensityModifierDictionary.TryGetValue("WeatherMakerDampeningZoneScript", out currentValue))
                        {
                            WeatherMakerScript.Instance.IntensityModifierDictionary["WeatherMakerDampeningZoneScript"] = Mathf.Lerp(currentValue, 1.0f, t.CurrentValue);
                        }
                    }
                    if (WeatherMakerThunderAndLightningScript.Instance != null)
                    {
                        WeatherMakerThunderAndLightningScript.Instance.LightningBoltScript.LightParameters.LightIntensity =
                            Mathf.Lerp(WeatherMakerThunderAndLightningScript.Instance.LightningBoltScript.LightParameters.LightIntensity, 1.0f, t.CurrentValue);
                    }
                }, (t) =>
                {
                    if (WeatherMakerAudioManagerScript.Instance != null)
                    {
                        WeatherMakerAudioManagerScript.Instance.VolumeModifierDictionary.Remove("WeatherMakerSoundDamperZoneScript");
                    }
                    if (WeatherMakerScript.Instance != null)
                    {
                        WeatherMakerScript.Instance.IntensityModifierDictionary.Remove("WeatherMakerIntensityDamperZoneScript");
                    }
                });
            }
        }
    }
}