using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    public interface IWetDecal
    {
        /// <summary>
        ///     Transformation matrix which places this decal into world space
        /// </summary>
        Matrix4x4 WorldTransform { get; }

        /// <summary>
        ///     Bounds of the decal in world space
        /// </summary>
        BoundingSphere Bounds { get; }

        /// <summary>
        ///     Settings used to render this decal
        /// </summary>
        [NotNull] IDecalSettings Settings { get; }

        /// <summary>
        ///     Update this decal
        /// </summary>
        /// <param name="dt"></param>
        void Step(float dt);

        /// <summary>
        /// The mesh used to render this decal when `Shape = Mesh`
        /// </summary>
        [CanBeNull] Mesh Mesh { get; }
    }
}