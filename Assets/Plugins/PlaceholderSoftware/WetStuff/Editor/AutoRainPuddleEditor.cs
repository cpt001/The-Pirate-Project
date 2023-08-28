using PlaceholderSoftware.WetStuff.Weather;
using UnityEditor;

namespace PlaceholderSoftware.WetStuff
{
    [CustomEditor(typeof(AutoRainPuddle))]
    [CanEditMultipleObjects]
    public class AutoRainPuddleEditor
        : RainPuddleEditor
    {
        public AutoRainPuddleEditor()
            : base(new AutoReferences())
        {
        }

        private class AutoReferences
            : References
        {
            private bool _wetnessOk
            {
                get
                {
                    if (_wetness == null)
                        return false;

                    foreach (var o in Target.targetObjects)
                    {
                        var item = (AutoRainPuddle)o;
                        if (item.WetnessSource == null)
                            return false;
                    }

                    return true;
                }
            }
            public override bool Ok
            {
                get { return base.Ok && _wetnessOk; }
            }

            public override bool Expanded
            {
                get
                {
                    if (!Ok)
                        return true;
                    else
                        return base.Expanded;
                }
                set
                {
                    base.Expanded = value;
                }
            }

            private SerializedProperty _wetness;
            private SerializedProperty _rainSpeed;
            private SerializedProperty _drySpeed;

            public override void OnEnable(SerializedObject target)
            {
                base.OnEnable(target);

                _wetness = target.FindProperty("WetnessSource");
                _rainSpeed = target.FindProperty("RainingSpeed");
                _drySpeed = target.FindProperty("DryingSpeed");
            }

            protected override void DrawContent()
            {
                if (!_wetnessOk)
                    EditorGUILayout.HelpBox("Choose a wetness source", MessageType.Warning);

                EditorGUILayout.PropertyField(_wetness);

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_rainSpeed);
                EditorGUILayout.PropertyField(_drySpeed);
                EditorGUI.indentLevel--;

                base.DrawContent();
            }
        }
    }
}
