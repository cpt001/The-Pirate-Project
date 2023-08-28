Shader "Hidden/ReconstructNormals"
{
    SubShader
    {
        Fog { Mode Off }
        Cull Off
        ZWrite Off
        ZTest Always
                
        Pass
        {
            Name "GeometryNormals"
            Blend SrcAlpha OneMinusSrcAlpha, Zero One

            CGPROGRAM

            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D_float _CameraDepthTexture;
            sampler2D _WetDecalSaturationMask;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ray : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = float4(v.vertex.xyz, 1);
                o.uv = ComputeScreenPos(o.vertex);
                o.ray = UnityObjectToViewPos(v.vertex) * float3(-1, -1, 1);
                return o;
            }

            float4 calcPosition(float2 uv, float3 ray)
            {
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
                depth = Linear01Depth(depth);
                return float4(ray * depth, depth);
            }

            float4 depthClampPosition(float2 uv, float3 ray, float4 center_pos)
            {
                float4 pos = calcPosition(uv, ray);
                float weight = saturate(abs(pos.w - center_pos.w) * 100000);
                return float4(lerp(pos.xyz, center_pos.xyz, weight), 1 - weight);
            }

            void frag(
                v2f i,
                out float4 output: SV_TARGET0)
            {
                float3 ray = i.ray * (_ProjectionParams.z / i.ray.z);
                float3 ray_dx = ddx(ray);
                float3 ray_dy = ddy(ray);

                float2 uv = i.uv.xy / i.uv.w;
                float2 uv_dx = ddx(uv);
                float2 uv_dy = ddy(uv);

                float4 vpos = calcPosition(uv, ray);

                float4 bottom = depthClampPosition(uv - uv_dy, ray - ray_dy, vpos);
                float4 top = depthClampPosition(uv + uv_dy, ray + ray_dy, vpos);
                float4 right = depthClampPosition(uv + uv_dx, ray + ray_dx, vpos);
                float4 left = depthClampPosition(uv - uv_dx, ray - ray_dx, vpos);

                float3 dx = right.xyz - left.xyz;
                float3 dy = top.xyz - bottom.xyz;

                float3 normal = normalize(cross(dx, dy));

//                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
//                depth = Linear01Depth(depth);
//
//                float4 vpos = float4(i.ray * depth, 1);
//                float3 normal = normalize(cross(ddx(vpos.xyz), ddy(vpos.xyz)));
                normal = float3(normal.x, -normal.y, normal.z);
                float3 wnormal = normalize(mul((float3x3)unity_CameraToWorld, normal));

                float confidence = saturate((top.w + bottom.w) * (left.w + right.w));
                float wetness = tex2D(_WetDecalSaturationMask, i.uv).r;
                
                output = float4(wnormal * 0.5 + 0.5, confidence * saturate((wetness - 0.5) * 2));
            }

            ENDCG
        }
    }
}