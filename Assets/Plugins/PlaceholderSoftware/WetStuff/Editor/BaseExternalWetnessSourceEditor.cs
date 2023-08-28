using UnityEditor;

namespace PlaceholderSoftware.WetStuff
{
    public abstract class BaseExternalWetnessSourceEditor
        : Editor
    {
        private readonly string _name;
        //private readonly string _editorWetnessProperty;
        //private readonly string _editorDeltaProperty;

        protected BaseExternalWetnessSourceEditor(string name, string editorDeltaProperty)
        {
            _name = name;
            //_editorWetnessProperty = editorWetnessProperty;
            //_editorDeltaProperty = editorDeltaProperty;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(string.Format("This component reads wetness data from '{0}' for Wet Stuff", _name), MessageType.Info);

            //using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            //{
            //    if (EditorApplication.isPlaying)
            //    {
            //        EditorGUILayout.HelpBox(string.Format("Wetness values are being supplied by {0}", _name), MessageType.Info);

            //        var bews = (BaseExternalWetnessSource)target;
            //        EditorGUILayout.Slider("Editor Current Wetness", bews.CurrentWetness, 0, 1);
            //        EditorGUILayout.Slider("Editor Delta Wetness", bews.DeltaWetness, -0.1f, 0.1f);
            //    }
            //    else
            //    {
            //        var tgt = new SerializedObject(target);
            //        var current = tgt.FindProperty(_editorWetnessProperty);
            //        var delta = tgt.FindProperty(_editorDeltaProperty);

            //        EditorGUILayout.PropertyField(current);
            //        EditorGUILayout.PropertyField(delta);

            //        tgt.ApplyModifiedProperties();
            //    }
            //}
        }
    }
}
