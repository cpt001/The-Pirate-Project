using System;

namespace PlaceholderSoftware.WetStuff
{
    /// <summary>
    ///     A type with reference counted shared state.
    /// </summary>
    /// <typeparam name="T">The type of state shared between instances.</typeparam>
    internal abstract class RcSharedState<T> : IDisposable
        where T : IDisposable
    {
        private static T _sharedState;
        private static uint _count;

        protected T SharedState
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(GetType().Name, "Cannot access shared state on disposed object");

                return _sharedState;
            }
        }

        public bool IsDisposed { get; private set; }

        protected RcSharedState(Func<T> factory)
        {
            var id = _count++;
            if (id == 0) _sharedState = factory();
        }

        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                var remaining = --_count;
                if (remaining == 0)
                {
                    _sharedState.Dispose();
                    _sharedState = default(T);
                }

                IsDisposed = true;
            }
        }
    }
}