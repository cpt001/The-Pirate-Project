using System;
using PlaceholderSoftware.WetStuff.Rendering;
using PlaceholderSoftware.WetStuff.Timeline.Settings;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    public interface IDecalSettings
    {
        /// <summary>
        ///     Determines if the decal projects wetness or dryness.
        /// </summary>
        WetDecalMode Mode { get; }

        /// <summary>
        ///     How wet the decal appears to be (between 0 and 1)
        /// </summary>
        float Saturation { get; }

        /// <summary>
        ///     Maximum number of pixels to jitter the saturation mask sampling
        /// </summary>
        float SampleJitter { get; }

        /// <summary>
        ///     Determines if detail layer sample jittering should be used
        /// </summary>
        bool EnableJitter { get; }

        /// <summary>
        ///     How sharply the decal fades off at edges
        /// </summary>
        float FaceSharpness { get; }

        /// <summary>
        ///     The detail layer mode
        /// </summary>
        LayerMode LayerMode { get; }

        /// <summary>
        ///     Gets or sets the layer texture projection mode.
        /// </summary>
        ProjectionMode LayerProjection { get; }

        /// <summary>
        ///     Per pixel saturation projected down the decal's x axis
        /// </summary>
        [NotNull]
        DecalLayer XLayer { get; }

        /// <summary>
        ///     Per pixel saturation projected down the decal's y axis
        /// </summary>
        [NotNull]
        DecalLayer YLayer { get; }

        /// <summary>
        ///     Per pixel saturation projected down the decal's z axis
        /// </summary>
        [NotNull]
        DecalLayer ZLayer { get; }

        /// <summary>
        ///     The shape of the decal
        /// </summary>
        DecalShape Shape { get; }

        /// <summary>
        ///     The distance from the edge of the shape from which saturation begins to fade, between 0 and 1.
        /// </summary>
        float EdgeFadeoff { get; }
    }

    public enum WetDecalMode
    {
        Wet,
        Dry
    }

    public enum LayerMode
    {
        None,
        Single,
        Triplanar
    }

    public enum DecalShape
    {
        Cube,
        Sphere,
        Mesh,
    }

    public enum ProjectionMode
    {
        Local,
        World
    }

    [Serializable]
    public class DecalLayer
    {
        [SerializeField, UsedImplicitly] private DecalLayerChannel _channel2;
        [SerializeField, UsedImplicitly] private DecalLayerChannel _channel3;
        [SerializeField, UsedImplicitly] private DecalLayerChannel _channel4;
        [SerializeField, UsedImplicitly] private DecalLayerChannel _channel1;

        [SerializeField, Tooltip("Texture with 4 channels (RGBA) of saturation"), UsedImplicitly]
        private Texture2D _layerMask;

        [SerializeField, Tooltip("Offset the position of the layer mask image"), UsedImplicitly]
        private Vector2 _layerMaskOffset;

        [SerializeField, Tooltip("Scale the layer mask image"), UsedImplicitly]
        private Vector2 _layerMaskScale;
#pragma warning disable 0414    // Value assigned but never used
        [SerializeField, UsedImplicitly] private bool _editorSectionFoldout;
#pragma warning restore 0414

        public event Action<bool> Changed;

        /// <summary>
        ///     Gets or sets the layer texture scale (x, y) and offset (z, w).
        /// </summary>
        public Vector4 LayerMaskScaleOffset
        {
            get { return new Vector4(_layerMaskScale.x, _layerMaskScale.y, _layerMaskOffset.x, _layerMaskOffset.y); }
            set
            {
                _layerMaskScale = new Vector2(value.x, value.y);
                _layerMaskOffset = new Vector2(value.z, value.w);
                OnChanged(true);
            }
        }

        /// <summary>
        ///     Gets or sets the layer threshold texture.
        /// </summary>
        [CanBeNull]
        public Texture2D LayerMask
        {
            get { return _layerMask; }
            set
            {
                _layerMask = value;
                OnChanged(true);
            }
        }

        /// <summary>
        ///     Gets the threshold control for the layer texture's red channel.
        /// </summary>
        public DecalLayerChannel Channel1
        {
            get { return _channel1; }
        }

        /// <summary>
        ///     Gets the threshold control for the layer texture's green channel.
        /// </summary>
        public DecalLayerChannel Channel2
        {
            get { return _channel2; }
        }

        /// <summary>
        ///     Gets the threshold control for the layer texture's blue channel.
        /// </summary>
        public DecalLayerChannel Channel3
        {
            get { return _channel3; }
        }

        /// <summary>
        ///     Gets the threshold control for the layer texture's alpha channel.
        /// </summary>
        public DecalLayerChannel Channel4
        {
            get { return _channel4; }
        }

        public DecalLayerChannelIndexer Channels
        {
            get { return new DecalLayerChannelIndexer(this); }
        }

        public DecalLayer()
        {
            _layerMaskScale = new Vector2(1, 1);
            _layerMaskOffset = new Vector2(0, 0);
            _channel1 = new DecalLayerChannel { Mode = DecalLayerChannel.DecalChannelMode.SimpleRangeRemap };
            _channel2 = new DecalLayerChannel();
            _channel3 = new DecalLayerChannel();
            _channel4 = new DecalLayerChannel();
        }

        public void Init()
        {
            Changed = null;

            _channel1.Init();
            _channel2.Init();
            _channel3.Init();
            _channel4.Init();

            Action handler = () => OnChanged(false);
            _channel1.Changed += handler;
            _channel2.Changed += handler;
            _channel3.Changed += handler;
            _channel4.Changed += handler;
        }

        public void EvaluateInputRange(out Vector4 start, out Vector4 end)
        {
            var r = Channel1.EvaluateInputRange();
            var g = Channel2.EvaluateInputRange();
            var b = Channel3.EvaluateInputRange();
            var a = Channel4.EvaluateInputRange();

            start = new Vector4(r.x, g.x, b.x, a.x);
            end = new Vector4(r.y, g.y, b.y, a.y);
        }

        public void EvaluateOutputRange(out Vector4 start, out Vector4 end)
        {
            var r = Channel1.EvaluateOutputRange();
            var g = Channel2.EvaluateOutputRange();
            var b = Channel3.EvaluateOutputRange();
            var a = Channel4.EvaluateOutputRange();

            start = new Vector4(r.x, g.x, b.x, a.x);
            end = new Vector4(r.y, g.y, b.y, a.y);
        }

        internal void EvaluateRanges(out LayerParameters parameters)
        {
            Vector4 layerInputEnd;
            EvaluateInputRange(out parameters.LayerInputStart, out layerInputEnd);
            parameters.LayerInputExtent = layerInputEnd - parameters.LayerInputStart;

            EvaluateOutputRange(out parameters.LayerOutputStart, out parameters.LayerOutputEnd);
        }

        protected virtual void OnChanged(bool requiresRebuild)
        {
            var handler = Changed;
            if (handler != null) handler(requiresRebuild);
        }

        internal DecalSettingsDataContainer.DecalLayerData Get()
        {
            return new DecalSettingsDataContainer.DecalLayerData(Channel1.Get(), Channel2.Get(), Channel3.Get(), Channel4.Get());
        }

        public void Apply(DecalSettingsDataContainer.DecalLayerData data)
        {
            Channel1.Apply(data.Channel1);
            Channel2.Apply(data.Channel2);
            Channel3.Apply(data.Channel3);
            Channel4.Apply(data.Channel4);
        }
    }

    public struct DecalLayerChannelIndexer
    {
        private readonly DecalLayer _layer;

        public DecalLayerChannel this[int index]
        {
            get
            {
                switch (index)
                {
                    case 1:
                        return _layer.Channel1;
                    case 2:
                        return _layer.Channel2;
                    case 3:
                        return _layer.Channel3;
                    case 4:
                        return _layer.Channel4;

                    default:
                        throw new IndexOutOfRangeException("Channel index must be 1, 2, 3, or 4");
                }
            }
        }

        public DecalLayerChannelIndexer(DecalLayer layer)
        {
            _layer = layer;
        }
    }

    /// <summary>
    ///     Declares how a channel of a decal layer texture is mapped to a wetness value.
    /// </summary>
    /// <remarks>
    ///     Each channel of a layer mask texture is mapped to an output wetness.
    ///     This mapping is performed by comparing the texture sample to an input range, and remapping that input range to an
    ///     output range.
    ///     When running in Simple mode, these input and output ranges are derived from a "threshold" and "softness" value.
    /// </remarks>
    [Serializable]
    public class DecalLayerChannel
    {
        #region Nested Types

        /// <summary>
        ///     Specifies the type of mapping used on this channel
        /// </summary>
        public enum DecalChannelMode
        {
            /// <summary>
            ///     No output
            /// </summary>
            Disabled,

            /// <summary>
            ///     Input values are output values
            /// </summary>
            Passthrough,

            /// <summary>
            ///     Threshold and softness are used to generate output values
            /// </summary>
            SimpleRangeRemap,

            /// <summary>
            ///     Threshold, softness and output range are used to generate output values
            /// </summary>
            AdvancedRangeRemap
        }

        #endregion

        public const float MinSoftness = 5 / 255f;
        public const float MaxSoftness = 1;

        [SerializeField, Range(MinSoftness, MaxSoftness), Tooltip("How steep the transition is from wet to dry")]
        private float _inputRangeSoftness;

        [SerializeField, Range(0, 1), Tooltip("Threshold for which values are considered wet")]
        private float _inputRangeThreshold;

        [SerializeField, Tooltip("How the input texture data is transformed into wetness values")]
        private DecalChannelMode _mode;

        [SerializeField, MinMax(0, 1), Tooltip("Limit the minimum and maximum wetness values of the output")]
        private Vector2 _outputRange;

        /// <summary>
        ///     Gets or sets the decal layer configuration mode.
        /// </summary>
        public DecalChannelMode Mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                OnChanged();
            }
        }

        /// <summary>
        ///     Gets the input range start used when in simple and advanced mode.
        /// </summary>
        public float InputRangeThreshold
        {
            get { return _inputRangeThreshold; }
            set
            {
                var changed = Math.Abs(_inputRangeThreshold - value) > float.Epsilon;
                _inputRangeThreshold = value;
                if (changed)
                    OnChanged();
            }
        }

        /// <summary>
        ///     Gets the input range extent used when in simple and advanced mode.
        /// </summary>
        public float InputRangeSoftness
        {
            get { return _inputRangeSoftness; }
            set
            {
                if (value < MinSoftness)
                    value = MinSoftness;
                else if (value > MaxSoftness)
                    value = MaxSoftness;

                var changed = Math.Abs(_inputRangeSoftness - value) > float.Epsilon;
                _inputRangeSoftness = value;
                if (changed)
                    OnChanged();
            }
        }

        /// <summary>
        ///     Gets the output range used when in Advanced mode.
        /// </summary>
        public Vector2 OutputRange
        {
            get { return _outputRange; }
            set
            {
                var changed = (_outputRange - value).SqrMagnitude() > float.Epsilon;
                _outputRange = value;
                if (changed)
                    OnChanged();
            }
        }

        public DecalLayerChannel()
        {
            Mode = DecalChannelMode.Disabled;
            InputRangeThreshold = 0.5f;
            InputRangeSoftness = 1.0f;
            OutputRange = new Vector2(0, 1);
        }

        public event Action Changed;

        internal void Init()
        {
            Changed = null;
        }

        private static Vector2 EvaluateRange(float threshold, float softness)
        {
            var start = 1 - threshold;
            var extent = Math.Max(softness, 0.0001f);

            start = start * (1 + extent) - extent;

            return new Vector2(start, start + extent);
        }

        public static Vector2 EvaluateInputRange(DecalChannelMode mode, float threshold, float softness)
        {
            if (mode == DecalChannelMode.Disabled)
                return new Vector2(1, 1);

            if (mode == DecalChannelMode.Passthrough)
                return new Vector2(0, 1);

            return EvaluateRange(threshold, softness);
        }

        public Vector2 EvaluateInputRange()
        {
            return EvaluateInputRange(Mode, InputRangeThreshold, InputRangeSoftness);
        }

        public static Vector2 EvaluateOutputRange(DecalChannelMode mode, Vector2 range)
        {
            if (mode != DecalChannelMode.AdvancedRangeRemap)
                return new Vector2(0, 1);

            return range;
        }

        public Vector2 EvaluateOutputRange()
        {
            return EvaluateOutputRange(Mode, OutputRange);
        }

        protected virtual void OnChanged()
        {
            var handler = Changed;
            if (handler != null) handler();
        }

        internal DecalSettingsDataContainer.DecalLayerChannelData Get()
        {
            return new DecalSettingsDataContainer.DecalLayerChannelData(InputRangeThreshold, InputRangeSoftness, OutputRange);
        }

        internal void Apply(DecalSettingsDataContainer.DecalLayerChannelData data)
        {
            InputRangeThreshold = data.InputRangeThreshold;
            InputRangeSoftness = data.InputRangeSoftness;

            OutputRange = data.OutputRange;
        }
    }
}