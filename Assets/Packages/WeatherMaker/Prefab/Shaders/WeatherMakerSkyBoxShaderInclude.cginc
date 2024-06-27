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

#ifndef __WEATHER_MAKER_SKY_BOX_SHADER__
#define __WEATHER_MAKER_SKY_BOX_SHADER__

#include "WeatherMakerLightShaderInclude.cginc"
#include "WeatherMakerAuroraShaderInclude.cginc"
#include "WeatherMakerSkyShaderInclude.cginc"
#include "WeatherMakerAtmosphereShaderInclude.cginc"
#include "WeatherMakerCloudVolumetricAtmosphereShaderInclude.cginc"

fixed4 SkyTexturedColor(fixed4 skyColor, fixed3 nightColor, fixed2 uv)
{
	fixed4 dayColor = tex2D(_DayTex, uv) * _WeatherMakerDayMultiplier;
	fixed4 dawnDuskColor = tex2D(_DawnDuskTex, uv);
	fixed4 dawnDuskColor2 = dawnDuskColor * _WeatherMakerDawnDuskMultiplier;
	dayColor += dawnDuskColor2;

	// hide night texture wherever dawn/dusk is opaque, reduce if clouds
	nightColor *= (1.0 - dawnDuskColor.a);

	// blend texture on top of sky
	fixed4 result = ((dayColor * dayColor.a) + (skyColor * (1.0 - dayColor.a)));

	// blend previous result on top of night
	return ((result * result.a) + (fixed4(nightColor, 1.0) * (1.0 - result.a)));
}

fixed4 SkyNonTexturedColor(fixed4 skyColor, fixed3 nightColor)
{
	return skyColor + fixed4(nightColor, 0.0);
}

fixed4 ComputeSkySphereColorFullScreen(float3 rayDir, float2 uv, float2 screenUV, float depth01, bool doMie, bool doNight)
{
	//return tex2D(_NightTex, uv);

	fixed4 result = fixed4Zero;

	// uncomment next line to mirror sky along y = 0
	// rayDir.y = abs(rayDir.y);

	rayDir = normalize(rayDir);
	float3 origRay = rayDir;
	rayDir.y += _WeatherMakerSkyYOffset2D;
	rayDir = normalize(rayDir);

	fixed4 skyColor;
	fixed3 nightColor;
	fixed sunMoon;

	UNITY_BRANCH
	if (WM_ENABLE_PROCEDURAL_TEXTURED_SKY || WM_ENABLE_PROCEDURAL_SKY || WM_ENABLE_PROCEDURAL_SKY_ATMOSPHERE || WM_ENABLE_PROCEDURAL_TEXTURED_SKY_ATMOSPHERE)
	{
		fixed4 skyColor;
		fixed groundAlpha = (_WeatherMakerSkyGroundColor.a * (origRay.y < 0.0));

		UNITY_BRANCH
		if (WM_ENABLE_PROCEDURAL_TEXTURED_SKY || WM_ENABLE_PROCEDURAL_SKY)
		{
			procedural_sky_info sky = CalculateScatteringCoefficients(_WeatherMakerSunDirectionUp, _WeatherMakerSunColor.rgb, 1.0, rayDir);
			procedural_sky_info sky2 = CalculateScatteringColor(_WeatherMakerSunDirectionUp, _WeatherMakerSunColor.rgb, _WeatherMakerSunVar1.x, rayDir, sky.inScatter, sky.outScatter, doMie);
			skyColor.rgb = sky2.skyColor.rgb;
			skyColor.rgb *= skyTintColor;
			skyColor.a = min(1.0, _NightDuskMultiplier * max(skyColor.r, max(skyColor.g, skyColor.b)));
		}
		else
		{
			fixed lightColorA = _WeatherMakerDirLightColor[0].a;
			lightColorA *= lightColorA;
			lightColorA *= lightColorA;
			lightColorA *= lightColorA;
			lightColorA = min(1.0, lightColorA);
			skyColor.rgb = ComputeAtmosphericScatteringSkyColor(rayDir, doMie);
			skyColor.rgb *= skyTintColor * lightColorA;
			skyColor.a = min(1.0, _NightDuskMultiplier * max(skyColor.r, max(skyColor.g, skyColor.b)));
		}

		skyColor.rgb = lerp(skyColor.rgb, _WeatherMakerSkyGroundColor.rgb, groundAlpha);
		nightColor = (doNight ? lerp(GetNightColor(origRay, uv, skyColor.a), _WeatherMakerSkyGroundColor.rgb, groundAlpha) : fixed4Zero);

		UNITY_BRANCH
		if (WM_ENABLE_PROCEDURAL_TEXTURED_SKY)
		{
			result = SkyTexturedColor(skyColor, nightColor, uv);
		}
		else
		{
			result = SkyNonTexturedColor(skyColor, nightColor);
		}
	}
	else // WM_ENABLE_TEXTURED_SKY
	{
		nightColor = (doNight ? GetNightColor(origRay, uv, 0.0) : fixed4Zero);
		fixed4 dayColor = tex2D(_DayTex, uv) * _WeatherMakerDayMultiplier;
		fixed4 dawnDuskColor = (tex2D(_DawnDuskTex, uv) * _WeatherMakerDawnDuskMultiplier);
		result = (dayColor + dawnDuskColor + fixed4(nightColor, 0.0));
	}

	result.rgb += _WeatherMakerSkyAddColor;
	result.rgb += abs(RandomFloat(origRay) * _WeatherMakerSkyDitherLevel);
	result.a = 1.0;

	return result;
}

fixed4 ComputeSkySphereColorVolumetric(wm_volumetric_data i, bool doMie, bool doNight)
{
	// use depth buffer and camera frustum to get an exact world position and take that distance from the camera
	float3 rayDir = i.rayDir;
	float2 screenUV = i.projPos.xy / max(0.0001, i.projPos.w);
	float depth01 = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(screenUV);
	return ComputeSkySphereColorFullScreen(rayDir, i.uv.xy, screenUV, depth01, doMie, true);
}

#endif // __WEATHER_MAKER_SKY_BOX_SHADER__