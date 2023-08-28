using System;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Rendering
{
    /// <summary>
    ///     A set of 4 not-null arrays used to hold the layer range data
    /// </summary>
    internal struct LayerArrays
    {
        [NotNull] public readonly Vector4[] LayerInputStart;
        [NotNull] public readonly Vector4[] LayerInputExtent;
        [NotNull] public readonly Vector4[] LayerOutputStart;
        [NotNull] public readonly Vector4[] LayerOutputEnd;

        private LayerArrays([NotNull] Vector4[] layerInputStart, [NotNull] Vector4[] layerInputExtent, [NotNull] Vector4[] layerOutputStart, [NotNull] Vector4[] layerOutputEnd)
        {
            LayerInputStart = layerInputStart;
            LayerInputExtent = layerInputExtent;
            LayerOutputStart = layerOutputStart;
            LayerOutputEnd = layerOutputEnd;
        }

        public static LayerArrays? Create([CanBeNull] Vector4[] layerInputStart, [CanBeNull] Vector4[] layerInputExtent, [CanBeNull] Vector4[] layerOutputStart, [CanBeNull] Vector4[] layerOutputEnd)
        {
            // Check if the first array is null, if so assume they should all be null
            if (layerInputStart == null)
            {
                if (layerInputExtent != null) throw new ArgumentException("First array was null, all subsequent arrays should be null", "layerInputExtent");
                if (layerOutputStart != null) throw new ArgumentException("First array was null, all subsequent arrays should be null", "layerOutputStart");
                if (layerOutputEnd != null) throw new ArgumentException("First array was null, all subsequent arrays should be null", "layerOutputEnd");

                return null;
            }

            // First array was not null, so none of the rest may be null
            if (layerInputExtent == null) throw new ArgumentNullException("layerInputExtent");
            if (layerOutputStart == null) throw new ArgumentNullException("layerOutputStart");
            if (layerOutputEnd == null) throw new ArgumentNullException("layerOutputEnd");

            return new LayerArrays(layerInputStart, layerInputExtent, layerOutputStart, layerOutputEnd);
        }

        public void LoadInto([NotNull] MaterialPropertyBlock properties, int layerInputStartId, int layerInputExtentId, int layerOutputStartId, int layerOutputEndId)
        {
            properties.SetVectorArray(layerInputStartId, LayerInputStart);
            properties.SetVectorArray(layerInputExtentId, LayerInputExtent);
            properties.SetVectorArray(layerOutputStartId, LayerOutputStart);
            properties.SetVectorArray(layerOutputEndId, LayerOutputEnd);
        }
    }
}