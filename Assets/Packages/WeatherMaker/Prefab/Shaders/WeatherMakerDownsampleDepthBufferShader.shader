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

Shader "WeatherMaker/WeatherMakerDownsampleDepthBufferShader"
{
	Properties
	{
	}
	Subshader
	{
		Cull Off Lighting Off ZWrite Off ZTest Always Fog { Mode Off } Blend One Zero

		CGINCLUDE

		#pragma target 3.5
		#pragma exclude_renderers gles
		#pragma exclude_renderers d3d9

		#define WEATHER_MAKER_ENABLE_TEXTURE_DEFINES
		#define WEATHER_MAKER_IS_FULL_SCREEN_EFFECT

#if defined(UNITY_REVERSED_Z)
		#define DEPTH_SAMPLE_MODE 1 // 0 = max, 1 = min, 2 = alternate max/min
		#define DEPTH_OFFSET1 -0.5
		#define DEPTH_OFFSET2 0.5
#else
		#define DEPTH_SAMPLE_MODE 0 // 0 = max, 1 = min, 2 = alternate max/min
		#define DEPTH_OFFSET1 -0.5
		#define DEPTH_OFFSET2 0.5
#endif

		// snap uv to center
		#define SNAP_UV(uv)// uv = (floor((uv * _DepthTexelSizeSource.zw) + 0.5)) * _DepthTexelSizeSource.xy;

		#include "WeatherMakerCoreShaderInclude.cginc"

		uniform float4 _DepthTexelSizeSource;
        uniform float4 _DepthTexelSizeDest;

		static const float2 tapsDepth1 = (float2(DEPTH_OFFSET1, DEPTH_OFFSET1) * _DepthTexelSizeSource);
		static const float2 tapsDepth2 = (float2(DEPTH_OFFSET2, DEPTH_OFFSET1) * _DepthTexelSizeSource);
		static const float2 tapsDepth3 = (float2(DEPTH_OFFSET1, DEPTH_OFFSET2) * _DepthTexelSizeSource);
		static const float2 tapsDepth4 = (float2(DEPTH_OFFSET2, DEPTH_OFFSET2) * _DepthTexelSizeSource);

#define SAMPLE_DEPTH_4(source, uv) \
		float depth1 = UNITY_SAMPLE_DEPTH(SAMPLE_DEPTH_TEXTURE(source, uv + tapsDepth1)); \
		float depth2 = UNITY_SAMPLE_DEPTH(SAMPLE_DEPTH_TEXTURE(source, uv + tapsDepth2)); \
		float depth3 = UNITY_SAMPLE_DEPTH(SAMPLE_DEPTH_TEXTURE(source, uv + tapsDepth3)); \
		float depth4 = UNITY_SAMPLE_DEPTH(SAMPLE_DEPTH_TEXTURE(source, uv + tapsDepth4));

#if DEPTH_SAMPLE_MODE == 0 // max

#define DownsampleDepth(source, uv) \
		SNAP_UV(uv) \
		SAMPLE_DEPTH_4(source, uv); \
		return (max(depth1, max(depth2, max(depth3, depth4))));

#elif DEPTH_SAMPLE_MODE == 1 // min

#define DownsampleDepth(source, uv) \
		SNAP_UV(uv) \
		SAMPLE_DEPTH_4(source, uv); \
		return (min(depth1, min(depth2, min(depth3, depth4))));

#elif DEPTH_SAMPLE_MODE == 2 // checkerboard

#define DownsampleDepth(source, uv) \
		SNAP_UV(uv) \
		SAMPLE_DEPTH_4(source, uv); \
		float minDepth = min(min(depth1, depth2), min(depth3, depth4)); \
		float maxDepth = max(max(depth1, depth2), max(depth3, depth4)); \
		int2 position = floor(uv * _DepthTexelSizeDest.zw); \
		int index = fmod(position.x + position.y, 2.0); \
		return lerp(maxDepth, minDepth, index);

#else

#error Invalid downsample mode

#endif

		wm_full_screen_fragment_vertex_uv vert(wm_full_screen_vertex v)
		{
			WM_INSTANCE_VERT(v, wm_full_screen_fragment_vertex_uv, o);
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = AdjustFullScreenUV(v.uv);
			return o;
		}

		float4 frag1(wm_full_screen_fragment_vertex_uv i) : SV_Target
		{
			WM_INSTANCE_FRAG(i);
			float depth = (UNITY_SAMPLE_DEPTH(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy)));
            return float4(depth, depth, depth, depth);
		}

		float4 frag2(wm_full_screen_fragment_vertex_uv i) : SV_Target
		{
			WM_INSTANCE_FRAG(i);

#if UNITY_VERSION >= 201901 && UNITY_VERSION < 201903

			// Unity 2019.1 and 2019.2 depth buffer are upside down in VR
			i.uv.y = lerp(i.uv.y, 1.0 - i.uv.y, _WeatherMakerVREnabled);

#endif

			DownsampleDepth(_CameraDepthTexture, i.uv.xy);
		}

		float4 frag3(wm_full_screen_fragment_vertex_uv i) : SV_Target
		{
			WM_INSTANCE_FRAG(i);
			DownsampleDepth(_CameraDepthTextureHalf, i.uv.xy);
		}

		float4 frag4(wm_full_screen_fragment_vertex_uv i) : SV_Target
		{
			WM_INSTANCE_FRAG(i);
			DownsampleDepth(_CameraDepthTextureQuarter, i.uv.xy);
		}

		ENDCG

		Pass // straight up copy
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag1
			#pragma multi_compile_instancing

			ENDCG
		}

		Pass // copy full depth texture to half
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag2
			#pragma multi_compile_instancing

			ENDCG
		}

		Pass // copy half depth texture to quarter
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag3
			#pragma multi_compile_instancing

			ENDCG
		}

		Pass // copy quarter depth texture to eighth
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag4
			#pragma multi_compile_instancing

			ENDCG
		}
	}

	Fallback Off
}