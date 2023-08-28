using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    [CustomEditor(typeof(PlaceholderSoftware.WetStuff.WetStuff))]
    [CanEditMultipleObjects]
    internal class WetSurfaceDecalsEditor
        : Editor
    {
        public override void OnInspectorGUI()
        {
            var s = new SerializedObject(target);

            CheckCameraIsDeferred(s);

            //EditorGUILayout.PropertyField(s.FindProperty("_ambientModificationFactor"));

            EditorGUILayout.Space();

            if (GUILayout.Button("Diagnostic Settings"))
                DebugSettingsEditor.GoToSettings();
            if (GUILayout.Button("Render Settings"))
                RenderSettingsEditor.GoToSettings();

            s.ApplyModifiedProperties();
        }

        private static void CheckCameraIsDeferred([NotNull] SerializedObject o)
        {
            var pathErr = false;

            foreach (var wsd in o.targetObjects.OfType<PlaceholderSoftware.WetStuff.WetStuff>())
            {
                var camera = wsd.gameObject.GetComponent<Camera>();
                if (camera && camera.renderingPath != RenderingPath.DeferredShading)
                    pathErr = true;
            }

            if (pathErr)
                EditorGUILayout.HelpBox("Camera must use Deferred rendering path", MessageType.Error);
        }
    }
}