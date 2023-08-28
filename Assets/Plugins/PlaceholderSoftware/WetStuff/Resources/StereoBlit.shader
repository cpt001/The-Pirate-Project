Shader "Hidden/StereoBlit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			#include "UnityCG.cginc"
            			
			sampler2D _MainTex;

			fixed4 frag (v2f_img i) : SV_Target
			{
                return tex2D(_MainTex, UnityStereoTransformScreenSpaceTex(i.uv));
			}
			ENDCG
		}
	}
}
