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

#ifndef __WEATHER_MAKER_CLOUD_VOLUMETRIC_SAMPLING_SHADER__
#define __WEATHER_MAKER_CLOUD_VOLUMETRIC_SAMPLING_SHADER__

// https://github.com/greje656/clouds

#include "WeatherMakerCloudVolumetricWeatherShaderInclude.cginc"

float SampleCloudDensityDetails(float noise, float3 marchPos, float heightFrac, float4 weatherData, float lod)
{
	// apply details if needed
	UNITY_BRANCH
	if (volumetricPositionDetailScale > 0.0 && noise > VOLUMETRIC_CLOUD_MIN_NOISE_VALUE && _CloudNoiseDetailPowerVolumetric > 0.0)
	{
		marchPos += volumetricAnimationDetail;

		UNITY_BRANCH
		if (volumetricPositionCurlScale > 0.0)
		{
			// modify detail pos using curl lookup
			float4 curlPos = float4(marchPos * volumetricPositionCurlScale, lod);
			float3 curl = (tex3Dlod(_CloudNoiseCurlVolumetric, curlPos).rgb * 2.0) - 1.0; // curl tex is 0-1, map to -1,1
			curl *= volumetricPositionCurlIntensity * (1.0 - (heightFrac * 0.5));
			marchPos += curl;
		}

		float4 noisePos = float4(marchPos * volumetricPositionDetailScale, lod);

		// retrieve detail texture
		float detail = tex3Dlod(_CloudNoiseDetailVolumetric, noisePos).a;

		// apply feathery details lower down
		float detailModifier = lerp(detail, 1.0f - detail, saturate(heightFrac * _CloudNoiseDetailHeightMultiplier));

		float cloudType = CloudVolumetricGetType(weatherData);
		float density = CloudVolumetricGetDensity(weatherData);

		// erode detail from noise
		float heightFracType = saturate(Remap(heightFrac, 0.0, _CloudTypeVolumetric, 0.0, 1.0));
		float detailPowerCurve = SampleCurve(heightFracType, _CloudNoiseDetailPowerCurveVolumetric) * detailModifier * _CloudNoiseDetailPowerVolumetric;
		noise = saturate(Remap(noise, detailPowerCurve, 1.0, 0.0, 1.0) * _CloudNoiseScalarVolumetric * density);

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

float GetCloudRoundness(float3 marchPos, float4 weatherData, float heightFrac)
{
	UNITY_BRANCH
	if (heightFrac < 0.0 || heightFrac > 1.0)
	{
		return 0.0;
	}
	else
	{
		// roundness
		float cloudType = CloudVolumetricGetType(weatherData);
		float4 gradient = GetDensityHeightGradientForHeight(heightFrac, cloudType);
		float cloudRoundness = gradient.x * volumetricPositionShapeScaleExists;

		UNITY_BRANCH
		if (_CloudAnvilStrength > 0.0 && heightFrac >= _CloudAnvilStart)
		{
			// anvil
			float coverage = CloudVolumetricGetCoverage(weatherData);
			float density = CloudVolumetricGetDensity(weatherData);
			float anvilXZ = 1.0 + (0.1 * ((frac(marchPos.x * 0.0001)) + (frac(marchPos.z * 0.0002))));
			float start = _CloudAnvilStart * anvilXZ;
			float anvilTaper = max(0.0, Remap(pow(heightFrac, 0.5), start, 1.0, 0.1, 1.0));
			float anvilStrength = saturate(_CloudAnvilStrength * density * cloudType * anvilTaper);
			cloudRoundness = max(cloudRoundness, anvilStrength);
		}

		return cloudRoundness;
	}
}

float CloudNoiseSampleToCloudNoise(fixed4 noiseSample, float3 marchPos, float heightFrac, float coverage, float noiseMultiplier)
{

#define VOLUMETRIC_CLOUD_SAMPLE_REMAP

	// important! we must modulate shape noise by coverage to greatly reduce repeating patterns in the sky of noise
	// this must be done before applying density or detail modifiers

	float4 noiseSampleWeighted = noiseSample;

#if defined(VOLUMETRIC_CLOUD_SAMPLE_REMAP)

	noiseSampleWeighted.gba = lerp(-(1.0f - noiseSampleWeighted.gba), -noiseSampleWeighted.gba, _CloudWeights.gba >= 0.0);

#endif

	noiseSampleWeighted *= _CloudWeights;

#if !defined(VOLUMETRIC_CLOUD_SAMPLE_REMAP)

	// restrict noise to correct gradient for the height
	float3 heightGradient = float3(SmoothStepGradient(heightFrac, _CloudGradientStratus),
		SmoothStepGradient(heightFrac, _CloudGradientStratoCumulus), SmoothStepGradient(heightFrac, _CloudGradientCumulus));
	noiseSampleWeighted.gba *= heightGradient;

#endif

	float noiseFbm = clamp(noiseSampleWeighted.g + noiseSampleWeighted.b + noiseSampleWeighted.a, -1.0, 1.0);

#if defined(VOLUMETRIC_CLOUD_SAMPLE_REMAP)

	float noise = Remap(noiseSampleWeighted.r, noiseFbm, 1.0, 0.0, 1.0) * noiseMultiplier;
	noise = max(0.0, Remap(noise, 1.0 - coverage, 1.0, 0.0, 1.0));

#else

	float noise = (noiseSampleWeighted.r + noiseFbm) * 0.25 * noiseMultiplier;
	noise = smoothstep(0.1, 0.6, noise);
	noise = saturate(noise - (1.0 - coverage)) * coverage;

#endif

	return noise;
}

float SampleCloudDensity(float3 marchPos, float4 weatherData, float heightFrac, float lod, bool sampleDetails, out bool sampled)
{
	float noise = 0.0;
	float coverage = CloudVolumetricGetCoverage(weatherData);
	float noiseMultiplier = GetCloudRoundness(marchPos, weatherData, heightFrac);
	float maxNoiseWillProduceCloud = Remap(noiseMultiplier, 1.0 - coverage, 1.0, 0.0, 1.0);
	sampled = maxNoiseWillProduceCloud > VOLUMETRIC_CLOUD_MIN_NOISE_VALUE;

	// early exit if multiplier is too low or out of cloud layer
	UNITY_BRANCH
	if (sampled)
	{
		marchPos += (volumetricWindDir1 * _WeatherMakerTime.y);
		float4 noisePos = float4((marchPos + volumetricAnimationShape) * volumetricPositionShapeScale, lod);

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

		float4 noiseSample = tex3Dlod(_CloudNoiseShapeVolumetric, noisePos);

#endif

		noise = CloudNoiseSampleToCloudNoise(noiseSample, marchPos, heightFrac, coverage, noiseMultiplier);

		// apply details if needed, should not even need to branch here as the parameter is a constant for any calls
		if (sampleDetails)
		{
			noise = SampleCloudDensityDetails(noise, marchPos, heightFrac, weatherData, lod);
		}
	}

	return noise;
}

float SampleCloudDensity(float3 marchPos, float4 weatherData, float heightFrac, float lod, bool sampleDetails)
{
	bool sampled;
	return SampleCloudDensity(marchPos, weatherData, heightFrac, lod, sampleDetails, sampled);
}

static const float volumetricCameraHeightFrac = GetCloudHeightFractionForPoint(WEATHER_MAKER_CLOUD_CAMERA_POS);
static const float4 volumetricCameraWeatherData = CloudVolumetricSampleWeather(WEATHER_MAKER_CLOUD_CAMERA_POS, volumetricCameraHeightFrac, 0.0);
static const float volumetricCameraCloudDensity = SampleCloudDensity(WEATHER_MAKER_CLOUD_CAMERA_POS, volumetricCameraWeatherData, volumetricCameraHeightFrac, 0.0, true);

#endif // __WEATHER_MAKER_CLOUD_VOLUMETRIC_SAMPLING_SHADER__
