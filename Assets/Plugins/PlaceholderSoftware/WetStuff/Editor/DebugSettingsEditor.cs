using System;
using PlaceholderSoftware.WetStuff.Debugging;
using UnityEditor;

namespace PlaceholderSoftware.WetStuff
{
    [CustomEditor(typeof(DebugSettings))]
    internal class DebugSettingsEditor
        : Editor
    {
        private string[] _categoryNames;
        private int[] _categoryValues;
        private bool _showLogSettings;

        public void Awake()
        {
            _showLogSettings = true;
            _categoryNames = Enum.GetNames(typeof(LogCategory));
            _categoryValues = (int[])Enum.GetValues(typeof(LogCategory));
        }

        public override void OnInspectorGUI()
        {
            var settings = (DebugSettings)target;

            using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
            {
                DrawLogSettings(settings);

                if (changeCheckScope.changed)
                    EditorUtility.SetDirty(settings);
            }
        }

        private void DrawLogSettings([NotNull] DebugSettings settings)
        {
            _showLogSettings = EditorGUILayout.Foldout(_showLogSettings, "Log Levels");
            if (_showLogSettings)
            {
                EditorGUI.indentLevel++;

                for (var i = 0; i < _categoryNames.Length; i++)
                    settings.SetLevel(_categoryValues[i], (LogLevel)EditorGUILayout.EnumPopup(_categoryNames[i], settings.GetLevel(_categoryValues[i])));

                EditorGUI.indentLevel--;
            }
        }

        public static void GoToSettings()
        {
            var logSettings = LoadLogSettings();
            EditorApplication.delayCall += () =>
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = logSettings;
            };
        }

        private static DebugSettings LoadLogSettings()
        {
            var asset = AssetDatabase.LoadAssetAtPath<DebugSettings>(DebugSettings.SettingsFilePath);
            if (asset == null)
            {
                asset = CreateInstance<DebugSettings>();
                AssetDatabase.CreateAsset(asset, DebugSettings.SettingsFilePath);
                AssetDatabase.SaveAssets();
            }

            return asset;
        }
    }
}