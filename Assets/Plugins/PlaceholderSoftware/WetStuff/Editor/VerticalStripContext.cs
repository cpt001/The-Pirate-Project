using System;
using UnityEditor;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    internal struct VerticalStripContext
        : IDisposable
    {
        private readonly Color _color;
        private readonly float _xOffset;
        private readonly float _minHeight;
        private readonly float _tickWidth;
        private readonly float _lineWidth;
        private readonly Rect _top;

        public VerticalStripContext(Color color, float xOffset = 2, float minHeight = 24, float tickWidth = 8, float lineWidth = 1)
        {
            _color = color;
            _xOffset = xOffset;
            _minHeight = minHeight;
            _tickWidth = tickWidth;
            _lineWidth = lineWidth;
            _top = EditorGUILayout.GetControlRect(false, 0);
        }

        public void Dispose()
        {
            var bot = EditorGUILayout.GetControlRect(false, 0);

            var topTick = new Rect(_top.xMin + _xOffset, _top.yMax + EditorGUIUtility.singleLineHeight / 2, _tickWidth, _lineWidth);
            var botTick = new Rect(_top.xMin + _xOffset, bot.yMax - EditorGUIUtility.singleLineHeight / 2, _tickWidth, _lineWidth);
            var height = bot.yMax - _top.yMin;
            var vertical = new Rect(_top.xMin, topTick.yMin, _lineWidth, botTick.yMax - topTick.yMin);
            var midTick = new Rect(_top.xMin, _top.yMin + height / 2 - _lineWidth / 2, _tickWidth, _lineWidth);

            if (height > _minHeight)
            {
                EditorGUI.DrawRect(topTick, _color);
                EditorGUI.DrawRect(botTick, _color);
                EditorGUI.DrawRect(vertical, _color);
            }
            else
                EditorGUI.DrawRect(midTick, _color);
        }
    }
}