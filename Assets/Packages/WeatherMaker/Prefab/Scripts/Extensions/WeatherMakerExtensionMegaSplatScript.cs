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
    /// Integration for mega splat
    /// </summary>
    [AddComponentMenu("Weather Maker/Extensions/MegaSplat", 4)]
    public class WeatherMakerExtensionMegaSplatScript : WeatherMakerExtensionRainSnowSeasonScript

#if __MEGASPLAT__

        <MegaSplatTerrainManager>

#else

        <UnityEngine.MonoBehaviour>

#endif

    {

#if __MEGASPLAT__

        /// <summary>The maximum rain level</summary>
        [Tooltip("The maximum rain level")]
        [Range(0.0f, 1.0f)]
        public float MaxRain = 1.0f;

        /// <summary>The maximum puddles level</summary>
        [Tooltip("The maximum puddles level")]
        [Range(0.0f, 1.0f)]
        public float MaxPuddles = 1.0f;

        /// <summary>The maximum ripples level</summary>
        [Tooltip("The maximum ripples level")]
        [Range(0.0f, 1.0f)]
        public float MaxRipples = 1.0f;

        /// <summary>The maximum snow level</summary>
        [Tooltip("The maximum snow level")]
        [Range(0.0f, 1.0f)]
        public float MaxSnow = 1.0f;

        private bool changes;

        protected override void OnUpdateRain(float rain)
        {
            if (TypeScript == null)
            {
                return;
            }

            Vector4 pw = TypeScript.templateMaterial.GetVector(WMS._GlobalPorosityWetness);
            float puddles = TypeScript.templateMaterial.GetFloat(WMS._PuddleBlend);
            float ripples = TypeScript.templateMaterial.GetFloat(WMS._RainIntensity);

            Vector4 pw2 = pw;
            pw2.y = Mathf.Min(rain, MaxRain);
            if (pw != pw2)
            {
                TypeScript.templateMaterial.SetVector(WMS._GlobalPorosityWetness, pw2);
                changes = true;
            }

            float puddles2 =Mathf.Min(rain, MaxPuddles) * 60.0f; // for some reason mega splat uses 0-60 as the puddle value, see MegaSplat_Example_Mesh.shader
            if (puddles != puddles2)
            {
                TypeScript.templateMaterial.SetFloat(WMS._PuddleBlend, puddles2);
                changes = true;
            }

            float ripples2 = Mathf.Min(rain, MaxRipples);
            if (ripples != ripples2)
            {
                TypeScript.templateMaterial.SetFloat(WMS._RainIntensity, ripples2);
                changes = true;
            }
        }

        protected override void OnUpdateSnow(float snow)
        {
            if (TypeScript == null)
            {
                return;
            }

            float snow1 = TypeScript.templateMaterial.GetFloat(WMS._SnowAmount);
            float snow2 = Mathf.Min(snow, MaxSnow);
            if (snow1 != snow2)
            {
                TypeScript.templateMaterial.SetFloat(WMS._SnowAmount, snow2);
                changes = true;
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (TypeScript != null && changes)
            {
                TypeScript.Sync();
                changes = false;
            }
        }

#endif

    }
}
