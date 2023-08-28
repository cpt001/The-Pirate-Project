Shader "WetStuff/DrySurfaceMask"
{
    Properties
    {
        _XLayer("XSaturation", 2D) = "white" {}
        _YLayer("YSaturation", 2D) = "white" {}
        _ZLayer("ZSaturation", 2D) = "white" {}
        _XLayerScaleOffset("XLayerScaleOffset", Vector) = (1,1,0,0)
        _YLayerScaleOffset("YLayerScaleOffset", Vector) = (1,1,0,0)
        _ZLayerScaleOffset("ZLayerScaleOffset", Vector) = (1,1,0,0)
        _BlueNoiseRgba("BlueNoise", 2D) = "white" {}
        _RandomnessTiling("RandomnessTiling", Vector) = (1,1,0,0)
        _SampleJitter("SampleJitter", Vector) = (0,0,0,0)
        [PerRendererData] _Wetness("Saturation", Range(0,1)) = 0.6
        [PerRendererData] _Fadeout("Fadeout", Range(0,1)) = 0.1
        [PerRendererData] _EdgeSharpness("EdgeSharpness", Range(0.001, 10)) = 1
        [PerRendererData] _XLayerInputStart("XLayerInputStart", Vector) = (0,0,0,0)
        [PerRendererData] _XLayerInputExtent("XLayerInputExtent", Vector) = (1,1,1,1)
        [PerRendererData] _XLayerOutputStart("XLayerOutputStart", Vector) = (0,0,0,0)
        [PerRendererData] _XLayerOutputEnd("XLayerOutputEnd", Vector) = (1,1,1,1)
        [PerRendererData] _YLayerInputStart("YLayerInputStart", Vector) = (0,0,0,0)
        [PerRendererData] _YLayerInputExtent("YLayerInputExtent", Vector) = (1,1,1,1)
        [PerRendererData] _YLayerOutputStart("YLayerOutputStart", Vector) = (0,0,0,0)
        [PerRendererData] _YLayerOutputEnd("YLayerOutputEnd", Vector) = (1,1,1,1)
        [PerRendererData] _ZLayerInputStart("ZLayerInputStart", Vector) = (0,0,0,0)
        [PerRendererData] _ZLayerInputExtent("ZLayerInputExtent", Vector) = (1,1,1,1)
        [PerRendererData] _ZLayerOutputStart("ZLayerOutputStart", Vector) = (0,0,0,0)
        [PerRendererData] _ZLayerOutputEnd("ZLayerOutputEnd", Vector) = (1,1,1,1)
    }
    
    SubShader
    {
        Fog { Mode Off }
        ZWrite Off
        Blend Zero OneMinusSrcColor

        Pass
        {
            Name "FrontFacing"

            Cull Back
            ZTest LEqual

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex mask_vert
            #pragma fragment mask_frag
            #pragma multi_compile_instancing

            #pragma multi_compile __ LAYERS_SINGLE LAYERS_TRIPLANAR            
            #pragma multi_compile __ LAYER_PROJECTION_WORLD
            #pragma multi_compile __ JITTER_LAYERS
            #pragma multi_compile __ SHAPE_CIRCLE SHAPE_SQUARE SHAPE_MESH

            #include "UnityCG.cginc"
            #include "WetSurfaceCore.cginc"
            ENDCG
        }

        Pass
        {
            Name "BackFacing"

            Cull Front
            ZTest GEqual

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex mask_vert
            #pragma fragment mask_frag
            #pragma multi_compile_instancing

            #pragma multi_compile __ LAYERS_SINGLE LAYERS_TRIPLANAR
            #pragma multi_compile __ LAYER_PROJECTION_WORLD
            #pragma multi_compile __ JITTER_LAYERS
            #pragma multi_compile __ SHAPE_CIRCLE SHAPE_SQUARE SHAPE_MESH

            #include "UnityCG.cginc"
            #include "WetSurfaceCore.cginc"
            ENDCG
        }

        Pass
        {
            Name "FrontFacingStencil"

            Cull Back
            ZTest LEqual

            Stencil
            {
                Ref 1
                ReadMask  1
                Comp equal
                Pass keep
            }

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex mask_vert
            #pragma fragment mask_frag
            #pragma multi_compile_instancing

            #pragma multi_compile __ LAYERS_SINGLE LAYERS_TRIPLANAR
            #pragma multi_compile __ LAYER_PROJECTION_WORLD
            #pragma multi_compile __ JITTER_LAYERS
            #pragma multi_compile __ SHAPE_CIRCLE SHAPE_SQUARE SHAPE_MESH

            #include "UnityCG.cginc"
            #include "WetSurfaceCore.cginc"
            ENDCG
        }

        Pass
        {
            Name "BackFacingStencil"

            Cull Front
            ZTest GEqual

            Stencil
            {
                Ref 1
                ReadMask  1
                Comp equal
                Pass keep
            }

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex mask_vert
            #pragma fragment mask_frag
            #pragma multi_compile_instancing

            #pragma multi_compile __ LAYERS_SINGLE LAYERS_TRIPLANAR
            #pragma multi_compile __ LAYER_PROJECTION_WORLD
            #pragma multi_compile __ JITTER_LAYERS
            #pragma multi_compile __ SHAPE_CIRCLE SHAPE_SQUARE SHAPE_MESH

            #include "UnityCG.cginc"
            #include "WetSurfaceCore.cginc"
            ENDCG
        }
    }
}