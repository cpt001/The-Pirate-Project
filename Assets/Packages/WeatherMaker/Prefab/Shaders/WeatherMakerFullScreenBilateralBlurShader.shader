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



Shader "WeatherMaker/WeatherMakerFullScreenBilateralBlurShader"
{
	Properties
	{
		[HideInInspector]
		_MainTex("Texture", any) = "" {}
		[HideInInspector]
		_DepthTex("Depth Tex", any) = "" {}
		_BlurDitherLevel("Blur dither level", Range(0.0, 1.0)) = 0.001
		_UpsampleDepthThreshold("Upsample depth threshold", Range(0.01, 1000.0)) = 1.5
		_GaussBlurDeviation("Blur deviation", Range(0.1, 10.0)) = 1.5
		_GuassBlurDepthFactor("Blur depth factor", Range(0.0, 1000.0)) = 100.0
		_BlendOp("Blend Op", Int) = 0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		BlendOp[_BlendOp]

		CGINCLUDE

		#pragma target 3.5
		#pragma exclude_renderers gles
		#pragma exclude_renderers d3d9
		
        //--------------------------------------------------------------------------------------------
        // Bilateral blur and upsample config
        //--------------------------------------------------------------------------------------------        
        #define FULL_RES_BLUR_KERNEL_SIZE 7
        #define HALF_RES_BLUR_KERNEL_SIZE 6
        #define QUARTER_RES_BLUR_KERNEL_SIZE 5
		#define EIGHTH_RES_BLUR_KERNEL_SIZE 4
        //--------------------------------------------------------------------------------------------

		#define PI 3.1415927f
		#define WEATHER_MAKER_IS_FULL_SCREEN_EFFECT
		#define WEATHER_MAKER_MAIN_TEX_SAMPLERS
		#define WEATHER_MAKER_ENABLE_TEXTURE_DEFINES

		#include "WeatherMakerCoreShaderInclude.cginc"	
	
		uniform float _BlurDitherLevel;
		uniform float _UpsampleDepthThreshold;
		uniform float _GaussBlurDeviation;
		uniform float _GuassBlurDepthFactor;

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			WM_BASE_VERTEX_INPUT
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
			WM_BASE_VERTEX_TO_FRAG
		};

		struct v2fUpsample
		{
			float2 uv : TEXCOORD0;
			float2 uv00 : TEXCOORD1;
			float2 uv01 : TEXCOORD2;
			float2 uv10 : TEXCOORD3;
			float2 uv11 : TEXCOORD4;
			float4 vertex : SV_POSITION;
			WM_BASE_VERTEX_TO_FRAG
		};

		v2f vert(appdata v)
		{
			WM_INSTANCE_VERT(v, v2f, o);
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = AdjustFullScreenUV(v.uv);
			return o;
		}

		//-----------------------------------------------------------------------------------------
		// vertUpsample
		//-----------------------------------------------------------------------------------------
        v2fUpsample vertUpsample(appdata v, float2 texelSize)
        {
			WM_INSTANCE_VERT(v, v2fUpsample, o);
            o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = AdjustFullScreenUV(v.uv);
            o.uv00 = o.uv - 0.5 * texelSize;
            o.uv10 = o.uv00 + float2(texelSize.x, 0);
            o.uv01 = o.uv00 + float2(0, texelSize.y);
            o.uv11 = o.uv00 + texelSize.xy;
            return o;
        }

		//-----------------------------------------------------------------------------------------
		// BilateralUpsample
		//-----------------------------------------------------------------------------------------
		float4 BilateralUpsample(v2fUpsample input)
		{
			float4 highResDepth = WM_LINEAR_DEPTH_01(UNITY_SAMPLE_DEPTH(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, input.uv)));
			float4 lowResDepth;

			// downsample depth is already linear 01
			lowResDepth[0] = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(input.uv00);
			lowResDepth[1] = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(input.uv10);
            lowResDepth[2] = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(input.uv01);
            lowResDepth[3] = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(input.uv11);
			float4 depthDiff = abs(lowResDepth - highResDepth) * _ProjectionParams.z;
			float accumDiff = dot(depthDiff, float4(1, 1, 1, 1));

			UNITY_BRANCH
			if (accumDiff < _UpsampleDepthThreshold) // small error, not an edge -> use bilinear filter
			{
				// needs to be linear sampler, make sure 0 alpha pixels stay 0 and don't blead alpha into this pixel
				fixed4 finalSample = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_MainTex, _point_clamp_sampler, input.uv);
                if (finalSample.a > 0.0)
                {
                    fixed4 linearSample = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_MainTex, _linear_clamp_sampler, input.uv);
                    if (linearSample.a >= finalSample.a)
                    {
                        finalSample = linearSample;
                    }
                }
                return finalSample;
			}
			else
			{
				// find nearest sample
				float minDepthDiff = depthDiff[0];
				float2 nearestUv = input.uv00;

				if (depthDiff[1] < minDepthDiff)
				{
					nearestUv = input.uv10;
					minDepthDiff = depthDiff[1];
				}

				if (depthDiff[2] < minDepthDiff)
				{
					nearestUv = input.uv01;
					minDepthDiff = depthDiff[2];
				}

				if (depthDiff[3] < minDepthDiff)
				{
					nearestUv = input.uv11;
					minDepthDiff = depthDiff[3];
				}

				// needs to be point sampler
				return WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_MainTex, _point_clamp_sampler, nearestUv);
			}
		}

		//-----------------------------------------------------------------------------------------
		// GaussianWeight
		//-----------------------------------------------------------------------------------------
		float GaussianWeight(float offset, float deviation)
		{
			float weight = 1.0f / sqrt(2.0f * PI * deviation * deviation);
			weight *= exp(-(offset * offset) / (2.0f * deviation * deviation));
			return weight;
		}

		//-----------------------------------------------------------------------------------------
		// BilateralBlur
		//-----------------------------------------------------------------------------------------
		float4 BilateralBlur(v2f input, float2 direction, const int kernelRadius)
		{
			//const float deviation = kernelRadius / 2.5;
			const float deviation = kernelRadius / _GaussBlurDeviation; // make it really strong

			float2 uv = input.uv;
            float4 centerColorPoint = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_MainTex, _point_clamp_sampler, uv);

            UNITY_BRANCH
            if (centerColorPoint.a == 0.0)
            {
                return fixed4Zero;
            }
            else
            {
                float4 centerColor = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_MainTex, _linear_clamp_sampler, uv);
                centerColor = (centerColor.a >= centerColorPoint.a ? centerColor : centerColorPoint);

                float3 color = centerColor.rgb;
                float centerDepth = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(uv);
                float weightSum = 0;

                // gaussian weight is computed from constants only -> will be computed in compile time
                float weight = GaussianWeight(0, deviation);
                color *= weight;
                weightSum += weight;

                UNITY_UNROLL
                for (int i = -kernelRadius; i < 0; i += 1)
                {
                    float2 offset = (direction * i);
                    float4 pointColor = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_MainTex, _point_clamp_sampler, input.uv + offset);

                    UNITY_BRANCH
                    if (pointColor.a > 0.0)
                    {
                        float4 sampleColor = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_MainTex, _linear_clamp_sampler, input.uv + offset);
                        sampleColor = (sampleColor.a >= pointColor.a ? sampleColor : pointColor);
                        float sampleDepth = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(input.uv + offset); // UNITY_SAMPLE_DEPTH(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, input.uv + offset));
                        float depthDiff = abs(centerDepth - sampleDepth);
                        float dFactor = depthDiff * _GuassBlurDepthFactor;
                        float w = exp2(-(dFactor * dFactor));

                        // gaussian weight is computed from constants only -> will be computed in compile time
                        weight = GaussianWeight(i, deviation) * w;
                        color += (weight * sampleColor.rgb);
                        weightSum += weight;
                    }
                }

                UNITY_UNROLL
                for (i = 1; i <= kernelRadius; i += 1)
                {
                    float2 offset = (direction * i);
                    float4 pointColor = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_MainTex, _point_clamp_sampler, input.uv + offset);

                    UNITY_BRANCH
                    if (pointColor.a > 0.0)
                    {
                        float4 sampleColor = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_MainTex, _linear_clamp_sampler, input.uv + offset);
                        sampleColor = (sampleColor.a >= pointColor.a ? sampleColor : pointColor);
                        float sampleDepth = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(input.uv + offset); //UNITY_SAMPLE_DEPTH(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, input.uv + offset));
                        float depthDiff = abs(centerDepth - sampleDepth);
                        float dFactor = depthDiff * _GuassBlurDepthFactor;
                        float w = exp2(-(dFactor * dFactor));

                        // gaussian weight is computed from constants only -> will be computed in compile time
                        weight = GaussianWeight(i, deviation) * w;
                        color += (weight * sampleColor.rgb);
                        weightSum += weight;
                    }
                }

                color /= weightSum;
                ApplyDither(color.rgb, input.uv.xy, _BlurDitherLevel);
                return float4(color, centerColor.a);
            }
		}

		ENDCG

		// pass 0 - horizontal blur full
		Pass
		{
			Blend One Zero

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment horizontalFrag
            #pragma target 3.5
			#pragma multi_compile_instancing
			
			fixed4 horizontalFrag(v2f input) : SV_Target
			{
				WM_INSTANCE_FRAG(input);
                return BilateralBlur(input, float2(_CameraDepthTexture_TexelSize.x, 0.0), FULL_RES_BLUR_KERNEL_SIZE);
			}

			ENDCG
		}

		// pass 1 - vertical blur full
		Pass
		{
			Blend One Zero

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment verticalFrag
            #pragma target 3.5
			#pragma multi_compile_instancing
			
			fixed4 verticalFrag(v2f input) : SV_Target
			{
				WM_INSTANCE_FRAG(input);
                return BilateralBlur(input, float2(0.0, _CameraDepthTexture_TexelSize.y), FULL_RES_BLUR_KERNEL_SIZE);
			}

			ENDCG
		}

		// pass 2 - horizontal blur half
		Pass
		{
			Blend One Zero

			CGPROGRAM

            #pragma vertex vert
            #pragma fragment horizontalFrag
            #pragma target 3.5
			#pragma multi_compile_instancing

			fixed4 horizontalFrag(v2f input) : SV_Target
			{
				WM_INSTANCE_FRAG(input);
	            return BilateralBlur(input, float2(_CameraDepthTextureHalf_TexelSize.x, 0.0), HALF_RES_BLUR_KERNEL_SIZE);
			}

			ENDCG
		}

		// pass 3 - vertical blur half
		Pass
		{
			Blend One Zero

			CGPROGRAM

            #pragma vertex vert
            #pragma fragment verticalFrag
            #pragma target 3.5
			#pragma multi_compile_instancing

			fixed4 verticalFrag(v2f input) : SV_Target
			{
				WM_INSTANCE_FRAG(input);
	            return BilateralBlur(input, float2(0.0, _CameraDepthTextureHalf_TexelSize.y), HALF_RES_BLUR_KERNEL_SIZE);
			}

			ENDCG
		}

		// pass 4 - horizontal blur (quarter res)
		Pass
		{
			Blend One Zero

			CGPROGRAM

            #pragma vertex vert
            #pragma fragment horizontalFrag
            #pragma target 3.5
			#pragma multi_compile_instancing

			fixed4 horizontalFrag(v2f input) : SV_Target
			{
				WM_INSTANCE_FRAG(input);
                return BilateralBlur(input, float2(_CameraDepthTextureQuarter_TexelSize.x, 0.0), QUARTER_RES_BLUR_KERNEL_SIZE);
			}

			ENDCG
		}

		// pass 5 - vertical blur (quarter res)
		Pass
		{
			Blend One Zero

			CGPROGRAM

            #pragma vertex vert
            #pragma fragment verticalFrag
            #pragma target 3.5
			#pragma multi_compile_instancing

			fixed4 verticalFrag(v2f input) : SV_Target
			{
				WM_INSTANCE_FRAG(input);
                return BilateralBlur(input, float2(0.0, _CameraDepthTextureQuarter_TexelSize.y), QUARTER_RES_BLUR_KERNEL_SIZE);
			}

			ENDCG
		}

		// pass 6 - horizontal blur (eighth res)
		Pass
		{
			Blend One Zero

			CGPROGRAM

            #pragma vertex vert
            #pragma fragment horizontalFrag
            #pragma target 3.5
			#pragma multi_compile_instancing

			fixed4 horizontalFrag(v2f input) : SV_Target
			{
				WM_INSTANCE_FRAG(input);
                return BilateralBlur(input, float2(_CameraDepthTextureEighth_TexelSize.x, 0.0), EIGHTH_RES_BLUR_KERNEL_SIZE);
			}

			ENDCG
		}

		// pass 7 - vertical blur (eighth res)
		Pass
		{
			Blend One Zero

			CGPROGRAM

            #pragma vertex vert
            #pragma fragment verticalFrag
            #pragma target 3.5
			#pragma multi_compile_instancing

			fixed4 verticalFrag(v2f input) : SV_Target
			{
				WM_INSTANCE_FRAG(input);
                return BilateralBlur(input, float2(0.0, _CameraDepthTextureEighth_TexelSize.y), EIGHTH_RES_BLUR_KERNEL_SIZE);
			}

			ENDCG
		}

		// pass 8 - blit full to full
		Pass
		{
			Blend [_SrcBlendMode] [_DstBlendMode]

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag		
            #pragma target 3.5
			#pragma multi_compile_instancing

			float4 frag(v2f input) : SV_Target
			{
				WM_INSTANCE_FRAG(input);
				return WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_MainTex, _linear_clamp_sampler, input.uv);
			}

			ENDCG
		}

		// pass 9 - bilateral upsample half to full
		Pass
		{
			Blend [_SrcBlendMode] [_DstBlendMode]

			CGPROGRAM

			#pragma vertex vertUpsampleToFull
			#pragma fragment frag		
            #pragma target 3.5
			#pragma multi_compile_instancing

			v2fUpsample vertUpsampleToFull(appdata v)
			{
                return vertUpsample(v, _CameraDepthTextureHalf_TexelSize);
			}
			float4 frag(v2fUpsample input) : SV_Target
			{
				WM_INSTANCE_FRAG(input);
				return BilateralUpsample(input);
			}

			ENDCG
		}

		// pass 10 - bilateral upsample quarter to full
		Pass
		{
			Blend [_SrcBlendMode] [_DstBlendMode]

			CGPROGRAM

            #pragma vertex vertUpsampleToFull
            #pragma fragment frag		
            #pragma target 3.5
			#pragma multi_compile_instancing

			v2fUpsample vertUpsampleToFull(appdata v)
			{
                return vertUpsample(v, _CameraDepthTextureQuarter_TexelSize);
			}
			float4 frag(v2fUpsample input) : SV_Target
			{
				WM_INSTANCE_FRAG(input);
				return BilateralUpsample(input);
			}

			ENDCG
		}

		// pass 11 - bilateral upsample eighth to full
		Pass
		{
			Blend [_SrcBlendMode] [_DstBlendMode]

			CGPROGRAM

            #pragma vertex vertUpsampleToFull
            #pragma fragment frag		
            #pragma target 3.5
			#pragma multi_compile_instancing

			v2fUpsample vertUpsampleToFull(appdata v)
			{
                return vertUpsample(v, _CameraDepthTextureEighth_TexelSize);
			}
			float4 frag(v2fUpsample input) : SV_Target
			{
				WM_INSTANCE_FRAG(input);
				return BilateralUpsample(input);
			}

			ENDCG
		}

		// pass 12 - blit min
		Pass
		{
			Blend One Zero
			BlendOp Min

			CGPROGRAM

			#pragma vertex vertexBlit
			#pragma fragment frag		
            #pragma target 3.5
			#pragma multi_compile_instancing

			wm_full_screen_fragment_vertex_uv vertexBlit(wm_full_screen_vertex v)
			{
                return full_screen_vertex_shader_vertex_uv(v);
			}
			float4 frag(wm_full_screen_fragment_vertex_uv input) : SV_Target
			{
				WM_INSTANCE_FRAG(input);
				return WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_MainTex, _linear_clamp_sampler, input.uv);
			}

			ENDCG
		}
	}
}