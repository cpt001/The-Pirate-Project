using PlaceholderSoftware.WetStuff.Components;
using UnityEditor;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    public class BaseSectionEditor
        : Editor
    {
        private readonly IComponent<SerializedObject>[] _sections;

        public BaseSectionEditor(params IComponent<SerializedObject>[] sections)
        {
            _sections = sections;
        }

        [UsedImplicitly]
        private void OnEnable()
        {
            foreach (var section in _sections)
                section.OnEnable(serializedObject);
        }

        [UsedImplicitly]
        public override void OnInspectorGUI()
        {
            GUILayout.Space(4);

            foreach (var section in _sections)
            {
                if (section.IsVisible)
                    section.Draw();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}