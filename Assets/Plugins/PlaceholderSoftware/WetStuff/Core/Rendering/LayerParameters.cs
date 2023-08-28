using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Rendering
{
    internal struct LayerParameters
    {
        public Vector4 LayerInputStart;
        public Vector4 LayerInputExtent;
        public Vector4 LayerOutputStart;
        public Vector4 LayerOutputEnd;

        internal void LoadInto([NotNull] MaterialPropertyBlock properties, int inputStartId, int inputExtentId, int outputStartId, int outputEndId)
        {
            properties.SetVector(inputStartId, LayerInputStart);
            properties.SetVector(inputExtentId, LayerInputExtent);
            properties.SetVector(outputStartId, LayerOutputStart);
            properties.SetVector(outputEndId, LayerOutputEnd);
        }

        public void LoadInto(LayerArrays arrays, int index)
        {
            arrays.LayerInputStart[index] = LayerInputStart;
            arrays.LayerInputExtent[index] = LayerInputExtent;
            arrays.LayerOutputStart[index] = LayerOutputStart;
            arrays.LayerOutputEnd[index] = LayerOutputEnd;
        }
    }
}