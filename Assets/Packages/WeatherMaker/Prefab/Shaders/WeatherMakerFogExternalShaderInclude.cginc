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

// note: define NULL_ZONE_RENDER_MASK for the type of mask your shader is rendering (see NullZoneScript.cs)

#ifndef WEATHER_MAKER_FOG_EXTERNAL_SHADER_INCLUDED
#define WEATHER_MAKER_FOG_EXTERNAL_SHADER_INCLUDED

#include "WeatherMakerCloudVolumetricShadowsShaderInclude.cginc"
#include "WeatherMakerFogShaderInclude.cginc"

#define EXTERNAL_FOG_PASS_FUNC ComputeWeatherMakerFog
#define EXTERNAL_SHADOW_PASS_FUNC ComputeWeatherMakerShadowsFade

// --------------------------------------------------------------------------------
// External shader integration functions
// --------------------------------------------------------------------------------

// compute fog light using built in weather maker lighting
// color is the current pixel color
// worldPos the world space position of the pixel
// volumetric can be true for volumetric point,spot,area light or false for just dir light
// remarks:
// include these 4 properties in your transparent shader definition or set them somewhere in your shader code:
// Properties
// {
// 	_PointSpotLightMultiplier("Point/Spot Light Multiplier", Range(0, 10)) = 1
// 	_DirectionalLightMultiplier("Directional Light Multiplier", Range(0, 10)) = 1
// 	_AmbientLightMultiplier("Ambient Light Multiplier", Range(0, 10)) = 2
//  _WeatherMakerFogVolumetricLightMode("WeatherMakerFogVolumetricLightMode", Range(0, 2)) = 0 // 0 for no point/spot/area lights, 1 for lights but no shadows, 2 for full light and shadows
// }
fixed4 ComputeWeatherMakerFog
(
	fixed4 color,
	float3 worldPos,
	bool volumetric
);

// compute weather maker shadow term for primary dir light and fog (returns 0 to 1, 0 is full shadow, 1 is no shadow)
// worldPos the world space position of the pixel
// existing shadow is simply the shadow multiplier (0 to 1) if it is known for the pixel, pass 1.0 if it is unknown
// sampleDetails can be true for extra shadow details or false for better performance
float ComputeWeatherMakerShadows
(
	float3 worldPos,
	float existingShadow,
	bool sampleDetails
);

// --------------------------------------------------------------------------------
fixed4 ComputeWeatherMakerFog(fixed4 color, float3 worldPos, bool volumetric)
{
	float3 rayDir = normalize(worldPos - WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET);

	UNITY_BRANCH
	if (_WeatherMakerFogMode == 0 || _WeatherMakerFogDensity == 0)
	{
		return color;
	}
	else
	{
		// fog is a special case where we use the camera pos to get the true depth
		float3 rayOrigin = WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET;
		float depth = distance(rayOrigin, worldPos);
		float3 startPos;
		float noise;
		float2 uv = 0.001 * (worldPos.xz + worldPos.y);
		RaycastFogBoxFullScreen(rayOrigin, rayDir, depth, startPos, noise);
		float fogFactor = saturate(CalculateFogFactorWithDither(depth, uv) * noise);

#if !defined(UNITY_PASS_FORWARDADD)

		float4 fogColor = ComputeFogLighting(startPos, rayDir, depth, fogFactor, uv, noise, volumetric);
		return lerp(color, fogColor, fogFactor);

#else

		return lerp(color, fixed4(0.0, 0.0, 0.0, 0.0), fogFactor);

#endif

	}
}

float ComputeWeatherMakerShadows(float3 worldPos, float existingShadow, bool sampleDetails)
{
	return min(_WeatherMakerFogGlobalShadow, ComputeCloudShadowStrengthTexture(worldPos, 0, existingShadow, sampleDetails));
}

float ComputeWeatherMakerShadowsFade(float3 worldPos, float existingShadow, bool sampleDetails, inout half shadowFade)
{
	shadowFade = 0.0;
	return min(_WeatherMakerFogGlobalShadow, ComputeCloudShadowStrengthTexture(worldPos, 0, existingShadow, sampleDetails));
}

#endif
