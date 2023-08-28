using PlaceholderSoftware.WetStuff.Components;
using PlaceholderSoftware.WetStuff.Timeline.Settings;
using UnityEditor;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Timeline
{
    [CustomEditor(typeof(DecalSettingsClip))]
    internal class DecalSettingsClipsEditor
        : BaseSectionEditor
    {
        #region  Nested Types

        private class CoreSettings
            : BaseFoldoutSection<SerializedObject>
        {
            private SerializedProperty _saturation;

            protected override GUIContent Title
            {
                get { return new GUIContent("Wet Decal Clip"); }
            }

            protected override void DrawContent()
            {
                EditorGUILayout.PropertyField(_saturation);
            }

            public override void OnEnable(SerializedObject target)
            {
                base.OnEnable(target);

                _saturation = target.FindProperty("Template.Data.Saturation");
            }
        }

        private class BaseDirectionalSettings
            : BaseDecalLayerSection
        {
            protected BaseDirectionalSettings(string title, string basePath, LayerMode visible)
                : base(
                    title, basePath, null, visible, //todo fix
                    null, null, null,
                    "Channel1", "Channel2", "Channel3", "Channel4",
                    null, "InputRangeThreshold", "InputRangeSoftness", "OutputRange", "_editorSectionFoldout",
                    "_settings._shape"
                )
            {
            }
        }

        private class XLayerSettings
            : BaseDirectionalSettings
        {
            public XLayerSettings()
                : base("X Detail Layers", "Template.Data.XLayer", LayerMode.Triplanar)
            {
            }
        }

        private class YLayerSettings
            : BaseDirectionalSettings
        {
            public YLayerSettings()
                : base("Y Detail Layers", "Template.Data.YLayer", LayerMode.Triplanar)
            {
            }
        }

        private class ZLayerSettings
            : BaseDirectionalSettings
        {
            public ZLayerSettings()
                : base("Z Detail Layers", "Template.Data.ZLayer", LayerMode.Triplanar)
            {
            }
        }

        #endregion

        public DecalSettingsClipsEditor()
            : base(new CoreSettings { Expanded = true }, new XLayerSettings(), new YLayerSettings(), new ZLayerSettings())
        {
        }
    }
}