using System;
using UnityEditor;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Components
{
    /// <summary>
    /// Base class for sections in a UI which can be enabled/disabled/expanded/collapsed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseSection<T>
        : IComponent<T>
    {
        public virtual bool Expanded { get; set; }
        protected abstract bool Enabled { get; set; }

        [NotNull]
        protected abstract GUIContent Title { get; }

        protected T Target { get; private set; }

        public virtual bool IsVisible
        {
            get { return true; }
        }

        public virtual void OnEnable([NotNull] T target)
        {
            Target = target;
        }

        public void Draw()
        {
            using (new EditorGUILayout.VerticalScope(Expanded ? Styles.ContentOutline : GUIStyle.none))
            {
                Expanded = DrawHeader(Title);
                if (Expanded)
                    using (new EditorGUI.DisabledScope(!Enabled))
                    using (new EditorGUILayout.VerticalScope(Styles.LeftPadding))
                        DrawContent();
            }
        }

        /// <summary>
        ///     Draw the header for this section
        /// </summary>
        /// <param name="title"></param>
        /// <returns>Return whether the rest of the section content should be drawn</returns>
        protected abstract bool DrawHeader([NotNull] GUIContent title);

        protected abstract void DrawContent();
    }

    public abstract class BasicSection<T>
        : BaseSection<T>
    {
        protected override bool Enabled
        {
            get { return true; }
            set { throw new NotSupportedException(); }
        }

        protected override bool DrawHeader(GUIContent title)
        {
            EditorGuiHelper2.TitleHeader(title);
            return true;
        }
    }

    public abstract class BasicSerializedObjectSection
        : BasicSection<SerializedObject>
    {
    }

    public abstract class BaseFoldoutSection<T>
        : BaseSection<T>
    {
        protected override bool Enabled
        {
            get { return true; }
            set { throw new NotSupportedException(); }
        }

        protected override bool DrawHeader(GUIContent title)
        {
            return EditorGuiHelper2.FoldHeader(title, Expanded, GetHeaderMenu(), GetHelpUrl());
        }

        [CanBeNull]
        protected virtual GenericMenu GetHeaderMenu()
        {
            return null;
        }

        [CanBeNull]
        protected virtual string GetHelpUrl()
        {
            return null;
        }
    }

    public abstract class BaseSerializedObjectFoldoutSection
        : BaseFoldoutSection<SerializedObject>
    {
        private bool _expandedField;
        private SerializedProperty _expandedProperty;

        public override bool Expanded
        {
            get { return _expandedProperty == null ? _expandedField : _expandedProperty.boolValue; }
            set
            {
                if (_expandedProperty == null)
                    _expandedField = value;
                else
                    _expandedProperty.boolValue = value;
            }
        }

        public override void OnEnable(SerializedObject target)
        {
            base.OnEnable(target);

            _expandedProperty = FindExpandedProperty(target);
        }

        [CanBeNull]
        protected abstract SerializedProperty FindExpandedProperty([NotNull] SerializedObject obj);
    }

    internal abstract class BaseToggleSection<T>
        : BaseSection<T>
    {
        protected override bool DrawHeader(GUIContent title)
        {
            var expanded = Expanded;
            var enabled = Enabled;

            EditorGuiHelper2.CheckHeader(title, ref enabled, ref expanded, GetHeaderMenu(), GetHelpUrl());

            Enabled = enabled;
            return expanded;
        }

        [CanBeNull]
        protected virtual GenericMenu GetHeaderMenu()
        {
            return null;
        }

        [CanBeNull]
        protected virtual string GetHelpUrl()
        {
            return null;
        }
    }

    internal abstract class BaseSerializedObjectToggleSection
        : BaseToggleSection<SerializedObject>
    {
        private SerializedProperty _enabled;

        protected override bool Enabled
        {
            get { return _enabled.boolValue; }
            set { _enabled.boolValue = value; }
        }

        public override void OnEnable(SerializedObject target)
        {
            base.OnEnable(target);

            _enabled = FindEnabledProperty(target);
        }

        [NotNull]
        protected abstract SerializedProperty FindEnabledProperty([NotNull] SerializedObject obj);
    }

    internal class DefaultInspectorSection
        : BaseFoldoutSection<SerializedObject>
    {
        private Editor _editor;

        protected override GUIContent Title
        {
            get { return new GUIContent("Default Inspector"); }
        }

        protected override void DrawContent()
        {
            if (_editor == null)
                _editor = Editor.CreateEditor(Target.targetObjects);
            _editor.DrawDefaultInspector();
        }
    }
}