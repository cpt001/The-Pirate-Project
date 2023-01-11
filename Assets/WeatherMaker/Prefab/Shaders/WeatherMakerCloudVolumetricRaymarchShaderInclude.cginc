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

#ifndef __WEATHER_MAKER_CLOUD_VOLUMETRIC_RAYMARCH_SHADER__
#define __WEATHER_MAKER_CLOUD_VOLUMETRIC_RAYMARCH_SHADER__

#include "WeatherMakerCloudVolumetricSamplingShaderInclude.cginc"
#include "WeatherMakerCloudVolumetricLightingShaderInclude.cginc"
#include "WeatherMakerCloudVolumetricAtmosphereShaderInclude.cginc"
#include "WeatherMakerCoreShaderInclude.cginc"

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
	CloudState state,
	inout uint marches,
	out fixed horizonFade
)
{
	fixed4 cloudColor = fixed4Zero;
	float startOpticalDepth = min(1.0, distanceToCloud * invVolumetricMaxOpticalDistance);
	horizonFade = ComputeCloudColorVolumetricHorizonFade(startOpticalDepth);

	// if no night multiplier, we can early exit as the sky will be mapped to this pixel
	UNITY_BRANCH
	if (_WeatherMakerNightMultiplier == 0.0 && volumetricIsAboveClouds == 0.0 && horizonFade < 0.001)
	{
		return fixed4Zero;
	}

	uint i = 0;
	float skyAmbientMultiplier = 1.0;// clamp(startOpticalDepth * VOLUMETRIC_SKY_AMBIENT_OPTICAL_DEPTH_MULTIPLIER, 0.5, 1.0);
	float absRayY = abs(rayDir.y);
	uint sampleCount = uint(lerp(volumetricSampleCountRange.y, volumetricSampleCountRange.x, absRayY));
	float invSampleCount = 1.0 / float(sampleCount);
	//float ditherRay = abs(RandomFloat(marchPos)) * lerp(_CloudRayDitherVolumetric.y, _CloudRayDitherVolumetric.x, pow(absRayY, 1.5));
	float ditherRay = state.dithering.b * lerp(_CloudRayDitherVolumetric.y, _CloudRayDitherVolumetric.x, pow(absRayY, 1.5));

	// if ray-march x or y is less than 1, it is considered a percentage of the ray length step unit
	float marchLengthStepMultiplierPercent = rayLength * invSampleCount * _CloudRaymarchMultiplierVolumetric;
	float marchLength = lerp((marchLengthStepMultiplierPercent * _CloudRayMarchParameters.x), _CloudRaymarchMultiplierVolumetric * _CloudRayMarchParameters.x, _CloudRayMarchParameters.x > 1.0);
	float marchLengthFull = lerp((marchLengthStepMultiplierPercent * _CloudRayMarchParameters.y), _CloudRaymarchMultiplierVolumetric * _CloudRayMarchParameters.y, _CloudRayMarchParameters.y > 1.0);
	marchLength = clamp(marchLength, VOLUMETRIC_MIN_STEP_LENGTH, VOLUMETRIC_MAX_STEP_LENGTH);
	marchLengthFull = clamp(marchLengthFull, VOLUMETRIC_MIN_STEP_LENGTH, VOLUMETRIC_MAX_STEP_LENGTH);

	float3 marchDirLong = (rayDir * marchLengthFull);
	float3 marchDirShort = (rayDir * marchLength);
	float3 marchDir;
	marchPos += (rayDir * ditherRay * 256.0);
	float heightFrac = 0.0;
	float cloudSample = 0.0;
	float cloudSampleTotal = 0.0;
	float4 lightSample;
	float4 weatherData;
	float marchLerp;
	float3 t, s;
	bool sampled;

	// increase lod for clouds that are farther away
	float lod = min(volumetricLod.y, volumetricLod.x + (startOpticalDepth * VOLUMETRIC_LOD_OPTICAL_DEPTH_MULTIPLIER));
	float marchLerpPower = lerp(_CloudRayMarchParameters.z, _CloudRayMarchParameters.w, startOpticalDepth * startOpticalDepth);
	uint marchLerpIndex = 0;
	float sdf;
	float3 sdfDir = (rayDir * _WeatherMakerWeatherMapScale.w) / max(0.01, length(rayDir.xz));
	float marchMultiplier = 1.0;

#if defined(VOLUMETRIC_CLOUD_ENABLE_AMBIENT_SKY_DENSITY_SAMPLE)

	float3 ambientPos;

#endif

	UNITY_LOOP
	while (i++ < sampleCount && cloudColor.a < VOLUMETRIC_CLOUD_MAX_ALPHA && heightFrac >= -0.01 && heightFrac <= 1.01)
	{
		heightFrac = GetCloudHeightFractionForPoint(marchPos);

		UNITY_BRANCH
		if (heightFrac <= _CloudTypeVolumetric)
		{
			weatherData = CloudVolumetricSampleWeather(marchPos + (volumetricWindDir2 * heightFrac), heightFrac, lod);

			// min coverage
			UNITY_BRANCH
			if (CloudVolumetricGetCoverage(weatherData) > _CloudCoverVolumetricMinimumForCloud)
			{
				cloudSample = SampleCloudDensity(marchPos, weatherData, heightFrac, lod, false, sampled);

				// soft particles
				UNITY_BRANCH
				if (cloudSample > VOLUMETRIC_CLOUD_MIN_NOISE_VALUE && depth < _ProjectionParams.z)
				{
					float partZ = distance(marchPos, WEATHER_MAKER_CLOUD_CAMERA_POS);
					float diff = (depth - partZ);
					float multiplier = saturate(_CloudInvFade * diff);

					// if we have gotten close enough or beyond the depth buffer, we are done
					i = lerp(sampleCount, i, multiplier > 0.001);

					// adjust cloud sample
					cloudSample *= multiplier;
				}

				// denote expensive march performed
				marches += sampled;

				// march at reduced march speed when maybe in cloud
				marchMultiplier = _CloudRaymarchMaybeInCloudStepMultiplier;

				UNITY_BRANCH
				if (cloudSample > VOLUMETRIC_CLOUD_MIN_NOISE_VALUE)
				{
					// sample just details using the shape noise from the above call which was done without details
					cloudSample = SampleCloudDensityDetails(cloudSample, marchPos, heightFrac, weatherData, lod);

					// do we still have a cloud?
					UNITY_BRANCH
					if (cloudSample > VOLUMETRIC_CLOUD_MIN_NOISE_VALUE)
					{
						// march at reduced march speed when in cloud
						marchMultiplier = _CloudRaymarchInCloudStepMultiplier;
						cloudSampleTotal += cloudSample;
						lightSample.rgb = SampleAmbientLight(marchPos, rayDir, rayLength, skyAmbientMultiplier, skyColor, heightFrac, weatherData);
						lightSample.rgb += SampleDirLightSources(marchPos, rayDir, heightFrac, cloudSample, cloudSampleTotal, lod, state);
						lightSample.rgb += SamplePointLightSources(marchPos, rayDir, heightFrac, cloudSample, cloudSampleTotal, lod, uv);
						lightSample.a = min(1.0, cloudSample);

						/*
						float depth01 = distance(WEATHER_MAKER_CLOUD_CAMERA_POS, marchPos) * atmosphere01;
						s = UNITY_SAMPLE_TEX3D_LOD(_WeatherMakerInscatteringLUT, float3(uv.x, uv.y, depth01), 0.0);
						t = UNITY_SAMPLE_TEX3D_LOD(_WeatherMakerExtinctionLUT, float3(uv.x, uv.y, depth01), 0.0);
						lightSample.rgb *= lerp(t, fixed3One, state.fade);
						lightSample.rgb += s;
						*/

						lightSample.rgb *= lightSample.a;

						// accumulate color
						cloudColor += ((1.0 - cloudColor.a) * lightSample);
					}
				}
			}
			else if ((sdf = CloudVolumetricGetDistance(weatherData)) < 0.99)
			{
				// flip back to pixel space, protect against 0 sdf values
				sdf = round(1.0 / max(sdf, 0.01));

				// march to next sdf position
				marchPos += (sdf * sdfDir);
			}
		}

		// increase march based on march index and power
		marchLerp = pow(saturate(float(marchLerpIndex++) * invSampleCount), marchLerpPower);
		marchDir = lerp(marchDirShort, marchDirLong, marchLerp);
		marchPos += (marchDir * marchMultiplier);
		marchMultiplier = 1.0;
	}

	// tidy up last 0.01 of alpha
	cloudColor.a = min(1.0, cloudColor.a * VOLUMETRIC_CLOUD_MAX_ALPHA_INV);
	
	UNITY_BRANCH
	if (horizonFade > 0.0)
	{
		// reduce horizon fade for bright values, these cut through the sky scattering better, think white parts of clouds at horizon
		fixed cloudLuminosity = min(1.0, Luminance(cloudColor.rgb * cloudColor.a));

		// reduce luminosity power
		cloudLuminosity = pow(cloudLuminosity, _CloudHorizonFadeVolumetric.x);

		// bulk back up for any luminosity that remains
		cloudLuminosity *= 1.5;

		// luminosity fights through horizon fade
		horizonFade = lerp(horizonFade, 1.0, min(1.0, horizonFade * cloudLuminosity));
	}

	return cloudColor;
}

CloudColorResult ComputeCloudColorVolumetric(float3 rayDir, float4 uv, float depth, float depth01, inout CloudState state)
{
	// perturb ray y as we are away from the cloud layer
	float offset = lerp(_CloudRayOffsetVolumetric, 0.0, volumetricBelowCloudsSquared);
	float3 cloudRayDir = normalize(float3(rayDir.x, rayDir.y + offset, rayDir.z));
	float3 marchPos, marchPos2;
	float3 endPos, endPos2;
	float rayLength, rayLength2;
	float distanceToSphere, distanceToSphere2;
	fixed tmpHorizonFade;

	// determine what (if any) part of the cloud volume we intersected
	uint iterations = SetupCloudRaymarch(WEATHER_MAKER_CLOUD_CAMERA_POS, cloudRayDir, depth, depth,
		marchPos, endPos, rayLength, distanceToSphere, marchPos2, endPos2, rayLength2, distanceToSphere2);

	UNITY_BRANCH
	if (iterations > 0)
	{
		iterations = (volumetricIsAboveMiddleClouds ? 1 : iterations);
		float horizonFade = 1.0;
		fixed4 cloudLightColors[2] = { fixed4Zero, fixed4Zero };
		uint marches = 0;
		fixed3 skyColor = volumetricCloudAmbientColorSky;

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
				state,
				marches,
				tmpHorizonFade
			);

			// if we hit back half of sphere, reduce horizon fade by the front part alpha
			horizonFade = (iterationIndex == 0 ? tmpHorizonFade : lerp(tmpHorizonFade, horizonFade, cloudLightColors[0].a));

			// if we have enough cloud, exit the loop
			iterationIndex = lerp(iterationIndex, 2, cloudLightColors[iterationIndex].a >= 0.999);
		}

		// custom blend
		cloudLightColors[1].rgb = (cloudLightColors[0].rgb + (cloudLightColors[1].rgb * (1.0 - cloudLightColors[0].a)));
		cloudLightColors[1].a = max(cloudLightColors[0].a, cloudLightColors[1].a);
		fixed4 finalColor = FinalizeVolumetricCloudColor(cloudLightColors[1] * _CloudColorVolumetric, uv, marches);
		CloudColorResult result = { finalColor, horizonFade, 1.0 };
		return result;
	}
	else
	{
		// missed cloud layer entirely
		CloudColorResult result = { fixed4Zero, 1.0, 0.0 };
		return result;
	}
}

#endif // __WEATHER_MAKER_CLOUD_VOLUMETRIC_RAYMARCH_SHADER__
