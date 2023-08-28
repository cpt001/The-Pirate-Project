Shader "Hidden/WS_BlurNormals"
{
    properties
    {
        _MainTex("", 2D) = "white" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D_float _CameraDepthTexture;
    sampler2D _WetDecalSaturationMask;
    sampler2D _Source;
    float4 _Source_ST;
    float2 _Source_TexelSize;

    struct appdata
    {
        float4 vertex : POSITION;
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        float4 uv : TEXCOORD0;
        float2 samples[4] : TEXCOORD1;
    };

    float sampleDepth(float2 uv)
    {
        float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
        return Linear01Depth(depth);
    }

    float3 sampleNormal(float2 uv)
    {
        return tex2D(_Source, uv).xyz * 2 - 1;
    }

    float depthWeight(float2 uv, float depth)
    {
        float d = sampleDepth(uv);
        return 1 - saturate(abs(depth - d) * 100000);        
    }

    v2f vert(appdata v, float2 direction)
    {
        v2f o;
        o.vertex = float4(v.vertex.xyz, 1);

        float2 offset1 = direction * _Source_TexelSize * 1.38461538;
        float2 offset2 = direction * _Source_TexelSize * 3.23076923;

        o.uv = UnityStereoScreenSpaceUVAdjust(ComputeScreenPos(o.vertex), _Source_ST);
        o.samples[0] = o.uv + offset1;
        o.samples[1] = o.uv - offset1;
        o.samples[2] = o.uv + offset2;
        o.samples[3] = o.uv - offset2;

        return o;
    }

    v2f vertHorizontal(appdata v)
    {
        return vert(v, float2(1, 0));
    }

    v2f vertVertical(appdata v)
    {
        return vert(v, float2(0, 1));
    }

    float4 frag(v2f i) : SV_TARGET0
    {
        float depth = sampleDepth(i.uv);
        float3 normal = sampleNormal(i.uv);// * 0.22702702;

        normal += sampleNormal(i.samples[0]) * depthWeight(i.samples[0], depth);// * 0.31621621;
        normal += sampleNormal(i.samples[1]) * depthWeight(i.samples[1], depth);// * 0.31621621;
        normal += sampleNormal(i.samples[2]) * depthWeight(i.samples[2], depth);// * 0.07027027;
        normal += sampleNormal(i.samples[3]) * depthWeight(i.samples[3], depth);// * 0.07027027;

        return float4(normalize(normal) * 0.5 + 0.5, 1);
    }

    float4 fragFinal(v2f i) : SV_TARGET0
    {
        float3 normal = frag(i).xyz;
        float wetness = tex2D(_WetDecalSaturationMask, i.uv).r;
        return float4(normal, saturate((wetness - 0.7) * (1 / 0.3)));
    }

    ENDCG

    SubShader
    {
        Fog { Mode Off }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "BlurHorizontal_NonStenciled"

            CGPROGRAM
            #pragma vertex vertHorizontal
            #pragma fragment frag
            ENDCG
        }

        Pass
        {
            Name "BlurVertical_NonStenciled"
            Blend SrcAlpha OneMinusSrcAlpha, Zero One

            CGPROGRAM
            #pragma vertex vertVertical
            #pragma fragment fragFinal
            ENDCG
        }

        Pass
        {
            Name "BlurHorizontal_Stenciled"

            Stencil
            {
                Ref 1
                ReadMask 1
                Comp equal
                Pass keep
            }

            CGPROGRAM
            #pragma vertex vertHorizontal
            #pragma fragment frag
            ENDCG
        }

        Pass
        {
            Name "BlurVertical_Stenciled"
            Blend SrcAlpha OneMinusSrcAlpha, Zero One

            Stencil
            {
                Ref 1
                ReadMask 1
                Comp equal
                Pass keep
            }

            CGPROGRAM
            #pragma vertex vertVertical
            #pragma fragment fragFinal
            ENDCG
        }
    }
}