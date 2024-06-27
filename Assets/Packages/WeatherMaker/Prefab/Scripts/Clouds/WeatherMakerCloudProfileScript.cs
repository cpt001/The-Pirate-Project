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
    /// Types of clouds
    /// </summary>
    public enum WeatherMakerCloudType
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Light
        /// </summary>
        Light = 10,

        /// <summary>
        /// LightScattered
        /// </summary>
        LightScattered = 15,

        /// <summary>
        /// LightMedium
        /// </summary>
        LightMedium = 20,

        /// <summary>
        /// LightMediumScattered
        /// </summary>
        LightMediumScattered = 25,

        /// <summary>
        /// Medium
        /// </summary>
        Medium = 30,

        /// <summary>
        /// MediumScattered
        /// </summary>
        MediumScattered = 35,

        /// <summary>
        /// MediumHeavy
        /// </summary>
        MediumHeavy = 40,

        /// <summary>
        /// MediumHeavyScattered
        /// </summary>
        MediumHeavyScattered = 45,

        /// <summary>
        /// MediumHeavyScatteredStormy
        /// </summary>
        MediumHeavyScatteredStormy = 47,

        /// <summary>
        /// HeavyDark
        /// </summary>
        HeavyDark = 50,

        /// <summary>
        /// HeavyScattered
        /// </summary>
        HeavyScattered = 55,

        /// <summary>
        /// HeavyBright
        /// </summary>
        HeavyBright = 60,

        /// <summary>
        /// Storm
        /// </summary>
        Storm = 70,

        /// <summary>
        /// Custom
        /// </summary>
        Custom = 250,

        /// <summary>
        /// Overcast
        /// </summary>
        Overcast = 48
    }

    /// <summary>
    /// Weather maker cloud profile script, contains flat and volumetric layers
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherMakerCloudProfile", menuName = "WeatherMaker/Cloud Profile", order = 40)]
    [System.Serializable]
    public class WeatherMakerCloudProfileScript : ScriptableObject
    {
        /// <summary>The first, and lowest cloud layer, null for none</summary>
        [Header("Layers")]
        [Tooltip("The first, and lowest cloud layer, null for none")]
        public WeatherMakerCloudLayerProfileScript CloudLayer1;

        /// <summary>The second, and second lowest cloud layer, null for none</summary>
        [Tooltip("The second, and second lowest cloud layer, null for none")]
        public WeatherMakerCloudLayerProfileScript CloudLayer2;

        /// <summary>The third, and third lowest cloud layer, null for none</summary>
        [Tooltip("The third, and third lowest cloud layer, null for none")]
        public WeatherMakerCloudLayerProfileScript CloudLayer3;

        /// <summary>The fourth, and highest cloud layer, null for none</summary>
        [Tooltip("The fourth, and highest cloud layer, null for none")]
        public WeatherMakerCloudLayerProfileScript CloudLayer4;

        /// <summary>Allow a single layer of volumetric clouds. In the future, more volumetric layers might be supported</summary>
        [Tooltip("Allow a single layer of volumetric clouds. In the future, more volumetric layers might be supported")]
        public WeatherMakerCloudVolumetricProfileScript CloudLayerVolumetric1;

        /// <summary>How much to multiply directional light intensities by when clouds are showing. Ignored for volumetric clouds.</summary>
        [Header("Lighting - intensity")]
        [Tooltip("How much to multiply directional light intensities by when clouds are showing. Ignored for volumetric clouds.")]
        [Range(0.0f, 1.0f)]
        public float DirectionalLightIntensityMultiplier = 1.0f;

        /// <summary>How much the clouds reduce directional light scattering. Affects fog and other volumetric effects.</summary>
        [Tooltip("How much the clouds reduce directional light scattering. Affects fog and other volumetric effects.")]
        [Range(0.0f, 1.0f)]
        public float DirectionalLightScatterMultiplier = 1.0f;

        /// <summary>Amount of shadow for atmospheric scattering. Lower values create more shadow.</summary>
        [Tooltip("Amount of shadow for atmospheric scattering. Lower values create more shadow.")]
        [Range(0.0f, 1.0f)]
        public float DirectionalLightAtmosphericShadow = 0.1f;

        /// <summary>How much clouds affect directional light intensity, lower values ensure no reduction. Ignored for volumetric clouds.</summary>
        [Tooltip("How much clouds affect directional light intensity, lower values ensure no reduction. Ignored for volumetric clouds.")]
        [Range(0.0f, 3.0f)]
        public float CloudLightStrength = 1.0f;

        /// <summary>Set a custom weather map texture, bypassing the auto-generated weather map</summary>
        [Header("Weather map (volumetric only)")]
        [Tooltip("Set a custom weather map texture, bypassing the auto-generated weather map")]
        public Texture WeatherMapRenderTextureOverride;

        /// <summary>Set a custom weather map texture mask, this will mask out all areas of the weather map based on lower alpha values.</summary>
        [Tooltip("Set a custom weather map texture mask, this will mask out all areas of the weather map based on lower alpha values.")]
        public Texture WeatherMapRenderTextureMask;

        /*
        /// <summary>Velocity of weather map mask in uv coordinates (0 - 1)</summary>
        [Tooltip("Velocity of weather map mask in uv coordinates (0 - 1)")]
        public Vector2 WeatherMapRenderTextureMaskVelocity;

        /// <summary>Offset of weather map mask (0 - 1). Velocity is applied automatically but you can set manually as well.</summary>
        [Tooltip("Offset of weather map mask (0 - 1). Velocity is applied automatically but you can set manually as well.")]
        public Vector2 WeatherMapRenderTextureMaskOffset;

        /// <summary>Clamp for weather map mask offset to ensure that it does not go too far out of bounds.</summary>
        [Tooltip("Clamp for weather map mask offset to ensure that it does not go too far out of bounds.")]
        public Vector2 WeatherMapRenderTextureMaskOffsetClamp = new Vector2(-1.1f, 1.1f);
        */

        /// <summary>World scale.</summary>
        [Tooltip("World scale.")]
        [Range(0.000001f, 0.001f)]
        public float WorldScale = 0.00001f;

        /// <summary>Noise scale (x).</summary>
        [SingleLine("Noise scale (x).")]
        public RangeOfFloats NoiseScaleX = new RangeOfFloats(1.0f, 1.0f);

        /// <summary>Noise scale (y).</summary>
        [SingleLine("Noise scale (y).")]
        public RangeOfFloats NoiseScaleY = new RangeOfFloats(1.0f, 1.0f);

        /// <summary>Weather map cloud coverage velocity.</summary>
        [Tooltip("Weather map cloud coverage velocity.")]
        [Header("Coverage")]
        public Vector3 WeatherMapCloudCoverageVelocity = new Vector3(11.0f, 15.0f, 0.0f);

        /// <summary>Amount of noise to apply to weather map cloud coverage. 0 = all perlin, 1 = all worley.</summary>
        [MinMaxSlider(0.0f, 1.0f, "Amount of noise to apply to weather map cloud coverage. 0 = all perlin, 1 = all worley.")]
        public RangeOfFloats WeatherMapCloudCoverageNoiseType = new RangeOfFloats(0.0f, 0.0f);

        /// <summary>Scale of cloud coverage. Higher values produce smaller clouds.</summary>
        [MinMaxSlider(0.01f, 100.0f, "Scale of cloud coverage. Higher values produce smaller clouds.")]
        public RangeOfFloats WeatherMapCloudCoverageScale = new RangeOfFloats(4.0f, 16.0f);

        /// <summary>Rotation of cloud coverage. Rotates coverage map around center of weather map.</summary>
        [MinMaxSlider(-360.0f, 360.0f, "Rotation of cloud coverage. Rotates coverage map around center of weather map.")]
        public RangeOfFloats WeatherMapCloudCoverageRotation = new RangeOfFloats(-30.0f, 30.0f);

        /// <summary>Cloud coverage adder. Higher values create more cloud coverage.</summary>
        [MinMaxSlider(-1.0f, 1.0f, "Cloud coverage adder. Higher values create more cloud coverage.")]
        public RangeOfFloats WeatherMapCloudCoverageAdder = new RangeOfFloats { Minimum = 0.0f, Maximum = 0.0f };

        /// <summary>Cloud coverage power. Higher values create more firm cloud coverage edges.</summary>
        [MinMaxSlider(0.0f, 16.0f, "Cloud coverage power. Higher values create more firm cloud coverage edges.")]
        public RangeOfFloats WeatherMapCloudCoveragePower = new RangeOfFloats { Minimum = 1.0f, Maximum = 1.0f };

        /// <summary>Weather map cloud coverage warp, min values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).</summary>
        [SingleLine("Weather map cloud coverage warp, min values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).")]
        public Vector4 WeatherMapCloudCoverageWarpMin = Vector4.zero;

        [System.NonSerialized]
        internal Vector4 weatherMapCloudCoverageWarpMinPrev = Vector4.zero;

        /// <summary>Weather map cloud coverage warp, max values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).</summary>
        [SingleLine("Weather map cloud coverage warp, max values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).")]
        public Vector4 WeatherMapCloudCoverageWarpMax = Vector4.zero;

        [System.NonSerialized]
        internal Vector4 weatherMapCloudCoverageWarpMaxPrev = Vector4.zero;

        [System.NonSerialized]
        private Vector4? weatherMapCloudCoverageWarp;

        internal Vector4 WeatherMapCloudCoverageWarp
        {
            get
            {
                if (weatherMapCloudCoverageWarp == null)
                {
                    float l1 = Mathf.Pow(randomizer.Random(), 2.0f);
                    float l2 = Mathf.Pow(randomizer.Random(), 2.0f);
                    weatherMapCloudCoverageWarp = new Vector4
                    (
                        randomizer.Random(WeatherMapCloudCoverageWarpMin.x, WeatherMapCloudCoverageWarpMax.x),
                        randomizer.Random(WeatherMapCloudCoverageWarpMin.y, WeatherMapCloudCoverageWarpMax.y),
                        Mathf.Lerp(WeatherMapCloudCoverageWarpMin.z, WeatherMapCloudCoverageWarpMax.z, l1),
                        Mathf.Lerp(WeatherMapCloudCoverageWarpMin.w, WeatherMapCloudCoverageWarpMax.w, l2)
                    );
                    weatherMapCloudCoverageWarpMinPrev = WeatherMapCloudCoverageWarpMin;
                    weatherMapCloudCoverageWarpMaxPrev = WeatherMapCloudCoverageWarpMax;
                }
                return weatherMapCloudCoverageWarp.Value;
            }
            set
            {
                weatherMapCloudCoverageWarp = value;
                weatherMapCloudCoverageWarpMinPrev = WeatherMapCloudCoverageWarpMin;
                weatherMapCloudCoverageWarpMaxPrev = WeatherMapCloudCoverageWarpMax;
            }
        }

        /// <summary>Weather map cloud coverage negation velocity, xy units per second, z change per second</summary>
        [Tooltip("Weather map cloud coverage negation velocity, xy units per second, z change per second")]
        [Header("Coverage Negation")]
        public Vector3 WeatherMapCloudCoverageNegationVelocity = new Vector3(0.0f, 0.0f, 0.0f);

        /// <summary>Scale of cloud coverage negation. 0 for none. Higher values produce smaller negation areas.</summary>
        [MinMaxSlider(0.0f, 100.0f, "Scale of cloud coverage negation . Higher values produce smaller negation areas.")]
        public RangeOfFloats WeatherMapCloudCoverageNegationScale = new RangeOfFloats(1.0f, 1.0f);

        /// <summary>Rotation of cloud coverage negation. Rotates coverage negation map around center of weather map.</summary>
        [MinMaxSlider(-360.0f, 360.0f, "Rotation of cloud coverage negation. Rotates coverage negation map around center of weather map.")]
        public RangeOfFloats WeatherMapCloudCoverageNegationRotation = new RangeOfFloats(-30.0f, 30.0f);

        /// <summary>Cloud coverage negation adder. Higher values create more cloud coverage negation.</summary>
        [MinMaxSlider(-1.0f, 1.0f, "Cloud coverage negation adder. Higher values create more cloud coverage negation.")]
        public RangeOfFloats WeatherMapCloudCoverageNegationAdder = new RangeOfFloats { Minimum = -1.0f, Maximum = -1.0f };

        /// <summary>Cloud coverage negation power. Higher values create more firm cloud coverage negation edges.</summary>
        [MinMaxSlider(0.0f, 16.0f, "Cloud coverage negation power. Higher values create more firm cloud coverage negation edges.")]
        public RangeOfFloats WeatherMapCloudCoverageNegationPower = new RangeOfFloats { Minimum = 1.0f, Maximum = 1.0f };

        /// <summary>Max cloud coverage negation offset. Moves the negation noise around.</summary>
        //[SingleLine("Cloud coverage negation adder. Higher values create more cloud coverage negation.")]
        //public Vector2 WeatherMapCloudCoverageNegationOffset = new Vector2(5000.0f, 50000.0f);

        //[System.NonSerialized]
        //internal Vector2 weatherMapCloudCoverageNegationOffsetCalculated;

        /// <summary>Weather map cloud coverage negation warp, min values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).</summary>
        [SingleLine("Weather map cloud coverage negation warp, min values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).")]
        public Vector4 WeatherMapCloudCoverageNegationWarpMin = Vector4.zero;

        [System.NonSerialized]
        internal Vector4 weatherMapCloudCoverageNegationWarpMinPrev = Vector4.zero;

        /// <summary>Weather map cloud coverage negation warp, max values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).</summary>
        [SingleLine("Weather map cloud coverage negation warp, max values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).")]
        public Vector4 WeatherMapCloudCoverageNegationWarpMax = Vector4.zero;

        [System.NonSerialized]
        internal Vector4 weatherMapCloudCoverageNegationWarpMaxPrev = Vector4.zero;

        [System.NonSerialized]
        private Vector4? weatherMapCloudCoverageNegationWarp;

        internal Vector4 WeatherMapCloudCoverageNegationWarp
        {
            get
            {
                if (weatherMapCloudCoverageNegationWarp == null)
                {
                    float l1 = Mathf.Pow(randomizer.Random(), 2.0f);
                    float l2 = Mathf.Pow(randomizer.Random(), 2.0f);
                    weatherMapCloudCoverageNegationWarp = new Vector4
                    (
                        randomizer.Random(WeatherMapCloudCoverageNegationWarpMin.x, WeatherMapCloudCoverageNegationWarpMax.x),
                        randomizer.Random(WeatherMapCloudCoverageNegationWarpMin.y, WeatherMapCloudCoverageNegationWarpMax.y),
                        Mathf.Lerp(WeatherMapCloudCoverageNegationWarpMin.z, WeatherMapCloudCoverageNegationWarpMax.z, l1),
                        Mathf.Lerp(WeatherMapCloudCoverageNegationWarpMin.w, WeatherMapCloudCoverageNegationWarpMax.w, l2)
                    );
                    weatherMapCloudCoverageNegationWarpMinPrev = WeatherMapCloudCoverageNegationWarpMin;
                    weatherMapCloudCoverageNegationWarpMaxPrev = WeatherMapCloudCoverageNegationWarpMax;
                }
                return weatherMapCloudCoverageNegationWarp.Value;
            }
            set
            {
                weatherMapCloudCoverageNegationWarp = value;
                weatherMapCloudCoverageNegationWarpMinPrev = WeatherMapCloudCoverageNegationWarpMin;
                weatherMapCloudCoverageNegationWarpMaxPrev = WeatherMapCloudCoverageNegationWarpMax;
            }
        }

        /// <summary>Weather map cloud Density velocity, xy units per second, z change per second</summary>
        [Tooltip("Weather map cloud density velocity, xy units per second, z change per second")]
        [Header("Density")]
        public Vector3 WeatherMapCloudDensityVelocity = new Vector3(11.0f, 15.0f, 0.0f);

        /// <summary>Amount of noise to apply to weather map cloud Density. 0 = all perlin, 1 = all worley.</summary>
        [MinMaxSlider(0.0f, 1.0f, "Amount of noise to apply to weather map cloud Density. 0 = all perlin, 1 = all worley.")]
        public RangeOfFloats WeatherMapCloudDensityNoiseType = new RangeOfFloats(1.0f, 1.0f);

        /// <summary>Scale of cloud Density. Higher values produce smaller clouds.</summary>
        [MinMaxSlider(0.01f, 100.0f, "Scale of cloud Density. Higher values produce smaller clouds.")]
        public RangeOfFloats WeatherMapCloudDensityScale = new RangeOfFloats(4.0f, 16.0f);

        /// <summary>Rotation of cloud Density. Rotates Density map around center of weather map.</summary>
        [MinMaxSlider(-360.0f, 360.0f, "Rotation of cloud Density. Rotates Density map around center of weather map.")]
        public RangeOfFloats WeatherMapCloudDensityRotation = new RangeOfFloats(-30.0f, 30.0f);

        /// <summary>Cloud Density adder. Higher values create more cloud Density.</summary>
        [MinMaxSlider(-1.0f, 1.0f, "Cloud Density adder. Higher values create more cloud Density.")]
        public RangeOfFloats WeatherMapCloudDensityAdder = new RangeOfFloats { Minimum = 0.0f, Maximum = 0.0f };

        /// <summary>Cloud Density power. Higher values create more firm cloud Density edges.</summary>
        [MinMaxSlider(0.0f, 16.0f, "Cloud Density power. Higher values create more firm cloud Density edges.")]
        public RangeOfFloats WeatherMapCloudDensityPower = new RangeOfFloats { Minimum = 1.0f, Maximum = 1.0f };

        /// <summary>Weather map cloud Density warp, min values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).</summary>
        [SingleLine("Weather map cloud Density warp, min values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).")]
        public Vector4 WeatherMapCloudDensityWarpMin = Vector4.zero;

        [System.NonSerialized]
        internal Vector4 weatherMapCloudDensityWarpMinPrev = Vector4.zero;

        /// <summary>Weather map cloud Density warp, max values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).</summary>
        [SingleLine("Weather map cloud Density warp, max values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).")]
        public Vector4 WeatherMapCloudDensityWarpMax = Vector4.zero;

        [System.NonSerialized]
        internal Vector4 weatherMapCloudDensityWarpMaxPrev = Vector4.zero;

        [System.NonSerialized]
        private Vector4? weatherMapCloudDensityWarp;

        internal Vector4 WeatherMapCloudDensityWarp
        {
            get
            {
                if (weatherMapCloudDensityWarp == null)
                {
                    float l1 = Mathf.Pow(randomizer.Random(), 2.0f);
                    float l2 = Mathf.Pow(randomizer.Random(), 2.0f);
                    weatherMapCloudDensityWarp = new Vector4
                    (
                        randomizer.Random(WeatherMapCloudDensityWarpMin.x, WeatherMapCloudDensityWarpMax.x),
                        randomizer.Random(WeatherMapCloudDensityWarpMin.y, WeatherMapCloudDensityWarpMax.y),
                        Mathf.Lerp(WeatherMapCloudDensityWarpMin.z, WeatherMapCloudDensityWarpMax.z, l1),
                        Mathf.Lerp(WeatherMapCloudDensityWarpMin.w, WeatherMapCloudDensityWarpMax.w, l2)
                    );
                    weatherMapCloudDensityWarpMinPrev = WeatherMapCloudDensityWarpMin;
                    weatherMapCloudDensityWarpMaxPrev = WeatherMapCloudDensityWarpMax;
                }
                return weatherMapCloudDensityWarp.Value;
            }
            set
            {
                weatherMapCloudDensityWarp = value;
                weatherMapCloudDensityWarpMinPrev = WeatherMapCloudDensityWarpMin;
                weatherMapCloudDensityWarpMaxPrev = WeatherMapCloudDensityWarpMax;
            }
        }

        /// <summary>Weather map cloud Density negation velocity, xy units per second, z change per second</summary>
        [Tooltip("Weather map cloud Density negation velocity, xy units per second, z change per second")]
        [Header("Density Negation")]
        public Vector3 WeatherMapCloudDensityNegationVelocity = new Vector3(0.0f, 0.0f, 0.0f);

        /// <summary>Scale of cloud Density negation. 0 for none. Higher values produce smaller negation areas.</summary>
        [MinMaxSlider(0.0f, 100.0f, "Scale of cloud Density negation . Higher values produce smaller negation areas.")]
        public RangeOfFloats WeatherMapCloudDensityNegationScale = new RangeOfFloats(1.0f, 1.0f);

        /// <summary>Rotation of cloud Density negation. Rotates Density negation map around center of weather map.</summary>
        [MinMaxSlider(-360.0f, 360.0f, "Rotation of cloud Density negation. Rotates Density negation map around center of weather map.")]
        public RangeOfFloats WeatherMapCloudDensityNegationRotation = new RangeOfFloats(-30.0f, 30.0f);

        /// <summary>Cloud Density negation adder. Higher values create more cloud Density negation.</summary>
        [MinMaxSlider(-1.0f, 1.0f, "Cloud Density negation adder. Higher values create more cloud Density negation.")]
        public RangeOfFloats WeatherMapCloudDensityNegationAdder = new RangeOfFloats { Minimum = -1.0f, Maximum = -1.0f };

        /// <summary>Cloud Density negation power. Higher values create more firm cloud Density negation edges.</summary>
        [MinMaxSlider(0.0f, 16.0f, "Cloud Density negation power. Higher values create more firm cloud Density negation edges.")]
        public RangeOfFloats WeatherMapCloudDensityNegationPower = new RangeOfFloats { Minimum = 1.0f, Maximum = 1.0f };

        /// <summary>Max cloud Density negation offset. Moves the negation noise around.</summary>
        //[SingleLine("Cloud Density negation adder. Higher values create more cloud Density negation.")]
        //public Vector2 WeatherMapCloudDensityNegationOffset = new Vector2(5000.0f, 50000.0f);

        //[System.NonSerialized]
        //internal Vector2 weatherMapCloudDensityNegationOffsetCalculated;

        /// <summary>Weather map cloud Density negation warp, min values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).</summary>
        [SingleLine("Weather map cloud Density negation warp, min values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).")]
        public Vector4 WeatherMapCloudDensityNegationWarpMin = Vector4.zero;

        [System.NonSerialized]
        internal Vector4 weatherMapCloudDensityNegationWarpMinPrev = Vector4.zero;

        /// <summary>Weather map cloud Density negation warp, max values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).</summary>
        [SingleLine("Weather map cloud Density negation warp, max values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).")]
        public Vector4 WeatherMapCloudDensityNegationWarpMax = Vector4.zero;

        [System.NonSerialized]
        internal Vector4 weatherMapCloudDensityNegationWarpMaxPrev = Vector4.zero;

        [System.NonSerialized]
        private Vector4? weatherMapCloudDensityNegationWarp;

        internal Vector4 WeatherMapCloudDensityNegationWarp
        {
            get
            {
                if (weatherMapCloudDensityNegationWarp == null)
                {
                    float l1 = Mathf.Pow(randomizer.Random(), 2.0f);
                    float l2 = Mathf.Pow(randomizer.Random(), 2.0f);
                    weatherMapCloudDensityNegationWarp = new Vector4
                    (
                        randomizer.Random(WeatherMapCloudDensityNegationWarpMin.x, WeatherMapCloudDensityNegationWarpMax.x),
                        randomizer.Random(WeatherMapCloudDensityNegationWarpMin.y, WeatherMapCloudDensityNegationWarpMax.y),
                        Mathf.Lerp(WeatherMapCloudDensityNegationWarpMin.z, WeatherMapCloudDensityNegationWarpMax.z, l1),
                        Mathf.Lerp(WeatherMapCloudDensityNegationWarpMin.w, WeatherMapCloudDensityNegationWarpMax.w, l2)
                    );
                    weatherMapCloudDensityNegationWarpMinPrev = WeatherMapCloudDensityNegationWarpMin;
                    weatherMapCloudDensityNegationWarpMaxPrev = WeatherMapCloudDensityNegationWarpMax;
                }
                return weatherMapCloudDensityNegationWarp.Value;
            }
            set
            {
                weatherMapCloudDensityNegationWarp = value;
                weatherMapCloudDensityNegationWarpMinPrev = WeatherMapCloudDensityNegationWarpMin;
                weatherMapCloudDensityNegationWarpMaxPrev = WeatherMapCloudDensityNegationWarpMax;
            }
        }

        /// <summary>Weather map cloud type velocity, xy units per second, z change per second</summary>
        [Tooltip("Weather map cloud type velocity, xy units per second, z change per second")]
        [Header("Type")]
        public Vector3 WeatherMapCloudTypeVelocity = new Vector3(17.0f, 10.0f, 0.0f);

        /// <summary>Amount of noise to apply to weather map cloud type. 0 = all perlin, 1 = all worley.</summary>
        [MinMaxSlider(0.0f, 1.0f, "Amount of noise to apply to weather map cloud type. 0 = all perlin, 1 = all worley.")]
        public RangeOfFloats WeatherMapCloudTypeNoiseType = new RangeOfFloats(0.0f, 0.0f);

        /// <summary>Scale of cloud types. Higher values produce more jagged clouds.</summary>
        [MinMaxSlider(0.01f, 100.0f, "Scale of cloud types. Higher values produce more jagged clouds.")]
        public RangeOfFloats WeatherMapCloudTypeScale = new RangeOfFloats(2.0f, 8.0f);

        /// <summary>Rotation of cloud type. Rotates cloud type map around center of weather map.</summary>
        [MinMaxSlider(-360.0f, 360.0f, "Rotation of cloud type. Rotates cloud type map around center of weather map.")]
        public RangeOfFloats WeatherMapCloudTypeRotation;

        /// <summary>Cloud type adder. Higher values create more cloud type.</summary>
        [MinMaxSlider(-1.0f, 1.0f, "Cloud type adder. Higher values create more cloud type.")]
        public RangeOfFloats WeatherMapCloudTypeAdder = new RangeOfFloats { Minimum = 0.0f, Maximum = 0.0f };

        /// <summary>Cloud type power. Higher values create more firm cloud type edges.</summary>
        [MinMaxSlider(0.0f, 16.0f, "Cloud type power. Higher values create more firm cloud type edges.")]
        public RangeOfFloats WeatherMapCloudTypePower = new RangeOfFloats { Minimum = 1.0f, Maximum = 1.0f };

        /// <summary>Weather map cloud type warp, min values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).</summary>
        [SingleLine("Weather map cloud type warp, min values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).")]
        public Vector4 WeatherMapCloudTypeWarpMin = Vector4.zero;

        [System.NonSerialized]
        internal Vector4 weatherMapCloudTypeWarpMinPrev = Vector4.zero;

        /// <summary>Weather map cloud type warp, max values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).</summary>
        [SingleLine("Weather map cloud type warp, max values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).")]
        public Vector4 WeatherMapCloudTypeWarpMax = Vector4.zero;

        [System.NonSerialized]
        internal Vector4 weatherMapCloudTypeWarpMaxPrev = Vector4.zero;

        [System.NonSerialized]
        private Vector4? weatherMapCloudTypeWarp;

        internal Vector4 WeatherMapCloudTypeWarp
        {
            get
            {
                if (weatherMapCloudTypeWarp == null)
                {
                    float l1 = Mathf.Pow(randomizer.Random(), 2.0f);
                    float l2 = Mathf.Pow(randomizer.Random(), 2.0f);
                    weatherMapCloudTypeWarp = new Vector4
                    (
                        randomizer.Random(WeatherMapCloudTypeWarpMin.x, WeatherMapCloudTypeWarpMax.x),
                        randomizer.Random(WeatherMapCloudTypeWarpMin.y, WeatherMapCloudTypeWarpMax.y),
                        Mathf.Lerp(WeatherMapCloudTypeWarpMin.z, WeatherMapCloudTypeWarpMax.z, l1),
                        Mathf.Lerp(WeatherMapCloudTypeWarpMin.w, WeatherMapCloudTypeWarpMax.w, l2)
                    );
                    weatherMapCloudTypeWarpMinPrev = WeatherMapCloudTypeWarpMin;
                    weatherMapCloudTypeWarpMaxPrev = WeatherMapCloudTypeWarpMax;
                }
                return weatherMapCloudTypeWarp.Value;
            }
            set
            {
                weatherMapCloudTypeWarp = value;
                weatherMapCloudTypeWarpMinPrev = WeatherMapCloudTypeWarpMin;
                weatherMapCloudTypeWarpMaxPrev = WeatherMapCloudTypeWarpMax;
            }
        }

        /// <summary>Weather map cloud type negation velocity, xy units per second, z change per second</summary>
        [Tooltip("Weather map cloud type negation velocity, xy units per second, z change per second")]
        [Header("Type Negation")]
        public Vector3 WeatherMapCloudTypeNegationVelocity = new Vector3(0.0f, 0.0f, 0.0f);

        /// <summary>Scale of cloud type negation. 0 for none. Higher values produce smaller negation areas.</summary>
        [MinMaxSlider(0.0f, 100.0f, "Scale of cloud type negation . Higher values produce smaller negation areas.")]
        public RangeOfFloats WeatherMapCloudTypeNegationScale = new RangeOfFloats(1.0f, 1.0f);

        /// <summary>Rotation of cloud type negation. Rotates type negation map around center of weather map.</summary>
        [MinMaxSlider(-360.0f, 360.0f, "Rotation of cloud type negation. Rotates type negation map around center of weather map.")]
        public RangeOfFloats WeatherMapCloudTypeNegationRotation = new RangeOfFloats(-30.0f, 30.0f);

        /// <summary>Cloud type negation adder. Higher values create more cloud type negation.</summary>
        [MinMaxSlider(-1.0f, 1.0f, "Cloud type negation adder. Higher values create more cloud type negation.")]
        public RangeOfFloats WeatherMapCloudTypeNegationAdder = new RangeOfFloats { Minimum = -1.0f, Maximum = -1.0f };

        /// <summary>Cloud type negation power. Higher values create more firm cloud type negation edges.</summary>
        [MinMaxSlider(0.0f, 16.0f, "Cloud type negation power. Higher values create more firm cloud type negation edges.")]
        public RangeOfFloats WeatherMapCloudTypeNegationPower = new RangeOfFloats { Minimum = 1.0f, Maximum = 1.0f };

        /// <summary>Max cloud type negation offset. Moves the negation noise around.</summary>
        //[SingleLine("Cloud type negation adder. Higher values create more cloud type negation.")]
        //public Vector2 WeatherMapCloudTypeNegationOffset = new Vector2(5000.0f, 50000.0f);

        //[System.NonSerialized]
        //internal Vector2 weatherMapCloudTypeNegationOffsetCalculated;

        /// <summary>Weather map cloud type negation warp, min values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).</summary>
        [SingleLine("Weather map cloud type negation warp, min values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).")]
        public Vector4 WeatherMapCloudTypeNegationWarpMin = Vector4.zero;

        [System.NonSerialized]
        internal Vector4 weatherMapCloudTypeNegationWarpMinPrev = Vector4.zero;

        /// <summary>Weather map cloud type negation warp, max values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).</summary>
        [SingleLine("Weather map cloud type negation warp, max values. X = warp noise scale x, Y = warp noise scale y, Z = warp noise scale x influence (0-1), W = warp noise scale y influence (0-1).")]
        public Vector4 WeatherMapCloudTypeNegationWarpMax = Vector4.zero;

        [System.NonSerialized]
        internal Vector4 weatherMapCloudTypeNegationWarpMaxPrev = Vector4.zero;

        [System.NonSerialized]
        private Vector4? weatherMapCloudTypeNegationWarp;

        internal Vector4 WeatherMapCloudTypeNegationWarp
        {
            get
            {
                if (weatherMapCloudTypeNegationWarp == null)
                {
                    float l1 = Mathf.Pow(randomizer.Random(), 2.0f);
                    float l2 = Mathf.Pow(randomizer.Random(), 2.0f);
                    weatherMapCloudTypeNegationWarp = new Vector4
                    (
                        randomizer.Random(WeatherMapCloudTypeNegationWarpMin.x, WeatherMapCloudTypeNegationWarpMax.x),
                        randomizer.Random(WeatherMapCloudTypeNegationWarpMin.y, WeatherMapCloudTypeNegationWarpMax.y),
                        Mathf.Lerp(WeatherMapCloudTypeNegationWarpMin.z, WeatherMapCloudTypeNegationWarpMax.z, l1),
                        Mathf.Lerp(WeatherMapCloudTypeNegationWarpMin.w, WeatherMapCloudTypeNegationWarpMax.w, l2)
                    );
                    weatherMapCloudTypeNegationWarpMinPrev = WeatherMapCloudTypeNegationWarpMin;
                    weatherMapCloudTypeNegationWarpMaxPrev = WeatherMapCloudTypeNegationWarpMax;
                }
                return weatherMapCloudTypeNegationWarp.Value;
            }
            set
            {
                weatherMapCloudTypeNegationWarp = value;
                weatherMapCloudTypeNegationWarpMinPrev = WeatherMapCloudTypeNegationWarpMin;
                weatherMapCloudTypeNegationWarpMaxPrev = WeatherMapCloudTypeNegationWarpMax;
            }
        }

        /// <summary>Cloud height.</summary>
        [Header("Planet (volumetric only)")]
        [SingleLine("Cloud height.")]
        public RangeOfFloats CloudHeight = new RangeOfFloats { Minimum = 1500, Maximum = 2000 };

        /// <summary>Cloud height top - clouds extend from CloudHeight to this value.</summary>
        [SingleLine("Cloud height top - clouds extend from CloudHeight to this value.")]
        public RangeOfFloats CloudHeightTop = new RangeOfFloats { Minimum = 4000, Maximum = 5000 };

        /// <summary>Planet radius for sphere cloud mapping. 1200000.0 seems to work well.</summary>
        [Tooltip("Planet radius for sphere cloud mapping. 1200000.0 seems to work well.")]
        public float CloudPlanetRadius = 1200000.0f;

        /// <summary>
        /// Checks whether clouds are enabled
        /// </summary>
        public bool CloudsEnabled { get; private set; }

        /// <summary>
        /// Sum of cloud cover, max of 1
        /// </summary>
        public float CloudCoverTotal { get; private set; }

        /// <summary>
        /// Sum of cloud density, max of 1
        /// </summary>
        public float CloudDensityTotal { get; private set; }

        /// <summary>
        /// Sum of cloud light absorption, max of 1
        /// </summary>
        public float CloudLightAbsorptionTotal { get; private set; }

        /// <summary>
        /// Aurora profile
        /// </summary>
        public WeatherMakerAuroraProfileScript AuroraProfile { get; set; }

        /// <summary>
        /// Atmosphere profile
        /// </summary>
        public WeatherMakerAtmosphereProfileScript AtmosphereProfile { get; set; }

        /// <summary>
        /// Directional light block
        /// </summary>
        public float CloudDirectionalLightDirectBlock { get; private set; }

        private Vector3 cloudNoiseVelocityAccum1;
        private Vector3 cloudNoiseVelocityAccum2;
        private Vector3 cloudNoiseVelocityAccum3;
        private Vector3 cloudNoiseVelocityAccum4;

        private Vector3 velocityAccumCoverage;
        private Vector3 velocityAccumDensity;
        private Vector3 velocityAccumType;
        private Vector3 velocityAccumCoverageNegation;
        private Vector3 velocityAccumDensityNegation;
        private Vector3 velocityAccumTypeNegation;

        internal bool isAnimating;
        internal DigitalRuby.WeatherMaker.IRandomizer randomizer = DigitalRuby.WeatherMaker.WeatherMakerRandomizer.Unity;

        //unused currently
        //internal Vector3 cloudCoverageOffset;
        //internal Vector3 cloudCoverageNegationOffset;
        //internal Vector3 cloudTypeOffset;
        //internal Vector3 cloudTypeNegationOffset;

        private readonly WeatherMakerShaderPropertiesScript shaderProps = new WeatherMakerShaderPropertiesScript();

        private void SetShaderVolumetricCloudShaderProperties(WeatherMakerShaderPropertiesScript props, Texture weatherMap, WeatherMakerCelestialObjectScript sun, Camera camera)
        {
            float sunLookup = sun.GetGradientLookup();
            float ambientMultiplier = (WeatherMakerFullScreenCloudsScript.Instance == null || WeatherMakerFullScreenCloudsScript.Instance.GlobalAmbientMultiplierGradient == null ||
                WeatherMakerFullScreenCloudsScript.Instance.GlobalAmbientMultiplierGradient.alphaKeys.Length == 0 ? 1.0f :
                WeatherMakerFullScreenCloudsScript.Instance.GlobalAmbientMultiplierGradient.Evaluate(sunLookup).a);

            if (!isAnimating)
            {
                CloudLayerVolumetric1.UpdateCurves();
            }

            if (weatherMap != null)
            {
                props.SetTexture(WMS._WeatherMakerWeatherMapTexture, weatherMap);
                props.SetVector(WMS._WeatherMakerWeatherMapScale, new Vector4(NoiseScaleX.GetLastValue(randomizer),
                    NoiseScaleY.GetLastValue(randomizer),
                    WorldScale,
                    (1.0f / WorldScale) / (float)weatherMap.width));
            }
            props.SetFloat(WMS._CloudInvFade, 20.0f * (1.0f / camera.farClipPlane));
            props.SetFloat(WMS._CloudCoverVolumetricMinimumForCloud, CloudLayerVolumetric1.CloudCoverMinimum);
            props.SetFloat(WMS._CloudCoverVolumetric, CloudLayerVolumetric1.CloudCover.GetLastValue(randomizer));
            props.SetFloatArray(WMS._CloudCoverageCurve, CloudLayerVolumetric1.cloudCoverCurve);
            props.SetFloat(WMS._CloudCoverSecondaryVolumetric, CloudLayerVolumetric1.CloudCoverSecondary.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudDensityVolumetric, CloudLayerVolumetric1.CloudDensity.GetLastValue(randomizer));
            props.SetFloatArray(WMS._CloudDensityCurve, CloudLayerVolumetric1.cloudDensityCurve);
            props.SetFloat(WMS._CloudDensitySecondaryVolumetric, CloudLayerVolumetric1.CloudDensitySecondary.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudTypeVolumetric, CloudLayerVolumetric1.CloudType.GetLastValue(randomizer));
            props.SetFloatArray(WMS._CloudTypeCurve, CloudLayerVolumetric1.cloudTypeCurve);
            props.SetFloat(WMS._CloudTypeSecondaryVolumetric, CloudLayerVolumetric1.CloudTypeSecondary.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudNoiseScalarVolumetric, CloudLayerVolumetric1.CloudNoiseShapeScalar.GetLastValue(randomizer) *
                WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudNoiseMultiplier);
            props.SetVector(WMS._CloudWeights, CloudLayerVolumetric1.CloudNoiseShapeWeights);
            props.SetInt(WMS._CloudDirLightRaySampleCount, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudDirLightRaySampleCount);
            props.SetFloat(WMS._CloudPlanetRadiusVolumetric, CloudPlanetRadius);
            props.SetTexture(WMS._CloudNoiseShapeVolumetric, CloudLayerVolumetric1.CloudNoiseShape);
            props.SetTexture(WMS._CloudNoiseDetailVolumetric, CloudLayerVolumetric1.CloudNoiseDetail);
            props.SetTexture(WMS._CloudNoiseCurlVolumetric, CloudLayerVolumetric1.CloudNoiseCurl);
            props.SetVector(WMS._CloudShapeAnimationVelocity, CloudLayerVolumetric1.CloudShapeAnimationVelocity);
            props.SetVector(WMS._CloudDetailAnimationVelocity, CloudLayerVolumetric1.CloudDetailAnimationVelocity);
            props.SetFloatArray(WMS._CloudNoiseDetailPowerCurveVolumetric, CloudLayerVolumetric1.cloudNoiseDetailPowerCurve);

            Vector4 cloudNoiseScale = new Vector4(CloudLayerVolumetric1.CloudNoiseShapeScale.GetLastValue(randomizer),
                CloudLayerVolumetric1.CloudNoiseDetailScale.GetLastValue(randomizer),
                CloudLayerVolumetric1.CloudNoiseDetailCurlScale.GetLastValue(randomizer),
                CloudLayerVolumetric1.CloudNoiseDetailCurlIntensity.GetLastValue(randomizer));
            props.SetVector(WMS._CloudNoiseScaleVolumetric, cloudNoiseScale);
            props.SetFloat(WMS._CloudNoiseDetailPowerVolumetric, CloudLayerVolumetric1.CloudNoiseDetailPower.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudNoiseDetailHeightMultiplier, CloudLayerVolumetric1.CloudNoiseDetailHeightMultiplier.GetLastValue(randomizer));

            if (WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudAllowAnvils)
            {
                props.SetFloat(WMS._CloudAnvilStrength, CloudLayerVolumetric1.CloudAnvilStrength.GetLastValue(randomizer));
                props.SetFloat(WMS._CloudAnvilStart, CloudLayerVolumetric1.CloudAnvilStart.GetLastValue(randomizer));
            }
            else
            {
                props.SetFloat(WMS._CloudAnvilStrength, 0.0f);
                props.SetFloat(WMS._CloudAnvilStart, 1.0f);
            }

            // lower cloud level sphere
            // assign global shader so shadow map can see them
            props.SetFloat(WMS._CloudStartVolumetric, CloudHeight.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudStartInverseVolumetric, 1.0f / CloudHeight.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudStartSquaredVolumetric, CloudHeight.GetLastValue(randomizer) * CloudHeight.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudPlanetStartVolumetric, CloudHeight.GetLastValue(randomizer) + CloudPlanetRadius);
            props.SetFloat(WMS._CloudPlanetStartSquaredVolumetric, Mathf.Pow(CloudHeight.GetLastValue(randomizer) + CloudPlanetRadius, 2.0f));

            // height of top minus bottom cloud layer
            float height = CloudHeightTop.GetLastValue(randomizer) - CloudHeight.GetLastValue(randomizer);
            props.SetFloat(WMS._CloudHeightVolumetric, height);
            props.SetFloat(WMS._CloudHeightInverseVolumetric, 1.0f / height);
            height *= height;
            props.SetFloat(WMS._CloudHeightSquaredVolumetric, height);
            props.SetFloat(WMS._CloudHeightSquaredInverseVolumetric, 1.0f / height);

            // upper cloud level sphere
            props.SetFloat(WMS._CloudEndVolumetric, CloudHeightTop.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudEndInverseVolumetric, CloudHeightTop.GetLastValue(randomizer));
            height = CloudHeightTop.GetLastValue(randomizer) * CloudHeightTop.GetLastValue(randomizer);
            props.SetFloat(WMS._CloudEndSquaredVolumetric, height);
            props.SetFloat(WMS._CloudEndSquaredInverseVolumetric, 1.0f / height);
            props.SetFloat(WMS._CloudPlanetEndVolumetric, CloudHeightTop.GetLastValue(randomizer) + CloudPlanetRadius);
            props.SetFloat(WMS._CloudPlanetEndSquaredVolumetric, Mathf.Pow(CloudHeightTop.GetLastValue(randomizer) + CloudPlanetRadius, 2.0f));

            props.SetFloat(WMS._CloudPlanetRadiusNegativeVolumetric, -CloudPlanetRadius);
            props.SetFloat(WMS._CloudPlanetRadiusSquaredVolumetric, CloudPlanetRadius * CloudPlanetRadius);

            props.SetVector(WMS._CloudHenyeyGreensteinPhaseVolumetric, CloudLayerVolumetric1.CloudHenyeyGreensteinPhase);
            props.SetFloat(WMS._CloudRayOffsetVolumetric, CloudLayerVolumetric1.CloudRayOffset +
                (WeatherMakerFullScreenCloudsScript.Instance == null ? 0.0f : WeatherMakerFullScreenCloudsScript.Instance.CloudRayOffset));

            props.SetVector(WMS._CloudGradientStratus, CloudLayerVolumetric1.CloudGradientStratusVector);
            props.SetVector(WMS._CloudGradientStratoCumulus, CloudLayerVolumetric1.CloudGradientStratoCumulusVector);
            props.SetVector(WMS._CloudGradientCumulus, CloudLayerVolumetric1.CloudGradientCumulusVector);

            props.SetVector(WMS._CloudNoiseSampleCountVolumetric, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudSampleCount.ToVector2());
            props.SetFloat(WMS._CloudRaymarchMaybeInCloudStepMultiplier, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudRaymarchMaybeInCloudStepMultiplier);
            props.SetFloat(WMS._CloudRaymarchInCloudStepMultiplier, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudRaymarchInCloudStepMultiplier);
            props.SetFloat(WMS._CloudNoiseLod, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudFlatLayerLod);
            props.SetVector(WMS._CloudNoiseLodVolumetric, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudLod.ToVector2());
            props.SetColor(WMS._CloudColorVolumetric, CloudLayerVolumetric1.CloudColor);
            props.SetColor(WMS._CloudDirColorVolumetric, CloudLayerVolumetric1.CloudDirLightGradientColorColor);
            props.SetColor(WMS._CloudEmissionColorVolumetric, CloudLayerVolumetric1.CloudEmissionGradientColorColor);
            props.SetFloat(WMS._CloudDirLightMultiplierVolumetric, CloudLayerVolumetric1.CloudDirLightMultiplier);
            props.SetFloat(WMS._WeatherMakerDirectionalLightScatterMultiplier, DirectionalLightScatterMultiplier);
            props.SetFloat(WMS._WeatherMakerCloudAtmosphereShadow, DirectionalLightAtmosphericShadow);
            props.SetFloat(WMS._CloudLightDitherLevel, CloudLayerVolumetric1.CloudLightDitherLevel);
            props.SetFloat(WMS._CloudPointSpotLightMultiplierVolumetric, CloudLayerVolumetric1.CloudPointSpotLightMultiplier);
            props.SetFloat(WMS._CloudAmbientGroundIntensityVolumetric, CloudLayerVolumetric1.CloudAmbientGroundIntensity * ambientMultiplier);
            props.SetFloat(WMS._CloudAmbientSkyIntensityVolumetric, CloudLayerVolumetric1.CloudAmbientSkyIntensity * ambientMultiplier);
            float backgroundSkyStyle = (WeatherMakerSkySphereScript.Instance == null || WeatherMakerSkySphereScript.Instance.SkySphereProfile == null ||
                WeatherMakerSkySphereScript.Instance.SkySphereProfile.SkyMode == WeatherMakeSkyMode.ProceduralUnityStyle ||
                WeatherMakerSkySphereScript.Instance.SkySphereProfile.SkyMode == WeatherMakeSkyMode.ProceduralTexturedUnityStyle ? 0.0f : 1.0f);
            props.SetFloat(WMS._CloudAmbientSkyHeightMultiplierVolumetric, CloudLayerVolumetric1.CloudAmbientSkyHeightMultiplier);
            props.SetFloat(WMS._CloudAmbientGroundHeightMultiplierVolumetric, CloudLayerVolumetric1.CloudAmbientGroundHeightMultiplier * ambientMultiplier);
            props.SetFloat(WMS._CloudAmbientShadowVolumetric, CloudLayerVolumetric1.CloudAmbientShadow * 0.333f);
            props.SetFloat(WMS._CloudLightAbsorptionVolumetric, CloudLayerVolumetric1.CloudLightAbsorption * WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudAbsorptionMultiplier);
            props.SetFloat(WMS._CloudDirLightIndirectMultiplierVolumetric, CloudLayerVolumetric1.CloudDirLightIndirectMultiplier);
            props.SetFloat(WMS._CloudPowderMultiplierVolumetric, CloudLayerVolumetric1.CloudPowderMultiplier.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudOpticalDistanceMultiplierVolumetric, CloudLayerVolumetric1.CloudOpticalDistanceMultiplier);
            props.SetFloat(WMS._CloudDirLightSampleCount, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudDirLightSampleCount);
            props.SetFloat(WMS._CloudDirLightSubSampleCount, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudDirLightSubSampleCount);
            props.SetFloat(WMS._CloudLightStepMultiplierVolumetric, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudDirLightStepMultiplier);

            Vector4 fade = CloudLayerVolumetric1.CloudHorizonFade;
            const float horizonDawnDuskMultiplier = 0.5f;
            const float horizonNightMultiplier = 0.0f;
            float horizonDawnDusk = WeatherMakerDayNightCycleManagerScript.Instance.DawnDuskMultiplier * horizonDawnDuskMultiplier;
            float horizonNight = WeatherMakerDayNightCycleManagerScript.Instance.NightMultiplier * horizonNightMultiplier;
            float horizonTotal = 1.0f - Mathf.Min(1.0f, horizonDawnDusk + horizonNight);
            fade.x = Mathf.Clamp(fade.x, 0.0f, 16.0f);
            fade.y = Mathf.Clamp(fade.y, 0.1f, 16.0f);
            fade.z = Mathf.Clamp(fade.z, 0.1f, 16.0f);
            fade.w = Mathf.Clamp(fade.w * horizonTotal, 0.0f, 1.0f);
            props.SetVector(WMS._CloudHorizonFadeVolumetric, fade);

            // increase light steps as light goes to horizon for more realistic lighting
            WeatherMakerCelestialObjectScript primaryLight = WeatherMakerLightManagerScript.Instance.PrimaryDirectionalLight;
            float lightGradient = (primaryLight == null ? 1.0f : Mathf.Min(1.0f, 4.0f * Mathf.Abs(primaryLight.transform.forward.y)));
            lightGradient *= lightGradient;
            lightGradient *= lightGradient;
            int stepCount = WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudDirLightSampleCount;
            float stepCountMultiplier = WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudDirLightHorizonMultiplier;
            float lightStepCountMultiplier = Mathf.Lerp(stepCountMultiplier, 1.0f, lightGradient);
            int maxStepCount = Mathf.Min(16, Mathf.RoundToInt((float)stepCount * lightStepCountMultiplier));
            props.SetFloat(WMS._CloudDirLightSampleCount, maxStepCount);

            props.SetFloat(WMS._CloudLightStepMultiplierVolumetric,
                WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudDirLightStepMultiplier);
            props.SetFloat(WMS._CloudLightRadiusMultiplierVolumetric, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudDirLightRadiusMultiplier);
            props.SetFloat(WMS._CloudLightDistantMultiplierVolumetric, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudLightDistantMultiplier);
            props.SetVector(WMS._CloudRayDitherVolumetric, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudRayDither.ToVector2() *
                CloudLayerVolumetric1.CloudDitherMultiplier.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudRaymarchMultiplierVolumetric, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudRaymarchMultiplier);
            props.SetVector(WMS._CloudRayMarchParameters, CloudLayerVolumetric1.CloudRayMarchParameters);

            // sample details for dir light ray march if max lod is small
            props.SetFloat(WMS._CloudDirLightLod, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudDirLightLod);
            props.SetInt(WMS._CloudRaymarchSampleDetailsForDirLight, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudDirLightSampleDetails ? 1 : 0);
            props.SetFloat(WMS._WeatherMakerCloudShadowSampleShadowMap, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudShadowMapMinPower);

            // flat clouds
            float cloudCover1 = 0.0f;
            float cloudCover2 = 0.0f;
            float cloudCover3 = 0.0f;
            float cloudCover4 = 0.0f;
            if (isAnimating || CloudLayerVolumetric1 == null || CloudLayerVolumetric1.CloudCover.Maximum == 0.0f || !WeatherMakerScript.Instance.PerformanceProfile.EnableVolumetricClouds ||
                (CloudLayerVolumetric1.FlatLayerMask & WeatherMakerVolumetricCloudsFlatLayerMask.One) == WeatherMakerVolumetricCloudsFlatLayerMask.One)
            {
                cloudCover1 = CloudLayer1.CloudCover.GetLastValue(randomizer);
            }
            if (isAnimating || CloudLayerVolumetric1 == null || CloudLayerVolumetric1.CloudCover.Maximum == 0.0f || !WeatherMakerScript.Instance.PerformanceProfile.EnableVolumetricClouds ||
                (CloudLayerVolumetric1.FlatLayerMask & WeatherMakerVolumetricCloudsFlatLayerMask.Two) == WeatherMakerVolumetricCloudsFlatLayerMask.Two)
            {
                cloudCover2 = CloudLayer2.CloudCover.GetLastValue(randomizer);
            }
            if (isAnimating || CloudLayerVolumetric1 == null || CloudLayerVolumetric1.CloudCover.Maximum == 0.0f || !WeatherMakerScript.Instance.PerformanceProfile.EnableVolumetricClouds ||
                (CloudLayerVolumetric1.FlatLayerMask & WeatherMakerVolumetricCloudsFlatLayerMask.Three) == WeatherMakerVolumetricCloudsFlatLayerMask.Three)
            {
                cloudCover3 = CloudLayer3.CloudCover.GetLastValue(randomizer);
            }
            if (isAnimating || CloudLayerVolumetric1 == null || CloudLayerVolumetric1.CloudCover.Maximum == 0.0f || !WeatherMakerScript.Instance.PerformanceProfile.EnableVolumetricClouds ||
                (CloudLayerVolumetric1.FlatLayerMask & WeatherMakerVolumetricCloudsFlatLayerMask.Four) == WeatherMakerVolumetricCloudsFlatLayerMask.Four)
            {
                cloudCover4 = CloudLayer4.CloudCover.GetLastValue(randomizer);
            }
            props.SetTexture(WMS._CloudNoise1, CloudLayer1.CloudNoise ?? Texture2D.blackTexture);
            props.SetTexture(WMS._CloudNoise2, CloudLayer2.CloudNoise ?? Texture2D.blackTexture);
            props.SetTexture(WMS._CloudNoise3, CloudLayer3.CloudNoise ?? Texture2D.blackTexture);
            props.SetTexture(WMS._CloudNoise4, CloudLayer4.CloudNoise ?? Texture2D.blackTexture);
            Vector4 scaleReducer = new Vector4(1.0f / Mathf.Max(1.0f, CloudLayer1.CloudHeight.GetLastValue(randomizer)), 1.0f / Mathf.Max(1.0f, CloudLayer2.CloudHeight.GetLastValue(randomizer)),
                1.0f / Mathf.Max(1.0f, CloudLayer3.CloudHeight.GetLastValue(randomizer)), 1.0f / Mathf.Max(1.0f, CloudLayer4.CloudHeight.GetLastValue(randomizer)));
            props.SetVector(WMS._CloudNoiseScale, new Vector4(CloudLayer1.CloudNoiseScale.GetLastValue(randomizer) * scaleReducer.x,
                CloudLayer2.CloudNoiseScale.GetLastValue(randomizer) * scaleReducer.y, CloudLayer3.CloudNoiseScale.GetLastValue(randomizer) * scaleReducer.z,
                CloudLayer4.CloudNoiseScale.GetLastValue(randomizer) * scaleReducer.w));
            props.SetVector(WMS._CloudNoiseMultiplier, new Vector4(CloudLayer1.CloudNoiseMultiplier.GetLastValue(randomizer),
                CloudLayer2.CloudNoiseMultiplier.GetLastValue(randomizer), CloudLayer3.CloudNoiseMultiplier.GetLastValue(randomizer), CloudLayer4.CloudNoiseMultiplier.GetLastValue(randomizer)));
            props.SetVector(WMS._CloudNoiseAdder, new Vector4(CloudLayer1.CloudNoiseAdder.GetLastValue(randomizer),
                CloudLayer2.CloudNoiseAdder.GetLastValue(randomizer), CloudLayer3.CloudNoiseAdder.GetLastValue(randomizer), CloudLayer4.CloudNoiseAdder.GetLastValue(randomizer)));
            props.SetVector(WMS._CloudNoiseDither, new Vector4(CloudLayer1.CloudNoiseDither.GetLastValue(randomizer), CloudLayer2.CloudNoiseDither.GetLastValue(randomizer),
                CloudLayer3.CloudNoiseDither.GetLastValue(randomizer), CloudLayer4.CloudNoiseDither.GetLastValue(randomizer)));
            props.SetVectorArray(WMS._CloudNoiseVelocity, cloudNoiseVelocityAccum1, cloudNoiseVelocityAccum2, cloudNoiseVelocityAccum3, cloudNoiseVelocityAccum4);
            props.SetFloatArrayRotation(WMS._CloudNoiseRotation, CloudLayer1.CloudNoiseRotation.GetLastValue(randomizer), CloudLayer2.CloudNoiseRotation.GetLastValue(randomizer), CloudLayer3.CloudNoiseRotation.GetLastValue(randomizer), CloudLayer4.CloudNoiseRotation.GetLastValue(randomizer));
            props.SetVector(WMS._CloudHeight, new Vector4(CloudLayer1.CloudHeight.GetLastValue(randomizer), CloudLayer2.CloudHeight.GetLastValue(randomizer),
                CloudLayer3.CloudHeight.GetLastValue(randomizer), CloudLayer4.CloudHeight.GetLastValue(randomizer)));
            props.SetVector(WMS._CloudCover, new Vector4(cloudCover1, cloudCover2, cloudCover3, cloudCover4));
            props.SetFloatArray(WMS._CloudRayOffset, CloudLayer1.CloudRayOffset, CloudLayer2.CloudRayOffset, CloudLayer3.CloudRayOffset, CloudLayer4.CloudRayOffset);
            props.SetColorArray(WMS._CloudColor,
                CloudLayer1.CloudColor * sun.GetGradientColor(CloudLayer1.CloudGradientColor),
                CloudLayer2.CloudColor * sun.GetGradientColor(CloudLayer2.CloudGradientColor),
                CloudLayer3.CloudColor * sun.GetGradientColor(CloudLayer3.CloudGradientColor),
                CloudLayer4.CloudColor * sun.GetGradientColor(CloudLayer4.CloudGradientColor));
            props.SetColorArray(WMS._CloudEmissionColor,
                CloudLayer1.CloudEmissionColor,
                CloudLayer2.CloudEmissionColor,
                CloudLayer3.CloudEmissionColor,
                CloudLayer4.CloudEmissionColor);
            props.SetFloatArray(WMS._CloudAmbientGroundMultiplier,
                CloudLayer1.CloudAmbientGroundMultiplier.GetLastValue(randomizer) * ambientMultiplier,
                CloudLayer2.CloudAmbientGroundMultiplier.GetLastValue(randomizer) * ambientMultiplier,
                CloudLayer3.CloudAmbientGroundMultiplier.GetLastValue(randomizer) * ambientMultiplier,
                CloudLayer4.CloudAmbientGroundMultiplier.GetLastValue(randomizer) * ambientMultiplier);
            props.SetFloatArray(WMS._CloudAmbientSkyMultiplier,
                CloudLayer1.CloudAmbientSkyMultiplier.GetLastValue(randomizer) * ambientMultiplier,
                CloudLayer2.CloudAmbientSkyMultiplier.GetLastValue(randomizer) * ambientMultiplier,
                CloudLayer3.CloudAmbientSkyMultiplier.GetLastValue(randomizer) * ambientMultiplier,
                CloudLayer4.CloudAmbientSkyMultiplier.GetLastValue(randomizer) * ambientMultiplier);
            props.SetVectorArray(WMS._CloudScatterMultiplier,
                CloudLayer1.CloudScatterMultiplier,
                CloudLayer2.CloudScatterMultiplier,
                CloudLayer3.CloudScatterMultiplier,
                CloudLayer4.CloudScatterMultiplier);
            /*
            if (CloudLayer1.CloudNoiseMask != null || CloudLayer2.CloudNoiseMask != null || CloudLayer3.CloudNoiseMask != null || CloudLayer4.CloudNoiseMask != null)
            {
                cloudMaterial.SetTexture(WMS._CloudNoiseMask1, CloudLayer1.CloudNoiseMask ?? Texture2D.whiteTexture);
                cloudMaterial.SetTexture(WMS._CloudNoiseMask2, CloudLayer2.CloudNoiseMask ?? Texture2D.whiteTexture);
                cloudMaterial.SetTexture(WMS._CloudNoiseMask3, CloudLayer3.CloudNoiseMask ?? Texture2D.whiteTexture);
                cloudMaterial.SetTexture(WMS._CloudNoiseMask4, CloudLayer4.CloudNoiseMask ?? Texture2D.whiteTexture);
                WeatherMakerShaderIds.SetVectorArray(cloudMaterial, "_CloudNoiseMaskOffset",
                    CloudLayer1.CloudNoiseMaskOffset,
                    CloudLayer2.CloudNoiseMaskOffset,
                    CloudLayer3.CloudNoiseMaskOffset,
                    CloudLayer4.CloudNoiseMaskOffset);
                WeatherMakerShaderIds.SetVectorArray(cloudMaterial, "_CloudNoiseMaskVelocity", cloudNoiseMaskVelocityAccum1, cloudNoiseMaskVelocityAccum2, cloudNoiseMaskVelocityAccum3, cloudNoiseMaskVelocityAccum4);
                WeatherMakerShaderIds.SetFloatArray(cloudMaterial, "_CloudNoiseMaskScale",
                    (CloudLayer1.CloudNoiseMask == null ? 0.0f : CloudLayer1.CloudNoiseMaskScale * scaleReducer),
                    (CloudLayer2.CloudNoiseMask == null ? 0.0f : CloudLayer2.CloudNoiseMaskScale * scaleReducer),
                    (CloudLayer3.CloudNoiseMask == null ? 0.0f : CloudLayer3.CloudNoiseMaskScale * scaleReducer),
                    (CloudLayer4.CloudNoiseMask == null ? 0.0f : CloudLayer4.CloudNoiseMaskScale * scaleReducer));
                WeatherMakerShaderIds.SetFloatArrayRotation(cloudMaterial, "_CloudNoiseMaskRotation",
                    CloudLayer1.CloudNoiseMaskRotation.GetLastValue(randomizer),
                    CloudLayer2.CloudNoiseMaskRotation.GetLastValue(randomizer),
                    CloudLayer3.CloudNoiseMaskRotation.GetLastValue(randomizer),
                    CloudLayer4.CloudNoiseMaskRotation.GetLastValue(randomizer));
            }
            */
            props.SetVector(WMS._CloudLightAbsorption, new Vector4(
                CloudLayer1.CloudLightAbsorption.GetLastValue(randomizer),
                CloudLayer2.CloudLightAbsorption.GetLastValue(randomizer),
                CloudLayer3.CloudLightAbsorption.GetLastValue(randomizer),
                CloudLayer4.CloudLightAbsorption.GetLastValue(randomizer)));

            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.UpdateShaderPropertiesForCamera(props, camera);
            }
        }

        /// <summary>
        /// Set shader cloud values
        /// </summary>
        /// <param name="cloudMaterial">Cloud material</param>
        /// <param name="cloudProbe">Cloud probe shader</param>
        /// <param name="camera">Camera</param>
        /// <param name="weatherMap">Weather map texture</param>
        public void SetShaderCloudParameters(Material cloudMaterial, ComputeShader cloudProbe, Camera camera, Texture weatherMap)
        {
            if (WeatherMakerScript.Instance == null ||
                WeatherMakerScript.Instance.PerformanceProfile == null ||
                WeatherMakerDayNightCycleManagerScript.Instance == null ||
                WeatherMakerLightManagerScript.Instance == null)
            {
                return;
            }

            WeatherMakerCelestialObjectScript sun = (camera == null || !camera.orthographic ? WeatherMakerLightManagerScript.Instance.SunPerspective : WeatherMakerLightManagerScript.Instance.SunOrthographic);
            if (sun == null)
            {
                return;
            }

            if (!isAnimating)
            {
                CloudLayerVolumetric1.CloudDirLightGradientColorColor = sun.GetGradientColor(CloudLayerVolumetric1.CloudDirLightGradientColor);
                CloudLayerVolumetric1.CloudEmissionGradientColorColor = sun.GetGradientColor(CloudLayerVolumetric1.CloudEmissionGradientColor);
                CloudLayerVolumetric1.CloudGradientStratusVector = WeatherMakerCloudVolumetricProfileScript.CloudHeightGradientToVector4(CloudLayerVolumetric1.CloudGradientStratus);
                CloudLayerVolumetric1.CloudGradientStratoCumulusVector = WeatherMakerCloudVolumetricProfileScript.CloudHeightGradientToVector4(CloudLayerVolumetric1.CloudGradientStratoCumulus);
                CloudLayerVolumetric1.CloudGradientCumulusVector = WeatherMakerCloudVolumetricProfileScript.CloudHeightGradientToVector4(CloudLayerVolumetric1.CloudGradientCumulus);
            }

            // update global shader values
            shaderProps.Update(null);
            SetShaderVolumetricCloudShaderProperties(shaderProps, weatherMap, sun, camera);

            // update compute shader values
            if (cloudProbe != null)
            {
                shaderProps.Update(cloudProbe);
                SetShaderVolumetricCloudShaderProperties(shaderProps, weatherMap, sun, camera);
            }

            Shader.SetGlobalInt(WMS._WeatherMakerCloudVolumetricShadowSampleCount, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudShadowSampleCount);
            Shader.SetGlobalInt(WMS._WeatherMakerCloudVolumetricShadowLod, WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudShadowLod);

            if (CloudsEnabled)
            {
                float cover = Mathf.Min(1.0f, CloudCoverTotal * (1.5f - CloudLightAbsorptionTotal));
                float sunIntensityMultiplier = Mathf.Clamp(1.0f - (cover * CloudLightStrength), 0.2f, 1.0f);
                float sunIntensityMultiplierWithoutLightStrength = Mathf.Clamp(1.0f - (cover * cover * 0.85f), 0.2f, 1.0f);
                float cloudShadowReducer = sunIntensityMultiplierWithoutLightStrength;
                float dirLightMultiplier = sunIntensityMultiplier * Mathf.Lerp(1.0f, DirectionalLightIntensityMultiplier, cover);
                Shader.SetGlobalFloat(WMS._WeatherMakerCloudGlobalShadow2, cloudShadowReducer);
                CloudGlobalShadow = cloudShadowReducer = Mathf.Min(cloudShadowReducer, Shader.GetGlobalFloat(WMS._WeatherMakerCloudGlobalShadow));
                CloudDirectionalLightDirectBlock = dirLightMultiplier;

                // if we have shadows turned on, use screen space shadows
                if (QualitySettings.shadows != ShadowQuality.Disable && WeatherMakerLightManagerScript.ScreenSpaceShadowMode != UnityEngine.Rendering.BuiltinShaderMode.Disabled &&
                    WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudShadowDownsampleScale != WeatherMakerDownsampleScale.Disabled &&
                    WeatherMakerScript.Instance.PerformanceProfile.VolumetricCloudShadowSampleCount > 0)
                {
                    // do not reduce light intensity or shadows, screen space shadows are being used
                    WeatherMakerLightManagerScript.Instance.DirectionalLightIntensityMultipliers.Remove("WeatherMakerFullScreenCloudsScript");
                    Shader.SetGlobalFloat(WMS._WeatherMakerDirLightMultiplier, 1.0f);
                    Shader.SetGlobalFloat(WMS._WeatherMakerCloudGlobalShadow, 1.0f);
                }
                else
                {
                    // save dir light multiplier so flat clouds can adjust to the dimmed light
                    Shader.SetGlobalFloat(WMS._WeatherMakerDirLightMultiplier, 1.0f / Mathf.Max(0.0001f, dirLightMultiplier));
                    Shader.SetGlobalFloat(WMS._WeatherMakerCloudGlobalShadow, cloudShadowReducer);

                    // brighten up on orthographic, looks better
                    if (WeatherMakerScript.Instance.MainCamera != null && WeatherMakerScript.Instance.MainCamera.orthographic)
                    {
                        sunIntensityMultiplier = Mathf.Min(1.0f, sunIntensityMultiplier * 2.0f);
                    }

                    // we rely on sun intensity reduction to fake shadows
                    WeatherMakerLightManagerScript.Instance.DirectionalLightIntensityMultipliers["WeatherMakerFullScreenCloudsScript"] = dirLightMultiplier;
                }
            }
            else
            {
                WeatherMakerLightManagerScript.Instance.DirectionalLightIntensityMultipliers.Remove("WeatherMakerFullScreenCloudsScript");
                WeatherMakerLightManagerScript.Instance.DirectionalLightShadowIntensityMultipliers.Remove("WeatherMakerFullScreenCloudsScript");
            }
        }

        private void LoadDefaultLayerIfNeeded(ref WeatherMakerCloudLayerProfileScript script)
        {
            if (script == null && WeatherMakerScript.Instance != null)
            {
                script = WeatherMakerScript.Instance.LoadResource<WeatherMakerCloudLayerProfileScript>("WeatherMakerCloudLayerProfile_None");
            }
        }

        private void LoadDefaultLayerIfNeeded(ref WeatherMakerCloudVolumetricProfileScript script)
        {
            if (script == null && WeatherMakerScript.Instance != null)
            {
                script = WeatherMakerScript.Instance.LoadResource<WeatherMakerCloudVolumetricProfileScript>("WeatherMakerCloudLayerProfileVolumetric_None");
            }
        }

        private void CheckWarpChange()
        {
            // support editor changes for warping
            if (weatherMapCloudCoverageWarpMinPrev != WeatherMapCloudCoverageWarpMin ||
                weatherMapCloudCoverageWarpMaxPrev != WeatherMapCloudCoverageWarpMax)
            {
                weatherMapCloudCoverageWarpMinPrev = WeatherMapCloudCoverageWarpMin;
                weatherMapCloudCoverageWarpMaxPrev = WeatherMapCloudCoverageWarpMax;
                weatherMapCloudCoverageWarp = null;
            }
            if (weatherMapCloudCoverageNegationWarpMinPrev != WeatherMapCloudCoverageNegationWarpMin ||
                weatherMapCloudCoverageNegationWarpMaxPrev != WeatherMapCloudCoverageNegationWarpMax)
            {
                weatherMapCloudCoverageNegationWarpMinPrev = WeatherMapCloudCoverageNegationWarpMin;
                weatherMapCloudCoverageNegationWarpMaxPrev = WeatherMapCloudCoverageNegationWarpMax;
                weatherMapCloudCoverageNegationWarp = null;
            }

            if (weatherMapCloudTypeWarpMinPrev != WeatherMapCloudTypeWarpMin ||
                weatherMapCloudTypeWarpMaxPrev != WeatherMapCloudTypeWarpMax)
            {
                weatherMapCloudTypeWarpMinPrev = WeatherMapCloudTypeWarpMin;
                weatherMapCloudTypeWarpMaxPrev = WeatherMapCloudTypeWarpMax;
                weatherMapCloudTypeWarp = null;
            }
            if (weatherMapCloudTypeNegationWarpMinPrev != WeatherMapCloudTypeNegationWarpMin ||
                weatherMapCloudTypeNegationWarpMaxPrev != WeatherMapCloudTypeNegationWarpMax)
            {
                weatherMapCloudTypeNegationWarpMinPrev = WeatherMapCloudTypeNegationWarpMin;
                weatherMapCloudTypeNegationWarpMaxPrev = WeatherMapCloudTypeNegationWarpMax;
                weatherMapCloudTypeNegationWarp = null;
            }
        }

        private void UpdateWeatherMap(WeatherMakerFullScreenCloudsScript script, WeatherMakerShaderPropertiesScript props, Texture weatherMap, float weatherMapSeed, Camera camera)
        {
            props.SetTexture(WMS._WeatherMakerWeatherMapTexture, weatherMap);
            props.SetVector(WMS._WeatherMakerWeatherMapScale, new Vector4(NoiseScaleX.GetLastValue(randomizer), NoiseScaleY.GetLastValue(randomizer), WorldScale, 1.0f / WorldScale));

            props.SetFloat(WMS._CloudCoverageNoiseType, WeatherMapCloudCoverageNoiseType.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudDensityNoiseType, WeatherMapCloudDensityNoiseType.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudTypeNoiseType, WeatherMapCloudTypeNoiseType.GetLastValue(randomizer));

            props.SetFloat(WMS._CloudCoverageNoiseTypeInv, 1.0f - WeatherMapCloudCoverageNoiseType.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudDensityNoiseTypeInv, 1.0f - WeatherMapCloudDensityNoiseType.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudTypeNoiseTypeInv, 1.0f - WeatherMapCloudTypeNoiseType.GetLastValue(randomizer));

            props.SetFloat(WMS._CloudCoverVolumetric, CloudLayerVolumetric1.CloudCover.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudDensityVolumetric, CloudLayerVolumetric1.CloudDensity.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudTypeVolumetric, CloudLayerVolumetric1.CloudType.GetLastValue(randomizer));

            props.SetFloat(WMS._CloudCoverSecondaryVolumetric, CloudLayerVolumetric1.CloudCoverSecondary.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudDensitySecondaryVolumetric, CloudLayerVolumetric1.CloudDensitySecondary.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudTypeSecondaryVolumetric, CloudLayerVolumetric1.CloudTypeSecondary.GetLastValue(randomizer));

            props.SetVector(WMS._CloudVelocity, new Vector3(WeatherMapCloudCoverageVelocity.x, WeatherMapCloudCoverageVelocity.z, WeatherMapCloudCoverageVelocity.y));
            props.SetVector(WMS._CloudCoverageVelocity, velocityAccumCoverage);
            props.SetVector(WMS._CloudDensityVelocity, velocityAccumDensity);
            props.SetVector(WMS._CloudTypeVelocity, velocityAccumType);

            props.SetVector(WMS._CloudCoverageNegationVelocity, velocityAccumCoverageNegation);
            props.SetVector(WMS._CloudDensityNegationVelocity, velocityAccumDensityNegation);
            props.SetVector(WMS._CloudTypeNegationVelocity, velocityAccumTypeNegation);

            props.SetFloat(WMS._CloudCoverageFrequency, WeatherMapCloudCoverageScale.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudDensityFrequency, WeatherMapCloudDensityScale.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudTypeFrequency, WeatherMapCloudTypeScale.GetLastValue(randomizer));

            props.SetFloat(WMS._CloudCoverageNegationFrequency, WeatherMapCloudCoverageNegationScale.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudDensityNegationFrequency, WeatherMapCloudDensityNegationScale.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudTypeNegationFrequency, WeatherMapCloudTypeNegationScale.GetLastValue(randomizer));

            float r = WeatherMapCloudCoverageRotation.GetLastValue(randomizer) * Mathf.Deg2Rad;
            props.SetVector(WMS._CloudCoverageRotation, new Vector2(Mathf.Sin(r), Mathf.Cos(r)));
            r = WeatherMapCloudDensityNegationRotation.GetLastValue(randomizer) * Mathf.Deg2Rad;
            props.SetVector(WMS._CloudDensityRotation, new Vector2(Mathf.Sin(r), Mathf.Cos(r)));
            r = WeatherMapCloudTypeNegationRotation.GetLastValue(randomizer) * Mathf.Deg2Rad;
            props.SetVector(WMS._CloudTypeRotation, new Vector2(Mathf.Sin(r), Mathf.Cos(r)));

            r = WeatherMapCloudCoverageNegationRotation.GetLastValue(randomizer) * Mathf.Deg2Rad;
            props.SetVector(WMS._CloudCoverageNegationRotation, new Vector2(Mathf.Sin(r), Mathf.Cos(r)));
            r = WeatherMapCloudDensityNegationRotation.GetLastValue(randomizer) * Mathf.Deg2Rad;
            props.SetVector(WMS._CloudDensityNegationRotation, new Vector2(Mathf.Sin(r), Mathf.Cos(r)));
            r = WeatherMapCloudTypeRotation.GetLastValue(randomizer) * Mathf.Deg2Rad;
            props.SetVector(WMS._CloudTypeNegationRotation, new Vector2(Mathf.Sin(r), Mathf.Cos(r)));

            props.SetFloat(WMS._CloudCoverageAdder, WeatherMapCloudCoverageAdder.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudDensityAdder, WeatherMapCloudDensityAdder.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudTypeAdder, WeatherMapCloudTypeAdder.GetLastValue(randomizer));

            props.SetFloat(WMS._CloudCoverageNegationAdder, WeatherMapCloudCoverageNegationAdder.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudDensityNegationAdder, WeatherMapCloudDensityNegationAdder.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudTypeNegationAdder, WeatherMapCloudTypeNegationAdder.GetLastValue(randomizer));

            //props.SetVector(WMS._CloudCoverageOffset, cloudCoverageOffset);
            //props.SetVector(WMS._CloudDensityOffset, cloudDensityOffset);
            //props.SetVector(WMS._CloudTypeOffset, cloudTypeOffset);

            //props.SetVector(WMS._CloudCoverageNegationOffset, weatherMapCloudCoverageNegationOffsetCalculated);
            //props.SetVector(WMS._CloudDensityNegationOffset, weatherMapCloudDensityNegationOffsetCalculated);
            //props.SetVector(WMS._CloudTypeNegationOffset, weatherMapCloudTypeNegationOffsetCalculated);

            props.SetFloat(WMS._CloudCoveragePower, WeatherMapCloudCoveragePower.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudDensityPower, WeatherMapCloudDensityPower.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudTypePower, WeatherMapCloudTypePower.GetLastValue(randomizer));

            props.SetFloat(WMS._CloudCoverageNegationPower, WeatherMapCloudCoverageNegationPower.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudDensityNegationPower, WeatherMapCloudDensityNegationPower.GetLastValue(randomizer));
            props.SetFloat(WMS._CloudTypeNegationPower, WeatherMapCloudTypeNegationPower.GetLastValue(randomizer));

            props.SetVector(WMS._CloudCoverageWarpScale, WeatherMapCloudCoverageWarp);
            props.SetVector(WMS._CloudDensityWarpScale, WeatherMapCloudDensityWarp);
            props.SetVector(WMS._CloudTypeWarpScale, WeatherMapCloudTypeWarp);

            props.SetVector(WMS._CloudCoverageNegationWarpScale, WeatherMapCloudCoverageNegationWarp);
            props.SetVector(WMS._CloudDensityNegationWarpScale, WeatherMapCloudDensityNegationWarp);
            props.SetVector(WMS._CloudTypeNegationWarpScale, WeatherMapCloudTypeNegationWarp);

            //props.SetVector(WMS._MaskOffset, WeatherMapRenderTextureMaskOffset);
            props.SetFloat(WMS._WeatherMakerWeatherMapSeed, weatherMapSeed);
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.UpdateShaderPropertiesForCamera(props, camera);
            }

            if (script.VolumetricCloudBoxRemap == null)
            {
                props.SetVector(WMS._WeatherMakerCloudVolumetricWeatherMapRemapBoxMin, Vector4.zero);
                props.SetVector(WMS._WeatherMakerCloudVolumetricWeatherMapRemapBoxMax, Vector4.zero);
            }
            else
            {
                Vector4 min = script.VolumetricCloudBoxRemap.bounds.min;
                Vector4 max = script.VolumetricCloudBoxRemap.bounds.max;
                min.w = max.y - min.y;
                max.w = (min.w > 0.0f ? 1.0f / min.w : 0.0f);
                props.SetVector(WMS._WeatherMakerCloudVolumetricWeatherMapRemapBoxMin, min);
                props.SetVector(WMS._WeatherMakerCloudVolumetricWeatherMapRemapBoxMax, max);
            }
        }

        /// <summary>
        /// Update the weather map
        /// </summary>
        /// <param name="script">Script</param>
        /// <param name="weatherMapMaterial">Weather map material</param>
        /// <param name="camera">Camera</param>
        /// <param name="cloudProbeShader">Cloud probe shader</param>
        /// <param name="weatherMap">Weather map render texture</param>
        /// <param name="weatherMapSeed">Weather map seed</param>
        public void UpdateWeatherMap(WeatherMakerFullScreenCloudsScript script, Material weatherMapMaterial, Camera camera, ComputeShader cloudProbeShader, RenderTexture weatherMap, float weatherMapSeed)
        {
            if (CloudLayerVolumetric1 == null)
            {
                return;
            }
            if (camera == null)
            {
                camera = Camera.main;
                if (camera == null)
                {
                    return;
                }
            }

            shaderProps.Update(null);
            UpdateWeatherMap(script, shaderProps, weatherMap, weatherMapSeed, camera);
            shaderProps.Update(weatherMapMaterial);
            UpdateWeatherMap(script, shaderProps, weatherMap, weatherMapSeed, camera);
            if (cloudProbeShader != null)
            {
                shaderProps.Update(cloudProbeShader);
                UpdateWeatherMap(script, shaderProps, weatherMap, weatherMapSeed, camera);
            }
        }

        /// <summary>
        /// Ensure all layers have profiles
        /// </summary>
        public void EnsureNonNullLayers()
        {
            LoadDefaultLayerIfNeeded(ref CloudLayer1);
            LoadDefaultLayerIfNeeded(ref CloudLayer2);
            LoadDefaultLayerIfNeeded(ref CloudLayer3);
            LoadDefaultLayerIfNeeded(ref CloudLayer4);
            LoadDefaultLayerIfNeeded(ref CloudLayerVolumetric1);
        }

        /// <summary>
        /// Deep clone the entire profile
        /// </summary>
        /// <returns></returns>
        public WeatherMakerCloudProfileScript Clone()
        {
            WeatherMakerCloudProfileScript clone = ScriptableObject.Instantiate(this);
            clone.EnsureNonNullLayers();
            clone.CloudLayer1 = ScriptableObject.Instantiate(clone.CloudLayer1);
            clone.CloudLayer2 = ScriptableObject.Instantiate(clone.CloudLayer2);
            clone.CloudLayer3 = ScriptableObject.Instantiate(clone.CloudLayer3);
            clone.CloudLayer4 = ScriptableObject.Instantiate(clone.CloudLayer4);
            clone.CloudLayerVolumetric1 = ScriptableObject.Instantiate(clone.CloudLayerVolumetric1);
            CopyStateTo(clone);
            return clone;
        }

        /// <summary>
        /// Update
        /// </summary>
        public void Update()
        {
            EnsureNonNullLayers();
            CheckWarpChange();
            CloudsEnabled =
            (
                (CloudLayerVolumetric1.CloudColor.a > 0.0f && CloudLayerVolumetric1.CloudCover.GetLastValue(randomizer) > 0.001f) ||
                (CloudLayer1.CloudNoise != null && CloudLayer1.CloudColor.a > 0.0f && CloudLayer1.CloudCover.GetLastValue(randomizer) > 0.0f) ||
                (CloudLayer2.CloudNoise != null && CloudLayer2.CloudColor.a > 0.0f && CloudLayer2.CloudCover.GetLastValue(randomizer) > 0.0f) ||
                (CloudLayer3.CloudNoise != null && CloudLayer3.CloudColor.a > 0.0f && CloudLayer3.CloudCover.GetLastValue(randomizer) > 0.0f) ||
                (CloudLayer4.CloudNoise != null && CloudLayer4.CloudColor.a > 0.0f && CloudLayer4.CloudCover.GetLastValue(randomizer) > 0.0f) ||
                (AuroraProfile != null && AuroraProfile.AuroraEnabled)
            );
            bool enableVol = WeatherMakerScript.Instance.PerformanceProfile.EnableVolumetricClouds;
            float volCover = (enableVol ? CloudLayerVolumetric1.CloudCover.GetLastValue(randomizer) : 0.0f);
            float volAbsorption = (enableVol ? Mathf.Min(1.0f, (Mathf.Clamp(1.0f - (CloudLayerVolumetric1.CloudCover.GetLastValue(randomizer)), 0.0f, 1.0f))) : 0.0f);
            //float volAbsorption = (enableVol ? Mathf.Min(1.0f, (Mathf.Clamp(1.0f - (CloudLayerVolumetric1.CloudCover.GetLastValue(randomizer)), 0.0f, 1.0f))) : 0.0f);
            CloudCoverTotal = Mathf.Min(1.0f, (CloudLayer1.CloudCover.GetLastValue(randomizer) + CloudLayer2.CloudCover.GetLastValue(randomizer) +
                CloudLayer3.CloudCover.GetLastValue(randomizer) + CloudLayer4.CloudCover.GetLastValue(randomizer) + volCover));
            CloudDensityTotal = Mathf.Min(1.0f, volCover);
            CloudLightAbsorptionTotal = volAbsorption +
                (CloudLayer1.CloudCover.GetLastValue(randomizer) * CloudLayer1.CloudLightAbsorption.GetLastValue(randomizer)) +
                (CloudLayer2.CloudCover.GetLastValue(randomizer) * CloudLayer2.CloudLightAbsorption.GetLastValue(randomizer)) +
                (CloudLayer3.CloudCover.GetLastValue(randomizer) * CloudLayer3.CloudLightAbsorption.GetLastValue(randomizer)) +
                (CloudLayer4.CloudCover.GetLastValue(randomizer) * CloudLayer4.CloudLightAbsorption.GetLastValue(randomizer));

            float velocityScaleFlat = Time.deltaTime;
            //cloudNoiseMaskVelocityAccum1 += (CloudLayer1.CloudNoiseMaskVelocity * velocityScaleFlat);
            //cloudNoiseMaskVelocityAccum2 += (CloudLayer2.CloudNoiseMaskVelocity * velocityScaleFlat);
            //cloudNoiseMaskVelocityAccum3 += (CloudLayer3.CloudNoiseMaskVelocity * velocityScaleFlat);
            //cloudNoiseMaskVelocityAccum4 += (CloudLayer4.CloudNoiseMaskVelocity * velocityScaleFlat);
            cloudNoiseVelocityAccum1 += (CloudLayer1.CloudNoiseVelocity * velocityScaleFlat);
            cloudNoiseVelocityAccum2 += (CloudLayer2.CloudNoiseVelocity * velocityScaleFlat);
            cloudNoiseVelocityAccum3 += (CloudLayer3.CloudNoiseVelocity * velocityScaleFlat);
            cloudNoiseVelocityAccum4 += (CloudLayer4.CloudNoiseVelocity * velocityScaleFlat);

            float velocityScaleVolumetric = Time.deltaTime * 10.0f * WorldScale;
            velocityAccumCoverage += (WeatherMapCloudCoverageVelocity * velocityScaleVolumetric);
            velocityAccumDensity += (WeatherMapCloudDensityVelocity * velocityScaleVolumetric);
            velocityAccumType += (WeatherMapCloudTypeVelocity * velocityScaleVolumetric);

            velocityAccumCoverageNegation += (WeatherMapCloudCoverageNegationVelocity * velocityScaleVolumetric);
            velocityAccumDensityNegation += (WeatherMapCloudDensityNegationVelocity * velocityScaleVolumetric);
            velocityAccumTypeNegation += (WeatherMapCloudTypeNegationVelocity * velocityScaleVolumetric);

            /*
            WeatherMapRenderTextureMaskOffset += (WeatherMapRenderTextureMaskVelocity * Time.deltaTime);

            // ensure mask offset does not go to far out of bounds
            WeatherMapRenderTextureMaskOffset.x = Mathf.Clamp(WeatherMapRenderTextureMaskOffset.x, WeatherMapRenderTextureMaskOffsetClamp.x, WeatherMapRenderTextureMaskOffsetClamp.y);
            WeatherMapRenderTextureMaskOffset.y = Mathf.Clamp(WeatherMapRenderTextureMaskOffset.y, WeatherMapRenderTextureMaskOffsetClamp.x, WeatherMapRenderTextureMaskOffsetClamp.y);
            */
        }

        /// <summary>
        /// Copy profile state to another profile
        /// </summary>
        /// <param name="other">Other profile</param>
        public void CopyStateTo(WeatherMakerCloudProfileScript other)
        {
            other.velocityAccumCoverage = velocityAccumCoverage;
            other.velocityAccumDensity = velocityAccumDensity;
            other.velocityAccumType = velocityAccumType;

            other.velocityAccumCoverageNegation = velocityAccumCoverageNegation;
            other.velocityAccumDensityNegation = velocityAccumDensityNegation;
            other.velocityAccumTypeNegation = velocityAccumTypeNegation;

            other.weatherMapCloudCoverageWarp = null;
            other.weatherMapCloudDensityWarp = null;
            other.weatherMapCloudTypeWarp = null;

            other.weatherMapCloudCoverageNegationWarp = null;
            other.weatherMapCloudDensityNegationWarp = null;
            other.weatherMapCloudTypeNegationWarp = null;

            other.cloudNoiseVelocityAccum1 = this.cloudNoiseVelocityAccum1;
            other.cloudNoiseVelocityAccum2 = this.cloudNoiseVelocityAccum2;
            other.cloudNoiseVelocityAccum3 = this.cloudNoiseVelocityAccum3;
            other.cloudNoiseVelocityAccum4 = this.cloudNoiseVelocityAccum4;

            other.CloudCoverTotal = this.CloudCoverTotal;
            other.CloudDensityTotal = this.CloudDensityTotal;
            other.CloudLightAbsorptionTotal = this.CloudLightAbsorptionTotal;
            other.CloudsEnabled = this.CloudsEnabled;

            /*
            other.WeatherMapRenderTextureMaskVelocity = this.WeatherMapRenderTextureMaskVelocity;
            other.WeatherMapRenderTextureMaskOffset = this.WeatherMapRenderTextureMaskOffset;
            */
        }

        /// <summary>
        /// Cloud global shadow value
        /// </summary>
        public float CloudGlobalShadow { get; private set; }
    }
}
