using System;
using System.Collections.Generic;
using PlaceholderSoftware.WetStuff.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PlaceholderSoftware.WetStuff
{
    [RequireComponent(typeof(ParticleSystem)), HelpURL("https://placeholder-software.co.uk/wetstuff/docs/Reference/ParticleWetSplatter/")]
    public class ParticleWetSplatter : MonoBehaviour
    {
        #region Nested Types

        private class DecalSettingsSaturationProxy
            : IDecalSettings
        {
            private readonly IDecalSettings _settings;

            public float SaturationMultiplier { get; set; }

            public DecalSettingsSaturationProxy(IDecalSettings settings)
            {
                _settings = settings;
            }

            public WetDecalMode Mode
            {
                get { return _settings.Mode; }
            }

            public float Saturation
            {
                get { return _settings.Saturation * SaturationMultiplier; }
            }

            public float SampleJitter
            {
                get { return _settings.SampleJitter; }
            }

            public bool EnableJitter
            {
                get { return _settings.EnableJitter; }
            }

            public float FaceSharpness
            {
                get { return _settings.FaceSharpness; }
            }

            public ProjectionMode LayerProjection
            {
                get { return _settings.LayerProjection; }
            }

            public LayerMode LayerMode
            {
                get { return _settings.LayerMode; }
            }

            public DecalLayer XLayer
            {
                get { return _settings.XLayer; }
            }

            public DecalLayer YLayer
            {
                get { return _settings.YLayer; }
            }

            public DecalLayer ZLayer
            {
                get { return _settings.ZLayer; }
            }

            public DecalShape Shape
            {
                get { return _settings.Shape; }
            }

            public float EdgeFadeoff
            {
                get { return _settings.EdgeFadeoff; }
            }
        }

        private class Splatter
            : IWetDecal
        {
            private readonly DecalSettingsSaturationProxy _settings;
            private readonly ParticleWetSplatter _splatters;
            private Matrix4x4 _localTransform;

            private Transform _parent;
            private float _remainingLifetime;
            private WetDecalSystem.DecalRenderHandle _render;
            private int _settingsDirty;

            private float _totalLifetime;

            public DecalSettingsSaturationProxy Settings
            {
                get { return _settings; }
            }

            public Mesh Mesh
            {
                get { return null; }
            }

            public float AgeingRate { get; set; }

            public bool IsActive
            {
                get { return _remainingLifetime > 0; }
            }

            public Splatter([NotNull] ParticleWetSplatter splatters, [NotNull] DecalSettings settings)
            {
                _splatters = splatters;

                _settings = new DecalSettingsSaturationProxy(settings);
                settings.Changed += DecalSettingsChanged;
            }

            public Matrix4x4 WorldTransform
            {
                get { return _parent.localToWorldMatrix * _localTransform; }
            }

            public BoundingSphere Bounds
            {
                get
                {
                    var transform = WorldTransform;
                    var translation = transform.GetColumn(3);
                    var scale = new Vector3(transform.m00, transform.m11, transform.m22);
                    return new BoundingSphere(new Vector3(translation.x, translation.y, translation.z), scale.magnitude);
                }
            }

            IDecalSettings IWetDecal.Settings
            {
                get { return _settings; }
            }

            public void Step(float dt)
            {
                // Calculate saturation (modulated by change over lifetime if that's enabled)
                var saturation = _splatters.Core.Saturation;
                if (_splatters.Lifetime.Enabled)
                {
                    _remainingLifetime -= dt * AgeingRate;
                    saturation *= _splatters.Lifetime.Saturation.Evaluate((_totalLifetime - _remainingLifetime) / _totalLifetime);
                }

                if (_settingsDirty > 0 || Math.Abs(_settings.SaturationMultiplier - saturation) > float.Epsilon)
                {
                    _settings.SaturationMultiplier = saturation;
                    _render.UpdateProperties(_settingsDirty > 1);
                    _settingsDirty = 0;
                }
            }

            private void DecalSettingsChanged(bool rebuild)
            {
                //if (_render != null) {
                //    _render.UpdateProperties(rebuild);
                //}
                //else {
                _settingsDirty = rebuild ? 2 : 1;
                //}
            }

            public void Reset()
            {
                _totalLifetime = 0;
                _remainingLifetime = 0;
                AgeingRate = 1;

                if (_render.IsValid)
                    _render.Dispose();
            }

            public void Initialize(Vector3 position, Vector3 normal, Vector3 velocity, [NotNull] Transform parent)
            {
                var size = _splatters.Core.DecalSize;
                if (_splatters.RandomizeSize.Enabled)
                    size *= Random.Range(_splatters.RandomizeSize.MinInflation, _splatters.RandomizeSize.MaxInflation);

                var rotationAngle = Mathf.Acos(Vector3.Dot(Vector3.up, normal)) * Mathf.Rad2Deg;
                var rotationAxis = Vector3.Cross(Vector3.up, normal).normalized;
                var orientation = Quaternion.AngleAxis(rotationAngle, rotationAxis);

                position += normal * _splatters.Core.VerticalOffset;

                // Modify the transform according to the particle impact velocity
                if (_splatters.ImpactVelocity.Enabled)
                {
                    // Project impact velocity onto plane defined by collision normal
                    var velocityOnPlane = velocity - Vector3.Dot(velocity, normal) * normal;
                    var speedOnPlane = velocityOnPlane.magnitude;
                    var dirOnPlane = velocityOnPlane / speedOnPlane;

                    // Align with impact velocity (randomised orientation should be equally spread around this axis)
                    var impactOrientation = Quaternion.LookRotation(dirOnPlane, Vector3.up);
                    orientation *= impactOrientation;

                    // Stretch along impact direction according to impact speed
                    var stretch = _splatters.ImpactVelocity.Scale.Evaluate(speedOnPlane);
                    size.z *= stretch;

                    // Offset position according to impact speed and length
                    var offset = _splatters.ImpactVelocity.Offset.Evaluate(speedOnPlane);
                    position += dirOnPlane * offset * size.z;
                }

                // Randomize the orientation. Rotating a max of half the range ccw, and half the range cw (i.e. distributing the rotation around the current facing direction)
                if (_splatters.RandomizeOrientation.Enabled)
                    orientation *= Quaternion.AngleAxis((Random.value - 0.5f) * _splatters.RandomizeOrientation.RandomDegrees, Vector3.up);

                var worldTransform = Matrix4x4.TRS(position, orientation, size);

                _parent = parent;
                _localTransform = parent.worldToLocalMatrix * worldTransform;

                _totalLifetime = _splatters.Lifetime.Enabled
                    ? Random.Range(_splatters.Lifetime.MinLifetime, _splatters.Lifetime.MaxLifetime)
                    : 1;
                _remainingLifetime = _totalLifetime;
                AgeingRate = 1;

                _render = _splatters._decalSystem.Add(this);
            }
        }

        [Serializable]
        public class BaseToggleableSettings
        {
            [SerializeField] private bool _enabled;

            public bool Enabled
            {
                get { return _enabled; }
                set { _enabled = value; }
            }
        }

        [Serializable]
        public class CoreSettings
        {
            [Tooltip("Chance of a decal being created when a particle impact occurs"), SerializeField]
            private float _decalChance;

            [Tooltip("Base size of the decals"), SerializeField]
            private Vector3 _decalSize = new Vector3(1f, 0.2f, 1f);

            [Tooltip("How wet the decals appear to be"), SerializeField]
            private float _saturation;

            [Tooltip("Decal position offset from the particle impact point"), SerializeField]
            private float _verticalOffset;

            #region accessors

            public Vector3 DecalSize
            {
                get { return _decalSize; }
                set { _decalSize = value; }
            }

            public float VerticalOffset
            {
                get { return _verticalOffset; }
                set { _verticalOffset = value; }
            }

            public float DecalChance
            {
                get { return _decalChance; }
                set { _decalChance = value; }
            }

            public float Saturation
            {
                get { return _saturation; }
                set { _saturation = value; }
            }

            #endregion
        }

        [Serializable]
        public class LimitSettings
            : BaseToggleableSettings
        {
            [Tooltip("Change the chance of a decal being created as more decals are visible"), SerializeField]
            private AnimationCurve _decalChance = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.75f, 0.5f), new Keyframe(1, 0));

            [Tooltip("Maximum number of decals which may be visible at once"), SerializeField]
            private int _maxDecals = 10;

            #region accessors

            public int MaxDecals
            {
                get { return _maxDecals; }
                set { _maxDecals = value; }
            }

            public AnimationCurve DecalChance
            {
                get { return _decalChance; }
                set { _decalChance = value; }
            }

            #endregion
        }

        [Serializable]
        public class LifetimeSettings
            : BaseToggleableSettings
        {
            [Tooltip("Maximum lifetime of decals"), SerializeField]
            private float _maxLifetime = 60;

            [Tooltip("Minimum lifetime of decals"), SerializeField]
            private float _minLifetime = 30;

            [Tooltip("Change in saturation over the decal lifetime"), SerializeField]
            private AnimationCurve _saturation = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 0.9f), new Keyframe(1, 0));

            #region accessors

            public float MinLifetime
            {
                get { return _minLifetime; }
                set { _minLifetime = value; }
            }

            public float MaxLifetime
            {
                get { return _maxLifetime; }
                set { _maxLifetime = value; }
            }

            public AnimationCurve Saturation
            {
                get { return _saturation; }
                set { _saturation = value; }
            }

            #endregion
        }

        [Serializable]
        public class ImpactVelocitySettings
            : BaseToggleableSettings
        {
            [Tooltip("Position offset along particle impact direction based on the impact velocity"), SerializeField]
            private AnimationCurve _offset = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0, 0.25f));

            [Tooltip("Horizontal scale to apply to decals based on the impact velocity"), SerializeField]
            private AnimationCurve _scale = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1.5f));

            #region accessors

            public AnimationCurve Offset
            {
                get { return _offset; }
                set { _offset = value; }
            }

            public AnimationCurve Scale
            {
                get { return _scale; }
                set { _scale = value; }
            }

            #endregion
        }

        [Serializable]
        public class RandomizeSizeSettings
            : BaseToggleableSettings
        {
            [Tooltip("Maximum amount to multiply with base scale"), SerializeField]
            private float _maxInflation = 1.25f;

            [Tooltip("Minimum amount to multiply with base scale"), SerializeField]
            private float _minInflation = 0.75f;

            public float MinInflation
            {
                get { return _minInflation; }
                set { _minInflation = value; }
            }

            public float MaxInflation
            {
                get { return _maxInflation; }
                set { _maxInflation = value; }
            }
        }

        [Serializable]
        public class RandomizeOrientationSettings
            : BaseToggleableSettings
        {
            [Tooltip("Maximum number of degrees to randomly rotate decal"), SerializeField]
            private float _randomDegrees = 180;

            public float RandomDegrees
            {
                get { return _randomDegrees; }
                set { _randomDegrees = value; }
            }
        }

        [Serializable]
        public class RecyclingSettings
            : BaseToggleableSettings
        {
            [Tooltip("How much to artifically accelerate the ageing rate of a random decal when a decal is not created due to the decal limit"), SerializeField]
            private float _maxAcceleratedAgeing;

            [Tooltip("Threshold below which an active decal may be stolen to create new decals when needed"), SerializeField]
            private float _stealThreshold;

            public float MaxAcceleratedAgeing
            {
                get { return _maxAcceleratedAgeing; }
                set { _maxAcceleratedAgeing = value; }
            }

            public float StealThreshold
            {
                get { return _stealThreshold; }
                set { _stealThreshold = value; }
            }
        }

        #endregion

        [ItemCanBeNull] private readonly List<Splatter> _activeSplatters;
        private readonly List<ParticleCollisionEvent> _collisionEvents;
        private readonly List<Splatter> _inactiveSplatters;
        private WetDecalSystem _decalSystem;
        private DateTime _lastCleanedNullsUtc = DateTime.MinValue;
        private int _nonPlacedByChance;
        private ParticleSystem _particleSystem;
        private int _totalDecalCount;
        
        // ReSharper disable FieldCanBeMadeReadOnly.Local (cannot be readonly: confuses unity serialization)

        [SerializeField] private CoreSettings _core = new CoreSettings();
        [SerializeField] private LimitSettings _limit = new LimitSettings();
        [SerializeField] private ImpactVelocitySettings _impactVelocity = new ImpactVelocitySettings();
        [SerializeField] private RandomizeSizeSettings _randomizeSize = new RandomizeSizeSettings();
        [SerializeField] private RandomizeOrientationSettings _randomizeOrientation = new RandomizeOrientationSettings();
        [SerializeField] private RecyclingSettings _recycling = new RecyclingSettings();
        [SerializeField] private LifetimeSettings _lifetime = new LifetimeSettings();

        [NonSerialized] private float _templatesWeightSum;
        [NonSerialized] private ParticleWetSplatterTemplate[] _templates;

        // ReSharper restore FieldCanBeMadeReadOnly.Local

        public CoreSettings Core
        {
            get { return _core; }
        }

        public LimitSettings Limit
        {
            get { return _limit; }
        }

        public ImpactVelocitySettings ImpactVelocity
        {
            get { return _impactVelocity; }
        }

        public RandomizeSizeSettings RandomizeSize
        {
            get { return _randomizeSize; }
        }

        public RandomizeOrientationSettings RandomizeOrientation
        {
            get { return _randomizeOrientation; }
        }

        public RecyclingSettings Recycling
        {
            get { return _recycling; }
        }

        public LifetimeSettings Lifetime
        {
            get { return _lifetime; }
        }

        public ParticleWetSplatter()
        {
            _collisionEvents = new List<ParticleCollisionEvent>();

            _activeSplatters = new List<Splatter>();
            _inactiveSplatters = new List<Splatter>();
        }

        protected virtual void Start()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        protected virtual void OnEnable()
        {
            if (_decalSystem == null)
                _decalSystem = new WetDecalSystem();

            _templates = GetComponents<ParticleWetSplatterTemplate>();
            _templatesWeightSum = 0;
            for (var i = 0; i < _templates.Length; i++)
                _templatesWeightSum += _templates[i].Probability;
        }

        protected virtual void OnDisable()
        {
            // Clean up all the individual splatters
            for (var i = 0; i < _activeSplatters.Count; i++)
            {
                var splatter = _activeSplatters[i];
                if (splatter != null)
                    splatter.Reset();
            }

            // Move them all to inactive
            _inactiveSplatters.AddRange(_activeSplatters);
            _activeSplatters.Clear();
        }

        protected virtual void OnDestroy()
        {
            _decalSystem.Dispose();
        }

        protected virtual void Update()
        {
            // Create splatter decals for each collision event
            for (var i = 0; i < _collisionEvents.Count; i++)
            {
                // Calculate the chance to create a decal given the number of decals already created and how close that brings us to the decal limit
                var chance = Core.DecalChance;
                if (Limit.Enabled)
                {
                    chance *= Limit.DecalChance.Evaluate(((float)_activeSplatters.Count) / Limit.MaxDecals);

                    // Increase the chance of a decal every time a decal is not placed
                    chance *= Math.Min(2, Mathf.Pow(1.1f, _nonPlacedByChance));
                }

                // Skip ahead if we lose the lottery
                if (Random.value > chance)
                {
                    _nonPlacedByChance++;
                    continue;
                }

                // Sometimes we get null events from Unity, skip this one if it has null data
                var collision = _collisionEvents[i];
                if (collision.colliderComponent == null)
                    continue;

                // Try to create a decal for this collision event, this may fail if we've hit the limit and can't recycle anything
                var splatter = GetOrCreateSplatter();
                if (splatter != null)
                {
                    // Reset the count of how much we've lost the lottery recently
                    _nonPlacedByChance = 0;

                    splatter.Initialize(collision.intersection, collision.normal, collision.velocity, collision.colliderComponent.transform);

                    _activeSplatters.Add(splatter);
                }
                else
                {
                    // We failed to create a decal, if necessary accelerate the ageing of another decal
                    if (_activeSplatters.Count > 0 && Recycling.Enabled && Recycling.MaxAcceleratedAgeing > 0)
                    {
                        var index = Random.Range(0, _activeSplatters.Count);
                        var splat = _activeSplatters[index];

                        if (splat != null)
                            splat.AgeingRate = 1 + Math.Max(splat.AgeingRate - 1, Recycling.MaxAcceleratedAgeing * Random.value);
                    }
                }
            }

            _collisionEvents.Clear();

            // Update active splatters
            var nulls = 0;
            for (var i = _activeSplatters.Count - 1; i >= 0; i--)
            {
                var splatter = _activeSplatters[i];
                if (splatter == null)
                {
                    nulls++;
                    continue;
                }

                if (splatter.IsActive)
                    splatter.Step(Time.deltaTime);
                else
                {
                    splatter.Reset();
                    _inactiveSplatters.Add(splatter);

                    nulls++;
                    _activeSplatters[i] = null;
                }
            }

            // Once some null items have accumulated remove them all at once
            // Use a significantly smaller threshold if recycling of decals is enabled
            if (nulls > 0)
            {
                var nullLimit = 25;
                if (Limit.Enabled && Recycling.Enabled)
                    nullLimit = Math.Min(nullLimit, Limit.MaxDecals / 16);
                if (nulls > nullLimit || DateTime.UtcNow - _lastCleanedNullsUtc > TimeSpan.FromSeconds(5))
                {
                    _activeSplatters.RemoveNulls(nulls);
                    _lastCleanedNullsUtc = DateTime.UtcNow;
                }
            }
        }

        public void Clear()
        {
            // Reset counters
            _nonPlacedByChance = 0;
            _totalDecalCount = 0;

            // Delete active decals
            for (var i = _activeSplatters.Count - 1; i >= 0; i--)
            {
                var splatter = _activeSplatters[i];
                if (splatter != null)
                    splatter.Reset();
            }

            _activeSplatters.Clear();

            // Delete inactive decals
            _inactiveSplatters.Clear();
        }

        protected virtual void OnParticleCollision(GameObject other)
        {
            _particleSystem.GetCollisionEvents(other, _collisionEvents);
        }

        [CanBeNull]
        private DecalSettings ChooseSettings()
        {
            if (_templates.Length == 0)
                return null;

            var sum = 0f;
            var r = Random.value * _templatesWeightSum;
            for (var i = 0; i < _templates.Length - 1; i++)
            {
                var t = _templates[i];

                if (r + sum < t.Probability)
                    return t.Settings;
                sum += t.Probability;
            }

            return _templates[_templates.Length - 1].Settings;
        }

        [CanBeNull]
        private Splatter GetOrCreateSplatter()
        {
            // Try to recycle an inactive decal if one is available
            if (_inactiveSplatters.Count > 0)
            {
                var last = _inactiveSplatters.Count - 1;
                var splatter = _inactiveSplatters[last];
                _inactiveSplatters.RemoveAt(last);
                return splatter;
            }

            // If limit is enabled and we've hit it we can either steal one (if enabled) or give up (if not)
            if (_limit.Enabled && _totalDecalCount >= _limit.MaxDecals)
            {
                // Early exit if stealing is not enabled
                if (!_recycling.Enabled || _recycling.StealThreshold <= 0)
                    return null;

                // Find the first decal which we can steal. We'll assume the oldest decals are at the start so only search a maximum of 25 items
                for (var i = 0; i < Math.Min(25, _activeSplatters.Count); i++)
                {
                    var splat = _activeSplatters[i];
                    if (splat == null)
                        continue;

                    var error = splat.Bounds.radius * splat.Settings.Saturation;
                    if (error < _recycling.StealThreshold)
                    {
                        // We've found a decal which comes in below the threshold, immediately kill it
                        splat.Reset();
                        _activeSplatters.RemoveAt(i);

                        return splat;
                    }
                }

                // Didn't find anything, give up :(
                return null;
            }

            // Choose settings for a new decal
            var settings = ChooseSettings();
            if (settings == null)
                return null;

            // Create a new decal
            _totalDecalCount++;
            return new Splatter(this, settings);
        }

        private void DrawGizmo(bool selected)
        {
            ////Draw impact points
            //for (var i = 0; i < _collisionEvents.Count; i++)
            //{
            //    var e = _collisionEvents[i];

            //    Gizmos.matrix = Matrix4x4.identity;

            //    Gizmos.color = Color.gray;
            //    Gizmos.DrawSphere(e.intersection, 0.05f);

            //    Gizmos.color = Color.red;
            //    Gizmos.DrawLine(e.intersection, e.intersection + e.normal * 0.5f);

            //    Gizmos.color = Color.green;
            //    Gizmos.DrawLine(e.intersection, e.intersection + e.velocity);

            //    var norm = e.normal.normalized;
            //    var proj = e.velocity - Vector3.Dot(e.velocity, norm) * norm;
            //    Gizmos.color = Color.blue;
            //    Gizmos.DrawLine(e.intersection, e.intersection + proj);
            //}

            // Draw splatters
            for (var i = 0; i < _activeSplatters.Count; i++)
            {
                var splatter = _activeSplatters[i];
                if (splatter == null)
                    continue;

                var col = new Color(0.0f, 0.7f, 1f, 1.0f);
                Gizmos.matrix = splatter.WorldTransform;

                // draw the cube faces
                col.a = selected ? 0.3f : 0.1f;
                col.a *= isActiveAndEnabled ? 0.15f : 0.1f;
                Gizmos.color = col;
                Gizmos.DrawCube(Vector3.zero, Vector3.one);

                // draw the cube edges
                col.a = selected ? 0.5f : 0.2f;
                col.a *= isActiveAndEnabled ? 1 : 0.75f;
                Gizmos.color = col;
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            DrawGizmo(true);
        }
    }
}