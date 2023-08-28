using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PlaceholderSoftware.WetStuff.Datastructures;
using PlaceholderSoftware.WetStuff.Rendering;
using UnityEngine;
using Object = UnityEngine.Object;
using RenderSettings = PlaceholderSoftware.WetStuff.Rendering.RenderSettings;

namespace PlaceholderSoftware.WetStuff
{
    /// <summary>
    ///     Keeps track of all WetDecal instances active in the scene.
    /// </summary>
    internal class WetDecalSystem
        : RcSharedState<WetDecalSystem.State>
    {
        #region Nested Types

        /// <summary>
        ///     A handle to a single decal being rendered
        /// </summary>
        public struct DecalRenderHandle
            : IDisposable
        {
            private readonly uint _id;
            private readonly DecalRenderInstance _instance;

            public bool IsValid
            {
                get { return _instance != null && _id == _instance.Epoch; }
            }

            internal DecalRenderHandle([NotNull] DecalRenderInstance instance)
            {
                _instance = instance;
                _id = instance.Epoch;
            }

            public void UpdateProperties(bool batchPropertiesChanged)
            {
                if (IsValid)
                    _instance.UpdateProperties(batchPropertiesChanged);
                else
                    throw new ObjectDisposedException("This handle has already been disposed");
            }

            /// <summary>
            ///     Dispose this handle to remove the decal from the scene
            /// </summary>
            public void Dispose()
            {
                if (IsValid)
                    _instance.Dispose();
                else
                    throw new ObjectDisposedException("This handle has already been disposed");
            }
        }

        internal sealed class DecalRenderInstance
            : IDisposable
        {
            private static readonly Pool<DecalRenderInstance> Pool = new Pool<DecalRenderInstance>(256, () => new DecalRenderInstance());
            private MaterialBatch _batch;
            private InstanceProperties _properties;

            private WetDecalSystem _system;

            internal uint Epoch { get; private set; }

            public IWetDecal Decal { get; private set; }
            public MaterialPropertyBlock PropertyBlock { get; private set; }

            public InstanceProperties Properties
            {
                get { return _properties; }
            }

            public void Dispose()
            {
                if (_batch != null)
                {
                    _batch.Remove(this);
                    _batch = null;
                }

                Epoch++;
                if (Epoch != uint.MaxValue)
                    Pool.Put(this);
            }

            [NotNull]
            internal static DecalRenderInstance Create([NotNull] IWetDecal decal, [NotNull] WetDecalSystem system)
            {
                var registration = Pool.Get();
                registration._system = system;
                registration.Initialize(decal);

                return registration;
            }

            private void Initialize(IWetDecal decal)
            {
                Decal = decal;

                if (PropertyBlock != null)
                    PropertyBlock.Clear();

                UpdateProperties(true);
            }

            private static Vector2? Jitter(float amount, [CanBeNull] Texture texture)
            {
                if (texture == null)
                    return null;

                var r = new Vector2(amount / texture.width, amount / texture.height);
                return r;
            }

            private static Vector2 Min(Vector2? a, Vector2? b, Vector2? c)
            {
                var min = a ?? b ?? c ?? Vector2.zero;

                if (a.HasValue) min = Vector2.Min(min, a.Value);
                if (b.HasValue) min = Vector2.Min(min, b.Value);
                if (c.HasValue) min = Vector2.Min(min, c.Value);

                return min;
            }

            public void UpdateProperties(bool batchPropertiesChanged)
            {
                var settings = Decal.Settings;

                // Move decal to appropriate batch if needed
                if (batchPropertiesChanged)
                {
                    // Find shader permutation
                    var permuatation = new MaterialPermutation(settings.Mode, settings.LayerMode, settings.LayerProjection, settings.Shape, settings.EnableJitter);

                    // Choose the smallest jitter value of the three textures (or zero if they are all null)
                    var jitter = Min(
                        settings.LayerMode == LayerMode.Triplanar ? Jitter(settings.SampleJitter, settings.XLayer.LayerMask) : null,
                        settings.LayerMode != LayerMode.None ? Jitter(settings.SampleJitter, settings.YLayer.LayerMask) : null,
                        settings.LayerMode == LayerMode.Triplanar ? Jitter(settings.SampleJitter, settings.ZLayer.LayerMask) : null
                    );

                    // Collect material properties
                    var materialProperties = new MaterialProperties(
                        settings.XLayer.LayerMask,
                        settings.XLayer.LayerMaskScaleOffset,
                        settings.YLayer.LayerMask,
                        settings.YLayer.LayerMaskScaleOffset,
                        settings.ZLayer.LayerMask,
                        settings.ZLayer.LayerMaskScaleOffset,
                        _system.SharedState.BlueNoiseRGBA,
                        new Vector2(29, 31),
                        jitter
                    );

                    // Update batch
                    if (_batch != null) _batch.Remove(this);

                    _batch = _system.SharedState.FindBatch(permuatation, materialProperties);
                    _batch.Add(this);
                }

                // Update instance properties struct                
                _properties.Saturation = settings.Saturation;
                _properties.Fadeout = settings.EdgeFadeoff;
                _properties.EdgeSharpness = settings.FaceSharpness;

                if (settings.LayerMode != LayerMode.None)
                    settings.YLayer.EvaluateRanges(out _properties.YLayer);

                if (settings.LayerMode == LayerMode.Triplanar)
                {
                    settings.XLayer.EvaluateRanges(out _properties.XLayer);
                    settings.ZLayer.EvaluateRanges(out _properties.ZLayer);
                }

                // Update material properties block if running non-instanced
                if (!RenderSettings.Instance.EnableInstancing)
                {
                    if (PropertyBlock == null) PropertyBlock = new MaterialPropertyBlock();

                    PropertyBlock.Clear();
                    _properties.LoadInto(PropertyBlock, settings.LayerMode);
                }
            }
        }

        private struct MaterialBatchId
            : IEquatable<MaterialBatchId>
        {
            private readonly MaterialPermutation _permutation;
            private readonly MaterialProperties _properties;

            public MaterialBatchId(MaterialPermutation permutation, MaterialProperties properties)
            {
                _permutation = permutation;
                _properties = properties;
            }

            #region Equality

            public bool Equals(MaterialBatchId other)
            {
                return _permutation.Equals(other._permutation) && _properties.Equals(other._properties);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;

                return obj is MaterialBatchId && Equals((MaterialBatchId) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_permutation.GetHashCode() * 397) ^ _properties.GetHashCode();
                }
            }

            public static bool operator ==(MaterialBatchId left, MaterialBatchId right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(MaterialBatchId left, MaterialBatchId right)
            {
                return !left.Equals(right);
            }

            #endregion
        }

        internal class MaterialBatch : IDisposable
        {
            private readonly List<DecalRenderInstance> _decals;
            private readonly Material _material;
            public BoundingSphere[] BoundingSpheres { get; private set; }
            public ReadOnlyCollection<DecalRenderInstance> Decals { get; private set; }
            public MaterialProperties Properties { get; private set; }
            public MaterialPermutation Permutation { get; private set; }

            public MaterialBatch(Shader shader, MaterialPermutation permutation, MaterialProperties properties)
            {
                Properties = properties;
                Permutation = permutation;
                _decals = new List<DecalRenderInstance>();
                BoundingSpheres = new BoundingSphere[16];
                Decals = new ReadOnlyCollection<DecalRenderInstance>(_decals);

                var material = new Material(shader) {
                    hideFlags = HideFlags.DontSave
                };

                if (permutation.LayerMode == LayerMode.Single)
                    material.EnableKeyword("LAYERS_SINGLE");
                else if (permutation.LayerMode == LayerMode.Triplanar)
                    material.EnableKeyword("LAYERS_TRIPLANAR");

                if (permutation.LayerProjectionMode == ProjectionMode.Local)
                    material.EnableKeyword("LAYER_PROJECTION_LOCAL");
                else
                    material.EnableKeyword("LAYER_PROJECTION_WORLD");

                if (permutation.EnableJitter)
                    material.EnableKeyword("JITTER_LAYERS");

                if (permutation.Shape == DecalShape.Sphere)
                    material.EnableKeyword("SHAPE_CIRCLE");
                else if (permutation.Shape == DecalShape.Cube)
                    material.EnableKeyword("SHAPE_SQUARE");
                else if (permutation.Shape == DecalShape.Mesh)
                    material.EnableKeyword("SHAPE_MESH");
                else
                    throw new InvalidOperationException("Unknown decal shape `" + permutation.Shape + "`");

                properties.LoadInto(material);

                _material = material;
            }

            public void Dispose()
            {
                for (var i = _decals.Count - 1; i >= 0; i--) _decals[i].Dispose();

                Object.DestroyImmediate(_material);
            }

            [NotNull]
            public Material GetMaterial(bool instanced)
            {
                if (_material.enableInstancing != instanced)
                    _material.enableInstancing = instanced;

                return _material;
            }

            internal void Add(DecalRenderInstance decal)
            {
                _decals.Add(decal);
            }

            internal void Remove(DecalRenderInstance decal)
            {
                _decals.Remove(decal);
            }

            public void Update()
            {
                // Make sure we have enough space in the culling array
                if (BoundingSpheres.Length < _decals.Count)
                {
                    var length = BoundingSpheres.Length;
                    while (length < _decals.Count) length *= 2;

                    BoundingSpheres = new BoundingSphere[length];
                }

                // Update bounding spheres for culling
                for (var i = 0; i < _decals.Count; i++) BoundingSpheres[i] = _decals[i].Decal.Bounds;
            }
        }

        internal class State : IDisposable
        {
            private readonly Dictionary<MaterialBatchId, MaterialBatch> _batches;
            private readonly List<MaterialBatch> _batchesList;
            private readonly Shader _dryMaskShader;
            private readonly Shader _wetMaskShader;

            public uint BatchEpoch { get; private set; }
            public Texture2D BlueNoiseRGBA { get; private set; }
            public ReadOnlyCollection<MaterialBatch> Batches { get; private set; }
            public int LastUpdated { get; set; }
            public List<IWetDecal> ToUpdate { get; set; }
            public List<IWetDecal> Updating { get; set; }

            public State()
            {
                _batches = new Dictionary<MaterialBatchId, MaterialBatch>();
                _batchesList = new List<MaterialBatch>();
                _wetMaskShader = Shader.Find("WetStuff/WetSurfaceMask");
                _dryMaskShader = Shader.Find("WetStuff/DrySurfaceMask");
                BlueNoiseRGBA = (Texture2D) Resources.Load("FreeBlueNoiseTextures/128_128/LDR_RGBA_2");
                Batches = new ReadOnlyCollection<MaterialBatch>(_batchesList);
                LastUpdated = 0;
                ToUpdate = new List<IWetDecal>();
                Updating = new List<IWetDecal>();
            }

            public void Dispose()
            {
                foreach (var batch in _batchesList)
                    batch.Dispose();

                _batches.Clear();
                _batchesList.Clear();
            }

            public MaterialBatch FindBatch(MaterialPermutation permutation, MaterialProperties properties)
            {
                var id = new MaterialBatchId(permutation, properties);

                MaterialBatch batch;
                if (!_batches.TryGetValue(id, out batch))
                {
                    batch = new MaterialBatch(permutation.SelectShader(_wetMaskShader, _dryMaskShader), permutation, properties);
                    _batches[id] = batch;
                    _batchesList.Add(batch);

                    unchecked
                    {
                        BatchEpoch++;
                    }
                }

                return batch;
            }
        }

        #endregion

        internal ReadOnlyCollection<MaterialBatch> Batches
        {
            get { return SharedState.Batches; }
        }

        internal uint BatchEpoch
        {
            get { return SharedState.BatchEpoch; }
        }

        public WetDecalSystem()
            : base(() => new State())
        {
        }

        public DecalRenderHandle Add([NotNull] IWetDecal decal)
        {
            var instance = DecalRenderInstance.Create(decal, this);
            return new DecalRenderHandle(instance);
        }

        public void QueueForUpdate(IWetDecal decal)
        {
            SharedState.ToUpdate.Add(decal);
        }

        public void Update()
        {
            var state = SharedState;

            // We only want to update once per frame, even if there are multiple system instances active
            if (state.LastUpdated == Time.frameCount)
                return;

            // Update decals
            var tmp = state.Updating;
            state.Updating = state.ToUpdate;
            state.ToUpdate = tmp;

            var dt = Application.isPlaying ? Time.deltaTime : 0;
            for (var i = 0; i < state.Updating.Count; i++)
                state.Updating[i].Step(dt);

            state.Updating.Clear();

            // Update batches
            for (var i = 0; i < state.Batches.Count; i++)
                state.Batches[i].Update();

            state.LastUpdated = Time.frameCount;
        }
    }
}