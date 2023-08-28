using UnityEditor;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    internal static class EditorGuiHelper2
    {
        private static void Header(GUIContent title, out Rect rect, [CanBeNull] GenericMenu details, [CanBeNull] string helpUrl)
        {
            rect = GUILayoutUtility.GetRect(16f, 22f, Styles.header);
            GUI.Box(rect, title, Styles.header);

            // Calculate the rectangles for the two icons of the right hand side
            var detailRect = new Rect(rect.x + rect.width - Styles.paneOptionsIcon.width - 5f, rect.y + Styles.paneOptionsIcon.height / 2f + 1f, Styles.paneOptionsIcon.width, Styles.paneOptionsIcon.height);
            var helpRect = new Rect(rect.x + rect.width - Styles.helpIcon.width - 5f, rect.y + Styles.helpIcon.height / 2f, Styles.helpIcon.width, Styles.helpIcon.height);

            // Display the icon for a generic menu dropdown on the right
            if (details != null)
            {
                // Bump the help icon over to the left
                helpRect.x -= detailRect.width + 2;

                GUI.DrawTexture(detailRect, Styles.paneOptionsIcon);

                // Check if the user clicked the menu icon (expand rect to the full height of the header)
                var e = Event.current;
                var detailClick = new Rect(detailRect.xMin, rect.yMin, detailRect.width, rect.height);
                if (e.type == EventType.MouseDown && detailClick.Contains(e.mousePosition))
                {
                    details.ShowAsContext();
                    e.Use();
                }
            }

            // Display the help icon
            if (helpUrl != null)
            {
                GUI.DrawTexture(helpRect, Styles.helpIcon);

                // Check if the user clicked the help icon (expand rect to the full height of the header)
                var e = Event.current;
                var helpClick = new Rect(helpRect.xMin, rect.yMin, helpRect.width, rect.height);
                if (e.type == EventType.MouseDown && helpClick.Contains(e.mousePosition))
                {
                    Application.OpenURL(helpUrl);
                    e.Use();
                }
            }
        }

        /// <summary>
        ///     Display a header with a title
        /// </summary>
        /// <param name="title"></param>
        /// <param name="detailMenu"></param>
        /// <param name="helpUrl"></param>
        internal static void TitleHeader(GUIContent title, [CanBeNull] GenericMenu detailMenu = null, [CanBeNull] string helpUrl = null)
        {
            Rect rect;
            Header(title, out rect, detailMenu, helpUrl);
        }

        /// <summary>
        ///     Display a header bar with a checkbox
        /// </summary>
        /// <param name="title"></param>
        /// <param name="ticked"></param>
        /// <param name="expanded"></param>
        /// <param name="detailMenu"></param>
        /// <param name="helpUrl"></param>
        /// <returns></returns>
        internal static void CheckHeader(GUIContent title, ref bool ticked, ref bool expanded, [CanBeNull] GenericMenu detailMenu = null, [CanBeNull] string helpUrl = null)
        {
            Rect rect;
            Header(title, out rect, detailMenu, helpUrl);

            var toggleRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);
            var e = Event.current;

            if (e.type == EventType.Repaint)
                Styles.headerCheckbox.Draw(toggleRect, false, false, ticked, false);

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                if (toggleRect.Contains(e.mousePosition))
                {
                    ticked = !ticked;

                    if (ticked)
                        expanded = true;
                }
                else
                    expanded = !expanded;

                e.Use();
            }
        }

        /// <summary>
        ///     Display a header bar with a fold arrow
        /// </summary>
        /// <param name="title"></param>
        /// <param name="folded"></param>
        /// <param name="detailMenu"></param>
        /// <param name="helpUrl"></param>
        /// <returns></returns>
        internal static bool FoldHeader(GUIContent title, bool folded, [CanBeNull] GenericMenu detailMenu = null, [CanBeNull] string helpUrl = null)
        {
            Rect rect;
            Header(title, out rect, detailMenu, helpUrl);

            var foldoutRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            var e = Event.current;

            if (e.type == EventType.Repaint)
                Styles.headerFoldout.Draw(foldoutRect, false, false, folded, false);

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                folded = !folded;
                e.Use();
            }

            return folded;
        }
    }
}