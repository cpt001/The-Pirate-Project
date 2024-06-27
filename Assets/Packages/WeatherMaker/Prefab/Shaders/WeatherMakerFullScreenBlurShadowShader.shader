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

// http://rastergrid.com/blog/2010/09/efficient-gaussian-blur-with-linear-sampling/
// _MainTex must be bilinear

Shader "WeatherMaker/WeatherMakerFullScreenBlurShadowShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "red" {}
		_BlurDepthMin("Blur depth threshold", Range(0.0, 1.0)) = 0.01
		_BlurDitherLevel("Blur dither level", Range(0.0, 1.0)) = 0.0004
		_Blur7("Blur 7 tap, 0 for 17 tap", Int) = 0
		_BlendOp("Blend Op", Int) = 0
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest[_ZTest]
		BlendOp[_BlendOp]
		Blend[_SrcBlendMode][_DstBlendMode]

		CGINCLUDE

		#pragma target 3.5
		#pragma exclude_renderers gles
		#pragma exclude_renderers d3d9
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma multi_compile_instancing

		#define WEATHER_MAKER_IS_FULL_SCREEN_EFFECT
		#define WEATHER_MAKER_ENABLE_TEXTURE_DEFINES
		#define WEATHER_MAKER_MAIN_TEX_SAMPLERS

		#include "WeatherMakerCoreShaderInclude.cginc"

		uniform float _BlurDepthMin;
		uniform float _BlurDitherLevel;
		uniform int _Blur7;

		struct v2f
		{
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;
			float4 offsets : TEXCOORD1;

			WM_BASE_VERTEX_TO_FRAG
		};

		v2f vert (wm_appdata_base v)
		{
			WM_INSTANCE_VERT(v, v2f, o);
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = AdjustFullScreenUV(v.texcoord);

			if (_Blur7 == 1.0)
			{
				// take top left 3 and bottom right 3 plus center pixel average
				o.offsets = float4(_MainTex_TexelSize.x * 0.333333, _MainTex_TexelSize.y * 0.333333, 0.0, 0.0);
			}
			else
			{
				// (0.4,-1.2) , (-1.2,-0.4) , (1.2,0.4) and (-0.4,1.2).
				o.offsets = float4
				(
					_MainTex_TexelSize.x * 0.75,
					_MainTex_TexelSize.x * -0.75,
					_MainTex_TexelSize.y * 0.75,
					_MainTex_TexelSize.y * -0.75
				);
			}

			return o;
		}

		ENDCG

		// optimized blur
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			fixed4 blurTap(float2 uv)
			{
				return WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_MainTex, _linear_clamp_sampler, uv);
			}

			fixed4 frag(v2f i) : SV_Target
            {
                WM_INSTANCE_FRAG(i);

                float sourceDepth = GetDepth01(i.uv);
                float depth1 = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(i.uv);

                fixed4 col1 = blurTap(i.uv);
                fixed col1Alpha = col1.a;

                UNITY_BRANCH
                if (_Blur7)
                {
                    // 7 tap approximation with 2 texture lookups
                    float2 uv1 = i.uv;
                    float2 uv2 = float2(i.uv.x - i.offsets.x, i.uv.y - i.offsets.y);
                    float2 uv3 = float2(i.uv.x + i.offsets.x, i.uv.y + i.offsets.y);
                    float depth2 = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(uv2);
                    float depth3 = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(uv3);
                    //fixed4 col2 = WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, uv2);
                    //fixed4 col3 = WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, uv3);
                    fixed4 col2 = blurTap(uv2);
                    fixed4 col3 = blurTap(uv3);
                    float weight1 = abs(sourceDepth - depth1);
                    float weight2 = abs(sourceDepth - depth2);
                    float weight3 = abs(sourceDepth - depth3);
                    float match1 = (weight1 < _BlurDepthMin);
                    float match2 = (weight2 < _BlurDepthMin) && col2.a > 0.0;
                    float match3 = (weight3 < _BlurDepthMin) && col3.a > 0.0;
                    float count = match1 + match2 + match3;
                    col1.rgb = (match1 * col1.rgb);
                    col1.rgb += (match2 * col2.rgb);
                    col1.rgb += (match3 * col3.rgb);
                    col1.rgb /= count;
                    col1 *= (count != 0);
                }
                else
                {
                    // 17 tap approximation with 4 texture lookups
                    float2 uv1 = i.uv;
                    float2 uv2 = float2(i.uv.x + i.offsets.x, i.uv.y - i.offsets.w);
                    float2 uv3 = float2(i.uv.x - i.offsets.y, i.uv.y - i.offsets.z);
                    float2 uv4 = float2(i.uv.x + i.offsets.y, i.uv.y + i.offsets.z);
                    float2 uv5 = float2(i.uv.x - i.offsets.x, i.uv.y + i.offsets.w);
                    float depth2 = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(uv2);
                    float depth3 = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(uv3);
                    float depth4 = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(uv4);
                    float depth5 = WM_SAMPLE_DEPTH_DOWNSAMPLED_01(uv5);
                    //fixed4 col2 = WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, uv2);
                    //fixed4 col3 = WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, uv3);
                    //fixed4 col4 = WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, uv4);
                    //fixed4 col5 = WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, uv5);
                    fixed4 col2 = blurTap(uv2);
                    fixed4 col3 = blurTap(uv3);
                    fixed4 col4 = blurTap(uv4);
                    fixed4 col5 = blurTap(uv5);
                    float weight1 = abs(sourceDepth - depth1);
                    float weight2 = abs(sourceDepth - depth2);
                    float weight3 = abs(sourceDepth - depth3);
                    float weight4 = abs(sourceDepth - depth4);
                    float weight5 = abs(sourceDepth - depth5);
                    float match1 = (weight1 < _BlurDepthMin);
                    float match2 = (weight2 < _BlurDepthMin) && col2.a > 0.0;
                    float match3 = (weight3 < _BlurDepthMin) && col3.a > 0.0;
                    float match4 = (weight4 < _BlurDepthMin) && col4.a > 0.0;
                    float match5 = (weight5 < _BlurDepthMin) && col5.a > 0.0;
                    float count = match1 + match2 + match3 + match4 + match5;

                    col1.rgb = (match1 * col1.rgb);
                    col1.rgb += (match2 * col2.rgb);
                    col1.rgb += (match3 * col3.rgb);
                    col1.rgb += (match4 * col4.rgb);
                    col1.rgb += (match5 * col5.rgb);
                    col1.rgb /= count;
                    col1 *= (count != 0);
                }

                ApplyDither(col1.rgb, i.uv.xy, _BlurDitherLevel);
                return col1;
            }

            ENDCG
		}
	}
}
