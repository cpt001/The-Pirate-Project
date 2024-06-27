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

Shader "WeatherMaker/WeatherMakerBoxFogShader"
{
	Properties
	{
		[KeywordEnum(None, Constant, Linear, Exponential, ExponetialSquared)] _WeatherMakerFogMode("Fog Mode", Int) = 2
		_WeatherMakerFogColor("Fog Color", Color) = (0,1,1,1)
		_WeatherMakerFogNoiseScale("Fog Noise Scale", Range(0.0, 1.0)) = 0.0005
		_WeatherMakerFogNoiseMultiplier("Fog Noise Multiplier", Range(0.01, 1.0)) = 0.15
		_WeatherMakerFogNoiseVelocity("Fog Noise Velocity", Vector) = (0.01, 0.01, 0, 0)
		_WeatherMakerFogDensity("Fog Density", Range(0.0, 1.0)) = 0.05
		[PerRendererData] _WeatherMakerFogSpherePosition("Fog Sphere Position (x,y,z,radius squared)", Vector) = (0, 0, 0, 0)
		[PerRendererData] _WeatherMakerFogBoxCenter("Fog Sphere Center", Vector) = (0, 0, 0, 0)
		[PerRendererData] _WeatherMakerFogVolumePower("Fog Sphere Power", Vector) = (0, 0, 0, 0)
		_WeatherMakerFogFactorMax("Maximum Fog Factor", Range(0.01, 1)) = 1
		_PointSpotLightMultiplier("Point/Spot Light Multiplier", Range(0, 10)) = 1
		_DirectionalLightMultiplier("Directional Light Multiplier", Range(0, 10)) = 1
		_AmbientLightMultiplier("Ambient Light Multiplier", Range(0, 10)) = 2
	}
	Category
	{
		Tags{ "Queue" = "Transparent-10" }
		Cull Front Lighting Off ZWrite Off ZTest Always Fog { Mode Off }
		ColorMask RGBA
		Blend One OneMinusSrcAlpha

		CGINCLUDE

		#pragma target 3.5
		#pragma exclude_renderers gles
		#pragma exclude_renderers d3d9
		
		#define WEATHER_MAKER_ENABLE_TEXTURE_DEFINES

		ENDCG

		SubShader
		{
			Pass
			{
				CGPROGRAM

				#pragma vertex fog_volume_vertex_shader
				#pragma fragment fog_sphere_fragment_shader
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma glsl_no_auto_normalization
				#pragma multi_compile_instancing
				#pragma multi_compile WEATHER_MAKER_SHADOWS_ONE_CASCADE WEATHER_MAKER_SHADOWS_SPLIT_SPHERES
				#pragma multi_compile __ UNITY_URP

				#define WEATHER_MAKER_FOG_VOLUME
				#define NULL_ZONE_RENDER_MASK 2 // fog is 2

				#include "WeatherMakerFogVertFragShaderInclude.cginc"

				ENDCG
			}
		}
	}
	Fallback "VertexLit"
}
