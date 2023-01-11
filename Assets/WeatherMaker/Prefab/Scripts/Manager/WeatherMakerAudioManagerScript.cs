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

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Audio manager script, controls all ambient sounds and audio zones
    /// </summary>
    public class WeatherMakerAudioManagerScript : MonoBehaviour
    {
        /// <summary>Change the volume of all weather maker sounds.</summary>
        [Range(0.0f, 1.0f)]
        [Tooltip("Change the volume of all weather maker sounds.")]
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("VolumeModifier")]
        private float volumeModifier = 1.0f;

        private float cachedVolumeModifier = 1.0f;
        private float cachedWeatherVolumeModifier = 1.0f;
        private float cachedAmbientVolumeModifier = 1.0f;

        /// <summary>
        /// Get or set the global volume modifier. Note that setting the volume modifier only sets the 'WeatherMakerAudioManagerScript' volume modifier in VolumeModifierDictionary.
        /// There may be other additional volume modifiers that get applied when you get the value of VolumeModifier.
        /// </summary>
        public float VolumeModifier
        {
            get
            {
                float baseVolume = 1.0f;
                foreach (float multiplier in VolumeModifierDictionary.Values)
                {
                    baseVolume *= multiplier;
                }
                return baseVolume;
            }
            set
            {
                VolumeModifierDictionary["WeatherMakerAudioManagerScript"] = volumeModifier = value;
            }
        }

        /// <summary>Multiplier for precipitation, wind and lightning</summary>
        [Tooltip("Multiplier for precipitation, wind and lightning")]
        [Range(0.0f, 1.0f)]
        public float WeatherVolumeModifier = 1.0f;

        /// <summary>Multiplier for sound zones (ambient sounds)</summary>
        [Tooltip("Multiplier for sound zones (ambient sounds)")]
        [Range(0.0f, 1.0f)]
        public float AmbientVolumeModifier = 1.0f;

        /// <summary>
        /// Allows adding additional volume modifiers by key
        /// </summary>
        [NonSerialized]
        public readonly System.Collections.Generic.Dictionary<string, float> VolumeModifierDictionary = new System.Collections.Generic.Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

        private void OnEnable()
        {
            WeatherMakerScript.EnsureInstance(this, ref instance);
        }

        private void LateUpdate()
        {
            VolumeModifier = volumeModifier;
            cachedVolumeModifier = VolumeModifier;
            cachedWeatherVolumeModifier = WeatherVolumeModifier * cachedVolumeModifier;
            cachedAmbientVolumeModifier = AmbientVolumeModifier * cachedVolumeModifier;
        }

        private void OnDestroy()
        {
            WeatherMakerScript.ReleaseInstance(ref instance);
        }

        /// <summary>
        /// Cached volume modifier, applied automatically to weather and ambient sounds
        /// </summary>
        public static float CachedVolumeModifier
        {
            get { return Instance == null ? 1.0f : Instance.cachedVolumeModifier; }
        }

        /// <summary>
        /// Cached weather volume modifier, applied automatically to Weather Maker weather sounds
        /// </summary>
        public static float CachedWeatherVolumeModifier
        {
            get { return Instance == null ? 1.0f : Instance.cachedWeatherVolumeModifier; }
        }

        /// <summary>
        /// Cached ambient volume modifier, applied automatically to Weather Maker ambient sounds
        /// </summary>
        public static float CachedAmbientVolumeModifier
        {
            get { return Instance == null ? 1.0f : Instance.cachedAmbientVolumeModifier; }
        }

        private static WeatherMakerAudioManagerScript instance;
        /// <summary>
        /// Shared instance of audio manager script
        /// </summary>
        public static WeatherMakerAudioManagerScript Instance
        {
            get { return WeatherMakerScript.FindOrCreateInstance(ref instance); }
        }
    }
}
