using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Rendering
{
    internal struct MaterialProperties
        : IEquatable<MaterialProperties>
    {
        private static readonly int XLayerId = Shader.PropertyToID("_XLayer");
        private static readonly int XLayerScaleOffsetId = Shader.PropertyToID("_XLayerScaleOffset");
        private static readonly int YLayerId = Shader.PropertyToID("_YLayer");
        private static readonly int YLayerScaleOffsetId = Shader.PropertyToID("_YLayerScaleOffset");
        private static readonly int ZLayerId = Shader.PropertyToID("_ZLayer");
        private static readonly int ZLayerScaleOffsetId = Shader.PropertyToID("_ZLayerScaleOffset");
        private static readonly int BlueNoiseRgbaId = Shader.PropertyToID("_BlueNoiseRgba");
        private static readonly int RandomnessTilingId = Shader.PropertyToID("_RandomnessTiling");
        private static readonly int SampleJitterId = Shader.PropertyToID("_SampleJitter");

        public readonly Texture2D XLayer;
        public readonly Vector4 XLayerScaleOffset;

        public readonly Texture2D YLayer;
        public readonly Vector4 YLayerScaleOffset;

        public readonly Texture2D ZLayer;
        public readonly Vector4 ZLayerScaleOffset;

        public readonly Texture2D BlueNoiseRgba;
        public readonly Vector2 RandomnessTiling;
        public readonly Vector2 SampleJitter;

        public MaterialProperties(
            Texture2D xLayer, Vector4 xLayerScaleOffset,
            Texture2D yLayer, Vector4 yLayerScaleOffset,
            Texture2D zLayer, Vector4 zLayerScaleOffset,
            Texture2D blueNoiseRgba, Vector2 randomnessTiling, Vector2 sampleJitter)
        {
            XLayer = xLayer;
            XLayerScaleOffset = xLayerScaleOffset;
            YLayer = yLayer;
            YLayerScaleOffset = yLayerScaleOffset;
            ZLayer = zLayer;
            ZLayerScaleOffset = zLayerScaleOffset;
            BlueNoiseRgba = blueNoiseRgba;
            RandomnessTiling = randomnessTiling;
            SampleJitter = sampleJitter;
        }

        #region Equality

        public override bool Equals(object obj)
        {
            return obj is MaterialProperties && Equals((MaterialProperties) obj);
        }

        public bool Equals(MaterialProperties other)
        {
            return EqualityComparer<Vector4>.Default.Equals(XLayerScaleOffset, other.XLayerScaleOffset) &&
                   EqualityComparer<Texture2D>.Default.Equals(XLayer, other.XLayer) &&
                   EqualityComparer<Vector4>.Default.Equals(YLayerScaleOffset, other.YLayerScaleOffset) &&
                   EqualityComparer<Texture2D>.Default.Equals(YLayer, other.YLayer) &&
                   EqualityComparer<Vector4>.Default.Equals(ZLayerScaleOffset, other.ZLayerScaleOffset) &&
                   EqualityComparer<Texture2D>.Default.Equals(ZLayer, other.ZLayer) &&
                   EqualityComparer<Texture2D>.Default.Equals(BlueNoiseRgba, other.BlueNoiseRgba) &&
                   EqualityComparer<Vector2>.Default.Equals(RandomnessTiling, other.RandomnessTiling) &&
                   EqualityComparer<Vector2>.Default.Equals(SampleJitter, other.SampleJitter);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 47332817;
                hashCode = hashCode * -1521134295 + EqualityComparer<Vector4>.Default.GetHashCode(XLayerScaleOffset);
                hashCode = hashCode * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(XLayer);
                hashCode = hashCode * -1521134295 + EqualityComparer<Vector4>.Default.GetHashCode(YLayerScaleOffset);
                hashCode = hashCode * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(YLayer);
                hashCode = hashCode * -1521134295 + EqualityComparer<Vector4>.Default.GetHashCode(ZLayerScaleOffset);
                hashCode = hashCode * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(ZLayer);
                hashCode = hashCode * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(BlueNoiseRgba);
                hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(RandomnessTiling);
                hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(SampleJitter);
                return hashCode;
            }
        }

        public static bool operator ==(MaterialProperties properties1, MaterialProperties properties2)
        {
            return properties1.Equals(properties2);
        }

        public static bool operator !=(MaterialProperties properties1, MaterialProperties properties2)
        {
            return !(properties1 == properties2);
        }

        #endregion

        public void LoadInto([NotNull] Material properties)
        {
            if (XLayer != null)
            {
                properties.SetTexture(XLayerId, XLayer);
                properties.SetVector(XLayerScaleOffsetId, XLayerScaleOffset);
            }

            if (YLayer != null)
            {
                properties.SetTexture(YLayerId, YLayer);
                properties.SetVector(YLayerScaleOffsetId, YLayerScaleOffset);
            }

            if (ZLayer != null)
            {
                properties.SetTexture(ZLayerId, ZLayer);
                properties.SetVector(ZLayerScaleOffsetId, ZLayerScaleOffset);
            }

            properties.SetTexture(BlueNoiseRgbaId, BlueNoiseRgba);
            properties.SetVector(RandomnessTilingId, RandomnessTiling);
            properties.SetVector(SampleJitterId, SampleJitter);
        }
    }
}