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

#ifndef __WEATHER_MAKER_CLOUD_VOLUMETRIC_LIGHTING_SHADER__
#define __WEATHER_MAKER_CLOUD_VOLUMETRIC_LIGHTING_SHADER__

#include "WeatherMakerCloudVolumetricSamplingShaderInclude.cginc"
#include "WeatherMakerCloudShaderInclude.cginc"

// uncomment to use linear instead of exponential ambient sampling
// #define VOLUMETRIC_CLOUD_AMBIENT_MODE_LINEAR

#define VOLUMETRIC_CLOUD_ENABLE_POINT_LIGHTS
#define VOLUMETRIC_MAX_LIGHT_DENSITY 10.0
#define VOLUMETRIC_LIGHT_FIRST_STEP_MULTIPLIER 1.5

static const fixed3 volumetricCloudAmbientColorGround = (_WeatherMakerAmbientLightColorGround * _CloudAmbientGroundIntensityVolumetric);
static const fixed3 volumetricCloudAmbientColorSky = (_WeatherMakerAmbientLightColorSky * _CloudAmbientSkyIntensityVolumetric);
static const fixed3 volumetricCloudAmbientColorSkyLuminance = Luminance(volumetricCloudAmbientColorSky);
static const float volumetricCloudAmbientSkyPower = 1.0 - _CloudAmbientSkyHeightMultiplierVolumetric;
static const float volumetricCloudAmbientGroudMultiplier = 1.0 - _CloudAmbientGroundHeightMultiplierVolumetric;

float CloudVolumetricBeerLambert(float density)
{
	// TODO: Multiply by precipitation or density for rain/snow clouds
	return exp2(-density);
}

float CloudVolumetricPowder(float density, DirLightPrecomputation dirLight, float3 lightDir)
{
	// reduce powder multiplier as light approaches horizon, or as light intensity goes below 1
	// reduce powder effect as view turns towards the light or light intensity drops
	// reduce powder effect as powder multiplier approaches 0
	float powder = saturate((1.0 - exp2(density * -2.0)) * dirLight.powderMultiplier);
	return lerp(1.0, powder, dirLight.powderAngle);
}

float3 CloudVolumetricLightEnergy(DirLightPrecomputation dirLight, float densitySample,
	float eyeDensity, float densityToLight, float3 lightDir, float3 rayDir)
{
	// With E as light energy, d as the density sampled for lighting, p as the absorption multiplier for rain, g as our eccentricity in light direction, and θ as the angle between the view and light rays,
	// calculate lighting - E = 2.0 * e−dp * (1 − e−2d) * (1/4π) * (1 − g2 1 + g2 − 2g cos(θ)3/2).
	float beerLambert = CloudVolumetricBeerLambert(densityToLight);
	float powder = CloudVolumetricPowder(densitySample, dirLight, lightDir);
	return float3(beerLambert, powder, dirLight.hg);
}

fixed3 SampleDirLightSources(float3 marchPos, float3 rayDir, float startHeightFrac, float cloudSample,
	float eyeDensity, float lod, inout CloudState state)
{
	fixed3 lightTotal = fixed3Zero;

	UNITY_BRANCH
	if (_CloudDirLightMultiplierVolumetric <= 0.0)
	{
		return lightTotal;
	}

	float indirectHeightFrac = max(0.3, startHeightFrac);
	lod = max(_CloudDirLightLod, lod + _CloudDirLightLod);
	float stepSize = state.lightStepSize;

	// take advantage of the fact that lights are sorted by perspective/ortho and then by intensity
	UNITY_LOOP
	for (uint lightIndex = 0; lightIndex < uint(_WeatherMakerDirLightCount) && lightIndex < 2 && _WeatherMakerDirLightVar1[lightIndex].y == 0.0 && _WeatherMakerDirLightColor[lightIndex].a > 0.0; lightIndex++)
	{
		float3 lightDir = state.dirLight[lightIndex].lightDir;
		float lightShadow = state.dirLight[lightIndex].shadowPower;
		fixed4 lightColor = _WeatherMakerDirLightColor[lightIndex];
		float3 lightStep = (lightDir.xyz * stepSize);
		float heightFrac;
		float4 weatherData;
		float coneRadius = state.dirLight[lightIndex].lightConeRadius;
		float coneRadiusStep = coneRadius;
		coneRadius *= VOLUMETRIC_LIGHT_FIRST_STEP_MULTIPLIER;
		float3 samplePos;
		float3 energy;
		float densityToLight = 0.0;
		float subSampleDensity;
		float3 pos = marchPos + (lightStep * VOLUMETRIC_LIGHT_FIRST_STEP_MULTIPLIER);

		UNITY_LOOP
		for (uint i = 0; i < volumetricLightIterations && densityToLight < VOLUMETRIC_MAX_LIGHT_DENSITY; i++)
		{
			subSampleDensity = 0.0;

			UNITY_LOOP
			for (uint j = 0; j < volumetricLightSubIterations; j++)
			{
				// sample in the cone, take the march pos and perturb by random vector and cone radius
				samplePos = pos + (weatherMakerRandomCone[(i + j) % 16] * coneRadius);

				// ensure a minimum height - if this goes too low, lighting gets really ugly near the horizon
				heightFrac = max(0.01, GetCloudHeightFractionForPoint(samplePos));

				UNITY_BRANCH
				if (heightFrac > 1.0)
				{
					// marched out of cloud volume
					i = j = volumetricLightIterations;
				}
				else
				{
					// lookup position for cloud density
					weatherData = CloudVolumetricSampleWeather(samplePos, heightFrac, lod);

					UNITY_BRANCH
					if (CloudVolumetricGetCoverage(weatherData) > _CloudCoverVolumetricMinimumForCloud)
					{
						UNITY_BRANCH
						if (WM_CAMERA_RENDER_MODE_NORMAL)
						{
							subSampleDensity += (lightShadow * SampleCloudDensity(samplePos, weatherData, heightFrac, lod, _CloudRaymarchSampleDetailsForDirLight));
						}
						else
						{
							// fast approximation, this is just a reflection, who cares...
							fixed coverage = CloudVolumetricGetCoverage(weatherData);
							fixed type = CloudVolumetricGetType(weatherData);
							subSampleDensity += ((coverage * coverage * GetDensityHeightGradientForHeight(heightFrac, type).x));
						}
					}
				}
			}

			// average the samples
			densityToLight += (subSampleDensity * invVolumetricLightSubIterations);

			// march to next position
			coneRadius += coneRadiusStep;
			pos += lightStep;
		}

		UNITY_BRANCH
		if (_CloudLightDistantMultiplierVolumetric > 0.0 && densityToLight < VOLUMETRIC_MAX_LIGHT_DENSITY)
		{
			// one final sample farther away for distant cloud
			samplePos = pos + (lightStep * _CloudLightDistantMultiplierVolumetric);
			heightFrac = GetCloudHeightFractionForPoint(samplePos);

			UNITY_BRANCH
			if (heightFrac <= 1.0)
			{
				weatherData = CloudVolumetricSampleWeather(samplePos, heightFrac, lod + 1.0);

				UNITY_BRANCH
				if (CloudVolumetricGetCoverage(weatherData) > _CloudCoverVolumetricMinimumForCloud)
				{
					UNITY_BRANCH
					if (WM_CAMERA_RENDER_MODE_NORMAL)
					{
						densityToLight += (lightShadow *
							SampleCloudDensity(samplePos, weatherData, heightFrac, lod + 1.0, _CloudRaymarchSampleDetailsForDirLight));
					}
					else
					{
						// fast approximation, this is just a reflection, who cares...
						fixed coverage = CloudVolumetricGetCoverage(weatherData);
						fixed type = CloudVolumetricGetType(weatherData);
						densityToLight += (lightShadow * (coverage * coverage * GetDensityHeightGradientForHeight(heightFrac, type).x));
					}
				}
			}
		}

		// one sample for flat cloud light density
		UNITY_BRANCH
		if (densityToLight < VOLUMETRIC_MAX_LIGHT_DENSITY)
		{
			fixed flatShadows = 0.5 * ComputeFlatCloudShadows(lightDir, marchPos, lod);
			densityToLight += flatShadows;
		}

		state.fade = 0.0;// clamp(1.0 / densityToLight, 0.0, 0.5);

		energy = CloudVolumetricLightEnergy(state.dirLight[lightIndex], cloudSample, eyeDensity,
			densityToLight, lightDir, rayDir);
		fixed energyScalar = energy.x * energy.y * energy.z;
		energyScalar = max(0.0, energyScalar + state.lightColorDithering); // dither light for further banding reduction

		// cascade shadow map
		UNITY_BRANCH
		if (_WeatherMakerCloudShadowSampleShadowMap < 1.0 && lightIndex == 0 && energyScalar > _WeatherMakerCloudShadowSampleShadowMap && _WeatherMakerDirLightPower[lightIndex].z > 0.05)
		{
			energyScalar *= lerp(wm_sample_shadow_world_pos(marchPos, _WeatherMakerCloudShadowSampleShadowMap), 1.0, _WeatherMakerDirLightPower[lightIndex].w);
			//energyScalar = min(energyScalar, lerp(wm_sample_shadow_world_pos(marchPos, _WeatherMakerCloudShadowSampleShadowMap), 1.0, _WeatherMakerDirLightPower[lightIndex].w));
		}

		// indirect + direct light
		lightTotal += (state.dirLight[lightIndex].indirectLight * indirectHeightFrac) + (lightColor.rgb * energyScalar);
	}

	return lightTotal * _CloudDirColorVolumetric;
}

fixed3 SamplePointLightSources(float3 marchPos, float3 rayDir, float startHeightFrac, float cloudSample, float eyeDensity, float lod, float4 uv)
{

#if defined(VOLUMETRIC_CLOUD_ENABLE_POINT_LIGHTS)

	fixed3 lightTotal = ComputePointSpotLightCloud(cloudSample, marchPos);

#else

	fixed3 lightTotal = fixed3Zero;

#endif

	return lightTotal;
}

// https://en.wikipedia.org/wiki/Exponential_integral
float ExponentialIntegral(float v)
{
	return 0.5772156649015328606065 + log(0.0001 + abs(v)) + v * (1.0 + v * (0.25 + v * ((1.0 / 18.0) + v * ((1.0 / 96.0) + v * (1.0 / 600.0)))));
}

fixed3 SampleAmbientLight(float3 marchPos, float3 rayDir, float rayLength, float skyMultiplier, fixed3 skyColor, float heightFrac, float4 weatherData)
{

#define VOLUMETRIC_AMBIENT_REDUCTION _CloudAmbientShadowVolumetric
#define VOLUMETRIC_AMBIENT_STEP1 0.1
#define VOLUMETRIC_AMBIENT_STEP2 0.2
#define VOLUMETRIC_AMBIENT_STEP3 0.3

	fixed skyReducer = 1.0;
	fixed groundReducer = 1.0;
	float coverage = CloudVolumetricGetCoverage(weatherData);

	UNITY_BRANCH
	if (coverage > 0.1)
	{
		float coverageSquared = coverage * coverage;
		coverageSquared *= VOLUMETRIC_AMBIENT_REDUCTION * (1.0 + (RandomFloat(marchPos) * 0.05));
		skyReducer -= (coverageSquared * GetCloudRoundness(marchPos, weatherData, heightFrac + VOLUMETRIC_AMBIENT_STEP1));
		skyReducer -= (coverageSquared * GetCloudRoundness(marchPos, weatherData, heightFrac + VOLUMETRIC_AMBIENT_STEP2));
		skyReducer -= (coverageSquared * GetCloudRoundness(marchPos, weatherData, heightFrac + VOLUMETRIC_AMBIENT_STEP3));

		groundReducer -= (coverageSquared * GetCloudRoundness(marchPos, weatherData, heightFrac - VOLUMETRIC_AMBIENT_STEP1));
		groundReducer -= (coverageSquared * GetCloudRoundness(marchPos, weatherData, heightFrac - VOLUMETRIC_AMBIENT_STEP2));
		groundReducer -= (coverageSquared * GetCloudRoundness(marchPos, weatherData, heightFrac - VOLUMETRIC_AMBIENT_STEP3));
	}

#if defined(VOLUMETRIC_CLOUD_AMBIENT_MODE_LINEAR)

	// reduce sky light at lower heights
	// reduce ground light at higher heights
	fixed groundHeightFrac = 1.0 - min(1.0, heightFrac * _CloudAmbientGroundHeightMultiplierVolumetric);
	skyColor *= skyMultiplier * heightFrac * _CloudAmbientSkyHeightMultiplierVolumetric * skyReducer;
	fixed3 groundColor = volumetricCloudAmbientColorGround * groundHeightFrac * groundReducer;
	return _CloudEmissionColorVolumetric + skyColor + groundColor;

#else

	/*
	// // page 12-15 https://patapom.com/topics/Revision2013/Revision%202013%20-%20Real-time%20Volumetric%20Rendering%20Course%20Notes.pdf
	float Hp = 1.0 - heightFrac; // VolumeTop - _Position.y; // Height to the top of the volume
	float a = -volumetricAmbientSkyPower * Hp;
	float3 isotropicScatteringTop = skyColor * max(0.0, exp(a) - a * ExponentialIntegral(a));
	float Hb = heightFrac; // _Position.y - VolumeBottom; // Height to the bottom of the volume
	a = -volumetricAmbientGroudMultiplier * Hb;
	float3 isotropicScatteringBottom = volumetricCloudAmbientColorGround * max(0.0, exp(a) - a * ExponentialIntegral(a));
	return _CloudEmissionColorVolumetric + (isotropicScatteringTop + isotropicScatteringBottom);
	*/

	float Hp = pow(heightFrac, volumetricCloudAmbientSkyPower) - 1.0;
	float3 scatterTop = skyColor * skyMultiplier * max(0.0, exp(Hp) - Hp * ExponentialIntegral(Hp)) * skyReducer;
	float Hb = -(heightFrac * volumetricCloudAmbientGroudMultiplier);
	float3 scatterBottom = volumetricCloudAmbientColorGround * max(0.0, exp(Hb) - Hb * ExponentialIntegral(Hb)) * groundReducer;
	return _CloudEmissionColorVolumetric + (scatterTop + scatterBottom);

#endif

}

fixed4 FinalizeVolumetricCloudColor(fixed4 color, float4 uv, uint marches)
{

#if VOLUMETRIC_CLOUD_RENDER_MODE == 2

	color.rgb = (float)marches / float(_CloudNoiseSampleCountVolumetric.y);
	color.a = 1.0;

#else

	UNITY_BRANCH
	if (color.a > 0.0)
	{

#if defined(UNITY_COLORSPACE_GAMMA)

        color.rgb *= 1.4;

#else

        //color.rgb = pow(color.rgb, 2.2);
		color.rgb = GammaToLinearSpace(color.rgb);

#endif

		UNITY_BRANCH
		if (_WeatherMakerEnableToneMapping)
		{
			color.rgb = FilmicTonemapFull(color.rgb, 2.0);
		}

        // soften, the rgb is already pre-multiplied, but the alpha channel is not
        color.a *= color.a;
	}

	ApplyDither(color.rgb, uv.xy, 0.0025);
	color *= color.a; // pre-multiply

#endif

	return color;
}

fixed ComputeCloudColorVolumetricHorizonFade(float opticalDepth)
{
	UNITY_BRANCH
	if (_CloudHorizonFadeVolumetric.w > 0.0)
	{
		float hf = min(1.0, (1.0 - opticalDepth) * _CloudHorizonFadeVolumetric.y);
		hf = pow(hf, _CloudHorizonFadeVolumetric.z);
		hf = saturate(Remap(hf, 0.1, 1.0, 0.0, 1.0)); // smooth it out
		hf = lerp(1.0, hf, _CloudHorizonFadeVolumetric.w);
		return hf;
	}
	else
	{
		return 1.0;
	}
}

#endif // __WEATHER_MAKER_CLOUD_VOLUMETRIC_LIGHTING_SHADER__
