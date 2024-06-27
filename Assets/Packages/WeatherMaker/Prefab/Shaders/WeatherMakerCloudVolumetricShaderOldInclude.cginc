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

#ifndef __WEATHER_MAKER_CLOUD_VOLUMETRIC_SHADER__
#define __WEATHER_MAKER_CLOUD_VOLUMETRIC_SHADER__

float ComputeCloudShadowStrengthTexture(float3 worldPos, uint dirIndex, float existingShadow, bool sampleDetails);
float ComputeCloudShadowStrengthTextureLOD(float3 worldPos, uint dirIndex, float existingShadow, bool sampleDetails, float lod);
float ComputeCloudShadowStrength(float3 worldPos, uint dirIndex, float existingShadow, bool sampleDetails, float lodMultiplier);

#include "WeatherMakerCloudShaderInclude.cginc"
#include "WeatherMakerCloudNoiseShaderInclude.cginc"

// comment out to turn off volumetric clouds and remove all shader code for volumetric clouds
#define WEATHER_MAKER_ENABLE_VOLUMETRIC_CLOUDS

// 1 = normal lighting, 2 = heatmap (raymarch cost)
#define VOLUMETRIC_CLOUD_RENDER_MODE 1

// whether point lights should be enabled, comment out to disable
#define VOLUMETRIC_CLOUD_ENABLE_POINT_LIGHTS

// reduce sample count as optical depth increases
#define VOLUMETRIC_SAMPLE_COUNT_OPTICAL_DEPTH_REDUCER 3.0

// increase lod as optical depth increases
#define VOLUMETRIC_LOD_OPTICAL_DEPTH_MULTIPLIER 10.0

// uncomment to use linear instead of exponential ambient sampling
// #define VOLUMETRIC_CLOUD_AMBIENT_MODE_LINEAR

// control powder base value when powder formula does not apply
#define VOLUMETRIC_POWDER_BASE_VALUE 1.0

// before multiplying powder ray y, add this value to it
#define VOLUMETRIC_POWDER_RAY_Y_ADDER 0.1

// multiply ray y by this and multiply by powder value
#define VOLUMETRIC_POWDER_RAY_Y_MULTIPLIER 2.0

// max henyey greenstein value
#define VOLUMETRIC_MAX_HENYEY_GREENSTEIN 5.0

// multiply height by this before applying cloud detail
#define VOLUMETRIC_DETAIL_HEIGHT_MULTIPLIER 10.0

// optical depth in horizon fade goes to this power first
#define VOLUMETRIC_HORIZON_FADE_OPTICAL_DEPTH_POWER 1.4

// subtract from optical depth before applying horizon fade, helps prevent higher up clouds becoming slightly transparent
#define VOLUMETRIC_HORIZON_FADE_OPTICAL_DEPTH_SUBTRACTOR 0.05

// min horizon fade, 0 for complete fade
#define VOLUMETRIC_MIN_HORIZON_FADE 0.0

// dither horizon fade to reduce banding
#define VOLUMETRIC_HORIZON_FADE_DITHER 0.005

// amount to multiply optical depth by to increase sky ambient for clouds (clouds closer up have less sky ambient)
#define VOLUMETRIC_SKY_AMBIENT_OPTICAL_DEPTH_MULTIPLIER 10.0

// minimum value for sky ambient optical multiplier
#define VOLUMETRIC_SKY_AMBIENT_MIN_OPTICAL_DEPTH_MULTIPLIER 0.2

// min amount of distance for each ray march
#define VOLUMETRIC_MIN_STEP_LENGTH 64.0

// min ray length for volumetric ray march
#define VOLUMETRIC_MIN_RAY_LENGTH _CloudHeightVolumetric

// min noise value to process cloud
#define VOLUMETRIC_CLOUD_MIN_NOISE_VALUE 0.001

// minimum coverage to sample cloud, lower this value if you see missed / hard cloud edges or clouds blinking in and out when moving fast
#define VOLUMETRIC_CLOUD_MINIMUM_COVERAGE_FOR_CLOUD 0.1

// minimum influence cloud type has on sample density
#define VOLUMETRIC_CLOUD_MIN_CLOUD_TYPE_DENSITY_MULTIPLIER 0.25

// maximum influence cloud type has on sample density
#define VOLUMETRIC_CLOUD_MAX_CLOUD_TYPE_DENSITY_MULTIPLIER 1.0

// minimum influence cloud type has on curl effect
#define VOLUMETRIC_CLOUD_MIN_CLOUD_TYPE_CURL_MULTIPLIER 0.5

// maximum influence cloud type has on curl effect
#define VOLUMETRIC_CLOUD_MAX_CLOUD_TYPE_CURL_MULTIPLIER 4.0

// max ray march length for volumetric cloud shadows
#define VOLUMETRIC_CLOUD_SHADOW_MAX_RAY_LENGTH 8192.0

// change to 1 to enable volumetric cloud shadow fade
#define WEATHER_MAKER_ENABLE_SHADOW_FADE 0

// WARNING - THIS WILL BE VERY PERFORMANCE INTENSIVE AND CAN LOCK UP THE EDITOR
//#define VOLUMETRIC_CLOUD_REAL_TIME_NOISE

#if defined(WEATHER_MAKER_ENABLE_VOLUMETRIC_CLOUDS)

uniform sampler3D _CloudNoiseShapeVolumetric;
uniform sampler3D _CloudNoiseDetailVolumetric;
uniform sampler2D _CloudNoiseCurlVolumetric;
uniform sampler2D _WeatherMakerWeatherMapTexture;
uniform float4 _WeatherMakerWeatherMapTexture_TexelSize;
uniform sampler2D _WeatherMakerCloudShadowTexture;
uniform float4 _WeatherMakerCloudShadowTexture_TexelSize;
uniform uint2 _CloudNoiseSampleCountVolumetric;
uniform float2 _CloudNoiseLodVolumetric;
uniform float4 _CloudNoiseScaleVolumetric; // shape scale, details scale, curl scale, curl multiplier
uniform float _CloudNoiseScalarVolumetric;
uniform float _CloudNoiseDetailPowerVolumetric;
uniform fixed4 _CloudColorVolumetric;
uniform fixed4 _CloudDirColorVolumetric;
uniform fixed4 _CloudEmissionColorVolumetric;

uniform float3 _CloudShapeAnimationVelocity;
uniform float3 _CloudDetailAnimationVelocity;

uniform float _CloudAmbientGroundIntensityVolumetric;
uniform float _CloudAmbientSkyIntensityVolumetric;
uniform float2 _CloudBackgroundSkyIntensityVolumetric;
uniform float _CloudAmbientSkyHeightMultiplierVolumetric;
uniform float _CloudAmbientGroundHeightMultiplierVolumetric;
uniform float _CloudLightAbsorptionVolumetric;
uniform float _CloudDirLightIndirectMultiplierVolumetric;
uniform float _CloudShapeNoiseMinVolumetric;
uniform float _CloudShapeNoiseMaxVolumetric;
uniform float _CloudPowderMultiplierVolumetric;
uniform float _CloudBottomFadeVolumetric;

//uniform float _CloudMaxRayLengthMultiplierVolumetric;
#define _CloudMaxRayLengthMultiplierVolumetric 2

uniform float _CloudRaymarchMultiplierVolumetric;
uniform float _CloudRayDitherVolumetric;
uniform float _CloudOpticalDistanceMultiplierVolumetric;
uniform float _CloudHorizonFadeMultiplierVolumetric;
uniform float4 _CloudRayMarchParameters;

uniform float _CloudCoverVolumetric;
uniform float _CloudCoverSecondaryVolumetric;
uniform float _CloudTypeVolumetric;
uniform float _CloudTypeSecondaryVolumetric;
uniform float _CloudDensityVolumetric;

//uniform float _CloudHeightNoisePowerVolumetric;
#define _CloudHeightNoisePowerVolumetric 16.0

// cloud layer start from sea level
uniform float _CloudStartVolumetric;
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
uniform float _CloudEndSquaredVolumetric;
uniform float _CloudEndSquaredInverseVolumetric;
uniform float _CloudPlanetEndVolumetric; // cloud end + planet radius
uniform float _CloudPlanetEndSquaredVolumetric; // cloud end + planet radius, squared

uniform float _CloudPlanetRadiusVolumetric;
uniform float _CloudPlanetRadiusNegativeVolumetric;
uniform float _CloudPlanetRadiusSquaredVolumetric;

uniform float4 _WeatherMakerCloudVolumetricWeatherMapRemapBoxMin;
uniform float4 _WeatherMakerCloudVolumetricWeatherMapRemapBoxMax;

//uniform float4 _CloudHenyeyGreensteinPhaseVolumetric;
#define _CloudHenyeyGreensteinPhaseVolumetric float4(0.7, -0.4, 0.1, 1.0)

uniform float _CloudShadowThresholdVolumetric;
uniform float _CloudShadowPowerVolumetric;
uniform float _CloudShadowMultiplierVolumetric;
uniform float _CloudRayOffsetVolumetric;

//uniform float _CloudMinRayYVolumetric;
#define _CloudMinRayYVolumetric -1.0

uniform float _CloudLightStepMultiplierVolumetric;
uniform uint _CloudDirLightSampleCount;
uniform float _CloudDirLightLod;
uniform uint _WeatherMakerCloudVolumetricShadowSampleCount;

uniform float4 _WeatherMakerWeatherMapScale; // x = noise scale x, y = noise scale y, z = world to uv, w = world units per pixel
uniform float _CloudShadowMapAdder;
uniform float _CloudShadowMapMultiplier;
uniform float _CloudShadowMapPower;
uniform float _WeatherMakerCloudVolumetricShadowDither;
uniform sampler2D _WeatherMakerCloudShadowDetailTexture;
uniform float _WeatherMakerCloudShadowDetailScale;
uniform float _WeatherMakerCloudShadowDetailIntensity;
uniform float _WeatherMakerCloudShadowDetailFalloff;
uniform float _WeatherMakerCloudShadowSampleShadowMap;

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

// ray march optimizations

//uniform uint _CloudRaymarchSkipThreshold; // 3
#define _CloudRaymarchSkipThreshold 32

//uniform float _CloudRaymarchMaybeInCloudStepMultiplier; // 1.0
#define _CloudRaymarchMaybeInCloudStepMultiplier 0.5

//uniform float _CloudRaymarchInCloudStepMultiplier; // 1.0
#define _CloudRaymarchInCloudStepMultiplier 0.2

//uniform float _CloudRaymarchSkipMultiplier; // 1.1
#define _CloudRaymarchSkipMultiplier 1.02

//uniform uint _CloudRaymarchSkipMultiplierMaxCount; // 16
#define _CloudRaymarchSkipMultiplierMaxCount 32

uniform uint _CloudRaymarchSampleDetailsForDirLight;

// weather map
uniform float _WeatherMakerWeatherMapSeed;

uniform float _CloudCoverageFrequency;
uniform float2 _CloudCoverageRotation;
uniform float3 _CloudCoverageVelocity; // needs to be prescaled by _WeatherMakerWeatherMapScale.z
uniform float3 _CloudCoverageOffset;
uniform float _CloudCoverageMultiplier;
uniform float _CloudCoverageAdder;
uniform float _CloudCoveragePower;
uniform float _CloudCoverageProfileInfluence;
uniform sampler2D _CloudCoverageTexture;
uniform float _CloudCoverageTextureMultiplier;
uniform float _CloudCoverageTextureScale;

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

float GetCloudHeightFractionForPoint(float3 worldPos);
float4 CloudVolumetricSampleWeather(float3 pos, float lod);
float SampleCloudDensity(float3 marchPos, float4 weatherData, float heightFrac, float lod, bool sampleDetails);

// random vectors on the unit sphere
//static const float3 volumetricConeRandomVectors[6] =
//{
	//float3(0.38051305f,  0.92453449f, -0.02111345f),
	//float3(-0.50625799f, -0.03590792f, -0.86163418f),
	//float3(-0.32509218f, -0.94557439f,  0.01428793f),
	//float3(0.09026238f, -0.27376545f,  0.95755165f),
	//float3(0.28128598f,  0.42443639f, -0.86065785f),
	//float3(-0.16852403f,  0.14748697f,  0.97460106f),
//};


//static const float4 STRATUS_GRADIENT = float4(0.0, 0.01912, 0.12752, 0.21854);
//static const float4 STRATOCUMULUS_GRADIENT = float4(0.0, 0.03021, 0.32742, 0.61758);
//static const float4 CUMULUS_GRADIENT = float4(0.0, 0.0625, 0.78, 0.95);

//static const float4 STRATUS_GRADIENT = float4(0.02f, 0.05f, 0.09f, 0.11f);
//static const float4 STRATOCUMULUS_GRADIENT = float4(0.02f, 0.2f, 0.48f, 0.625f);
//static const float4 CUMULUS_GRADIENT = float4(0.01f, 0.0625f, 0.78f, 1.0f);

static const float volumetricPositionShapeScale = _CloudHeightInverseVolumetric * _CloudNoiseScaleVolumetric.x;
static const float volumetricPositionDetailScale = volumetricPositionShapeScale * _CloudNoiseScaleVolumetric.y;
static const float volumetricPositionCurlScale = volumetricPositionShapeScale * _CloudNoiseScaleVolumetric.z;
static const float volumetricPositionCurlIntensity = _CloudNoiseScaleVolumetric.w;
static const float3 volumetricPlanetCenter = float3(WEATHER_MAKER_CAMERA_POS.x, _CloudPlanetRadiusNegativeVolumetric, WEATHER_MAKER_CAMERA_POS.z); // TODO: Support true spherical worlds
static const float4 volumetricSphereSurface = float4(volumetricPlanetCenter, _CloudPlanetRadiusSquaredVolumetric);
static const float4 volumetricSphereInner = float4(volumetricPlanetCenter, _CloudPlanetStartSquaredVolumetric);
static const float4 volumetricSphereOutter = float4(volumetricPlanetCenter, _CloudPlanetEndSquaredVolumetric);
static const fixed3 volumetricCloudAmbientColorGround = (_WeatherMakerAmbientLightColorGround * _CloudAmbientGroundIntensityVolumetric);
static const fixed3 volumetricCloudAmbientColorSky = (_WeatherMakerAmbientLightColorSky * _CloudAmbientSkyIntensityVolumetric);
static const float3 volumetricCloudDownVector = normalize(volumetricPlanetCenter - WEATHER_MAKER_CAMERA_POS);
static const float3 volumetricCloudUpVector = normalize(WEATHER_MAKER_CAMERA_POS - volumetricPlanetCenter);
static const float volumetricCloudNoiseMultiplier = 0.25 * _CloudDensityVolumetric;
static const float cloudCameraIsInCloudLayer = (WEATHER_MAKER_CAMERA_POS.y >= _CloudStartVolumetric && WEATHER_MAKER_CAMERA_POS.y <= _CloudHeightVolumetric);
static const float volumetricCloudMaxRayLength = _CloudHeightVolumetric * _CloudMaxRayLengthMultiplierVolumetric;
static const float invVolumetricCloudMaxRayLength = 1.0 / volumetricCloudMaxRayLength;
static const float volumetricMaxOpticalDistance = _CloudHeightVolumetric * _CloudOpticalDistanceMultiplierVolumetric;
static const float invVolumetricMaxOpticalDistance = 1.0 / volumetricMaxOpticalDistance;
static const float3 volumetricAnimationShape = (_CloudShapeAnimationVelocity * _WeatherMakerTime.y);
static const float3 volumetricAnimationDetail = (_CloudDetailAnimationVelocity * _WeatherMakerTime.y);
static const float volumetricCameraHeightFrac = GetCloudHeightFractionForPoint(WEATHER_MAKER_CAMERA_POS);
static const float4 volumetricCameraWeatherData = CloudVolumetricSampleWeather(WEATHER_MAKER_CAMERA_POS, 0.0);
static const float volumetricCameraCloudDensity = SampleCloudDensity(WEATHER_MAKER_CAMERA_POS, volumetricCameraWeatherData, volumetricCameraHeightFrac, 0.0, true);
static const float volumetricMinRayY = lerp(_CloudMinRayYVolumetric, -1.0, WEATHER_MAKER_CAMERA_POS.y >= _CloudStartVolumetric);
static const float volumetricCloudMaybeInCloudStepMultiplier = lerp(1.0, _CloudRaymarchMaybeInCloudStepMultiplier, _CloudRaymarchSkipThreshold != 0);
static const float volumetricCloudInCloudStepMultiplier = lerp(1.0, _CloudRaymarchInCloudStepMultiplier, _CloudRaymarchSkipThreshold != 0);
static const float volumetricCloudNoiseScalar = _CloudDensityVolumetric * _CloudNoiseScalarVolumetric;
static const float volumetricCloudShadowSampleCountInv = 0.95 * (1.0 / float(max(1.0, float(_WeatherMakerCloudVolumetricShadowSampleCount))));
//static const float volumetricCloudNoiseShadowScalar = clamp(volumetricCloudNoiseScalar, 0.5, 1.0);
static const float volumetricCloudLightAbsorption = 2.0 * _CloudLightAbsorptionVolumetric;
static const float volumetricCloudMaxShadow = saturate((1.0 + _CloudShadowMapAdder) * _CloudShadowMapMultiplier);// *volumetricCloudNoiseShadowScalar);
static const float volumetricCloudMaxShadowInv = saturate(1.0 / max(0.0001, volumetricCloudMaxShadow));
static const float volumetricCloudShadowDetailIntensity = _WeatherMakerCloudShadowDetailIntensity * 5.0;
static const float volumetricCloudShadowDetailFalloff = _WeatherMakerCloudShadowDetailFalloff;
static const float3 volumetricCloudPlanePos = float3(WEATHER_MAKER_CAMERA_POS.x, _CloudHeightVolumetric * 0.99, WEATHER_MAKER_CAMERA_POS.z);
static const float3 volumetricCloudPlaneNormal = float3(0.0, 1.0, 0.0);
static const float volumetricCloudShadowScale = _ProjectionParams.w * 0.5;

// weather map
static const float cloudDensity = min(1.0, _CloudDensityVolumetric);
static const float cloudCoverageInfluence = _CloudCoverageProfileInfluence * _CloudCoverVolumetric * cloudDensity;
static const float cloudCoverageInfluence2 = (1.0 + (_CloudCoverageProfileInfluence * _CloudCoverVolumetric * cloudDensity)) * _CloudCoverageMultiplier * min(1.0, _CloudCoverVolumetric * cloudDensity * 2.0);
static const float3 cloudCoverageVelocity = (_CloudCoverageOffset + float3(_WeatherMakerWeatherMapSeed, _WeatherMakerWeatherMapSeed, _WeatherMakerWeatherMapSeed) + _CloudCoverageVelocity);
static const bool cloudCoverageIsMin = (cloudCoverageInfluence < 0.01 && _CloudCoverageAdder <= 0.0);
static const bool cloudCoverageIsMax = (cloudCoverageInfluence > 0.999 && _CloudCoverageAdder >= 0.0);
static const float cloudCoverageTextureMultiplier = (_CloudCoverSecondaryVolumetric + _CloudCoverageTextureMultiplier);
static const float cloudCoverageTextureScale = (4.0 / max(_CloudCoverageTextureScale, _CloudCoverageFrequency));

static const float cloudTypeInfluence = _CloudTypeProfileInfluence * _CloudTypeVolumetric;
static const float cloudTypeInfluence2 = (1.0 + (_CloudTypeProfileInfluence * _CloudTypeVolumetric)) * _CloudTypeMultiplier * _CloudTypeVolumetric;
static const float3 cloudTypeVelocity = (_CloudTypeOffset + float3(_WeatherMakerWeatherMapSeed, _WeatherMakerWeatherMapSeed, _WeatherMakerWeatherMapSeed) + _CloudTypeVelocity);
static const bool cloudTypeIsMin = (cloudTypeInfluence < 0.01 && _CloudTypeAdder <= 0.0);
static const bool cloudTypeIsMax = (cloudTypeInfluence > 0.999 && _CloudTypeAdder >= 0.0);
static const float cloudTypeTextureMultiplier = (_CloudTypeSecondaryVolumetric + _CloudTypeTextureMultiplier);
static const float cloudTypeTextureScale = (4.0 / max(_CloudTypeTextureScale, _CloudTypeFrequency));

static const float3 weatherMapCameraPos = WEATHER_MAKER_CAMERA_POS * _WeatherMakerWeatherMapScale.z;

// reduce dir light sample for reflections and cubemaps
static const uint volumetricLightIterations = ceil(min(15.0, _CloudDirLightSampleCount) *
(
	WM_CAMERA_RENDER_MODE_NORMAL +
	lerp(0.0, 0.5, WM_CAMERA_RENDER_MODE_REFLECTION) +
	lerp(0.0, 0.5, WM_CAMERA_RENDER_MODE_CUBEMAP)
));
static const float invVolumetricLightIterations = 1.0f / max(1.0, float(volumetricLightIterations));
static const bool volumetricLightSampleDistant = (_CloudDirLightSampleCount >= 6);

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

// per fragment state
static float4 _dirLightPrecomputedValues[MAX_LIGHT_COUNT]; // x = eye dot, y = y light intensity modifier, z = light intensity squared, w = 0

// volumetric clouds --------------------------------------------------------------------------------
inline float2 AdjustWeatherMapSamplePosUnbounded(float3 worldPos)
{
	float2 pos = worldPos.xz;
	pos -= WEATHER_MAKER_CAMERA_POS.xz;
	pos *= _WeatherMakerWeatherMapScale.z;
	pos += 0.5; // 0.5, 0.5 is center of weather map at world pos xz of 0,0, as camera moves they will tile through the weather map
	//float xValue = (abs(0.5 - pos.x) * 2.0);
	//float yValue = (abs(0.5 - pos.y) * 2.0);
	//pos.x = lerp(pos.x, 0.0, xValue * xValue);
	//pos.y = lerp(pos.y, 0.0, yValue * yValue);
	return pos;
}

inline float2 AdjustWeatherMapSamplePosBounded(float3 worldPos)
{
	float2 volumetricCloudBoxInverseScale = 1.0 / (_WeatherMakerCloudVolumetricWeatherMapRemapBoxMax.xz - _WeatherMakerCloudVolumetricWeatherMapRemapBoxMin.xz);
	float2 uv = worldPos.xz - _WeatherMakerCloudVolumetricWeatherMapRemapBoxMin.xz;
	uv *= volumetricCloudBoxInverseScale;
	return uv;
}

inline float4 CloudVolumetricSampleWeather(float3 pos, float lod)
{
	UNITY_BRANCH
	if (_WeatherMakerCloudVolumetricWeatherMapRemapBoxMin.w == 0.0)
	{
		float2 uv = AdjustWeatherMapSamplePosUnbounded(pos);
		return tex2Dlod(_WeatherMakerWeatherMapTexture, float4(uv, 0.0, lod));
	}
	else
	{
		float2 uv = AdjustWeatherMapSamplePosBounded(pos);
		if (uv.x < 0.0 || uv.y < 0.0 || uv.x > 1.0 || uv.y > 1.0)
		{
			return float4Zero;
		}
		else
		{
			return tex2Dlod(_WeatherMakerWeatherMapTexture, float4(uv, 0.0, lod));
		}
	}
}

// sample weather using 17 tap
inline float4 CloudVolumetricSampleWeatherGaussian17(float3 pos, float lod)
{
	static const float4 offsets = float4
	(
		_WeatherMakerWeatherMapTexture_TexelSize.x * 0.4,
		_WeatherMakerWeatherMapTexture_TexelSize.x * 1.2,
		_WeatherMakerWeatherMapTexture_TexelSize.y * 0.4,
		_WeatherMakerWeatherMapTexture_TexelSize.y * 1.2
	);

	UNITY_BRANCH
	if (_WeatherMakerCloudVolumetricWeatherMapRemapBoxMin.w == 0.0)
	{
		float2 uv = AdjustWeatherMapSamplePosUnbounded(pos);
		return GaussianBlur_Texture2D_17Tap(_WeatherMakerWeatherMapTexture, uv, offsets, 0.1, 0.0);
	}
	else
	{
		float2 uv = AdjustWeatherMapSamplePosBounded(pos);
		if (uv.x < 0.0 || uv.y < 0.0 || uv.x > 1.0 || uv.y > 1.0)
		{
			return float4Zero;
		}
		else
		{
			return GaussianBlur_Texture2D_17Tap(_WeatherMakerWeatherMapTexture, uv, offsets, 0.1, 0.0);
		}
	}
}

inline float CloudVolumetricGetCoverage(float4 weatherData)
{
	return weatherData.r;
}

inline float CloudVolumetricGetCloudType(float4 weatherData)
{
	// weather b channel tells the cloud type 0.0 = stratus, 0.5 = stratocumulus, 1.0 = cumulus
	return weatherData.b;
}

inline float CloudVolumetricGetDistance(float4 weatherData)
{
	// weather a channel has sdf value, nearest weathermap pixel with a value
	return weatherData.a;
}

inline float CloudVolumetricBeerLambert(float density)
{
	// TODO: Multiply by precipitation or density for rain/snow clouds
	return exp2(-density);
}

float CloudVolumetricPowder(float density, float lightDotEye, float lightIntensity, float3 lightDir, float3 rayDir)
{
	UNITY_BRANCH
	if (_CloudPowderMultiplierVolumetric <= 0.0)
	{
		return 1.0;
	}
	else
	{
		static const float oneMinusMultiplier = max(0.0, 1.0 - _CloudPowderMultiplierVolumetric);

		// base powder term
		float powder = 1.0 - exp2(-2.0 * density);

		// increase powder intensity
		powder = saturate(powder * _CloudPowderMultiplierVolumetric);

		// reduce powder as angle from sun decreases, reduce powder as light is at horizon (light is passing through more air)
		// in dim light or when light as horizon, powder effect looks bad
		float powderHorizonLerp = saturate((lightDir.y + VOLUMETRIC_POWDER_RAY_Y_ADDER) * VOLUMETRIC_POWDER_RAY_Y_MULTIPLIER);
		powder = lerp(VOLUMETRIC_POWDER_BASE_VALUE, powder, (1.0 - (lightDotEye * lightDotEye)) * powderHorizonLerp) * powderHorizonLerp;

		// smoothly lerp to 1 if powder is less than 1
		return lerp(powder, 1.0, oneMinusMultiplier);
	}
}

float CloudVolumetricHenyeyGreensteinVolumetric(float lightDotEye, float lightIntensity, float3 lightDir)
{
	// f(x) = (1 - g)^2 / (4PI * (1 + g^2 - 2g*cos(x))^[3/2])
	// _CloudHenyeyGreensteinPhase.x = forward, _CloudHenyeyGreensteinPhase.y = back
	static const float g = _CloudHenyeyGreensteinPhaseVolumetric.x;
	static const float gSquared = g * g;
	static const float oneMinusGSquared = (1.0 - gSquared);
	static const float onePlusGSquared = 1.0 + gSquared;
	static const float twoGSquared = 2.0 * g;
	float falloff = onePlusGSquared - (twoGSquared * lightDotEye);
	float forward = (oneMinusGSquared / (pow(falloff, 1.5)));

	static const float g2 = _CloudHenyeyGreensteinPhaseVolumetric.y;
	static const float gSquared2 = g2 * g2;
	static const float oneMinusGSquared2 = (1.0 - gSquared2);
	static const float onePlusGSquared2 = 1.0 + gSquared2;
	static const float twoGSquared2 = 2.0 * g2;
	float falloff2 = onePlusGSquared2 - (twoGSquared2 * lightDotEye);
	float back = oneMinusGSquared2 / (pow(falloff2, 1.5));

	return min(VOLUMETRIC_MAX_HENYEY_GREENSTEIN, ((forward * _CloudHenyeyGreensteinPhaseVolumetric.z) + (back * _CloudHenyeyGreensteinPhaseVolumetric.w)));

	/*
	float g = _CloudHenyeyGreensteinPhase.x;
	float g2 = g * g;
	float h = _CloudHenyeyGreensteinPhase.z * ((1.0f - g2) / pow((1.0f + g2 - 2.0f * g * lightDotEye), 1.5f));
	g = _CloudHenyeyGreensteinPhase.y;
	g2 = g * g;
	h += (_CloudHenyeyGreensteinPhase.w * ((1.0f - g2) / pow((1.0f + g2 - 2.0f * g * lightDotEye), 1.5f)));
	return h;
	*/
}

float3 CloudVolumetricLightEnergy(float lightDotEye, float densitySample, float eyeDensity, float densityToLight, float lightIntensity, float3 lightDir, float3 rayDir)
{
	// With E as light energy, d as the density sampled for lighting, p as the absorption multiplier for rain, g as our eccentricity in light direction, and θ as the angle between the view and light rays,
	// calculate lighting - E = 2.0 * e−dp * (1 − e−2d) * (1/4π) * (1 − g2 1 + g2 − 2g cos(θ)3/2).
	float beerLambert = CloudVolumetricBeerLambert(densityToLight);
	float powder = CloudVolumetricPowder(densitySample, lightDotEye, lightIntensity, lightDir, rayDir);
	float henyeyGreenstein = CloudVolumetricHenyeyGreensteinVolumetric(lightDotEye, lightIntensity, lightDir);
	return float3(beerLambert, powder, henyeyGreenstein);
}

inline float GetCloudHeightFractionForPoint(float3 worldPos)
{
	UNITY_BRANCH
	if (_CloudPlanetRadiusVolumetric > 0.0)
	{
		static const float cloudPlanetRadiusStart = _CloudPlanetRadiusVolumetric + _CloudStartVolumetric;
		return _CloudHeightInverseVolumetric * (distance(worldPos, volumetricPlanetCenter) - cloudPlanetRadiusStart);
	}
	else if (_WeatherMakerCloudVolumetricWeatherMapRemapBoxMin.w == 0.0)
	{
		return ((worldPos.y - _CloudStartVolumetric) * _CloudHeightInverseVolumetric);
	}
	else
	{
		float heightPos = worldPos.y - _WeatherMakerCloudVolumetricWeatherMapRemapBoxMin.y;
		return heightPos * _WeatherMakerCloudVolumetricWeatherMapRemapBoxMax.w;
	}
}

float SmoothStepGradient(float zeroToOne, float4 gradient)
{
	return smoothstep(gradient.x, gradient.y, zeroToOne) - smoothstep(gradient.z, gradient.w, zeroToOne);
}

float GetDensityHeightGradientForHeight(float heightFrac, float cloudType)
{
	// 0 = fully stratus, 0.5 = fully stratocumulus, 1 = fully cumulus
	float stratus = 1.0f - saturate(cloudType * 2.0f);
	float stratoCumulus = 1.0f - abs(cloudType - 0.5f) * 2.0f;
	float cumulus = saturate((cloudType - 0.5f) * 2.0f);
	float4 cloudGradient = (_CloudGradientStratus * stratus) + (_CloudGradientStratoCumulus * stratoCumulus) + (_CloudGradientCumulus * cumulus);
	return SmoothStepGradient(heightFrac, cloudGradient);
}

float SampleCloudDensityDetails(float noise, float4 weatherData, float3 marchPos, float heightFrac, float lod)
{
	// apply details if needed
	UNITY_BRANCH
	if (volumetricPositionDetailScale > 0.0 && noise > VOLUMETRIC_CLOUD_MIN_NOISE_VALUE)
	{
		float4 noisePos = float4((marchPos + volumetricAnimationDetail) * volumetricPositionDetailScale, lod);

		UNITY_BRANCH
		if (volumetricPositionCurlScale > 0.0)
		{
			// modify detail pos using curl lookup
			float4 curlPos = float4((noisePos.xz) * volumetricPositionCurlScale, 0.0, lod);
			float3 curl = (tex2Dlod(_CloudNoiseCurlVolumetric, curlPos).rgb * 2.0) - 1.0; // curl tex is 0-1, map to -1,1
			curl *= volumetricPositionCurlIntensity * (1.0 - heightFrac);
			noisePos.xyz += (curl * lerp(VOLUMETRIC_CLOUD_MIN_CLOUD_TYPE_CURL_MULTIPLIER, VOLUMETRIC_CLOUD_MAX_CLOUD_TYPE_CURL_MULTIPLIER, (1.0 - CloudVolumetricGetCloudType(weatherData))));
		}

		// erode details away from noise value - single alpha value, gpu pro 7 way
		float detail = tex3Dlod(_CloudNoiseDetailVolumetric, noisePos).a;
		float detailModifier = lerp(detail, 1.0f - detail, saturate(heightFrac * VOLUMETRIC_DETAIL_HEIGHT_MULTIPLIER));
		noise = saturate(Remap(noise, detailModifier * _CloudNoiseDetailPowerVolumetric, 1.0, 0.0, 1.0));

		// cloud density generally reduces at lower heights, smoothstep is better than lerp here
		noise *= smoothstep(0.0, _CloudBottomFadeVolumetric, heightFrac);
		noise = min(1.0, noise * _CloudNoiseScalarVolumetric * lerp(VOLUMETRIC_CLOUD_MIN_CLOUD_TYPE_DENSITY_MULTIPLIER, VOLUMETRIC_CLOUD_MAX_CLOUD_TYPE_DENSITY_MULTIPLIER, CloudVolumetricGetCloudType(weatherData)));

		/*
		// erode away cloud using detail texture and height gradient, not much different from gpu pro 7 way and more costly so not using
		float3 erosion = 1.0 - tex3Dlod(_CloudNoiseDetailVolumetric, noisePos);

		// erode differently at different heights and cloud types
		erosion *= heightGradient;

		// compute erosion noise value
		float erosionNoise = (erosion.r + erosion.g + erosion.b) * 0.3334; // average erosion values
		erosionNoise *= smoothstep(1.0, 0.0, noise) * 0.5; // erode less with thicker clouds
		noise = saturate(noise - erosionNoise);
		*/
	}

	return noise;
}

inline float CloudNoiseSampleToCloudNoise(fixed4 noiseSample, float heightFrac, float4 weatherData, float multiplier)
{
	float coverage = CloudVolumetricGetCoverage(weatherData);

	// create height gradient for the 3 samples of worley noise
	float3 heightGradient = float3(SmoothStepGradient(heightFrac, _CloudGradientStratus),
		SmoothStepGradient(heightFrac, _CloudGradientStratoCumulus), SmoothStepGradient(heightFrac, _CloudGradientCumulus));

	// multiply worley noise samples by height gradients
	noiseSample.gba *= heightGradient; // dont modify perlin / worley in this step

	// combine all into final noise
	float noise = (noiseSample.r + noiseSample.g + noiseSample.b + noiseSample.a) * volumetricCloudNoiseMultiplier * multiplier;
	noise = pow(noise, min(1.0, _CloudHeightNoisePowerVolumetric * heightFrac));

	// smoothstep noise to a range, helps reduce clutter / noise of the clouds
	//noise = smoothstep(_CloudShapeNoiseMinVolumetric, _CloudShapeNoiseMaxVolumetric, noise);
	noise = smoothstep(0.1, 0.6, noise);

	// remap function for noise against coverage, see gpu gems pro 7
	noise = saturate(noise - (1.0 - coverage)) * coverage;

	return noise;
}

float SampleCloudDensity(float3 marchPos, float4 weatherData, float heightFrac, float lod, bool sampleDetails)
{
	float noise = 0.0;
	float heightGradientSingle;
	float cloudType = CloudVolumetricGetCloudType(weatherData);

	// avoid sampling out of bounds of the cloud layer or no coverage
	UNITY_BRANCH
	if (volumetricPositionShapeScale > 0.0 && heightFrac >= 0.0 && heightFrac <= 1.0 &&
		(heightGradientSingle = GetDensityHeightGradientForHeight(heightFrac, cloudType)) > VOLUMETRIC_CLOUD_MIN_NOISE_VALUE)
	{
		float4 noisePos = float4((marchPos + volumetricAnimationShape) * volumetricPositionShapeScale, lod);

		// https://github.com/greje656/clouds
		// smoothly combine all three cloud layer gradients against the noise gba channels using just the height in the cloud layer
		// this produces nicer looking results with whispy clouds at lower height and a variety of puffy clouds higher up
		// this looks nicer than the gpu gems pro 7 remap and fbm style sampling in my opinion

#if defined(VOLUMETRIC_CLOUD_REAL_TIME_NOISE)

		float noiseSamples[4];

		UNITY_LOOP
		for (uint idx = 0; idx < 4; idx++)
		{
			noiseSamples[idx] = CloudNoise(noisePos, _RealTimeCloudNoiseShapeTypes[idx], _RealTimeCloudNoiseShapePerlinParam1[idx],
				_RealTimeCloudNoiseShapePerlinParam2[idx], _RealTimeCloudNoiseShapeWorleyParam1[idx], _RealTimeCloudNoiseShapeWorleyParam2[idx]);
		}

		float4 noiseSample = float4(noiseSamples[0], noiseSamples[1], noiseSamples[2], noiseSamples[3]);

#else

		float4 noiseSample = tex3Dlod(_CloudNoiseShapeVolumetric, noisePos); // (RandomFloat(marchPos) * 0.5);

#endif

		noise = CloudNoiseSampleToCloudNoise(noiseSample, heightFrac, weatherData, heightGradientSingle);

		// apply details if needed, should not even need to branch here as the parameter is a constant for any calls
		if (sampleDetails)
		{
			noise = SampleCloudDensityDetails(noise, weatherData, marchPos, heightFrac, lod);
		}
	}

	return noise;
}

fixed3 SampleDirLightSources(float3 marchPos, float3 rayDir, float startHeightFrac, float cloudSample, float eyeDensity, float lod)
{
	fixed3 lightTotal = fixed3Zero;

	UNITY_BRANCH
	if (_CloudDirLightMultiplierVolumetric <= 0.0)
	{
		return lightTotal;
	}

	startHeightFrac = max(0.3, startHeightFrac);
	lod += _CloudDirLightLod;

	// take advantage of the fact that lights are sorted by perspective/ortho and then by intensity
	UNITY_LOOP
	for (uint lightIndex = 0; lightIndex < uint(_WeatherMakerDirLightCount) && _WeatherMakerDirLightVar1[lightIndex].y == 0.0 && _WeatherMakerDirLightColor[lightIndex].a > 0.0; lightIndex++)
	{
		float3 lightDir = _WeatherMakerDirLightPosition[lightIndex].xyz;

		// skip the light if it is below the horizon
		// TODO: Support true spherical worlds with a different lightDir.y check
		UNITY_BRANCH
		if (lightDir.y >= -0.15)
		{
			// make sure we don't walk down, this causes artifacts at horizon
			lightDir.y = max(0.1, lightDir.y);
			float3 lightStep = (lightDir.xyz * volumetricDirLightStepSize);
			fixed4 lightColor = _WeatherMakerDirLightColor[lightIndex];

			//causes flicker, figure out why...
			//float randomDither = 1.0 + (_CloudRayDitherVolumetric * RandomFloat(lightStep));
			//lightStep *= randomDither; // dither march dir slightly to avoid banding

			float heightFrac;
			float4 weatherData;
			float coneRadiusStep = lightStep;
			float coneRadius = lightStep;
			float3 samplePos;
			float3 energy;
			float densityToLight = 0.0;
			float3 pos = marchPos + lightStep;

			UNITY_LOOP
			for (uint i = 0; i < volumetricLightIterations; i++)
			{
				// sample in the cone, take the march pos and perturb by random vector and cone radius
				samplePos = pos + (weatherMakerRandomCone[i] * coneRadius);

				// ensure we don't march out of the cloud layer
				samplePos.y = max(marchPos.y, samplePos.y);
				//samplePos.y = clamp(samplePos.y, _CloudStartVolumetric, _CloudEndVolumetric);

				// lookup position for cloud density
				weatherData = CloudVolumetricSampleWeather(samplePos, lod);

				UNITY_BRANCH
				if (CloudVolumetricGetCoverage(weatherData) > VOLUMETRIC_CLOUD_MINIMUM_COVERAGE_FOR_CLOUD)
				{
					// ensure a minimum height - if this goes too low, lighting gets really ugly near the horizon
					heightFrac = max(0.1, GetCloudHeightFractionForPoint(samplePos));
					//heightFrac = GetCloudHeightFractionForPoint(samplePos);

					UNITY_BRANCH
					if (WM_CAMERA_RENDER_MODE_NORMAL)
					{
						densityToLight += SampleCloudDensity(samplePos, weatherData, heightFrac, lod, _CloudRaymarchSampleDetailsForDirLight);
					}
					else
					{
						// fast approximation, this is just a reflection, who cares...
						fixed coverage = CloudVolumetricGetCoverage(weatherData);
						fixed type = CloudVolumetricGetCloudType(weatherData);
						densityToLight += (coverage * coverage * GetDensityHeightGradientForHeight(heightFrac, type));
					}
				}

				// march to next positions
				coneRadius += coneRadiusStep;
				pos += lightStep;
			}

			UNITY_BRANCH
			if (volumetricLightSampleDistant)
			{
				// one final sample farther away for distant cloud
				pos += (lightStep * 9.0);
				weatherData = CloudVolumetricSampleWeather(pos, lod);

				UNITY_BRANCH
				if (CloudVolumetricGetCoverage(weatherData) > VOLUMETRIC_CLOUD_MINIMUM_COVERAGE_FOR_CLOUD)
				{
					heightFrac = GetCloudHeightFractionForPoint(pos);

					UNITY_BRANCH
					if (WM_CAMERA_RENDER_MODE_NORMAL)
					{
						densityToLight += SampleCloudDensity(pos, weatherData, heightFrac, lod, _CloudRaymarchSampleDetailsForDirLight);
					}
					else
					{
						// fast approximation, this is just a reflection, who cares...
						fixed coverage = CloudVolumetricGetCoverage(weatherData);
						fixed type = CloudVolumetricGetCloudType(weatherData);
						densityToLight += (coverage * coverage * GetDensityHeightGradientForHeight(heightFrac, type));
					}
				}
			}

			float lightIntensity = _dirLightPrecomputedValues[lightIndex].y;

			energy = CloudVolumetricLightEnergy(_dirLightPrecomputedValues[lightIndex].x, cloudSample, eyeDensity, densityToLight * _CloudLightAbsorptionVolumetric, lightIntensity, lightDir, rayDir);
			fixed energyScalar = energy.x * energy.y * energy.z;

			UNITY_BRANCH
			if (_WeatherMakerCloudShadowSampleShadowMap < 1.0 && lightIndex == 0 && energyScalar > _WeatherMakerCloudShadowSampleShadowMap && _WeatherMakerDirLightPower[lightIndex].z > 0.05)
			{
				energyScalar = min(energyScalar, lerp(wm_sample_shadow_world_pos(marchPos, _WeatherMakerCloudShadowSampleShadowMap), 1.0, _WeatherMakerDirLightPower[lightIndex].w));
			}

			// indirect
			lightTotal += (lightColor.rgb * startHeightFrac * _dirLightPrecomputedValues[lightIndex].z * _CloudDirLightIndirectMultiplierVolumetric) +

				// direct
				(lightColor.rgb * lightIntensity * energyScalar * _CloudDirLightMultiplierVolumetric);

			// TODO: Can create a jarring transition as sun goes down and moon comes up, but the performance hit for rendering two dir lights is a lot, so
			// for now just accept this as a limitation
			// only sample first dir light for now...
			break;
		}
	}

	return lightTotal * _CloudDirColorVolumetric;
}

#if defined(VOLUMETRIC_CLOUD_ENABLE_POINT_LIGHTS)

fixed3 SamplePointLightSources(float3 marchPos, float3 rayDir, float startHeightFrac, float cloudSample, float eyeDensity, float lod, float4 uv)
{
	fixed3 lightTotal = fixed3Zero;

	UNITY_BRANCH
	if (_WeatherMakerPointLightCount > 0 || _WeatherMakerSpotLightCount > 0)
	{
		//lod++;

		UNITY_LOOP
		for (uint lightIndex = 0; lightIndex < uint(_WeatherMakerPointLightCount); lightIndex++)
		{
			float3 toLight = _WeatherMakerPointLightPosition[lightIndex].xyz - marchPos;
			float lengthSq = max(0.000001, dot(toLight, toLight));
			fixed atten = (1.0 / (1.0 + (lengthSq * _WeatherMakerPointLightAtten[lightIndex].z)));
			lightTotal += (saturate(atten) * max(0.5, cloudSample) * _WeatherMakerPointLightColor[lightIndex].a * _WeatherMakerPointLightColor[lightIndex].rgb);

			/*
			UNITY_BRANCH
			if (atten > 0.0)			{
				atten = saturate(atten * lightDither);
				float3 toLightNorm = normalize(toLight);
				float lightStepAmount = length(toLight) * invVolumetricLightIterationsNonDir;
				float3 lightStep = toLightNorm * lightStepAmount;
				float heightFrac;
				float4 weatherData;
				float coneRadius = lightStep;
				float3 samplePos;
				float3 energy;
				float densityToLight = 0.0;
				float3 pos = marchPos + lightStep;

				UNITY_LOOP
				for (uint lightStepIndex = 0.0; lightStepIndex < volumetricLightIterationsNonDir; lightStepIndex++)
				{
					heightFrac = GetCloudHeightFractionForPoint(pos);

					// sample in the cone, take the march pos and perturb by random vector and cone radius
					samplePos = pos + (_CloudConeRandomVectors[lightStepIndex] * coneRadius);

					// lookup position for cloud density
					weatherData = CloudVolumetricSampleWeather(samplePos, lod);
					densityToLight += SampleCloudDensity(samplePos, weatherData, heightFrac, lod, (densityToLight < 0.3));

					// march to next positions
					coneRadius += lightStep;
					pos += lightStep;
				}

				fixed4 lightColor = _WeatherMakerPointLightColor[lightIndex];
				fixed3 lightRgb = lightColor.rgb * atten * lightColor.a;
				energy = CloudVolumetricBeerLambert(densityToLight) * _CloudPointSpotLightMultiplierVolumetric;
				lightTotal += (lightRgb * energy);
			}
			*/
		}

		//ApplyDither(lightTotal.rgb, uv.xy, 0.02);

		UNITY_BRANCH
		if (_WeatherMakerSpotLightCount > 0)
		{
			UNITY_LOOP
			for (uint lightIndex = 0; lightIndex < uint(_WeatherMakerSpotLightCount); lightIndex++)
			{
				float3 toLight = _WeatherMakerSpotLightPosition[lightIndex].xyz - marchPos;
				float lengthSq = max(0.000001, dot(toLight, toLight));
				fixed atten = (1.0 / (1.0 + (lengthSq * _WeatherMakerSpotLightAtten[lightIndex].z)));
				toLight *= rsqrt(lengthSq);
				float theta = max(0.0, dot(toLight, -_WeatherMakerSpotLightDirection[lightIndex].xyz));
				atten *= saturate((theta - _WeatherMakerSpotLightAtten[lightIndex].x) * _WeatherMakerSpotLightAtten[lightIndex].y);
				lightTotal += (saturate(atten) * max(0.5, cloudSample) * _WeatherMakerSpotLightColor[lightIndex].a * _WeatherMakerSpotLightColor[lightIndex].rgb);
			}
		}

		lightTotal.rgb = max(0.0, lightTotal.rgb + (_CloudLightDitherLevel * RandomFloat(rayDir + _WeatherMakerTime.x)));
	}

	return lightTotal;
}

#endif

// https://en.wikipedia.org/wiki/Exponential_integral
float ExponentialIntegral(float v)
{
	return 0.5772156649015328606065 + log(0.0001 + abs(v)) + v * (1.0 + v * (0.25 + v * ((1.0 / 18.0) + v * ((1.0 / 96.0) + v * (1.0 / 600.0)))));
}

fixed3 SampleAmbientLight(float3 rayDir, float rayLength, float skyMultiplier, float heightFrac, fixed3 skyColor, fixed3 backgroundSkyColor, float4 weatherData)
{

#if defined(VOLUMETRIC_CLOUD_AMBIENT_MODE_LINEAR)

	// reduce sky light at lower heights
	// reduce ground light at higher heights
	fixed groundHeightFrac = 1.0 - min(1.0, heightFrac * _CloudAmbientGroundHeightMultiplierVolumetric);
	skyColor *= min(1.0, skyMultiplier * heightFrac * _CloudAmbientSkyHeightMultiplierVolumetric);
	fixed3 groundColor = volumetricCloudAmbientColorGround * groundHeightFrac;
	return _CloudEmissionColorVolumetric + skyColor + groundColor;

#else

	// // page 12-15 https://patapom.com/topics/Revision2013/Revision%202013%20-%20Real-time%20Volumetric%20Rendering%20Course%20Notes.pdf
	/*
	float Hp = VolumeTop - _Position.y; // Height to the top of the volume
	float a = -_ExtinctionCoeff * Hp;
	float3 IsotropicScatteringTop = IsotropicLightTop * max( 0.0, exp( a ) - a * Ei( a ));
	float Hb = _Position.y - VolumeBottom; // Height to the bottom of the volume
	a = -_ExtinctionCoeff * Hb;
	float3 IsotropicScatteringBottom = IsotropicLightBottom * max( 0.0, exp( a ) - a * Ei( a ));
	return IsotropicScatteringTop + IsotropicScatteringBottom;
	*/

	//float Hp = -intensity * saturate(1.0 - heightFrac);
	static const float ambientSkyPower = 1.0 - _CloudAmbientSkyHeightMultiplierVolumetric;
	float Hp = pow(heightFrac, ambientSkyPower) - 1.0;
	float3 scatterTop = skyColor * skyMultiplier * max(0.0, exp(Hp) - Hp * ExponentialIntegral(Hp));
	//float Hb = -intensity * heightFrac;
	static const float ambientGroudMultiplier = 1.0 - _CloudAmbientGroundHeightMultiplierVolumetric;
	float Hb = -(heightFrac * ambientGroudMultiplier);
	float3 scatterBottom = volumetricCloudAmbientColorGround * max(0.0, exp(Hb) - Hb * ExponentialIntegral(Hb));
	return _CloudEmissionColorVolumetric + (scatterTop + scatterBottom);

#endif

}

inline fixed4 FinalizeVolumetricCloudColor(fixed4 color, float4 uv, bool depthInCloud, uint marches)
{

#if defined(WEATHER_MAKER_ENABLE_VOLUMETRIC_CLOUDS) && VOLUMETRIC_CLOUD_RENDER_MODE == 2

	color.rgb = (float)marches / float(_CloudNoiseSampleCountVolumetric.y);
	color.a = 1.0;

#else

	UNITY_BRANCH
	if (color.a >= WM_MIN_PIXEL_VALUE)
	{

#if defined(UNITY_COLORSPACE_GAMMA)

        color.rgb *= 1.4;

#else

        color.rgb = pow(color.rgb, 2.2);

#endif

		UNITY_BRANCH
		if (_WeatherMakerEnableToneMapping)
		{
			color.rgb = FilmicTonemapFull(color.rgb, 2.0);
		}

        // pre-multiply
        color *= color.a;

        // soften further, remove lines
        color *= color.a;
	}

    // if depth position is not in the cloud layer, ensure a min pixel value
    color.a = lerp(max(color.a, WM_MIN_PIXEL_VALUE), color.a, depthInCloud);

#endif

	return color;
}

fixed ComputeCloudColorVolumetricHorizonFade(fixed4 color, fixed3 backgroundSkyColor, float opticalDepth, float3 cloudRay, float distanceToCloud)
{
	UNITY_BRANCH
	if (_CloudHorizonFadeMultiplierVolumetric > 0.0 && distanceToCloud > 1000.0)
	{
		// horizon fade
		// calculate horizon fade
		fixed fade = pow(opticalDepth, VOLUMETRIC_HORIZON_FADE_OPTICAL_DEPTH_POWER);
		fade = smoothstep(0.0, 1.0, fade - VOLUMETRIC_HORIZON_FADE_OPTICAL_DEPTH_SUBTRACTOR);

		// increase horizon fade dither to reduce banding as main dir light decreases in intensity
		fade *= (1.0 + (RandomFloat(cloudRay * _WeatherMakerTime.y) * VOLUMETRIC_HORIZON_FADE_DITHER)) * _CloudHorizonFadeMultiplierVolumetric;
		fade = clamp((1.0 - fade), VOLUMETRIC_MIN_HORIZON_FADE, 1.0);
		return fade;
	}
	else
	{
		return 1.0;
	}
}

fixed4 RaymarchVolumetricClouds
(
	float3 marchPos,
	float3 endPos,
	float rayLength,
	float distanceToCloud,
	float3 rayDir,
	float3 origRayDir,
	float4 uv,
	float depth,
	fixed3 skyColor,
	fixed3 backgroundSkyColor,
	inout uint marches,
	inout float opticalDepth
)
{
	UNITY_BRANCH
	if (rayDir.y < volumetricMinRayY || rayLength < 0.01)
	{
		return fixed4Zero;
	}

	fixed4 cloudColor = fixed4Zero;
	float startOpticalDepth = min(1.0, distanceToCloud * invVolumetricMaxOpticalDistance);
	fixed horizonFade = ComputeCloudColorVolumetricHorizonFade(cloudColor, backgroundSkyColor, startOpticalDepth, origRayDir, distanceToCloud);
	UNITY_BRANCH
	if (horizonFade < 0.01)
	{
		return cloudColor;
	}

	uint i = 0;
    float ditherFloat = (_CloudRayDitherVolumetric * RandomFloat(rayDir));
	float randomDither = 1.0 + ditherFloat;
    ditherFloat *= 0.02;

	uint sampleCount = uint(lerp(volumetricSampleCountRange.y, volumetricSampleCountRange.x, min(1.0, rayDir.y * rayDir.y * 10.0)));
	float skyAmbientMultiplier = clamp(startOpticalDepth * VOLUMETRIC_SKY_AMBIENT_OPTICAL_DEPTH_MULTIPLIER, VOLUMETRIC_SKY_AMBIENT_MIN_OPTICAL_DEPTH_MULTIPLIER, 1.0);
	// reduce sample count for clouds that are farther away
	sampleCount /= max(1.0, startOpticalDepth * VOLUMETRIC_SAMPLE_COUNT_OPTICAL_DEPTH_REDUCER);

	float invSampleCount = 1.0 / float(sampleCount);
    float minRayLengthForMarchDetermination = max(VOLUMETRIC_MIN_RAY_LENGTH, rayLength);

    // if above or in clouds, use a modified ray march adjustment
    float opticalDepthMarchMultiplier1 = max(1.0, startOpticalDepth * _CloudRayMarchParameters.w); // non-flythrough
    float opticalDepthMarchMultiplier2 = lerp(1.0, _CloudRayMarchParameters.w, (startOpticalDepth + (1.0 - abs(origRayDir.y))) * 0.5); // fly-through

    // as camera approaches cloud layer, lerp to fly-through ray march modifier
    float opticalDepthMarchMultiplier = randomDither * lerp(opticalDepthMarchMultiplier2, opticalDepthMarchMultiplier1, saturate(_CloudHeightInverseVolumetric * (_CloudStartVolumetric - WEATHER_MAKER_CAMERA_POS.y)));
    
	float marchLength = clamp(min(volumetricCloudMaxRayLength, minRayLengthForMarchDetermination) * invSampleCount * _CloudRaymarchMultiplierVolumetric, VOLUMETRIC_MIN_STEP_LENGTH, _CloudRayMarchParameters.x);
	float marchLengthFull = clamp(min(volumetricCloudMaxRayLength, minRayLengthForMarchDetermination) * invSampleCount * _CloudRaymarchMultiplierVolumetric, VOLUMETRIC_MIN_STEP_LENGTH, _CloudRayMarchParameters.y);
	marchLength *= opticalDepthMarchMultiplier;
	marchLengthFull *= opticalDepthMarchMultiplier;
	float3 marchDir = (rayDir + ditherFloat) * marchLength;
	float3 marchDirFull = (rayDir + ditherFloat) * marchLengthFull;
	marchPos += (marchDir * 0.5);
	float3 startMarchDirOrig = marchDir;
	float3 startMarchDir = marchDir;
	float3 startMarchDirFull = marchDirFull;
	float3 marchDirNorm = normalize(marchDir);
	float heightFrac = 0.0;
	float cloudSample = 0.0;
	float cloudSampleTotal = 0.0;
	float4 lightSample;
	float4 weatherData;
	float marchLerp;

	// increase lod for clouds that are farther away
	float lod = min(volumetricLod.y, volumetricLod.x + (startOpticalDepth * VOLUMETRIC_LOD_OPTICAL_DEPTH_MULTIPLIER));
	uint inCloud = 0;
	uint maybeInCloud = 0;
	uint zeroSampleCounter = 0;
	uint zeroSampleCounter2 = 0;
	uint skipCloud = 0;
	float3 lastZeroPos = marchPos;
	float sdf;

#if defined(VOLUMETRIC_CLOUD_ENABLE_AMBIENT_SKY_DENSITY_SAMPLE)

	float3 ambientPos;

#endif

	UNITY_LOOP
	while (i++ < sampleCount && cloudColor.a < 0.999 && heightFrac > -0.001 && heightFrac < 1.001)// && distance(marchPos, WEATHER_MAKER_CAMERA_POS) < distance(endPos, WEATHER_MAKER_CAMERA_POS))
	{
		maybeInCloud = 0;

		UNITY_BRANCH
		if (_CloudRayMarchParameters.z > 0.0)
		{
			marchLerp = min(1.0, float(i) * _CloudRayMarchParameters.z);
			startMarchDir = lerp(startMarchDirOrig, startMarchDirFull, marchLerp);
			marchDir = lerp(marchDir, marchDirFull, marchLerp);
		}

		heightFrac = GetCloudHeightFractionForPoint(marchPos);

		// filter on bottom fade
		UNITY_BRANCH
		if (smoothstep(0.0, _CloudBottomFadeVolumetric, heightFrac) > 0.15)
		{
			weatherData = CloudVolumetricSampleWeather(marchPos, lod);
			marches++;

			// min coverage
			UNITY_BRANCH
			if (CloudVolumetricGetCoverage(weatherData) > VOLUMETRIC_CLOUD_MINIMUM_COVERAGE_FOR_CLOUD)
			{
				maybeInCloud = 1;
				cloudSample = SampleCloudDensity(marchPos, weatherData, heightFrac, lod, false);

				UNITY_BRANCH
				if (cloudSample > VOLUMETRIC_CLOUD_MIN_NOISE_VALUE)
				{
					inCloud = 1;
					UNITY_BRANCH
					if (zeroSampleCounter2 != 0)
					{
						// move back to last zero pos and resample
						marchPos = lastZeroPos;
						zeroSampleCounter = 0.0;
						zeroSampleCounter2 = 0.0;

						// march at reduced march speed when maybe in cloud
						marchDir = startMarchDir * volumetricCloudMaybeInCloudStepMultiplier;

						// cannot use continue here, crashes metal on iOS/MAC
						skipCloud = 2;
					}
					else
					{
						// we did not hit the increased march optimization, simply reset the 0 counter since we are maybe in a cloud
						zeroSampleCounter = 0;
					}

					// sample just details using the shape noise from the above call which was done without details
					cloudSample = SampleCloudDensityDetails(cloudSample, weatherData, marchPos, heightFrac, lod);

					// soft particles
					UNITY_BRANCH
					if (depth < _ProjectionParams.z)
					{
						float partZ = distance(marchPos, WEATHER_MAKER_CAMERA_POS);
						float diff = (depth - partZ);

#define CLOUD_INV_FADE 0.001

                        float multiplier = saturate(CLOUD_INV_FADE * diff);

                        // if we have gotten close enough or beyond the depth buffer, we are done
                        i = lerp(sampleCount, i, multiplier > 0.0001);

                        // adjust cloud sample
                        cloudSample *= multiplier;
					}

					UNITY_BRANCH
					if (cloudSample > VOLUMETRIC_CLOUD_MIN_NOISE_VALUE)
					{
						// march at reduced march speed when in cloud
						marchDir = startMarchDir * volumetricCloudInCloudStepMultiplier;
						cloudSampleTotal += cloudSample;

						lightSample.rgb = SampleAmbientLight(rayDir, rayLength, skyAmbientMultiplier, heightFrac, skyColor, backgroundSkyColor, weatherData);
						lightSample.rgb += SampleDirLightSources(marchPos, rayDir, heightFrac, cloudSample, cloudSampleTotal, lod);

#if defined(VOLUMETRIC_CLOUD_ENABLE_POINT_LIGHTS)

						lightSample.rgb += SamplePointLightSources(marchPos, rayDir, heightFrac, cloudSample, cloudSampleTotal, lod, uv);

#endif

						lightSample.a = cloudSample;
						lightSample.rgb *= cloudSample;

						// accumulate color
						cloudColor = ((1.0 - cloudColor.a) * lightSample) + cloudColor;
					}
					else
					{
						marchDir = startMarchDir * volumetricCloudMaybeInCloudStepMultiplier;
					}
				}
				else if (inCloud)
				{
					inCloud = 0;
					marchDir = startMarchDir;
				}
			}
			else if (inCloud)
			{
				inCloud = 0;
				marchDir = startMarchDir;
			}

			UNITY_BRANCH
			if (skipCloud != 2)
			{
				// if we are not in a cloud, see if we need to move forward faster
				skipCloud = (_CloudRaymarchSkipThreshold != 0 && !inCloud && ++zeroSampleCounter > _CloudRaymarchSkipThreshold);

				// marchDir multiplies every zero sample to hopefully get through the clouds faster with less texture lookups
				// stop multiplying once enough missed samples in a row are found so that march is not too large, missing clouds
				// we increase the distance slowly each step using _CloudRaymarchSkipMultiplier, this avoids a lot of artifacts and missed pixels and clouds
				marchDir = lerp(marchDir, marchDir * _CloudRaymarchSkipMultiplier, skipCloud && zeroSampleCounter2 < _CloudRaymarchSkipMultiplierMaxCount);
				lastZeroPos = lerp(lastZeroPos, marchPos, skipCloud);
				zeroSampleCounter2 = lerp(zeroSampleCounter2, zeroSampleCounter2 + 1, skipCloud);
			}
		}

		UNITY_BRANCH
		if (skipCloud != 2)
		{
			// if we are not in a cloud, see if we can march faster based on a signed distance field
			UNITY_BRANCH
			if (maybeInCloud == 0 && (sdf = CloudVolumetricGetDistance(weatherData)) < 1.0)
			{
				float worldUnits = sdf * volumetricCloudWeatherMapSdfScale;
				float multiplier = max(0.1, length(marchDirNorm.xz));
				worldUnits = max(marchLength, worldUnits / multiplier);
				marchPos += (marchDirNorm * worldUnits);
			}
			else
			{
				marchPos += marchDir;
			}
		}
		else
		{
			skipCloud = 0;
		}
	}

	cloudColor *= horizonFade;

	return cloudColor;
}

uint SetupCloudRaymarch(float3 worldSpaceCameraPos, float3 rayDir, float depth, float depth2,
	float4 innerSphereXYZRadSq, float4 outterSphereXYZRadSq,
	out float3 startPos, out float3 endPos, out float rayLength, out float distanceToSphere,
	out float3 startPos2, out float3 endPos2, out float rayLength2, out float distanceToSphere2)
{
	UNITY_BRANCH
	if (_CloudPlanetRadiusVolumetric > 0.0)
	{
		//return SetupPlanetRaymarch(worldSpaceCameraPos, rayDir, depth, depth2, volumetricSphereInner, volumetricSphereOutter,
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

fixed4 ComputeCloudColorVolumetric(float3 rayDir, float4 uv, float depth, fixed3 backgroundSkyColor)
{
	float3 cloudRayDir = normalize(float3(rayDir.x, rayDir.y + _CloudRayOffsetVolumetric, rayDir.z));
	float3 marchPos, marchPos2;
	float3 endPos, endPos2;
	float rayLength, rayLength2;
	float distanceToSphere, distanceToSphere2;

	// determine what (if any) part of the cloud volume we intersected
	uint iterations = SetupCloudRaymarch(WEATHER_MAKER_CAMERA_POS, cloudRayDir, depth, depth, volumetricSphereInner, volumetricSphereOutter,
		marchPos, endPos, rayLength, distanceToSphere, marchPos2, endPos2, rayLength2, distanceToSphere2);

	UNITY_BRANCH
	if (iterations > 0)
	{
		// precompute directional light values
		UNITY_UNROLL
		for (uint lightIndex = 0; lightIndex < uint(_WeatherMakerDirLightCount); lightIndex++)
		{
			float intensity = _WeatherMakerDirLightColor[lightIndex].a;
			float eyeDot = dot(rayDir, _WeatherMakerDirLightPosition[lightIndex].xyz);
			_dirLightPrecomputedValues[lightIndex] = float4
			(
				max(0.0, eyeDot),
				max(intensity, max(0.33, (eyeDot + 1.0) * 0.5) * _WeatherMakerDirLightVar1[lightIndex].w),
				_dirLightPrecomputedValues[lightIndex].z = min(intensity, intensity * intensity),
				0.0
			);
		}

		fixed4 cloudLightColors[2] = { fixed4Zero, fixed4Zero };
		float3 skyRay = float3(cloudRayDir.x, abs(cloudRayDir.y), cloudRayDir.z);
		uint marches = 0;
		float opticalDepth = 1.0;
		fixed3 skyColor = volumetricCloudAmbientColorSky + backgroundSkyColor;

		UNITY_LOOP
		for (uint iterationIndex = 0; iterationIndex < iterations; iterationIndex++)
		{
			cloudLightColors[iterationIndex] = RaymarchVolumetricClouds
			(
				lerp(marchPos, marchPos2, iterationIndex),
				lerp(endPos, endPos2, iterationIndex),
				lerp(rayLength, rayLength2, iterationIndex),
				lerp(distanceToSphere, distanceToSphere2, iterationIndex),
				cloudRayDir,
				rayDir,
				uv,
				depth,
				skyColor,
				backgroundSkyColor,
				marches,
				opticalDepth
			);
		}

		// custom blend
		cloudLightColors[1].rgb = (cloudLightColors[0].rgb + (cloudLightColors[1].rgb * (1.0 - cloudLightColors[0].a)));
		cloudLightColors[1].a = max(cloudLightColors[0].a, cloudLightColors[1].a);
        bool depthInCloud = (endPos.y >= _CloudStartVolumetric && endPos.y <= _CloudEndVolumetric);
		return FinalizeVolumetricCloudColor(cloudLightColors[1] * _CloudColorVolumetric, uv, depthInCloud, marches);
	}
	else
	{
		// missed cloud layer entirely
		return fixed4Zero;
	}
}

#endif // WEATHER_MAKER_ENABLE_VOLUMETRIC_CLOUDS

inline float ComputeCloudShadowDetails(float3 worldPos, uint dirIndex, float shadow, float lod)
{
	// optimize out details on very lightly shadowed areas
	shadow = clamp(shadow, _WeatherMakerDirLightPower[dirIndex].w, 1.0);

	// apply cloud detail shadows if desired
	UNITY_BRANCH
	if (_WeatherMakerDirLightPower[dirIndex].z > 0.0 && shadow > _WeatherMakerDirLightPower[dirIndex].w && shadow < 1.0 && volumetricCloudShadowDetailIntensity > 0.0)
	{
		fixed invShadow = 1.0 - shadow;
		fixed cloudShadowDetailSample = tex2Dlod(_WeatherMakerCloudShadowDetailTexture, float4((worldPos.xz + _CloudCoverageVelocity.xz + _CloudCoverageOffset.xz) * _WeatherMakerCloudShadowDetailScale, 0.0, lod)).a;
		cloudShadowDetailSample = min(volumetricCloudMaxShadow, saturate(3.0 * (invShadow - 0.1)) * cloudShadowDetailSample * volumetricCloudShadowDetailIntensity);
		cloudShadowDetailSample = lerp(cloudShadowDetailSample, invShadow, volumetricCloudShadowDetailFalloff);
		shadow = max(_WeatherMakerDirLightPower[dirIndex].w, 1.0 - max(invShadow, cloudShadowDetailSample));
	}

	return shadow;
}

// worldPos does not have _WeatherMakerCameraOriginOffset applied
float ComputeCloudShadowStrengthTextureLOD(float3 worldPos, uint dirIndex, float existingShadow, bool sampleDetails, float lod)
{
	// return ComputeCloudShadowStrength(worldPos, dirIndex, existingShadow, sampleDetails, 0.0);

	float3 rayDir = _WeatherMakerDirLightPosition[dirIndex].xyz;
	float origRayY = rayDir.y;

	UNITY_BRANCH
	if (existingShadow > 0.001 && worldPos.y <= volumetricCloudPlanePos.y && origRayY > -0.1 && _CloudCoverVolumetric > 0.0)
	{
		rayDir.y = max(0.05, rayDir.y);
		float offsetMultiplier = max(0.0, worldPos.y) / max(0.001, rayDir.y);
		float2 offset = rayDir.xz * offsetMultiplier;
		float2 shadowUV = worldPos.xz - offset - _WeatherMakerCameraOriginOffset.xz;
		shadowUV *= volumetricCloudShadowScale;
		shadowUV += 0.5;
		float shadow = tex2Dlod(_WeatherMakerCloudShadowTexture, float4(shadowUV, 0.0, lod)).r;

		UNITY_BRANCH
		if (sampleDetails)
		{
			shadow = ComputeCloudShadowDetails(worldPos, dirIndex, shadow, lod);
		}

		return min(existingShadow, shadow);
	}
	else
	{
		return existingShadow;
	}
}

float ComputeCloudShadowStrengthTexture(float3 worldPos, uint dirIndex, float existingShadow, bool sampleDetails)
{
	return ComputeCloudShadowStrengthTextureLOD(worldPos, dirIndex, existingShadow, sampleDetails, 0.0);
}

float ComputeCloudShadowStrength(float3 worldPos, uint dirIndex, float existingShadow, bool sampleDetails, float lodMultiplier)
{
	float shadowValue = min(existingShadow, weatherMakerGlobalShadow);
	float maxShadow = _WeatherMakerDirLightPower[dirIndex].w;

	UNITY_BRANCH
	if (shadowValue > maxShadow && _WeatherMakerShadowsEnabled && _CloudCoverVolumetric > 0.0)
	{

#if defined(WEATHER_MAKER_ENABLE_VOLUMETRIC_CLOUDS)

		// take advantage of the fact that dir lights are supported by perspective/ortho and then by intensity
		UNITY_BRANCH
		if (_WeatherMakerCloudVolumetricShadowSampleCount > 0 && dirIndex < uint(_WeatherMakerDirLightCount) &&
			_WeatherMakerDirLightVar1[dirIndex].y == 0.0 && _WeatherMakerDirLightColor[dirIndex].a > 0.0)
		{
			float3 rayDir = _WeatherMakerDirLightPosition[dirIndex].xyz;
			float worldPosDistance = distance(worldPos, WEATHER_MAKER_CAMERA_POS);
			float worldPosDistance01 = worldPosDistance * _ProjectionParams.w;

#if WEATHER_MAKER_ENABLE_SHADOW_FADE == 1

			float fade = 1.0 - (min(1.0, worldPosDistance01 * _WeatherMakerCloudShadowDistanceFade) * max(0.0, rayDir.y * 2.0));

			UNITY_BRANCH
			if (fade > 0.01)

#endif

			{
				float3 startPos, startPos2;
				float3 endPos, endPos2;
				float rayLength, rayLength2;
				float distanceToSphere, distanceToSphere2;

				// ensure a minimum amount of cloud intersection, don't want a horizontal ray
				float rayY = max(0.05, rayDir.y + _CloudRayOffsetVolumetric);
				float3 cloudRayDir = normalize(float3(rayDir.x, rayY, rayDir.z));
				SetupCloudRaymarch(worldPos, cloudRayDir, 1000000.0, 0.0, volumetricSphereInner, volumetricSphereOutter,
					startPos, endPos, rayLength, distanceToSphere, startPos2, endPos2, rayLength2, distanceToSphere2);

				float cloudCoverage = 0.0;
				float3 marchPos = startPos;
				float heightFrac;
				float4 weatherData;
				float cloudType;
				float dither = RandomFloat(worldPos);
				float randomDither = 1.0 + (_WeatherMakerCloudVolumetricShadowDither * dither);
				float3 marchDir = rayDir * min(VOLUMETRIC_CLOUD_SHADOW_MAX_RAY_LENGTH, rayLength) * volumetricCloudShadowSampleCountInv;
				fixed lod = max(0.0, (worldPosDistance01 - 0.333)) * 4.0 * lodMultiplier;
				fixed lodMap = max(0.0, (worldPosDistance01 * 4.0 * lodMultiplier));
				float samp;

				UNITY_LOOP
				for (uint i = 0; i < _WeatherMakerCloudVolumetricShadowSampleCount && cloudCoverage < 0.999; i++)
				{
					marchPos += marchDir;
					heightFrac = GetCloudHeightFractionForPoint(marchPos);
					weatherData = CloudVolumetricSampleWeatherGaussian17(marchPos, lodMap);
					cloudType = CloudVolumetricGetCloudType(weatherData);
					samp = (CloudVolumetricGetCoverage(weatherData) * GetDensityHeightGradientForHeight(heightFrac, cloudType));
					//samp = SampleCloudDensity(marchPos, weatherData, heightFrac, lod, true);
					samp += _CloudShadowMapAdder;
					samp *= _CloudShadowMapMultiplier;// *volumetricCloudNoiseShadowScalar;
					samp = saturate(samp);
					samp = pow(samp, _CloudShadowMapPower);
					samp *= randomDither;
					cloudCoverage += samp;
				}

				fixed flatCoverage = ComputeFlatCloudShadows(rayDir, worldPos);
				cloudCoverage = min(1.0, cloudCoverage + flatCoverage);
				fixed cloudCoverageSquared = cloudCoverage * cloudCoverage;

				// apply cloud detail shadows if desired
				UNITY_BRANCH
				if (sampleDetails && cloudCoverageSquared > 0.0 && cloudCoverageSquared < 1.0 && volumetricCloudShadowDetailIntensity > 0.0)
				{
					fixed cloudShadowDetailSample = tex2Dlod(_WeatherMakerCloudShadowDetailTexture, float4((worldPos.xz + _CloudCoverageVelocity.xz + _CloudCoverageOffset.xz) * _WeatherMakerCloudShadowDetailScale, 0.0, lod)).a;
					cloudShadowDetailSample = min(volumetricCloudMaxShadow, saturate(3.0 * (cloudCoverage - 0.1)) * cloudShadowDetailSample * volumetricCloudShadowDetailIntensity);
					cloudShadowDetailSample = lerp(cloudShadowDetailSample, cloudCoverage, volumetricCloudShadowDetailFalloff);
					cloudCoverage = max(cloudCoverage, cloudShadowDetailSample);
				}
				cloudCoverage *= randomDither;

#if WEATHER_MAKER_ENABLE_SHADOW_FADE == 1

				cloudCoverage *= fade;

#endif

				cloudCoverage = 1.0 - cloudCoverage;
				shadowValue = min(shadowValue, max(cloudCoverage, maxShadow));
			}
		}

#endif

	}

	return shadowValue;
}

#endif // __WEATHER_MAKER_CLOUD_SHADER__
