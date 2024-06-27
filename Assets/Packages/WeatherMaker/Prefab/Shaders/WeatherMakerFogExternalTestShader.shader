Shader "WeatherMaker/WeatherMakerFogExternalShader"
{
    Properties
    {
        _PointSpotLightMultiplier("Point/Spot Light Multiplier", Range(0, 10)) = 1
        _DirectionalLightMultiplier("Directional Light Multiplier", Range(0, 10)) = 1
        _AmbientLightMultiplier("Ambient Light Multiplier", Range(0, 10)) = 2
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-1" }


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "WeatherMakerFogExternalShaderInclude.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
            };


            v2f vert(appdata v)
            {
                v2f o;
                o.uv = v.uv;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = float4(0,0,0,1);
                col = ComputeWeatherMakerFog(col, i.worldPos, false);
                return col;
            }
            ENDCG
        }
    }
}
