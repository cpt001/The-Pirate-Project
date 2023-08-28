// https://gist.github.com/LotteMakesStuff/0de9be35044bab97cbe79b9ced695585

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    [CustomPropertyDrawer(typeof(MinMaxAttribute))]
    public class MinMaxDrawer
        : PropertyDrawer
    {
        public override void OnGUI(Rect position, [NotNull] SerializedProperty property, [NotNull] GUIContent label)
        {
            // Cast the attribute to make life easier
            var minMax = (MinMaxAttribute) attribute;

            // Try to retrieve the tooltip
            var tooltipAttr = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), false).OfType<TooltipAttribute>().FirstOrDefault();
            if (tooltipAttr != null)
                label.tooltip = tooltipAttr.tooltip;

            // This only works on a vector2 field! print error and early exit
            if (property.propertyType != SerializedPropertyType.Vector2)
            {
                EditorGUI.HelpBox(position, "MinMax can only be used with `Vector2` field", MessageType.Error);
                return;
            }

            // if we are flagged to draw in a special mode, lets modify the drawing rectangle to draw only one line at a time
            if (minMax.ShowDebugValues || minMax.ShowEditRange)
                position = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // Pull out a bunch of helpful min/max values....
            var minValue = property.vector2Value.x; // The currently set minimum and maximum value
            var maxValue = property.vector2Value.y;
            var minLimit = minMax.MinLimit; // The limit for both min and max, min cant go lower than minLimit and maax cant top maxLimit
            var maxLimit = minMax.MaxLimit;

            // And ask unity to draw them all nice for us!
            EditorGUI.MinMaxSlider(position, label, ref minValue, ref maxValue, minLimit, maxLimit);

            // Save results into property
            property.vector2Value = new Vector2(minValue, maxValue);

            // Do we have a special mode flagged? time to draw lines!
            if (minMax.ShowDebugValues || minMax.ShowEditRange)
            {
                var isEditable = minMax.ShowEditRange;

                if (!isEditable)
                    GUI.enabled = false; // if were just in debug mode and not edit mode, make sure all the UI is read only!

                // Move the draw rect on by one line
                position.y += EditorGUIUtility.singleLineHeight;

                var val = new Vector4(minLimit, minValue, maxValue, maxLimit); // Shove the values and limits into a vector4 and draw them all at once
                val = EditorGUI.Vector4Field(position, "MinLimit/MinVal/MaxVal/MaxLimit", val);

                GUI.enabled = false; // The range part is always read only
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.FloatField(position, "Selected Range", maxValue - minValue);
                GUI.enabled = true; // Remember to make the UI editable again!

                if (isEditable)
                    property.vector2Value = new Vector2(val.y, val.z); // Save off any change to the value~
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var minMax = (MinMaxAttribute) attribute;

            // By default just return the standard line height
            var size = EditorGUIUtility.singleLineHeight;

            // If we have a special mode, add two extra lines!
            if (minMax.ShowEditRange || minMax.ShowDebugValues)
                size += EditorGUIUtility.singleLineHeight * 2;

            return size;
        }
    }
}