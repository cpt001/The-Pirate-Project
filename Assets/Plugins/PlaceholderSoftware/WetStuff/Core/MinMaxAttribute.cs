using UnityEngine;

namespace PlaceholderSoftware.WetStuff
{
    public class MinMaxAttribute : PropertyAttribute
    {
        public float MaxLimit = 1;
        public float MinLimit;
        public bool ShowDebugValues;
        public bool ShowEditRange;

        public MinMaxAttribute(int min, int max)
        {
            MinLimit = min;
            MaxLimit = max;
        }
    }
}