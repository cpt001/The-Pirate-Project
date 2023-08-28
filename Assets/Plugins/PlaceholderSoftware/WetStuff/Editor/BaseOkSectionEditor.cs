using System;
using PlaceholderSoftware.WetStuff.Components;
using UnityEditor;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    public class BaseOkSectionEditor
        : Editor
    {
        private readonly IOkComponent<SerializedObject>[] _sections;

        public BaseOkSectionEditor([NotNull] params IOkComponent<SerializedObject>[] sections)
        {
            if (sections == null)
                throw new ArgumentNullException("sections");

            _sections = sections;
        }

        [UsedImplicitly]
        protected virtual void OnEnable()
        {
            foreach (var section in _sections)
                section.OnEnable(serializedObject);
        }

        [UsedImplicitly]
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Space(4);

            var allOk = true;
            foreach (var section in _sections)
            {
                using (new EditorGUI.DisabledScope(!allOk))
                    section.Draw();
                allOk &= section.Ok;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
