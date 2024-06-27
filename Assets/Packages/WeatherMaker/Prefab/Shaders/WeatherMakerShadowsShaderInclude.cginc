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

#ifndef __WEATHER_MAKER_SHADOWS_SHADER__
#define __WEATHER_MAKER_SHADOWS_SHADER__

#include "UnityShadowLibrary.cginc"

#if defined(UNITY_URP)

#define MAX_SHADOW_CASCADES 4

UNITY_DECLARE_SCREENSPACE_TEXTURE(_ScreenSpaceShadowmapTexture);
UNITY_DECLARE_SHADOWMAP(_MainLightShadowmapTexture);
UNITY_DECLARE_SHADOWMAP(_AdditionalLightsShadowmapTexture);

CBUFFER_START(_MainLightShadowBuffer)
// Last cascade is initialized with a no-op matrix. It always transforms
// shadow coord to half3(0, 0, NEAR_PLANE). We use this trick to avoid
// branching since ComputeCascadeIndex can return cascade index = MAX_SHADOW_CASCADES
float4x4    _MainLightWorldToShadow[MAX_SHADOW_CASCADES + 1];
float4      _CascadeShadowSplitSpheres0;
float4      _CascadeShadowSplitSpheres1;
float4      _CascadeShadowSplitSpheres2;
float4      _CascadeShadowSplitSpheres3;
float4      _CascadeShadowSplitSphereRadii;
half4       _MainLightShadowOffset0;
half4       _MainLightShadowOffset1;
half4       _MainLightShadowOffset2;
half4       _MainLightShadowOffset3;
half4       _MainLightShadowData;    // (x: shadowStrength)
float4      _MainLightShadowmapSize; // (xy: 1/width and 1/height, zw: width and height)
CBUFFER_END

#define _WeatherMakerShadowMapTexture _MainLightShadowmapTexture
#define _WeatherMakerShadowMapTexture_TexelSize _MainLightShadowmapSize;

#define CASCADE_SHADOW_SPLIT_SPHERE0 _CascadeShadowSplitSpheres0
#define CASCADE_SHADOW_SPLIT_SPHERE1 _CascadeShadowSplitSpheres1
#define CASCADE_SHADOW_SPLIT_SPHERE2 _CascadeShadowSplitSpheres2
#define CASCADE_SHADOW_SPLIT_SPHERE3 _CascadeShadowSplitSpheres3
#define CASCADE_SHADOW_RADII _CascadeShadowSplitSphereRadii
#define WORLD_TO_SHADOW0 _MainLightWorldToShadow[0]
#define WORLD_TO_SHADOW1 _MainLightWorldToShadow[2]
#define WORLD_TO_SHADOW2 _MainLightWorldToShadow[2]
#define WORLD_TO_SHADOW3 _MainLightWorldToShadow[3]

#else

UNITY_DECLARE_SHADOWMAP(_WeatherMakerShadowMapTexture);
uniform float4 _WeatherMakerShadowMapTexture_TexelSize;

#define CASCADE_SHADOW_SPLIT_SPHERE0 unity_ShadowSplitSpheres[0]
#define CASCADE_SHADOW_SPLIT_SPHERE1 unity_ShadowSplitSpheres[1]
#define CASCADE_SHADOW_SPLIT_SPHERE2 unity_ShadowSplitSpheres[2]
#define CASCADE_SHADOW_SPLIT_SPHERE3 unity_ShadowSplitSpheres[3]
#define CASCADE_SHADOW_RADII unity_ShadowSplitSqRadii
#define WORLD_TO_SHADOW0 unity_WorldToShadow[0]
#define WORLD_TO_SHADOW1 unity_WorldToShadow[2]
#define WORLD_TO_SHADOW2 unity_WorldToShadow[2]
#define WORLD_TO_SHADOW3 unity_WorldToShadow[3]

#endif

uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_WeatherMakerShadowMapSSTexture);
uniform float4 _WeatherMakerShadowMapSSTexture_TexelSize;

float4 unity_ShadowCascadeScales;

//
// Keywords based defines
//
#undef GET_CASCADE_WEIGHTS
#undef GET_CASCADE_WEIGHTS_Z
#if defined (SHADOWS_SPLIT_SPHERES) || defined(WEATHER_MAKER_SHADOWS_SPLIT_SPHERES)
#define GET_CASCADE_WEIGHTS(wpos)		wm_getCascadeWeights_splitSpheres(wpos)
#define GET_CASCADE_WEIGHTS_Z(wpos, z)  wm_getCascadeWeights_splitSpheres(wpos)
#else
#define GET_CASCADE_WEIGHTS(wpos)		wm_getCascadeWeights( wpos, distance(wpos, WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET) * _ProjectionParams.w )
#define GET_CASCADE_WEIGHTS_Z(wpos, z)  wm_getCascadeWeights( wpos, z )
#endif

#undef GET_SHADOW_COORDINATES
#if defined (SHADOWS_SINGLE_CASCADE)
#define GET_SHADOW_COORDINATES(wpos,cascadeWeights) wm_getShadowCoord_SingleCascade(wpos)
#else
#define GET_SHADOW_COORDINATES(wpos,cascadeWeights) wm_getShadowCoord(wpos,cascadeWeights)
#endif

/**
 * Gets the cascade weights based on the world position of the fragment.
 * Returns a float4 with only one component set that corresponds to the appropriate cascade.
 */
fixed4 wm_getCascadeWeights(float3 wpos, float z)
{
	fixed4 zNear = float4(z >= _LightSplitsNear);
	fixed4 zFar = float4(z < _LightSplitsFar);
	fixed4 weights = zNear * zFar;
	return weights;
}

/**
 * Gets the cascade weights based on the world position of the fragment and the poisitions of the split spheres for each cascade.
 * Returns a float4 with only one component set that corresponds to the appropriate cascade.
 */
fixed4 wm_getCascadeWeights_splitSpheres(float3 wpos)
{
	float3 fromCenter0 = wpos - CASCADE_SHADOW_SPLIT_SPHERE0.xyz;
	float3 fromCenter1 = wpos - CASCADE_SHADOW_SPLIT_SPHERE1.xyz;
	float3 fromCenter2 = wpos - CASCADE_SHADOW_SPLIT_SPHERE2.xyz;
	float3 fromCenter3 = wpos - CASCADE_SHADOW_SPLIT_SPHERE3.xyz;
	float4 distances2 = float4(dot(fromCenter0, fromCenter0), dot(fromCenter1, fromCenter1), dot(fromCenter2, fromCenter2), dot(fromCenter3, fromCenter3));
	fixed4 weights = float4(distances2 < CASCADE_SHADOW_RADII);
	weights.yzw = saturate(weights.yzw - weights.xyz);
	return weights;
}

/**
 * Returns the shadowmap coordinates for the given fragment based on the world position and z-depth.
 * These coordinates belong to the shadowmap atlas that contains the maps for all cascades.
 */
float4 wm_getShadowCoord(float4 wpos, fixed4 cascadeWeights)
{
	float3 sc0 = mul(WORLD_TO_SHADOW0, wpos).xyz;
	float3 sc1 = mul(WORLD_TO_SHADOW1, wpos).xyz;
	float3 sc2 = mul(WORLD_TO_SHADOW2, wpos).xyz;
	float3 sc3 = mul(WORLD_TO_SHADOW3, wpos).xyz;
	float4 shadowMapCoordinate = float4(sc0 * cascadeWeights[0] + sc1 * cascadeWeights[1] + sc2 * cascadeWeights[2] + sc3 * cascadeWeights[3], 1);
#if defined(UNITY_REVERSED_Z)
	float noCascadeWeights = 1 - dot(cascadeWeights, float4(1, 1, 1, 1));
	shadowMapCoordinate.z += noCascadeWeights;
#endif
	return shadowMapCoordinate;
}

/**
 * Same as the getShadowCoord; but optimized for single cascade
 */
float4 wm_getShadowCoord_SingleCascade(float4 wpos)
{
	return float4(mul(WORLD_TO_SHADOW0, wpos).xyz, 0);
}

float wm_sample_shadow_world_pos(float3 worldPos, float maxValue)
{
	float4 worldPos4 = float4(worldPos, 1.0);
	float4 cascadeWeights = GET_CASCADE_WEIGHTS(worldPos4);
	float4 samplePos = GET_SHADOW_COORDINATES(worldPos4, cascadeWeights);
	return max(maxValue, UNITY_SAMPLE_SHADOW(_WeatherMakerShadowMapTexture, samplePos));
}

#define SHAD4MULT1 0.3
#define SHAD4MULT2 0.6
#define SHAD4OFFS1 (_WeatherMakerShadowMapTexture_TexelSize.x * SHAD4MULT1)
#define SHAD4OFFS2 (_WeatherMakerShadowMapTexture_TexelSize.x * SHAD4MULT2)
#define SHAD4OFFS3 (_WeatherMakerShadowMapTexture_TexelSize.y * SHAD4MULT1)
#define SHAD4OFFS4 (_WeatherMakerShadowMapTexture_TexelSize.y * SHAD4MULT2)

#define UNITY_SAMPLE_SHADOW_4(tex, coord) \
	(( \
	UNITY_SAMPLE_SHADOW(tex, (float3(coord.x - SHAD4OFFS1, coord.y - SHAD4OFFS4, coord.z + SHAD4OFFS3))) + \
	UNITY_SAMPLE_SHADOW(tex, (float3(coord.x - SHAD4OFFS2, coord.y + SHAD4OFFS3, coord.z - SHAD4OFFS1))) + \
	UNITY_SAMPLE_SHADOW(tex, (float3(coord.x + SHAD4OFFS2, coord.y - SHAD4OFFS3, coord.z + SHAD4OFFS1))) + \
	UNITY_SAMPLE_SHADOW(tex, (float3(coord.x + SHAD4OFFS1, coord.y + SHAD4OFFS4, coord.z - SHAD4OFFS2))) \
	) * 0.25)

#endif // __WEATHER_MAKER_SHADOWS_SHADER__