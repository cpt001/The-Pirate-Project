namespace PlaceholderSoftware.WetStuff.Timeline.Mixers
{
    internal class BoolMixer
        : BaseMixer<bool>
    {
        private float _false;
        private float _true;

        public override void Start()
        {
            base.Start();

            _true = 0;
            _false = 0;
        }

        protected override void MixImpl(float weight, bool data)
        {
            if (data)
                _true += weight;
            else
                _false += weight;
        }

        protected override bool GetResult(bool defaultValue)
        {
            return _true > _false;
        }
    }
}