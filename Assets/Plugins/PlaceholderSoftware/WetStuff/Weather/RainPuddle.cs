using System;
using System.Collections.Generic;
using PlaceholderSoftware.WetStuff.Debugging;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Weather
{
    /// <summary>
    ///     A puddle which expands when it is raining and shrinks when it is not
    /// </summary>
    [ExecuteInEditMode, HelpURL("https://placeholder-software.co.uk/wetstuff/docs/Reference/RainPuddle/")]
    public class RainPuddle
        : MonoBehaviour
    {
        #region Nested Types

        public enum RainState
        {
            Drying,
            Raining
        }

        /// <summary>
        ///     The values of a single channel in a puddle.
        /// </summary>
        [Serializable]
        public struct ChannelValues
        {
            [Range(0, 1)] public float Threshold;
            [Range(0, 1)] public float Softness;
            [Range(0, 1)] public float OutMin;
            [Range(0, 1)] public float OutMax;

            private ChannelValues(float threshold, float softness, float outMin, float outMax)
            {
                Threshold = threshold;
                Softness = softness;
                OutMin = outMin;
                OutMax = outMax;
            }

            public override string ToString()
            {
                return string.Format("T:{0}, S:{1}, Out:{2}..{3}", Threshold, Softness, OutMin, OutMax);
            }

            public void Apply([NotNull] DecalLayerChannel channel)
            {
                channel.InputRangeThreshold = Threshold;
                channel.InputRangeSoftness = Softness;
                channel.OutputRange = new Vector2(OutMin, OutMax);
            }

            public static ChannelValues Lerp(ChannelValues a, ChannelValues b, ChannelCurves curves, float progress)
            {
                return new ChannelValues(
                    Mathf.Lerp(a.Threshold, b.Threshold, curves.Threshold.Evaluate(progress)),
                    Mathf.Lerp(a.Softness, b.Softness, curves.Softness.Evaluate(progress)),
                    Mathf.Lerp(a.OutMin, b.OutMin, curves.OutputMin.Evaluate(progress)),
                    Mathf.Lerp(a.OutMax, b.OutMax, curves.OutputMax.Evaluate(progress))
                );
            }
        }

        [Serializable]
        public struct ChannelCurves
        {
            public AnimationCurve Threshold;
            public AnimationCurve Softness;
            public AnimationCurve OutputMin;
            public AnimationCurve OutputMax;

            public static readonly ChannelCurves Default = new ChannelCurves {
                Threshold = AnimationCurve.EaseInOut(0, 0, 1, 1),
                Softness = AnimationCurve.EaseInOut(0, 0, 1, 1),
                OutputMin = AnimationCurve.EaseInOut(0, 0, 1, 1),
                OutputMax = AnimationCurve.EaseInOut(0, 0, 1, 1)
            };
        }

        [Serializable]
        public struct DecalState
        {
            [Range(0, 1)] public float Saturation;
            public ChannelValues Red;
            public ChannelValues Blue;
            public ChannelValues Green;
            public ChannelValues Alpha;

            public DecalState(float saturation, ChannelValues red, ChannelValues green, ChannelValues blue, ChannelValues alpha)
            {
                Saturation = saturation;
                Red = red;
                Blue = blue;
                Green = green;
                Alpha = alpha;
            }

            public override string ToString()
            {
                return string.Format("S:{0}, R:{{{1}}},- G:{{{2}}}, B:{{{3}}}, A:{{{4}}}", Saturation, Red, Green, Blue, Alpha);
            }

            public void Apply([NotNull] WetDecal decal)
            {
                decal.Settings.Saturation = Saturation;
                Red.Apply(decal.Settings.YLayer.Channel1);
                Blue.Apply(decal.Settings.YLayer.Channel2);
                Green.Apply(decal.Settings.YLayer.Channel3);
                Alpha.Apply(decal.Settings.YLayer.Channel4);
            }

            public static DecalState Lerp(DecalState a, DecalState b, DecalStateCurves curves, float progress)
            {
                return new DecalState(
                    Mathf.Lerp(a.Saturation, b.Saturation, curves.Saturation.Evaluate(progress)),
                    ChannelValues.Lerp(a.Red, b.Red, curves.Red, progress),
                    ChannelValues.Lerp(a.Green, b.Green, curves.Green, progress),
                    ChannelValues.Lerp(a.Blue, b.Blue, curves.Blue, progress),
                    ChannelValues.Lerp(a.Alpha, b.Alpha, curves.Alpha, progress)
                );
            }
        }

        [Serializable]
        public struct DecalStateCurves
        {
            public AnimationCurve Saturation;
            public ChannelCurves Red;
            public ChannelCurves Blue;
            public ChannelCurves Green;
            public ChannelCurves Alpha;

            public static readonly DecalStateCurves Default = new DecalStateCurves {
                Saturation = AnimationCurve.EaseInOut(0, 0, 1, 1),
                Red = ChannelCurves.Default,
                Blue = ChannelCurves.Default,
                Green = ChannelCurves.Default,
                Alpha = ChannelCurves.Default
            };
        }

        private class RainDecal : IDisposable
        {
            private readonly RainPuddle _rain;
            private readonly WetDecal _decal;

            private float _progress;
            private float _rateScale;
            private float _retiredAge;
            private RainDecalState _mode;
            private DecalState _state;
            private DecalState _baseState;

            public bool IsDead
            {
                get { return _mode == RainDecalState.Retired && IsComplete; }
            }

            public bool IsComplete
            {
                get { return _progress >= 1; }
            }

            public RainDecal(RainPuddle rain, WetDecal decal)
            {
                _rain = rain;
                _decal = decal;
            }

            public void Rain(float initialProgress = 0)
            {
                _baseState = _rain.DryState;
                _state = _baseState;
                _progress = initialProgress;
                _rateScale = 1;
                _mode = RainDecalState.Raining;

                _state.Apply(_decal);

                _decal.gameObject.SetActive(true);
            }

            public void Dry()
            {
                if (_progress > float.Epsilon)
                {
                    _baseState = _state;
                    _rateScale = 1 / _progress;
                    _progress = 0;
                }
                else
                {
                    _rateScale = 1;
                    _progress = 1;
                }

                _mode = RainDecalState.Drying;
            }

            public void Retire()
            {
                if (_mode == RainDecalState.Raining)
                    Dry();

                _mode = RainDecalState.Retired;
                _retiredAge = _progress;
            }

            public void Dispose()
            {
                DestroyImmediate(_decal.gameObject);
            }

            public bool Update()
            {
                if (_decal == null)
                    return false;

                _progress = Mathf.Clamp01(_progress + _rain.Rate * Time.deltaTime * _rateScale);

                if (_mode == RainDecalState.Raining)
                    _state = DecalState.Lerp(_baseState, _rain.WetState, _rain.Raining, _progress);
                else
                    _state = DecalState.Lerp(_baseState, _rain.DryState, _rain.Drying, _progress);

                if (_mode == RainDecalState.Retired)
                {
                    var alpha = (_progress - _retiredAge) / (1 - _retiredAge);
                    if (float.IsNaN(alpha)) alpha = 1;
                    _state.Saturation *= _rain.RetireFadeout.Evaluate(alpha);
                }

                _state.Apply(_decal);

                if (IsDead)
                    _decal.gameObject.SetActive(false);

                return !IsDead;
            }
        }

        private enum RainDecalState
        {
            Raining,
            Drying,
            Retired
        }

        #endregion

        private static readonly Log Log = Logs.Create(LogCategory.Integration, typeof(RainPuddle).Name);

        [NonSerialized] private readonly List<RainDecal> _raining;
        [NonSerialized] private readonly List<RainDecal> _drying;
        [NonSerialized] private readonly List<RainDecal> _retired;
        [NonSerialized] private readonly Stack<RainDecal> _dead;

#pragma warning disable 0649
        [SerializeField, UsedImplicitly] private bool _referencesEditorSectionExpanded;
        [SerializeField, UsedImplicitly] private bool _wetCurvesEditorSectionExpanded;
        [SerializeField, UsedImplicitly] private bool _dryCurvesEditorSectionExpanded;
        [SerializeField, UsedImplicitly] private bool _wetStateEditorSectionExpanded;
        [SerializeField, UsedImplicitly] private bool _dryStateEditorSectionExpanded;
        [SerializeField, UsedImplicitly] private bool _retireEditorSectionExpanded;

        [SerializeField, UsedImplicitly] private bool _autoPlay;
#pragma warning restore 0649

        public float Rate { get; set; }

        public DecalState WetState;
        public DecalState DryState;
        public DecalStateCurves Raining; // = DecalStateCurves.Default;
        public DecalStateCurves Drying; // = DecalStateCurves.Default;
        public AnimationCurve RetireFadeout = AnimationCurve.EaseInOut(0, 1, 1, 0);
        public WetDecal DecalPrefab;

        public RainState State { get; private set; }

        public RainPuddle()
        {
            Rate = 0.05f;
            _raining = new List<RainDecal>();
            _drying = new List<RainDecal>();
            _retired = new List<RainDecal>();
            _dead = new Stack<RainDecal>();
        }

        public void BeginRaining(float initialProgress = 0)
        {
            Log.Debug("Beginning raining cycle");

            Retire(_drying);
            Retire(_raining);

            var decal = Spawn();
            if (decal != null)
            {
                decal.Rain(initialProgress);
                _raining.Add(decal);
            }

            State = RainState.Raining;
        }

        public void BeginDrying()
        {
            Log.Debug("Beginning drying cycle");

            Dry(_raining);

            State = RainState.Drying;
        }

        [CanBeNull] private RainDecal Spawn()
        {
            if (_dead.Count > 0)
            {
                Log.Trace("Spawning rain decal, recycling old instance");
                return _dead.Pop();
            }

            if (DecalPrefab == null)
                return null;

            Log.Trace("Spawning rain decal, instantiating new instance");

            var decal = Instantiate(DecalPrefab, transform);
            decal.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy;

            return new RainDecal(this, decal);
        }

        protected void Awake()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
#endif
        }

        protected void OnDestroy()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        private void EditorApplication_playModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                Reset();
        }

        private void Reset()
        {
            Reset(_raining);
            Reset(_drying);
            Reset(_retired);
            Reset(_dead);

            foreach (var decal in GetComponentsInChildren<WetDecal>())
                DestroyImmediate(decal.gameObject);

            State = RainState.Drying;
        }

        private void Reset([NotNull] List<RainDecal> decals)
        {
            for (var i = decals.Count - 1; i >= 0; i--)
            {
                decals[i].Dispose();
            }

            decals.Clear();
        }

        private void Reset([NotNull] Stack<RainDecal> decals)
        {
            while (decals.Count > 0)
                decals.Pop().Dispose();
        }
#endif

        protected virtual void Update()
        {
#if UNITY_EDITOR
            if (_autoPlay)
            {
                if (UnityEditor.Selection.activeGameObject == gameObject)
                {
                    Rate = 0.05f;

                    if (_raining.Count == 0 && (_drying.Count == 0 || _drying[_drying.Count - 1].IsComplete))
                        BeginRaining();
                    else if (_raining.Count > 0 && _raining[_raining.Count - 1].IsComplete)
                        BeginDrying();
                }
            }

            if (UnityEditor.Selection.activeGameObject != gameObject && !Application.isPlaying)
                Reset();
#endif

            Update(_raining);
            Update(_drying);
            Update(_retired);
        }

        private void Update([NotNull] List<RainDecal> decals)
        {
            for (var i = decals.Count - 1; i >= 0; i--)
            {
                var decal = decals[i];
                if (!decal.Update())
                {
                    decals.RemoveAt(i);

                    if (Application.isPlaying)
                    {
                        Log.Trace("Rain decal died, queueing for recycling");
                        _dead.Push(decal);
                    }
                    else
                    {
                        Log.Trace("Rain decal died, destroying");
                        decal.Dispose();
                    }
                }
            }
        }

        private void Retire([NotNull] List<RainDecal> decals)
        {
            for (var i = 0; i < decals.Count; i++)
            {
                Log.Trace("Rain decal retired");

                decals[i].Retire();
                _retired.Add(decals[i]);
            }

            decals.Clear();
        }

        private void Dry([NotNull] List<RainDecal> decals)
        {
            for (var i = 0; i < decals.Count; i++)
            {
                Log.Trace("Rain decal drying");

                decals[i].Dry();
                _drying.Add(decals[i]);
            }

            decals.Clear();
        }
    }
}