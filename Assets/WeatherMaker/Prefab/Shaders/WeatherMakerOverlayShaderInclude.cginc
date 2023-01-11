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

#ifndef __WEATHER_MAKER_OVERLAY_SHADER__
#define __WEATHER_MAKER_OVERLAY_SHADER__

#include "WeatherMakerFogShaderInclude.cginc"

// define WEATHER_MAKER_OVERLAY_ENABLE_REFLECTION(reflColor, overlayColor) return finalColor to turn on reflection probe

uniform sampler2D _OverlayTexture;
uniform sampler2D _OverlayNormalTexture;

uniform float _OverlayIntensity;
uniform float _OverlayReflectionIntensity;
uniform float _OverlayNormalReducer;
uniform float _OverlayScale;
uniform fixed2 _OverlayOffset;
uniform fixed2 _OverlayVelocity;
uniform fixed3 _OverlayColor;
uniform fixed4 _OverlaySpecularColor;
uniform fixed _OverlaySpecularIntensity = 4.0;
uniform fixed _OverlaySpecularPower = 4.0;

uniform float _OverlayMinHeight;
uniform fixed _OverlayMinHeightNoiseMultiplier;
uniform fixed _OverlayMinHeightNoiseAdder;
uniform fixed _OverlayMinHeightFalloffMultiplier;
uniform fixed _OverlayMinHeightFalloffPower;

uniform sampler2D _OverlayNoiseHeightTexture;
uniform fixed _OverlayMinHeightNoiseScale;
uniform fixed2 _OverlayMinHeightNoiseOffset;
uniform fixed2 _OverlayMinHeightNoiseVelocity;

uniform sampler2D _OverlayNoiseTexture;
uniform fixed _OverlayNoiseMultiplier;
uniform fixed _OverlayNoisePower;
uniform fixed _OverlayNoiseAdder;
uniform fixed _OverlayNoiseScale;
uniform fixed2 _OverlayNoiseOffset;
uniform fixed2 _OverlayNoiseVelocity;

uniform int _OverlayNoiseEnabled;
uniform int _OverlayMinHeightNoiseEnabled;
#define WM_OVERLAY_NOISE_ENABLED (_OverlayNoiseEnabled)
#define WM_OVERLAY_MIN_HEIGHT_ENABLED (_OverlayMinHeight > 0.0)
#define WM_OVERLAY_MIN_HEIGHT_NOISE_ENABLED (_OverlayMinHeightNoiseEnabled)

// reflectionColor.a is intensity of reflection
fixed4 ComputeOverlayColor(float3 worldPos, float3 rayDir, float3 normal, float depth, float2 uv)
{
	fixed alpha = ClipWorldPosNullZonesAlpha(worldPos);
	float noiseMultiplier = 1.0;

	if (WM_OVERLAY_MIN_HEIGHT_ENABLED)
	{
		float worldPosYWithNoise = worldPos.y;

		if (WM_OVERLAY_MIN_HEIGHT_NOISE_ENABLED)
		{
			// sample noise from texture
			float heightNoise = (CalculateNoiseXZ(_OverlayNoiseHeightTexture, worldPos, _OverlayMinHeightNoiseScale, _OverlayMinHeightNoiseOffset, _OverlayMinHeightNoiseVelocity, _OverlayMinHeightNoiseMultiplier, _OverlayMinHeightNoiseAdder) - 0.5) * 15.0;
			worldPosYWithNoise += heightNoise;

			// if world pos y is too low, clip
			clip(0.5 - (worldPosYWithNoise < _OverlayMinHeight));
		}
		else
		{
			// basic noise function
			float heightNoise = _OverlayMinHeightNoiseAdder + ((sin(worldPos.x * 0.5) * 4.0 * _OverlayMinHeightNoiseMultiplier) - (sin(worldPos.z * 0.5) * 4.0) * _OverlayMinHeightNoiseMultiplier);
			worldPosYWithNoise += heightNoise;

			// if world pos y is too low, clip
			clip(0.5 - (worldPosYWithNoise < _OverlayMinHeight));
		}

		noiseMultiplier = pow(min(1.0, (worldPosYWithNoise / _OverlayMinHeight) * _OverlayMinHeightFalloffMultiplier), _OverlayMinHeightFalloffPower);
	}

	float noise = 1.0;
	if (WM_OVERLAY_NOISE_ENABLED)
	{
		noise = CalculateNoiseXZ(_OverlayNoiseTexture, worldPos, _OverlayNoiseScale, _OverlayNoiseOffset, _OverlayNoiseVelocity, _OverlayNoiseMultiplier, _OverlayNoiseAdder);
		noise = pow(abs(noise), _OverlayNoisePower) * sign(noise) * noiseMultiplier;
	}

	float normalReducer = max(0.0, (normal.y - _OverlayNormalReducer));
	float overlayAmount = normalReducer * _OverlayIntensity;
	float4 overlayLookup = float4((worldPos.xz * _OverlayScale) + _OverlayOffset + (max(0.1, normal.y) * _OverlayVelocity), 0.0, 0.0);
	fixed4 overlayColor = tex2Dlod(_OverlayTexture, overlayLookup);

	overlayAmount = saturate(overlayAmount * overlayColor.a * noise);

#if defined(WEATHER_MAKER_OVERLAY_ENABLE_REFLECTION)

	fixed4 reflectionColor;
	reflectionColor.rgb = ComputeReflectionColor(worldPos, rayDir, normal, uv, _OverlaySpecularColor);
	reflectionColor.a = _OverlayReflectionIntensity;

#endif

	// if no overlay at all, just exit out, avoid pointless light calculations
	clip(-0.0001 + overlayAmount);

	alpha *= overlayAmount;
	wm_world_space_light_params p;
	p.worldPos = worldPos;
	p.worldNormal = normal;
	p.diffuseColor = overlayColor.rgb;

#if !defined(WEATHER_MAKER_LIGHT_NO_SPECULAR)

	fixed4 specularColor = fixed4(_OverlaySpecularColor.rgb, _OverlaySpecularIntensity);
	p.rayDir = rayDir;
	p.specularColor = specularColor.rgb * specularColor.a;
	p.specularPower = _OverlaySpecularPower;

#endif

	p.ambientColor = _WeatherMakerAmbientLightColorGround;
	p.shadowStrength = 1.0;

#if defined(WEATHER_MAKER_SHADOWS_SCREEN)

	p.uv = uv;

#endif

	overlayColor.rgb = CalculateLightColorWorldSpace(p) * _OverlayColor * overlayAmount * alpha;

#if defined(WEATHER_MAKER_OVERLAY_ENABLE_REFLECTION)

	overlayColor = WEATHER_MAKER_OVERLAY_ENABLE_REFLECTION(reflectionColor, overlayColor);

#endif

	ApplyDither(overlayColor.rgb, uv, 0.0002);

	return fixed4(overlayColor.rgb, alpha);
}

bool GetOverlayInputs(float2 screenUV, float3 forwardLine, out float3 rayDir, out float3 worldPos, out float3 normal, out float depth)
{
	UNITY_BRANCH
	if (_WeatherMakerDeferredShading)
	{
		float3 gBufferNormal = WM_SAMPLE_FULL_SCREEN_TEXTURE(_CameraGBufferTexture2, screenUV.xy).xyz;

		// if normal is 0,0,0 then exit out early, usually this is the far plane if nothing rendered
		clip(-0.5 + (gBufferNormal.x != 0.0 || gBufferNormal.y != 0.0 || gBufferNormal.z != 0.0));

		// normal is already world space

#if UNITY_URP

		// URP normals are signed and already normalized
		normal = gBufferNormal;

#else

		normal = normalize((gBufferNormal * 2.0) - 1.0);

#endif

		depth = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(screenUV);
	}
	else
	{
		// forward rendering
		depth;
		normal;
		GetDepth01AndNormal(screenUV, depth, normal);

		// normals at far plane are not zero sadly, so we have to exit out to eliminate far plane artifacts
		clip(-0.5 + (depth <= 0.7));

		if (normal.x != 0.0 || normal.y != 0.0 || normal.z != 0.0)
		{
			normal = mul((float3x3)_WeatherMakerInverseView[unity_StereoEyeIndex], normal).xyz;
		}
	}

	rayDir = GetFullScreenRayDir(forwardLine);
	
	if (WM_CAMERA_RENDER_MODE_CUBEMAP)
	{
		worldPos = WEATHER_MAKER_CAMERA_POS + (rayDir * depth * _ProjectionParams.z);
	}
	else
	{
		worldPos = WEATHER_MAKER_CAMERA_POS + (depth * forwardLine);
	}

	return (normal.x != 0.0 || normal.y != 0.0 || normal.z != 0.0);
}

#endif
