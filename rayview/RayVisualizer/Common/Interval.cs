using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    struct ClosedInterval
    {
        private float _min, _max;
        public static readonly ClosedInterval ALL = new ClosedInterval(float.NegativeInfinity, float.PositiveInfinity);
        public static readonly ClosedInterval EMPTY = new ClosedInterval(float.PositiveInfinity, float.NegativeInfinity);
        public static readonly ClosedInterval POSITIVES = new ClosedInterval(0, float.PositiveInfinity);
        public static readonly ClosedInterval NEGATIVES = new ClosedInterval(float.NegativeInfinity, 0);
        public float Min { get { return _min; } }
        public float Max { get { return _max; } }
        public bool IsEmpty { get { return _min > _max; } }

        public ClosedInterval(float min, float max)
        {
            _min = min;
            _max = max;
        }

        public bool Contains(float val)
        {
            return val >= _min && val <= _max;
        }

        public static ClosedInterval operator &(ClosedInterval i1, ClosedInterval i2)
        {
            return new ClosedInterval(Math.Max(i1._min, i2._min), Math.Min(i1._max, i2._max));
        }

        public static ClosedInterval operator +(ClosedInterval i1, float m)
        {
            return new ClosedInterval(i1._min + m, i1._max + m);
        }

        public static ClosedInterval operator -(ClosedInterval i1, float m)
        {
            return new ClosedInterval(i1._min - m, i1._max - m);
        }

        /// <summary>
        /// "val is in T*m" iff "val = u*m for some u in T"
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static ClosedInterval operator *(ClosedInterval i1, float m)
        {
            if (i1.IsEmpty)
                return i1;
            if (m < 0)
                return new ClosedInterval(i1._max * m, i1._min * m);
            if (m == 0)
                return new ClosedInterval(0, 0);
            return new ClosedInterval(i1._min * m, i1._max * m);
        }

        /// <summary>
        /// "val is in T/m" iff "m*val is in T" 
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static ClosedInterval operator /(ClosedInterval i1, float d)
        {
            if (d == 0)
                return i1.Contains(0) ? ALL : EMPTY;
            if (d > 0)
                return new ClosedInterval(i1._min / d, i1._max / d);
            return new ClosedInterval(i1._max / d, i1._min / d);
        }

        /// <summary>
        /// Every point in i1 is less than every point in i2.
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <returns></returns>
        public static bool operator <<(ClosedInterval i1, ClosedInterval i2)
        {
            return (i1._max < i2._min || i1.IsEmpty || i2.IsEmpty);
        }

        /// <summary>
        /// Every point in i1 is greater than every point in i2.
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <returns></returns>
        public static bool operator >>(ClosedInterval i1, ClosedInterval i2)
        {
            return (i1._min > i2._max || i1.IsEmpty || i2.IsEmpty);
        }
    }
}
