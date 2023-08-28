using UnityEditor;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    public static class Styles
    {
        public static readonly GUIStyle tickStyleRight;
        public static readonly GUIStyle tickStyleLeft;
        public static readonly GUIStyle tickStyleCenter;

        public static readonly GUIStyle preSlider;
        public static readonly GUIStyle preSliderThumb;
        public static readonly GUIStyle preButton;
        public static readonly GUIStyle preDropdown;

        public static readonly GUIStyle preLabel;
        public static readonly GUIStyle hueCenterCursor;
        public static readonly GUIStyle hueRangeCursor;

        public static readonly GUIStyle centeredBoldLabel;
        public static readonly GUIStyle wheelThumb;
        public static readonly Vector2 wheelThumbSize;

        public static readonly GUIStyle header;
        public static readonly GUIStyle headerCheckbox;
        public static readonly GUIStyle headerFoldout;

        public static readonly Texture2D playIcon;
        public static readonly Texture2D checkerIcon;
        public static readonly Texture2D paneOptionsIcon;
        public static readonly Texture2D helpIcon;

        public static readonly GUIStyle centeredMiniLabel;

        public static readonly GUIStyle LeftPadding;
        public static readonly GUIStyle ContentOutline;

        static Styles()
        {
            tickStyleRight = new GUIStyle("Label") {
                alignment = TextAnchor.MiddleRight,
                fontSize = 9
            };

            tickStyleLeft = new GUIStyle("Label") {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 9
            };

            tickStyleCenter = new GUIStyle("Label") {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9
            };

            preSlider = new GUIStyle("PreSlider");
            preSliderThumb = new GUIStyle("PreSliderThumb");
            preButton = new GUIStyle("PreButton");
            preDropdown = new GUIStyle("preDropdown");

            preLabel = new GUIStyle("ShurikenLabel");

            hueCenterCursor = new GUIStyle("ColorPicker2DThumb") {
                normal = {
                    background = (Texture2D) EditorGUIUtility.LoadRequired("Builtin Skins/DarkSkin/Images/ShurikenPlus.png")
                },
                fixedWidth = 6,
                fixedHeight = 6
            };

            hueRangeCursor = new GUIStyle(hueCenterCursor) {
                normal = {
                    background = (Texture2D) EditorGUIUtility.LoadRequired("Builtin Skins/DarkSkin/Images/CircularToggle_ON.png")
                }
            };

            wheelThumb = new GUIStyle("ColorPicker2DThumb");

            centeredBoldLabel = new GUIStyle(GUI.skin.GetStyle("Label")) {
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold
            };

            centeredMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel) {
                alignment = TextAnchor.UpperCenter
            };

            wheelThumbSize = new Vector2(
                !Mathf.Approximately(wheelThumb.fixedWidth, 0f) ? wheelThumb.fixedWidth : wheelThumb.padding.horizontal,
                !Mathf.Approximately(wheelThumb.fixedHeight, 0f) ? wheelThumb.fixedHeight : wheelThumb.padding.vertical
            );

            header = new GUIStyle("ShurikenModuleTitle") {
                font = new GUIStyle("Label").font,
                border = new RectOffset(15, 7, 4, 4),
                fixedHeight = 22,
                contentOffset = new Vector2(20f, -2f)
            };

            headerCheckbox = new GUIStyle("ShurikenCheckMark");
            headerFoldout = new GUIStyle("Foldout");

            playIcon = (Texture2D) EditorGUIUtility.LoadRequired("Builtin Skins/DarkSkin/Images/IN foldout act.png");
            checkerIcon = (Texture2D) EditorGUIUtility.LoadRequired("Icons/CheckerFloor.png");
            helpIcon = (Texture2D) EditorGUIUtility.IconContent("_Help").image;

            if (EditorGUIUtility.isProSkin)
                paneOptionsIcon = (Texture2D) EditorGUIUtility.LoadRequired("Builtin Skins/DarkSkin/Images/pane options.png");
            else
                paneOptionsIcon = (Texture2D) EditorGUIUtility.LoadRequired("Builtin Skins/LightSkin/Images/pane options.png");

            LeftPadding = new GUIStyle {
                padding = new RectOffset(15, 0, 0, 0)
            };

            ContentOutline = new GUIStyle(EditorStyles.helpBox) {
                padding = new RectOffset(0, 0, 0, 1),
                margin = new RectOffset(0, 0, 0, 3)
            };
        }
    }
}