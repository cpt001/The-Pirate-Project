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
    /// Performance profile, contains all properties that change Weather Maker quality
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherMakerPerformanceProfile", menuName = "WeatherMaker/Performance Profile", order = 999)]
    public class WeatherMakerPerformanceProfileScript : ScriptableObject
    {
        /// <summary>Whether to allow volumetric clouds, if false the volumetric cloud member of any cloud profile will be nulled before being set.</summary>
        [Header("Volumetric Clouds - Setup")]
        [Tooltip("Whether to allow volumetric clouds, if false the volumetric cloud member of any cloud profile will be nulled before being set.")]
        public bool EnableVolumetricClouds = true;

        /// <summary>Whether to allow flat cloud layers with volumetric clouds. Default is all flat layers.</summary>
        [Tooltip("Whether to allow flat cloud layers with volumetric clouds. Default is all flat layers.")]
        [WeatherMaker.EnumFlag("Allowed flat layers")]
        public WeatherMakerVolumetricCloudsFlatLayerMask VolumetricCloudFlatLayerMask = WeatherMakerVolumetricCloudsFlatLayerMask.One | WeatherMakerVolumetricCloudsFlatLayerMask.Two | WeatherMakerVolumetricCloudsFlatLayerMask.Three | WeatherMakerVolumetricCloudsFlatLayerMask.Four;

        /// <summary>Level of detail for flat cloud layers.</summary>
        [Tooltip("Level of detail for flat cloud layers.")]
        [Range(0.0f, 4.0f)]
        public float VolumetricCloudFlatLayerLod = 0.0f;

        /// <summary>Downsample scale for clouds</summary>
        [Tooltip("Downsample scale for clouds")]
        public WeatherMakerDownsampleScale VolumetricCloudDownsampleScale = WeatherMakerDownsampleScale.HalfResolution;

        /// <summary>Temporal reprojection size for clouds</summary>
        [Tooltip("Temporal reprojection size for clouds")]
        public WeatherMakerTemporalReprojectionSize VolumetricCloudTemporalReprojectionSize = WeatherMakerTemporalReprojectionSize.TwoByTwo;

        /// <summary>Downsample scale for cloud post process (dir light rays, etc.)</summary>
        [Tooltip("Downsample scale for cloud post process (dir light rays, etc.)")]
        public WeatherMakerDownsampleScale VolumetricCloudDownsampleScalePostProcess = WeatherMakerDownsampleScale.QuarterResolution;

        /// <summary>Volumetric cloud weather map size. Higher weather maps allow smoother clouds and movement at the cost of a larger weather map texture. This should be a power of 2.</summary>
        [Tooltip("Volumetric cloud weather map size. Higher weather maps allow smoother clouds and movement at the cost of a larger weather map texture. This should be a power of 2.")]
        [Range(128, 4096)]
        public int VolumetricCloudWeatherMapSize = 1024;

        /// <summary>Volumetric cloud shadow texture size. Higher values provide smoother shadows at the cost of a larger lookup texture. This should be a power of 2.</summary>
        [Tooltip("Volumetric cloud shadow texture size. Higher values provide smoother shadows at the cost of a larger lookup texture. This should be a power of 2.")]
        [Range(128, 4096)]
        public int VolumetricCloudShadowTextureSize = 1024;

        /// <summary>Number of samples for volumetric cloud shadows.</summary>
        [Tooltip("Number of samples for volumetric cloud shadows.")]
        [Range(0, 16)]
        public int VolumetricCloudShadowSampleCount = 5;

        /// <summary>LOD for texture sampling for volumetric cloud shadows.</summary>
        [Tooltip("LOD for texture sampling for volumetric cloud shadows.")]
        [Range(0, 4)]
        public int VolumetricCloudShadowLod = 1;

        /// <summary>Volumetric cloud sample count range</summary>
        [Header("Volumetric Clouds - Raymarch")]
        [MinMaxSlider(16, 256, "Volumetric cloud sample count range")]
        public RangeOfIntegers VolumetricCloudSampleCount = new RangeOfIntegers { Minimum = 64, Maximum = 256 };

        /// <summary>Ray march multiplier. Greater than 1.0 improves performance but can reduce quality.</summary>
        [Tooltip("Ray march multiplier. Greater than 1.0 improves performance but can reduce quality.")]
        [Range(1.0f, 10.0f)]
        public float VolumetricCloudRaymarchMultiplier = 2.0f;

        /// <summary>Min/max dither cloud ray direction to try and avoid banding, 0 for none.</summary>
        [MinMaxSlider(0.0f, 1.0f, "Min/max dither cloud ray direction to try and avoid banding, 0 for none.")]
        public RangeOfFloats VolumetricCloudRayDither = new RangeOfFloats(0.033f, 0.1f);

        /// <summary>Reduce cloud raymarch by this amount while possibly in a cloud. Reduce for better cloud details but watch out for artifacts. Ignored if skip threshold is 0.</summary>
        [Tooltip("Reduce cloud raymarch by this amount while possibly in a cloud. Reduce for better cloud details but watch out for artifacts. Ignored if skip threshold is 0.")]
        [Range(0.01f, 1.0f)]
        public float VolumetricCloudRaymarchMaybeInCloudStepMultiplier = 1.0f;

        /// <summary>Reduce cloud raymarch by this amount while in a cloud. Reduce for better cloud details but watch out for artifacts. Ignored if skip threshold is 0.</summary>
        [Tooltip("Reduce cloud raymarch by this amount while in a cloud. Reduce for better cloud details but watch out for artifacts. Ignored if skip threshold is 0.")]
        [Range(0.01f, 1.0f)]
        public float VolumetricCloudRaymarchInCloudStepMultiplier = 1.0f;

        /// <summary>Allows increasing noise values taken with volumetric clouds to get out of raymarching faster. Makes thicker, heavier, less whispy clouds.</summary>
        [Tooltip("Allows increasing noise values taken with volumetric clouds to get out of raymarching faster. Makes thicker, heavier, less whispy clouds.")]
        [Range(1.0f, 4.0f)]
        public float VolumetricCloudNoiseMultiplier = 1.0f;

        /// <summary>Whether to allow anvils for volumetric clouds. This can impact performance.</summary>
        [Tooltip("Whether to allow anvils for volumetric clouds. This can impact performance.")]
        public bool VolumetricCloudAllowAnvils;

        /// <summary>Volumetric cloud noise lod, used to increase mip level during noise sampling.</summary>
        [MinMaxSlider(-10.0f, 10.0f, "Volumetric cloud noise lod, used to increase mip level during noise sampling.")]
        public RangeOfFloats VolumetricCloudLod = new RangeOfFloats { Minimum = 0.0f, Maximum = 1.0f };

        /// <summary>Volumetric cloud dir light sample count for cloud self shadowing.</summary>
        [Tooltip("Volumetric cloud dir light sample count for cloud self shadowing.")]
        [Range(0, 16)]
        [Header("Volumetric Clouds - Dir Light")]
        public int VolumetricCloudDirLightSampleCount = 4;

        /// <summary>Number of sub-samples to take for directional light march. Can impact performance but increases lighting realism.</summary>
        [Tooltip("Number of sub-samples to take for directional light march. Can impact performance but increases lighting realism.")]
        [Range(1, 4)]
        public int VolumetricCloudDirLightSubSampleCount = 1;

        /// <summary>Volumetric cloud dir light sample count multiplier as light approaches horizon.</summary>
        [Tooltip("Volumetric cloud dir light sample count multiplier as light approaches horizon.")]
        [Range(1.0f, 5.0f)]
        public float VolumetricCloudDirLightHorizonMultiplier = 1.0f;

        /// <summary>Multiplier for directional light ray march distance for cloud self shadowing.</summary>
        [Tooltip("Multiplier for directional light ray march distance for cloud self shadowing.")]
        [Range(0.001f, 1.0f)]
        public float VolumetricCloudDirLightStepMultiplier = 0.4f;

        /// <summary>Multiplier for directional light ray march cone radius for cloud self shadowing.</summary>
        [Tooltip("Multiplier for directional light ray march cone radius for cloud self shadowing.")]
        [Range(0.0f, 10.0f)]
        public float VolumetricCloudDirLightRadiusMultiplier = 1.0f;

        /// <summary>Multiplier for one distant directional light step for volumetric clouds lighting.</summary>
        [Tooltip("Multiplier for one distant directional light step for volumetric clouds lighting.")]
        [Range(0.0f, 16.0f)]
        public float VolumetricCloudLightDistantMultiplier = 6.0f;

        /// <summary>Multiplier for volumetric cloud self shadowing.</summary>
        [Tooltip("Multiplier for volumetric cloud self shadowing.")]
        [Range(1.0f, 4.0f)]
        public float VolumetricCloudAbsorptionMultiplier = 1.0f;

        /// <summary>LOD to add to dir light samples.</summary>
        [Tooltip("LOD to add to dir light samples.")]
        [Range(0.0f, 4.0f)]
        public float VolumetricCloudDirLightLod = 1.0f;

        /// <summary>Whether to sample volumetric cloud details for dir light self shadowing</summary>
        [Tooltip("Whether to sample volumetric cloud details for dir light self shadowing")]
        public bool VolumetricCloudDirLightSampleDetails = true;

        /// <summary>Sample count for volumetric cloud dir light rays. Set to 0 to disable.</summary>
        [Header("Volumetric Clouds - Dir Light God Rays")]
        [Tooltip("Sample count for volumetric cloud dir light rays. Set to 0 to disable.")]
        [Range(0, 64)]
        public int VolumetricCloudDirLightRaySampleCount = 16;

        /// <summary>Dithering intensity for dir light rays.</summary>
        [Tooltip("Dithering intensity for dir light rays.")]
        [Range(-1.0f, 1.0f)]
        public float VolumetricCloudDirLightRayDither = 0.1f;

        /// <summary>Falloff multiplier for dir light rays.</summary>
        [Tooltip("Falloff multiplier for dir light rays.")]
        [Range(0.0f, 1.0f)]
        public float VolumetricCloudDirLightRayFalloffMultiplier = 1.0f;

        /// <summary>Volumetric cloud shadow downsample scale</summary>
        [Header("Volumetric Clouds - Shadows")]
        [Tooltip("Volumetric cloud shadow downsample scale")]
        public WeatherMakerDownsampleScale VolumetricCloudShadowDownsampleScale = WeatherMakerDownsampleScale.FullResolution;

        /// <summary>Whether to allow volumetric cloud reflections.</summary>
        [Tooltip("Whether to allow volumetric cloud reflections.")]
        public bool VolumetricCloudAllowReflections = true;

        /// <summary>Whether to allow volumetric cloud shadows in reflections.</summary>
        [Tooltip("Whether to allow volumetric cloud shadows in reflections.")]
        public bool VolumetricCloudReflectionShadows = true;

        /// <summary>The minimum power to use for the cascade shadow map on the clouds. Set to 1 to not use the cascade shadow map.</summary>
        [Tooltip("The minimum power to use for the cascade shadow map on the clouds. Set to 1 to not use the cascade shadow map.")]
        [Range(0.0f, 1.0f)]
        public float VolumetricCloudShadowMapMinPower = 1.0f;

        /// <summary>Enable atmospheric scattering</summary>
        [Header("Atmospheric Scattering")]
        [Tooltip("Enable atmospheric scattering")]
        public WeatherMakerAtmosphereQuality AtmosphereQuality = WeatherMakerAtmosphereQuality.Medium;

        /// <summary>Atmospheric scattering light shaft sample count, 0 to disable</summary>
        [Tooltip("Atmospheric scattering light shaft sample count, 0 to disable")]
        [Range(0, 64)]
        public int AtmosphericLightShaftSampleCount = 8;

        /// <summary>Atmospheric scattering light shaft max ray length</summary>
        [Tooltip("Atmospheric scattering light shaft max ray length")]
        [Range(100, 1000000)]
        public float AtmosphericLightShaftMaxRayLength = 5000;

        /// <summary>Aurora sample count max</summary>
        [Header("Aurora Borealis / Northern Lights")]
        [Tooltip("Aurora sample count max")]
        [Range(0, 64)]
        public int AuroraSampleCount = 42;

        /// <summary>Aurora sub-sample count max</summary>
        [Tooltip("Aurora sub-sample count max")]
        [Range(1, 4)]
        public int AuroraSubSampleCount = 4;

        /// <summary>Whether to enable precipitation collision. This can impact performance, so turn off if this is a problem.</summary>
        [Header("Precipitation")]
        [Tooltip("Whether to enable precipitation collision. This can impact performance, so turn off if this is a problem.")]
        public bool EnablePrecipitationCollision;

        /// <summary>Whether per pixel lighting is enabled - currently precipitation particles are the only materials that support this. Turn off if you see a performance impact.</summary>
        [Tooltip("Whether per pixel lighting is enabled - currently precipitation particles are the only materials that support this. " +
            "Turn off if you see a performance impact.")]
        public bool EnablePerPixelLighting = true;

        /// <summary>Downsample scale for fog</summary>
        [Header("Fog")]
        [Tooltip("Downsample scale for fog")]
        public WeatherMakerDownsampleScale FogDownsampleScale = WeatherMakerDownsampleScale.HalfResolution;

        /// <summary>Downsample scale for fog full screen shafts</summary>
        [Tooltip("Downsample scale for fog full screen shafts")]
        public WeatherMakerDownsampleScale FogDownsampleScaleFullScreenShafts = WeatherMakerDownsampleScale.HalfResolution;

        /// <summary>Temporal reprojection size for fog</summary>
        [Tooltip("Temporal reprojection size for fog")]
        public WeatherMakerTemporalReprojectionSize FogTemporalReprojectionSize = WeatherMakerTemporalReprojectionSize.TwoByTwo;

        /// <summary>Whether to enable volumetric fog point/spot lights. Fog always uses directional lights. Disable to improve performance.</summary>
        [Tooltip("Whether to enable volumetric fog point/spot lights. Fog always uses directional lights. Disable to improve performance.")]
        public bool EnableFogLights = true;

        /// <summary>Sample count for full screen fog sun shafts. Set to 0 to disable.</summary>
        [Tooltip("Sample count for full screen fog sun shafts. Set to 0 to disable.")]
        [Range(0, 64)]
        public int FogFullScreenSunShaftSampleCount = 16;

        /// <summary>Fog noise sample count, 0 to disable fog noise.</summary>
        [Tooltip("Fog noise sample count, 0 to disable fog noise.")]
        [Range(0, 128)]
        public int FogNoiseSampleCount = 40;

        /// <summary>Fog dir light shadow sample count, 0 to disable fog shadows.</summary>
        [Tooltip("Fog dir light shadow sample count, 0 to disable fog shadows.")]
        [Range(0, 128)]
        public int FogShadowSampleCount = 0;

        /// <summary>Whether to enable water caustics</summary>
        [Header("Water")]
        [Tooltip("Whether to enable water caustics")]
        public bool WaterEnableCaustics = true;

        /// <summary>Whether to enable water refraction</summary>
        [Tooltip("Whether to enable water refraction")]
        public bool WaterEnableRefraction = true;

        /// <summary>Whether to enable water foam</summary>
        [Tooltip("Whether to enable water foam")]
        public bool WaterEnableFoam = true;

        /// <summary>Whether to ignore reflection probe cameras. For an indoor scene or if you don't care to reflect clouds, fog, etc. set this to true.</summary>
        [Header("Reflections")]
        [Tooltip("Whether to ignore reflection probe cameras. For an indoor scene or if you don't care to reflect clouds, fog, etc. set this to true.")]
        public bool IgnoreReflectionProbes;

        /// <summary>Reflection texture size, bigger textures look better but at a cost of additional rendering time. Set to 127 to disable reflections.</summary>
        [Tooltip("Reflection texture size, bigger textures look better but at a cost of additional rendering time. Set to 127 to disable reflections.")]
        [Range(127, 4096)]
        public int ReflectionTextureSize = 1024;

        /// <summary>Reflection shadow mode.</summary>
        [Tooltip("Reflection shadow mode.")]
        public ShadowQuality ReflectionShadows = ShadowQuality.HardOnly;

        /// <summary>Whether to enable tonemapping for effects that support it. If you are using post process color grading, consider setting this to false.</summary>
        [Header("Post Processing")]
        [Tooltip("Whether to enable tonemapping for effects that support it. If you are using post process color grading, consider setting this to false.")]
        public bool EnableTonemap = true;
    }
}
