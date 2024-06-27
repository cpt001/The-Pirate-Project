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

Shader "Hidden/Universal Render Pipeline/FallbackError"
{
    Properties
	{
		_MainTex ("Color (RGBA)", 2D) = "orange" {}
		_Color ("Tint Color (RGBA)", Color) = (1, 1, 1, 1)
    }

    SubShader
	{
        Tags { "Queue" = "Transparent" }
		LOD 100

		CGINCLUDE

		#pragma target 3.5
		#pragma exclude_renderers gles
		#pragma exclude_renderers d3d9
		
		ENDCG

        Pass
		{
			ZWrite Off
			Cull Off
			Blend SrcAlpha One
 
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			#pragma multi_compile_instancing

			#include "../WeatherMakerCoreShaderInclude.cginc"

			uniform sampler2D _MainTex;
			uniform fixed4 _Color;

			struct appdata_t
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				WM_BASE_VERTEX_INPUT
			};

		    struct v2f
            {
				float4 pos : SV_POSITION;
				fixed4 color : COLOR0;
				half2 uv : TEXCOORD0;
				WM_BASE_VERTEX_TO_FRAG
            };
 
            v2f vert(appdata_t v)
            {
				WM_INSTANCE_VERT(v, v2f, o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color;
                return o; 
            }
			
            fixed4 frag (v2f v) : SV_Target
			{       
				WM_INSTANCE_FRAG(v);
				fixed4 result = tex2D(_MainTex, v.uv) * v.color * _Color;
				return result;
            }

            ENDCG
        }
    }
 
    Fallback Off
}