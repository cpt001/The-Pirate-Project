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

Shader "WeatherMaker/WeatherMakerVortexShader"
{
    Properties
	{
		_MainTex ("Color (RGBA)", 2D) = "orange" {}
		_Radius ("Radius", Range(0, 1)) = 0.2
		_InnerRadius ("Inner Radius", Range(0, 1)) = 0.0
		_Angle ("Angle", Range(-6.3, 6.3)) = 1.5
		_Rotation ("Rotation", Float) = 0.0
		_RotationTimeScale("TimeScale", Range(-100.0, 100.0)) = 1.0
		_Center ("Center", Vector) = (0.5, 0.5, 0.0, 0.0)
	}

	SubShader
	{
		ZWrite Off Cull Back Blend SrcAlpha Zero

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

			#include "../WeatherMakerMathShaderInclude.cginc"

			uniform float _Radius;
			uniform float _InnerRadius;
			uniform float _Angle;
			uniform float _Rotation;
			uniform float _RotationTimeScale;
			uniform float2 _Center;
	
			static const float2 rotationSinCos = SinCos(_Rotation + (_WeatherMakerTime.x * _RotationTimeScale));

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

				float2 textureCoordinateToUse = v.uv;
				float dist = distance(_Center, textureCoordinateToUse);

				UNITY_BRANCH
				if (dist < _InnerRadius)
				{
					return fixed4(0.0, 0.5, 0.5, 1.0);
				}
				else if (dist <= _Radius)
				{
					textureCoordinateToUse -= _Center;
					float percent = (_Radius - dist) / _Radius;
					float theta = percent * percent * _Angle * 8.0;
					float s, c;
					sincos(theta, s, c);
					textureCoordinateToUse = float2(dot(textureCoordinateToUse, float2(c, -s)), dot(textureCoordinateToUse, float2(s, c)));
					textureCoordinateToUse = RotateUV(textureCoordinateToUse, rotationSinCos.x, rotationSinCos.y);
					textureCoordinateToUse += _Center;
				}

				fixed4 result = WM_SAMPLE_FULL_SCREEN_TEXTURE(_MainTex, textureCoordinateToUse);
				return result;
            }

            ENDCG
        }
    }
 
    Fallback Off
}