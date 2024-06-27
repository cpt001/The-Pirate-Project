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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{

#pragma warning disable 1591

    public static class WMS
    {
        #region Buffers

        private static readonly Color[] tmpColorArray = new Color[4];
        private static readonly float[] tmpFloatArray = new float[4];
        private static readonly float[] tmpFloatArray2 = new float[8];
        private static readonly Vector4[] tmpVectorArray = new Vector4[4];

        #endregion Buffers

        #region Ids

        private static int ID(string s) { return Shader.PropertyToID(s); }

        public static readonly int _AlphaMultiplierAnimation = ID("_AlphaMultiplierAnimation");
        public static readonly int _AlphaMultiplierAnimation2 = ID("_AlphaMultiplierAnimation2");
        public static readonly int _BlendOp = ID("_BlendOp");
        public static readonly int _Blur7 = ID("_Blur7");
        public static readonly int _BlurSize = ID("_BlurSize");
        public static readonly int _BlurNeedsSourceDepth = ID("_BlurNeedsSourceDepth");
        public static readonly int _CameraColorAttachmentA = ID("_CameraColorAttachmentA");
        public static readonly int _CameraDepthTexture = ID("_CameraDepthTexture");
        public static readonly int _CameraDepthTextureEighth = ID("_CameraDepthTextureEighth");
        public static readonly int _CameraDepthTextureHalf = ID("_CameraDepthTextureHalf");
        public static readonly int _CameraDepthTextureOne = ID("_CameraDepthTextureOne");
        public static readonly int _CameraDepthTextureQuarter = ID("_CameraDepthTextureQuarter");
        public static readonly int _CameraOpaqueTexture = ID("_CameraOpaqueTexture");
        public static readonly int _CloudAmbientGroundIntensityVolumetric = ID("_CloudAmbientGroundIntensityVolumetric");
        public static readonly int _CloudAmbientGroundHeightMultiplierVolumetric = ID("_CloudAmbientGroundHeightMultiplierVolumetric");
        public static readonly int _CloudAmbientGroundMultiplier = ID("_CloudAmbientGroundMultiplier");
        public static readonly int _CloudAmbientSkyMultiplier = ID("_CloudAmbientSkyMultiplier");
        public static readonly int _CloudAmbientSkyHeightMultiplierVolumetric = ID("_CloudAmbientSkyHeightMultiplierVolumetric");
        public static readonly int _CloudAmbientSkyIntensityVolumetric = ID("_CloudAmbientSkyIntensityVolumetric");
        public static readonly int _CloudAmbientShadowVolumetric = ID("_CloudAmbientShadowVolumetric");
        public static readonly int _CloudAnvilStrength = ID("_CloudAnvilStrength");
        public static readonly int _CloudAnvilStart = ID("_CloudAnvilStart");
        public static readonly int _CloudColor = ID("_CloudColor");
        public static readonly int _CloudColorVolumetric = ID("_CloudColorVolumetric");

        public static readonly int _CloudCover = ID("_CloudCover");
        public static readonly int _CloudCoverageAdder = ID("_CloudCoverageAdder");
        public static readonly int _CloudCoverageCurve = ID("_CloudCoverageCurve");
        public static readonly int _CloudCoverageFrequency = ID("_CloudCoverageFrequency");
        public static readonly int _CloudCoverageNoiseType = ID("_CloudCoverageNoiseType");
        public static readonly int _CloudCoverageNoiseTypeInv = ID("_CloudCoverageNoiseTypeInv");
        public static readonly int _CloudCoverageOffset = ID("_CloudCoverageOffset");
        public static readonly int _CloudCoveragePower = ID("_CloudCoveragePower");
        public static readonly int _CloudCoverageRotation = ID("_CloudCoverageRotation");
        public static readonly int _CloudCoverageVelocity = ID("_CloudCoverageVelocity");
        public static readonly int _CloudCoverageWarpScale = ID("_CloudCoverageWarpScale");
        public static readonly int _CloudCoverageNegationAdder = ID("_CloudCoverageNegationAdder");
        public static readonly int _CloudCoverageNegationFrequency = ID("_CloudCoverageNegationFrequency");
        public static readonly int _CloudCoverageNegationOffset = ID("_CloudCoverageNegationOffset");
        public static readonly int _CloudCoverageNegationPower = ID("_CloudCoverageNegationPower");
        public static readonly int _CloudCoverageNegationRotation = ID("_CloudCoverageNegationRotation");
        public static readonly int _CloudCoverageNegationVelocity = ID("_CloudCoverageNegationVelocity");
        public static readonly int _CloudCoverageNegationWarpScale = ID("_CloudCoverageNegationWarpScale");
        public static readonly int _CloudCoverVolumetric = ID("_CloudCoverVolumetric");
        public static readonly int _CloudCoverVolumetricMinimumForCloud = ID("_CloudCoverVolumetricMinimumForCloud");
        public static readonly int _CloudCoverSecondaryVolumetric = ID("_CloudCoverSecondaryVolumetric");

        public static readonly int _CloudDensity = ID("_CloudDensity");
        public static readonly int _CloudDensityAdder = ID("_CloudDensityAdder");
        public static readonly int _CloudDensityCurve = ID("_CloudDensityCurve");
        public static readonly int _CloudDensityFrequency = ID("_CloudDensityFrequency");
        public static readonly int _CloudDensityNoiseType = ID("_CloudDensityNoiseType");
        public static readonly int _CloudDensityNoiseTypeInv = ID("_CloudDensityNoiseTypeInv");
        public static readonly int _CloudDensityOffset = ID("_CloudDensityOffset");
        public static readonly int _CloudDensityPower = ID("_CloudDensityPower");
        public static readonly int _CloudDensityRotation = ID("_CloudDensityRotation");
        public static readonly int _CloudDensityVelocity = ID("_CloudDensityVelocity");
        public static readonly int _CloudDensityWarpScale = ID("_CloudDensityWarpScale");
        public static readonly int _CloudDensityNegationAdder = ID("_CloudDensityNegationAdder");
        public static readonly int _CloudDensityNegationFrequency = ID("_CloudDensityNegationFrequency");
        public static readonly int _CloudDensityNegationOffset = ID("_CloudDensityNegationOffset");
        public static readonly int _CloudDensityNegationPower = ID("_CloudDensityNegationPower");
        public static readonly int _CloudDensityNegationRotation = ID("_CloudDensityNegationRotation");
        public static readonly int _CloudDensityNegationVelocity = ID("_CloudDensityNegationVelocity");
        public static readonly int _CloudDensityNegationWarpScale = ID("_CloudDensityNegationWarpScale");
        public static readonly int _CloudDensityVolumetric = ID("_CloudDensityVolumetric");
        public static readonly int _CloudDensitySecondaryVolumetric = ID("_CloudDensitySecondaryVolumetric");
        public static readonly int _CloudDetailAnimationVelocity = ID("_CloudDetailAnimationVelocity");
        public static readonly int _CloudNoiseDetailHeightMultiplier = ID("_CloudNoiseDetailHeightMultiplier");
        public static readonly int _CloudNoiseDetailPowerVolumetric = ID("_CloudNoiseDetailPowerVolumetric");
        public static readonly int _CloudNoiseDetailPowerCurveVolumetric = ID("_CloudNoiseDetailPowerCurveVolumetric");
        public static readonly int _CloudDirColorVolumetric = ID("_CloudDirColorVolumetric");
        public static readonly int _CloudDirLightLod = ID("_CloudDirLightLod");
        public static readonly int _CloudDirLightIndirectMultiplierVolumetric = ID("_CloudDirLightIndirectMultiplierVolumetric");
        public static readonly int _CloudDirLightMultiplierVolumetric = ID("_CloudDirLightMultiplierVolumetric");
        public static readonly int _CloudDirLightRaySampleCount = ID("_CloudDirLightRaySampleCount");
        public static readonly int _CloudDirLightSampleCount = ID("_CloudDirLightSampleCount");
        public static readonly int _CloudDirLightSubSampleCount = ID("_CloudDirLightSubSampleCount");
        public static readonly int _CloudEmissionColor = ID("_CloudEmissionColor");
        public static readonly int _CloudEmissionColorVolumetric = ID("_CloudEmissionColorVolumetric");
        public static readonly int _CloudEndSquaredInverseVolumetric = ID("_CloudEndSquaredInverseVolumetric");
        public static readonly int _CloudEndSquaredVolumetric = ID("_CloudEndSquaredVolumetric");
        public static readonly int _CloudEndVolumetric = ID("_CloudEndVolumetric");
        public static readonly int _CloudEndInverseVolumetric = ID("_CloudEndInverseVolumetric");
        public static readonly int _CloudInvFade = ID("_CloudInvFade");
        public static readonly int _CloudNoiseFrame = ID("_CloudNoiseFrame");
        public static readonly int _CloudGradientCumulus = ID("_CloudGradientCumulus");
        public static readonly int _CloudGradientStratoCumulus = ID("_CloudGradientStratoCumulus");
        public static readonly int _CloudGradientStratus = ID("_CloudGradientStratus");
        public static readonly int _CloudHeight = ID("_CloudHeight");
        public static readonly int _CloudHeightInverseVolumetric = ID("_CloudHeightInverseVolumetric");
        public static readonly int _CloudHeightSquaredInverseVolumetric = ID("_CloudHeightSquaredInverseVolumetric");
        public static readonly int _CloudHeightSquaredVolumetric = ID("_CloudHeightSquaredVolumetric");
        public static readonly int _CloudHeightVolumetric = ID("_CloudHeightVolumetric");
        public static readonly int _CloudHenyeyGreensteinPhaseVolumetric = ID("_CloudHenyeyGreensteinPhaseVolumetric");
        public static readonly int _CloudHorizonFadeVolumetric = ID("_CloudHorizonFadeVolumetric");
        public static readonly int _CloudLightAbsorption = ID("_CloudLightAbsorption");
        public static readonly int _CloudLightAbsorptionVolumetric = ID("_CloudLightAbsorptionVolumetric");
        public static readonly int _CloudLightDistantMultiplierVolumetric = ID("_CloudLightDistantMultiplierVolumetric");
        public static readonly int _CloudLightDitherLevel = ID("_CloudLightDitherLevel");
        public static readonly int _CloudLightStepMultiplierVolumetric = ID("_CloudLightStepMultiplierVolumetric");
        public static readonly int _CloudLightRadiusMultiplierVolumetric = ID("_CloudLightRadiusMultiplierVolumetric");
        public static readonly int _CloudNoise1 = ID("_CloudNoise1");
        public static readonly int _CloudNoise2 = ID("_CloudNoise2");
        public static readonly int _CloudNoise3 = ID("_CloudNoise3");
        public static readonly int _CloudNoise4 = ID("_CloudNoise4");
        public static readonly int _CloudNoiseAdder = ID("_CloudNoiseAdder");
        public static readonly int _CloudNoiseCurlVolumetric = ID("_CloudNoiseCurlVolumetric");
        public static readonly int _CloudNoiseDetailCurlVolumetric = ID("_CloudNoiseDetailCurlVolumetric");
        public static readonly int _CloudNoiseDetailVolumetric = ID("_CloudNoiseDetailVolumetric");
        public static readonly int _CloudNoiseDither = ID("_CloudNoiseDither");
        public static readonly int _CloudNoiseLod = ID("_CloudNoiseLod");
        public static readonly int _CloudNoiseLodVolumetric = ID("_CloudNoiseLodVolumetric");
        public static readonly int _CloudNoiseMask1 = ID("_CloudNoiseMask1");
        public static readonly int _CloudNoiseMask2 = ID("_CloudNoiseMask2");
        public static readonly int _CloudNoiseMask3 = ID("_CloudNoiseMask3");
        public static readonly int _CloudNoiseMask4 = ID("_CloudNoiseMask4");
        public static readonly int _CloudNoiseMultiplier = ID("_CloudNoiseMultiplier");
        public static readonly int _CloudNoiseRotation = ID("_CloudNoiseRotation");
        public static readonly int _CloudNoisePerlinParams1 = ID("_CloudNoisePerlinParams1");
        public static readonly int _CloudNoisePerlinParams2 = ID("_CloudNoisePerlinParams2");
        public static readonly int _CloudNoiseSampleCountVolumetric = ID("_CloudNoiseSampleCountVolumetric");
        public static readonly int _CloudNoiseScalarVolumetric = ID("_CloudNoiseScalarVolumetric");
        public static readonly int _CloudNoiseScaleVolumetric = ID("_CloudNoiseScaleVolumetric");
        public static readonly int _CloudNoiseShapeVolumetric = ID("_CloudNoiseShapeVolumetric");
        public static readonly int _CloudNoiseShapeCurlMultiplier = ID("_CloudNoiseShapeCurlMultiplier");
        public static readonly int _CloudNoiseScale = ID("_CloudNoiseScale");
        public static readonly int _CloudNoiseType = ID("_CloudNoiseType");
        public static readonly int _CloudNoiseVelocity = ID("_CloudNoiseVelocity");
        public static readonly int _CloudNoiseWorleyParams1 = ID("_CloudNoiseWorleyParams1");
        public static readonly int _CloudNoiseWorleyParams2 = ID("_CloudNoiseWorleyParams2");
        public static readonly int _CloudOpticalDistanceMultiplierVolumetric = ID("_CloudOpticalDistanceMultiplierVolumetric");
        public static readonly int _CloudPlanetEndSquaredVolumetric = ID("_CloudPlanetEndSquaredVolumetric");
        public static readonly int _CloudPlanetEndVolumetric = ID("_CloudPlanetEndVolumetric");
        public static readonly int _CloudPlanetRadiusNegativeVolumetric = ID("_CloudPlanetRadiusNegativeVolumetric");
        public static readonly int _CloudPlanetRadiusSquaredVolumetric = ID("_CloudPlanetRadiusSquaredVolumetric");
        public static readonly int _CloudPlanetRadiusVolumetric = ID("_CloudPlanetRadiusVolumetric");
        public static readonly int _CloudPlanetStartSquaredVolumetric = ID("_CloudPlanetStartSquaredVolumetric");
        public static readonly int _CloudPlanetStartVolumetric = ID("_CloudPlanetStartVolumetric");
        public static readonly int _CloudPointSpotLightMultiplierVolumetric = ID("_CloudPointSpotLightMultiplierVolumetric");
        public static readonly int _CloudPowderMultiplierVolumetric = ID("_CloudPowderMultiplierVolumetric");
        public static readonly int _CloudRayDitherVolumetric = ID("_CloudRayDitherVolumetric");
        public static readonly int _CloudRaymarchMultiplierVolumetric = ID("_CloudRaymarchMultiplierVolumetric");
        public static readonly int _CloudRaymarchMaybeInCloudStepMultiplier = ID("_CloudRaymarchMaybeInCloudStepMultiplier");
        public static readonly int _CloudRaymarchInCloudStepMultiplier = ID("_CloudRaymarchInCloudStepMultiplier");
        public static readonly int _CloudRayMarchParameters = ID("_CloudRayMarchParameters");
        public static readonly int _CloudRaymarchSampleDetailsForDirLight = ID("_CloudRaymarchSampleDetailsForDirLight");
        public static readonly int _CloudRayOffset = ID("_CloudRayOffset");
        public static readonly int _CloudRayOffsetVolumetric = ID("_CloudRayOffsetVolumetric");
        public static readonly int _CloudScatterMultiplier = ID("_CloudScatterMultiplier");
        public static readonly int _CloudShadowMapAdder = ID("_CloudShadowMapAdder");
        public static readonly int _CloudShadowMapMultiplier = ID("_CloudShadowMapMultiplier");
        public static readonly int _CloudShadowMapPower = ID("_CloudShadowMapPower");
        public static readonly int _CloudShapeAnimationVelocity = ID("_CloudShapeAnimationVelocity");
        public static readonly int _CloudStartSquaredVolumetric = ID("_CloudStartSquaredVolumetric");
        public static readonly int _CloudStartVolumetric = ID("_CloudStartVolumetric");
        public static readonly int _CloudStartInverseVolumetric = ID("_CloudStartInverseVolumetric");

        public static readonly int _CloudTypeAdder = ID("_CloudTypeAdder");
        public static readonly int _CloudTypeCurve = ID("_CloudTypeCurve");
        public static readonly int _CloudTypeFrequency = ID("_CloudTypeFrequency");
        public static readonly int _CloudTypeNegationAdder = ID("_CloudTypeNegationAdder");
        public static readonly int _CloudTypeNegationFrequency = ID("_CloudTypeNegationFrequency");
        public static readonly int _CloudTypeNegationOffset = ID("_CloudTypeNegationOffset");
        public static readonly int _CloudTypeNegationPower = ID("_CloudTypeNegationPower");
        public static readonly int _CloudTypeNegationRotation = ID("_CloudTypeNegationRotation");
        public static readonly int _CloudTypeNegationVelocity = ID("_CloudTypeNegationVelocity");
        public static readonly int _CloudTypeNegationWarpScale = ID("_CloudTypeNegationWarpScale");
        public static readonly int _CloudTypeNoiseType = ID("_CloudTypeNoiseType");
        public static readonly int _CloudTypeNoiseTypeInv = ID("_CloudTypeNoiseTypeInv");
        public static readonly int _CloudTypeOffset = ID("_CloudTypeOffset");
        public static readonly int _CloudTypePower = ID("_CloudTypePower");
        public static readonly int _CloudTypeRotation = ID("_CloudTypeRotation");
        public static readonly int _CloudTypeSecondaryVolumetric = ID("_CloudTypeSecondaryVolumetric");
        public static readonly int _CloudTypeVelocity = ID("_CloudTypeVelocity");
        public static readonly int _CloudTypeVolumetric = ID("_CloudTypeVolumetric");
        public static readonly int _CloudTypeWarpScale = ID("_CloudTypeWarpScale");

        public static readonly int _CloudVelocity = ID("_CloudVelocity");
        public static readonly int _CloudWeights = ID("_CloudWeights");
        public static readonly int _Cull = ID("_Cull");
        public static readonly int _DirectionalLightMultiplier = ID("_DirectionalLightMultiplier");
        public static readonly int _PointSpotLightMultiplier = ID("_PointSpotLightMultiplier");
        public static readonly int _DawnDuskTex = ID("_DawnDuskTex");
        public static readonly int _DayTex = ID("_DayTex");
        public static readonly int _DepthTexelSizeDest = ID("_DepthTexelSizeDest");
        public static readonly int _DepthTexelSizeSource = ID("_DepthTexelSizeSource");
        public static readonly int _DstBlendMode = ID("_DstBlendMode");
        public static readonly int _EmissionColor = ID("_EmissionColor");
        public static readonly int _Global_WetnessParams = ID("_Global_WetnessParams");
        public static readonly int _GlobalPorosityWetness = ID("_GlobalPorosityWetness");
        public static readonly int _Global_PuddleParams = ID("_Global_PuddleParams");
        public static readonly int _Global_RainIntensity = ID("_Global_RainIntensity");
        public static readonly int _Global_SnowLevel = ID("_Global_SnowLevel");
        public static readonly int _Global_StreamMax = ID("_Global_StreamMax");
        public static readonly int _KuwaharaCoords = ID("_KuwaharaCoords");
        public static readonly int _KuwaharaCoordsLength = ID("_KuwaharaCoordsLength");
        public static readonly int _MaskOffset = ID("_MaskOffset");
        public static readonly int _MainLightShadowmapTexture = ID("_MainLightShadowmapTexture");
        public static readonly int _MainTex = ID("_MainTex");
        public static readonly int _MainTex2 = ID("_MainTex2");
        public static readonly int _MainTex3 = ID("_MainTex3");
        public static readonly int _MainTex4 = ID("_MainTex4");
        public static readonly int _MainTex5 = ID("_MainTex5");
        public static readonly int _MainTex6 = ID("_MainTex6");
        public static readonly int _MaskTex = ID("_MaskTex");
        public static readonly int _PrevSdfTex = ID("_PrevSdfTex");
        public static readonly int _PrevSdfTex1 = ID("_PrevSdfTex1");
        public static readonly int _PrevSdfTex2 = ID("_PrevSdfTex2");
        public static readonly int _PrevSdfTex3 = ID("_PrevSdfTex3");
        public static readonly int _PrevSdfTex4 = ID("_PrevSdfTex4");
        public static readonly int _PrevSdfTex5 = ID("_PrevSdfTex5");
        public static readonly int _PrevSdfTex6 = ID("_PrevSdfTex6");
        public static readonly int _CurSdfTex = ID("_CurSdfTex");
        public static readonly int _WeatherMakerFogFactorMax = ID("_WeatherMakerFogFactorMax");
        public static readonly int _NightDuskMultiplier = ID("_NightDuskMultiplier");
        public static readonly int _NightIntensity = ID("_NightIntensity");
        public static readonly int _NightFogColor = ID("_NightFogColor");
        public static readonly int _NightFogPower = ID("_NightFogPower");
        public static readonly int _NightPower = ID("_NightPower");
        public static readonly int _NightSkyMultiplier = ID("_NightSkyMultiplier");
        public static readonly int _NightTex = ID("_NightTex");
        public static readonly int _NightTwinkleMinimum = ID("_NightTwinkleMinimum");
        public static readonly int _NightTwinkleRandomness = ID("_NightTwinkleRandomness");
        public static readonly int _NightTwinkleSpeed = ID("_NightTwinkleSpeed");
        public static readonly int _NightTwinkleVariance = ID("_NightTwinkleVariance");
        public static readonly int _NightVisibilityThreshold = ID("_NightVisibilityThreshold");
        public static readonly int _NullZoneCount = ID("_NullZoneCount");
        public static readonly int _NullZonesCenter = ID("_NullZonesCenter");
        public static readonly int _NullZonesMax = ID("_NullZonesMax");
        public static readonly int _NullZonesMin = ID("_NullZonesMin");
        public static readonly int _NullZonesParams = ID("_NullZonesParams");
        public static readonly int _NullZonesQuaternion = ID("_NullZonesQuaternion");
        public static readonly int _OverlayColor = ID("_OverlayColor");
        public static readonly int _OverlayIntensity = ID("_OverlayIntensity");
        public static readonly int _OverlayReflectionIntensity = ID("_OverlayReflectionIntensity");
        public static readonly int _OverlayMinHeight = ID("_OverlayMinHeight");
        public static readonly int _OverlayMinHeightFalloffMultiplier = ID("_OverlayMinHeightFalloffMultiplier");
        public static readonly int _OverlayMinHeightFalloffPower = ID("_OverlayMinHeightFalloffPower");
        public static readonly int _OverlayMinHeightNoiseAdder = ID("_OverlayMinHeightNoiseAdder");
        public static readonly int _OverlayMinHeightNoiseEnabled = ID("_OverlayMinHeightNoiseEnabled");
        public static readonly int _OverlayMinHeightNoiseMultiplier = ID("_OverlayMinHeightNoiseMultiplier");
        public static readonly int _OverlayMinHeightNoiseOffset = ID("_OverlayMinHeightNoiseOffset");
        public static readonly int _OverlayMinHeightNoiseScale = ID("_OverlayMinHeightNoiseScale");
        public static readonly int _OverlayMinHeightNoiseVelocity = ID("_OverlayMinHeightNoiseVelocity");
        public static readonly int _OverlayNoiseAdder = ID("_OverlayNoiseAdder");
        public static readonly int _OverlayNoiseEnabled = ID("_OverlayNoiseEnabled");
        public static readonly int _OverlayNoiseHeightTexture = ID("_OverlayNoiseHeightTexture");
        public static readonly int _OverlayNoiseMultiplier = ID("_OverlayNoiseMultiplier");
        public static readonly int _OverlayNoiseOffset = ID("_OverlayNoiseOffset");
        public static readonly int _OverlayNoisePower = ID("_OverlayNoisePower");
        public static readonly int _OverlayNoiseScale = ID("_OverlayNoiseScale");
        public static readonly int _OverlayNoiseTexture = ID("_OverlayNoiseTexture");
        public static readonly int _OverlayNoiseVelocity = ID("_OverlayNoiseVelocity");
        public static readonly int _OverlayNormalReducer = ID("_OverlayNormalReducer");
        public static readonly int _OverlayOffset = ID("_OverlayOffset");
        public static readonly int _OverlayReflectionTexture = ID("_OverlayReflectionTexture");
        public static readonly int _OverlayScale = ID("_OverlayScale");
        public static readonly int _OverlaySpecularColor = ID("_OverlaySpecularColor");
        public static readonly int _OverlaySpecularIntensity = ID("_OverlaySpecularIntensity");
        public static readonly int _OverlaySpecularPower = ID("_OverlaySpecularPower");
        public static readonly int _OverlayTexture = ID("_OverlayTexture");
        public static readonly int _OverlayVelocity = ID("_OverlayVelocity");
        public static readonly int _ParticleDitherLevel = ID("_ParticleDitherLevel");
        public static readonly int _PuddleBlend = ID("_PuddleBlend");
        public static readonly int _Radius = ID("_Radius");
        public static readonly int _RainIntensity = ID("_RainIntensity");
        public static readonly int _RealTimeCloudNoiseShapeTypes = ID("_RealTimeCloudNoiseShapeTypes");
        public static readonly int _RealTimeCloudNoiseShapePerlinParam1 = ID("_RealTimeCloudNoiseShapePerlinParam1");
        public static readonly int _RealTimeCloudNoiseShapePerlinParam2 = ID("_RealTimeCloudNoiseShapePerlinParam2");
        public static readonly int _RealTimeCloudNoiseShapeWorleyParam1 = ID("_RealTimeCloudNoiseShapeWorleyParam1");
        public static readonly int _RealTimeCloudNoiseShapeWorleyParam2 = ID("_RealTimeCloudNoiseShapeWorleyParam2");
        public static readonly int _ReflectionIntensity = ID("_ReflectionIntensity");
        public static readonly int _ScreenSpaceShadowmapTexture = ID("_ScreenSpaceShadowmapTexture");
        public static readonly int _SdfPixelSize = ID("_SdfPixelSize");
        public static readonly int _SnowAmount = ID("_SnowAmount");
        public static readonly int _Specular = ID("_Specular");
        public static readonly int _SrcBlendMode = ID("_SrcBlendMode");
        public static readonly int _TemporalReprojection_InverseProjection = ID("_TemporalReprojection_InverseProjection");
        public static readonly int _TemporalReprojection_InverseProjectionView = ID("_TemporalReprojection_InverseProjectionView");
        public static readonly int _TemporalReprojection_InverseView = ID("_TemporalReprojection_InverseView");
        public static readonly int _TemporalReprojection_ipivpvp = ID("_TemporalReprojection_ipivpvp");
        public static readonly int _TemporalReprojection_PrevDepth = ID("_TemporalReprojection_PrevDepth");
        public static readonly int _TemporalReprojection_PrevFrame = ID("_TemporalReprojection_PrevFrame");
        public static readonly int _TemporalReprojection_PreviousView = ID("_TemporalReprojection_PreviousView");
        public static readonly int _TemporalReprojection_PreviousViewProjection = ID("_TemporalReprojection_PreviousViewProjection");
        public static readonly int _TemporalReprojection_Projection = ID("_TemporalReprojection_Projection");
        public static readonly int _TemporalReprojection_SimilarityMax = ID("_TemporalReprojection_SimilarityMax");
        public static readonly int _TemporalReprojection_SubFrame = ID("_TemporalReprojection_SubFrame");
        public static readonly int _TemporalReprojection_SubFrameNumber = ID("_TemporalReprojection_SubFrameNumber");
        public static readonly int _TemporalReprojection_SubPixelSize = ID("_TemporalReprojection_SubPixelSize");
        public static readonly int _TemporalReprojection_View = ID("_TemporalReprojection_View");
        public static readonly int _TintColor = ID("_TintColor");
        public static readonly int _WaterCausticsTintColor = ID("_WaterCausticsTintColor");
        public static readonly int _WaterDepthThreshold = ID("_WaterDepthThreshold");
        public static readonly int _WaterFoamParam1 = ID("_WaterFoamParam1");
        public static readonly int _WaterFogDensity = ID("_WaterFogDensity");
        public static readonly int _WaterReflective = ID("_WaterReflective");
        public static readonly int _WaterRefractionStrength = ID("_WaterRefractionStrength");
        public static readonly int _WaterUnderwater = ID("_WaterUnderwater");
        public static readonly int _WaterWave1 = ID("_WaterWave1");
        public static readonly int _WaterWave2 = ID("_WaterWave2");
        public static readonly int _WaterWave3 = ID("_WaterWave3");
        public static readonly int _WaterWave4 = ID("_WaterWave4");
        public static readonly int _WaterWave5 = ID("_WaterWave5");
        public static readonly int _WaterWave6 = ID("_WaterWave6");
        public static readonly int _WaterWave7 = ID("_WaterWave7");
        public static readonly int _WaterWave8 = ID("_WaterWave8");
        public static readonly int _WaterWave1_Precompute = ID("_WaterWave1_Precompute");
        public static readonly int _WaterWave2_Precompute = ID("_WaterWave2_Precompute");
        public static readonly int _WaterWave3_Precompute = ID("_WaterWave3_Precompute");
        public static readonly int _WaterWave4_Precompute = ID("_WaterWave4_Precompute");
        public static readonly int _WaterWave5_Precompute = ID("_WaterWave5_Precompute");
        public static readonly int _WaterWave6_Precompute = ID("_WaterWave6_Precompute");
        public static readonly int _WaterWave7_Precompute = ID("_WaterWave7_Precompute");
        public static readonly int _WaterWave8_Precompute = ID("_WaterWave8_Precompute");
        public static readonly int _WaterWave1_Params1 = ID("_WaterWave1_Params1");
        public static readonly int _WaterWave2_Params1 = ID("_WaterWave2_Params1");
        public static readonly int _WaterWave3_Params1 = ID("_WaterWave3_Params1");
        public static readonly int _WaterWave4_Params1 = ID("_WaterWave4_Params1");
        public static readonly int _WaterWave5_Params1 = ID("_WaterWave5_Params1");
        public static readonly int _WaterWave6_Params1 = ID("_WaterWave6_Params1");
        public static readonly int _WaterWave7_Params1 = ID("_WaterWave7_Params1");
        public static readonly int _WaterWave8_Params1 = ID("_WaterWave8_Params1");
        public static readonly int _WaterWaveMultiplier = ID("_WaterWaveMultiplier");
        public static readonly int _WeatherMakerAmbientLightColor = ID("_WeatherMakerAmbientLightColor");
        public static readonly int _WeatherMakerAmbientLightColorEquator = ID("_WeatherMakerAmbientLightColorEquator");
        public static readonly int _WeatherMakerAmbientLightColorGround = ID("_WeatherMakerAmbientLightColorGround");
        public static readonly int _WeatherMakerAmbientLightColorSky = ID("_WeatherMakerAmbientLightColorSky");
        public static readonly int _WeatherMakerAdjustFullScreenUVStereoDisable = ID("_WeatherMakerAdjustFullScreenUVStereoDisable");
        public static readonly int _WeatherMakerAreaLightAtten = ID("_WeatherMakerAreaLightAtten");
        public static readonly int _WeatherMakerAreaLightColor = ID("_WeatherMakerAreaLightColor");
        public static readonly int _WeatherMakerAreaLightCount = ID("_WeatherMakerAreaLightCount");
        public static readonly int _WeatherMakerAreaLightDirection = ID("_WeatherMakerAreaLightDirection");
        public static readonly int _WeatherMakerAreaLightMaxPosition = ID("_WeatherMakerAreaLightMaxPosition");
        public static readonly int _WeatherMakerAreaLightMinPosition = ID("_WeatherMakerAreaLightMinPosition");
        public static readonly int _WeatherMakerAreaLightPosition = ID("_WeatherMakerAreaLightPosition");
        public static readonly int _WeatherMakerAreaLightPositionEnd = ID("_WeatherMakerAreaLightPositionEnd");
        public static readonly int _WeatherMakerAreaLightRotation = ID("_WeatherMakerAreaLightRotation");
        public static readonly int _WeatherMakerAreaLightVar1 = ID("_WeatherMakerAreaLightVar1");
        public static readonly int _WeatherMakerAreaLightViewportPosition = ID("_WeatherMakerAreaLightViewportPosition");
        public static readonly int _WeatherMakerAtmosphereHeight = ID("_WeatherMakerAtmosphereHeight");
        public static readonly int _WeatherMakerAtmosphereLevel = ID("_WeatherMakerAtmosphereLevel");
        public static readonly int _WeatherMakerAtmosphereLightShaftSampleCount = ID("_WeatherMakerAtmosphereLightShaftSampleCount");
        public static readonly int _WeatherMakerAtmosphereLightShaftMaxRayLength = ID("_WeatherMakerAtmosphereLightShaftMaxRayLength");
        public static readonly int _WeatherMakerAtmosphereLightShaftTexture = ID("_WeatherMakerAtmosphereLightShaftTexture");
        public static readonly int _WeatherMakerAtmospherePlanetRadius = ID("_WeatherMakerAtmospherePlanetRadius");
        public static readonly int _WeatherMakerAtmosphereRayLength = ID("_WeatherMakerAtmosphereRayLength");
        public static readonly int _WeatherMakerAuroraSampleCount = ID("_WeatherMakerAuroraSampleCount");
        public static readonly int _WeatherMakerAuroraSubSampleCount = ID("_WeatherMakerAuroraSubSampleCount");
        public static readonly int _WeatherMakerAuroraAnimationSpeed = ID("_WeatherMakerAuroraAnimationSpeed");
        public static readonly int _WeatherMakerAuroraMarchScale = ID("_WeatherMakerAuroraMarchScale");
        public static readonly int _WeatherMakerAuroraScale = ID("_WeatherMakerAuroraScale");
        public static readonly int _WeatherMakerAuroraColor = ID("_WeatherMakerAuroraColor");
        public static readonly int _WeatherMakerAuroraColorKeys = ID("_WeatherMakerAuroraColorKeys");
        public static readonly int _WeatherMakerAuroraIntensity = ID("_WeatherMakerAuroraIntensity");
        public static readonly int _WeatherMakerAuroraOctave = ID("_WeatherMakerAuroraOctave");
        public static readonly int _WeatherMakerAuroraPower = ID("_WeatherMakerAuroraPower");
        public static readonly int _WeatherMakerAuroraHeightFadePower = ID("_WeatherMakerAuroraHeightFadePower");
        public static readonly int _WeatherMakerAuroraHeight = ID("_WeatherMakerAuroraHeight");
        public static readonly int _WeatherMakerAuroraPlanetRadius = ID("_WeatherMakerAuroraPlanetRadius");
        public static readonly int _WeatherMakerAuroraDistanceFade = ID("_WeatherMakerAuroraDistanceFade");
        public static readonly int _WeatherMakerAuroraDither = ID("_WeatherMakerAuroraDither");
        public static readonly int _WeatherMakerAuroraSeed = ID("_WeatherMakerAuroraSeed");
        public static readonly int _WeatherMakerBackgroundSkyTexture = ID("_WeatherMakerBackgroundSkyTexture");
        public static readonly int _WeatherMakerBlueNoiseTexture = ID("_WeatherMakerBlueNoiseTexture");
        public static readonly int _WeatherMakerCameraFrustumCorners = ID("_WeatherMakerCameraFrustumCorners");
        public static readonly int _WeatherMakerCameraFrustumRays = ID("_WeatherMakerCameraFrustumRays");
        public static readonly int _WeatherMakerCameraFrustumRaysTemporal = ID("_WeatherMakerCameraFrustumRaysTemporal");
        public static readonly int _WeatherMakerCameraRenderMode = ID("_WeatherMakerCameraRenderMode");
        public static readonly int _WeatherMakerCameraOriginOffset = ID("_WeatherMakerCameraOriginOffset");
        public static readonly int _WeatherMakerCameraPosComputeShader = ID("_WeatherMakerCameraPosComputeShader");
        public static readonly int _WeatherMakerCloudAtmosphereShadow = ID("_WeatherMakerCloudAtmosphereShadow");
        public static readonly int _WeatherMakerCloudGlobalShadow = ID("_WeatherMakerCloudGlobalShadow");
        public static readonly int _WeatherMakerCloudGlobalShadow2 = ID("_WeatherMakerCloudGlobalShadow2");
        public static readonly int _WeatherMakerCloudShadowDetailTexture = ID("_WeatherMakerCloudShadowDetailTexture");
        public static readonly int _WeatherMakerCloudShadowDetailScale = ID("_WeatherMakerCloudShadowDetailScale");
        public static readonly int _WeatherMakerCloudShadowDetailIntensity = ID("_WeatherMakerCloudShadowDetailIntensity");
        public static readonly int _WeatherMakerCloudShadowDetailFalloff = ID("_WeatherMakerCloudShadowDetailFalloff");
        public static readonly int _WeatherMakerCloudShadowDistanceFade = ID("_WeatherMakerCloudShadowDistanceFade");
        public static readonly int _WeatherMakerCloudVolumetricShadowLod = ID("_WeatherMakerCloudVolumetricShadowLod");
        public static readonly int _WeatherMakerCloudShadowTexture = ID("_WeatherMakerCloudShadowTexture");
        public static readonly int _WeatherMakerCloudShadowSampleShadowMap = ID("_WeatherMakerCloudShadowSampleShadowMap");
        public static readonly int _WeatherMakerCloudVolumetricShadowSampleCount = ID("_WeatherMakerCloudVolumetricShadowSampleCount");
        public static readonly int _WeatherMakerCloudVolumetricShadow = ID("_WeatherMakerCloudVolumetricShadow");
        public static readonly int _WeatherMakerCloudVolumetricShadowDither = ID("_WeatherMakerCloudVolumetricShadowDither");
        public static readonly int _WeatherMakerCloudVolumetricWeatherMapRemapBoxMin = ID("_WeatherMakerCloudVolumetricWeatherMapRemapBoxMin");
        public static readonly int _WeatherMakerCloudVolumetricWeatherMapRemapBoxMax = ID("_WeatherMakerCloudVolumetricWeatherMapRemapBoxMax");
        public static readonly int _WeatherMakerCameraCubemap = ID("_WeatherMakerCameraCubemap");
        public static readonly int _WeatherMakerDawnDuskMultiplier = ID("_WeatherMakerDawnDuskMultiplier");
        public static readonly int _WeatherMakerDawnDuskMultiplierInv = ID("_WeatherMakerDawnDuskMultiplierInv");
        public static readonly int _WeatherMakerDayMultiplier = ID("_WeatherMakerDayMultiplier");
        public static readonly int _WeatherMakerDayMultiplierInv = ID("_WeatherMakerDayMultiplierInv");
        public static readonly int _WeatherMakerDeferredShading = ID("_WeatherMakerDeferredShading");
        public static readonly int _WeatherMakerDensityScaleHeight = ID("_WeatherMakerDensityScaleHeight");
        public static readonly int _WeatherMakerDitherTexture = ID("_WeatherMakerDitherTexture");
        public static readonly int _WeatherMakerAtmosphereDistanceScale = ID("_WeatherMakerAtmosphereDistanceScale");
        public static readonly int _WeatherMakerDirLightColor = ID("_WeatherMakerDirLightColor");
        public static readonly int _WeatherMakerDirLightCount = ID("_WeatherMakerDirLightCount");
        public static readonly int _WeatherMakerDirLightDirection = ID("_WeatherMakerDirLightDirection");
        public static readonly int _WeatherMakerDirLightMultiplier = ID("_WeatherMakerDirLightMultiplier");
        public static readonly int _WeatherMakerDirLightPosition = ID("_WeatherMakerDirLightPosition");
        public static readonly int _WeatherMakerDirLightPower = ID("_WeatherMakerDirLightPower");
        public static readonly int _WeatherMakerDirLightQuaternion = ID("_WeatherMakerDirLightQuaternion");
        public static readonly int _WeatherMakerDirLightVar1 = ID("_WeatherMakerDirLightVar1");
        public static readonly int _WeatherMakerDirLightViewportPosition = ID("_WeatherMakerDirLightViewportPosition");
        public static readonly int _WeatherMakerDirectionalLightScatterMultiplier = ID("_WeatherMakerDirectionalLightScatterMultiplier");
        public static readonly int _WeatherMakerDownsampleScale = ID("_WeatherMakerDownsampleScale");
        public static readonly int _WeatherMakerEnableToneMapping = ID("_WeatherMakerEnableToneMapping");
        public static readonly int _WeatherMakerExtinctionLUT = ID("_WeatherMakerExtinctionLUT");
        public static readonly int _WeatherMakerExtinctionLUT2 = ID("_WeatherMakerExtinctionLUT2");
        public static readonly int _WeatherMakerExtinctionM = ID("_WeatherMakerExtinctionM");
        public static readonly int _WeatherMakerExtinctionR = ID("_WeatherMakerExtinctionR");
        public static readonly int _WeatherMakerFogBoxCenter = ID("_WeatherMakerFogBoxCenter");
        public static readonly int _WeatherMakerFogBoxMax = ID("_WeatherMakerFogBoxMax");
        public static readonly int _WeatherMakerFogBoxMaxDir = ID("_WeatherMakerFogBoxMaxDir");
        public static readonly int _WeatherMakerFogBoxMin = ID("_WeatherMakerFogBoxMin");
        public static readonly int _WeatherMakerFogBoxMinDir = ID("_WeatherMakerFogBoxMinDir");
        public static readonly int _WeatherMakerFogCloudShadowStrength = ID("_WeatherMakerFogCloudShadowStrength");
        public static readonly int _WeatherMakerFogColor = ID("_WeatherMakerFogColor");
        public static readonly int _WeatherMakerFogDensity = ID("_WeatherMakerFogDensity");
        public static readonly int _WeatherMakerFogDensityScatter = ID("_WeatherMakerFogDensityScatter");
        public static readonly int _WeatherMakerFogDirectionalLightScatterIntensity = ID("_WeatherMakerFogDirectionalLightScatterIntensity");
        public static readonly int _WeatherMakerFogDitherLevel = ID("_WeatherMakerFogDitherLevel");
        public static readonly int _WeatherMakerFogEmissionColor = ID("_WeatherMakerFogEmissionColor");
        public static readonly int _WeatherMakerFogFactorMultiplier = ID("_WeatherMakerFogFactorMultiplier");
        public static readonly int _WeatherMakerFogGlobalShadow = ID("_WeatherMakerFogGlobalShadow");
        public static readonly int _WeatherMakerFogHeight = ID("_WeatherMakerFogHeight");
        public static readonly int _WeatherMakerFogHeightVariance = ID("_WeatherMakerFogHeightVariance");
        public static readonly int _WeatherMakerFogHeightVarianceScale = ID("_WeatherMakerFogHeightVarianceScale");
        public static readonly int _WeatherMakerFogHeightFalloffPower = ID("_WeatherMakerFogHeightFalloffPower");
        public static readonly int _WeatherMakerFogLightAbsorption = ID("_WeatherMakerFogLightAbsorption");
        public static readonly int _WeatherMakerFogLightFalloff = ID("_WeatherMakerFogLightFalloff");
        public static readonly int _WeatherMakerFogLightShadowBrightness = ID("_WeatherMakerFogLightShadowBrightness");
        public static readonly int _WeatherMakerFogLightShadowDecay = ID("_WeatherMakerFogLightShadowDecay");
        public static readonly int _WeatherMakerFogLightShadowDither = ID("_WeatherMakerFogLightShadowDither");
        public static readonly int _WeatherMakerFogLightShadowDitherMagic = ID("_WeatherMakerFogLightShadowDitherMagic");
        public static readonly int _WeatherMakerFogLightShadowInvSampleCount = ID("_WeatherMakerFogLightShadowInvSampleCount");
        public static readonly int _WeatherMakerFogLightShadowMaxRayLength = ID("_WeatherMakerFogLightShadowMaxRayLength");
        public static readonly int _WeatherMakerFogLightShadowMultiplier = ID("_WeatherMakerFogLightShadowMultiplier");
        public static readonly int _WeatherMakerFogLightShadowPower = ID("_WeatherMakerFogLightShadowPower");
        public static readonly int _WeatherMakerFogLightShadowSampleCount = ID("_WeatherMakerFogLightShadowSampleCount");
        public static readonly int _WeatherMakerFogLightSunIntensityReducer = ID("_WeatherMakerFogLightSunIntensityReducer");
        public static readonly int _WeatherMakerFogLinearFogFactor = ID("_WeatherMakerFogLinearFogFactor");
        public static readonly int _WeatherMakerFogMaxFogFactor = ID("_WeatherMakerFogMaxFogFactor");
        public static readonly int _WeatherMakerFogMinShadow = ID("_WeatherMakerFogMinShadow");
        public static readonly int _WeatherMakerFogMode = ID("_WeatherMakerFogMode");
        public static readonly int _WeatherMakerFogNoiseAdder = ID("_WeatherMakerFogNoiseAdder");
        public static readonly int _WeatherMakerFogNoiseEnabled = ID("_WeatherMakerFogNoiseEnabled");
        public static readonly int _WeatherMakerFogNoiseMultiplier = ID("_WeatherMakerFogNoiseMultiplier");
        public static readonly int _WeatherMakerFogNoisePercent = ID("_WeatherMakerFogNoisePercent");
        public static readonly int _WeatherMakerFogNoisePositionOffset = ID("_WeatherMakerFogNoisePositionOffset");
        public static readonly int _WeatherMakerFogNoiseSampleCount = ID("_WeatherMakerFogNoiseSampleCount");
        public static readonly int _WeatherMakerFogNoiseSampleCountInverse = ID("_WeatherMakerFogNoiseSampleCountInverse");
        public static readonly int _WeatherMakerFogNoiseScale = ID("_WeatherMakerFogNoiseScale");
        public static readonly int _WeatherMakerFogNoiseVelocity = ID("_WeatherMakerFogNoiseVelocity");
        public static readonly int _WeatherMakerFogSpherePosition = ID("_WeatherMakerFogSpherePosition");
        public static readonly int _WeatherMakerFogStartDepth = ID("_WeatherMakerFogStartDepth");
        public static readonly int _WeatherMakerFogEndDepth = ID("_WeatherMakerFogEndDepth");
        public static readonly int _WeatherMakerFogSunShaftMode = ID("_WeatherMakerFogSunShaftMode");
        public static readonly int _WeatherMakerFogSunShaftsDitherMagic = ID("_WeatherMakerFogSunShaftsDitherMagic");
        public static readonly int _WeatherMakerFogSunShaftsParam1 = ID("_WeatherMakerFogSunShaftsParam1");
        public static readonly int _WeatherMakerFogSunShaftsParam2 = ID("_WeatherMakerFogSunShaftsParam2");
        public static readonly int _WeatherMakerFogSunShaftsTintColor = ID("_WeatherMakerFogSunShaftsTintColor");
        public static readonly int _WeatherMakerFogSunShaftsBackgroundIntensity = ID("_WeatherMakerFogSunShaftsBackgroundIntensity");
        public static readonly int _WeatherMakerFogSunShaftsBackgroundTintColor = ID("_WeatherMakerFogSunShaftsBackgroundTintColor");
        public static readonly int _WeatherMakerFogVolumePower = ID("_WeatherMakerFogVolumePower");
        public static readonly int _WeatherMakerFogVolumetricLightMode = ID("_WeatherMakerFogVolumetricLightMode");
        public static readonly int _WeatherMakerInscatteringLUT = ID("_WeatherMakerInscatteringLUT");
        public static readonly int _WeatherMakerInscatteringLUT2 = ID("_WeatherMakerInscatteringLUT2");
        public static readonly int _WeatherMakerInscatteringLUT_Dimensions = ID("_WeatherMakerInscatteringLUT_Dimensions");
        public static readonly int _WeatherMakerIncomingLight = ID("_WeatherMakerIncomingLight");
        public static readonly int _WeatherMakerInverseProj = ID("_WeatherMakerInverseProj");
        public static readonly int _WeatherMakerInverseView = ID("_WeatherMakerInverseView");
        public static readonly int _WeatherMakerAtmosphereLightShaftEnable = ID("_WeatherMakerAtmosphereLightShaftEnable");
        public static readonly int _WeatherMakerMieG = ID("_WeatherMakerMieG");
        public static readonly int _WeatherMakerNightMultiplier = ID("_WeatherMakerNightMultiplier");
        public static readonly int _WeatherMakerNightMultiplierInv = ID("_WeatherMakerNightMultiplierInv");
        public static readonly int _WeatherMakerNoiseTexture3D = ID("_WeatherMakerNoiseTexture3D");
        public static readonly int _WeatherMakerPerPixelLighting = ID("_WeatherMakerPerPixelLighting");
        public static readonly int _WeatherMakerPrecipitationLightMultiplier = ID("_WeatherMakerPrecipitationLightMultiplier");
        public static readonly int _WeatherMakerParticleDensityLUT = ID("_WeatherMakerParticleDensityLUT");
        public static readonly int _WeatherMakerPointLightAtten = ID("_WeatherMakerPointLightAtten");
        public static readonly int _WeatherMakerPointLightColor = ID("_WeatherMakerPointLightColor");
        public static readonly int _WeatherMakerPointLightCount = ID("_WeatherMakerPointLightCount");
        public static readonly int _WeatherMakerPointLightDirection = ID("_WeatherMakerPointLightDirection");
        public static readonly int _WeatherMakerPointLightPosition = ID("_WeatherMakerPointLightPosition");
        public static readonly int _WeatherMakerPointLightVar1 = ID("_WeatherMakerPointLightVar1");
        public static readonly int _WeatherMakerPointLightViewportPosition = ID("_WeatherMakerPointLightViewportPosition");
        public static readonly int _WeatherMakerRandomVectors = ID("_WeatherMakerRandomVectors");
        public static readonly int _WeatherMakerScatteringM = ID("_WeatherMakerScatteringM");
        public static readonly int _WeatherMakerScatteringR = ID("_WeatherMakerScatteringR");
        public static readonly int _WeatherMakerSkyAddColor = ID("_WeatherMakerSkyAddColor");
        public static readonly int _WeatherMakerSkyAtmosphereParams = ID("_WeatherMakerSkyAtmosphereParams");
        public static readonly int _WeatherMakerSkyboxLUT = ID("_WeatherMakerSkyboxLUT");
        public static readonly int _WeatherMakerSkyboxLUT2 = ID("_WeatherMakerSkyboxLUT2");
        public static readonly int _WeatherMakerSkyboxLUT_Dimensions = ID("_WeatherMakerSkyboxLUT_Dimensions");
        public static readonly int _WeatherMakerSkyDitherLevel = ID("_WeatherMakerSkyDitherLevel");
        public static readonly int _WeatherMakerSkyEnableNightTwinkle = ID("_WeatherMakerSkyEnableNightTwinkle");
        public static readonly int _WeatherMakerSkyEnableSunEclipse = ID("_WeatherMakerSkyEnableSunEclipse");
        public static readonly int _WeatherMakerSkyFade = ID("_WeatherMakerSkyFade");
        public static readonly int _WeatherMakerSkyGroundColor = ID("_WeatherMakerSkyGroundColor");
        public static readonly int _WeatherMakerSkyLightPIScattering = ID("_WeatherMakerSkyLightPIScattering");
        public static readonly int _WeatherMakerSkyLightScattering = ID("_WeatherMakerSkyLightScattering");
        public static readonly int _WeatherMakerSkyMie = ID("_WeatherMakerSkyMie");
        public static readonly int _WeatherMakerSkyMieG = ID("_WeatherMakerSkyMieG");
        public static readonly int _WeatherMakerSkyRadius = ID("_WeatherMakerSkyRadius");
        public static readonly int _WeatherMakerSkyRenderType = ID("_WeatherMakerSkyRenderType");
        public static readonly int _WeatherMakerSkyRotation = ID("_WeatherMakerSkyRotation");
        public static readonly int _WeatherMakerSkyScale = ID("_WeatherMakerSkyScale");
        public static readonly int _WeatherMakerSkyTintColor = ID("_WeatherMakerSkyTintColor");
        public static readonly int _WeatherMakerSkyTintColor2 = ID("_WeatherMakerSkyTintColor2");
        public static readonly int _WeatherMakerSkyTotalMie = ID("_WeatherMakerSkyTotalMie");
        public static readonly int _WeatherMakerSkyTotalRayleigh = ID("_WeatherMakerSkyTotalRayleigh");
        public static readonly int _WeatherMakerSkyYOffset2D = ID("_WeatherMakerSkyYOffset2D");
        public static readonly int _WeatherMakerSpotLightAtten = ID("_WeatherMakerSpotLightAtten");
        public static readonly int _WeatherMakerSpotLightColor = ID("_WeatherMakerSpotLightColor");
        public static readonly int _WeatherMakerSpotLightCount = ID("_WeatherMakerSpotLightCount");
        public static readonly int _WeatherMakerSpotLightDirection = ID("_WeatherMakerSpotLightDirection");
        public static readonly int _WeatherMakerSpotLightPosition = ID("_WeatherMakerSpotLightPosition");
        public static readonly int _WeatherMakerSpotLightSpotEnd = ID("_WeatherMakerSpotLightSpotEnd");
        public static readonly int _WeatherMakerSpotLightVar1 = ID("_WeatherMakerSpotLightVar1");
        public static readonly int _WeatherMakerSpotLightViewportPosition = ID("_WeatherMakerSpotLightViewportPosition");
        public static readonly int _WeatherMakerStereoEyeIndex = ID("_WeatherMakerStereoEyeIndex");
        public static readonly int _WeatherMakerSunColor = ID("_WeatherMakerSunColor");
        public static readonly int _WeatherMakerSunDirectionDown = ID("_WeatherMakerSunDirectionDown");
        public static readonly int _WeatherMakerSunDirectionDown2D = ID("_WeatherMakerSunDirectionDown2D");
        public static readonly int _WeatherMakerSunDirectionUp = ID("_WeatherMakerSunDirectionUp");
        public static readonly int _WeatherMakerSunDirectionUp2D = ID("_WeatherMakerSunDirectionUp2D");
        public static readonly int _WeatherMakerSunLightPower = ID("_WeatherMakerSunLightPower");
        public static readonly int _WeatherMakerSunPositionNormalized = ID("_WeatherMakerSunPositionNormalized");
        public static readonly int _WeatherMakerSunPositionWorldSpace = ID("_WeatherMakerSunPositionWorldSpace");
        public static readonly int _WeatherMakerSunTintColor = ID("_WeatherMakerSunTintColor");
        public static readonly int _WeatherMakerSunVar1 = ID("_WeatherMakerSunVar1");
        public static readonly int _WeatherMakerTemporalReprojectionEnabled = ID("_WeatherMakerTemporalReprojectionEnabled");
        public static readonly int _WeatherMakerTemporalReprojectionAlphaThreshold = ID("_WeatherMakerTemporalReprojectionAlphaThreshold");
        public static readonly int _WeatherMakerTemporalUV = ID("_WeatherMakerTemporalUV");
        public static readonly int _WeatherMakerTemporalUV_FragmentShader = ID("_WeatherMakerTemporalUV_FragmentShader");
        public static readonly int _WeatherMakerTemporalUV_VertexShaderProjection = ID("_WeatherMakerTemporalUV_VertexShaderProjection");
        public static readonly int _WeatherMakerTime = ID("_WeatherMakerTime");
        public static readonly int _WeatherMakerTimeSin = ID("_WeatherMakerTimeSin");
        public static readonly int _WeatherMakerTimeAngle = ID("_WeatherMakerTimeAngle");
        public static readonly int _WeatherMakerView = ID("_WeatherMakerView");
        public static readonly int _WeatherMakerVolumetricPointSpotMultiplier = ID("_WeatherMakerVolumetricPointSpotMultiplier");
        public static readonly int _WeatherMakerVREnabled = ID("_WeatherMakerVREnabled");
        public static readonly int _WeatherMakerWaterDepthMaxUV = ID("_WeatherMakerWaterDepthMaxUV");
        public static readonly int _WeatherMakerWeatherMapScale = ID("_WeatherMakerWeatherMapScale");
        public static readonly int _WeatherMakerWeatherMapSeed = ID("_WeatherMakerWeatherMapSeed");
        public static readonly int _WeatherMakerWeatherMapTexture = ID("_WeatherMakerWeatherMapTexture");
        public static readonly int _WeatherMakerWeatherMapTextureStatic = ID("_WeatherMakerWeatherMapTextureStatic");
        public static readonly int _WeatherMakerWeatherMapMaskTexture = ID("_WeatherMakerWeatherMapMaskTexture");
        public static readonly int _WeatherMakerYDepthParams = ID("_WeatherMakerYDepthParams");
        public static readonly int _WeatherMakerYDepthTexture = ID("_WeatherMakerYDepthTexture");
        public static readonly int _ZTest = ID("_ZTest");
        public static readonly int _ZWrite = ID("_ZWrite");
        public static readonly int rtp_snow_strength = ID("rtp_snow_strength");
        public static readonly int TERRAIN_GlobalWetness = ID("TERRAIN_GlobalWetness");
        public static readonly int TERRAIN_RainIntensity = ID("TERRAIN_RainIntensity");

        #endregion Ids

        static WMS()
        {
        }

        public static void SetColorArray(Material m, int prop, Color c1, Color c2, Color c3, Color c4)
        {
            tmpColorArray[0] = c1;
            tmpColorArray[1] = c2;
            tmpColorArray[2] = c3;
            tmpColorArray[3] = c4;
            m.SetColorArray(prop, tmpColorArray);
        }

        public static void SetFloatArray(Material m, int prop, float f1, float f2, float f3, float f4)
        {
            tmpFloatArray[0] = f1;
            tmpFloatArray[1] = f2;
            tmpFloatArray[2] = f3;
            tmpFloatArray[3] = f4;
            m.SetFloatArray(prop, tmpFloatArray);
        }

        public static void SetFloatArrayRotation(Material m, int prop, float f1, float f2, float f3, float f4)
        {
            tmpFloatArray2[0] = Mathf.Cos(f1 * Mathf.Deg2Rad);
            tmpFloatArray2[1] = Mathf.Cos(f2 * Mathf.Deg2Rad);
            tmpFloatArray2[2] = Mathf.Cos(f3 * Mathf.Deg2Rad);
            tmpFloatArray2[3] = Mathf.Cos(f4 * Mathf.Deg2Rad);
            tmpFloatArray2[4] = Mathf.Sin(f1 * Mathf.Deg2Rad);
            tmpFloatArray2[5] = Mathf.Sin(f2 * Mathf.Deg2Rad);
            tmpFloatArray2[6] = Mathf.Sin(f3 * Mathf.Deg2Rad);
            tmpFloatArray2[7] = Mathf.Sin(f4 * Mathf.Deg2Rad);
            m.SetFloatArray(prop, tmpFloatArray2);
        }

        public static void SetVectorArray(Material m, int prop, Vector4 v1, Vector4 v2, Vector4 v3, Vector4 v4)
        {
            tmpVectorArray[0] = v1;
            tmpVectorArray[1] = v2;
            tmpVectorArray[2] = v3;
            tmpVectorArray[3] = v4;
            m.SetVectorArray(prop, tmpVectorArray);
        }

        public static void DisableKeywords(this Material m, params string[] keywords)
        {
            foreach (string keyword in keywords)
            {
                m.DisableKeyword(keyword);
            }
        }

        public static void Initialize()
        {
        }
    }

#pragma warning restore 1591

}
