Shader "Demo/ExcludeWetness" {
	Properties {
	}
	SubShader {
        // no culling or depth
        Cull Off
        ZTest Always
        ZWrite Off

        // no fog
        Fog { Mode Off }

        Blend Off

        Stencil
        {
            Ref 0
            WriteMask 1
            Comp always
            Pass replace
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma exclude_renderers nomrt

            struct appdata
            {
                float4 pos : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = mul(unity_ObjectToWorld, v.pos);
                return o;
            }
            
            void frag(v2f i)
            {
            }

            ENDCG
        }
	}
}
