using System;

namespace PlaceholderSoftware.WetStuff.Timeline.Mixers
{
    internal abstract class BaseMixer<T>
    {
        private bool _started;

        public virtual void Start()
        {
            _started = true;
        }

        public void Mix(float weight, T value)
        {
            if (!_started)
                throw new InvalidOperationException("Cannot add mix input before starting mixer");

            MixImpl(weight, value);
        }

        public T Result(T defaultValue)
        {
            if (!_started)
                throw new InvalidOperationException("Cannot get mix result before starting mixer");
            _started = false;

            return GetResult(defaultValue);
        }

        protected abstract void MixImpl(float weight, T data);

        protected abstract T GetResult(T defaultValue);
    }
}