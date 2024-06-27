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

// http://bitsquid.blogspot.com/2016/07/volumetric-clouds.html
// https://github.com/greje656/clouds
// http://patapom.com/topics/Revision2013/Revision%202013%20-%20Real-time%20Volumetric%20Rendering%20Course%20Notes.pdf

Shader "WeatherMaker/WeatherMakerFullScreenCloudsShader"
{
	Properties
	{
		_PointSpotLightMultiplier("Point/Spot Light Multiplier", Range(0, 10)) = 1
		_DirectionalLightMultiplier("Directional Light Multiplier", Range(0, 10)) = 1
		_AmbientLightMultiplier("Ambient light multiplier", Range(0, 4)) = 1
	}
	SubShader
	{
		Cull Off Lighting Off ZWrite Off ZTest Always Fog { Mode Off }
		Blend [_SrcBlendMode][_DstBlendMode]

		CGINCLUDE

		#pragma target 3.5
		#pragma exclude_renderers gles
		#pragma exclude_renderers d3d9
        #pragma multi_compile_local __ WEATHER_MAKER_DOWNSAMPLE_2 WEATHER_MAKER_DOWNSAMPLE_4 WEATHER_MAKER_DOWNSAMPLE_8

		#define WEATHER_MAKER_DEPTH_SHADOWS_OFF
		#define WEATHER_MAKER_LIGHT_NO_DIR_LIGHT
		#define WEATHER_MAKER_LIGHT_NO_NORMALS
		#define WEATHER_MAKER_LIGHT_NO_SPECULAR
		#define WEATHER_MAKER_IS_FULL_SCREEN_EFFECT
		#define WEATHER_MAKER_ENABLE_TEXTURE_DEFINES

		#include "WeatherMakerCloudShaderUniformsInclude.cginc"

		void GetDepthAndRay(float4 uv, inout float3 rayDir, float3 forwardLine, out float depth, out float depth01)
		{
			rayDir = GetFullScreenRayDir(rayDir);

			// uncomment to mirror clouds down
			// rayDir.y = abs(rayDir.y);

            depth01 = (_WeatherMakerVREnabled ? 1.0 : WM_SAMPLE_DEPTH_DOWNSAMPLED_TEMPORAL_REPROJECTION_01_KEYWORD(uv.xy));
			if (depth01 >= 1.0)
			{
				// if depth is max value, make an "infinite" depth
				depth = 1000000000;
			}
			else if (WM_CAMERA_RENDER_MODE_CUBEMAP)
			{
				depth = length(rayDir * depth01 * _ProjectionParams.z);
			}
			else
			{
				depth = length(depth01 * forwardLine);
			}
		}

		ENDCG

		// color pass
		Pass
		{
			CGPROGRAM

			#pragma vertex full_screen_vertex_shader
			#pragma fragment temporal_reprojection_fragment_custom
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			#pragma multi_compile_instancing
			#pragma multi_compile WEATHER_MAKER_SHADOWS_ONE_CASCADE WEATHER_MAKER_SHADOWS_SPLIT_SPHERES

			#define WEATHER_MAKER_TEMPORAL_REPROJECTION_FRAGMENT_TYPE wm_full_screen_fragment
			#define WEATHER_MAKER_TEMPORAL_REPROJECTION_FRAGMENT_FUNC full_screen_clouds_frag_impl
			#define WEATHER_MAKER_TEMPORAL_REPROJECTION_BLEND_FUNC blendCloudTemporal

			//#define WEATHER_MAKER_TEMPORAL_REPROJECTION_OFF_SCREEN_FUNC offScreenCloudTemporal

			// leave commented out unless testing performance, red areas are full shader runs, try to minimize these
			//#define WEATHER_MAKER_TEMPORAL_REPROJECTION_SHOW_OVERDRAW fixed4(1,0,0,1)

			fixed4 blendCloudTemporal(fixed4 prev, fixed4 cur, fixed4 diff, float4 uv, wm_full_screen_fragment i);
			fixed4 offScreenCloudTemporal(fixed4 prev, fixed4 cur, float4 uv, wm_full_screen_fragment i);
			fixed4 full_screen_clouds_frag_impl(wm_full_screen_fragment i) : SV_Target;

			#include "WeatherMakerTemporalReprojectionShaderInclude.cginc"
			#include "WeatherMakerCloudVolumetricRaymarchShaderInclude.cginc"
			//#include "WeatherMakerCloudVolumetricShaderOldInclude.cginc"

			uniform UNITY_DECLARE_SCREENSPACE_TEXTURE(_WeatherMakerBackgroundSkyTexture);

			fixed4 blendCloudTemporal(fixed4 prev, fixed4 cur, fixed4 diff, float4 uv, wm_full_screen_fragment i)
			{
				// if blocked by depth, we have to resend the full pixel, otherwise artifacts everywhere
                UNITY_BRANCH
				if (prev.a < WM_MIN_PIXEL_VALUE || cur.a < WM_MIN_PIXEL_VALUE)
				{
					prev = CLOUD_COLOR_FORCE_REDRAW;
				}
                else
                {
                    // make sure no naughtly 0 alpha pixels are neighbors, if so we have to stick with just the pixel value
                    // else if not we can use hardware linear sampling

                    float2 uv1 = float2(uv.z + temporalReprojectionPrevFrameBlurOffsets.x, uv.w + temporalReprojectionPrevFrameBlurOffsets.z);
                    float2 uv2 = float2(uv.z, uv.w + temporalReprojectionPrevFrameBlurOffsets.z);
                    float2 uv3 = float2(uv.z + temporalReprojectionPrevFrameBlurOffsets.y, uv.w + temporalReprojectionPrevFrameBlurOffsets.z);
                    float2 uv4 = float2(uv.z + temporalReprojectionPrevFrameBlurOffsets.x, uv.w);
                    float2 uv5 = float2(uv.z + temporalReprojectionPrevFrameBlurOffsets.y, uv.w);
                    float2 uv6 = float2(uv.z + temporalReprojectionPrevFrameBlurOffsets.x, uv.w + temporalReprojectionPrevFrameBlurOffsets.w);
                    float2 uv7 = float2(uv.z, uv.w + temporalReprojectionPrevFrameBlurOffsets.w);
                    float2 uv8 = float2(uv.z + temporalReprojectionPrevFrameBlurOffsets.y, uv.w + temporalReprojectionPrevFrameBlurOffsets.w);

                    fixed4 a1 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _point_clamp_sampler, uv1);
                    fixed4 a2 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _point_clamp_sampler, uv2);
                    fixed4 a3 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _point_clamp_sampler, uv3);
                    fixed4 a4 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _point_clamp_sampler, uv4);
                    fixed4 a5 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _point_clamp_sampler, uv5);
                    fixed4 a6 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _point_clamp_sampler, uv6);
                    fixed4 a7 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _point_clamp_sampler, uv7);
                    fixed4 a8 = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _point_clamp_sampler, uv8);
					fixed allA = a1.a * a2.a * a3.a * a4.a * a5.a * a6.a * a7.a * a8.a;

                    // can linear blend because no neighbor blocked by depth buffer
                    prev = lerp(prev, WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_TemporalReprojection_PrevFrame, _linear_clamp_sampler, uv.zw), ceil(allA));

					fixed3 minV = min(a1.rgb, min(a2.rgb, min(a3.rgb, min(a4.rgb, min(a5.rgb, min(a6.rgb, min(a7.rgb, a8.rgb)))))));
					fixed3 maxV = max(a1.rgb, max(a2.rgb, max(a3.rgb, max(a4.rgb, max(a5.rgb, max(a6.rgb, max(a7.rgb, a8.rgb)))))));
					fixed prevSum = prev.r + prev.g + prev.b;

					// move rapidly to the current color if out of bounds
					prev = lerp(prev, cur, 0.5 * (prevSum < minV.r + minV.g + minV.b || prevSum > maxV.r + maxV.g + maxV.b));
                }

                return prev;
			}

			fixed4 offScreenCloudTemporal(fixed4 prev, fixed4 cur, float4 uv, wm_full_screen_fragment i)
			{
				return cur;
			}

			float PrecomputeCloudVolumetricHenyeyGreensteinVolumetric(DirLightPrecomputation dirLight)
			{

#define VOLUMETRIC_MAX_HENYEY_GREENSTEIN 5.0

				// https://www.diva-portal.org/smash/get/diva2:1223894/FULLTEXT01.pdf
				// f(x) = (1 - g)^2 / (4PI * (1 + g^2 - 2g*cos(x))^[3/2])
				// _CloudHenyeyGreensteinPhase.x = forward, _CloudHenyeyGreensteinPhase.y = back
				static const float g = _CloudHenyeyGreensteinPhaseVolumetric.x;
				static const float gSquared = g * g;
				static const float oneMinusGSquared = (1.0 - gSquared);
				static const float onePlusGSquared = 1.0 + gSquared;
				static const float twoG = 2.0 * g;
				float falloff = pow(PI * (onePlusGSquared - (twoG * dirLight.eyeDot)), 1.5);
				float forward = oneMinusGSquared / falloff;

				static const float g2 = _CloudHenyeyGreensteinPhaseVolumetric.y;
				static const float gSquared2 = g2 * g2;
				static const float oneMinusGSquared2 = (1.0 - gSquared2);
				static const float onePlusGSquared2 = 1.0 + gSquared2;
				static const float twoG2 = 2.0 * g2;
				float falloff2 = pow(PI * (onePlusGSquared2 - (twoG2 * dirLight.eyeDot)), 1.5);
				float back = oneMinusGSquared2 / falloff2;

				// hg back lighting is more dim than hg forward light as light intensity goes below 1
				return min(VOLUMETRIC_MAX_HENYEY_GREENSTEIN, (((forward * _CloudHenyeyGreensteinPhaseVolumetric.z) + (back * dirLight.intensity * _CloudHenyeyGreensteinPhaseVolumetric.w))));
			}

			DirLightPrecomputation PrecomputeDirLight(in CloudState state, float3 rayDir, uint lightIndex)
			{
				UNITY_BRANCH
				if (_WeatherMakerDirLightColor[lightIndex].a > 0.0)
				{
					fixed3 lightColor = _WeatherMakerDirLightColor[lightIndex].rgb;
					float3 lightDir = _WeatherMakerDirLightPosition[lightIndex].xyz;
					// make sure we don't walk down, this causes artifacts at horizon
					lightDir.y = max(0.1, lightDir.y);

					float intensity = min(1.0, _WeatherMakerDirLightColor[lightIndex].a);
					float intensitySquared = intensity * intensity;
					float eyeDot = dot(rayDir, lightDir);
					DirLightPrecomputation item;
					item.eyeDot = eyeDot;
					float energy = max(_WeatherMakerDirLightColor[lightIndex].a, max(0.33, (eyeDot + 1.0) * 0.5) * _WeatherMakerDirLightVar1[lightIndex].w) * _CloudDirLightMultiplierVolumetric;
					item.intensity = intensity;
					item.intensitySquared = intensitySquared;
					item.hg = PrecomputeCloudVolumetricHenyeyGreensteinVolumetric(item) * energy;
					item.lightDir = lightDir;
					float powderEyeDot = ((eyeDot * -0.5) + 0.5);
					item.powderMultiplier = lerp(1.0, _CloudPowderMultiplierVolumetric, min(1.0, 4.0 * intensity * lightDir.y * lightDir.y));
					item.powderAngle = min(1.0, _CloudPowderMultiplierVolumetric * powderEyeDot * intensity) * min(1.0, _CloudPowderMultiplierVolumetric);

					// horizontal lights cause more shadow because more air and general cloud to go through
					item.shadowPower = (1.0 - lightDir.y);
					item.shadowPower *= item.shadowPower;
					item.shadowPower += 0.5;
					item.shadowPower *= _CloudLightAbsorptionVolumetric;

					// reduce cone radius as light goes horizontal, light is more diffused
					item.lightConeRadius = state.lightStepSize * _CloudLightRadiusMultiplierVolumetric * clamp(lightDir.y * 2.5, 0.1, 1.0);
					item.indirectLight = item.intensity * lightColor * _CloudDirLightIndirectMultiplierVolumetric;
					return item;
				}
				else
				{
					return emptyDirLight;
				}
			}

			CloudState PrecomputeCloudState(float3 rayDir, float2 uv)
			{
				CloudState state;
				state.dithering = tex2Dlod(_WeatherMakerBlueNoiseTexture, float4(uv + _WeatherMakerTemporalUV_FragmentShader, 0.0, 0.0));
				state.lightStepSize = volumetricDirLightStepSize * (1.0 + (state.dithering * 0.015));
				state.lightColorDithering = (state.dithering * 0.005);
				state.fade = 0.0;

				UNITY_UNROLL
				for (uint lightIndex = 0; lightIndex < uint(MAX_LIGHT_COUNT); lightIndex++)
				{
					state.dirLight[lightIndex] = PrecomputeDirLight(state, rayDir, lightIndex);
				}

				return state;
			}

			fixed4 full_screen_clouds_frag_impl(wm_full_screen_fragment i) : SV_Target
			{
				float hitCloud = 0.0;
				float depth, depth01;
				float3 cloudRay = i.forwardLine;
				GetDepthAndRay(i.uv, cloudRay, i.forwardLine, depth, depth01);
				CloudState state = PrecomputeCloudState(cloudRay, i.uv.xy);
				fixed4 finalColor = fixed4Zero;
                //return fixed4(depth01, depth01, depth01, 1.0);

				// assumption- flat clouds are above volumetric clouds
				CloudColorResult flatColor = ComputeFlatCloudColorAll(cloudRay, depth, i.uv, _CloudNoiseLod, state);
				hitCloud = flatColor.hitCloud;
				finalColor = flatColor.color;

				// volumetric layer
				UNITY_BRANCH
				if (_CloudCoverVolumetric > 0.0)
				{
					// https://gamedev.stackexchange.com/questions/138813/whats-the-difference-between-alpha-and-premulalpha
					CloudColorResult volumetricColor = ComputeCloudColorVolumetric(cloudRay, i.uv, depth, depth01, state);
					hitCloud = min(1.0, hitCloud + volumetricColor.hitCloud);
					fixed horizonFade = volumetricColor.fade;
					finalColor = volumetricColor.color + (finalColor * (1.0 - volumetricColor.color.a));

					// old stuff
					//return ComputeCloudColorVolumetric(cloudRay, i.uv, depth, 0);

#if VOLUMETRIC_CLOUD_RENDER_MODE == 1 // render mode 1 = normal render mode

					UNITY_BRANCH
					if (horizonFade < 1.0 && finalColor.a > 0.0)
					{
						// return fixed4(horizonFade, horizonFade, horizonFade, 1.0);

						UNITY_BRANCH
						if (volumetricIsAboveClouds)
						{
							// above clouds, just remove alpha
							finalColor *= horizonFade;
						}
						else
						{
							// retrieve sky color without sun, moon or stars
							fixed4 backgroundSkyColor = WM_SAMPLE_FULL_SCREEN_TEXTURE(_WeatherMakerBackgroundSkyTexture, i.uv.xy + _WeatherMakerTemporalUV_FragmentShader);

							// at night, make the sky disappear so stars don't show through the clouds
							backgroundSkyColor = lerp(backgroundSkyColor, fixed4(_WeatherMakerAmbientLightColorEquator, 1.0), _WeatherMakerNightMultiplier);

							// reduce cloud opacity by horizon fade
							finalColor.rgb *= horizonFade;

							// TODO: This is very close, there is a jerk on the sky when showing/hiding clouds, figure out why...
							// add in background sky to simulate atmospheric scattering to desired intensity
							finalColor.rgb += (backgroundSkyColor.rgb * (1.0 - horizonFade) * finalColor.a);
						}
					}

#endif

				}

				// northern lights layer
				UNITY_BRANCH
				if (finalColor.a < 0.999 && weatherMakerNightMultiplierSquared > 0.0 && _WeatherMakerAuroraIntensity > 0.0)
				{
					fixed4 auroraColor = ComputeAurora(WEATHER_MAKER_CAMERA_POS, cloudRay, i.uv, depth) * weatherMakerNightMultiplierSquared;
					// pre-multiply blend
					finalColor = finalColor + (auroraColor * (1.0 - finalColor.a));
				}

				// DEBUG:
				// alpha of 0 should only be behind depth buffer, if it is showing up in a cloud area, it is a bug and will show in red
				//finalColor = lerp(fixed4(1.0, 0.0, 0.0, 1.0), finalColor, ceil(finalColor.a));

				// ensure a minimum pixel value so we don't re-render these clouds un-necessarily
				finalColor.a = lerp(finalColor.a, max(finalColor.a, WM_MIN_PIXEL_VALUE), hitCloud);
				return finalColor;
			}

			ENDCG
		}

		// depth write pass (linear 0 - 1)
		Pass
		{
			CGPROGRAM

			#pragma vertex full_screen_vertex_shader
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			#pragma multi_compile_instancing

			#include "WeatherMakerCloudVolumetricUniformsShaderInclude.cginc"

			float4 frag(wm_full_screen_fragment i) : SV_Target
			{ 
				WM_INSTANCE_FRAG(i);

				UNITY_BRANCH
				if (_CloudCoverVolumetric > 0.0)
				{
					UNITY_BRANCH
					if (unity_OrthoParams.w == 0.0)
					{
						// get the 0-1 depth of the cloud layer start
						float depth, depth01;
						float3 rayDir = i.forwardLine;
						// don't use ray offset, we want the exact depth buffer value
						GetDepthAndRay(i.uv, rayDir, i.forwardLine, depth, depth01);
						float3 startPos, startPos2;
						float3 endPos, endPos2;
						float rayLength, rayLength2;
						float distanceToSphere, distanceToSphere2;
						uint iterations = SetupCloudRaymarch(WEATHER_MAKER_CAMERA_POS, rayDir, depth, depth,
							startPos, endPos, rayLength, distanceToSphere, startPos2, endPos2, rayLength2, distanceToSphere2);

						// return 1.0 / ((_ZBufferParams.x * z) + _ZBufferParams.y);
						// TODO: The left of this lerp is incorrect far above the clouds, figure out why...
						//float cloudLayerDepth01 = GetDepth01FromWorldSpaceRay(rayDir, distanceToSphere);
						float depthPos = length(depth01 * i.forwardLine);
						float cloudLayerDepth01 = saturate(lerp(0.0, depth01, distanceToSphere / depthPos));
						return min(depth01, cloudLayerDepth01);
					}
					else
					{
						// orthographic not supported
						return 1.0;
					}
				}
				else
				{
					return 1.0;
				}
			}

			ENDCG
		}
		
		// cloud ray pass
		Pass
		{
			Blend One Zero

			CGPROGRAM

			#pragma vertex full_screen_vertex_shader
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			#pragma multi_compile_instancing

			#include "WeatherMakerCloudVolumetricRaymarchShaderInclude.cginc"

			fixed4 frag(wm_full_screen_fragment i) : SV_Target
			{ 
				WM_INSTANCE_FRAG(i);

#if VOLUMETRIC_CLOUD_RENDER_MODE == 1
				
				fixed3 shaftColor = fixed3Zero;
				fixed4 pixelColor = WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex2, i.uv.xy);
				UNITY_BRANCH
				if (pixelColor.a > 0.0)
				{
					// take advantage of the fact that dir lights are sorted by perspective/ortho and then by intensity
					UNITY_LOOP
					for (uint lightIndex = 0;
						lightIndex < uint(_WeatherMakerDirLightCount) &&
						_WeatherMakerDirLightVar1[lightIndex].y == 0.0 &&
						_WeatherMakerDirLightColor[lightIndex].a > 0.001 &&
						_WeatherMakerDirLightVar1[lightIndex].z > 0.001; lightIndex++)
					{
						shaftColor += ComputeDirLightShaftColor(i.uv.xy, 0.01, _WeatherMakerDirLightViewportPosition[lightIndex], _WeatherMakerDirLightColor[lightIndex] * _WeatherMakerDirLightVar1[lightIndex].z, pixelColor);
					}
				}
				return fixed4(shaftColor.rgb, 0.0);

#else

				return fixed4Zero;

#endif

			}

			ENDCG
		}

		// cloud ray blit pass
		Pass
		{
			Blend One One

			CGPROGRAM

			#pragma vertex full_screen_vertex_shader
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			#pragma multi_compile_instancing

			#include "WeatherMakerCloudVolumetricRaymarchShaderInclude.cginc"

			fixed4 frag(wm_full_screen_fragment i) : SV_Target
			{ 
				WM_INSTANCE_FRAG(i);

				// (0.4,-1.2) , (-1.2,-0.4) , (1.2,0.4) and (-0.4,1.2).
				static const float4 offsets = float4
				(
					_MainTex4_TexelSize.x * 0.4,
					_MainTex4_TexelSize.x * 1.2,
					_MainTex4_TexelSize.y * 0.4,
					_MainTex4_TexelSize.y * 1.2
				);

				fixed4 c;
				GaussianBlur17Tap(c, _MainTex4, i.uv.xy, offsets, 1.0);
				return c;
				//return WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex4, i.uv);
			}

			ENDCG
		}

		// atmosphere blit pass
		Pass
		{
			Blend One Zero

			CGPROGRAM

			#pragma vertex full_screen_vertex_shader
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			#pragma multi_compile_instancing
			#pragma multi_compile WEATHER_MAKER_SHADOWS_ONE_CASCADE WEATHER_MAKER_SHADOWS_SPLIT_SPHERES

			#include "WeatherMakerSkyBoxShaderInclude.cginc"

			fixed4 frag(wm_full_screen_fragment i) : SV_Target
			{
				WM_INSTANCE_FRAG(i);

				fixed4 pixelColor = WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex2, i.uv.xy);
				float depth01 = GetDepth01(i.uv.xy);

				UNITY_BRANCH
				if (depth01 < 1.0)
				{
					float len = length(i.forwardLine);
					depth01 = saturate(len * depth01 * atmosphere01);
					pixelColor = ComputeAtmosphericScatteringFog(i.uv.xy, WEATHER_MAKER_CAMERA_POS, GetFullScreenRayDir(i.forwardLine),
						len, pixelColor, depth01);
				}

				return pixelColor;
			}

			ENDCG
		}

		// atmosphere light shaft pass
		Pass
		{
			Blend One Zero

			CGPROGRAM

			#pragma vertex full_screen_vertex_shader
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			#pragma multi_compile_instancing
			#pragma multi_compile WEATHER_MAKER_SHADOWS_ONE_CASCADE WEATHER_MAKER_SHADOWS_SPLIT_SPHERES
			#pragma multi_compile __ UNITY_URP

			#include "WeatherMakerSkyBoxShaderInclude.cginc"

			static const float maxShadow = _WeatherMakerCloudAtmosphereShadow * _WeatherMakerDirLightColor[0].a;

			fixed4 frag(wm_full_screen_fragment i) : SV_Target
			{
				WM_INSTANCE_FRAG(i);
				float rayLength = length(i.forwardLine) * WM_SAMPLE_DEPTH_DOWNSAMPLED_01_KEYWORD(i.uv.xy);
				float shadow = ComputeAtmosphericLightShafts(i.vertex.xy, WEATHER_MAKER_CAMERA_POS_NO_ORIGIN_OFFSET, GetFullScreenRayDir(i.forwardLine), rayLength);
				shadow *= shadow;
				shadow = max(maxShadow, min(1.0, shadow * _WeatherMakerDirLightColor[0].a));
				shadow = max(shadow, min(weatherMakerGlobalShadow4, 0.1));
				shadow = saturate(shadow * (1.0 + (RandomFloat2D(i.uv.xy) * 0.05)));
				return shadow;
			}

			ENDCG
		}
	}
}
