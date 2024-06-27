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

#ifndef __WEATHER_MAKER_SHADER_LIGHT__
#define __WEATHER_MAKER_SHADER_LIGHT__

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "UnityPBSLighting.cginc"
#include "UnityStandardBRDF.cginc"
#include "HLSLSupport.cginc"

#include "WeatherMakerCoreShaderInclude.cginc"
#include "WeatherMakerMathShaderInclude.cginc"
#include "WeatherMakerNoiseShaderInclude.cginc"
#include "WeatherMakerShadowsShaderInclude.cginc"

// filter lights...
// WEATHER_MAKER_FILTER_LIGHT_ORTHOGRAPHIC - must define ORTHOGRAPHIC_MODE if orthographic

// disable features...
// WEATHER_MAKER_LIGHT_NO_DIR_LIGHT - no dir light calculations
// WEATHER_MAKER_LIGHT_NO_NORMALS - no normal calculations, also removes specular
// WEATHER_MAKER_LIGHT_NO_SPECULAR - no specular calculations
// WEATHER_MAKER_DEPTH_SHADOWS_OFF - turn off depth shadows

// enable features...
// WEATHER_MAKER_LIGHT_SPECULAR_SPARKLE - enable specular sparkle (if WEATHER_MAKER_LIGHT_NO_NORMALS is defined and WEATHER_MAKER_LIGHT_NO_SPECULAR is not defined)
// WEATHER_MAKER_LIGHT_SPECULAR_TRANSLUCENT - allow specular translucency (i.e. water, glass, etc.) specular highlight on opposite side of surface
// WEATHER_MAKER_SHADOWS_SCREEN allow screen space shadow lighting
// WEATHER_MAKER_SHADOWS_DEPTH_EXTERNAL_FUNC - set to a float func(float3 worldPos, int dirIndex, float existingShadow, bool sampleDetails) which returns shadow strength for a world pos

#define MAX_LIGHT_COUNT 16

struct wm_world_space_light_params
{
	float3 worldPos;
	float shadowStrength; // 0 for global fog/cloud shadow, > 0 to 1 for screen space or shadow map

#if !defined(WEATHER_MAKER_LIGHT_NO_DIFFUSE)

	fixed3 diffuseColor;
	fixed3 ambientColor;

#endif

#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS)

	float3 worldNormal;

#if !defined(WEATHER_MAKER_LIGHT_NO_SPECULAR)

	float3 rayDir;
	fixed3 specularColor;
	fixed specularPower;

#endif

#endif

#if defined(WEATHER_MAKER_LIGHT_SPECULAR_SPARKLE)

	fixed distanceToWorldPosSquared; // distance from camera to worldPos
	sampler2D sparkleNoise;
	fixed3 sparkleTintColor;
	fixed4 sparkleScale;
	fixed4 sparkleOffset;
	fixed4 sparkleFade;

#endif

#if defined(WEATHER_MAKER_SHADOWS_SCREEN)

	float2 uv;

#endif

};

// Lighting --------------------------------------------------------------------------------

// time of day multipliers, all add up to 1
uniform fixed _WeatherMakerDayMultiplier;
uniform fixed _WeatherMakerDawnDuskMultiplier;
uniform fixed _WeatherMakerNightMultiplier;
uniform fixed _WeatherMakerDayMultiplierInv;
uniform fixed _WeatherMakerDawnDuskMultiplierInv;
uniform fixed _WeatherMakerNightMultiplierInv;

// ambient light
uniform fixed3 _WeatherMakerAmbientLightColor;
uniform fixed3 _WeatherMakerAmbientLightColorSky;
uniform fixed3 _WeatherMakerAmbientLightColorGround;
uniform fixed3 _WeatherMakerAmbientLightColorEquator;

static const fixed3 weatherMakerAmbientLightColorCombined = (_WeatherMakerAmbientLightColor + _WeatherMakerAmbientLightColorSky + _WeatherMakerAmbientLightColorGround);

static const float3 weatherMakerRandomCone[17] = // random vectors for cone sampling
{
	float3(-0.07864, -0.20610, 0.06905),
	float3(0.07096, -0.19096, 0.12691),
	float3(0.01922, 0.23669, 0.14018),
	float3(-0.09469, -0.13924, -0.23133),
	float3(-0.26556, 0.01159, 0.03149),
	float3(-0.09375, -0.16036, -0.25845),
	float3(-0.30011, 0.12470, 0.04241),
	float3(0.01836, -0.30818, 0.03992),
	float3(0.24588, -0.20103, 0.02396),
	float3(-0.11921, -0.22320, -0.11015),
	float3(0.11726, -0.18701, -0.04830),
	float3(-0.01300, 0.02882, 0.08764),
	float3(-0.10065, 0.17805, 0.10268),
	float3(0.04206, -0.09293, -0.26513),
	float3(-0.02481, -0.19277, 0.24655),
	float3(-0.25913, 0.00741, 0.07627),
	float3(0.043241, -0.19523, -0.075213)
};

// dir lights
uniform int _WeatherMakerDirLightCount;
uniform float4 _WeatherMakerDirLightPosition[MAX_LIGHT_COUNT]; // to dir light
uniform float4 _WeatherMakerDirLightDirection[MAX_LIGHT_COUNT]; // from dir light, w = isSun
uniform float4 _WeatherMakerDirLightColor[MAX_LIGHT_COUNT];
uniform float4 _WeatherMakerDirLightViewportPosition[MAX_LIGHT_COUNT];
uniform float4 _WeatherMakerDirLightPower[MAX_LIGHT_COUNT]; // power, multiplier, shadow strength, 1 - shadow strength
uniform float4 _WeatherMakerDirLightQuaternion[MAX_LIGHT_COUNT]; // convert from world space to light space
uniform float4 _WeatherMakerDirLightVar1[MAX_LIGHT_COUNT]; // x = how much have dir light changed since the last frame, y = 0 for perspective, 1 for ortho, z = shaft multiplier, w = horizon intensity
uniform float _WeatherMakerDirLightMultiplier;

// point lights
uniform int _WeatherMakerPointLightCount;
uniform float4 _WeatherMakerPointLightPosition[MAX_LIGHT_COUNT]; // w = range squared
uniform float4 _WeatherMakerPointLightDirection[MAX_LIGHT_COUNT];
uniform float4 _WeatherMakerPointLightColor[MAX_LIGHT_COUNT];
uniform float4 _WeatherMakerPointLightAtten[MAX_LIGHT_COUNT]; // -1, 1, quadratic atten / range squared, 1 / range squared
uniform float4 _WeatherMakerPointLightVar1[MAX_LIGHT_COUNT]; // x = how much have point light changed since the last frame, yzw = unused

// spot lights
uniform int _WeatherMakerSpotLightCount;
uniform float4 _WeatherMakerSpotLightPosition[MAX_LIGHT_COUNT]; // w = falloff resistor, thinner angles do not fall off at edges
uniform float4 _WeatherMakerSpotLightDirection[MAX_LIGHT_COUNT]; // w = end circle radius squared
uniform float4 _WeatherMakerSpotLightColor[MAX_LIGHT_COUNT];
uniform float4 _WeatherMakerSpotLightAtten[MAX_LIGHT_COUNT]; // outer cutoff, cutoff, quadratic atten / range squared, 1 / range squared
uniform float4 _WeatherMakerSpotLightSpotEnd[MAX_LIGHT_COUNT]; // w = range squared
uniform float4 _WeatherMakerSpotLightVar1[MAX_LIGHT_COUNT]; // x = how much have spot light changed since the last frame, yzw = unused

// area lights
uniform int _WeatherMakerAreaLightCount;
uniform float4 _WeatherMakerAreaLightPosition[MAX_LIGHT_COUNT]; // w = range squared
uniform float4 _WeatherMakerAreaLightPositionEnd[MAX_LIGHT_COUNT]; // w = 0
uniform float4 _WeatherMakerAreaLightRotation[MAX_LIGHT_COUNT]; // quaternion
uniform float4 _WeatherMakerAreaLightDirection[MAX_LIGHT_COUNT]; // w = 0
uniform float4 _WeatherMakerAreaLightMinPosition[MAX_LIGHT_COUNT]; // AABB min, w = 0
uniform float4 _WeatherMakerAreaLightMaxPosition[MAX_LIGHT_COUNT]; // AABB max, w = 0
uniform float4 _WeatherMakerAreaLightColor[MAX_LIGHT_COUNT];
uniform float4 _WeatherMakerAreaLightAtten[MAX_LIGHT_COUNT]; // 1 / diagonal radius squared, (width + height) * 0.5, quadratic atten / range squared, 1 / range squared
uniform float4 _WeatherMakerAreaLightVar1[MAX_LIGHT_COUNT]; // x = how much have area light changed since the last frame, yzw = unused

// ------------------------------------------------------------------------------------------

uniform float3 _WeatherMakerSunDirectionUp; // direction to sun
uniform float3 _WeatherMakerSunDirectionUp2D; // direction to sun
uniform float3 _WeatherMakerSunDirectionDown; // direction sun is facing
uniform float3 _WeatherMakerSunDirectionDown2D; // direction sun is facing
uniform float4 _WeatherMakerSunColor; // sun light color
uniform float4 _WeatherMakerSunTintColor; // sun tint color
uniform float3 _WeatherMakerSunPositionNormalized; // sun position in world space, normalized
uniform float3 _WeatherMakerSunPositionWorldSpace; // sun position in world space
uniform float4 _WeatherMakerSunLightPower; // power, multiplier, shadow strength, 1.0 - shadow strength
uniform float4 _WeatherMakerSunVar1; // scale, sun intensity ^ 0.5, sun intensity ^ 0.75, sun intensity ^ 2
uniform uint _WeatherMakerShadowsEnabled;

uniform fixed4 _TintColor;
uniform fixed3 _EmissiveColor;
uniform fixed _Intensity;
uniform fixed _DirectionalLightMultiplier;
uniform fixed _PointSpotLightMultiplier;
uniform fixed _AmbientLightMultiplier;
uniform fixed _WeatherMakerCloudGlobalShadow; // global shadow for clouds, applies to every pixel evenly
uniform fixed _WeatherMakerCloudGlobalShadow2; // global shadow for clouds for special effects like fog, applies to every pixel evenly
uniform fixed _WeatherMakerFogGlobalShadow; // global shadow for fog, applies to every pixel evenly

static const fixed weatherMakerGlobalShadow = min(_WeatherMakerCloudGlobalShadow, _WeatherMakerFogGlobalShadow);
static const fixed weatherMakerGlobalShadow2 = min(_WeatherMakerCloudGlobalShadow2, _WeatherMakerFogGlobalShadow);
static const fixed weatherMakerGlobalShadow4 = weatherMakerGlobalShadow2 * weatherMakerGlobalShadow2 * weatherMakerGlobalShadow2 * weatherMakerGlobalShadow2;

uniform int _WeatherMakerFogSunShaftMode;
uniform fixed4 _WeatherMakerFogSunShaftsParam1;
uniform fixed4 _WeatherMakerFogSunShaftsParam2;
uniform fixed4 _WeatherMakerFogSunShaftsTintColor;
uniform fixed4 _WeatherMakerFogSunShaftsDitherMagic;
uniform fixed _WeatherMakerFogSunShaftsBackgroundIntensity;
uniform fixed3 _WeatherMakerFogSunShaftsBackgroundTintColor;

// is lighting per pixel or per vertex?
uniform int _WeatherMakerPerPixelLighting;

#define WM_FOG_SUN_SHAFT_ENABLE (_WeatherMakerFogSunShaftMode == 1)

uniform fixed _WeatherMakerDirectionalLightScatterMultiplier;

fixed CalculateDirLightAtten(float3 lightDir
	
#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS)

	, float3 normal
	
#endif

	)
{

#if defined(ORTHOGRAPHIC_MODE)

	fixed atten = pow(max(0.0, dot(lightDir, float3(0.0, 0.0, -1.0))), 0.5);

#elif !defined(WEATHER_MAKER_LIGHT_NO_NORMALS)

	fixed atten = max(0.0, dot(lightDir, normal));

#else

	fixed atten = 1.0;

#endif

	// average in a little bit from the direct line of site
	//atten = ((atten * 0.7) + (max(0.0, dot(worldViewDir, -normal)) * 0.3)) * 0.5;

	// horizonFade
	// atten *= saturate((lightDir.y + 0.1) * 3.0);

	return atten * _DirectionalLightMultiplier;
}

fixed CalculatePointLightAtten(float3 worldPos,
	
#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS)

	float3 worldNormal,
	
#endif
	
	float3 worldLightPos, float4 lightAtten)
{
	float3 toLight = (worldLightPos - worldPos);

#if defined(ORTHOGRAPHIC_MODE)

	// ignore view normal and point straight out along z axis
	fixed atten = dot(fixed3(0, 0, -1), normalize(toLight));

#else

	float lengthSq = max(0.000001, dot(toLight, toLight));
	fixed atten = (1.0 / (1.0 + (lengthSq * lightAtten.z)));
	fixed attenFalloff = 1.0 - pow(min(1.0, (lengthSq * lightAtten.w)), 2.0);
	atten *= attenFalloff;

#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS)

	toLight *= rsqrt(lengthSq);
	atten *= max(0.0, dot(worldNormal, toLight));

#endif

#endif

	return atten * _PointSpotLightMultiplier;
}

fixed CalculateSpotLightAtten(float3 worldPos,
	
#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS)

	float3 worldNormal,
	
#endif
	
	float3 worldLightPos, float3 worldLightDir, float4 lightAtten)
{
	float3 toLight = (worldLightPos - worldPos);
	float lengthSq = max(0.000001, dot(toLight, toLight));
	fixed atten = (1.0 / (1.0 + (lengthSq * lightAtten.z)));
	toLight *= rsqrt(lengthSq);

#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS)

	atten *= max(0.0, dot(worldNormal, toLight));

#endif

	float theta = max(0.0, dot(toLight, -worldLightDir.xyz));
	atten *= saturate((theta - lightAtten.x) * lightAtten.y);

	return atten * _PointSpotLightMultiplier;
}

fixed CalculateAreaLightAtten(float3 worldPos,

#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS)

	float3 worldNormal,

#endif
	
	float3 worldLightPos, float3 worldLightDir, float4 worldLightRot, float3 lightBoxMin, float3 lightBoxMax, float4 lightAtten)
{
	// area light is in non-rotated local space, rotate the world coordinate around the light position to get it into the same space
	float3 toLight = (worldPos - worldLightPos);
	toLight = RotatePointZeroOriginQuaternion(toLight, worldLightRot);
	float inBox = PointBoxIntersect(toLight, lightBoxMin, lightBoxMax);
	float lengthSq = max(0.000001, dot(toLight, toLight));
	fixed atten = (1.0 / (1.0 + (lengthSq * lightAtten.z)));
	fixed attenFalloff = 1.0 - min(1.0, (lengthSq * lightAtten.w));
	atten *= attenFalloff;

	float dx = max(lightBoxMin.x - toLight.x, toLight.x - lightBoxMax.x);
	float dy = max(lightBoxMin.y - toLight.y, toLight.y - lightBoxMax.y);
	attenFalloff = min(1.0, ((dx + dx) * (dy + dy)) * lightAtten.x);
	atten *= attenFalloff;

#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS)

	toLight *= rsqrt(lengthSq);
	// rotate normal to get it in light space
	worldNormal = RotatePointZeroOriginQuaternion(worldNormal, worldLightRot);
	atten *= max(0.0, -dot(worldNormal, toLight));

#endif
	
	return inBox * atten * _PointSpotLightMultiplier;
}

float CalculateDirLightDepthShadowPower(float3 worldPos, int dirIndex)
{

#if !defined(WEATHER_MAKER_DEPTH_SHADOWS_OFF) && !defined(ORTHOGRAPHIC_MODE)

	// for now include the first dir light only in shadow calc, otherwise just use 1
	if (_WeatherMakerShadowsEnabled && weatherMakerGlobalShadow > 0.0 && dirIndex == 0 && _WeatherMakerDirLightColor[dirIndex].a > 0.01 &&
		_WeatherMakerDirLightPower[dirIndex].z > 0.0)
	{
		float shadowPower;
		float4 cascadeWeights = GET_CASCADE_WEIGHTS(worldPos);
		bool inside = dot(cascadeWeights, float4(1, 1, 1, 1)) < 4;

		UNITY_BRANCH
		if (inside)
		{
			float4 samplePos = GET_SHADOW_COORDINATES(float4(worldPos, 1.0), cascadeWeights);
			shadowPower = UNITY_SAMPLE_SHADOW(_WeatherMakerShadowMapTexture, samplePos.xyz);
		}
		else
		{
			shadowPower = 1.0;
		}
		
#if defined(WEATHER_MAKER_SHADOWS_DEPTH_EXTERNAL_FUNC)

		shadowPower = WEATHER_MAKER_SHADOWS_DEPTH_EXTERNAL_FUNC(worldPos, dirIndex, shadowPower, false);

#else

		shadowPower = min(shadowPower, weatherMakerGlobalShadow);

#endif

		shadowPower = lerp(1.0, shadowPower, _WeatherMakerDirLightPower[dirIndex].z);
		return min(shadowPower, weatherMakerGlobalShadow);
	}
	else
	{
		return weatherMakerGlobalShadow;
	}

#else

	return weatherMakerGlobalShadow;

#endif

}

fixed CalculateDirLightScreenShadowPower(float2 uv)
{
	// in case no screen space shadows, use min of weatherMakerGlobalShadow when tex lookup will give 1
	return min(weatherMakerGlobalShadow, WM_SAMPLE_FULL_SCREEN_TEXTURE(_WeatherMakerShadowMapSSTexture, uv).r);
}

fixed CalculateSunVolumetricShadowPower
(
	int sampleCount,
	float3 worldPos,
	float3 rayDir,
	float ditherMultiplier,
	float depth,
	float maxDistance,
	float shadowPower,
	float shadowPowerFade,
	float minShadowStrength
)
{

#if !defined(WEATHER_MAKER_DEPTH_SHADOWS_OFF) && (defined(WEATHER_MAKER_SHADOWS_SOFT) || defined(WEATHER_MAKER_SHADOWS_HARD))

	if (_WeatherMakerShadowsEnabled && shadowPower > 0.001 && _WeatherMakerSunLightPower.z > 0.001 && sampleCount >= 1)
	{
		float4 cascadeWeights;
		float4 samplePos;
		float2 ditherXY = worldPos.xz;
		fixed shadowStrength = 0.0;
		float4 pos = float4(worldPos, 1.0);
		float amount = min(maxDistance, depth - distance(WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET, worldPos));
		sampleCount = int(clamp(ceil(amount) * 4.0, 1.0, sampleCount));
		float invSample = 1.0 / sampleCount;
		float dither = frac(cos(dot(ditherXY * _WeatherMakerTime.x, ditherMagic.xy)) * ditherMagic.z);
		dither *= (frac(dot(ditherMagic.xy, ditherXY * ditherMagic.zw)) - 0.5);
		dither = 1.0 + (dither * ditherMultiplier);
		fixed samp;
		float3 step = (rayDir * amount * invSample * dither);

		UNITY_LOOP
		for (int i = 0.0; i < int(sampleCount); i++)
		{
			pos.xyz += step;
			cascadeWeights = GET_CASCADE_WEIGHTS(pos.xyz);
			samplePos = GET_SHADOW_COORDINATES(pos, cascadeWeights);
			shadowStrength += UNITY_SAMPLE_SHADOW(_WeatherMakerShadowMapTexture, samplePos);
		}
		shadowStrength *= invSample;
		shadowStrength = _WeatherMakerSunLightPower.w + (shadowStrength * _WeatherMakerSunLightPower.z);
		return max(minShadowStrength, pow(shadowStrength, shadowPower * min(1.0, amount * shadowPowerFade)));
	}
	else

#endif

	{
		return 1.0;
	}
}

#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS) && !defined(WEATHER_MAKER_LIGHT_NO_SPECULAR)

fixed3 CalculateSpecularColor(fixed4 lightColor, float3 lightDir, fixed atten, wm_world_space_light_params p)
{
	// Phong
	//float3 reflectionVector = reflect(lightDir, p.worldNormal);
	//float4 specularTerm = pow(saturate(dot(reflectionVector, p.rayDir)), p.specularPower);

	// Blinn-Phong
	float3 halfVector = normalize(lightDir - p.rayDir);
	fixed halfVectorDot = max(0.0, dot(p.worldNormal, halfVector));

#if defined(WEATHER_MAKER_LIGHT_SPECULAR_TRANSLUCENT)

	halfVectorDot = max(halfVectorDot, dot(lightDir, p.rayDir));

#endif

	fixed specularTerm =  pow(halfVectorDot, p.specularPower);
	fixed3 specularColor = p.specularColor;

#if defined(WEATHER_MAKER_LIGHT_SPECULAR_SPARKLE)

	UNITY_BRANCH
	if (atten > 0.0 && p.sparkleScale.z <= 1.0 && p.sparkleScale.w > 0.0)
	{
		// sparkleFade = scale, speed, threshold, intensity
		float3 worldPos = p.worldPos;
		fixed distanceFade = min(1.0, p.sparkleFade.y / p.distanceToWorldPosSquared);
		float3 offset = (p.rayDir * p.sparkleOffset.x) +
			(p.rayDir * p.sparkleOffset.y) +
			(p.rayDir * p.sparkleOffset.z) +
			(p.rayDir * p.sparkleOffset.w);
		worldPos += offset;
		float3 samplePos = (worldPos * p.sparkleScale.x) + (p.rayDir * p.sparkleScale.y * _WeatherMakerTime.x);
		fixed sparkles = genericNoise3D(samplePos);
		fixed fade = min(1.0, 1.0 - (p.sparkleScale.z - sparkles));
		fade = distanceFade * p.sparkleScale.w * (((p.sparkleFade.x <= 0.0) * (sparkles >= p.sparkleScale.z)) + ((p.sparkleFade.x > 0.0) * pow(fade, p.sparkleFade.x)));
		sparkles *= fade;
		specularColor += (sparkles * p.sparkleTintColor);
	}

#endif

	fixed intensity = min(lightColor.a, lightColor.a * lightColor.a);
	return (lightColor.rgb * specularColor.rgb * (specularTerm * atten * intensity * (p.specularPower > 0.0)));
}

#endif

void CalculateDirLightColorWorldSpace(wm_world_space_light_params p, inout fixed3 diffuseLight, inout fixed3 specularLight)
{

#if !defined(WEATHER_MAKER_LIGHT_NO_DIR_LIGHT)

	UNITY_LOOP
	for (uint dirIndex = 0; dirIndex < uint(_WeatherMakerDirLightCount); dirIndex++)
	{

#if defined(WEATHER_MAKER_FILTER_LIGHT_ORTHOGRAPHIC) && defined(ORTHOGRAPHIC_MODE)

		UNITY_BRANCH
		if (_WeatherMakerDirLightVar1[dirIndex].y == 0.0)
		{
			continue;
		}

#endif

		fixed intensity = _WeatherMakerDirLightColor[dirIndex].a;
		fixed diffuseTerm;
		fixed shadowPower = 1.0;

#if defined(WEATHER_MAKER_SHADOWS_SCREEN) || (!defined(WEATHER_MAKER_DEPTH_SHADOWS_OFF) && !defined(ORTHOGRAPHIC_MODE))

		UNITY_BRANCH
		if (p.shadowStrength >= 0.01)
		{

#if defined(WEATHER_MAKER_SHADOWS_SCREEN)

			shadowPower = CalculateDirLightScreenShadowPower(p.uv);

#else

			shadowPower = CalculateDirLightDepthShadowPower(p.worldPos, dirIndex);

#endif
			
			// raise shadow power towards 1 as shadow strength reduces
			shadowPower = lerp(1.0, shadowPower, p.shadowStrength);

			diffuseTerm = intensity * shadowPower;

			// specular gets reduced quickly if any shadow
			shadowPower = saturate((shadowPower - 0.2) * 1.25);
			shadowPower *= shadowPower;
			shadowPower *= shadowPower;
			shadowPower *= shadowPower;
		}
		else if (p.shadowStrength < 0.0)
		{
			// no shadows
			diffuseTerm = intensity;
		}
		else
		{
			// global shadows
			diffuseTerm = intensity * weatherMakerGlobalShadow;
		}

#else

		diffuseTerm = intensity * lerp(1.0, weatherMakerGlobalShadow, p.shadowStrength >= 0.0);

#endif

		fixed atten = CalculateDirLightAtten(_WeatherMakerDirLightPosition[dirIndex].xyz

#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS)

			, p.worldNormal

#endif

		);
		diffuseTerm *= atten;

#if !defined(WEATHER_MAKER_LIGHT_NO_DIFFUSE)

		diffuseLight += (_WeatherMakerDirLightColor[dirIndex].rgb * diffuseTerm);

#endif

#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS) && !defined(WEATHER_MAKER_LIGHT_NO_SPECULAR)

		specularLight += CalculateSpecularColor(_WeatherMakerDirLightColor[dirIndex], _WeatherMakerDirLightPosition[dirIndex].xyz, atten * shadowPower * _WeatherMakerDirLightPower[dirIndex].z, p);

#endif

	}

#endif

}

void CalculatePointLightColorWorldSpace(wm_world_space_light_params p, inout fixed3 diffuseLight, inout fixed3 specularLight)
{
	UNITY_LOOP
	for (int pointIndex = 0; pointIndex < _WeatherMakerPointLightCount; pointIndex++)
	{
		float3 lightDir = normalize(_WeatherMakerPointLightPosition[pointIndex].xyz - p.worldPos);
		fixed pointAtten = CalculatePointLightAtten(p.worldPos,

#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS)

			p.worldNormal,

#endif

			_WeatherMakerPointLightPosition[pointIndex].xyz, _WeatherMakerPointLightAtten[pointIndex]);

#if !defined(WEATHER_MAKER_LIGHT_NO_DIFFUSE)

		diffuseLight += (_WeatherMakerPointLightColor[pointIndex].rgb * (pointAtten * _WeatherMakerPointLightColor[pointIndex].a));

#endif

#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS) && !defined(WEATHER_MAKER_LIGHT_NO_SPECULAR)

		specularLight += CalculateSpecularColor(_WeatherMakerPointLightColor[pointIndex], lightDir, pointAtten, p);

#endif

	}
}

void CalculateSpotLightColorWorldSpace(wm_world_space_light_params p, inout fixed3 diffuseLight, inout fixed3 specularLight)
{
	UNITY_LOOP
	for (int spotIndex = 0; spotIndex < _WeatherMakerSpotLightCount; spotIndex++)
	{
		float3 lightDir = normalize(_WeatherMakerSpotLightPosition[spotIndex].xyz - p.worldPos);
		fixed spotAtten = CalculateSpotLightAtten(p.worldPos,

#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS)

			p.worldNormal,

#endif

			_WeatherMakerSpotLightPosition[spotIndex].xyz, _WeatherMakerSpotLightDirection[spotIndex].xyz, _WeatherMakerSpotLightAtten[spotIndex]);

#if !defined(WEATHER_MAKER_LIGHT_NO_DIFFUSE)

		diffuseLight += (_WeatherMakerSpotLightColor[spotIndex].rgb * (spotAtten * _WeatherMakerSpotLightColor[spotIndex].a));

#endif

#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS) && !defined(WEATHER_MAKER_LIGHT_NO_SPECULAR)

		specularLight += CalculateSpecularColor(_WeatherMakerSpotLightColor[spotIndex], lightDir, spotAtten, p);

#endif

	}
}

void CalculateAreaLightColorWorldSpace(wm_world_space_light_params p, inout fixed3 diffuseLight, inout fixed3 specularLight)
{

#if !defined(WEATHER_MAKER_DISABLE_AREA_LIGHTS)

	UNITY_LOOP
	for (int areaIndex = 0; areaIndex < _WeatherMakerAreaLightCount; areaIndex++)
	{
		float3 lightDir = normalize(_WeatherMakerAreaLightPosition[areaIndex].xyz - p.worldPos);
		fixed areaAtten = CalculateAreaLightAtten(p.worldPos,


#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS)

			p.worldNormal,

#endif

			_WeatherMakerAreaLightPosition[areaIndex].xyz, _WeatherMakerAreaLightDirection[areaIndex],
			_WeatherMakerAreaLightRotation[areaIndex], _WeatherMakerAreaLightMinPosition[areaIndex].xyz,
			_WeatherMakerAreaLightMaxPosition[areaIndex].xyz, _WeatherMakerAreaLightAtten[areaIndex]);

#if !defined(WEATHER_MAKER_LIGHT_NO_DIFFUSE)

		diffuseLight += (_WeatherMakerAreaLightColor[areaIndex].rgb * (areaAtten * _WeatherMakerAreaLightColor[areaIndex].a));

#endif

#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS) && !defined(WEATHER_MAKER_LIGHT_NO_SPECULAR)

		specularLight += CalculateSpecularColor(_WeatherMakerAreaLightColor[areaIndex], lightDir, areaAtten, p);

#endif

	}

#endif

}

fixed3 CalculateLightColorWorldSpace(wm_world_space_light_params p)
{
	fixed3 diffuseLight = float3Zero;
	fixed shadowStrength = 1.0;
	fixed3 specularLight = float3Zero;

	CalculateDirLightColorWorldSpace(p, diffuseLight, specularLight);
	CalculatePointLightColorWorldSpace(p, diffuseLight, specularLight);
	CalculateSpotLightColorWorldSpace(p, diffuseLight, specularLight);
	CalculateAreaLightColorWorldSpace(p, diffuseLight, specularLight);

#if defined(WEATHER_MAKER_LIGHT_NO_DIFFUSE)

	return specularLight;

#else

	diffuseLight += (p.ambientColor.rgb * _AmbientLightMultiplier);
	diffuseLight *= p.diffuseColor;

#if !defined(WEATHER_MAKER_LIGHT_NO_NORMALS) && !defined(WEATHER_MAKER_LIGHT_NO_SPECULAR)

	diffuseLight += specularLight;

#endif

	return diffuseLight;

#endif

}

// pass specular.a of 0 to auto-detect roughness from gbuffer
fixed3 ComputeReflectionColor(float3 worldPos, float3 rayDir, float3 normal, float2 screenUV, fixed4 specular)
{

#if defined(WEATHER_MAKER_ENABLE_TEXTURE_DEFINES)

	// auto-detect specular
	UNITY_BRANCH
	if (specular.a <= 0.0)
	{
		specular = WM_SAMPLE_FULL_SCREEN_TEXTURE(_CameraGBufferTexture1, screenUV);
		specular.a = saturate(UNITY_SPECCUBE_LOD_STEPS * specular.a);
	}

#endif

	float3 reflVector = reflect(rayDir, normal);

#if (UNITY_SPECCUBE_BOX_PROJECTION)

	float blendDistance = unity_SpecCube1_ProbePosition.w;
	// For box projection, use expanded bounds as they are rendered; otherwise
	// box projection artifacts when outside of the box.
	float4 boxMin = unity_SpecCube0_BoxMin - float4(blendDistance, blendDistance, blendDistance, 0);
	float4 boxMax = unity_SpecCube0_BoxMax + float4(blendDistance, blendDistance, blendDistance, 0);
	reflVector = BoxProjectedCubemapDirection(reflVector, worldPos, unity_SpecCube0_ProbePosition, boxMin, boxMax);

#endif

	float4 val0 = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflVector, specular.a);
	fixed3 reflCol0 = DecodeHDR(val0, unity_SpecCube0_HDR);

#if (UNITY_SPECCUBE_BLENDING)

	float4 val1 = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(unity_SpecCube1, unity_SpecCube0, reflVector, specular.a);
	fixed3 reflCol1 = DecodeHDR(val1, unity_SpecCube1_HDR);
	return specular.rgb * ((reflCol0 * unity_SpecCube0_BoxMin.w) + (reflCol1 * (1.0 - unity_SpecCube0_BoxMin.w)));

#else

	return specular.rgb * reflCol0;

#endif

}

fixed3 ComputeDirLightShaftColor(float2 screenUV, fixed fogFactor, float4 viewportPos, fixed4 shaftColor, fixed4 pixelColor)
{
	fixed3 color = fixed3Zero;
	fixed3 shaftRGB = shaftColor.rgb * min(1.0, shaftColor.a * shaftColor.a * 10.0);

#if defined(WEATHER_MAKER_IS_FULL_SCREEN_EFFECT) && defined(WEATHER_MAKER_ENABLE_TEXTURE_DEFINES)

	UNITY_BRANCH
	if (WM_FOG_SUN_SHAFT_ENABLE && WM_CAMERA_RENDER_MODE_NORMAL && _WeatherMakerDirLightCount > 0 && _WeatherMakerFogSunShaftsParam1.y > 0 && viewportPos.z >= 0.0)
	{

#if defined(UNITY_COLORSPACE_GAMMA)

		static const float shaftBrightness = _WeatherMakerFogSunShaftsParam1.z * 0.3;

#else

		static const float shaftBrightness = _WeatherMakerFogSunShaftsParam1.z;

#endif

		// adjust UV coordinate if needed
		float2 uvViewportPos = AdjustFullScreenUV(viewportPos.xy);

		// determine how much to march each step - using the spread parameter (_WeatherMakerFogSunShaftsParam1.x) we can change the length of the sun-shafts
		float2 uvMarch = (uvViewportPos - screenUV) * _WeatherMakerFogSunShaftsParam1.x;

		float dither = frac(cos(dot(screenUV * _WeatherMakerTime.x, ditherMagic.xy)) * ditherMagic.z);
		dither *= (frac(dot(_WeatherMakerFogSunShaftsDitherMagic.xy, screenUV * _WeatherMakerFogSunShaftsDitherMagic.zw)) - 0.5);

		// adjust uv direction by dither
		uvMarch *= (1.0 + (_WeatherMakerFogSunShaftsParam2.z * dither));

		// start off with full step multiplier
		fixed stepMultiplier = _WeatherMakerFogSunShaftsParam2.x;

		fixed3 rgb;

		// ray march sample count times
		UNITY_LOOP
		for (int i = 0; i < int(_WeatherMakerFogSunShaftsParam1.y); i++)
		{
			// march from center of sun pixel towards target pixel
			screenUV += uvMarch;

			// read target and sum up all colors and average them
			rgb = WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex2, screenUV.xy).rgb;

			UNITY_BRANCH
			if (_WeatherMakerFogSunShaftsBackgroundIntensity > 0.0)
			{
				rgb += (WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex5, screenUV.xy).rgb * _WeatherMakerFogSunShaftsBackgroundTintColor * _WeatherMakerFogSunShaftsBackgroundIntensity);
			}

			// apply color using step multiplier and multiply by inverse sample count * weight
			color += (rgb * stepMultiplier);

			// decrease step multiplier by decay
			stepMultiplier *= _WeatherMakerFogSunShaftsParam2.y;
		}

		// multiply final color by brightness multiplier and reduce by fog factor and reduce for very thin fog factor
		fixed fogFactorReducer1 = (1.0 - fogFactor);
		fixed fogFactorReducer2 = min(1.0, lerp(0.0, 1.0, fogFactor * fogFactor * 1000.0));
		color *= shaftRGB * _WeatherMakerFogSunShaftsParam1.w * _WeatherMakerFogSunShaftsTintColor.rgb * viewportPos.w * shaftBrightness * fogFactorReducer1 * fogFactorReducer2;
	}

#endif

	return color;
}

#endif
