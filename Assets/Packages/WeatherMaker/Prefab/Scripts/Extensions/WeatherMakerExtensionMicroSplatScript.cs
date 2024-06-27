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
    /// Integration for micro splat
    /// </summary>
    [AddComponentMenu("Weather Maker/Extensions/MicroSplat", 5)]
    public class WeatherMakerExtensionMicroSplatScript : WeatherMakerExtensionRainSnowSeasonScript

#if __MICROSPLAT__

#if UNITY_2020_1_OR_NEWER

        <JBooth.MicroSplat.MicroSplatTerrain>

#else

        <MicroSplatTerrain>

#endif

#else

        <UnityEngine.MonoBehaviour>

#endif

    {

#if __MICROSPLAT__

        /// <summary>The maximum wetness level</summary>
        [Tooltip("The maximum wetness level")]
        [Range(0.0f, 1.0f)]
        public float MaxWetness = 1.0f;

        /// <summary>The maximum puddles level</summary>
        [Tooltip("The maximum puddles level")]
        [Range(0.0f, 1.0f)]
        public float MaxPuddle = 1.0f;

        /// <summary>The maximum ripples level</summary>
        [Tooltip("The maximum ripples level")]
        [Range(0.0f, 1.0f)]
        public float MaxRipples = 1.0f;

        /// <summary>The maximum rivers and streams level</summary>
        [Tooltip("The maximum rivers and streams level")]
        [Range(0.0f, 1.0f)]
        public float MaxRivers = 1.0f;

        /// <summary>The maximum snow level</summary>
        [Tooltip("The maximum snow level")]
        [Range(0.0f, 1.0f)]
        public float MaxSnow = 1.0f;

        protected override void OnUpdateRain(float rain)
        {
            Shader.SetGlobalVector(WMS._Global_WetnessParams, new Vector2(rain, MaxWetness));
            Shader.SetGlobalVector(WMS._Global_PuddleParams, new Vector2(rain, MaxPuddle));
            Shader.SetGlobalFloat(WMS._Global_RainIntensity, Mathf.Min(MaxRipples, rain));
            Shader.SetGlobalFloat(WMS._Global_StreamMax, Mathf.Min(MaxRivers, rain));
        }

        protected override void OnUpdateSnow(float snow)
        {
            Shader.SetGlobalFloat(WMS._Global_SnowLevel, Mathf.Min(MaxSnow, snow));
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            // micro splat does not need to sync to get stuff to show up
        }

#endif

    }
}
