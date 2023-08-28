using PlaceholderSoftware.WetStuff.Components;
using UnityEditor;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    [CustomEditor(typeof(WetDecal))]
    [CanEditMultipleObjects]
    internal class WetDecalEditor
        : BaseSectionEditor
    {
        #region Nested Types

        private class CoreSettings
            : BaseFoldoutSection<SerializedObject>
        {
            private SerializedProperty _edgeFadeoff;
            private SerializedProperty _edgeSharpness;
            private SerializedProperty _enableJitter;
            private SerializedProperty _jitter;
            private SerializedProperty _layerMode;
            private SerializedProperty _layerProjectionMode;
            private SerializedProperty _mode;
            private SerializedProperty _saturation;
            private SerializedProperty _shape;

            protected override GUIContent Title
            {
                get { return new GUIContent("Wet Decal"); }
            }

            protected override void DrawContent()
            {
                EditorGUILayout.PropertyField(_mode);
                EditorGUILayout.PropertyField(_saturation);
                EditorGUILayout.PropertyField(_shape);

                if (_shape.intValue != (int)DecalShape.Mesh)
                {
                    EditorGUILayout.PropertyField(_edgeFadeoff);
                    EditorGUILayout.PropertyField(_edgeSharpness);
                    EditorGUILayout.PropertyField(_layerMode);

                    var showLayerSettings = !_layerMode.hasMultipleDifferentValues && (LayerMode)_layerMode.enumValueIndex != LayerMode.None;
                    using (new EditorGUI.DisabledScope(!showLayerSettings))
                    {
                        EditorGUILayout.PropertyField(_layerProjectionMode);
                        EditorGUILayout.PropertyField(_enableJitter);

                        var showJitterSettings = !_enableJitter.hasMultipleDifferentValues && _enableJitter.boolValue;
                        using (new EditorGUI.DisabledScope(!showJitterSettings)) EditorGUILayout.PropertyField(_jitter);
                    }
                }
            }

            public override void OnEnable(SerializedObject target)
            {
                base.OnEnable(target);

                _mode = target.FindProperty("_settings._mode");
                _saturation = target.FindProperty("_settings._saturation");
                _shape = target.FindProperty("_settings._shape");
                _edgeFadeoff = target.FindProperty("_settings._edgeFadeoff");
                _edgeSharpness = target.FindProperty("_settings._faceSharpness");
                _layerMode = target.FindProperty("_settings._layerMode");
                _layerProjectionMode = target.FindProperty("_settings._layerProjection");
                _jitter = target.FindProperty("_settings._sampleJitter");
                _enableJitter = target.FindProperty("_settings._enableJitter");
            }
        }

        private class BaseDirectionalSettings
            : BaseDecalLayerSection
        {
            protected BaseDirectionalSettings(string title, string basePath, LayerMode visible)
                : base(
                    title, basePath, "_settings._layerMode", visible,
                    "_layerMask", "_layerMaskScale", "_layerMaskOffset",
                    "_channel1", "_channel2", "_channel3", "_channel4",
                    "_mode", "_inputRangeThreshold", "_inputRangeSoftness", "_outputRange", "_editorSectionFoldout", "_settings._shape"
                )
            {
            }
        }

        private class SingleLayerSettings
            : BaseDirectionalSettings
        {
            public SingleLayerSettings()
                : base("Detail Layer", "_settings._yLayer", LayerMode.Single)
            {
            }
        }

        private class XLayerSettings
            : BaseDirectionalSettings
        {
            public XLayerSettings()
                : base("X Detail Layer", "_settings._xLayer", LayerMode.Triplanar)
            {
            }
        }

        private class YLayerSettings
            : BaseDirectionalSettings
        {
            public YLayerSettings()
                : base("Y Detail Layer", "_settings._yLayer", LayerMode.Triplanar)
            {
            }
        }

        private class ZLayerSettings
            : BaseDirectionalSettings
        {
            public ZLayerSettings()
                : base("Z Detail Layer", "_settings._zLayer", LayerMode.Triplanar)
            {
            }
        }

        #endregion

        public WetDecalEditor()
            : base(new CoreSettings { Expanded = true }, new SingleLayerSettings(), new XLayerSettings(), new YLayerSettings(), new ZLayerSettings())
        {
        }
    }
}