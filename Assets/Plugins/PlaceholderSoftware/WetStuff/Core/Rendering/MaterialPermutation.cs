using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Rendering
{
    internal struct MaterialPermutation
        : IEquatable<MaterialPermutation>
    {
        public readonly WetDecalMode Mode;
        public readonly LayerMode LayerMode;
        public readonly ProjectionMode LayerProjectionMode;
        public readonly DecalShape Shape;
        public readonly bool EnableJitter;

        public MaterialPermutation(WetDecalMode mode, LayerMode layerMode, ProjectionMode layerProjectionMode, DecalShape shape, bool enableJitter)
        {
            Mode = mode;
            LayerMode = layerMode;
            LayerProjectionMode = layerProjectionMode;
            Shape = shape;
            EnableJitter = enableJitter;
        }

        public int RenderOrder
        {
            get { return Mode == WetDecalMode.Wet ? 0 : 1; }
        }

        public Shader SelectShader(Shader wet, Shader dry)
        {
            return Mode == WetDecalMode.Wet ? wet : dry;
        }

        #region Equality

        public override bool Equals(object obj)
        {
            if (!(obj is MaterialPermutation)) return false;

            return Equals((MaterialPermutation) obj);
        }

        public bool Equals(MaterialPermutation other)
        {
            return EqualityComparer<WetDecalMode>.Default.Equals(Mode, other.Mode) &&
                   EqualityComparer<LayerMode>.Default.Equals(LayerMode, other.LayerMode) &&
                   EqualityComparer<ProjectionMode>.Default.Equals(LayerProjectionMode, other.LayerProjectionMode) &&
                   EqualityComparer<DecalShape>.Default.Equals(Shape, other.Shape) &&
                   EnableJitter == other.EnableJitter;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 409612459;
                hashCode = hashCode * -1521134295 + Mode.GetHashCode();
                hashCode = hashCode * -1521134295 + LayerMode.GetHashCode();
                hashCode = hashCode * -1521134295 + LayerProjectionMode.GetHashCode();
                hashCode = hashCode * -1521134295 + Shape.GetHashCode();
                hashCode = hashCode * -1521134295 + EnableJitter.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(MaterialPermutation permutation1, MaterialPermutation permutation2)
        {
            return permutation1.Equals(permutation2);
        }

        public static bool operator !=(MaterialPermutation permutation1, MaterialPermutation permutation2)
        {
            return !(permutation1 == permutation2);
        }

        #endregion
    }
}