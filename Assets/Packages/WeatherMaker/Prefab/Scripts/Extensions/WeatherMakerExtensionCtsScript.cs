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
    /// Integration for CTS (Complete Terrain Shader)
    /// </summary>
    [AddComponentMenu("Weather Maker/Extensions/CTS", 4)]
    public class WeatherMakerExtensionCtsScript : WeatherMakerExtensionRainSnowSeasonScript

#if CTS_PRESENT

    <CTS.CTSWeatherManager>

#else

    <UnityEngine.MonoBehaviour>

#endif

    {

#if CTS_PRESENT

        /// <summary>The minimum rain power.</summary>
        [Tooltip("The minimum rain power.")]
        [Range(0.0f, 1.0f)]
        public float MinRainPower = 0.0f;

        /// <summary>The minimum snow power.</summary>
        [Tooltip("The minimum snow power.")]
        [Range(0.0f, 1.0f)]
        public float MinSnowPower = 0.0f;

        protected override void OnUpdateRain(float rain)
        {
            TypeScript.RainPower = Mathf.Max(MinRainPower, rain);
        }

        protected override void OnUpdateSnow(float snow)
        {
            TypeScript.SnowPower = Mathf.Max(MinSnowPower, snow);
        }

        protected override void OnUpdateSeason(float season)
        {
            TypeScript.Season = season;
        }

#endif

    }
}
