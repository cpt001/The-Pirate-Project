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

#ifndef __WEATHER_MAKER_SKY_SHADER__
#define __WEATHER_MAKER_SKY_SHADER__

#include "WeatherMakerLightShaderInclude.cginc"
#include "WeatherMakerAuroraShaderInclude.cginc"

#ifndef SKYBOX_COLOR_IN_TARGET_COLOR_SPACE
#if defined(SHADER_API_MOBILE)
#define SKYBOX_COLOR_IN_TARGET_COLOR_SPACE 1
#else
#define SKYBOX_COLOR_IN_TARGET_COLOR_SPACE 0
#endif
#endif

struct procedural_sky_info
{
	fixed3 inScatter;
	fixed3 outScatter;
	fixed3 skyColor;
};

struct v2fSky
{
	float4 vertex : SV_POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
	float4 ray : TEXCOORD1;
	float4 projPos : TEXCOORD2;
	float4 viewPos : TEXCOORD3;
	WM_BASE_VERTEX_TO_FRAG
};

struct v2fSkyLerp
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 ray : TEXCOORD1;
	float3 normal : NORMAL;
	fixed3 inScatter : COLOR0;
	fixed3 outScatter : COLOR1;
	WM_BASE_VERTEX_TO_FRAG
};

uniform sampler2D _DayTex;
uniform float4 _DayTex_ST;
uniform sampler2D _DawnDuskTex;
uniform float4 _DawnDuskTex_ST;
uniform sampler2D _NightTex;
uniform float4 _NightTex_ST;
uniform fixed _NightSkyMultiplier;
uniform fixed _NightVisibilityThreshold;
uniform fixed _NightIntensity;
uniform fixed _NightPower;
uniform fixed _NightTwinkleSpeed;
uniform fixed _NightTwinkleVariance;
uniform fixed _NightTwinkleMinimum;
uniform fixed _NightTwinkleRandomness;
uniform fixed _NightDuskMultiplier;
uniform fixed3 _NightFogColor;
uniform fixed _NightFogPower;

uniform fixed4 _WeatherMakerSkyMieG; // -mieG, mieG * mieG, mieG, sun mieDot
uniform fixed4 _WeatherMakerSkyRadius; // outer, outer * outer, inner, inner * inner
uniform fixed4 _WeatherMakerSkyMie; // x, y, z, w
uniform fixed4 _WeatherMakerSkyLightScattering;
uniform fixed4 _WeatherMakerSkyLightPIScattering;
uniform fixed4 _WeatherMakerSkyScale; // scale factor, scale depth, scale / scale depth, camera height
uniform fixed4 _WeatherMakerSkyTotalRayleigh; // w = sun fade
uniform fixed4 _WeatherMakerSkyTotalMie; // w = total sun intensity
uniform fixed3 _WeatherMakerSkyTintColor;
uniform fixed3 _WeatherMakerSkyTintColor2;
uniform fixed3 _WeatherMakerSkyAddColor;
uniform fixed4 _WeatherMakerSkyGroundColor;

uniform float4 _WeatherMakerSkyRotation;
 
uniform fixed _WeatherMakerSkyDitherLevel;
uniform int _WeatherMakerSkyAbsRayY = 0;
uniform fixed4 _WeatherMakerSkyFade;

uniform int _WeatherMakerSkyEnableSunEclipse;
#define WM_ENABLE_SKY_SUN_ECLIPSE (_WeatherMakerSkyEnableSunEclipse)

uniform int _WeatherMakerSkyEnableNightTwinkle;
#define WM_ENABLE_SKY_NIGHT_TWINKLE (_WeatherMakerSkyEnableNightTwinkle)

uniform int _WeatherMakerSkyRenderType;
#define WM_ENABLE_TEXTURED_SKY (_WeatherMakerSkyRenderType == 0)
#define WM_ENABLE_PROCEDURAL_TEXTURED_SKY (_WeatherMakerSkyRenderType == 1)
#define WM_ENABLE_PROCEDURAL_SKY (_WeatherMakerSkyRenderType == 2)
#define WM_ENABLE_PROCEDURAL_TEXTURED_SKY_ATMOSPHERE (_WeatherMakerSkyRenderType == 3)
#define WM_ENABLE_PROCEDURAL_SKY_ATMOSPHERE (_WeatherMakerSkyRenderType == 4)

static const float weatherMakerNightMultiplierSquared = _WeatherMakerNightMultiplier * _WeatherMakerNightMultiplier;
static const float weatherMakerNightFogMultiplier = _WeatherMakerDayMultiplierInv;

uniform fixed _WeatherMakerSkyYOffset2D;

static const float3 skyTintColor = _WeatherMakerSkyTintColor * _WeatherMakerSkyTintColor2;

fixed GetMiePhase(fixed size, fixed g, fixed g2, fixed eyeCos, fixed eyeCos2, fixed power)
{
	fixed temp = 1.0 + g2 + (2.0 * g * eyeCos);
	temp = max(1.0e-4, smoothstep(0.0, 0.005, temp) * temp);
	fixed mie = saturate(size * _WeatherMakerSkyMie.x * ((1.0 + eyeCos2) / temp));
	return pow(mie, power);
}

fixed GetSkyMiePhase(fixed eyeCos, fixed eyeCos2)
{
	return (_WeatherMakerSkyMie.x * (1.0 + eyeCos2) / pow((_WeatherMakerSkyMie.y + _WeatherMakerSkyMie.z * eyeCos), 1.5));
}

fixed GetRayleighPhase(fixed eyeCos2)
{
	return 0.75 + 0.75 * eyeCos2;
}

fixed GetRayleighPhase(fixed3 light, fixed3 ray)
{
	fixed eyeCos = dot(light, ray);
	return GetRayleighPhase(eyeCos * eyeCos);
}

fixed CalcSunSpot(fixed size, fixed3 vec1, fixed3 vec2)
{
	half3 delta = vec1 - vec2;
	half dist = length(delta);
	half spot = 1.0 - smoothstep(0.0, size, dist);
	return saturate(100 * spot * spot);
}

fixed4 GetSunColorFast(float3 sunNormal, fixed4 sunColor, fixed size, float3 ray)
{
	fixed sun = CalcSunSpot(size, sunNormal, ray);
	return (sun * sunColor);
}

float GetSkyScale(float inCos)
{
	float x = 1.0 - inCos;
#if defined(SHADER_API_N3DS)
	// The polynomial expansion here generates too many swizzle instructions for the 3DS vertex assembler
	// Approximate by removing x^1 and x^2
	return 0.25 * exp(-0.00287 + x * x * x * (-6.80 + x * 5.25));
#else
	return 0.25 * exp(-0.00287 + x * (0.459 + x * (3.83 + x * (-6.80 + x * 5.25))));
#endif

}

procedural_sky_info CalculateScatteringCoefficients(float3 lightDir, fixed3 lightColor, float scale, float3 eyeRay)
{
	procedural_sky_info o;
	eyeRay.y = lerp(eyeRay.y, abs(eyeRay.y), _WeatherMakerSkyAbsRayY);

	static const float outerRadius = _WeatherMakerSkyRadius.x;
	static const float outerRadius2 = _WeatherMakerSkyRadius.y;
	static const float innerRadius = _WeatherMakerSkyRadius.z;
	static const float innerRadius2 = _WeatherMakerSkyRadius.w;
	static const float scaleDepth = _WeatherMakerSkyScale.y;
	static const float scaleFactorOverDepth = _WeatherMakerSkyScale.z;
	static const float cameraHeight = _WeatherMakerSkyScale.w;
	float scaleFactor = _WeatherMakerSkyScale.x * scale;

	// the following is copied from Unity procedural sky shader
	float3 cameraPosition = float3(0.0, innerRadius + cameraHeight, 0.0);
	float far = sqrt(outerRadius2 + innerRadius2 * eyeRay.y * eyeRay.y - innerRadius2) - innerRadius * eyeRay.y;
	float startDepth = exp(scaleFactorOverDepth * (-cameraHeight));
	float startAngle = dot(eyeRay, cameraPosition) / (innerRadius + cameraHeight);
	float startOffset = startDepth * GetSkyScale(startAngle);
	float sampleLength = far * 0.5; // far / sampleCount
	float scaledLength = sampleLength * scaleFactor;
	float3 sampleRay = eyeRay * sampleLength;
	float3 samplePoint = cameraPosition + sampleRay * 0.5;
	float3 color = float3(0.0, 0.0, 0.0);

	// Loop through the sample rays
	UNITY_UNROLL
	for (uint i = 0; i < 2; i++)
	{
		float height = length(samplePoint);
		float invHeight = 1.0 / height;
		float depth = exp(scaleFactorOverDepth * (innerRadius - height));
		float scaleAtten = depth * scaledLength;
		float eyeAngle = dot(eyeRay, samplePoint) * invHeight;
		float lightAngle = dot(lightDir, samplePoint) * invHeight;
		float lightScatter = startOffset + depth * (GetSkyScale(lightAngle) - GetSkyScale(eyeAngle));
		float3 lightAtten = exp(-lightScatter * (_WeatherMakerSkyLightPIScattering.xyz + _WeatherMakerSkyLightPIScattering.w));
		color += (lightAtten * scaleAtten);
		samplePoint += sampleRay;
	}

	o.inScatter = lightColor * color * _WeatherMakerSkyLightScattering.xyz;
	o.outScatter = lightColor * color * _WeatherMakerSkyLightScattering.w;

	return o;
}

procedural_sky_info CalculateScatteringColor(float3 lightDir, fixed3 lightColor, fixed sunSize, float3 eyeRay,
	fixed3 inScatter, fixed3 outScatter, bool doSunMie)
{
	eyeRay.y = abs(eyeRay.y);
	float eyeCos = dot(lightDir, eyeRay);
	float eyeCos2 = eyeCos * eyeCos;
	procedural_sky_info o;
	o.inScatter = inScatter;
	o.outScatter = outScatter;

	o.skyColor = GetRayleighPhase(eyeCos2) * inScatter;

	if (doSunMie)
	{
		o.skyColor += (outScatter * GetSkyMiePhase(eyeCos, eyeCos2));
	}
	
	/*
	UNITY_BRANCH
	if (drawSunDisk)
	{
		o.skyColor.rgb += GetMiePhase(sunSize, eyeCos, eyeCos2, 1.18) * outScatter;
	}
	*/

#if defined(UNITY_COLORSPACE_GAMMA) && SKYBOX_COLOR_IN_TARGET_COLOR_SPACE

	o.skyColor.rgb = sqrt(o.skyColor.rgb);

#endif

	return o;
}

fixed3 CalculateSkyColorUnityStyleFragment(float3 eyeRay, bool doSunMie)
{
	UNITY_BRANCH
	if (_WeatherMakerSunDirectionUp.y < -0.3)
	{
		return fixed4Zero;
	}
	else
	{
		procedural_sky_info info = CalculateScatteringCoefficients(_WeatherMakerSunDirectionUp, _WeatherMakerSunColor.rgb, 1.0, eyeRay);
		info = CalculateScatteringColor(_WeatherMakerSunDirectionUp, _WeatherMakerSunColor.rgb, 0.0,
			eyeRay, info.inScatter, info.outScatter, doSunMie);
		return info.skyColor;
	}
}

fixed3 Uncharted2Tonemap(fixed3 x)
{
	const float A = 0.15;
	const float B = 0.50;
	const float C = 0.10;
	const float D = 0.20;
	const float E = 0.02;
	const float F = 0.30;
	return ( ( x * ( A * x + C * B ) + D * E ) / ( x * ( A * x + B ) + D * F ) ) - E / F;
}

fixed3 GetNightColor(float3 ray, float2 uv, float light)
{
    fixed3 nightFog = (weatherMakerNightFogMultiplier * pow((1.0 - abs(ray.y)), _NightFogPower) * _NightFogColor);

	UNITY_BRANCH
	if (_WeatherMakerNightMultiplier > 0.0)
	{

#if defined(WEATHER_MAKER_SKY_BOX_SHADER)

		// assume sphere uv mapping
		float3 rotatedRay = RotatePointZeroOriginQuaternion(ray, _WeatherMakerSkyRotation);
		uv.x = WrapFloat(atan2(rotatedRay.x, rotatedRay.z) / (-2.0 * PI));
		uv.y = WrapFloat(asin(rotatedRay.y) / PI + 0.5);

#endif

		fixed3 nightColor = tex2D(_NightTex, uv).rgb * _NightIntensity;
		nightColor *= (nightColor >= _NightVisibilityThreshold);
		fixed maxValue = max(nightColor.r, max(nightColor.g, nightColor.b));

		if (WM_ENABLE_SKY_NIGHT_TWINKLE)
		{
			fixed twinkleRandom = _NightTwinkleRandomness * RandomFloat2D(uv * _WeatherMakerTime.y);
			fixed twinkle = (maxValue > _NightTwinkleMinimum) * (twinkleRandom + (_NightTwinkleVariance * sin(_NightTwinkleSpeed * _WeatherMakerTime.y * maxValue)));
			nightColor *= (1.0 + twinkle);
		}

		// apply moon glow
		UNITY_LOOP
		for (uint i = 0; i < uint(_WeatherMakerDirLightCount); i++)
		{
			UNITY_BRANCH
			if (_WeatherMakerDirLightDirection[i].w == 0.0) // not a sun
			{
				fixed moonDot = pow(max(0.0, dot(_WeatherMakerDirLightPosition[i].xyz, ray)), _WeatherMakerDirLightPower[i].x);
				nightColor += (moonDot * _WeatherMakerDirLightColor[i].rgb * _WeatherMakerDirLightColor[i].a * _WeatherMakerDirLightPower[i].y
				
#if defined(UNITY_COLORSPACE_GAMMA)

					* 5.0

#endif
				
				);
			}
		}

        fixed nightIntensity = pow(_NightIntensity * _NightSkyMultiplier * (1.0 - light), _NightPower);
        nightColor *= nightIntensity;

#if defined(UNITY_COLORSPACE_GAMMA)

        nightColor *= 3.0;

#endif

        nightColor += nightFog;
        return nightColor;
	}
    else
    {
        return nightFog;
    }
}

#endif // __WEATHER_MAKER_SKY_SHADER__