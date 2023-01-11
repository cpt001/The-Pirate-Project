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

#ifndef __WEATHER_MAKER_CLOUD_VOLUMETRIC_SHADOWS_SHADER__
#define __WEATHER_MAKER_CLOUD_VOLUMETRIC_SHADOWS_SHADER__

#include "WeatherMakerCloudShaderInclude.cginc"
#include "WeatherMakerCloudVolumetricSamplingShaderInclude.cginc"

float ComputeCloudShadowDetails(float3 worldPos, uint dirIndex, float shadow, float lod)
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
		// take advantage of the fact that dir lights are supported by perspective/ortho and then by intensity
		UNITY_BRANCH
		if (_WeatherMakerCloudVolumetricShadowSampleCount > 0 && dirIndex < uint(_WeatherMakerDirLightCount) &&
			_WeatherMakerDirLightVar1[dirIndex].y == 0.0 && _WeatherMakerDirLightColor[dirIndex].a > 0.0)
		{
			float3 rayDir = _WeatherMakerDirLightPosition[dirIndex].xyz;
			float worldPosDistance = distance(worldPos, WEATHER_MAKER_CLOUD_CAMERA_POS);
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
				float rayY = max(0.015, rayDir.y);
				float3 cloudRayDir = normalize(float3(rayDir.x, rayY, rayDir.z));
				SetupCloudRaymarch(worldPos, cloudRayDir, 1000000.0, 0.0,
					startPos, endPos, rayLength, distanceToSphere, startPos2, endPos2, rayLength2, distanceToSphere2);

				float cloudCoverage = 0.0;
				float3 marchPos = startPos;
				float heightFrac;
				float4 weatherData;
				float cloudType;
				float dither = RandomFloat(worldPos);
				float randomDither = 1.0 + (_WeatherMakerCloudVolumetricShadowDither * dither);
				float3 marchDir = rayDir * min(VOLUMETRIC_CLOUD_SHADOW_MAX_RAY_LENGTH, rayLength) * volumetricCloudShadowSampleCountInv;
				marchPos += (marchDir * 0.15);
				float samp;
				bool sampled;

				UNITY_LOOP
				for (uint i = 0; i < _WeatherMakerCloudVolumetricShadowSampleCount && cloudCoverage < VOLUMETRIC_CLOUD_MAX_ALPHA; i++, marchPos += marchDir)
				{
					heightFrac = GetCloudHeightFractionForPoint(marchPos);
					//weatherData = CloudVolumetricSampleWeatherGaussian17(marchPos, heightFrac, _WeatherMakerCloudVolumetricShadowLod);
					weatherData = CloudVolumetricSampleWeather(marchPos, heightFrac, _WeatherMakerCloudVolumetricShadowLod);
					cloudType = CloudVolumetricGetType(weatherData);
					//samp = (CloudVolumetricGetCoverage(weatherData) * GetDensityHeightGradientForHeight(heightFrac, cloudType).x);
					samp = SampleCloudDensity(marchPos, weatherData, heightFrac, _WeatherMakerCloudVolumetricShadowLod, false, sampled);
					samp += _CloudShadowMapAdder;
					samp *= _CloudShadowMapMultiplier;
					samp = saturate(samp);
					samp = pow(samp, _CloudShadowMapPower);
					samp *= randomDither;
					cloudCoverage += samp;
				}
				cloudCoverage *= VOLUMETRIC_CLOUD_MAX_ALPHA_INV;
				fixed flatCoverage = ComputeFlatCloudShadows(rayDir, worldPos, _WeatherMakerCloudVolumetricShadowLod);
				cloudCoverage = min(1.0, cloudCoverage + flatCoverage);

				// apply cloud detail shadows if desired
				UNITY_BRANCH
				if (sampleDetails && cloudCoverage > 0.0 && cloudCoverage < 1.0 && volumetricCloudShadowDetailIntensity > 0.0)
				{
					fixed cloudShadowDetailSample = tex2Dlod(_WeatherMakerCloudShadowDetailTexture, float4((worldPos.xz + _CloudCoverageVelocity.xz + _CloudCoverageOffset.xz) * _WeatherMakerCloudShadowDetailScale, 0.0, _WeatherMakerCloudVolumetricShadowLod)).a;
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
	}

	return shadowValue;
}

#endif // __WEATHER_MAKER_CLOUD_VOLUMETRIC_SHADOWS_SHADER__
