using System;

namespace PlaceholderSoftware.WetStuff.Timeline.Mixers
{
    internal class SingleMixer
        : BaseMixer<float>
    {
        private float _sum;
        private float _weight;

        public override void Start()
        {
            base.Start();

            _weight = 0;
            _sum = 0;
        }

        protected override void MixImpl(float weight, float data)
        {
            _sum += data * weight;
            _weight += weight;
        }

        protected override float GetResult(float defaultValue)
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