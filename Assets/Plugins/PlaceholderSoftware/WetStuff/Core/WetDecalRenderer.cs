using System;
using System.Collections.Generic;
using PlaceholderSoftware.WetStuff.Debugging;
using PlaceholderSoftware.WetStuff.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using Object = UnityEngine.Object;
using RenderSettings = PlaceholderSoftware.WetStuff.Rendering.RenderSettings;

namespace PlaceholderSoftware.WetStuff
{
    /// <summary>
    ///     Renders all WetDecal instances into the saturation mask texture.
    /// </summary>
    internal class WetDecalRenderer : IDisposable
    {
        #region Nested Types

        public class DecalBatch : IDisposable, IComparable<DecalBatch>
        {
            private const int MaxInstancesPerBatch = 1023;

            private readonly Mesh _box;

            private readonly BatchCulling _culling;

            // Instancing data
            private readonly MaterialPropertyBlock _instancingPropertyBlock;
            private float[] _edgeFadeout;
            private float[] _edgeSharpness;
            private Matrix4x4[] _matrices;
            private float[] _saturation;
            private Vector4[] _xLayerInputExtent;
            private Vector4[] _xLayerInputStart;
            private Vector4[] _xLayerOutputEnd;
            private Vector4[] _xLayerOutputStart;
            private Vector4[] _yLayerInputExtent;
            private Vector4[] _yLayerInputStart;
            private Vector4[] _yLayerOutputEnd;
            private Vector4[] _yLayerOutputStart;
            private Vector4[] _zLayerInputExtent;
            private Vector4[] _zLayerInputStart;
            private Vector4[] _zLayerOutputEnd;
            private Vector4[] _zLayerOutputStart;

            public WetDecalSystem.MaterialBatch Batch { get; private set; }

            public DecalBatch([NotNull] Camera camera, [NotNull] Mesh box, [NotNull] WetDecalSystem.MaterialBatch batch)
            {
                Batch = batch;
                _box = box;

                _culling = new BatchCulling(
                    camera,
                    batch
                );

                _instancingPropertyBlock = new MaterialPropertyBlock();

                _matrices = new Matrix4x4[16];
            }

            public int CompareTo([CanBeNull] DecalBatch other)
            {
                if (other == null)
                    return -1;

                return Batch.Permutation.RenderOrder.CompareTo(other.Batch.Permutation.RenderOrder);
            }


            public void Dispose()
            {
                if (_culling != null) _culling.Dispose();
            }

            public void Update()
            {
            }

            public void PrepareDraw(CommandBuffer cmd)
            {
                // Update culling to calculate which decals are visible
                _culling.Update();

                // Render near decals
                Draw(cmd, 0, RenderSettings.Instance.EnableStencil ? 3 : 1);

                // Render distant decals
                Draw(cmd, 1, RenderSettings.Instance.EnableStencil ? 2 : 0);
            }

            private int Draw(CommandBuffer cmd, int distanceBand, int shaderPass)
            {
                var visible = _culling.Cull(distanceBand);

                if (RenderSettings.Instance.EnableInstancing)
                {
                    for (var i = 0; i < visible.Count; i += MaxInstancesPerBatch)
                    {
                        var count = Math.Min(MaxInstancesPerBatch, visible.Count - i);
                        // ReSharper disable once AssignNullToNotNullAttribute (justification: arraysegment array is not null)
                        var batch = new ArraySegment<int>(visible.Array, visible.Offset + i, count);
                        DrawInstanced(cmd, batch, shaderPass);
                    }
                }
                else
                {
                    for (var i = 0; i < visible.Count; i++)
                    {
                        // ReSharper disable once PossibleNullReferenceException (Justification: Array segment array cannot be null)
                        var decal = Batch.Decals[visible.Array[visible.Offset + i]];
                        DrawSingle(cmd, decal, shaderPass);
                    }
                }

                return visible.Count;
            }

            private static void EnsureArrayCapacity<T>([NotNull] ref T[] array, int count, int maxGrowth = MaxInstancesPerBatch)
            {
                if (array == null)
                    array = new T[count];
                else if (array.Length < count)
                    array = new T[Math.Max(count, Math.Min(array.Length * 2, maxGrowth))];
            }

            private void DrawSingle([NotNull] CommandBuffer cmd, [NotNull] WetDecalSystem.DecalRenderInstance decal, int shaderPass)
            {
                if (cmd == null) throw new ArgumentNullException("cmd");
                if (decal == null) throw new ArgumentNullException("decal");

                if (decal.Decal.Settings.Shape == DecalShape.Mesh)
                {
                    if (decal.Decal.Mesh != null)
                    {
                        var mesh = decal.Decal.Mesh;
                        for (var i = 0; i < mesh.subMeshCount; i++)
                            cmd.DrawMesh(mesh, decal.Decal.WorldTransform, Batch.GetMaterial(false), i, shaderPass, decal.PropertyBlock);
                    }
                }
                else
                    cmd.DrawMesh(_box, decal.Decal.WorldTransform, Batch.GetMaterial(false), 0, shaderPass, decal.PropertyBlock);
            }

            private void DrawInstanced([NotNull] CommandBuffer cmd, ArraySegment<int> visible, int shaderPass)
            {
                if (visible.Count < 1)
                    return;

                EnsureArrayCapacity(ref _matrices, visible.Count);
                EnsureArrayCapacity(ref _saturation, visible.Count);
                EnsureArrayCapacity(ref _edgeFadeout, visible.Count);
                EnsureArrayCapacity(ref _edgeSharpness, visible.Count);

                if (Batch.Permutation.LayerMode == LayerMode.None)
                {
                    _yLayerInputStart = null;
                    _yLayerInputExtent = null;
                    _yLayerOutputStart = null;
                    _yLayerOutputEnd = null;
                }
                else
                {
                    EnsureArrayCapacity(ref _yLayerInputStart, visible.Count);
                    EnsureArrayCapacity(ref _yLayerInputExtent, visible.Count);
                    EnsureArrayCapacity(ref _yLayerOutputStart, visible.Count);
                    EnsureArrayCapacity(ref _yLayerOutputEnd, visible.Count);
                }

                if (Batch.Permutation.LayerMode != LayerMode.Triplanar)
                {
                    _xLayerInputStart = null;
                    _xLayerInputExtent = null;
                    _xLayerOutputStart = null;
                    _xLayerOutputEnd = null;
                    _zLayerInputStart = null;
                    _zLayerInputExtent = null;
                    _zLayerOutputStart = null;
                    _zLayerOutputEnd = null;
                }
                else
                {
                    EnsureArrayCapacity(ref _xLayerInputStart, visible.Count);
                    EnsureArrayCapacity(ref _xLayerInputExtent, visible.Count);
                    EnsureArrayCapacity(ref _xLayerOutputStart, visible.Count);
                    EnsureArrayCapacity(ref _xLayerOutputEnd, visible.Count);
                    EnsureArrayCapacity(ref _zLayerInputStart, visible.Count);
                    EnsureArrayCapacity(ref _zLayerInputExtent, visible.Count);
                    EnsureArrayCapacity(ref _zLayerOutputStart, visible.Count);
                    EnsureArrayCapacity(ref _zLayerOutputEnd, visible.Count);
                }

                // Copy decal data into arrays
                var arx = LayerArrays.Create(_xLayerInputStart, _xLayerInputExtent, _xLayerOutputStart, _xLayerOutputEnd);
                var ary = LayerArrays.Create(_yLayerInputStart, _yLayerInputExtent, _yLayerOutputStart, _yLayerOutputEnd);
                var arz = LayerArrays.Create(_zLayerInputStart, _zLayerInputExtent, _zLayerOutputStart, _zLayerOutputEnd);
                for (var i = 0; i < visible.Count; i++)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var decal = Batch.Decals[visible.Array[visible.Offset + i]];

                    if (decal.Decal.Settings.Shape == DecalShape.Mesh)
                    {
                        DrawSingle(cmd, decal, shaderPass);
                        _saturation[i] = 0;
                    }
                    else
                    {
                        decal.Properties.LoadInto(
                            i,
                            _saturation,
                            _edgeFadeout,
                            _edgeSharpness,
                            arx,
                            ary,
                            arz
                        );
                    }

                    _matrices[i] = decal.Decal.WorldTransform;

                }

                // Set arrays into instancing property block
                _instancingPropertyBlock.Clear();
                InstanceProperties.LoadInto(
                    _instancingPropertyBlock,
                    _saturation,
                    _edgeFadeout,
                    _edgeSharpness,
                    arx,
                    ary,
                    arz
                );

                var mat = Batch.GetMaterial(true);
                cmd.DrawMeshInstanced(_box, 0, mat, shaderPass, _matrices, visible.Count, _instancingPropertyBlock);
            }
        }

        #endregion

        private static readonly int MaskId = Shader.PropertyToID("_WetDecalSaturationMask");

        private static readonly Log Log = Logs.Create(LogCategory.Graphics, typeof(WetDecalRenderer).Name);
        private readonly List<DecalBatch> _batches;
        private readonly Mesh _box;
        private readonly Camera _camera;
        private readonly Dictionary<WetDecalSystem.MaterialBatch, DecalBatch> _registeredBatches;

        private readonly WetDecalSystem _system;

        private uint? _batchEpoch;

        public Camera Camera
        {
            get { return _camera; }
        }

        public WetDecalRenderer([NotNull] Camera camera)
        {
            Log.AssertAndThrowPossibleBug((bool) camera, "178CD4B2-1FD8-4BF2-B14D-EC9CA50436CA", "camera is null");

            _camera = camera;
            _system = new WetDecalSystem();
            _batches = new List<DecalBatch>();
            _registeredBatches = new Dictionary<WetDecalSystem.MaterialBatch, DecalBatch>();

            _box = Primitives.CreateBox(1, 1, 1);
            _box.hideFlags = HideFlags.DontSave;
        }

        public void Dispose()
        {
            Log.Trace("Dispose");

            for (var i = 0; i < _batches.Count; i++)
                _batches[i].Dispose();

            _batches.Clear();
            _registeredBatches.Clear();

            if (_box != null)
                Object.DestroyImmediate(_box);

            _system.Dispose();
        }

        public void Update()
        {
            // Update decal bounding spheres
            _system.Update();

            // Ensure we have a rendering batch for each material batch in the decal system
            if (_batchEpoch != _system.BatchEpoch)
            {
                for (var i = 0; i < _system.Batches.Count; i++)
                {
                    var materialBatch = _system.Batches[i];
                    if (!_registeredBatches.ContainsKey(materialBatch))
                    {
                        var batch = new DecalBatch(_camera, _box, materialBatch);
                        _batches.Add(batch);
                        _registeredBatches[materialBatch] = batch;
                    }
                }

                _batches.Sort();
                _batchEpoch = _system.BatchEpoch;
            }

            // Update each rendering batch
            for (var i = 0; i < _batches.Count; i++)
                _batches[i].Update();
        }

        public void RecordCommandBuffer([NotNull] CommandBuffer cmd)
        {
            if (XRSettings.enabled && Camera.stereoEnabled)
            {
                var desc = XRSettings.eyeTextureDesc;
                desc.colorFormat = RenderTextureFormat.RFloat;
                desc.sRGB = false;

                cmd.GetTemporaryRT(MaskId, desc, FilterMode.Point);
            }
            else
            {
                cmd.GetTemporaryRT(
                    MaskId,
                    -1, -1, 24,
                    FilterMode.Point,
                    RenderTextureFormat.RFloat,
                    RenderTextureReadWrite.Linear,
                    1
                );
            }
            
            cmd.SetRenderTarget(MaskId, BuiltinRenderTextureType.CameraTarget);
            cmd.ClearRenderTarget(false, true, new Color(0, 0, 0, 0));

            for (var i = 0; i < _batches.Count; i++)
                _batches[i].PrepareDraw(cmd);
        }
    }
}