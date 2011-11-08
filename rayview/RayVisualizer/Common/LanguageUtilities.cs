using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public struct Union <T1, T2>
    {
        private T1 _asT1;
        private T2 _asT2;
        private bool _isT1;
        public bool IsT1 { get { return _isT1; } }
        public T1 AsFirst { get { return _asT1; } }
        public T2 AsSecond { get { return _asT2; } }

        public Union(T1 val)
        {
            _asT1 = val;
            _asT2 = default(T2);
            _isT1 = true;
        }

        public Union(T2 val)
        {
            _asT2 = val;
            _asT1 = default(T1);
            _isT1 = false;
        }

        public Ret Run<Ret>(Func<T1,Ret> ifT1, Func<T2,Ret> ifT2)
        {
            if (_isT1)
                return ifT1(_asT1);
            else
                return ifT2(_asT2);
        }
    }
}
