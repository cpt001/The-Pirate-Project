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

Shader "WeatherMaker/WeatherMakerFullScreenFogShader"
{
	Properties
	{
		_PointSpotLightMultiplier("Point/Spot Light Multiplier", Range(0, 10)) = 1
		_DirectionalLightMultiplier("Directional Light Multiplier", Range(0, 10)) = 1
		_AmbientLightMultiplier("Ambient Light Multiplier", Range(0, 10)) = 2
	}
	Category
	{
		Cull Off Lighting Off ZWrite Off ZTest Always Fog { Mode Off }

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
				Blend [_SrcBlendMode][_DstBlendMode]

				CGPROGRAM

				#pragma vertex full_screen_vertex_shader
				#pragma fragment temporal_reprojection_fragment_custom
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma glsl_no_auto_normalization
				#pragma multi_compile_instancing
				#pragma multi_compile WEATHER_MAKER_SHADOWS_ONE_CASCADE WEATHER_MAKER_SHADOWS_SPLIT_SPHERES
				#pragma multi_compile __ UNITY_URP

				#define WEATHER_MAKER_IS_FULL_SCREEN_EFFECT
				#define NULL_ZONE_RENDER_MASK 2 // fog is 2

				#include "WeatherMakerFogVertFragShaderInclude.cginc"

				#define WEATHER_MAKER_TEMPORAL_REPROJECTION_FRAGMENT_TYPE wm_full_screen_fragment
				#define WEATHER_MAKER_TEMPORAL_REPROJECTION_FRAGMENT_FUNC fog_box_full_screen_fragment_shader
				#define WEATHER_MAKER_TEMPORAL_REPROJECTION_BLEND_FUNC blendFogTemporal
				#define WEATHER_MAKER_TEMPORAL_REPROJECTION_OFF_SCREEN_FUNC offScreenFogTemporal

				// comment out to disable neighborhood clamping, generally leaving this on is much better than off
				#define WEATHER_MAKER_TEMPORAL_REPROJECTION_NEIGHBORHOOD_CLAMPING

				// leave commented out unless testing performance, red areas are full shader runs, try to minimize these
				// #define WEATHER_MAKER_TEMPORAL_REPROJECTION_SHOW_OVERDRAW fixed4(1,0,0,1)

				fixed4 blendFogTemporal(fixed4 prev, fixed4 cur, fixed4 diff, float4 uv, wm_full_screen_fragment i);
				fixed4 offScreenFogTemporal(fixed4 prev, fixed4 cur, float4 uv, wm_full_screen_fragment i);

				#include "WeatherMakerTemporalReprojectionShaderInclude.cginc"

				fixed4 blendFogTemporal(fixed4 prev, fixed4 cur, fixed4 diff, float4 uv, wm_full_screen_fragment i)
				{

#if defined(WEATHER_MAKER_TEMPORAL_REPROJECTION_NEIGHBORHOOD_CLAMPING) && !defined(SHADER_API_GLES3)

					// sample 8 of the nearby temporal pixels with the latest correct results and clamp the pixel color
					float2 uv1 = float2(i.uv.x + temporalReprojectionSubFrameBlurOffsets.x, i.uv.y - temporalReprojectionSubFrameBlurOffsets.w);
					float2 uv2 = float2(i.uv.x - temporalReprojectionSubFrameBlurOffsets.y, i.uv.y - temporalReprojectionSubFrameBlurOffsets.z);
					float2 uv3 = float2(i.uv.x + temporalReprojectionSubFrameBlurOffsets.y, i.uv.y + temporalReprojectionSubFrameBlurOffsets.z);
					float2 uv4 = float2(i.uv.x - temporalReprojectionSubFrameBlurOffsets.x, i.uv.y + temporalReprojectionSubFrameBlurOffsets.w);
					float2 uv5 = float2(i.uv.x + temporalReprojectionSubFrameBlurOffsets2.x, i.uv.y - temporalReprojectionSubFrameBlurOffsets2.w);
					float2 uv6 = float2(i.uv.x - temporalReprojectionSubFrameBlurOffsets2.y, i.uv.y - temporalReprojectionSubFrameBlurOffsets2.z);
					float2 uv7 = float2(i.uv.x + temporalReprojectionSubFrameBlurOffsets2.y, i.uv.y + temporalReprojectionSubFrameBlurOffsets2.z);
					float2 uv8 = float2(i.uv.x - temporalReprojectionSubFrameBlurOffsets2.x, i.uv.y + temporalReprojectionSubFrameBlurOffsets2.w);
					fixed4 col2 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_SubFrame, _linear_clamp_sampler, uv1);
					fixed4 col3 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_SubFrame, _linear_clamp_sampler, uv2);
					fixed4 col4 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_SubFrame, _linear_clamp_sampler, uv3);
					fixed4 col5 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_SubFrame, _linear_clamp_sampler, uv4);
					fixed4 col6 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_SubFrame, _linear_clamp_sampler, uv5);
					fixed4 col7 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_SubFrame, _linear_clamp_sampler, uv6);
					fixed4 col8 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_SubFrame, _linear_clamp_sampler, uv7);
					fixed4 col9 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_SubFrame, _linear_clamp_sampler, uv8);

					// we want to dither the neighboorhood clamping - for darker pixels we reduce this effect as it introduces artifacts
					// for brighter pixels, having a wider clamping range reduces flicker
					// varying the clamping range each frame also reduces flicker and introduces some variance which helps smooth things out
					// the parameters chosen here were mostly trial and error to reduce banding and jagged lines
					//fixed minA = min(cur.a, min(col2.a, min(col3.a, min(col4.a, min(col5.a, min(col6.a, min(col7.a, min(col8.a, col9.a))))))));
					fixed maxA = max(cur.a, max(col2.a, max(col3.a, max(col4.a, max(col5.a, max(col6.a, max(col7.a, max(col8.a, col9.a))))))));

					if (diff.w < 0.01)
					{
						// clamp the rgb to a smaller range of pixels to reduce bleeding pixels
						fixed3 minRgb = min(cur.rgb, min(col2.rgb, min(col3.rgb, min(col4.rgb, col5.rgb))));
						fixed3 maxRgb = max(cur.rgb, max(col2.rgb, max(col3.rgb, max(col4.rgb, col5.rgb))));
						prev.rgb = clamp(prev.rgb, minRgb, maxRgb);
					}

					// if alpha is changing enough, recompute full alpha (expensive)
					UNITY_BRANCH
					if (abs(prev.a - maxA) > 0.1)
					{

#if defined(WEATHER_MAKER_TEMPORAL_REPROJECTION_SHOW_OVERDRAW)

						return WEATHER_MAKER_TEMPORAL_REPROJECTION_SHOW_OVERDRAW;

#endif

						prev.a = ComputeFullScreenFogAlphaTemporalReprojection(i);
					}

#endif

					return prev;
				}

				fixed4 offScreenFogTemporal(fixed4 prev, fixed4 cur, float4 uv, wm_full_screen_fragment i)
				{

#if defined(WEATHER_MAKER_TEMPORAL_REPROJECTION_SHOW_OVERDRAW)

					return WEATHER_MAKER_TEMPORAL_REPROJECTION_SHOW_OVERDRAW;

#else

					cur.a = ComputeFullScreenFogAlphaTemporalReprojection(i);
					return cur;

#endif

				}

				ENDCG
			}

			// depth write pass (linear 0 - 1)
			Pass
			{
				CGPROGRAM

				#pragma vertex full_screen_vertex_shader
				#pragma fragment frag
				#pragma multi_compile_instancing

				#include "WeatherMakerCoreShaderInclude.cginc"

				float4 frag(wm_full_screen_fragment i) : SV_Target
				{ 
					WM_INSTANCE_FRAG(i);

					return WM_SAMPLE_DEPTH_DOWNSAMPLED_01(i.uv.xy);
				}

				ENDCG
			}

			// fog ray render pass
			Pass
			{
				Blend One Zero

				CGPROGRAM

				#define WEATHER_MAKER_IS_FULL_SCREEN_EFFECT
				#include "WeatherMakerFogShaderInclude.cginc"

				#pragma vertex full_screen_vertex_shader
				#pragma fragment frag
				#pragma multi_compile_instancing

				fixed4 frag(wm_full_screen_fragment i) : SV_Target
				{ 
					WM_INSTANCE_FRAG(i);

					fixed3 shaftColor = fixed3Zero;

					// take advantage of the fact that dir lights are sorted by perspective/ortho and then by intensity
					UNITY_LOOP
					for (uint lightIndex = 0;
						lightIndex < uint(_WeatherMakerDirLightCount) &&
						_WeatherMakerDirLightVar1[lightIndex].y == 0.0 &&
						_WeatherMakerDirLightColor[lightIndex].a > 0.001 &&
						_WeatherMakerDirLightVar1[lightIndex].z > 0.001; lightIndex++)
					{
						shaftColor.rgb += ComputeDirLightShaftColor(i.uv.xy, 0.01, _WeatherMakerDirLightViewportPosition[lightIndex],
							_WeatherMakerFogColor * _WeatherMakerDirLightColor[lightIndex] * _WeatherMakerDirLightVar1[lightIndex].z,
							fixed4One);

					}
					return fixed4(shaftColor, 0.0);
				}

				ENDCG
			}

			// fog ray blit pass
			Pass
			{
				Blend One One

				CGPROGRAM

				#define WEATHER_MAKER_IS_FULL_SCREEN_EFFECT
				#include "WeatherMakerFogShaderInclude.cginc"

				#pragma vertex full_screen_vertex_shader
				#pragma fragment frag
				#pragma multi_compile_instancing

				fixed4 frag(wm_full_screen_fragment i) : SV_Target
				{ 
					WM_INSTANCE_FRAG(i);

					return WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex4, i.uv);
				}

				ENDCG
			}
		}
	}
	Fallback "VertexLit"
}
