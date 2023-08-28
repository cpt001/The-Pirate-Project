using System;
using PlaceholderSoftware.WetStuff.Debugging;
using PlaceholderSoftware.WetStuff.Timeline.Settings;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    [Serializable]
    public class DecalSettings : IDecalSettings
    {
        private static readonly Log Log = Logs.Create(LogCategory.Core, typeof(DecalSettings).Name);

        [SerializeField, UsedImplicitly, Tooltip("The distance from the edge of the shape from which saturation begins to fade."), Range(0, 1)]
        private float _edgeFadeoff;

        [SerializeField, UsedImplicitly, Tooltip("Jitter sample positions of detail layer textures")]
        private bool _enableJitter;

        [SerializeField, UsedImplicitly, Tooltip("How sharply the decal fades around faces facing in different directions"), Range(0.001f, 10)]
        private float _faceSharpness;

        [SerializeField, UsedImplicitly, Tooltip("The detail layer mode")]
        private LayerMode _layerMode;

        [SerializeField, UsedImplicitly, Tooltip("The layer projection mode")]
        private ProjectionMode _layerProjection;

        [SerializeField, UsedImplicitly, Tooltip("Determines if the decal projects wetness, or if it drys other wet decals")]
        private WetDecalMode _mode;

        [SerializeField, UsedImplicitly, Tooltip("Maximum jitter in texels to the detail layer sampling coordinates"), Range(0, 10)]
        private float _sampleJitter;

        [SerializeField, UsedImplicitly, Tooltip("How wet the decal appears to be"), Range(0, 1)]
        private float _saturation;

        [SerializeField, UsedImplicitly, Tooltip("The shape of the decal")]
        private DecalShape _shape;

        [SerializeField, UsedImplicitly, Tooltip("Per pixel saturation projected down the decal's x axis")]
        private DecalLayer _xLayer;

        [SerializeField, UsedImplicitly, Tooltip("Per pixel saturation projected down the decal's y axis")]
        private DecalLayer _yLayer;

        [SerializeField, UsedImplicitly, Tooltip("Per pixel saturation projected down the decal's z axis")]
        private DecalLayer _zLayer;

        public DecalSettings()
        {
            _xLayer = new DecalLayer();
            _yLayer = new DecalLayer();
            _zLayer = new DecalLayer();
        }

        /// <summary>
        ///     Determines if the decal projects wetness, or if it drys other wet decals.
        /// </summary>
        public WetDecalMode Mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                OnChanged(true);
            }
        }

        /// <summary>
        ///     Gets or sets the overall saturation of the decal.
        /// </summary>
        public float Saturation
        {
            get { return _saturation; }
            set
            {
                var changed = Math.Abs(_saturation - value) > float.Epsilon;
                _saturation = value;
                if (changed)
                    OnChanged(false);
            }
        }

        /// <summary>
        ///     Get or set max amount of pixels offset to sample the saturation mask with.
        /// </summary>
        public float SampleJitter
        {
            get { return _sampleJitter; }
            set
            {
                var changed = Math.Abs(value - _sampleJitter) > float.Epsilon;
                _sampleJitter = value;
                if (changed)
                    OnChanged(false);
            }
        }

        /// <summary>
        ///     Get or set whether dithered sampling of mask textures is enabled
        /// </summary>
        public bool EnableJitter
        {
            get { return _enableJitter; }
            set
            {
                _enableJitter = value;
                OnChanged(true);
            }
        }

        /// <summary>
        ///     Get or set which layers are enabled
        /// </summary>
        public LayerMode LayerMode
        {
            get { return _layerMode; }
            set
            {
                _layerMode = value;
                OnChanged(true);
            }
        }

        /// <summary>
        ///     Get or set how mask textures are projected
        /// </summary>
        public ProjectionMode LayerProjection
        {
            get { return _layerProjection; }
            set
            {
                _layerProjection = value;
                OnChanged(true);
            }
        }

        public float FaceSharpness
        {
            get { return _faceSharpness; }
            set
            {
                var changed = Math.Abs(value - _faceSharpness) > float.Epsilon;
                _faceSharpness = value;
                if (changed) OnChanged(false);
            }
        }

        public DecalLayer XLayer
        {
            get { return _xLayer; }
        }

        public DecalLayer YLayer
        {
            get { return _yLayer; }
        }

        public DecalLayer ZLayer
        {
            get { return _zLayer; }
        }

        public DecalShape Shape
        {
            get { return _shape; }
            set
            {
                _shape = value;
                OnChanged(true);
            }
        }

        public float EdgeFadeoff
        {
            get { return _edgeFadeoff; }
            set
            {
                _edgeFadeoff = value;
                OnChanged(false);
            }
        }

        public event Action<bool> Changed;

        public void Init()
        {
            Changed = null;

            InitLayer(_xLayer);
            InitLayer(_yLayer);
            InitLayer(_zLayer);
        }

        private void InitLayer([NotNull] DecalLayer layer)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            Log.AssertAndThrowPossibleBug(layer != null, "12987B54-4215-4C3C-AD16-A9AA23EB17A8", "Layer Is Null");

            layer.Init();
            layer.Changed += OnChanged;
        }

        public virtual void OnChanged(bool requiresRebuild)
        {
            var handler = Changed;
            if (handler != null) handler(requiresRebuild);
        }

        internal DecalSettingsDataContainer.DecalSettingsData Get()
        {
            return new DecalSettingsDataContainer.DecalSettingsData(Saturation, _xLayer.Get(), _yLayer.Get(), _zLayer.Get());
        }

        internal void Apply(DecalSettingsDataContainer.DecalSettingsData data)
        {
            Saturation = data.Saturation;

            _xLayer.Apply(data.XLayer);
            _yLayer.Apply(data.YLayer);
            _zLayer.Apply(data.ZLayer);
        }
    }

    /// <summary>
    ///     A decal which modfies the surfaces that lie within it to look wet.
    /// </summary>
    [ExecuteInEditMode, DisallowMultipleComponent, HelpURL("https://placeholder-software.co.uk/wetstuff/docs/Reference/WetDecal/")]
    public class WetDecal
        : MonoBehaviour, IWetDecal
    {
        #region Nested Types

        private enum UpdateType
        {
            None,
            Normal,
            Rebuild
        }

        #endregion

        private static readonly Log Log = Logs.Create(LogCategory.Core, typeof(WetDecal).Name);
        
        // fields for serialization
        [SerializeField, UsedImplicitly] private DecalSettings _settings;

        // actual runtime state
        private WetDecalSystem _system;
        private Transform _transform;
        private WetDecalSystem.DecalRenderHandle _render;
        private UpdateType _requiredUpdate;

        /// <summary>
        ///     Gets the decal's render settings.
        /// </summary>
        public DecalSettings Settings
        {
            get { return _settings; }
        }

        private Transform TransformCache
        {
            get
            {
                if (_transform == null)
                    _transform = transform;
                return _transform;
            }
        }

        public WetDecal()
        {
            _settings = new DecalSettings {
                Saturation = 0.5f,
                EdgeFadeoff = 0.1f,
                LayerProjection = ProjectionMode.Local,
                FaceSharpness = 1,
                LayerMode = LayerMode.None
            };
        }

        IDecalSettings IWetDecal.Settings
        {
            get { return _settings; }
        }

        BoundingSphere IWetDecal.Bounds
        {
            get { return new BoundingSphere(TransformCache.position, TransformCache.lossyScale.magnitude); }
        }

        Matrix4x4 IWetDecal.WorldTransform
        {
            get { return TransformCache.localToWorldMatrix; }
        }

        private MeshFilter _meshCache;
        Mesh IWetDecal.Mesh
        {
            get { return _meshCache.sharedMesh; }
        }

        public void Step(float dt)
        {
            if (_render.IsValid)
                _render.UpdateProperties(_requiredUpdate >= UpdateType.Rebuild);
            else
                Log.Debug("Stepped, but render handle was not valid");

            _requiredUpdate = UpdateType.None;
        }

        private void OnSettingsChanged(bool rebuild)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse (Justification: Unity overrides null check)
            if (this != null && isActiveAndEnabled)
                RequireUpdate(rebuild ? UpdateType.Rebuild : UpdateType.Normal);
        }

        protected virtual void OnValidate()
        {
            Log.Trace("OnValidate");

            if (_system != null)
                RequireUpdate(UpdateType.Rebuild);
        }

        protected virtual void OnEnable()
        {
            Log.Trace("OnEnable");

            _settings.Init();
            _settings.Changed += OnSettingsChanged;

            _meshCache = GetComponent<MeshFilter>();

            if (_system == null)
                _system = new WetDecalSystem();
            _render = _system.Add(this);

            RequireUpdate(UpdateType.Rebuild, true);
        }

        protected virtual void OnDisable()
        {
            Log.Trace("OnDisable");

            if (_render.IsValid)
                _render.Dispose();
        }

        protected virtual void OnDestroy()
        {
            Log.Trace("OnDestroy");

            if (_system != null)
                _system.Dispose();
        }

        private void RequireUpdate(UpdateType type, bool immediate = false)
        {
            var current = _requiredUpdate;
            if (_requiredUpdate < type) _requiredUpdate = type;

            if (immediate)
                Step(0);
            else if (current == UpdateType.None)
                _system.QueueForUpdate(this);
        }

        #region Gizmos

        private void DrawGizmo(bool selected)
        {
            var col = new Color(0.0f, 0.7f, 1f, 1.0f);
            Gizmos.matrix = transform.localToWorldMatrix;

            // draw the faces
            if (Settings.Shape == DecalShape.Cube || Settings.Shape == DecalShape.Mesh)
            {
                col.a = selected ? 0.3f : 0.1f;
                col.a *= isActiveAndEnabled ? 0.15f : 0.1f;
                Gizmos.color = col;
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
            }
            else if (Settings.Shape == DecalShape.Sphere)
            {
                Gizmos.color = new Color(1, 1, 1, 0);
                Gizmos.DrawSphere(Vector3.zero, 0.5f);
            }
            else
                Log.Error("Unknown decal shape: '{0}'", Settings.Shape);

            // draw the edges
            col.a = selected ? 0.5f : 0.2f;
            col.a *= isActiveAndEnabled ? 1 : 0.75f;
            Gizmos.color = col;

            if (Settings.Shape == DecalShape.Cube || Settings.Shape == DecalShape.Mesh)
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            else if (Settings.Shape == DecalShape.Sphere)
                Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
        }

        protected virtual void OnDrawGizmos()
        {
            DrawGizmo(false);
        }

        protected virtual void OnDrawGizmosSelected()
        {
            DrawGizmo(true);
        }

        #endregion
    }
}