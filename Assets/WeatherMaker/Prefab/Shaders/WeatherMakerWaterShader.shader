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

Shader "WeatherMaker/WeatherMakerWaterShader"
{
	Properties
	{
		[Header(Main Textures)]
		[NoScaleOffset] _WaterTex("Water color (RGBA)", 2D) = "white" {}
		[NoScaleOffset] _WaterBumpMap("Water normals (Normal)", 2D) = "bump" {}
		[NoScaleOffset] _WaterFoam("Water foam (RGBA)", 2D) = "clear" {}
		//[NoScaleOffset] _WaterFoamBumpMap("Water foam normals (Normal)", 2D) = "bump" {}
		[NoScaleOffset] _CausticsTexture("Caustics Texture (A)", 3D) = "white" {}
		[NoScaleOffset] _WaterDisplacementMap("Water Displacement Map (A)", 2D) = "clear" {}

		[Header(Appearance)]
		_FresnelScale("FresnelScale", Range(0.15, 4.0)) = 0.75
		_DistortParams("Distortions (Bump waves, Reflection, Fresnel power, Fresnel bias)", Vector) = (1.0 ,1.0, 2.0, 1.15)
		_BumpTiling("Bump Tiling", Vector) = (1.0 ,1.0, -2.0, 3.0)
		_BumpDirection("Bump Direction & Speed", Vector) = (1.0 ,1.0, -1.0, 1.0)
		_InvFadeParameter("Fade parameters (Depth fade, normal fade height, refl strength, normal fade distance)", Vector) = (10.0, 0.002, 1.0, 0.003)
		_WaterDepthThreshold("Water depth threshold", Float) = 0.0
		_WaterRefractionStrength("Refraction strength", Range(0.0, 1.0)) = 1.0

		[Header(Colors)]
		_WaterColor("Surface color", Color) = (.54, .95, .99, 0.1)
		_SpecularColor("Surface specular color", Color) = (.72, .72, .72, 1)
		_SpecularIntensity("Surface specular intensity", Float) = 3.0
		_Shininess("Surface shininess", Range(2.0, 1024.0)) = 200.0
		_WaterFogColor("Water fog color", Color) = (0, 0, 0, 0)
		_WaterFogDensity("Water fog density", Range(0.0, 1.0)) = 0

		[Header(Sparkle)]
		_SparkleTintColor("Surface sparkle Tint", Color) = (0.45, 0.45, 0.45, 1.0)
		_SparkleScale("Sparkle scale - scale, speed, visible threshold (0-1), intensity", Vector) = (1, 128, 1, 8)
		_SparkleOffset("Sparkle offset - animation offsets (x,y,z,w)", Vector) = (32, 64, 128, 256)
		_SparkleFade("Sparkle fade - fade power, distance fade squared, 0, 0", Vector) = (8, 8192, 0, 0)
		[HideInInspector][NoScaleOffset] _SparkleNoise("Sparkle noise (unused)", 2D) = "white" {}

		[Header(Caustics)]
		_WaterCausticsTintColor("Caustics Tint Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_CausticsScale("Caustics Scale (scale, intensity, depth fade, distort multiplier)", Vector) = (0.01, 0.5, 3.0, 1.0)
		_CausticsVelocity("Caustics Animation Velocity (x, y, z, 0)", Vector) = (0.01, 0.02, 5.0, 0.0)

		[Header(Lighting)]
		_WaterShadowStrength("Water shadow strength", Range(0.0, 1.0)) = 1.0
		_DirectionalLightMultiplier("Directional Light Multiplier", Range(0, 10)) = 1
		_PointSpotLightMultiplier("Point/Spot Light Multiplier", Range(0, 10)) = 1
		_AmbientLightMultiplier("Ambient light multiplier", Range(0, 4)) = 0.5

		[Header(Waves)]
		_WaterWave1("1: Dir xz, amp, len", Vector) = (0, 0, 0, 0)
		_WaterWave1_Params1("Speed, height reducer, heightVariance, 0", Vector) = (1, 1, 0, 0)
		_WaterWave2("2: Dir xz, amp, len", Vector) = (0, 0, 0, 0)
		_WaterWave2_Params1("Speed, height reducer, heightVariance, 0", Vector) = (1, 1, 0, 0)
		_WaterWave3("3: Dir xz, amp, len", Vector) = (0, 0, 0, 0)
		_WaterWave3_Params1("Speed, height reducer, heightVariance, 0", Vector) = (1, 1, 0, 0)
		_WaterWave4("4: Dir xz, amp, len", Vector) = (0, 0, 0, 0)
		_WaterWave4_Params1("Speed, height reducer, heightVariance, 0", Vector) = (1, 1, 0, 0)
		_WaterWave5("5: Dir xz, amp, len", Vector) = (0, 0, 0, 0)
		_WaterWave5_Params1("Speed, height reducer, heightVariance, 0", Vector) = (1, 1, 0, 0)
		_WaterWave6("6: Dir xz, amp, len", Vector) = (0, 0, 0, 0)
		_WaterWave6_Params1("Speed, height reducer, heightVariance, 0", Vector) = (1, 1, 0, 0)
		_WaterWave7("7: Dir xz, amp, len", Vector) = (0, 0, 0, 0)
		_WaterWave7_Params1("Speed, height reducer, heightVariance, 0", Vector) = (1, 1, 0, 0)
		_WaterWave8("8: Dir xz, amp, len", Vector) = (0, 0, 0, 0)
		_WaterWave8_Params1("Speed, height reducer, heightVariance, 0", Vector) = (1, 1, 0, 0)
		_WaterWaveMultiplier("Wave Amplitude Multiplier", Range(0.0, 1.0)) = 1.0

		[Header(Displacement)]
		_WaterDisplacement1("1: Scale, amp, vel x, vel z", Vector) = (0, 0, 0, 0)
		_WaterDisplacement2("2: Scale, amp, vel x, vel z", Vector) = (0, 0, 0, 0)

		[Header(Foam)]
		_WaterFoamParam1("Scale, intensity, water amount fade, wave factor", Vector) = (0, 0, 0, 0)
		_WaterFoamParam2("Vel x, vel z, specular reduce, 0", Vector) = (0, 0, 1.0, 0)

		[Header(Shader State)]
		_WaterDitherLevel("Dithering", Range(0.0, 1.0)) = 0.002
		_SrcBlendMode("Source alpha blend", Int) = 5
		_DstBlendMode("Dest alpha blend", Int) = 10
		_Cull("Cull", Int) = 2 // 0 = off, 1 = front, 2 = back
		_ZTest("ZTest", Int) = 4
		_ZWrite("ZWrite", Int) = 0
	}

	
	Subshader
	{
		Tags { "Queue" = "AlphaTest+49" }

		Cull[_Cull]
		ZTest[_ZTest]
		ZWrite[_ZWrite]
		Fog { Mode Off }

		CGINCLUDE

		#pragma target 3.5
		#pragma exclude_renderers gles
		#pragma exclude_renderers d3d9

		#define WEATHER_MAKER_ENABLE_TEXTURE_DEFINES

		ENDCG

		GrabPass { "_CameraOpaqueTexture" }

		Pass
		{
			Tags { }
			Blend[_SrcBlendMode][_DstBlendMode]

			CGPROGRAM

			#pragma vertex vertWater
			#pragma fragment fragWaterForward

			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			#pragma multi_compile_instancing
			#pragma multi_compile WEATHER_MAKER_SHADOWS_ONE_CASCADE WEATHER_MAKER_SHADOWS_SPLIT_SPHERES
			#pragma multi_compile __ UNITY_URP

			#define WEATHER_MAKER_LIGHT_SPECULAR_TRANSLUCENT
			#define NULL_ZONE_RENDER_MASK 8 // water is 8

			#include "WeatherMakerCloudVolumetricShadowsShaderInclude.cginc"
			#include "WeatherMakerWaterShaderInclude.cginc"

			ENDCG
		}

		// shadow caster / depth pass using _WaterDepthThreshold for the base floor of the water
		Pass
		{
			Tags { "LightMode" = "Shadowcaster" }
			Fog{ Mode Off }
			ZWrite [_ZWrite]
            ZTest LEqual
			Offset 1, 1

			CGPROGRAM

			#ifndef UNITY_PASS_SHADOWCASTER
			#define UNITY_PASS_SHADOWCASTER
			#endif

			#define _GLOSSYENV 1

			#pragma vertex vertWaterShadow
			#pragma fragment fragWaterShadow
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			#pragma multi_compile_shadowcaster
			#pragma multi_compile_instancing

			#include "WeatherMakerCloudVolumetricShadowsShaderInclude.cginc"
			#include "WeatherMakerWaterShaderInclude.cginc"

			ENDCG
		}
	}

	/*
	// shadow collector pass
	Subshader
	{
		Pass
		{
			Tags { "LightMode" = "ShadowCollector" }
			Fog {Mode Off}
			ZWrite On ZTest LEqual

			CGPROGRAM

			#define SHADOW_COLLECTOR_PASS

			#pragma vertex vertsc
			#pragma fragment fragsc
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_shadowcollector
			
			#include "WeatherMakerWaterShaderInclude.cginc"

			struct appdata_water_shadow_collect
			{
				float4 vertex : POSITION;
				WM_BASE_VERTEX_INPUT
			};

			struct v2fsc
			{
				V2F_SHADOW_COLLECTOR;
				WM_BASE_VERTEX_TO_FRAG
			};

			v2fsc vertsc(appdata_water_shadow_collect v)
			{
				WM_INSTANCE_VERT(v, v2fsc, o);

				float3 tmpWorldPos, tmpNormal, tmpOffsets;
				ApplyGerstner(v.vertex, tmpWorldPos, tmpNormal, tmpOffsets);
				v.vertex.y -= _WaterDepthThreshold;

				TRANSFER_SHADOW_COLLECTOR(o);

				return o;
			}

			fixed4 fragsc(v2fsc i) : SV_Target
			{
				WM_INSTANCE_FRAG(i);
				SHADOW_COLLECTOR_FRAGMENT(i);
			}

			ENDCG
		}
	}
	*/

	Fallback Off
}
