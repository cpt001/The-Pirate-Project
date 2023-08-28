using System;
using System.Collections.Generic;

namespace PlaceholderSoftware.WetStuff.Datastructures
{
    /// <summary>
    ///     A pool of resources which will always return an instance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Pool<T>
        : IRecycler<T>
        where T : class
    {
        private readonly Func<T> _factory;
        private readonly Stack<T> _items;
        private readonly int _maxSize;

        public int Count
        {
            get { return _items.Count; }
        }

        public int Capacity
        {
            get { return _maxSize; }
        }

        public Pool(int maxSize, Func<T> factory)
        {
            _maxSize = maxSize;
            _factory = factory;
            _items = new Stack<T>(maxSize);
        }

        void IRecycler<T>.Recycle([NotNull] T item)
        {
            Put(item);
        }

        /// <summary>
        ///     Get an item from this pool
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            if (_items.Count > 0)
                return _items.Pop();

            return _factory();
        }

        /// <summary>
        ///     Return an item to the pool
        /// </summary>
        /// <param name="item"></param>
        public void Put([NotNull] T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (_items.Count < _maxSize)
                _items.Push(item);
        }
    }

    public interface IRecycler<in T> where T : class
    {
        void Recycle(T item);
    }
}