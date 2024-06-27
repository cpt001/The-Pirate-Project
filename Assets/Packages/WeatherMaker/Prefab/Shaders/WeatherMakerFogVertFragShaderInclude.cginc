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

// receive shadows: http://www.gamasutra.com/blogs/JoeyFladderak/20140416/215612/Let_there_be_shadow.php

#ifndef WEATHER_MAKER_FOG_VERT_FRAG_SHADER_INCLUDED
#define WEATHER_MAKER_FOG_VERT_FRAG_SHADER_INCLUDED

#include "WeatherMakerFogShaderInclude.cginc"

void PreFogFragment(wm_volumetric_data i, out float depth, out float depth01, out float2 screenUV, out float3 rayDir)
{
	// get the depth of this pixel
	screenUV = i.projPos.xy / i.projPos.w;
	depth = WM_SAMPLE_DEPTH(screenUV);
	depth01 = WM_LINEAR_DEPTH_01(depth);
	depth = length(DECODE_EYEDEPTH(depth) / normalize(i.viewPos).z);
	rayDir = normalize(i.rayDir);
}

fixed4 PostFogFragment(float3 startPos, float3 rayDir, float amount, float noise, float2 screenUV, out float fogFactor)
{
	GetNullZonesDepth(startPos, rayDir, amount);
	fogFactor = saturate(CalculateFogFactor(amount) * noise);
	return ComputeFogLighting(startPos, rayDir, amount, fogFactor, screenUV, noise, true);
}

fixed ComputeFullScreenFogAlphaTemporalReprojection(wm_full_screen_fragment i)
{
	UNITY_BRANCH
	if (_WeatherMakerFogMode == 0)
	{
		return 0.0;
	}
	else
	{
		float3 rayOrigin = WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET;
		float3 rayDir = GetFullScreenRayDir(i.forwardLine);
		float depth = WM_SAMPLE_DEPTH_DOWNSAMPLED_TEMPORAL_REPROJECTION_01(i.uv.xy);
		float noise;
		float3 startPos;
		float3 depthPos = rayOrigin + (depth * i.forwardLine);
		depth = distance(rayOrigin, depthPos);
		RaycastFogBoxFullScreen(rayOrigin, rayDir, depth, startPos, noise);
		return saturate(CalculateFogFactorWithDither(depth, i.uv) * noise);
	}
}

wm_volumetric_data fog_volume_vertex_shader(wm_appdata_base v)
{
	return GetVolumetricData(v);
}

fixed4 fog_box_full_screen_fragment_shader(wm_full_screen_fragment i) : SV_Target
{
	UNITY_BRANCH
	if (_WeatherMakerFogMode == 0)
	{
		return fixed4Zero;
	}
	else
	{
		float3 rayOrigin = WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET;
		float3 rayDir = GetFullScreenRayDir(i.forwardLine);
		float depth = WM_SAMPLE_DEPTH_DOWNSAMPLED_TEMPORAL_REPROJECTION_01(i.uv.xy);
		float noise;
		float3 startPos;
		float3 depthPos = rayOrigin + (depth * i.forwardLine);
		depth = distance(rayOrigin, depthPos);
		RaycastFogBoxFullScreen(rayOrigin, rayDir, depth, startPos, noise);
		float fogFactor = saturate(CalculateFogFactorWithDither(depth, i.uv) * noise);
		return ComputeFogLighting(startPos, rayDir, depth, fogFactor, i.uv, noise, true);
	}
}

fixed4 fog_box_fragment(wm_volumetric_data i, out float depth, out float fogFactor, out float3 rayDir)
{
	WM_INSTANCE_FRAG(i);
	float noise;
	float2 screenUV;
	float depth01;
	PreFogFragment(i, depth, depth01, screenUV, rayDir);
	float3 startPos;
	RaycastFogBox(rayDir, i.normal, screenUV, depth, startPos, noise);

	UNITY_BRANCH
	if (_WeatherMakerFogMode == 0)
	{
		fogFactor = 0.0;
		return fixed4Zero;
	}
	else
	{
		return PostFogFragment(startPos, rayDir, depth, noise, screenUV, fogFactor);
	}
}

fixed4 fog_box_fragment_shader(wm_volumetric_data i) : SV_TARGET
{
	WM_INSTANCE_FRAG(i);

	UNITY_BRANCH
	if (_WeatherMakerFogMode == 0)
	{
		return fixed4Zero;
	}

	float depth, fogFactor;
	float3 rayDir;
	return fog_box_fragment(i, depth, fogFactor, rayDir);
}

fixed4 fog_sphere_fragment_shader(wm_volumetric_data i) : SV_TARGET
{
	WM_INSTANCE_FRAG(i);

	UNITY_BRANCH
	if (_WeatherMakerFogMode == 0)
	{
		return fixed4Zero;
	}

	float noise, fogFactor;
	float2 screenUV;
	float3 startPos, rayDir;
	float depth, depth01;
	PreFogFragment(i, depth, depth01, screenUV, rayDir);
	RaycastFogSphere(rayDir, i.normal, depth, startPos, noise);
	return PostFogFragment(startPos, rayDir, depth, noise, screenUV, fogFactor);
}

#endif
