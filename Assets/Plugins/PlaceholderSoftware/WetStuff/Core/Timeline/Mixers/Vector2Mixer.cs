using System;
using UnityEngine;

namespace PlaceholderSoftware.WetStuff.Timeline.Mixers
{
    internal class Vector2Mixer
        : BaseMixer<Vector2>
    {
        private Vector2 _sum;
        private float _weight;

        public override void Start()
        {
            base.Start();

            _weight = 0;
            _sum = Vector2.zero;
        }

        protected override void MixImpl(float weight, Vector2 data)
        {
            _sum += data * weight;
            _weight += weight;
        }

        protected override Vector2 GetResult(Vector2 defaultValue)
        {
            // If weight is exactly right then just return the value as is
            if (Math.Abs(_weight - 1) < float.Epsilon)
                return _sum;

            // If the weight is too large normalise the accumulated value
            if (_weight > 1)
                return _sum * (1 / _weight);

            // Weight must be too small, mix in the default value
            return _sum + defaultValue * (1f - _weight);
        }
    }
}