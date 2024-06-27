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
    /// Types of precipitation
    /// </summary>
    public enum WeatherMakerPrecipitationType
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Rain
        /// </summary>
        Rain = 1,

        /// <summary>
        /// Snow
        /// </summary>
        Snow = 2,

        /// <summary>
        /// Sleet
        /// </summary>
        Sleet = 3,

        /// <summary>
        /// Hail
        /// </summary>
        Hail = 4,

        /// <summary>
        /// Custom precipitation
        /// </summary>
        Custom = 127
    }

    /// <summary>
    /// Precipitation profile, contains common precipitation rendering properties
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherMakerPrecipitationProfile", menuName = "WeatherMaker/Precipitation Profile", order = 25)]
    [System.Serializable]
    public class WeatherMakerPrecipitationProfileScript : WeatherMakerBaseScriptableObjectScript
    {
        /// <summary>Type of precipitation</summary>
        [Tooltip("Type of precipitation")]
        public WeatherMakerPrecipitationType PrecipitationType = WeatherMakerPrecipitationType.Rain;

        /// <summary>Range of intensities</summary>
        [MinMaxSlider(0.0f, 1.0f, "Range of intensities")]
        public RangeOfFloats IntensityRange = new RangeOfFloats { Minimum = 0.1f, Maximum = 0.3f };

        /// <summary>How often a new value from IntensityRange should be chosen</summary>
        [MinMaxSlider(0.0f, 120.0f, "How often a new value from IntensityRange should be chosen")]
        public RangeOfFloats IntensityRangeDuration = new RangeOfFloats { Minimum = 10.0f, Maximum = 60.0f };

        /// <summary>Tint the precipitation, useful for acid rain or other magical effects.</summary>
        [Tooltip("Tint the precipitation, useful for acid rain or other magical effects.")]
        [ColorUsage(true, true)]
        public Color PrecipitationTintColor = Color.white;

        /// <summary>Tint the precipitation mist, useful for acid rain or other magical effects.</summary>
        [Tooltip("Tint the precipitation mist, useful for acid rain or other magical effects.")]
        [ColorUsage(true, true)]
        public Color PrecipitationMistTintColor = Color.white;

        /// <summary>Tint the precipitation secondary, useful for acid rain or other magical effects.</summary>
        [Tooltip("Tint the precipitation secondary, useful for acid rain or other magical effects.")]
        [ColorUsage(true, true)]
        public Color PrecipitationSecondaryTintColor = Color.white;
    }
}
