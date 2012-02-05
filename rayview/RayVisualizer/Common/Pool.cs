using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class Pool<T>
        where T : class // There's no point in a pool for value types.
    {
        private Stack<T> _available;
        private Func<T> _factory;
        private int _numUsed;
        private int _constructorInvocations;

        public int NumAvailable { get { return _available.Count; } }
        public int NumInvocations { get { return _constructorInvocations; } }
        public int NumUsed { get { return _numUsed; } }

        public Pool(Func<T> factory)
        {
            _available = new Stack<T>();
            _factory = factory;
        }

        public T GetItem()
        {
            _numUsed++;
            if (_available.Count == 0)
            {
                _constructorInvocations++;
                return _factory();
            }
            return _available.Pop();
        }

        public void ReturnItem(ref T item)
        {
            _numUsed--;
            _available.Push(item);
            item = null;
        }
    }
}
