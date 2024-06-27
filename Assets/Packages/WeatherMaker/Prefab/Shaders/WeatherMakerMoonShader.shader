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

// https://alastaira.wordpress.com/2014/12/30/adding-shadows-to-a-unity-vertexfragment-shader-in-7-easy-steps/

Shader "WeatherMaker/WeatherMakerMoonShader"
{
	Properties
	{
		_MainTex("Moon Texture", 2D) = "white" {}
		_MaxFade("Moon Max Fade", Range(0.0, 1.0)) = 0.0
		_MaxEdgeFade("Moon Max Edge Fade", Range(0.0, 1.0)) = 1.0
		_HorizonFade("Moon Horizon Fade (X = Multiplier, Y = Power)", Vector) = (3.0, 3.0, 0.0, 0.0)
		_DirLightPower("Dir Light Power", Range(0.0, 4.0)) = 1.0
	}
	SubShader
	{
		Tags { "Queue" = "AlphaTest+52" }
		Cull Back Lighting Off ZWrite Off ZTest LEqual Blend One OneMinusSrcAlpha

		CGINCLUDE

		#pragma target 3.5
		#pragma exclude_renderers gles
		#pragma exclude_renderers d3d9
		
		#define WEATHER_MAKER_ENABLE_TEXTURE_DEFINES

		#include "WeatherMakerSkyShaderInclude.cginc"

		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma glsl_no_auto_normalization
		#pragma multi_compile_instancing
		#pragma multi_compile __ UNITY_URP

		uniform fixed _MaxFade;
		uniform fixed _MaxEdgeFade;
		uniform fixed2 _HorizonFade;
		uniform fixed _DirLightPower;

		static const fixed3 tintColor = _TintColor.rgb * _WeatherMakerSunColor.rgb * _TintColor.a;
		static const fixed moonFade = clamp(_WeatherMakerSunColor.a, _MaxFade, 1.0);

		struct vertexOutput
		{
			float4 pos: SV_POSITION;
			float3 normalWorld: NORMAL;
			float2 tex: TEXCOORD0;
			float4 grabPos: TEXCOORD1;
			float4 ray : TEXCOORD2;

#if defined(UNITY_URP)

			float4 projPos : TEXCOORD3;

#endif

			WM_BASE_VERTEX_TO_FRAG
		};

		vertexOutput vert(wm_appdata_base v)
		{
			WM_INSTANCE_VERT(v, vertexOutput, o);
			o.normalWorld = normalize(WorldSpaceVertexPosNear(v.normal));
			o.pos = UnityObjectToClipPosFarPlane(v.vertex);
			o.tex = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.grabPos = ComputeGrabScreenPos(o.pos);
			o.ray.xyz = -WorldSpaceViewDir(v.vertex);
			o.ray.w = 1.0 - saturate((_WeatherMakerSunDirectionUp.y + 0.2) * 14.0);

#if defined(UNITY_URP)

			o.projPos = ComputeScreenPos(o.pos);

#endif

			return o;
		}

		fixed4 frag(vertexOutput i) : SV_TARGET
		{
			WM_INSTANCE_FRAG(i);

#if defined(UNITY_URP)

			float2 screenUV = i.projPos.xy / max(0.0001, i.projPos.w);
			float depth = GetDepth01(screenUV);
			UNITY_BRANCH
			if (depth >= 1.0)

#endif

			{
				i.ray.xyz = normalize(i.ray.xyz);

				if (WM_ENABLE_SKY_SUN_ECLIPSE)
				{
					fixed lerpSun = CalcSunSpot(_WeatherMakerSunVar1.x * 144, _WeatherMakerSunDirectionUp, i.ray.xyz);
					fixed feather = saturate(dot(-i.ray.xyz, i.normalWorld) * 3.0);
					return fixed4(0.0, 0.0, 0.0, lerpSun * feather);
				}
				else
				{
					fixed4 moonColor = tex2D(_MainTex, i.tex.xy);
					fixed lightNormal = max(0.0, dot(i.normalWorld, _WeatherMakerSunDirectionUp));
					lightNormal = pow(lightNormal, _DirLightPower);
					fixed3 lightFinal = lightNormal * tintColor;
					fixed lightMax = min(1.0, max(lightFinal.r, max(lightFinal.g, lightFinal.b)));
					fixed lightMax3 = min(1.0, lightMax * 3.0);

					// alpha ramps up as night approaches or if the moon is lit by another light source
					moonColor.a = max(i.ray.w, lightMax);

					// moon blends with sky as sun is out more
					moonColor.a *= lerp(1.0, 0.334, lightMax);

					// fade moon out at night
					moonColor.a *= lerp(1.0, 0.0, (1.0 - lightMax3) * moonFade);

					// fade moon edges
					moonColor.a *= lerp(1.0, saturate(1.5 * dot(-i.ray.xyz, i.normalWorld)), _MaxEdgeFade);

					// fade at horizon
					moonColor.a *= pow(saturate(i.ray.y * _HorizonFade.x), _HorizonFade.y);

					// apply sun light
					moonColor.rgb *= lightFinal * moonColor.a;

					// soften alpha
					moonColor.a *= moonColor.a;
					
					return moonColor;
				}
			}

#if defined(UNITY_URP)

			else
			{
				return fixed4Zero;
			}

#endif

		}

		ENDCG

		Pass
		{
			Tags { }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			ENDCG
		}
	}

	FallBack Off
}
