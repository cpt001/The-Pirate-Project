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

#ifndef __WEATHER_MAKER_CLOUD_SHADER__
#define __WEATHER_MAKER_CLOUD_SHADER__

#include "WeatherMakerCloudShaderUniformsInclude.cginc"
#include "WeatherMakerSkyShaderInclude.cginc"

fixed ComputeCloudShadowStrength(float noise, uint cloudIndex);
fixed ComputeFlatCloudShadows(float3 rayDirLight, float3 worldPos, float lod = 0.0, uint startCloudIndex = 0);

// returns world pos of cloud plane intersect
float3 CloudRaycastWorldPosPlane(float3 ray, float3 worldPos, float depth, uint cloudIndex)
{
	float3 rayWithOffset = normalize(float3(ray.x, ray.y + _CloudRayOffset[cloudIndex], ray.z));
	float3 planePos = float3(0, _CloudHeight[cloudIndex], 0);
	float distanceToPlane;
	float planeMultiplier = RayPlaneIntersect(worldPos, rayWithOffset, float3(0.0, 1.0, 0.0), planePos, distanceToPlane);
	planeMultiplier *= (distanceToPlane < depth);
	float3 intersectPos = worldPos + (ray * distanceToPlane);
	return planeMultiplier * intersectPos;
}

float2 ComputeCloudNoise(float3 rayDir, float3 worldPos, float depth, sampler2D noiseTex, uint cloudIndex, float lod, out float3 worldPosHit)
{
	worldPosHit = float3Zero;

	UNITY_BRANCH
	if (_CloudCover[cloudIndex] > 0.01 && _CloudNoiseMultiplier[cloudIndex] > 0.01)
	{
		worldPosHit = CloudRaycastWorldPosPlane(rayDir, worldPos, depth, cloudIndex);
		UNITY_BRANCH
		if (worldPosHit.x != 0.0 && worldPosHit.y != 0.0 && worldPosHit.z != 0.0)
		{
			float3 noisePos = ((worldPosHit + _CloudNoiseVelocity[cloudIndex]) * _CloudNoiseScale[cloudIndex]);
			float4 uv = float4(RotateUV(noisePos.xz, _CloudNoiseRotation[cloudIndex + 4], _CloudNoiseRotation[cloudIndex]), 0.0, lod);
			float noise = (tex2Dlod(noiseTex, uv).a + _CloudNoiseAdder[cloudIndex]) * _CloudCover[cloudIndex] * _CloudNoiseMultiplier[cloudIndex];

			// dither noise in smaller noise values to prevent banding
			noise = lerp(noise * (1.0 + (RandomFloat(worldPosHit) * _CloudNoiseDither[cloudIndex])), noise, min(1.0, noise * 0.5));

			// make sure we didn't go negative
			noise = max(0.0, noise);

			// soft fade
			UNITY_BRANCH
			if (noise > 0.01 && depth < _ProjectionParams.z)
			{
				float partZ = distance(worldPosHit, WEATHER_MAKER_CLOUD_CAMERA_POS);
				float diff = (depth - partZ);
				noise *= saturate(_CloudInvFade * diff);
			}

			return float2(noise, 1.0);
		}
		else
		{
			return float2(0.0, 0.0);
		}
	}
	else
	{
		return float2(0.0, 0.0);
	}
}

float FlatCloudHg(DirLightPrecomputation dirLight, uint cloudIndex)
{

#define VOLUMETRIC_MAX_HENYEY_GREENSTEIN 5.0

	// https://www.diva-portal.org/smash/get/diva2:1223894/FULLTEXT01.pdf
	// f(x) = (1 - g)^2 / (4PI * (1 + g^2 - 2g*cos(x))^[3/2])
	// _CloudHenyeyGreensteinPhase.x = forward, _CloudHenyeyGreensteinPhase.y = back
	const float g = _CloudScatterMultiplier[cloudIndex].x;
	const float gSquared = g * g;
	const float oneMinusGSquared = (1.0 - gSquared);
	const float onePlusGSquared = 1.0 + gSquared;
	const float twoG = 2.0 * g;
	float falloff = pow(PI * (onePlusGSquared - (twoG * dirLight.eyeDot)), 1.5);
	float forward = oneMinusGSquared / falloff;

	const float g2 = _CloudScatterMultiplier[cloudIndex].y;
	const float gSquared2 = g2 * g2;
	const float oneMinusGSquared2 = (1.0 - gSquared2);
	const float onePlusGSquared2 = 1.0 + gSquared2;
	const float twoG2 = 2.0 * g2;
	float falloff2 = pow(PI * (onePlusGSquared2 - (twoG2 * dirLight.eyeDot)), 1.5);
	float back = oneMinusGSquared2 / falloff2;

	return min(VOLUMETRIC_MAX_HENYEY_GREENSTEIN, (((forward * _CloudScatterMultiplier[cloudIndex].z) + (back * _CloudScatterMultiplier[cloudIndex].w))));
}

fixed ComputeDirectionalLightPowerCloud(float noise, float3 rayDirLight, float3 worldPos, DirLightPrecomputation dirLight, float lod, uint cloudIndex)
{
	// Note- assumption is flat clouds are higher than volumetric

	// other cloud layer shadowing
	float densityToLight = ComputeFlatCloudShadows(rayDirLight, worldPos, lod, cloudIndex + 1);

	// self shadowing
	densityToLight += (0.5 * ComputeCloudShadowStrength(noise, cloudIndex));

	// henyey-greenstein formla
	float hg = FlatCloudHg(dirLight, cloudIndex);

	// beer law
	float beer = exp2(-densityToLight);

	return hg * beer * min(1.0, noise * 2.0);
}

fixed3 ComputeDirectionalLightCloud(float noise, float3 worldPos, float lod, int cloudIndex, inout CloudState state)
{
	fixed3 finalColor = fixed3Zero;
	UNITY_LOOP
	for (uint lightIndex = 0; lightIndex < uint(_WeatherMakerDirLightCount) && lightIndex < 2 && _WeatherMakerDirLightColor[lightIndex].a > 0.001; lightIndex++)
	{
		// direct light
		float3 lightDir = _WeatherMakerDirLightPosition[lightIndex].xyz;
		fixed4 lightColor = _WeatherMakerDirLightColor[lightIndex];
		finalColor += (lightColor.rgb *
			lightColor.a *
			ComputeDirectionalLightPowerCloud(noise, lightDir, worldPos, state.dirLight[lightIndex], lod, cloudIndex));
	}
	return finalColor;
}

// shared with volumetric clouds
fixed3 ComputePointSpotLightCloud(float noise, float3 worldPos)
{
	fixed3 finalColor = fixed3Zero;
	uint lightIndex;

	UNITY_LOOP
	for (lightIndex = 0; lightIndex < uint(_WeatherMakerPointLightCount) && _WeatherMakerPointLightColor[lightIndex].a > 0.001; lightIndex++)
	{
		float3 toLight = _WeatherMakerPointLightPosition[lightIndex].xyz - worldPos;
		float lengthSq = max(0.000001, dot(toLight, toLight));
		fixed atten = (1.0 / (1.0 + (lengthSq * _WeatherMakerPointLightAtten[lightIndex].z)));
		finalColor += (saturate(atten) * max(0.5, noise) * _CloudPointSpotLightMultiplierVolumetric *
			_WeatherMakerPointLightColor[lightIndex].a * _WeatherMakerPointLightColor[lightIndex].rgb);
	}

	UNITY_LOOP
	for (lightIndex = 0; lightIndex < uint(_WeatherMakerSpotLightCount); lightIndex++)
	{
		float3 toLight = _WeatherMakerSpotLightPosition[lightIndex].xyz - worldPos;
		float lengthSq = max(0.000001, dot(toLight, toLight));
		fixed atten = (1.0 / (1.0 + (lengthSq * _WeatherMakerSpotLightAtten[lightIndex].z)));
		toLight *= rsqrt(lengthSq);
		float theta = max(0.0, dot(toLight, -_WeatherMakerSpotLightDirection[lightIndex].xyz));
		atten *= saturate((theta - _WeatherMakerSpotLightAtten[lightIndex].x) * _CloudPointSpotLightMultiplierVolumetric *
			_WeatherMakerSpotLightAtten[lightIndex].y);
		finalColor += (saturate(atten) * max(0.5, noise) * _WeatherMakerSpotLightColor[lightIndex].a * _WeatherMakerSpotLightColor[lightIndex].rgb);
	}

	return finalColor;
}

fixed3 ComputeCloudLighting(float noise, float3 rayDir, float3 worldPos, float2 uv, float lod, uint cloudIndex, fixed alphaAccum, inout CloudState state)
{
	fixed3 finalColor = ComputeDirectionalLightCloud(noise, worldPos, lod, cloudIndex, state);
	finalColor += ComputePointSpotLightCloud(noise, worldPos);
	finalColor += (_WeatherMakerAmbientLightColorGround.rgb * _CloudAmbientGroundMultiplier[cloudIndex]);
	finalColor += (_WeatherMakerAmbientLightColorSky.rgb * _CloudAmbientSkyMultiplier[cloudIndex]);
	finalColor += (_CloudEmissionColor[cloudIndex].rgb * _CloudEmissionColor[cloudIndex].a);
	finalColor *= _CloudColor[cloudIndex].rgb;
	return finalColor;
}

fixed ComputeFlatCloudDensityBetween(float3 rayDir, float3 start, float3 end)
{
	float depth = distance(start, end);
	float3 worldPosHit;
	fixed flatDensity = ComputeCloudNoise(rayDir, start, depth, _CloudNoise1, 0, 0.0, worldPosHit).x;
	flatDensity += ComputeCloudNoise(rayDir, start, depth, _CloudNoise1, 1, 0.0, worldPosHit).x;
	flatDensity += ComputeCloudNoise(rayDir, start, depth, _CloudNoise1, 2, 0.0, worldPosHit).x;
	flatDensity += ComputeCloudNoise(rayDir, start, depth, _CloudNoise1, 3, 0.0, worldPosHit).x;
	return min(flatDensity, flatDensity * flatDensity);
}

fixed ComputeCloudShadowStrength(float noise, uint cloudIndex)
{
	return (noise * min(1.0, 2.0 * _CloudCover[0]) * _CloudLightAbsorption[0]);
}

fixed ComputeFlatCloudShadows(float3 rayDirLight, float3 worldPos, float lod, uint startCloudIndex)
{
	// flat layer shadows
	static const fixed depth = 1000000.0;
	fixed shadow = 0.0;
	float noise;
	float3 worldPosHit;

	UNITY_BRANCH
	if (startCloudIndex < 1 && _CloudCover[0] > 0.0 && _CloudLightAbsorption[0] > 0.0)
	{
		noise = ComputeCloudNoise(rayDirLight, worldPos, depth, _CloudNoise1, 0, lod, worldPosHit).x;
		shadow += ComputeCloudShadowStrength(noise, 0);
	}

	UNITY_BRANCH
	if (startCloudIndex < 2 && _CloudCover[1] > 0.0 && _CloudLightAbsorption[1] > 0.0)
	{
		noise = ComputeCloudNoise(rayDirLight, worldPos, depth, _CloudNoise1, 1, lod, worldPosHit).x;
		shadow += ComputeCloudShadowStrength(noise, 1);
	}

	UNITY_BRANCH
	if (startCloudIndex < 3 && _CloudCover[2] > 0.0 && _CloudLightAbsorption[2] > 0.0)
	{
		noise = ComputeCloudNoise(rayDirLight, worldPos, depth, _CloudNoise1, 2, lod, worldPosHit).x;
		shadow += ComputeCloudShadowStrength(noise, 2);
	}

	UNITY_BRANCH
	if (startCloudIndex < 4 && _CloudCover[3] > 0.0 && _CloudLightAbsorption[3] > 0.0)
	{
		noise = ComputeCloudNoise(rayDirLight, worldPos, depth, _CloudNoise1, 3, lod, worldPosHit).x;
		shadow += ComputeCloudShadowStrength(noise, 3);
	}

	return shadow;
}

CloudColorResult ComputeFlatCloudColor(float3 rayDir, float3 worldPos, float2 uv,
	float depth, sampler2D noiseTex, float4 screenUV, uint cloudIndex, float lod, fixed alphaAccum, inout CloudState state)
{
	float3 worldPosHit;
	float2 noise = ComputeCloudNoise(rayDir, worldPos, depth, noiseTex, cloudIndex, lod, worldPosHit);

	// fast out for transparent area, avoid lots of unnecessary calculations
	UNITY_BRANCH
	if (noise.x <= 0.005)
	{
		CloudColorResult result = { fixed4Zero, 1.0, noise.y };
		return result;
	}
	else
	{
		// compute lighting
		fixed3 lightColor = ComputeCloudLighting(noise.x, rayDir, worldPosHit, uv, lod, cloudIndex, alphaAccum, state);
		fixed4 finalColor = fixed4(lightColor, min(1.0, noise.x));
		CloudColorResult result = { finalColor, 1.0, noise.y };
		return result;
	}
}

CloudColorResult ComputeFlatCloudColorAll(float3 cloudRay, float depth, float4 uv, float lod, inout CloudState state)
{
	fixed4 cloudColor;
	fixed4 finalColor = fixed4Zero;
	float3 worldPos = WEATHER_MAKER_CLOUD_CAMERA_POS;
	float hitCloud = 0.0;

	// bottom layer
	CloudColorResult result1 = ComputeFlatCloudColor(cloudRay, worldPos, uv.xy, depth, _CloudNoise1, uv, 0, lod, finalColor.a, state);
	result1.color.rgb *= result1.color.a;
	finalColor = result1.color;
	hitCloud += result1.hitCloud;

	UNITY_BRANCH
	if (finalColor.a < 0.999)
	{
		// next layer
		CloudColorResult result2 = ComputeFlatCloudColor(cloudRay, worldPos, uv.xy, depth, _CloudNoise2, uv, 1, lod, finalColor.a, state);
		result2.color.rgb *= result2.color.a;
		finalColor += ((1.0 - finalColor.a) * result2.color);
		hitCloud += result2.hitCloud;
	}

	UNITY_BRANCH
	if (finalColor.a < 0.999)
	{
		// next layer
		CloudColorResult result3 = ComputeFlatCloudColor(cloudRay, worldPos, uv.xy, depth, _CloudNoise3, uv, 2, lod, finalColor.a, state);
		result3.color.rgb *= result3.color.a;
		finalColor += ((1.0 - finalColor.a) * result3.color);
		hitCloud += result3.hitCloud;
	}

	UNITY_BRANCH
	if (finalColor.a < 0.999)
	{
		// top layer
		CloudColorResult result4 = ComputeFlatCloudColor(cloudRay, worldPos, uv.xy, depth, _CloudNoise4, uv, 3, lod, finalColor.a, state);
		result4.color.rgb *= result4.color.a;
		finalColor += ((1.0 - finalColor.a) * result4.color);
		hitCloud += result4.hitCloud;
	}

	ApplyDither(finalColor.rgb, uv.xy, 0.01);

	// pre-multiply
	finalColor *= finalColor.a;

	CloudColorResult result = { finalColor, 1.0, min(1.0, hitCloud) };
	return result;
}

#endif // __WEATHER_MAKER_CLOUD_SHADER__
