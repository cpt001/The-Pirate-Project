Shader "Hidden/WetSurfaceModifier"
{
	SubShader
	{
		// no culling or depth
		Cull Off 
		ZTest Always
		ZWrite Off

		// no fog
		Fog { Mode Off }

		Blend 0 DstColor Zero
		Blend 1 Off
        Blend 2 DstColor Zero

        Pass
        {
            Name "NonStenciled"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma exclude_renderers nomrt

            #include "UnityCG.cginc"
            #include "WetSurfaceModifier.cginc"
            ENDCG
        }
        
		Pass
		{
            Name "Stenciled"

            Stencil
            {
                Ref 1
                ReadMask 1
                Comp equal
                Pass keep
            }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma exclude_renderers nomrt
			
            #include "UnityCG.cginc"
            #include "WetSurfaceModifier.cginc"
			ENDCG
		}
	}

	FallBack Off
}