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

Shader "WeatherMaker/WeatherMakerWeatherMapSdfShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { }
		LOD 100
		Blend One Zero
		Fog { Mode Off }
		ZWrite Off
		ZClip Off
		ZTest Off
		Cull Back

		CGINCLUDE

		#include "WeatherMakerCoreShaderInclude.cginc"

		#pragma target 3.5
		#pragma exclude_renderers gles
		#pragma exclude_renderers d3d9

		uniform UNITY_DECLARE_TEX2D(_MainTex);
		uniform UNITY_DECLARE_TEX2D(_PrevSdfTex);
		uniform UNITY_DECLARE_TEX2D(_CurSdfTex);

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
		};

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv;
			return o;
		}

		float4 _CurSdfTex_TexelSize;
		float4 _PrevSdfTex_TexelSize;

		uniform float4 _SdfPixelSize;
		
		static const float offset1 = -1.0;
		static const float offset2 = 1.0;
		static const float offset3 = -0.5;
		static const float offset4 = 0.5;

		static const float3 offsetsPrev[8] =
		{
			float3(offset1 * _PrevSdfTex_TexelSize.x, 0.0, 0.0), // left
			float3(offset2 * _PrevSdfTex_TexelSize.x, 0.0, 0.0), // right
			float3(0.0, offset1 * _PrevSdfTex_TexelSize.y, 0.0), // top
			float3(0.0, offset2 * _PrevSdfTex_TexelSize.y, 0.0), // bottom
			float3(offset1 * _PrevSdfTex_TexelSize.x, offset1 * _PrevSdfTex_TexelSize.y, 1.0), // top left
			float3(offset2 * _PrevSdfTex_TexelSize.x, offset1 * _PrevSdfTex_TexelSize.y, 1.0), // top right
			float3(offset1 * _PrevSdfTex_TexelSize.x, offset2 * _PrevSdfTex_TexelSize.y, 1.0), // bottom left
			float3(offset2 * _PrevSdfTex_TexelSize.x, offset2 * _PrevSdfTex_TexelSize.y, 1.0), // bottom right
		};

		static const float3 offsetsPrev2[4] =
		{
			float3(offset3 * _PrevSdfTex_TexelSize.x, 0.0, 0.0), // left
			float3(offset4 * _PrevSdfTex_TexelSize.x, 0.0, 0.0), // right
			float3(0.0, offset3 * _PrevSdfTex_TexelSize.y, 0.0), // top
			float3(0.0, offset4 * _PrevSdfTex_TexelSize.y, 0.0), // bottom
		};

		ENDCG

		// sdf downsample
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			fixed4 frag (v2f input) : SV_Target
			{
				fixed4 currentSdfCol1 = WM_SAMPLE_TEX2D_SAMPLER_LOD(_PrevSdfTex, _point_clamp_sampler, float4(input.uv + offsetsPrev2[0], 0.0, 0.0));
				fixed4 currentSdfCol2 = WM_SAMPLE_TEX2D_SAMPLER_LOD(_PrevSdfTex, _point_clamp_sampler, float4(input.uv + offsetsPrev2[1], 0.0, 0.0));
				fixed4 currentSdfCol3 = WM_SAMPLE_TEX2D_SAMPLER_LOD(_PrevSdfTex, _point_clamp_sampler, float4(input.uv + offsetsPrev2[2], 0.0, 0.0));
				fixed4 currentSdfCol4 = WM_SAMPLE_TEX2D_SAMPLER_LOD(_PrevSdfTex, _point_clamp_sampler, float4(input.uv + offsetsPrev2[3], 0.0, 0.0));
				currentSdfCol1.r = max(currentSdfCol1.r, max(currentSdfCol2.r, max(currentSdfCol3.r, currentSdfCol4.r)));
				return currentSdfCol1;
			}

			ENDCG
		}

		// sdf calculator
		Pass
		{
			ColorMask A

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			fixed4 frag (v2f input) : SV_Target
			{
				fixed4 currentSdfCol = WM_SAMPLE_TEX2D_SAMPLER_LOD(_PrevSdfTex, _point_clamp_sampler, float4(input.uv, 0.0, 0.0));

				// current sdf tex has an alpha that needs to be computed based off of the red channel of the texture
				UNITY_LOOP
				for (uint i = 0; i < 8; i++)
				{
					float2 offset = offsetsPrev[i].xy;
					float4 coord = float4(input.uv + offset, 0.0, 0.0);
					fixed4 samp = WM_SAMPLE_TEX2D_SAMPLER_LOD(_PrevSdfTex, _point_clamp_sampler, coord);
					if (samp.r >= _SdfPixelSize.z)
					{
						currentSdfCol.a = _SdfPixelSize.x;
						i = 8;
					}
				}

				return currentSdfCol;
			}

			ENDCG
		}
	}

	Fallback Off
}
