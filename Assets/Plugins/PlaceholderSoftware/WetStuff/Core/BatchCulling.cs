using System;
using PlaceholderSoftware.WetStuff.Debugging;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    internal class BatchCulling
        : IDisposable
    {
        private static readonly Log Log = Logs.Create(LogCategory.Graphics, typeof(BatchCulling).Name);

        private readonly WetDecalSystem.MaterialBatch _batch;
        private readonly Camera _camera;
        private readonly CullingGroup _culling;
        private readonly float[] _distanceThresholds;

#if UNITY_EDITOR
        private readonly Plane[] _frustumPlanes = new Plane[6];
#endif

        private int[] _visibleIndices;

        public BatchCulling([NotNull] Camera camera, [NotNull] WetDecalSystem.MaterialBatch batch)
        {
            if (camera == null) throw new ArgumentNullException("camera");
            if (batch == null) throw new ArgumentNullException("batch");

            _camera = camera;
            _batch = batch;

            _visibleIndices = new int[8];
            _distanceThresholds = new[] { camera.nearClipPlane, camera.farClipPlane };

            _culling = new CullingGroup { targetCamera = camera };
            _culling.SetBoundingDistances(_distanceThresholds);
            _culling.SetDistanceReferencePoint(camera.transform);
            _culling.SetBoundingSpheres(batch.BoundingSpheres);
            _culling.SetBoundingSphereCount(batch.Decals.Count);
        }

        public void Dispose()
        {
            _culling.Dispose();
        }

        public void Update()
        {
            // Set new bounding sphere array in the culling system if it has changed
            _culling.SetBoundingSpheres(_batch.BoundingSpheres);

            // Set decal count in the culling system if it has changed
            _culling.SetBoundingSphereCount(_batch.Decals.Count);

            // Update culling thresholds
            _distanceThresholds[0] = _camera.nearClipPlane;
            _distanceThresholds[1] = _camera.farClipPlane;
        }

        private static T[] EnsureArrayCapacity<T>([NotNull] T[] array, int count)
        {
            if (array.Length < count)
            {
                var l = Math.Max(count * 2, array.Length * 2);
                Log.Debug("Resizing array from {0} to {1}", array.Length, l);
                Array.Resize(ref array, l);
            }

            return array;
        }

        public ArraySegment<int> Cull(int distanceBand)
        {
            _visibleIndices = EnsureArrayCapacity(_visibleIndices, _batch.Decals.Count);

#if UNITY_EDITOR
            // Calculate the planes around the camera frustum
            GeometryUtility.CalculateFrustumPlanes(_camera.projectionMatrix * _camera.worldToCameraMatrix, _frustumPlanes);
            var planes = _frustumPlanes;

            // Pick our distance bands, for the first band implicitly go from -inf to whatever the band threshold is
            var near = distanceBand > 0 ? _distanceThresholds[distanceBand - 1] : float.NegativeInfinity;
            var far = _distanceThresholds[distanceBand];

            // Now check each decal individually
            var count = 0;
            for (var i = 0; i < _batch.Decals.Count; i++)
            {
                var bounds = _batch.BoundingSpheres[i];

                // Skip invalid bounds
                if (float.IsNaN(bounds.position.x) || float.IsNaN(bounds.position.y) || float.IsNaN(bounds.position.z))
                    continue;
                if (float.IsNaN(bounds.radius) || bounds.radius <= 0)
                    continue;

                // Is the AABB even inside the frustum? If not don't bother checking any further
                var aabb = new Bounds(bounds.position, 2 * new Vector3(bounds.radius, bounds.radius, bounds.radius));
                if (GeometryUtility.TestPlanesAABB(planes, aabb))
                {
                    // Calculate how far along -Z (camera look direction) the nearest point of the bounding sphere is
                    var viewPos = _camera.worldToCameraMatrix.MultiplyPoint(bounds.position);
                    var nearDistance = -viewPos.z - bounds.radius;

                    // Check if it's in the band, and if so include it in the output set
                    if (nearDistance >= near && nearDistance < far)
                        _visibleIndices[count++] = i;
                }
            }

            return new ArraySegment<int>(_visibleIndices, 0, count);
#else
            var v = _culling.QueryIndices(true, distanceBand, _visibleIndices, 0);
            return new ArraySegment<int>(_visibleIndices, 0, v);
#endif
        }
    }
}