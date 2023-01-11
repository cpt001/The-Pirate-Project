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

#ifndef __WEATHER_MAKER_CLOUD_VOLUMETRIC_UNIFORMS_SHADER__
#define __WEATHER_MAKER_CLOUD_VOLUMETRIC_UNIFORMS_SHADER__

#include "WeatherMakerMathShaderInclude.cginc"
#include "WeatherMakerCloudShaderUniformsInclude.cginc"

// WARNING - THIS WILL BE VERY PERFORMANCE INTENSIVE AND CAN LOCK UP THE EDITOR
//#define VOLUMETRIC_CLOUD_REAL_TIME_NOISE

#if defined(VOLUMETRIC_CLOUD_REAL_TIME_NOISE)

// #include "WeatherMakerCloudNoiseShaderInclude.cginc"

#endif

// 1 = normal lighting, 2 = heatmap (raymarch cost)
#define VOLUMETRIC_CLOUD_RENDER_MODE 1

// increase lod as optical depth increases
#define VOLUMETRIC_LOD_OPTICAL_DEPTH_MULTIPLIER 10.0

// amount to multiply optical depth by to increase sky ambient for clouds (clouds closer up have less sky ambient)
#define VOLUMETRIC_SKY_AMBIENT_OPTICAL_DEPTH_MULTIPLIER 10.0

// min amount of distance for each ray march
#define VOLUMETRIC_MIN_STEP_LENGTH 16.0

// max amount of distance for each ray march
#define VOLUMETRIC_MAX_STEP_LENGTH 8192.0

// min noise value to process cloud
#define VOLUMETRIC_CLOUD_MIN_NOISE_VALUE 0.01

// max alpha value to accumulate for clouds
#define VOLUMETRIC_CLOUD_MAX_ALPHA 0.99
#define VOLUMETRIC_CLOUD_MAX_ALPHA_INV 1.011//(1.0 / VOLUMETRIC_CLOUD_MAX_ALPHA)

// max ray march length for volumetric cloud shadows
#define VOLUMETRIC_CLOUD_SHADOW_MAX_RAY_LENGTH 8192.0

// change to 1 to enable volumetric cloud shadow fade
#define WEATHER_MAKER_ENABLE_SHADOW_FADE 0

uniform sampler3D _CloudNoiseShapeVolumetric;
uniform sampler3D _CloudNoiseDetailVolumetric;
uniform sampler3D _CloudNoiseCurlVolumetric;
uniform sampler2D _WeatherMakerWeatherMapTexture;
//UNITY_DECLARE_TEX2D(_WeatherMakerWeatherMapTexture);
uniform float4 _WeatherMakerWeatherMapTexture_TexelSize;
uniform sampler2D _WeatherMakerCloudShadowTexture;
uniform float4 _WeatherMakerCloudShadowTexture_TexelSize;
uniform uint2 _CloudNoiseSampleCountVolumetric;
uniform float2 _CloudNoiseLodVolumetric;
uniform float4 _CloudNoiseScaleVolumetric; // shape scale, details scale, curl scale, curl multiplier
uniform float _CloudNoiseScalarVolumetric;
uniform float _CloudNoiseDetailPowerVolumetric;
uniform float _CloudNoiseDetailHeightMultiplier;
uniform fixed4 _CloudColorVolumetric;
uniform fixed4 _CloudDirColorVolumetric;
uniform fixed4 _CloudEmissionColorVolumetric;

uniform float3 _CloudShapeAnimationVelocity;
uniform float3 _CloudDetailAnimationVelocity;

uniform float _CloudAmbientGroundIntensityVolumetric;
uniform float _CloudAmbientSkyIntensityVolumetric;
uniform float _CloudAmbientSkyHeightMultiplierVolumetric;
uniform float _CloudAmbientGroundHeightMultiplierVolumetric;
uniform float _CloudAmbientShadowVolumetric;
uniform float _CloudLightAbsorptionVolumetric;
uniform float _CloudDirLightIndirectMultiplierVolumetric;
uniform float _CloudPowderMultiplierVolumetric;
uniform float4 _CloudHenyeyGreensteinPhaseVolumetric;
uniform float _CloudRaymarchMultiplierVolumetric;
uniform float2 _CloudRayDitherVolumetric;
uniform float _CloudOpticalDistanceMultiplierVolumetric;
uniform float4 _CloudHorizonFadeVolumetric;
uniform float4 _CloudRayMarchParameters;

uniform float _CloudCoverVolumetricMinimumForCloud;
uniform float _CloudCoverVolumetric;
uniform float _CloudCoverSecondaryVolumetric;
uniform float _CloudDensityVolumetric;
uniform float _CloudDensitySecondaryVolumetric;
uniform float _CloudTypeVolumetric;
uniform float _CloudTypeSecondaryVolumetric;

// cloud layer start from sea level
uniform float _CloudStartVolumetric;
uniform float _CloudStartInverseVolumetric;
uniform float _CloudStartSquaredVolumetric;
uniform float _CloudPlanetStartVolumetric; // cloud start + planet radius
uniform float _CloudPlanetStartSquaredVolumetric; // cloud start + planet radius, squared

// cloud layer height from start (relative to start)
uniform float _CloudHeightVolumetric;
uniform float _CloudHeightInverseVolumetric;
uniform float _CloudHeightSquaredVolumetric;
uniform float _CloudHeightSquaredInverseVolumetric;

// cloud layer end from sea level
uniform float _CloudEndVolumetric;
uniform float _CloudEndInverseVolumetric;
uniform float _CloudEndSquaredVolumetric;
uniform float _CloudEndSquaredInverseVolumetric;
uniform float _CloudPlanetEndVolumetric; // cloud end + planet radius
uniform float _CloudPlanetEndSquaredVolumetric; // cloud end + planet radius, squared

uniform float _CloudPlanetRadiusVolumetric;
uniform float _CloudPlanetRadiusNegativeVolumetric;
uniform float _CloudPlanetRadiusSquaredVolumetric;

uniform float4 _WeatherMakerCloudVolumetricWeatherMapRemapBoxMin;
uniform float4 _WeatherMakerCloudVolumetricWeatherMapRemapBoxMax;

uniform float _CloudShadowThresholdVolumetric;
uniform float _CloudShadowPowerVolumetric;
uniform float _CloudShadowMultiplierVolumetric;
uniform float _CloudRayOffsetVolumetric;
uniform float _CloudLightStepMultiplierVolumetric;
uniform float _CloudLightRadiusMultiplierVolumetric;
uniform float _CloudLightDistantMultiplierVolumetric;
uniform uint _CloudDirLightSampleCount;
uniform uint _CloudDirLightSubSampleCount;
uniform float _CloudDirLightLod;
uniform uint _WeatherMakerCloudVolumetricShadowSampleCount;
uniform uint _WeatherMakerCloudVolumetricShadowLod;

uniform float4 _WeatherMakerWeatherMapScale;
uniform float _CloudShadowMapAdder;
uniform float _CloudShadowMapMultiplier;
uniform float _CloudShadowMapPower;
uniform float _WeatherMakerCloudVolumetricShadowDither;
uniform sampler2D _WeatherMakerCloudShadowDetailTexture;
uniform float _WeatherMakerCloudShadowDetailScale;
uniform float _WeatherMakerCloudShadowDetailIntensity;
uniform float _WeatherMakerCloudShadowDetailFalloff;
uniform float _WeatherMakerCloudShadowSampleShadowMap;
uniform float _WeatherMakerCloudAtmosphereShadow;

// fade volumetric shadows at horizon. 0 would be no fade, 1 would be linear fade, 2 is double distane fade, etc.
uniform float _WeatherMakerCloudShadowDistanceFade; // 1.75
//uniform uint _WeatherMakerShadowCascades;

// cloud dir light rays
uniform uint _CloudDirLightRaySampleCount;
uniform float _CloudDirLightRayDensity;
uniform float _CloudDirLightRayDecay;
uniform float _CloudDirLightRayWeight;
uniform float _CloudDirLightRayBrightness;

uniform float4 _CloudGradientStratus;
uniform float4 _CloudGradientStratoCumulus;
uniform float4 _CloudGradientCumulus;
uniform float4 _CloudWeights;

// ray march optimizations
uniform float _CloudRaymarchMaybeInCloudStepMultiplier; // 1.0
uniform float _CloudRaymarchInCloudStepMultiplier; // 1.0
uniform uint _CloudRaymarchSampleDetailsForDirLight;

// weather map
uniform float _WeatherMakerWeatherMapSeed;

uniform float _CloudCoverageFrequency;
uniform float2 _CloudCoverageRotation;
uniform float3 _CloudVelocity;
uniform float3 _CloudCoverageVelocity; // needs to be prescaled by _WeatherMakerWeatherMapScale.z
uniform float3 _CloudCoverageOffset;
uniform float _CloudCoverageMultiplier;
uniform float _CloudCoverageAdder;
uniform float _CloudCoveragePower;
uniform float _CloudCoverageProfileInfluence;
uniform sampler2D _CloudCoverageTexture;
uniform float _CloudCoverageTextureMultiplier;
uniform float _CloudCoverageTextureScale;
uniform float _CloudCoverageNoiseType;
uniform float _CloudCoverageNoiseTypeInv;
uniform float4 _CloudCoverageWarpScale;

uniform float _CloudCoverageNegationFrequency;
uniform float2 _CloudCoverageNegationRotation;
uniform float3 _CloudCoverageNegationVelocity; // needs to be prescaled by _WeatherMakerWeatherMapScale.z
uniform float3 _CloudCoverageNegationOffset;
uniform float _CloudCoverageNegationMultiplier;
uniform float _CloudCoverageNegationAdder;
uniform float _CloudCoverageNegationPower;
uniform float4 _CloudCoverageNegationWarpScale;

uniform float _CloudDensityFrequency;
uniform float2 _CloudDensityRotation;
uniform float3 _CloudDensityVelocity; // needs to be prescaled by _WeatherMakerWeatherMapScale.z
uniform float3 _CloudDensityOffset;
uniform float _CloudDensityMultiplier;
uniform float _CloudDensityAdder;
uniform float _CloudDensityPower;
uniform float _CloudDensityProfileInfluence;
uniform float _CloudDensityCoveragePower;
uniform sampler2D _CloudDensityTexture;
uniform float _CloudDensityTextureMultiplier;
uniform float _CloudDensityTextureScale;
uniform float _CloudDensityNoiseType;
uniform float _CloudDensityNoiseTypeInv;
uniform float4 _CloudDensityWarpScale;

uniform float _CloudDensityNegationFrequency;
uniform float2 _CloudDensityNegationRotation;
uniform float3 _CloudDensityNegationVelocity; // needs to be prescaled by _WeatherMakerWeatherMapScale.z
uniform float3 _CloudDensityNegationOffset;
uniform float _CloudDensityNegationMultiplier;
uniform float _CloudDensityNegationAdder;
uniform float _CloudDensityNegationPower;
uniform float4 _CloudDensityNegationWarpScale;

uniform float _CloudTypeFrequency;
uniform float2 _CloudTypeRotation;
uniform float3 _CloudTypeVelocity; // needs to be prescaled by _WeatherMakerWeatherMapScale.z
uniform float3 _CloudTypeOffset;
uniform float _CloudTypeMultiplier;
uniform float _CloudTypeAdder;
uniform float _CloudTypePower;
uniform float _CloudTypeProfileInfluence;
uniform float _CloudTypeCoveragePower;
uniform sampler2D _CloudTypeTexture;
uniform float _CloudTypeTextureMultiplier;
uniform float _CloudTypeTextureScale;
uniform float _CloudTypeNoiseType;
uniform float _CloudTypeNoiseTypeInv;
uniform float4 _CloudTypeWarpScale;

uniform float _CloudTypeNegationFrequency;
uniform float2 _CloudTypeNegationRotation;
uniform float3 _CloudTypeNegationVelocity; // needs to be prescaled by _WeatherMakerWeatherMapScale.z
uniform float3 _CloudTypeNegationOffset;
uniform float _CloudTypeNegationMultiplier;
uniform float _CloudTypeNegationAdder;
uniform float _CloudTypeNegationPower;
uniform float4 _CloudTypeNegationWarpScale;

// cloud shaping / curves
uniform float _CloudAnvilStrength;
uniform float _CloudAnvilStart;
uniform float _CloudCoverageCurve[16];
uniform float _CloudDensityCurve[16];
uniform float _CloudTypeCurve[16];
uniform float _CloudNoiseDetailPowerCurveVolumetric[16];

#if defined(VOLUMETRIC_CLOUD_REAL_TIME_NOISE)

// real-time noise
uniform int _RealTimeCloudNoiseShapeTypes[4];
uniform float4 _RealTimeCloudNoiseShapePerlinParam1[4];
uniform float4 _RealTimeCloudNoiseShapePerlinParam2[4];
uniform float4 _RealTimeCloudNoiseShapeWorleyParam1[4];
uniform float4 _RealTimeCloudNoiseShapeWorleyParam2[4];

/*
uniform int _RealTimeCloudNoiseDetailTypes[4];
uniform float4 _RealTimeCloudNoiseDetailPerlinParam1[4];
uniform float4 _RealTimeCloudNoiseDetailPerlinParam2[4];
uniform float4 _RealTimeCloudNoiseDetailWorleyParam1[4];
uniform float4 _RealTimeCloudNoiseDetailWorleyParam2[4];
*/

#endif

static const float volumetricPositionShapeScale = _CloudHeightInverseVolumetric * _CloudNoiseScaleVolumetric.x;
static const float volumetricPositionShapeScaleExists = (volumetricPositionShapeScale > 0.0);
static const float volumetricPositionDetailScale = _CloudHeightInverseVolumetric * _CloudNoiseScaleVolumetric.y;
static const float volumetricPositionCurlScale = (_CloudHeightInverseVolumetric * _CloudNoiseScaleVolumetric.z);
static const float volumetricPositionCurlIntensity = (_CloudHeightVolumetric * _CloudNoiseScaleVolumetric.w) / _CloudNoiseScaleVolumetric.z;
static const float3 volumetricPlanetCenter = float3(WEATHER_MAKER_CLOUD_CAMERA_POS.x, _CloudPlanetRadiusNegativeVolumetric, WEATHER_MAKER_CLOUD_CAMERA_POS.z); // TODO: Support true spherical worlds
static const float4 volumetricSphereSurface = float4(volumetricPlanetCenter, _CloudPlanetRadiusSquaredVolumetric);
static const float4 volumetricSphereInner = float4(volumetricPlanetCenter, _CloudPlanetStartSquaredVolumetric);
static const float4 volumetricSphereOutter = float4(volumetricPlanetCenter, _CloudPlanetEndSquaredVolumetric);
static const float3 volumetricCloudDownVector = normalize(volumetricPlanetCenter - WEATHER_MAKER_CLOUD_CAMERA_POS);
static const float3 volumetricCloudUpVector = normalize(WEATHER_MAKER_CLOUD_CAMERA_POS - volumetricPlanetCenter);
static const float cloudCameraIsInCloudLayer = (WEATHER_MAKER_CLOUD_CAMERA_POS.y >= _CloudStartVolumetric && WEATHER_MAKER_CLOUD_CAMERA_POS.y <= _CloudHeightVolumetric);
static const float volumetricMaxOpticalDistance = _CloudHeightVolumetric * _CloudOpticalDistanceMultiplierVolumetric;
static const float invVolumetricMaxOpticalDistance = 1.0 / volumetricMaxOpticalDistance;
static const float3 volumetricAnimationShape = (_CloudShapeAnimationVelocity * _WeatherMakerTime.y);
static const float3 volumetricAnimationDetail = (_CloudDetailAnimationVelocity * _WeatherMakerTime.y);
static const float volumetricCloudShadowSampleCountInv = 0.95 * (1.0 / float(max(1.0, float(_WeatherMakerCloudVolumetricShadowSampleCount))));
static const float volumetricCloudLightAbsorption = 2.0 * _CloudLightAbsorptionVolumetric;
static const float volumetricCloudMaxShadow = saturate((1.0 + _CloudShadowMapAdder) * _CloudShadowMapMultiplier);// *volumetricCloudNoiseShadowScalar);
static const float volumetricCloudMaxShadowInv = saturate(1.0 / max(0.0001, volumetricCloudMaxShadow));
static const float volumetricCloudShadowDetailIntensity = _WeatherMakerCloudShadowDetailIntensity * 5.0;
static const float volumetricCloudShadowDetailFalloff = _WeatherMakerCloudShadowDetailFalloff;
static const float3 volumetricCloudPlanePos = float3(WEATHER_MAKER_CLOUD_CAMERA_POS.x, _CloudHeightVolumetric * 0.5, WEATHER_MAKER_CLOUD_CAMERA_POS.z);
static const float3 volumetricCloudPlaneNormal = float3(0.0, 1.0, 0.0);
static const float volumetricCloudShadowScale = _ProjectionParams.w * 0.5;
static const float volumetricIsAboveClouds = WEATHER_MAKER_CLOUD_CAMERA_POS.y > _CloudEndVolumetric;
static const float volumetricIsAboveMiddleClouds = WEATHER_MAKER_CLOUD_CAMERA_POS.y > (_CloudStartVolumetric + _CloudEndVolumetric) * 0.5;
static const float volumetricIsBelowClouds = WEATHER_MAKER_CLOUD_CAMERA_POS.y < _CloudStartVolumetric;
static const float volumetricAboveClouds = saturate(1.0 - ((WEATHER_MAKER_CLOUD_CAMERA_POS.y - _CloudEndVolumetric) * _CloudStartInverseVolumetric));
static const float volumetricAboveCloudsSquared = volumetricAboveClouds * volumetricAboveClouds;
static const float volumetricBelowClouds = saturate(WEATHER_MAKER_CLOUD_CAMERA_POS.y * _CloudStartInverseVolumetric);
static const float volumetricBelowCloudsSquared = volumetricBelowClouds * volumetricBelowClouds;
static const float volumetricFromClouds = lerp(volumetricBelowClouds, volumetricAboveClouds, volumetricIsAboveClouds); // 1 means totally in the clouds, approaches 0 as movement of cloud height either above or below.
static const float volumetricFromCloudsSquared = volumetricFromClouds * volumetricFromClouds;
static const float volumetricCloudPlanetRadiusStart = _CloudPlanetRadiusVolumetric + _CloudStartVolumetric;
static const float volumetricCloudInvFade = min(0.01, (_ProjectionParams.w * 25.0));
static const float3 volumetricWindDir1 = _CloudVelocity * 0.1;
static const float3 volumetricWindDir2 = volumetricWindDir1 * 50.0 * _WeatherMakerWeatherMapTextureDynamic;
static const float volumetricCloudWeightsInv = 1.0 / (_CloudWeights.r + _CloudWeights.g + _CloudWeights.b + _CloudWeights.a);

// weather map
static const float cloudCoverageInfluence = _CloudCoverVolumetric * _CloudCoveragePower;
static const float cloudCoverageInfluence2 = (1.0 + (_CloudCoverageProfileInfluence * _CloudCoverVolumetric)) * _CloudCoverageMultiplier * min(1.0, _CloudCoverVolumetric * 2.0);
static const float3 cloudCoverageVelocity = (_CloudCoverageOffset + float3(_WeatherMakerWeatherMapSeed, _WeatherMakerWeatherMapSeed, _WeatherMakerWeatherMapSeed) + _CloudCoverageVelocity);
static const float3 cloudCoverageNegationVelocity = (_CloudCoverageNegationOffset + float3(_WeatherMakerWeatherMapSeed, _WeatherMakerWeatherMapSeed, _WeatherMakerWeatherMapSeed) + _CloudCoverageNegationVelocity);
static const bool cloudCoverageIsMin = (cloudCoverageInfluence < 0.01 && _CloudCoverageAdder <= 0.0);
static const bool cloudCoverageIsMax = (cloudCoverageInfluence > 0.999 && _CloudCoverageAdder >= 0.0);
static const float cloudCoverageTextureMultiplier = (_CloudCoverSecondaryVolumetric + _CloudCoverageTextureMultiplier);
static const float cloudCoverageTextureScale = (4.0 / max(_CloudCoverageTextureScale, _CloudCoverageFrequency));

static const float cloudDensityInfluence = _CloudDensityVolumetric * _CloudDensityPower;
static const float cloudDensityInfluence2 = (1.0 + (_CloudDensityProfileInfluence * _CloudDensityVolumetric)) * _CloudDensityMultiplier * _CloudDensityVolumetric;
static const float3 cloudDensityVelocity = (_CloudDensityOffset + float3(_WeatherMakerWeatherMapSeed, _WeatherMakerWeatherMapSeed, _WeatherMakerWeatherMapSeed) + _CloudDensityVelocity);
static const float3 cloudDensityNegationVelocity = (_CloudDensityNegationOffset + float3(_WeatherMakerWeatherMapSeed, _WeatherMakerWeatherMapSeed, _WeatherMakerWeatherMapSeed) + _CloudDensityNegationVelocity);
static const bool cloudDensityIsMin = (cloudDensityInfluence < 0.01 && _CloudDensityAdder <= 0.0);
static const bool cloudDensityIsMax = (cloudDensityInfluence > 0.999 && _CloudDensityAdder >= 0.0);
static const float cloudDensityTextureMultiplier = (_CloudDensitySecondaryVolumetric + _CloudDensityTextureMultiplier);
static const float cloudDensityTextureScale = (4.0 / max(_CloudDensityTextureScale, _CloudDensityFrequency));

static const float cloudTypeInfluence = _CloudTypeVolumetric * _CloudTypePower;
static const float cloudTypeInfluence2 = (1.0 + (_CloudTypeProfileInfluence * _CloudTypeVolumetric)) * _CloudTypeMultiplier * _CloudTypeVolumetric;
static const float3 cloudTypeVelocity = (_CloudTypeOffset + float3(_WeatherMakerWeatherMapSeed, _WeatherMakerWeatherMapSeed, _WeatherMakerWeatherMapSeed) + _CloudTypeVelocity);
static const float3 cloudTypeNegationVelocity = (_CloudTypeNegationOffset + float3(_WeatherMakerWeatherMapSeed, _WeatherMakerWeatherMapSeed, _WeatherMakerWeatherMapSeed) + _CloudTypeNegationVelocity);
static const bool cloudTypeIsMin = (cloudTypeInfluence < 0.01 && _CloudTypeAdder <= 0.0);
static const bool cloudTypeIsMax = (cloudTypeInfluence > 0.999 && _CloudTypeAdder >= 0.0);
static const float cloudTypeTextureMultiplier = (_CloudTypeSecondaryVolumetric + _CloudTypeTextureMultiplier);
static const float cloudTypeTextureScale = (4.0 / max(_CloudTypeTextureScale, _CloudTypeFrequency));

static const float3 weatherMapCameraPos = WEATHER_MAKER_CLOUD_CAMERA_POS * _WeatherMakerWeatherMapScale.z;

// reduce dir light sample for reflections and cubemaps
static const uint volumetricLightIterations = ceil(min(15.0, _CloudDirLightSampleCount) *
(
	WM_CAMERA_RENDER_MODE_NORMAL +
	lerp(0.0, 0.5, WM_CAMERA_RENDER_MODE_REFLECTION) +
	lerp(0.0, 0.5, WM_CAMERA_RENDER_MODE_CUBEMAP)
));
static const float invVolumetricLightIterations = 1.0 / max(1.0, float(volumetricLightIterations));

static const uint volumetricLightSubIterations = clamp(_CloudDirLightSubSampleCount *
(
	WM_CAMERA_RENDER_MODE_NORMAL +
	lerp(0.0, 0.5, WM_CAMERA_RENDER_MODE_REFLECTION) +
	lerp(0.0, 0.5, WM_CAMERA_RENDER_MODE_CUBEMAP)
), 1.0, 16.0);
static const float invVolumetricLightSubIterations = 1.0 / volumetricLightSubIterations;

// not doing point light samples currently
//static const uint volumetricLightIterationsNonDir = 3u;
//static const float invVolumetricLightIterationsNonDir = 0.9 * (1.0f / float(volumetricLightIterationsNonDir));

static const float volumetricDirLightStepSize = _CloudHeightVolumetric * _CloudLightStepMultiplierVolumetric * invVolumetricLightIterations;

// reduce sample count for reflections and cubemaps
static const float2 volumetricSampleCountRange = ceil(float2(_CloudNoiseSampleCountVolumetric.x, _CloudNoiseSampleCountVolumetric.y) *
(
	WM_CAMERA_RENDER_MODE_NORMAL +
	lerp(0.0, 0.4, WM_CAMERA_RENDER_MODE_REFLECTION) +
	lerp(0.0, 0.4, WM_CAMERA_RENDER_MODE_CUBEMAP)
));

// raise LOD for reflections and cubemaps
static const float2 volumetricLod = float2(_CloudNoiseLodVolumetric.x, _CloudNoiseLodVolumetric.y) +
(1.25 * WM_CAMERA_RENDER_MODE_REFLECTION) +
(1.5 * WM_CAMERA_RENDER_MODE_CUBEMAP);

static const float4 volumetricWeatherMapBlurOffsets = float4
(
	_WeatherMakerWeatherMapTexture_TexelSize.x * 0.4,
	_WeatherMakerWeatherMapTexture_TexelSize.x * 1.2,
	_WeatherMakerWeatherMapTexture_TexelSize.y * 0.4,
	_WeatherMakerWeatherMapTexture_TexelSize.y * 1.2
);

uint SetupCloudRaymarch(float3 worldSpaceCameraPos, float3 rayDir, float depth, float depth2,
	out float3 startPos, out float3 endPos, out float rayLength, out float distanceToSphere,
	out float3 startPos2, out float3 endPos2, out float rayLength2, out float distanceToSphere2)
{
	UNITY_BRANCH
	if (_CloudPlanetRadiusVolumetric > 0.0)
	{
		return SetupPlanetRaymarch(worldSpaceCameraPos, rayDir, depth, depth2, volumetricSphereSurface, volumetricSphereInner, volumetricSphereOutter,
			startPos, endPos, rayLength, distanceToSphere, startPos2, endPos2, rayLength2, distanceToSphere2);
	}
	else if (_WeatherMakerCloudVolumetricWeatherMapRemapBoxMin.w == 0.0)
	{
		return SetupPlanetRaymarchBox(worldSpaceCameraPos, rayDir, depth, float2(_CloudStartVolumetric, _CloudEndVolumetric),
			startPos, endPos, rayLength, distanceToSphere, startPos2, endPos2, rayLength2, distanceToSphere2);
	}
	else
	{
		// ray march through specified box using _WeatherMakerCloudVolumetricWeatherMapRemapBoxMin and _WeatherMakerCloudVolumetricWeatherMapRemapBoxMax
		return SetupPlanetRaymarchBoxArea(worldSpaceCameraPos, rayDir, depth, _WeatherMakerCloudVolumetricWeatherMapRemapBoxMin,
			_WeatherMakerCloudVolumetricWeatherMapRemapBoxMax, startPos, endPos, rayLength, distanceToSphere, startPos2, endPos2, rayLength2, distanceToSphere2);
	}
}

// sample a curve of 16 values using t, a value of 0 to 1
float SampleCurve(float t, const float curve[16])
{
	// get a value between 0 and 15 (not including 15)
	float f = t * 14.999;

	// fractional lerp to next value
	float ff = frac(f);

	// index into array
	uint idx = floor(f);

	// lerp from current index to next index, idx is always less than 15, last index in array
	return lerp(curve[idx], curve[idx + 1], ff);
}

#endif // __WEATHER_MAKER_CLOUD_VOLUMETRIC_UNIFORMS_SHADER__
