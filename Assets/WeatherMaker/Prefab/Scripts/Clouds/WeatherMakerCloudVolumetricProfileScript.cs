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
    /// Volumetric cloud profile
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherMakerCloudLayerVolumetricProfile", menuName = "WeatherMaker/Cloud Layer Volumetric Profile", order = 41)]
    public class WeatherMakerCloudVolumetricProfileScript : ScriptableObject
    {
        /// <summary>
        /// Number of elements in animation curves
        /// </summary>
        public const int CurveLength = 16;

        /// <summary>Cloud cover, controls how many clouds / how thick the clouds are.</summary>
        [Header("Clouds - cover")]
        [MinMaxSlider(0.0f, 1.0f, "Cloud cover, controls how many clouds / how thick the clouds are.")]
        public RangeOfFloats CloudCover = new RangeOfFloats(0.35f, 0.4f);

        /// <summary>Control cloud coverage multiplier over height.</summary>
        [Tooltip("Control cloud coverage multiplier over height.")]
        public AnimationCurve CloudCoverCurve;

        [System.NonSerialized]
        internal float[] cloudCoverCurve;

        /// <summary>Secondary / connected cloud cover, this controls how much the weather map cover texture is used.</summary>
        [MinMaxSlider(0.0f, 1.0f, "Secondary / connected cloud cover, this controls how much the weather map cover texture is used.")]
        public RangeOfFloats CloudCoverSecondary = new RangeOfFloats(0.0f, 0.0f);

        /// <summary>Cloud density, controls size clouds, such as anvils.</summary>
        [Header("Clouds - density")]
        [MinMaxSlider(0.0f, 1.0f, "Cloud density, controls size and bulkiness of clouds, such as anvils.")]
        public RangeOfFloats CloudDensity = new RangeOfFloats(1.0f, 1.0f);

        /// <summary>Control cloud density multiplier over height.</summary>
        [Tooltip("Control cloud density multiplier over height.")]
        public AnimationCurve CloudDensityCurve;

        [System.NonSerialized]
        internal float[] cloudDensityCurve;

        /// <summary>Secondary / connected cloud density, this controls how much the weather map density texture is used.</summary>
        [MinMaxSlider(0.0f, 1.0f, "Secondary / connected cloud density, this controls how much the weather map density texture is used.")]
        public RangeOfFloats CloudDensitySecondary = new RangeOfFloats(0.0f, 0.0f);

        /// <summary>Cloud type - 0 is lowest flattest type of cloud, 1 is largest and puffiest (cummulus)</summary>
        [Header("Clouds - type")]
        [MinMaxSlider(0.0f, 1.0f, "Cloud type - 0 is lowest flattest type of cloud, 1 is largest and puffiest (cummulus)")]
        public RangeOfFloats CloudType = new RangeOfFloats(0.4f, 0.6f);

        /// <summary>Control cloud type multiplier over height.</summary>
        [Tooltip("Control cloud type multiplier over height.")]
        public AnimationCurve CloudTypeCurve;

        [System.NonSerialized]
        internal float[] cloudTypeCurve;

        /// <summary>Secondary cloud type, this controls how much the weather map type texture is used.</summary>
        [MinMaxSlider(0.0f, 1.0f, "Secondary cloud type, this controls how much the weather map type texture is used.")]
        public RangeOfFloats CloudTypeSecondary = new RangeOfFloats(0.0f, 0.0f);

        /// <summary>Texture for cloud noise shape (perlin, worley) - RGBA</summary>
        [Header("Clouds - noise textures")]
        [Tooltip("3D Texture for cloud noise shape (perlin, worley) - RGBA")]
        public Texture CloudNoiseShape;

        /// <summary>Texture for cloud noise detail (worley) - A</summary>
        [Tooltip("3D Texture for cloud noise detail (worley) - A")]
        public Texture CloudNoiseDetail;

        /// <summary>Texture for cloud noise curl (turbulence) - RGB (XYZ)</summary>
        [Tooltip("2D Texture for cloud noise curl (turbulence) - RGB (XYZ)")]
        public Texture CloudNoiseCurl;

        /// <summary>Cloud noise shape scale.</summary>
        [Header("Clouds - shape noise")]
        [MinMaxSlider(0.01f, 3.0f, "Cloud noise shape scale.")]
        public RangeOfFloats CloudNoiseShapeScale = new RangeOfFloats(0.65f, 0.85f);

        /// <summary>Cloud noise shape scalar.</summary>
        [MinMaxSlider(0.01f, 10.0f, "Cloud noise shape scalar.")]
        public RangeOfFloats CloudNoiseShapeScalar = new RangeOfFloats(1.5f, 2.5f);

        /// <summary>Cloud weights for rgba channels.</summary>
        [SingleLine("Cloud weights for rgba channels.")]
        public Vector4 CloudNoiseShapeWeights = new Vector4(1.0f, 0.2f, 0.4f, 1.0f);

        /// <summary>Cloud anvil strength. Controls how intense anvils are.</summary>
        [Header("Clouds - anvils")]
        [MinMaxSlider(0.0f, 10.0f, "Cloud anvil strength. Controls how intense anvils are.")]
        public RangeOfFloats CloudAnvilStrength = new RangeOfFloats(0.0f, 0.0f);

        /// <summary>Cloud anvil height start, 0 to 1.</summary>
        [MinMaxSlider(0.0f, 1.0f, "Cloud anvil height start, 0 to 1.")]
        public RangeOfFloats CloudAnvilStart = new RangeOfFloats(0.5f, 0.8f);

        /// <summary>Cloud noise detail scale.</summary>
        [Header("Clouds - detail noise")]
        [MinMaxSlider(0.01f, 10.0f, "Cloud noise detail scale.")]
        public RangeOfFloats CloudNoiseDetailScale = new RangeOfFloats(1.25f, 1.65f);

        /// <summary>Cloud noise detail power, controls how much the detail noise affects the clouds.</summary>
        [MinMaxSlider(0.0f, 1.0f, "Cloud noise detail power, controls how much the detail noise affects the clouds.")]
        public RangeOfFloats CloudNoiseDetailPower = new RangeOfFloats(0.25f, 0.35f);

        /// <summary>Control cloud detail power multiplier over height.</summary>
        [Tooltip("Control cloud detail power multiplier over height.")]
        public AnimationCurve CloudNoiseDetailPowerCurve;

        [System.NonSerialized]
        internal float[] cloudNoiseDetailPowerCurve;

        /// <summary>As height increases, use less whispy detail noise.</summary>
        [MinMaxSlider(0.0f, 32.0f, "As height increases, use less whispy detail noise.")]
        public RangeOfFloats CloudNoiseDetailHeightMultiplier = new RangeOfFloats(15.0f, 20.0f);

        /// <summary>Cloud noise detail curl scale.</summary>
        [MinMaxSlider(0.0f, 1.0f, "Cloud noise detail curl scale.")]
        public RangeOfFloats CloudNoiseDetailCurlScale = new RangeOfFloats(0.05f, 0.1f);

        /// <summary>Cloud noise detail curl intensity.</summary>
        [MinMaxSlider(0.0f, 1.0f, "Cloud noise detail curl intensity.")]
        public RangeOfFloats CloudNoiseDetailCurlIntensity = new RangeOfFloats(0.05f, 0.1f);

        /// <summary>Cloud noise dither multiplier.</summary>
        [Header("Clouds - appearance")]
        [MinMaxSlider(0.0f, 10.0f, "Cloud noise dither multiplier.")]
        public RangeOfFloats CloudDitherMultiplier = new RangeOfFloats(1.0f, 1.0f);

        /// <summary>Max optical depth multiplier, determines lod, horizon fade and other sky blending effects</summary>
        [Tooltip("Max optical depth multiplier, determines lod, horizon fade and other sky blending effects")]
        [Range(1.0f, 100.0f)]
        public float CloudOpticalDistanceMultiplier = 10.0f;

        /// <summary>Fades clouds at horizon/larger optical depths. X = luminance multiplier, Y = optical depth multiplier, Z = power, W = intensity.</summary>
        [SingleLine("Fades clouds at horizon/larger optical depths. X = luminance multiplier, Y = optical depth multiplier, Z = power, W = intensity.")]
        public Vector4 CloudHorizonFade = new Vector4(2.0f, 1.05f, 4.0f, 1.0f);

        /// <summary>Offset the ray y direction from the horizon.</summary>
        [Tooltip("Offset the ray y direction from the horizon.")]
        [Range(-1.0f, 1.0f)]
        public float CloudRayOffset = 0.01f;

        /// <summary>Cloud ray march parameters. X: short step units, Y: long step units, Z: step power (short to long, higher values stay short), W: reserved.</summary>
        [SingleLine("Cloud ray march parameters. X: short step units, Y: long step units, Z: step power (short to long, higher values stay short), W: reserved.")]
        public Vector4 CloudRayMarchParameters = new Vector4(64.0f, 512.0f, 1.0f, 0.0f);

        /// <summary>The threshold to count a cloud coverage pixel (r channel) from the weather map as an adjacent sdf pixel. Set to 0 or 1 to disable sdf entirely.</summary>
        [Tooltip("The threshold to count a cloud coverage pixel (r channel) from the weather map as an adjacent sdf pixel. Set to 0 or 1 to disable sdf entirely.")]
        [Range(0.0f, 1.0f)]
        public float CloudSdfThreshold = 1.0f;

        /// <summary>Minimum coverage value to attempt cloud sampling.</summary>
        [Tooltip("Minimum coverage value to attempt cloud sampling.")]
        [Range(0.01f, 0.5f)]
        public float CloudCoverMinimum = 0.05f;

        /// <summary>Cloud shape animation/turbulence.</summary>
        [Header("Cloud animation/turbulence")]
        [Tooltip("Cloud shape animation/turbulence.")]
        public Vector3 CloudShapeAnimationVelocity = new Vector3(0.0f, -2.0f, 0.0f);

        /// <summary>Cloud detail animation/turbulence.</summary>
        [Tooltip("Cloud detail animation/turbulence.")]
        public Vector3 CloudDetailAnimationVelocity = new Vector3(0.0f, -1.3f, 0.0f);

        /// <summary>Cloud color.</summary>
        [Header("Clouds - colors")]
        [Tooltip("Cloud color.")]
        public Color CloudColor = Color.white;

        /// <summary>Cloud emission color, always emits this color regardless of lighting. Gradient is time of day with sun at horizon at center of gradient.</summary>
        [Tooltip(" Gradient is time of day with sun at horizon at center of gradient.")]
        public Gradient CloudEmissionGradientColor = new Gradient { colorKeys = new GradientColorKey[] { new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.black, 1.0f) } };

        [System.NonSerialized]
        internal Color CloudEmissionGradientColorColor;

        /// <summary>Cloud dir light gradient color, where center of gradient is sun at horizon, right is 'noon'.</summary>
        [Tooltip("Cloud dir light gradient color, where center of gradient is sun at horizon, right is 'noon'.")]
        public Gradient CloudDirLightGradientColor = new Gradient();

        [System.NonSerialized]
        internal Color CloudDirLightGradientColorColor;

        /// <summary>Cloud dir light multiplier</summary>
        [Header("Clouds - lights")]
        [Tooltip("Cloud dir light multiplier")]
        [Range(0.0f, 10.0f)]
        public float CloudDirLightMultiplier = 5.0f;

        /// <summary>Cloud light dither level, helps with night clouds banding</summary>
        [Tooltip("Cloud light dither level, helps with night clouds banding")]
        [Range(0.0f, 1.0f)]
        public float CloudLightDitherLevel = 0.0008f;

        /// <summary>Point/spot light multiplier</summary>
        [Tooltip("Point/spot light multiplier")]
        [Range(0.0f, 10.0f)]
        public float CloudPointSpotLightMultiplier = 1.0f;

        /// <summary>How much clouds absorb light, affects shadows in the clouds</summary>
        [Tooltip("How much clouds absorb light, affects shadows in the clouds")]
        [Range(0.0f, 64.0f)]
        public float CloudLightAbsorption = 4.0f;

        /// <summary>Henyey Greenstein Phase/Silver lining (x = forward, y = back, z = forward multiplier, w = back multiplier).</summary>
        [SingleLine("Henyey Greenstein Phase/Silver lining (x = forward, y = back, z = forward multiplier, w = back multiplier).")]
        public Vector4 CloudHenyeyGreensteinPhase = new Vector4(0.7f, -0.4f, 0.2f, 1.0f);

        /// <summary>Cloud powder multiplier / dark edge multiplier, brightens up bumps/billows in higher clouds</summary>
        [MinMaxSlider(0.0f, 8.0f, "Cloud powder multiplier / dark edge multiplier, brightens up bumps/billows in higher clouds")]
        public RangeOfFloats CloudPowderMultiplier = new RangeOfFloats(0.0f, 0.0f);

        /// <summary>Indirect directional light multiplier (indirect scattering)</summary>
        [Tooltip("Indirect directional light multiplier (indirect scattering)")]
        [Range(0.0f, 10.0f)]
        public float CloudDirLightIndirectMultiplier = 1.0f;

        /// <summary>Ambient ground intensity</summary>
        [Header("Clouds - ambient light")]
        [Tooltip("Ambient ground intensity")]
        [Range(0.0f, 100.0f)]
        public float CloudAmbientGroundIntensity = 5.0f;

        /// <summary>Ambient sky intensity, this is how much the ambient sky color from the day night cycle influences the cloud color</summary>
        [Tooltip("Ambient sky intensity, this is how much the ambient sky color from the day night cycle influences the cloud color")]
        [Range(0.0f, 100.0f)]
        public float CloudAmbientSkyIntensity = 4.0f;

        /// <summary>Increases ambient ground light towards higher cloud heights</summary>
        [Tooltip("Increases ambient ground light towards higher cloud heights")]
        [Range(0.0f, 1.0f)]
        public float CloudAmbientGroundHeightMultiplier = 1.0f;

        /// <summary>Increases ambient sky light towards lower cloud heights</summary>
        [Tooltip("Increases ambient sky light towards lower cloud heights")]
        [Range(0.0f, 1.0f)]
        public float CloudAmbientSkyHeightMultiplier = 1.0f;

        /// <summary>Ambient shadowing.</summary>
        [Tooltip("Ambient shadowing.")]
        [Range(0.0f, 1.0f)]
        public float CloudAmbientShadow = 0.3f;

        /// <summary>Stratus cloud gradient, controls cloud density over height (4 control points)</summary>
        [Header("Clouds - shape")]
        [Tooltip("Stratus cloud gradient, controls cloud density over height (4 control points)")]
        public Gradient CloudGradientStratus;
        internal Vector4 CloudGradientStratusVector;

        /// <summary>Stratocumulus cloud gradient, controls cloud density over height (4 control points)</summary>
        [Tooltip("Stratocumulus cloud gradient, controls cloud density over height (4 control points)")]
        public Gradient CloudGradientStratoCumulus;
        internal Vector4 CloudGradientStratoCumulusVector;

        /// <summary>Cumulus cloud gradient, controls cloud density over height (4 control points)</summary>
        [Tooltip("Cumulus cloud gradient, controls cloud density over height (4 control points)")]
        public Gradient CloudGradientCumulus;
        internal Vector4 CloudGradientCumulusVector;

        /// <summary>Allowed flat layers</summary>
        [Header("Clouds - flat layer allowance")]
        [WeatherMaker.EnumFlag("Allowed flat layers")]
        public WeatherMakerVolumetricCloudsFlatLayerMask FlatLayerMask = WeatherMakerVolumetricCloudsFlatLayerMask.Four;

        /// <summary>Dir light ray spread (0 - 1).</summary>
        [Header("Clouds - dir light rays")]
        [Tooltip("Dir light ray spread (0 - 1).")]
        [Range(0.0f, 1.0f)]
        public float CloudDirLightRaySpread = 0.65f;

        /// <summary>Increases the dir light ray brightness</summary>
        [Tooltip("Increases the dir light ray brightness")]
        [Range(0.0f, 10.0f)]
        public float CloudDirLightRayBrightness = 0.075f;

        /// <summary>Combined with each dir light ray march, this determines how much light is accumulated each step.</summary>
        [Tooltip("Combined with each dir light ray march, this determines how much light is accumulated each step.")]
        [Range(0.0f, 1000.0f)]
        public float CloudDirLightRayStepMultiplier = 21.0f;

        /// <summary>Determines light fall-off from start of dir light ray. Set to 1 for no fall-off.</summary>
        [Tooltip("Determines light fall-off from start of dir light ray. Set to 1 for no fall-off.")]
        [Range(0.5f, 1.0f)]
        public float CloudDirLightRayDecay = 0.97f;

        /// <summary>Dir light ray tint color. Alpha value determines tint intensity.</summary>
        [Tooltip("Dir light ray tint color. Alpha value determines tint intensity.")]
        public Color CloudDirLightRayTintColor = Color.white;

        /// <summary>
        /// Magic dither values for cloud rays
        /// </summary>
        public static readonly Vector4 CloudDirLightRayDitherMagic = new Vector4(2.34325f, 5.235345f, 1024.0f, 1024.0f);

        /// <summary>
        /// Progress for all internal lerp variables
        /// </summary>
        internal float lerpProgress;

        /// <summary>
        /// Create curves if needed
        /// </summary>
        internal void CreateCurves()
        {
            const float step = 1.0f / (float)CurveLength;
            if (cloudCoverCurve == null || cloudCoverCurve.Length < 2)
            {
                cloudCoverCurve = new float[CurveLength];
                for (int i = 0; i < CurveLength; i++)
                {
                    cloudCoverCurve[i] = (CloudCoverCurve.length < 2 ? 1.0f : CloudCoverCurve.Evaluate((float)i * step));
                }
            }
            if (cloudDensityCurve == null || cloudDensityCurve.Length < 2)
            {
                cloudDensityCurve = new float[CurveLength];
                for (int i = 0; i < CurveLength; i++)
                {
                    cloudDensityCurve[i] = (CloudDensityCurve.length < 2 ? 1.0f : CloudDensityCurve.Evaluate((float)i * step));
                }
            }
            if (cloudTypeCurve == null || cloudTypeCurve.Length < 2)
            {
                cloudTypeCurve = new float[CurveLength];
                for (int i = 0; i < CurveLength; i++)
                {
                    cloudTypeCurve[i] = (CloudTypeCurve.length < 2 ? 1.0f : CloudTypeCurve.Evaluate((float)i * step));
                }
            }
            if (cloudNoiseDetailPowerCurve == null || cloudNoiseDetailPowerCurve.Length < 2)
            {
                cloudNoiseDetailPowerCurve = new float[CurveLength];
                for (int i = 0; i < CurveLength; i++)
                {
                    cloudNoiseDetailPowerCurve[i] = (CloudNoiseDetailPowerCurve.length < 2 ? 1.0f : CloudNoiseDetailPowerCurve.Evaluate((float)i * step));
                }
            }
        }

        internal void UpdateCurves()
        {
            CreateCurves();

#if UNITY_EDITOR

            const float step = 1.0f / (float)CurveLength;
            for (int i = 0; i < CurveLength; i++)
            {
                cloudCoverCurve[i] = (CloudCoverCurve == null || CloudCoverCurve.length < 2 ? 1.0f : CloudCoverCurve.Evaluate((float)i * step));
                cloudDensityCurve[i] = (CloudDensityCurve == null || CloudDensityCurve.length < 2 ? 1.0f : CloudDensityCurve.Evaluate((float)i * step));
                cloudTypeCurve[i] = (CloudTypeCurve == null || CloudTypeCurve.length < 2 ? 1.0f : CloudTypeCurve.Evaluate((float)i * step));
                cloudNoiseDetailPowerCurve[i] = (CloudNoiseDetailPowerCurve == null || CloudNoiseDetailPowerCurve.length < 2 ? 1.0f : CloudNoiseDetailPowerCurve.Evaluate((float)i * step));
            }

#endif

        }

        /// <summary>
        /// Convert gradient times to Vector4
        /// </summary>
        /// <param name="gradient">Gradient</param>
        /// <returns>Vector4</returns>
        public static Vector4 CloudHeightGradientToVector4(Gradient gradient)
        {
            GradientColorKey[] colorKeys = gradient.colorKeys;
            int keyCount = colorKeys.Length;
            Vector4 vec;
            if (keyCount > 0)
            {
                vec.x = colorKeys[0].time;
                if (keyCount > 1)
                {
                    vec.y = colorKeys[1].time;
                    if (keyCount > 2)
                    {
                        vec.z = colorKeys[2].time;
                        if (keyCount > 3)
                        {
                            vec.w = colorKeys[3].time;
                        }
                        else
                        {
                            vec.w = vec.z;
                        }
                    }
                    else
                    {
                        vec.z = vec.w = vec.y;
                    }
                }
                else
                {
                    vec.y = vec.z = vec.w = vec.x;
                }
            }
            else
            {
                vec.x = vec.y = vec.z = vec.w = 0.0f;
            }
            return vec;
        }
    }

    /// <summary>
    /// Flay layer mask that are allowed with the volumetric clouds
    /// </summary>
    [System.Flags]
    public enum WeatherMakerVolumetricCloudsFlatLayerMask
    {
        /// <summary>
        /// Layer one allowed
        /// </summary>
        One = 1,

        /// <summary>
        /// Layer two allowed
        /// </summary>
        Two = 2,

        /// <summary>
        /// Layer three allowed
        /// </summary>
        Three = 4,

        /// <summary>
        /// Layer four allowed
        /// </summary>
        Four = 8
    }
}
