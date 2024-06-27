//
// https://github.com/SlightlyMad/AtmosphericScattering
//  Copyright(c) 2016, Michal Skalsky
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification,
//  are permitted provided that the following conditions are met:
//
//  1. Redistributions of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//
//  2. Redistributions in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the documentation
//     and/or other materials provided with the distribution.
//
//  3. Neither the name of the copyright holder nor the names of its contributors
//     may be used to endorse or promote products derived from this software without
//     specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
//  EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
//  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT
//  SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
//  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
//  OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
//  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
//  TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
//  EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#ifndef WEATHER_MAKER_ATMOSPHERE_SCATTERING_INCLUDE
#define WEATHER_MAKER_ATMOSPHERE_SCATTERING_INCLUDE

#include "WeatherMakerCloudVolumetricShadowsShaderInclude.cginc"
#include "WeatherMakerShadowsShaderInclude.cginc"

// keywords, not using shader variants to reduce compile times and permutations
// #define WEATHER_MAKER_ATMOSPHERE_REFERENCE // reference implementation
// #define WEATHER_MAKER_ATMOSPHERE_HIGH_QUALITY // highest quality but slow
// #define WEATHER_MAKER_ATMOSPHERE_RENDER_SUN_SKY // whether to render a sun in the sky
#define WEATHER_MAKER_ATMOSPHERE_LIGHT_SHAFTS // whether to render light shafts

uniform uint _WeatherMakerAtmosphereLevel;
uniform uint _WeatherMakerAtmosphereLightShaftEnable;
uniform uint _WeatherMakerAtmosphereLightShaftSampleCount;

uniform float _WeatherMakerAtmosphereHeight;
uniform float _WeatherMakerAtmospherePlanetRadius;
uniform float _WeatherMakerAtmosphereLightShaftMaxRayLength;
uniform float _WeatherMakerMieG;
uniform float _WeatherMakerAtmosphereDistanceScale;
uniform float _WeatherMakerAtmosphereRayLength;

uniform float2 _WeatherMakerDensityScaleHeight;

uniform float3 _WeatherMakerScatteringR;
uniform float3 _WeatherMakerScatteringM;
uniform float3 _WeatherMakerExtinctionR;
uniform float3 _WeatherMakerExtinctionM;
uniform float3 _WeatherMakerIncomingLight;
uniform float4 _WeatherMakerSkyAtmosphereParams; // skyAtmosphereThickness, skyAtmosphereTurbidity, skyExposure, 0.0f

// metal does not support GetDimensions...
uniform float3 _WeatherMakerSkyboxLUT_Dimensions;
uniform float3 _WeatherMakerInscatteringLUT_Dimensions;

uniform UNITY_DECLARE_TEX2D_FLOAT(_WeatherMakerParticleDensityLUT);
uniform UNITY_DECLARE_TEX2D_FLOAT(_WeatherMakerRandomVectors);
uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_WeatherMakerAtmosphereLightShaftTexture);

#if !defined(WEATHER_MAKER_COMPUTE_SHADER)

uniform UNITY_DECLARE_TEX3D_FLOAT(_WeatherMakerSkyboxLUT);

#if defined(WEATHER_MAKER_ATMOSPHERE_HIGH_QUALITY)

uniform UNITY_DECLARE_TEX3D_FLOAT(_WeatherMakerSkyboxLUT2);

#endif

uniform UNITY_DECLARE_TEX3D_FLOAT(_WeatherMakerInscatteringLUT);
uniform UNITY_DECLARE_TEX3D_FLOAT(_WeatherMakerExtinctionLUT);
uniform UNITY_DECLARE_TEX3D_FLOAT(_WeatherMakerInscatteringLUT2);
uniform UNITY_DECLARE_TEX3D_FLOAT(_WeatherMakerExtinctionLUT2);

#endif

#define ATMOSPHERE_THICKNESS _WeatherMakerSkyAtmosphereParams.x
#define ATMOSPHERE_TURBIDITY _WeatherMakerSkyAtmosphereParams.y
#define ATMOSPHERE_PLANET_CENTERH(h) float3(WEATHER_MAKER_CAMERA_POS.x, -_WeatherMakerAtmospherePlanetRadius + h, WEATHER_MAKER_CAMERA_POS.z)
static const float3 ATMOSPHERE_PLANET_CENTER = float3(WEATHER_MAKER_CAMERA_POS.x, -_WeatherMakerAtmospherePlanetRadius, WEATHER_MAKER_CAMERA_POS.z);
static const float invWeatherMakerAtmosphereLightShaftSampleCount = 1.0 / float(_WeatherMakerAtmosphereLightShaftSampleCount);

static const float invWeatherMakerAtmosphereSampleCount = 1.0 / float(_WeatherMakerInscatteringLUT_Dimensions.z);
static const float atmosphere01 = (1.0 / _WeatherMakerAtmosphereRayLength) * _WeatherMakerAtmosphereDistanceScale * 25.0;

float2 AtmosphereRaySphereIntersection(float3 rayOrigin, float3 rayDir, float3 sphereCenter, float sphereRadius)
{
	rayOrigin -= sphereCenter;
	float a = dot(rayDir, rayDir);
	float b = 2.0 * dot(rayOrigin, rayDir);
	float c = dot(rayOrigin, rayOrigin) - (sphereRadius * sphereRadius);
	float d = b * b - 4 * a * c;

	UNITY_BRANCH
	if (d < 0)
	{
		return -1;
	}
	else
	{
		d = sqrt(d);
		return float2(-b - d, -b + d) / (2 * a);
	}
}

void ApplyPhaseFunction(inout float3 scatterR, inout float3 scatterM, float cosAngle, float mieG)
{
	cosAngle = max(0.0, cosAngle);

	// r
	float phase = ATMOSPHERE_TURBIDITY * (3.0 / (16.0 * PI)) * (1 + (cosAngle * cosAngle));
	scatterR *= phase;

	// m
	float g = mieG;
	float g2 = g * g;
	phase = (1.0 / (4.0 * PI)) * ((3.0 * (1.0 - g2)) / (2.0 * (2.0 + g2))) * ((1 + cosAngle * cosAngle) / pow((1 + g2 - 2 * g*cosAngle), 3.0 / 2.0));
	scatterM *= phase;
}

void ApplyPhaseFunctionElek(inout float3 scatterR, inout float3 scatterM, float cosAngle, bool doSunMie)
{
	cosAngle = max(0.0, cosAngle);

	// r
	float phase = ATMOSPHERE_TURBIDITY * (8.0 / 10.0) / (4 * PI) * ((7.0 / 5.0) + 0.5 * cosAngle);
	scatterR *= phase;

	if (doSunMie)
	{
		// m
		float g = _WeatherMakerMieG;
		float g2 = g * g;
		phase = (1.0 / (4.0 * PI)) * ((3.0 * (1.0 - g2)) / (2.0 * (2.0 + g2))) * ((1 + cosAngle * cosAngle) / pow((1 + g2 - 2 * g*cosAngle), 3.0 / 2.0));
		scatterM *= phase;
	}
}

float3 RenderSun(float3 scatterM, float cosAngle, float3 rayDir)
{
	// TODO: Turns sky white, figure out why
	//fixed4 sunColor = WeatherMakerRenderSun(rayDir);
	//sunColor.rgb *= scatterM;
	//return sunColor;

	static const float g = 0.98;
	static const float g2 = g * g;
	float sun = pow(1.0 - g, 2.0) / (4.0 * PI * pow(max(0.0, 1.0 + g2 - 2.0 * g * cosAngle), 1.5));
	sun *= 0.003;
	return scatterM * sun;
}

void GetAtmosphereDensity(float3 position, float3 planetCenter, float3 lightDir, out float2 localDensity, out float2 densityToAtmTop)
{
	float height = length(position - planetCenter) - _WeatherMakerAtmospherePlanetRadius;
	localDensity = ATMOSPHERE_THICKNESS * exp(-height.xx / _WeatherMakerDensityScaleHeight.xy);
	float cosAngle = dot(normalize(position - planetCenter), -lightDir.xyz);
	float4 uv = float4(cosAngle * 0.5 + 0.5, (height / _WeatherMakerAtmosphereHeight), 0.0, 0.0);
	densityToAtmTop = ATMOSPHERE_THICKNESS * UNITY_SAMPLE_TEX2D_LOD(_WeatherMakerParticleDensityLUT, uv, 0.0).rg;
}

void ComputeLocalInscattering(float2 localDensity, float2 densityPA, float2 densityCP, out float3 localInscatterR, out float3 localInscatterM)
{
	float2 densityCPA = densityCP + densityPA;
	float3 Tr = densityCPA.x * _WeatherMakerExtinctionR;
	float3 Tm = densityCPA.y * _WeatherMakerExtinctionM;
	float3 extinction = exp(-(Tr + Tm));
	localInscatterR = localDensity.x * extinction;
	localInscatterM = localDensity.y * extinction;
}

float3 ComputeOpticalDepth(float2 density)
{
	float3 Tr = density.x * _WeatherMakerExtinctionR;
	float3 Tm = density.y * _WeatherMakerExtinctionM;
	float3 extinction = exp(-(Tr + Tm));
	return _WeatherMakerIncomingLight * extinction;
}

float3 IntegrateInscattering(float3 rayStart, float3 rayDir, float rayLength, float3 planetCenter, float distanceScale, float3 lightDir, float sampleCount, out float3 extinction, bool drawSun)
{
	float3 step = rayDir * (rayLength / sampleCount);
	float stepSize = length(step) * distanceScale;

	float2 densityCP = 0;
	float3 scatterR = 0;
	float3 scatterM = 0;

	float2 localDensity;
	float2 densityPA;

	float2 prevLocalDensity;
	float3 prevLocalInscatterR, prevLocalInscatterM;
	GetAtmosphereDensity(rayStart, planetCenter, lightDir, prevLocalDensity, densityPA);
	ComputeLocalInscattering(prevLocalDensity, densityPA, densityCP, prevLocalInscatterR, prevLocalInscatterM);

	// P - current integration point
	// C - camera position
	// A - top of the atmosphere
	UNITY_LOOP
	for (float s = 1.0; s < sampleCount; s += 1)
	{
		float3 p = rayStart + step * s;

		GetAtmosphereDensity(p, planetCenter, lightDir, localDensity, densityPA);
		densityCP += (localDensity + prevLocalDensity) * (stepSize / 2.0);

		prevLocalDensity = localDensity;

		float3 localInscatterR, localInscatterM;
		ComputeLocalInscattering(localDensity, densityPA, densityCP, localInscatterR, localInscatterM);

		scatterR += (localInscatterR + prevLocalInscatterR) * (stepSize / 2.0);
		scatterM += (localInscatterM + prevLocalInscatterM) * (stepSize / 2.0);

		prevLocalInscatterR = localInscatterR;
		prevLocalInscatterM = localInscatterM;
	}

	float3 m = scatterM;
	// phase function
	ApplyPhaseFunction(scatterR, scatterM, dot(rayDir, -lightDir.xyz), _WeatherMakerMieG);
	//scatterR = 0;
	float3 lightInscatter = (scatterR * _WeatherMakerScatteringR + scatterM * _WeatherMakerScatteringM) * _WeatherMakerIncomingLight;

	UNITY_BRANCH
	if (drawSun)
	{
		lightInscatter += RenderSun(m, dot(rayDir, -lightDir.xyz), rayDir);
	}

	float3 lightExtinction = exp(-(densityCP.x * _WeatherMakerExtinctionR + densityCP.y * _WeatherMakerExtinctionM));

	extinction = lightExtinction;
	return lightInscatter;
}

#if !defined(WEATHER_MAKER_COMPUTE_SHADER)

// rayStart does not have _WeatherMakerCameraOriginOffset applied
float ComputeAtmosphericLightShafts(float2 screenPos, float3 rayStart, float3 rayDir, float rayLength)
{
	static const float minShaft = 0.025;
	float yFade = saturate((_WeatherMakerSunDirectionUp.y - 0.05) * 3.0);
	UNITY_BRANCH
	if (yFade <= 0.0)
	{
		return minShaft;
	}
	else
	{
		float4 cascadeWeights;
		float4 samplePos;

		float shadowPower = 0.0;
		float stepAmount = min(rayLength, _WeatherMakerAtmosphereLightShaftMaxRayLength) * invWeatherMakerAtmosphereLightShaftSampleCount;
		float3 stepDir = rayDir * stepAmount;
		float4 wpos = float4(rayStart, 1.0);
		float2 interleavedPos = fmod(floor(screenPos), 8.0);
		float dither = tex2D(_WeatherMakerDitherTexture, interleavedPos / 8.0 + float2(0.5 / 8.0, 0.5 / 8.0)).a;
		float shadow;
		wpos.xyz += (stepDir * dither);

		// for dir light, ray march through the shadow map
		UNITY_LOOP
		for (uint i = 0; i < _WeatherMakerAtmosphereLightShaftSampleCount; i++)
		{
			cascadeWeights = GET_CASCADE_WEIGHTS(wpos.xyz);
			samplePos = GET_SHADOW_COORDINATES(wpos, cascadeWeights);
			shadow = UNITY_SAMPLE_SHADOW(_WeatherMakerShadowMapTexture, samplePos);
			shadowPower += max(minShaft, ComputeCloudShadowStrengthTextureLOD(wpos.xyz, 0, shadow, false, 2.0));
			wpos.xyz += stepDir;
		}
		shadowPower *= invWeatherMakerAtmosphereLightShaftSampleCount;

		/*
		float2 interleavedPos = fmod(floor(screenPos), 8.0);
		float offset = tex2D(_WeatherMakerDitherTexture, interleavedPos / 8.0 + float2(0.5 / 8.0, 0.5 / 8.0)).w;
		int stepCount = _WeatherMakerAtmosphereLightShaftSampleCount;
		float stepSize = rayLength * invWeatherMakerAtmosphereLightShaftSampleCount;
		float3 step = rayDir * stepSize;
		float3 currentPosition = rayStart + step * offset;
		float vlight = 0;

		UNITY_LOOP
		for (int i = 0; i < stepCount; ++i)
		{
			cascadeWeights = GET_CASCADE_WEIGHTS(currentPosition);
			samplePos = GET_SHADOW_COORDINATES(float4(currentPosition, 1.0), cascadeWeights);
			float atten = max(minShaft, ComputeCloudShadowStrengthTextureLOD(currentPosition.xyz, 0, UNITY_SAMPLE_SHADOW(_WeatherMakerShadowMapTexture, samplePos), false, 2.0));
			vlight += atten * stepSize;
			currentPosition += step;
		}

		vlight = max(0, vlight);
		float shadowPower = vlight / rayLength;
		*/

		shadowPower = lerp(minShaft, shadowPower, yFade);
		return shadowPower;
	}
}

fixed3 ComputeAtmosphericScatteringSkyColor(float3 rayDir, bool doSunMie)
{
	float3 rayStart = WEATHER_MAKER_CAMERA_POS;
	float3 lightDir = _WeatherMakerDirLightPosition[0].xyz;
	float3 planetCenter = ATMOSPHERE_PLANET_CENTER;
	rayStart.y = max(0.0, rayStart.y); // everything falls apart without this if camera goes below y of 0

#if defined(WEATHER_MAKER_ATMOSPHERE_REFERENCE)

	float2 intersection = AtmosphereRaySphereIntersection(rayStart, rayDir, planetCenter, _WeatherMakerAtmospherePlanetRadius + _WeatherMakerAtmosphereHeight);
	float rayLength = intersection.y;

	intersection = AtmosphereRaySphereIntersection(rayStart, rayDir, planetCenter, _WeatherMakerAtmospherePlanetRadius);
	rayLength = lerp(rayLength, min(rayLength, intersection.x), intersection.x > 0);

	float3 extinction;
	float3 inscattering = IntegrateInscattering(rayStart, rayDir, rayLength, planetCenter, 1, -lightDir, 16, extinction, true);
	return inscattering;

#else

	float4 scatterR = 0;
	float4 scatterM = 0;

	float height = length(rayStart - planetCenter) - _WeatherMakerAtmospherePlanetRadius;
	float3 normal = normalize(rayStart - planetCenter);
	float viewZenith = dot(normal, rayDir);
	float sunZenith = dot(normal, lightDir);

	float3 coords = float3(height / _WeatherMakerAtmosphereHeight, viewZenith * 0.5 + 0.5, sunZenith * 0.5 + 0.5);

	coords.x = pow(height / _WeatherMakerAtmosphereHeight, 0.5);
	float ch = -(sqrt(height * (2 * _WeatherMakerAtmospherePlanetRadius + height)) / (_WeatherMakerAtmospherePlanetRadius + height));

	// cannot lerp here, everything goes haywire sky is all white...
	//coords.y = lerp(0.5 * pow((ch - viewZenith) / (ch + 1), 0.2), 0.5 * pow((viewZenith - ch) / (1 - ch), 0.2) + 0.5, viewZenith > ch);
	coords.y = ((viewZenith > ch) ? 0.5 * pow((viewZenith - ch) / (1 - ch), 0.2) + 0.5 : 0.5 * pow((ch - viewZenith) / (ch + 1), 0.2));

	coords.z = 0.5 * ((atan(max(sunZenith, -0.1975) * tan(1.26*1.1)) / 1.1) + (1 - 0.26));

	scatterR = UNITY_SAMPLE_TEX3D(_WeatherMakerSkyboxLUT, coords);

	if (doSunMie)
	{

#if defined(WEATHER_MAKER_ATMOSPHERE_HIGH_QUALITY)

		scatterM.x = scatterR.w;
		scatterM.yz = UNITY_SAMPLE_TEX3D(_WeatherMakerSkyboxLUT2, coords).xy;

#else

		scatterM.xyz = scatterR.xyz * ((scatterR.w) / (scatterR.x));

#endif

	}

	// phase function
	ApplyPhaseFunctionElek(scatterR.xyz, scatterM.xyz, dot(rayDir, lightDir.xyz), doSunMie);
	float3 lightInscatter = ATMOSPHERE_THICKNESS * ((scatterR * _WeatherMakerScatteringR + scatterM * _WeatherMakerScatteringM) * _WeatherMakerIncomingLight);

#if defined(WEATHER_MAKER_ATMOSPHERE_RENDER_SUN_SKY)

	lightInscatter += RenderSun(m, dot(rayDir, lightDir.xyz), rayDir);

#endif

	return float4(max(0, lightInscatter * _WeatherMakerSkyAtmosphereParams.z), 1.0);

#endif

}

fixed4 ComputeAtmosphericScatteringFog(float2 uv, float3 rayStart, float3 rayDir, float rayLength, fixed4 background, float depth01)
{
	float3 extinction;
	float3 inscattering;

#if !defined(WEATHER_MAKER_ATMOSPHERE_REFERENCE)

	UNITY_BRANCH
	if (unity_StereoEyeIndex == 0)
	{
		inscattering = UNITY_SAMPLE_TEX3D_LOD(_WeatherMakerInscatteringLUT, float3(uv.x, uv.y, depth01), 0.0);
		extinction = UNITY_SAMPLE_TEX3D_LOD(_WeatherMakerExtinctionLUT, float3(uv.x, uv.y, depth01), 0.0);
	}
	else
	{
		inscattering = UNITY_SAMPLE_TEX3D_LOD(_WeatherMakerInscatteringLUT2, float3(uv.x, uv.y, depth01), 0.0);
		extinction = UNITY_SAMPLE_TEX3D_LOD(_WeatherMakerExtinctionLUT2, float3(uv.x, uv.y, depth01), 0.0);
	}

#else

	float3 planetCenter = ATMOSPHERE_PLANET_CENTER;
	float2 intersection = AtmosphereRaySphereIntersection(rayStart, rayDir, planetCenter, _WeatherMakerAtmospherePlanetRadius + _WeatherMakerAtmosphereHeight);
	rayLength = min(intersection.y, rayLength);

	intersection = AtmosphereRaySphereIntersection(rayStart, rayDir, planetCenter, _WeatherMakerAtmospherePlanetRadius);
	rayLength = lerp(rayLength, min(intersection.x, rayLength), intersection.x > 0.0);
	inscattering = IntegrateInscattering(rayStart, rayDir, rayLength, planetCenter, _WeatherMakerAtmosphereDistanceScale, _WeatherMakerSunDirectionDown, 16, extinction, false).xyz;

#endif					

#if defined(WEATHER_MAKER_ATMOSPHERE_LIGHT_SHAFTS)

	UNITY_BRANCH
	if (_WeatherMakerAtmosphereLightShaftEnable)
	{
		float shadow = WM_SAMPLE_FULL_SCREEN_TEXTURE(_WeatherMakerAtmosphereLightShaftTexture, uv.xy).r;
		inscattering *= shadow;
	}

#endif

	return float4((background.rgb * extinction) + inscattering, background.a);
}

#endif

#endif // WEATHER_MAKER_ATMOSPHERE_SCATTERING_INCLUDE
