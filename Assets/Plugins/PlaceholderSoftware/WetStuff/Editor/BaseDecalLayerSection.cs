using System;
using System.Collections.Generic;
using System.Linq;
using PlaceholderSoftware.WetStuff.Components;
using UnityEditor;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    internal abstract class BaseDecalLayerSection
        : BaseSerializedObjectFoldoutSection
    {
        #region  Nested Types

        private class ChannelDrawer
        {
            private static readonly LayerPreviewRenderer LayerPreview = new LayerPreviewRenderer();
            private readonly string _channelModeProperty;

            private readonly Color _color;
            private readonly string _inputSoftnessProperty;
            private readonly string _inputThresholdProperty;
            private readonly string _outputRangeProperty;
            private readonly Vector4 _weights;
            private PreviewRenderUtility _renderUtility;

            private bool _showPreview;

            public ChannelDrawer(
                Color color, Vector4 weights,
                [CanBeNull] string channelModeProperty,
                [NotNull] string inputThresholdProperty, [NotNull] string inputSoftnessProperty,
                [NotNull] string outputRangeProperty)
            {
                _color = color;
                _weights = weights;
                _channelModeProperty = channelModeProperty;
                _inputSoftnessProperty = inputSoftnessProperty;
                _inputThresholdProperty = inputThresholdProperty;
                _outputRangeProperty = outputRangeProperty;
            }

            public void Draw([NotNull] SerializedProperty property, [CanBeNull] SerializedProperty mask)
            {
                var modeProperty = property.FindPropertyRelative(_channelModeProperty);

                using (new VerticalStripContext(_color, lineWidth: 2))
                {
                    // Show mode property and show no more UI if it's disabled
                    if (modeProperty != null)
                    {
                        EditorGUILayout.PropertyField(modeProperty);

                        if ((DecalLayerChannel.DecalChannelMode) modeProperty.enumValueIndex == DecalLayerChannel.DecalChannelMode.Disabled)
                            return;
                    }

                    using (new EditorGUI.IndentLevelScope())
                    {
                        // Show advanced settings if we're explicitly in advanced mode (or we can't tell what the mode is)
                        var mode = modeProperty != null ? (DecalLayerChannel.DecalChannelMode) modeProperty.enumValueIndex : DecalLayerChannel.DecalChannelMode.AdvancedRangeRemap;

                        var inThreshold = property.FindPropertyRelative(_inputThresholdProperty);
                        var inSoftness = property.FindPropertyRelative(_inputSoftnessProperty);
                        var outRange = property.FindPropertyRelative(_outputRangeProperty);

                        // Show basic settings
                        var showInputRange = mode == DecalLayerChannel.DecalChannelMode.SimpleRangeRemap || mode == DecalLayerChannel.DecalChannelMode.AdvancedRangeRemap;
                        if (showInputRange)
                        {
                            EditorGUILayout.PropertyField(inThreshold);
                            EditorGUILayout.PropertyField(inSoftness);
                        }

                        // Show advanced settings if in advanced mode
                        var showOutputRange = mode == DecalLayerChannel.DecalChannelMode.AdvancedRangeRemap;
                        if (showOutputRange)
                            EditorGUILayout.PropertyField(outRange);

                        // If we were given no mask then no preview can be rendered, early exit
                        if (mask == null)
                            return;

                        // Create a list of reasons the visualiser cannot be displayed
                        var reasons = new List<string>();
                        if (mask.hasMultipleDifferentValues)
                            reasons.Add("'Layer Mask'");

                        if (showInputRange)
                        {
                            if (inThreshold.hasMultipleDifferentValues)
                                reasons.Add("'Input Threshold'");

                            if (inSoftness.hasMultipleDifferentValues)
                                reasons.Add("'Input Softness'");
                        }

                        if (showOutputRange)
                            if (outRange.hasMultipleDifferentValues)
                                reasons.Add("'Output Range'");

                        if (reasons.Count > 0)
                            EditorGUILayout.HelpBox(string.Format("Levels Visualiser unavailable with mixed {0} values", string.Join(", ", reasons.ToArray())), MessageType.Warning);
                        else
                        {
                            // Show a foldout section for the preview
                            if (modeProperty == null || (DecalLayerChannel.DecalChannelMode) modeProperty.enumValueIndex != DecalLayerChannel.DecalChannelMode.Disabled)
                                _showPreview = EditorGUILayout.Foldout(_showPreview, "Levels Visualizer");
                            else
                                _showPreview = false;

                            // Show the preview
                            if (_showPreview)
                            {
                                // Can't initialise this in a field initialiser because Unity won't less this be called in certan contexts
                                if (_renderUtility == null)
                                    _renderUtility = new PreviewRenderUtility(false);

                                // Calculate the area to draw the preview in
                                const float aspectRatio = 2.2f;
                                var width = EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth;
                                var height = width / aspectRatio;
                                var area = EditorGUILayout.GetControlRect(false, height);
                                area.yMax -= EditorGUIUtility.singleLineHeight / 2;
                                area.xMin += EditorGUIUtility.labelWidth;

                                var iRange = DecalLayerChannel.EvaluateInputRange(mode, inThreshold.floatValue, inSoftness.floatValue);
                                var oRange = DecalLayerChannel.EvaluateOutputRange(mode, outRange.vector2Value);
                                DrawLayerPreview(area, _renderUtility, LayerPreview, (Texture2D) mask.objectReferenceValue, iRange, oRange, _weights);
                            }
                        }
                    }
                }
            }

            private static void DrawLayerPreview(Rect position, [NotNull] PreviewRenderUtility preview, [NotNull] LayerPreviewRenderer renderer, [NotNull] Texture2D mask, Vector2 inputRange, Vector2 outputRange, Vector4 weights)
            {
                if (position.width <= 0 || position.height <= 0)
                    return;

                var aspect = position.width / position.height;
                const float extent = 1.4f;

#if UNITY_2017_1_OR_NEWER
                var camera = preview.camera;
#else
                var camera = preview.m_Camera;
#endif

                camera.projectionMatrix = Matrix4x4.Ortho(-extent, extent, -extent / aspect, extent / aspect, 0, 3);
                camera.worldToCameraMatrix = Matrix4x4.LookAt(new Vector3(0.25f - 0.25f, 0.25f - 0.25f, -1f), new Vector3(0f - 0.25f, 0 - 0.25f, 0f), Vector3.up);

                preview.BeginPreview(position, GUIStyle.none);
                {
                    GL.Clear(true, true, new Color(0.15f, 0.15f, 0.15f, 1));

                    renderer.Configure(mask, weights, inputRange.x, inputRange.y, outputRange.x, outputRange.y);
                    renderer.Render(preview);

#if UNITY_2017_1_OR_NEWER
                    preview.Render(updatefov: false);
#else
                    camera.Render();
#endif
                }
                var image = preview.EndPreview();

                GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
                GUI.DrawTexture(position, image, ScaleMode.StretchToFill, true);
                GL.sRGBWrite = false;
            }

            internal CopyPasteData.Channel Copy([NotNull] SerializedProperty property)
            {
                var modeProp = property.FindPropertyRelative(_channelModeProperty);
                var mode = modeProp == null ? null : (DecalLayerChannel.DecalChannelMode?) modeProp.enumValueIndex;

                var zeroes = mode.HasValue && mode.Value == DecalLayerChannel.DecalChannelMode.Disabled;

                return new CopyPasteData.Channel(
                    mode,
                    zeroes ? 0 : property.FindPropertyRelative(_inputThresholdProperty).floatValue,
                    zeroes ? 0 : property.FindPropertyRelative(_inputSoftnessProperty).floatValue,
                    zeroes ? Vector2.zero : property.FindPropertyRelative(_outputRangeProperty).vector2Value
                );
            }

            internal void Paste([NotNull] SerializedProperty property, CopyPasteData.Channel data)
            {
                var modeProp = property.FindPropertyRelative(_channelModeProperty);
                if (modeProp != null && data.Mode.HasValue)
                    modeProp.enumValueIndex = (int) data.Mode.Value;

                property.FindPropertyRelative(_inputThresholdProperty).floatValue = data.InputThreshold;
                property.FindPropertyRelative(_inputSoftnessProperty).floatValue = data.InputSoftness;

                property.FindPropertyRelative(_outputRangeProperty).vector2Value = data.OutputRange;
            }
        }

        [Serializable]
        private struct CopyPasteData
        {
            [SerializeField, UsedImplicitly] private Vector2 _layerMaskScale;
            public Vector2 LayerMaskScale { get { return _layerMaskScale; } }

            [SerializeField, UsedImplicitly] private Vector2 _layerMaskOffset;
            public Vector2 LayerMaskOffset { get { return _layerMaskOffset; } }

            [SerializeField, UsedImplicitly] private Channel _channel1;
            public Channel Channel1 { get { return _channel1; } }

            [SerializeField, UsedImplicitly] private Channel _channel2;
            public Channel Channel2 { get { return _channel2; } }

            [SerializeField, UsedImplicitly] private Channel _channel3;
            public Channel Channel3 { get { return _channel3; } }

            [SerializeField, UsedImplicitly] private Channel _channel4;
            public Channel Channel4 { get { return _channel4; } }

            public CopyPasteData(Vector2? layerMaskScale, Vector2? layerMaskOffset, Channel ch1, Channel ch2, Channel ch3, Channel ch4)
            {
                _layerMaskScale = layerMaskScale ?? new Vector2(1, 1);
                _layerMaskOffset = layerMaskOffset ?? new Vector2(0, 0);

                _channel1 = ch1;
                _channel2 = ch2;
                _channel3 = ch3;
                _channel4 = ch4;
            }

            [Serializable]
            public struct Channel
            {
                [SerializeField, UsedImplicitly] private int _mode;
                public DecalLayerChannel.DecalChannelMode? Mode
                {
                    get
                    {
                        if (_mode < 0)
                            return null;

                        return (DecalLayerChannel.DecalChannelMode)_mode;
                    }
                }

                [SerializeField, UsedImplicitly] private float _inputThreshold;
                public float InputThreshold { get { return _inputThreshold; } }

                [SerializeField, UsedImplicitly] private float _inputSoftness;
                public float InputSoftness { get { return _inputSoftness; } }

                [SerializeField, UsedImplicitly] private Vector2 _outputRange;
                public Vector2 OutputRange { get { return _outputRange; } }

                public Channel(DecalLayerChannel.DecalChannelMode? mode, float inputThreshold, float inputSoftness, Vector2 outputRange)
                {
                    _mode = mode.HasValue ? (int)mode.Value : -1;
                    _inputThreshold = inputThreshold;
                    _inputSoftness = inputSoftness;
                    _outputRange = outputRange;
                }
            }
        }
        #endregion

        private static readonly Color Red = new Color(226 / 255f, 77 / 255f, 36 / 255f);
        private static readonly Color Green = new Color(8 / 255f, 204 / 255f, 57 / 255f);
        private static readonly Color Blue = new Color(32 / 255f, 99 / 255f, 251 / 255f);
        private static readonly Color White = new Color(250 / 255f, 238 / 255f, 242 / 255f);

        private readonly string _basePath;
        private readonly ChannelDrawer _channel1;

        [NotNull] private readonly string _channel1Property;
        private readonly ChannelDrawer _channel2;
        [NotNull] private readonly string _channel2Property;
        private readonly ChannelDrawer _channel3;
        [NotNull] private readonly string _channel3Property;
        private readonly ChannelDrawer _channel4;
        [NotNull] private readonly string _channel4Property;
        [CanBeNull] private readonly string _layerMaskOffsetProperty;

        [CanBeNull] private readonly string _layerMaskProperty;
        [CanBeNull] private readonly string _layerMaskScaleProperty;
        private readonly string _layerModeProperty;

        private readonly string _decalShapeProperty;

        [CanBeNull] private readonly string _sectionExpandedProperty;

        private readonly GUIContent _title;
        private readonly LayerMode _visibleMode;

        private SerializedProperty _layerMode;
        private SerializedProperty _decalShape;

        protected override GUIContent Title
        {
            get { return _title; }
        }

        public override bool IsVisible
        {
            get
            {
                return _layerMode == null  || !_layerMode.hasMultipleDifferentValues && (LayerMode)_layerMode.enumValueIndex == _visibleMode && _decalShape.intValue != (int)DecalShape.Mesh;
            }
        }

        protected BaseDecalLayerSection(
            string title, string basePath,
            [CanBeNull] string layerModeProperty, LayerMode visibleMode,
            [CanBeNull] string layerMaskProperty, [CanBeNull] string layerMaskScaleProperty, [CanBeNull] string layerMaskOffsetProperty,
            [NotNull] string channel1Property, [NotNull] string channel2Property, [NotNull] string channel3Property, [NotNull] string channel4Property,
            [CanBeNull] string channelModeProperty,
            [NotNull] string channelInputThresholdProperty, [NotNull] string channelInputSoftnessProperty,
            [NotNull] string channelOutputRangeProperty, [CanBeNull] string sectionExpandedProperty,
            [NotNull] string decalShapeProperty
        )
        {
            _basePath = basePath;
            _layerModeProperty = layerModeProperty;
            _visibleMode = visibleMode;
            _decalShapeProperty = decalShapeProperty;
            _title = new GUIContent(title);

            _layerMaskProperty = layerMaskProperty;
            _layerMaskScaleProperty = layerMaskScaleProperty;
            _layerMaskOffsetProperty = layerMaskOffsetProperty;

            _channel1Property = channel1Property;
            _channel2Property = channel2Property;
            _channel3Property = channel3Property;
            _channel4Property = channel4Property;

            _sectionExpandedProperty = sectionExpandedProperty;

            _channel1 = new ChannelDrawer(Red, new Vector4(1, 0, 0, 0), channelModeProperty, channelInputThresholdProperty, channelInputSoftnessProperty, channelOutputRangeProperty);
            _channel2 = new ChannelDrawer(Green, new Vector4(0, 1, 0, 0), channelModeProperty, channelInputThresholdProperty, channelInputSoftnessProperty, channelOutputRangeProperty);
            _channel3 = new ChannelDrawer(Blue, new Vector4(0, 0, 1, 0), channelModeProperty, channelInputThresholdProperty, channelInputSoftnessProperty, channelOutputRangeProperty);
            _channel4 = new ChannelDrawer(White, new Vector4(0, 0, 0, 1), channelModeProperty, channelInputThresholdProperty, channelInputSoftnessProperty, channelOutputRangeProperty);
        }

        public override void OnEnable(SerializedObject target)
        {
            base.OnEnable(target);

            if (_layerModeProperty != null)
                _layerMode = target.FindProperty(_layerModeProperty);
            if (_decalShapeProperty != null)
                _decalShape = target.FindProperty(_decalShapeProperty);
        }

        protected override SerializedProperty FindExpandedProperty(SerializedObject target)
        {
            var prop = RelProp(target, _sectionExpandedProperty);
            return prop;
        }

        [NotNull]
        protected override GenericMenu GetHeaderMenu()
        {
            var g = new GenericMenu();
            g.AddItem(new GUIContent("Copy"), false, Copy);
            if (CanPaste())
                g.AddItem(new GUIContent("Paste"), false, Paste);
            else
                g.AddDisabledItem(new GUIContent("Paste"));

            return g;
        }

        private static bool CanPaste()
        {
            try
            {
                JsonUtility.FromJson<CopyPasteData>(EditorGUIUtility.systemCopyBuffer);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Paste()
        {
            var data = JsonUtility.FromJson<CopyPasteData>(EditorGUIUtility.systemCopyBuffer);

            var scaleProp = RelProp(Target, _layerMaskScaleProperty);
            var offsetProp = RelProp(Target, _layerMaskOffsetProperty);

            if (scaleProp != null)
                scaleProp.vector2Value = data.LayerMaskScale;

            if (offsetProp != null)
                offsetProp.vector2Value = data.LayerMaskOffset;

            _channel1.Paste(RelProp(Target, _channel1Property), data.Channel1);
            _channel2.Paste(RelProp(Target, _channel2Property), data.Channel2);
            _channel3.Paste(RelProp(Target, _channel3Property), data.Channel3);
            _channel4.Paste(RelProp(Target, _channel4Property), data.Channel4);

            Target.ApplyModifiedProperties();
        }

        private void Copy()
        {
            var scaleProp = RelProp(Target, _layerMaskScaleProperty);
            var offsetProp = RelProp(Target, _layerMaskOffsetProperty);

            var data = new CopyPasteData(
                scaleProp == null ? (Vector2?) null : scaleProp.vector2Value,
                offsetProp == null ? (Vector2?) null : offsetProp.vector2Value,
                _channel1.Copy(RelProp(Target, _channel1Property)),
                _channel2.Copy(RelProp(Target, _channel2Property)),
                _channel3.Copy(RelProp(Target, _channel3Property)),
                _channel4.Copy(RelProp(Target, _channel4Property))
            );

            EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(data, true);
        }

        [ContractAnnotation("name:null => null; name:notnull => notnull")]
        private SerializedProperty RelProp([NotNull] SerializedObject root, string name)
        {
            if (name == null)
                return null;

            var path = string.IsNullOrEmpty(_basePath) ? name : string.Format("{0}.{1}", _basePath, name);
            var prop = root.FindProperty(path);

            if (prop == null)
                throw new InvalidOperationException(string.Format("Cannot find property with path '{0}' on '{1}'", path, root.targetObjects.First().GetType().Name));

            return prop;
        }

        protected override void DrawContent()
        {
            // Draw the layer mask (if the property is not null)
            var layerMaskProperty = RelProp(Target, _layerMaskProperty);
            if (layerMaskProperty != null)
                EditorGUILayout.PropertyField(layerMaskProperty);

            var hasTexture = layerMaskProperty == null || layerMaskProperty.objectReferenceValue != null;
            using (new EditorGUI.DisabledScope(!hasTexture))
            {
                // Shown scale and offset settings
                if (_layerMaskScaleProperty != null)
                    EditorGUILayout.PropertyField(RelProp(Target, _layerMaskScaleProperty));

                if (_layerMaskOffsetProperty != null)
                    EditorGUILayout.PropertyField(RelProp(Target, _layerMaskOffsetProperty));

                // Show channel settings
                using (new EditorGUI.IndentLevelScope())
                {
                    _channel1.Draw(RelProp(Target, _channel1Property), layerMaskProperty);
                    _channel2.Draw(RelProp(Target, _channel2Property), layerMaskProperty);
                    _channel3.Draw(RelProp(Target, _channel3Property), layerMaskProperty);
                    _channel4.Draw(RelProp(Target, _channel4Property), layerMaskProperty);
                }
            }
        }
    }
}