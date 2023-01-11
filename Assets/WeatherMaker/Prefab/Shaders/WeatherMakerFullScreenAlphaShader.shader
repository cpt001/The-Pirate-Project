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

Shader "WeatherMaker/WeatherMakerFullScreenAlphaShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AlphaDitherLevel("Dither level", Range(0.0, 1.0)) = 0.0004
		_BlendOp("Blend Op", Int) = 0
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest [_ZTest]
		BlendOp[_BlendOp]
		Blend [_SrcBlendMode][_DstBlendMode]

		CGINCLUDE

		#pragma target 3.5
		#pragma exclude_renderers gles
		#pragma exclude_renderers d3d9
		
		#define WEATHER_MAKER_ENABLE_TEXTURE_DEFINES

		ENDCG

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			#define WEATHER_MAKER_IS_FULL_SCREEN_EFFECT
			#define WEATHER_MAKER_ENABLE_TEXTURE_DEFINES
			#define WEATHER_MAKER_MAIN_TEX_SAMPLERS

			#include "WeatherMakerCoreShaderInclude.cginc"

			uniform float _AlphaDitherLevel;
			uniform float _DepthBlockThreshold;;

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				WM_BASE_VERTEX_TO_FRAG
			};
	 
			v2f vert(wm_appdata_base v)
			{
				WM_INSTANCE_VERT(v, v2f, o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = AdjustFullScreenUV(v.texcoord.xy);
				return o;
			}

			fixed4 alphaTap(float2 uv)
			{
				fixed4 col = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_MainTex, _point_clamp_sampler, uv);

				UNITY_BRANCH
				if (col.a > 0.0)
				{
					fixed4 linearSample = WM_SAMPLE_FULL_SCREEN_TEXTURE_SAMPLER(_MainTex, _linear_clamp_sampler, uv);
                    if (linearSample.a >= col.a)
                    {
                        col = linearSample;
                    }
				}

				return col;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				WM_INSTANCE_FRAG(i);

				float sourceDepth = GetDepth01(i.uv);

				// debug depth
				//return fixed4(sourceDepth, sourceDepth, sourceDepth, 1.0);
				//return fixed4(depth1, depth1, depth1, 1.0);

				// occluded pixel
				UNITY_BRANCH
				if (sourceDepth < 1.0)
				{
					return fixed4Zero;
				}
				else
				{
					fixed4 col = alphaTap(i.uv);

					UNITY_BRANCH
					if (col.a > 0.0)
					{
						ApplyDither(col.rgb, i.uv.xy, _AlphaDitherLevel);
						return col;
					}
					else
					{
						return fixed4Zero;
					}
				}
			}

			ENDCG
		}
	}
}
