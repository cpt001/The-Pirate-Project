Shader "Hidden/LayerInputHeightmap"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

        Fog { Mode Off }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
                float4 color: TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
            float4 _Channels;
            float _Min;
            float _Max;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
                v2f o;

                float2 uv = v.vertex.xz + 0.5;
                float4 tex = saturate(tex2Dlod(_MainTex, float4(uv, 0, 0)) * _Channels);
                float height = max(max(tex.r, tex.g), max(tex.b, tex.a));
                
                v.vertex.y = height - 0.5;
				o.vertex = UnityObjectToClipPos(v.vertex);

                float above = saturate(height - _Max) * 10000;
                float below = saturate(_Min - height) * 10000;

                float4 activeColor = saturate(_Channels + _Channels.a);
                float4 inactiveColor = 0.3;

                o.color = lerp(activeColor, inactiveColor, saturate(above + below));
                o.color.a = 1;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                return i.color;
			}
			ENDCG
		}
	}
}
