using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Rendering
{
    internal struct InstanceProperties
    {
        private static readonly int SaturationId = Shader.PropertyToID("_Wetness");
        private static readonly int FadeoutId = Shader.PropertyToID("_Fadeout");
        private static readonly int EdgeSharpnessId = Shader.PropertyToID("_EdgeSharpness");
        private static readonly int XLayerInputStartId = Shader.PropertyToID("_XLayerInputStart");
        private static readonly int XLayerInputExtentId = Shader.PropertyToID("_XLayerInputExtent");
        private static readonly int XLayerOutputStartId = Shader.PropertyToID("_XLayerOutputStart");
        private static readonly int XLayerOutputEndId = Shader.PropertyToID("_XLayerOutputEnd");
        private static readonly int YLayerInputStartId = Shader.PropertyToID("_YLayerInputStart");
        private static readonly int YLayerInputExtentId = Shader.PropertyToID("_YLayerInputExtent");
        private static readonly int YLayerOutputStartId = Shader.PropertyToID("_YLayerOutputStart");
        private static readonly int YLayerOutputEndId = Shader.PropertyToID("_YLayerOutputEnd");
        private static readonly int ZLayerInputStartId = Shader.PropertyToID("_ZLayerInputStart");
        private static readonly int ZLayerInputExtentId = Shader.PropertyToID("_ZLayerInputExtent");
        private static readonly int ZLayerOutputStartId = Shader.PropertyToID("_ZLayerOutputStart");
        private static readonly int ZLayerOutputEndId = Shader.PropertyToID("_ZLayerOutputEnd");

        public float Saturation;
        public float Fadeout;
        public float EdgeSharpness;

        public LayerParameters XLayer;
        public LayerParameters YLayer;
        public LayerParameters ZLayer;

        public void LoadInto([NotNull] MaterialPropertyBlock properties, LayerMode mode)
        {
            properties.SetFloat(SaturationId, Saturation);
            properties.SetFloat(FadeoutId, Fadeout);
            properties.SetFloat(EdgeSharpnessId, EdgeSharpness);

            if (mode != LayerMode.None)
                YLayer.LoadInto(properties, YLayerInputStartId, YLayerInputExtentId, YLayerOutputStartId, YLayerOutputEndId);

            if (mode == LayerMode.Triplanar)
            {
                XLayer.LoadInto(properties, XLayerInputStartId, XLayerInputExtentId, XLayerOutputStartId, XLayerOutputEndId);
                ZLayer.LoadInto(properties, ZLayerInputStartId, ZLayerInputExtentId, ZLayerOutputStartId, ZLayerOutputEndId);
            }
        }

        public void LoadInto(
            int index,
            [NotNull] float[] saturation,
            [NotNull] float[] fadeout,
            [NotNull] float[] edgeSharpness,
            LayerArrays? x,
            LayerArrays? y,
            LayerArrays? z)
        {
            saturation[index] = Saturation;
            fadeout[index] = Fadeout;
            edgeSharpness[index] = EdgeSharpness;

            if (x.HasValue)
                XLayer.LoadInto(x.Value, index);

            if (y.HasValue)
                YLayer.LoadInto(y.Value, index);

            if (z.HasValue)
                ZLayer.LoadInto(z.Value, index);
        }

        public static void LoadInto(
            [NotNull] MaterialPropertyBlock properties,
            [NotNull] float[] saturation,
            [NotNull] float[] fadeout,
            [NotNull] float[] edgeSharpness,
            LayerArrays? x,
            LayerArrays? y,
            LayerArrays? z)
        {
            properties.SetFloatArray(SaturationId, saturation);
            properties.SetFloatArray(FadeoutId, fadeout);
            properties.SetFloatArray(EdgeSharpnessId, edgeSharpness);

            if (x.HasValue)
                x.Value.LoadInto(properties, XLayerInputStartId, XLayerInputExtentId, XLayerOutputStartId, XLayerOutputEndId);

            if (y.HasValue)
                y.Value.LoadInto(properties, YLayerInputStartId, YLayerInputExtentId, YLayerOutputStartId, YLayerOutputEndId);

            if (z.HasValue)
                z.Value.LoadInto(properties, ZLayerInputStartId, ZLayerInputExtentId, ZLayerOutputStartId, ZLayerOutputEndId);
        }
    }
}