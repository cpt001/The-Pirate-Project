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

#ifndef __WEATHER_MAKER_CLOUD_VOLUMETRIC_WEATHER_SHADER__
#define __WEATHER_MAKER_CLOUD_VOLUMETRIC_WEATHER_SHADER__

#include "WeatherMakerCloudVolumetricUniformsShaderInclude.cginc"

float2 AdjustWeatherMapSamplePosUnbounded(float3 worldPos)
{
	float2 pos = worldPos.xz;
	pos -= lerp(WEATHER_MAKER_CLOUD_CAMERA_POS.xz, float2Zero, _WeatherMakerWeatherMapTextureStatic);
	pos *= _WeatherMakerWeatherMapScale.z;
	pos += 0.5; // 0.5, 0.5 is center of weather map at world pos xz of 0,0, as camera moves they will tile through the weather map
	return pos;
}

float2 AdjustWeatherMapSamplePosBounded(float3 worldPos)
{
	float2 volumetricCloudBoxInverseScale = 1.0 / (_WeatherMakerCloudVolumetricWeatherMapRemapBoxMax.xz - _WeatherMakerCloudVolumetricWeatherMapRemapBoxMin.xz);
	float2 uv = worldPos.xz - _WeatherMakerCloudVolumetricWeatherMapRemapBoxMin.xz;
	uv *= volumetricCloudBoxInverseScale;
	return uv;
}

float4 CloudVolumetricSampleWeather(float3 pos, float heightFrac, float lod)
{
	float2 uv = float2(0.0, 0.0);
	float heightFrac2 = saturate(Remap(heightFrac, 0.0, _CloudTypeVolumetric, 0.0, 1.0));

	UNITY_BRANCH
	if (_WeatherMakerCloudVolumetricWeatherMapRemapBoxMin.w == 0.0)
	{
		uv = AdjustWeatherMapSamplePosUnbounded(pos);
	}
	else
	{
		uv = AdjustWeatherMapSamplePosBounded(pos);
	}

	fixed4 weatherData;

	UNITY_BRANCH
	if (uv.x < 0.0 || uv.y < 0.0 || uv.x > 1.0 || uv.y > 1.0)
	{
		weatherData = float4Zero;
	}
	else
	{
		float4 coord = float4(uv, 0.0, lod);
		weatherData = tex2Dlod(_WeatherMakerWeatherMapTexture, coord);
		weatherData.rgb *= float3
		(
			SampleCurve(heightFrac2, _CloudCoverageCurve),
			SampleCurve(heightFrac2, _CloudDensityCurve),
			SampleCurve(heightFrac2, _CloudTypeCurve)
		);
	}

	return weatherData;

	/*
	
	fixed4 linearSample = WM_SAMPLE_TEX2D_SAMPLER_LOD(_WeatherMakerWeatherMapTexture, _linear_mirror_sampler, coord);
	UNITY_BRANCH
	if (linearSample.a > 0.01 && linearSample.a < 0.99)
	{
		// sample texture twice - the alpha value must not be interpolated since it is an sdf
		coord.w = 0.0; // must sample max lod to get correct sdf
		fixed4 pointSample = WM_SAMPLE_TEX2D_SAMPLER_LOD(_WeatherMakerWeatherMapTexture, _point_mirror_sampler, coord);
		linearSample.a = pointSample.a;
	}
	return linearSample;
	*/
}

float CloudVolumetricGetCoverage(float4 weatherData)
{
	return weatherData.r;
}

float CloudVolumetricGetDensity(float4 weatherData)
{
	// weather g channel is cloud density/bulkiness
	return weatherData.g;
}

float CloudVolumetricGetType(float4 weatherData)
{
	// weather b channel tells the cloud type 0.0 = stratus, 0.5 = stratocumulus, 1.0 = cumulus
	return weatherData.b;
}

float CloudVolumetricGetDistance(float4 weatherData)
{
	// weather a channel has sdf value, nearest weathermap pixel with a value
	return weatherData.a;
}

float GetCloudHeightFractionForPoint(float3 worldPos)
{
	UNITY_BRANCH
	if (_CloudPlanetRadiusVolumetric > 0.0)
	{
		return _CloudHeightInverseVolumetric * (distance(worldPos, volumetricPlanetCenter) - _CloudPlanetStartVolumetric);
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

float4 GetDensityHeightGradientForHeight(float heightFrac, float cloudType)
{
	// 0 = fully stratus, 0.5 = fully stratocumulus, 1 = fully cumulus
	float stratus = 1.0f - saturate(cloudType * 2.0f);
	float stratoCumulus = 1.0f - abs(cloudType - 0.5f) * 2.0f;
	float cumulus = saturate((cloudType - 0.5f) * 2.0f);
	float4 cloudGradient = (_CloudGradientStratus * stratus) + (_CloudGradientStratoCumulus * stratoCumulus) + (_CloudGradientCumulus * cumulus);
	return float4(SmoothStepGradient(heightFrac, cloudGradient), stratus, stratoCumulus, cumulus);
}

#endif // __WEATHER_MAKER_CLOUD_VOLUMETRIC_WEATHER_SHADER__
