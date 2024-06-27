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
    /// Integration for relief terrain pack
    /// </summary>
    [AddComponentMenu("Weather Maker/Extensions/Relief Terrain Pack", 3)]
    public class WeatherMakerExtensionRtpTerrainScript : WeatherMakerExtensionRainSnowSeasonScript

#if RELIEF_TERRAIN_PACK_PRESENT

        <ReliefTerrain>

#else

        <UnityEngine.MonoBehaviour>

#endif

    {

#if RELIEF_TERRAIN_PACK_PRESENT

        /// <summary>The minimum water level.</summary>
        [Tooltip("The minimum water level.")]
        [Range(0.0f, 1.0f)]
        public float MinWaterLevel = 0.0f;

        /// <summary>The minimum wetness amount.</summary>
        [Tooltip("The minimum wetness amount.")]
        [Range(0.0f, 1.0f)]
        public float MinWetnessAmount = 0.0f;

        /// <summary>The minimum snow amount.</summary>
        [Tooltip("The minimum snow amount.")]
        [Range(0.0f, 1.0f)]
        public float MinSnowAmount = 0.0f;

        protected override void OnUpdateRain(float rain)
        {
            TypeScript.globalSettingsHolder.TERRAIN_RainIntensity = rain;
            TypeScript.globalSettingsHolder.TERRAIN_GlobalWetness = Mathf.Max(MinWetnessAmount, rain);
            Shader.SetGlobalFloat(WMS.TERRAIN_RainIntensity, rain);
            Shader.SetGlobalFloat(WMS.TERRAIN_GlobalWetness, TERRAIN_GlobalWetness);
        }

        protected override void OnUpdateSnow(float snow)
        {
            TypeScript.globalSettingsHolder._snow_strength = Mathf.Max(MinSnowAmount, snow);
            Shader.SetGlobalFloat(WMS.rtp_snow_strength, _snow_strength);
        }

#endif

    }
}
