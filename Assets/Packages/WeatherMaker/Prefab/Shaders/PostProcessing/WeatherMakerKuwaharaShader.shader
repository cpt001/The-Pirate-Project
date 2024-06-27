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

Shader "WeatherMaker/WeatherMakerKuwaharaShader"
{
    Properties
	{
		_MainTex ("Color (RGBA)", 2D) = "orange" {}
		_Radius ("Radius", Range(1, 8)) = 4
	}

		SubShader
	{
		ZWrite Off Cull Back Blend One Zero

		CGINCLUDE

		#pragma target 3.5
		#pragma exclude_renderers gles
		#pragma exclude_renderers d3d9

		ENDCG

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			#pragma multi_compile_instancing

			#define WEATHER_MAKER_ENABLE_TEXTURE_DEFINES
			#define WEATHER_MAKER_IS_FULL_SCREEN_EFFECT

			#include "../WeatherMakerCoreShaderInclude.cginc"

			uniform fixed _Radius;
			uniform float3 _KuwaharaCoords[324];
			uniform int _KuwaharaCoordsLength;

			struct region { int x1, y1, x2, y2; };
			static const region R[4] =
			{
				{-_Radius, -_Radius,       0,       0},
				{       0, -_Radius, _Radius,       0},
				{       0,        0, _Radius, _Radius},
				{-_Radius,        0,       0, _Radius}
			};
			static const float invRadius = 1.0 / (float((_Radius + 1) * (_Radius + 1)));

			struct appdata_t
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				WM_BASE_VERTEX_INPUT
			};

		    struct v2f
            {
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				WM_BASE_VERTEX_TO_FRAG
            };
 
            v2f vert(appdata_t v)
            {
				WM_INSTANCE_VERT(v, v2f, o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o; 
            }
			
            fixed4 frag (v2f v) : SV_Target
			{       
				WM_INSTANCE_FRAG(v);
				float2 uv = v.uv;
				float4 col = WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, uv);

				UNITY_BRANCH
				if (col.a < WM_MIN_PIXEL_VALUE)
				{
					return col;
				}

				float4 m[4] = { float4Zero, float4Zero, float4Zero, float4Zero };
				float4 s[4] = { float4Zero, float4Zero, float4Zero, float4Zero };
				float4 c;
				float2 step;
				int k, j, i;
				float min = 1e+2;
				float s2;
				float lt;

				UNITY_BRANCH
				if (_KuwaharaCoordsLength <= 0)
				{
					UNITY_LOOP
					for (k = 0; k < 4; ++k)
					{
						UNITY_LOOP
						for (step.y = uv.y + (R[k].y1 * _MainTex_TexelSize.y), j = R[k].y1; j <= R[k].y2; ++j, step.y += _MainTex_TexelSize.y)
						{
							UNITY_LOOP
							for (step.x = uv.x + (R[k].x1 * _MainTex_TexelSize.x), i = R[k].x1; i <= R[k].x2; ++i, step.x += _MainTex_TexelSize.x)
							{
								c = WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, step);
								m[k] += c;
								s[k] += c * c;
							}
						}
					}
				}
				else
				{
					float3 coord;
					UNITY_LOOP
					for (i = 0; i < _KuwaharaCoordsLength; i++)
					{
						coord = _KuwaharaCoords[i];
						c = WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, uv + coord.xy);
						m[coord.z] += c;
						s[coord.z] += c * c;
					}
				}

				UNITY_UNROLL
				for (k = 0; k < 4; ++k)
				{
					m[k] *= invRadius;
					s[k] = abs((s[k] * invRadius) - (m[k] * m[k]));
					s2 = s[k].r + s[k].g + s[k].b;
					lt = (s2 < min);
					min = lerp(min, s2, lt);
					col = lerp(col, m[k], lt);
				}

				return col;
            }

            ENDCG
        }
    }
 
    Fallback Off
}