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
    /// Wind profile script, contains all fields for configuring wind
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherMakerWindProfile", menuName = "WeatherMaker/Wind Profile", order = 25)]
    [System.Serializable]
    public class WeatherMakerWindProfileScript : WeatherMakerBaseScriptableObjectScript
    {
        /// <summary>The range of wind speed intensities. The wind zone wind main is set to WindIntensity * MaximumWindSpeed * WindMainMultiplier.</summary>
        [MinMaxSlider(0.0f, 1000.0f, "The range of wind speed intensities. The wind zone wind main is set to WindIntensity * MaximumWindSpeed * WindMainMultiplier.")]
        public RangeOfFloats WindSpeedRange = new RangeOfFloats { Minimum = 0.0f, Maximum = 100.0f };

        /// <summary>Whether to re-calculate wind intensity when wind changes. Default is true.</summary>
        [Tooltip("Whether to re-calculate wind intensity when wind changes. Default is true.")]
        public bool AutoWindIntensity = true;

        /// <summary>The maximum rotation the wind can change in degrees. For 2D, non-zero means random wind left or right.</summary>
        [MinMaxSlider(0.0f, 360.0f, "The maximum rotation the wind can change in degrees. For 2D, non-zero means random wind left or right.")]
        public RangeOfFloats WindMaximumChangeRotation = new RangeOfFloats { Minimum = 15.0f, Maximum = 60.0f };

        /// <summary>Random percent of total transition duration that is spent changing the wind rotation. Lower values rotate the wind faster.</summary>
        [MinMaxSlider(0.0f, 1.0f, "Random percent of total transition duration that is spent changing the wind rotation. Lower values rotate the wind faster.")]
        public RangeOfFloats WindChangeRotationDurationPercent = new RangeOfFloats { Minimum = 0.2f, Maximum = 0.5f };

        /// <summary>Multiply the wind zone wind main by this value.</summary>
        [Tooltip("Multiply the wind zone wind main by this value.")]
        [Range(0.0f, 1.0f)]
        public float WindMainMultiplier = 0.01f;

        /// <summary>Wind turbulence range - set to a maximum 0 for no random turbulence.</summary>
        [MinMaxSlider(0.0f, 1000.0f, "Wind turbulence range - set to a maximum 0 for no random turbulence.")]
        public RangeOfFloats WindTurbulenceRange = new RangeOfFloats { Minimum = 0.0f, Maximum = 100.0f };

        /// <summary>Wind pulse magnitude range - set to a maximum of 0 for no random pulse magnitude.</summary>
        [MinMaxSlider(0.0f, 100.0f, "Wind pulse magnitude range - set to a maximum of 0 for no random pulse magnitude.")]
        public RangeOfFloats WindPulseMagnitudeRange = new RangeOfFloats { Minimum = 2.0f, Maximum = 8.0f };

        /// <summary>Wind pulse frequency range - set to a maximum of 0 for no random pulse frequency.</summary>
        [MinMaxSlider(0.0f, 1.0f, "Wind pulse frequency range - set to a maximum of 0 for no random pulse frequency.")]
        public RangeOfFloats WindPulseFrequencyRange = new RangeOfFloats { Minimum = 0.01f, Maximum = 0.1f };

        /// <summary>Whether random wind can blow upwards. Default is false.</summary>
        [Tooltip("Whether random wind can blow upwards. Default is false.")]
        public bool AllowBlowUp = false;

        /// <summary>Additional sound volume multiplier for the wind</summary>
        [Tooltip("Additional sound volume multiplier for the wind")]
        [Range(0.0f, 2.0f)]
        public float WindSoundMultiplier = 0.5f;

        /// <summary>How often the wind speed and direction changes (minimum and maximum change interval in seconds). Set to 0 for no change.</summary>
        [MinMaxSlider(0.0f, 120.0f, "How often the wind speed and direction changes (minimum and maximum change interval in seconds). Set to 0 for no change.")]
        public RangeOfFloats WindChangeInterval = new RangeOfFloats { Minimum = 10.0f, Maximum = 30.0f };

        /// <summary>How fast the wind changes once a change is begun. Set to 0 for instant change.</summary>
        [MinMaxSlider(0.0f, 120.0f, "How fast the wind changes once a change is begun. Set to 0 for instant change.")]
        public RangeOfFloats WindChangeDuration = new RangeOfFloats { Minimum = 3.0f, Maximum = 10.0f };

        /// <summary>The wind sound to play. Null for default.</summary>
        [Tooltip("The wind sound to play. Null for default.")]
        public AudioClip WindAudioClip;

        /// <summary>How much the wind affects fog velocity, 0 for none.</summary>
        [Tooltip("How much the wind affects fog velocity, 0 for none.")]
        [Range(0.0f, 1.0f)]
        public float FogVelocityMultiplier = 0.001f;
    }
}
