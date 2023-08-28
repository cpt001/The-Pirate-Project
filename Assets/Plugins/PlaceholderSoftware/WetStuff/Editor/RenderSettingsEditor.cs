using PlaceholderSoftware.WetStuff.Rendering;
using UnityEditor;

namespace PlaceholderSoftware.WetStuff
{
    [CustomEditor(typeof(RenderSettings))]
    internal class RenderSettingsEditor
        : Editor
    {
        public void Awake()
        {
        }

        public override void OnInspectorGUI()
        {
            var settings = (RenderSettings) target;

            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
            {
                DrawRenderSettings(settings);

                if (changeCheckScope.changed)
                    EditorUtility.SetDirty(settings);
            }
        }

        private void DrawRenderSettings([NotNull] RenderSettings settings)
        {
            settings.EnableInstancing = EditorGUILayout.Toggle("Enable Instancing", settings.EnableInstancing);
            settings.EnableStencil = EditorGUILayout.Toggle("Enable Stencil", settings.EnableStencil);
            settings.EnableNormalSmoothing = EditorGUILayout.Toggle("Enable Normal Smoothing", settings.EnableNormalSmoothing);
        }

        public static void GoToSettings()
        {
            var renderSettings = LoadRenderSettings();
            EditorApplication.delayCall += () =>
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = renderSettings;
            };
        }

        private static RenderSettings LoadRenderSettings()
        {
            var asset = AssetDatabase.LoadAssetAtPath<RenderSettings>(RenderSettings.SettingsFilePath);
            if (asset == null)
            {
                asset = CreateInstance<RenderSettings>();
                AssetDatabase.CreateAsset(asset, RenderSettings.SettingsFilePath);
                AssetDatabase.SaveAssets();
            }

            return asset;
        }
    }
}