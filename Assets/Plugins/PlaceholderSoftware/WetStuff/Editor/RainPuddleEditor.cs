using System;
using PlaceholderSoftware.WetStuff.Components;
using PlaceholderSoftware.WetStuff.Weather;
using UnityEditor;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    [CustomEditor(typeof(RainPuddle))]
    [CanEditMultipleObjects]
    public class RainPuddleEditor
        : BaseOkSectionEditor
    {
        internal RainPuddleEditor([CanBeNull] References refsSection = null)
            : base(new EditorPlay(), refsSection ?? new References(), new WetState(), new DryState(), new RainingCurves(), new DryingCurves(), new FadeOut())
        {
        }

        private RainPuddleEditor()
            : this(new References())
        {
        }

        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        private class Default
            : DefaultInspectorSection, IOkComponent<SerializedObject>
        {
            public bool Ok { get { return true; } }
        }

        internal class References
            : BaseSerializedObjectFoldoutSection, IOkComponent<SerializedObject>
        {
            private bool _prefabOk
            {
                get
                {
                    if (_prefab == null)
                        return false;

                    foreach (var o in Target.targetObjects)
                    {
                        var item = (RainPuddle)o;
                        if (item.DecalPrefab == null)
                            return false;
                    }

                    return true;
                }
            }
            public virtual bool Ok
            {
                get { return _prefabOk; }
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

            private readonly GUIContent _nullTitle = new GUIContent("References (Requires Attention)");
            private readonly GUIContent _title = new GUIContent("References");
            protected override GUIContent Title { get { return !Ok ? _nullTitle : _title; } }

            private SerializedProperty _prefab;

            public override void OnEnable(SerializedObject target)
            {
                base.OnEnable(target);

                _prefab = target.FindProperty("DecalPrefab");
            }

            protected override void DrawContent()
            {
                if (!_prefabOk)
                    EditorGUILayout.HelpBox("Choose a wetness decal prefab", MessageType.Warning);
                EditorGUILayout.PropertyField(_prefab);
            }

            protected override SerializedProperty FindExpandedProperty(SerializedObject obj)
            {
                return obj.FindProperty("_referencesEditorSectionExpanded");
            }
        }

        private class EditorPlay
            : BasicSerializedObjectSection, IOkComponent<SerializedObject>
        {
            public bool Ok { get { return true; } }

            private readonly GUIContent _title = new GUIContent("Editor Preview");
            protected override GUIContent Title { get { return _title; } }

            private SerializedProperty _animate;

            public override void OnEnable(SerializedObject target)
            {
                _animate = target.FindProperty("_autoPlay");

                base.OnEnable(target);
            }

            protected override void DrawContent()
            {
                EditorGUILayout.PropertyField(_animate);

                var animating = _animate.boolValue || _animate.hasMultipleDifferentValues;
                using (new EditorGUI.DisabledScope(animating))
                {
                    if (GUILayout.Button("Rain"))
                        foreach (var tgt in Target.targetObjects)
                            ((RainPuddle)tgt).BeginRaining();

                    if (GUILayout.Button("Dry"))
                        foreach (var tgt in Target.targetObjects)
                            ((RainPuddle)tgt).BeginDrying();
                }
            }
        }

        private class BaseState
            : BaseSerializedObjectFoldoutSection, IOkComponent<SerializedObject>
        {
            public bool Ok { get { return true; } }

            private readonly GUIContent _title;
            protected override GUIContent Title { get { return _title; } }

            private readonly string _stateProperty;
            private readonly string _stateOpenProperty;

            private SerializedProperty _saturation;
            private SerializedProperty _red;
            private SerializedProperty _green;
            private SerializedProperty _blue;
            private SerializedProperty _alpha;
            private SerializedProperty _state;

            protected BaseState(string title, string stateProperty, string stateOpenProperty)
            {
                _title = new GUIContent(title);
                _stateProperty = stateProperty;
                _stateOpenProperty = stateOpenProperty;
            }

            public override void OnEnable(SerializedObject target)
            {
                base.OnEnable(target);

                _state = target.FindProperty(_stateProperty);
                _saturation = target.FindProperty(string.Format("{0}.Saturation", _stateProperty));
                _red = target.FindProperty(string.Format("{0}.Red", _stateProperty));
                _green = target.FindProperty(string.Format("{0}.Green", _stateProperty));
                _blue = target.FindProperty(string.Format("{0}.Blue", _stateProperty));
                _alpha = target.FindProperty(string.Format("{0}.Alpha", _stateProperty));
            }

            [NotNull] protected override GenericMenu GetHeaderMenu()
            {
                var g = new GenericMenu();

                if (CanCopy())
                    g.AddItem(new GUIContent("Copy"), false, Copy);
                else
                    g.AddDisabledItem(new GUIContent("Copy"));

                if (CanPaste())
                    g.AddItem(new GUIContent("Paste"), false, Paste);
                else
                    g.AddDisabledItem(new GUIContent("Paste"));

                return g;
            }

            protected override void DrawContent()
            {
                EditorGUILayout.PropertyField(_saturation);
                EditorGUILayout.PropertyField(_red, true);
                EditorGUILayout.PropertyField(_green, true);
                EditorGUILayout.PropertyField(_blue, true);
                EditorGUILayout.PropertyField(_alpha, true);
            }

            protected override SerializedProperty FindExpandedProperty(SerializedObject obj)
            {
                return obj.FindProperty(_stateOpenProperty);
            }

            #region copy/paste
            private RainPuddle.DecalState? TryGetCopyData()
            {
                if (_state.hasMultipleDifferentValues)
                    return null;

                var targetObject = _state.serializedObject.targetObjects[0];
                var field = targetObject.GetType().GetField(_state.propertyPath);
                var value = (RainPuddle.DecalState)field.GetValue(targetObject);

                return value;
            }

            private void Copy()
            {
                var copy = TryGetCopyData();
                if (copy != null)
                    EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(copy, true);
            }

            private bool CanCopy()
            {
                return !_state.hasMultipleDifferentValues;
            }

            private RainPuddle.DecalState? TryGetPasteData()
            {
                try
                {
                    var data = JsonUtility.FromJson<RainPuddle.DecalState>(EditorGUIUtility.systemCopyBuffer);
                    return data;
                }
                catch
                {
                    return null;
                }
            }

            private void Paste()
            {
                var data = TryGetPasteData();
                if (data == null)
                    return;

                var targetObjects = _state.serializedObject.targetObjects;
                var field = targetObjects[0].GetType().GetField(_state.propertyPath);

                foreach (var obj in targetObjects)
                {
                    field.SetValue(obj, data.Value);
                    EditorUtility.SetDirty(obj);
                }
            }

            private bool CanPaste()
            {
                return TryGetPasteData().HasValue;
            }
            #endregion
        }

        private class WetState
            : BaseState
        {
            public WetState()
                : base("Wet State", "WetState", "_wetStateEditorSectionExpanded")
            {

            }
        }

        private class DryState
            : BaseState
        {
            public DryState()
                : base("Dry State", "DryState", "_dryStateEditorSectionExpanded")
            {

            }
        }

        private class BaseCurves
            : BaseSerializedObjectFoldoutSection, IOkComponent<SerializedObject>
        {
            public bool Ok { get { return true; } }

            private readonly GUIContent _title;
            protected override GUIContent Title { get { return _title; } }

            private readonly string _curvesProperty;
            private readonly string _curvesOpenProperty;

            private SerializedProperty _saturation;
            private SerializedProperty _red;
            private SerializedProperty _green;
            private SerializedProperty _blue;
            private SerializedProperty _alpha;
            private SerializedProperty _curves;

            public BaseCurves(string title, string curvesProperty, string curvesOpenProperty)
            {
                _title = new GUIContent(title);
                _curvesProperty = curvesProperty;
                _curvesOpenProperty = curvesOpenProperty;
            }

            public override void OnEnable(SerializedObject target)
            {
                base.OnEnable(target);

                _curves = target.FindProperty(_curvesProperty);
                _saturation = target.FindProperty(string.Format("{0}.Saturation", _curvesProperty));
                _red = target.FindProperty(string.Format("{0}.Red", _curvesProperty));
                _green = target.FindProperty(string.Format("{0}.Green", _curvesProperty));
                _blue = target.FindProperty(string.Format("{0}.Blue", _curvesProperty));
                _alpha = target.FindProperty(string.Format("{0}.Alpha", _curvesProperty));
            }

            [NotNull] protected override GenericMenu GetHeaderMenu()
            {
                var g = new GenericMenu();

                if (CanCopy())
                    g.AddItem(new GUIContent("Copy"), false, Copy);
                else
                    g.AddDisabledItem(new GUIContent("Copy"));

                if (CanPaste())
                    g.AddItem(new GUIContent("Paste"), false, Paste);
                else
                    g.AddDisabledItem(new GUIContent("Paste"));

                return g;
            }

            protected override void DrawContent()
            {
                EditorGUILayout.PropertyField(_saturation);
                EditorGUILayout.PropertyField(_red, true);
                EditorGUILayout.PropertyField(_green, true);
                EditorGUILayout.PropertyField(_blue, true);
                EditorGUILayout.PropertyField(_alpha, true);
            }

            protected override SerializedProperty FindExpandedProperty(SerializedObject obj)
            {
                return obj.FindProperty(_curvesOpenProperty);
            }

            #region copy/paste
            private RainPuddle.DecalStateCurves? TryGetCopyData()
            {
                if (_curves.hasMultipleDifferentValues)
                    return null;

                var targetObject = _curves.serializedObject.targetObjects[0];
                var field = targetObject.GetType().GetField(_curves.propertyPath);
                var value = (RainPuddle.DecalStateCurves)field.GetValue(targetObject);

                return value;
            }

            private void Copy()
            {
                var copy = TryGetCopyData();
                if (copy != null)
                    EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(copy, true);
            }

            private bool CanCopy()
            {
                return !_curves.hasMultipleDifferentValues;
            }

            private RainPuddle.DecalStateCurves? TryGetPasteData()
            {
                try
                {
                    var data = JsonUtility.FromJson<RainPuddle.DecalStateCurves>(EditorGUIUtility.systemCopyBuffer);
                    return data;
                }
                catch
                {
                    return null;
                }
            }

            private void Paste()
            {
                var data = TryGetPasteData();
                if (data == null)
                    return;

                var targetObjects = _curves.serializedObject.targetObjects;
                var field = targetObjects[0].GetType().GetField(_curves.propertyPath);

                foreach (var obj in targetObjects)
                {
                    field.SetValue(obj, data.Value);
                    EditorUtility.SetDirty(obj);
                }

                Target.Update();
            }

            private bool CanPaste()
            {
                return TryGetPasteData().HasValue;
            }
            #endregion
        }

        private class RainingCurves
            : BaseCurves
        {
            public RainingCurves()
                : base("Raining Curves", "Raining", "_wetCurvesEditorSectionExpanded")
            {
            }
        }

        private class DryingCurves
            : BaseCurves
        {
            public DryingCurves()
                : base("Drying Curves", "Drying", "_dryCurvesEditorSectionExpanded")
            {
            }
        }

        private class FadeOut
            : BaseSerializedObjectFoldoutSection, IOkComponent<SerializedObject>
        {
            public bool Ok { get { return true; } }

            private readonly GUIContent _title = new GUIContent("Retire Fade");
            protected override GUIContent Title { get { return _title; } }

            private SerializedProperty _retire;

            public override void OnEnable(SerializedObject target)
            {
                base.OnEnable(target);

                _retire = target.FindProperty("RetireFadeout");
            }

            protected override void DrawContent()
            {
                EditorGUILayout.PropertyField(_retire);
            }

            protected override SerializedProperty FindExpandedProperty(SerializedObject obj)
            {
                return obj.FindProperty("_retireEditorSectionExpanded");
            }
        }
    }
}
