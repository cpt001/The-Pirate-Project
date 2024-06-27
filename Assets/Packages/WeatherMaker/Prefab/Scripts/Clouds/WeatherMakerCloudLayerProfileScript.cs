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
    /// Cloud layer profile, allows configuring a flat layer of clouds
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherMakerCloudLayerProfile", menuName = "WeatherMaker/Cloud Layer Profile", order = 50)]
    public class WeatherMakerCloudLayerProfileScript : ScriptableObject
    {
        /// <summary>2D Texture for cloud noise - use single channel alpha texture only.</summary>
        [Header("Clouds - noise")]
        [Tooltip("2D Texture for cloud noise - use single channel alpha texture only.")]
        public Texture CloudNoise;

        /*
        /// <summary>Cloud sample count, layer 1</summary>
        [Tooltip("Cloud sample count, layer 1")]
        [Range(1, 100)]
        public int CloudSampleCount = 1;

        /// <summary>Cloud sample step multiplier, up to 4 octaves.</summary>
        [SingleLine("Cloud sample step multiplier, up to 4 octaves.")]
        public Vector4 CloudSampleStepMultiplier = new Vector4(50.0f, 50.0f, 50.0f, 50.0f);

        /// <summary>Cloud sample dither magic, controls appearance of clouds through ray march</summary>
        [SingleLine("Cloud sample dither magic, controls appearance of clouds through ray march")]
        public Vector4 CloudSampleDitherMagic;

        /// <summary>Cloud sample dither intensit</summary>
        [SingleLine("Cloud sample dither intensit")]
        public Vector4 CloudSampleDitherIntensity;
        */

        /// <summary>Cloud noise scale.</summary>
        [MinMaxSlider(0.01f, 1.0f, "Cloud noise scale.")]
        public RangeOfFloats CloudNoiseScale = new RangeOfFloats(0.15f, 0.3f);

        /// <summary>Cloud noise adder.</summary>
        [MinMaxSlider(-1.0f, 1.0f, "Cloud noise adder.")]
        public RangeOfFloats CloudNoiseAdder = new RangeOfFloats(0.0f, 0.0f);

        /// <summary>Cloud noise multiplier.</summary>
        [MinMaxSlider(0.1f, 4.0f, "Cloud noise multiplier.")]
        public RangeOfFloats CloudNoiseMultiplier = new RangeOfFloats(1.2f, 1.5f);

        /// <summary>Cloud noise dither.</summary>
        [MinMaxSlider(0.0f, 1.0f, "Cloud noise dither")]
        public RangeOfFloats CloudNoiseDither = new RangeOfFloats(0.01f, 0.02f);

        /// <summary>Cloud noise velocity.</summary>
        [Tooltip("Cloud noise velocity.")]
        public Vector3 CloudNoiseVelocity;

        /// <summary>Cloud noise rotation in degrees.</summary>
        [MinMaxSlider(-360.0f, 360.0f, "Cloud noise rotation in degrees.")]
        public RangeOfFloats CloudNoiseRotation;

        /*
        /// <summary>Texture for masking cloud noise, makes clouds visible in only certain parts of the sky.</summary>
        [Tooltip("Texture for masking cloud noise, makes clouds visible in only certain parts of the sky.")]
        public Texture2D CloudNoiseMask;

        /// <summary>Cloud noise mask scale.</summary>
        [Tooltip("Cloud noise mask scale.")]
        [Range(0.000001f, 1.0f)]
        public float CloudNoiseMaskScale = 0.0001f;

        /// <summary>Offset for cloud noise mask.</summary>
        [Tooltip("Offset for cloud noise mask.")]
        public Vector2 CloudNoiseMaskOffset;

        /// <summary>Cloud noise mask velocity.</summary>
        [Tooltip("Cloud noise mask velocity.")]
        public Vector3 CloudNoiseMaskVelocity;

        /// <summary>Cloud noise mask rotation in degrees.</summary>
        [MinMaxSlider(-360.0f, 360.0f, "Cloud noise mask rotation in degrees.")]
        public RangeOfFloats CloudNoiseMaskRotation;
        */

        /// <summary>Cloud color, for lighting.</summary>
        [Header("Clouds - appearance")]
        [Tooltip("Cloud color, for lighting.")]
        public Color CloudColor = Color.white;

        /// <summary>Cloud emission color, always emits this color regardless of lighting.</summary>
        [Tooltip("Cloud emission color, always emits this color regardless of lighting.")]
        public Color CloudEmissionColor = Color.clear;

        /// <summary>Cloud gradient color, where center of gradient is sun at horizon, right is up.</summary>
        [Tooltip("Cloud gradient color, where center of gradient is sun at horizon, right is up.")]
        public Gradient CloudGradientColor;

        /// <summary>Cloud ambient ground light multiplier.</summary>
        [MinMaxSlider(0.0f, 20.0f, "Cloud ambient ground light multiplier.")]
        public RangeOfFloats CloudAmbientGroundMultiplier = new RangeOfFloats(1.0f, 1.5f);

        /// <summary>Cloud ambient sky light multiplier.</summary>
        [MinMaxSlider(0.0f, 20.0f, "Cloud ambient sky light multiplier.")]
        public RangeOfFloats CloudAmbientSkyMultiplier = new RangeOfFloats(1.0f, 1.5f);

        /// <summary>Cloud Scatter light multiplier (hg forward/back power, hg forward/back multiplier).</summary>
        [SingleLine("Cloud Scatter light multiplier (hg forward/back power, hg forward/back multiplier).")]
        public Vector4 CloudScatterMultiplier = new Vector4(-0.5f, 0.8f, 1.0f, 1.0f);

        /// <summary>Cloud height - affects how fast clouds move as player moves and affects scale.</summary>
        [MinMaxSlider(2000.0f, 50000.0f, "Cloud height - affects how fast clouds move as player moves and affects scale.")]
        public RangeOfFloats CloudHeight = new RangeOfFloats(5000.0f, 6000.0f);

        /// <summary>Cloud cover, controls how many clouds / how thick the clouds are.</summary>
        [MinMaxSlider(0.0f, 1.0f, "Cloud cover, controls how many clouds / how thick the clouds are.")]
        public RangeOfFloats CloudCover = new RangeOfFloats(0.3f, 0.5f);

        /// <summary>Cloud light absorption. As this approaches maximum, more light is absorbed.</summary>
        [MinMaxSlider(0.0f, 10.0f, "Cloud light absorption. As this approaches maximum, more light is absorbed.")]
        public RangeOfFloats CloudLightAbsorption = new RangeOfFloats(0.4f, 0.8f);

        /// <summary>Bring clouds down at the horizon at the cost of stretching over the top.</summary>
        [Tooltip("Bring clouds down at the horizon at the cost of stretching over the top.")]
        [Range(0.0f, 0.5f)]
        public float CloudRayOffset = 0.05f;
    }
}
