Shader "Hidden/LayerOutputHeightmap"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags{ "RenderType" = "Opaque" }
        LOD 100

        Fog{ Mode Off }

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

            struct vertdata
            {
                float4 color: TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Channels;
            float _LayerInputStart;
            float _LayerInputExtent;
            float _LayerOutputStart;
            float _LayerOutputEnd;
            
            vertdata vert(appdata v)
            {
                vertdata o;

                float2 uv = v.vertex.xz + 0.5;
                float4 tex = saturate(tex2Dlod(_MainTex, float4(uv, 0, 0)) * _Channels);
                float height = max(max(tex.r, tex.g), max(tex.b, tex.a));
                
                float4 remapped = saturate((height - _LayerInputStart) / _LayerInputExtent);
                float4 wetness = lerp(_LayerOutputStart, _LayerOutputEnd, remapped);

                v.vertex.y = saturate(wetness) - 0.5;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.color = saturate(_Channels + _Channels.a);
                o.color.a = 1;

                return o;
            }

            fixed4 frag(vertdata i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
