using PlaceholderSoftware.WetStuff.Components;
using UnityEditor;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    [CustomEditor(typeof(ParticleWetSplatter))]
    public class ParticleWetSplatterEditor
        : Editor
    {
        #region Nested Types

        private class DefaultInspector
            : BaseFoldoutSection<SerializedObject>
        {
            private readonly ParticleWetSplatterEditor _editor;

            protected override GUIContent Title
            {
                get { return new GUIContent("Default Inspector"); }
            }

            public DefaultInspector(ParticleWetSplatterEditor editor)
            {
                _editor = editor;
            }

            protected override void DrawContent()
            {
                _editor.DrawDefaultInspector();
            }
        }

        private class CoreSettings
            : BaseFoldoutSection<SerializedObject>
        {
            private SerializedProperty _decalChance;
            private SerializedProperty _decalSize;
            private SerializedProperty _saturation;
            private SerializedProperty _verticalOffset;

            protected override GUIContent Title
            {
                get { return new GUIContent("Particle Wet Splatter"); }
            }

            protected override void DrawContent()
            {
                if (EditorApplication.isPlaying)
                {
                    var reset = GUILayout.Button(new GUIContent("Clear Decals", "Remove all decals"));
                    if (reset)
                        for (var i = 0; i < Target.targetObjects.Length; i++)
                        {
                            var obj = Target.targetObjects[i];
                            var tObj = obj as ParticleWetSplatter;
                            if (tObj != null)
                                tObj.Clear();
                        }
                }

                EditorGUILayout.PropertyField(_decalSize);
                EditorGUILayout.PropertyField(_verticalOffset);
                EditorGUILayout.Slider(_decalChance, 0, 1);
                EditorGUILayout.Slider(_saturation, 0, 1);
            }

            public override void OnEnable(SerializedObject target)
            {
                base.OnEnable(target);

                _decalSize = target.FindProperty("_core._decalSize");
                _verticalOffset = target.FindProperty("_core._verticalOffset");
                _decalChance = target.FindProperty("_core._decalChance");
                _saturation = target.FindProperty("_core._saturation");
            }
        }

        private class LimitSettings
            : BaseSerializedObjectToggleSection
        {
            private SerializedProperty _decalChance;
            private SerializedProperty _maxDecals;

            protected override GUIContent Title
            {
                get { return new GUIContent("Decal Count Limit"); }
            }

            protected override void DrawContent()
            {
                EditorGUILayout.PropertyField(_maxDecals);
                EditorGUILayout.PropertyField(_decalChance);
            }

            public override void OnEnable(SerializedObject target)
            {
                base.OnEnable(target);

                _maxDecals = target.FindProperty("_limit._maxDecals");
                _decalChance = target.FindProperty("_limit._decalChance");
            }

            protected override SerializedProperty FindEnabledProperty(SerializedObject target)
            {
                return target.FindProperty("_limit._enabled");
            }
        }

        private class LifetimeSettings
            : BaseSerializedObjectToggleSection
        {
            private SerializedProperty _maxLifetime;
            private SerializedProperty _minLifetime;
            private SerializedProperty _saturation;

            protected override GUIContent Title
            {
                get { return new GUIContent("Lifetime", "Lifetime of the decal. This controls how long before the particle fades away."); }
            }

            protected override void DrawContent()
            {
                EditorGUILayout.PropertyField(_minLifetime);
                EditorGUILayout.PropertyField(_maxLifetime);

                if (_minLifetime.floatValue > _maxLifetime.floatValue)
                    _maxLifetime.floatValue = _minLifetime.floatValue;

                EditorGUILayout.PropertyField(_saturation);
            }

            public override void OnEnable(SerializedObject target)
            {
                base.OnEnable(target);

                _minLifetime = target.FindProperty("_lifetime._minLifetime");
                _maxLifetime = target.FindProperty("_lifetime._maxLifetime");
                _saturation = target.FindProperty("_lifetime._saturation");
            }

            protected override SerializedProperty FindEnabledProperty(SerializedObject target)
            {
                return target.FindProperty("_lifetime._enabled");
            }
        }

        private class RecyclingSettings
            : BaseSerializedObjectToggleSection
        {
            private SerializedProperty _lifetimeEnabled;
            private SerializedProperty _limitEnabled;

            private SerializedProperty _maxAcceleratedAgeing;
            private SerializedProperty _stealThreshold;

            protected override GUIContent Title
            {
                get { return new GUIContent("Recycling", "Once the maximum number of decals have been placed begin re-using older decals for new particle impacts"); }
            }

            protected override void DrawContent()
            {
                if (!_limitEnabled.boolValue)
                    EditorGUILayout.HelpBox("You set a maximum number of decals to use recycling", MessageType.Warning);

                using (new EditorGUI.DisabledScope(!_limitEnabled.boolValue))
                {
                    if (!_lifetimeEnabled.boolValue)
                        EditorGUILayout.HelpBox("You must enable decal lifetime to set lifetime related settings", MessageType.Warning);

                    using (new EditorGUI.DisabledScope(!_lifetimeEnabled.boolValue))
                    {
                        EditorGUILayout.PropertyField(_maxAcceleratedAgeing);
                        EditorGUILayout.PropertyField(_stealThreshold);
                    }
                }
            }

            public override void OnEnable(SerializedObject target)
            {
                base.OnEnable(target);

                _limitEnabled = target.FindProperty("_limit._enabled");
                _lifetimeEnabled = target.FindProperty("_lifetime._enabled");

                _maxAcceleratedAgeing = target.FindProperty("_recycling._maxAcceleratedAgeing");
                _stealThreshold = target.FindProperty("_recycling._stealThreshold");
            }

            protected override SerializedProperty FindEnabledProperty(SerializedObject target)
            {
                return target.FindProperty("_recycling._enabled");
            }
        }

        private class RandomizeSizeSettings
            : BaseSerializedObjectToggleSection
        {
            private SerializedProperty _maxInflation;
            private SerializedProperty _minInflation;

            protected override GUIContent Title
            {
                get { return new GUIContent("Randomize Size"); }
            }

            protected override void DrawContent()
            {
                EditorGUILayout.PropertyField(_minInflation);
                EditorGUILayout.PropertyField(_maxInflation);

                if (_maxInflation.floatValue < _minInflation.floatValue)
                    _maxInflation.floatValue = _minInflation.floatValue;
            }

            public override void OnEnable(SerializedObject target)
            {
                base.OnEnable(target);

                _minInflation = target.FindProperty("_randomizeSize._minInflation");
                _maxInflation = target.FindProperty("_randomizeSize._maxInflation");
            }

            protected override SerializedProperty FindEnabledProperty(SerializedObject target)
            {
                return target.FindProperty("_randomizeSize._enabled");
            }
        }

        private class RandomizeOrientationSettings
            : BaseSerializedObjectToggleSection
        {
            private SerializedProperty _randomDegrees;

            protected override GUIContent Title
            {
                get { return new GUIContent("Randomize Orientation"); }
            }

            protected override void DrawContent()
            {
                EditorGUILayout.Slider(_randomDegrees, 0, 180);
            }

            public override void OnEnable(SerializedObject target)
            {
                base.OnEnable(target);

                _randomDegrees = target.FindProperty("_randomizeOrientation._randomDegrees");
            }

            protected override SerializedProperty FindEnabledProperty(SerializedObject target)
            {
                return target.FindProperty("_randomizeOrientation._enabled");
            }
        }

        private class ImpactVelocitySettings
            : BaseSerializedObjectToggleSection
        {
            private SerializedProperty _offset;
            private SerializedProperty _scale;

            protected override GUIContent Title
            {
                get { return new GUIContent("Impact Velocity", "Stretch and align the decal with the impact velocity of the particle."); }
            }

            protected override void DrawContent()
            {
                EditorGUILayout.CurveField(_scale, Color.red, new Rect(new Vector2(0, 1), new Vector2(1, 2)));
                EditorGUILayout.CurveField(_offset, Color.red, new Rect(new Vector2(0, 0), new Vector2(2, 2)));
            }

            public override void OnEnable(SerializedObject target)
            {
                base.OnEnable(target);

                _scale = target.FindProperty("_impactVelocity._scale");
                _offset = target.FindProperty("_impactVelocity._offset");
            }

            protected override SerializedProperty FindEnabledProperty(SerializedObject target)
            {
                return target.FindProperty("_impactVelocity._enabled");
            }
        }

        #endregion

        private readonly IComponent<SerializedObject>[] _sections;

        public ParticleWetSplatterEditor()
        {
            _sections = new IComponent<SerializedObject>[] {
                new CoreSettings { Expanded = true },
                new LimitSettings(),
                new LifetimeSettings(),
                new RecyclingSettings(),
                new RandomizeSizeSettings(),
                new RandomizeOrientationSettings(),
                new ImpactVelocitySettings()
            };
        }

        private void OnEnable()
        {
            foreach (var section in _sections)
                section.OnEnable(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Space(5);

            foreach (var section in _sections)
                section.Draw();

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.HelpBox("Add one or more `Particle Wet Splatter Template` components to define the decals placed by this splatter component", MessageType.Info);
        }
    }
}