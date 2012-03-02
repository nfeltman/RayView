using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Simd;

namespace RayVisualizer.Common
{
    public interface TriangleAggregator<Aggregate>
    {
        Aggregate Op(Aggregate val1, Aggregate val2);
        void InplaceOp(ref Aggregate val1, Aggregate val2);
        Aggregate GetIdentity();
        bool IsIdentity(Aggregate agg);
    }
    public interface TriangleAggregator<Aggregate, Tri> : TriangleAggregator<Aggregate>
    {
        void InplaceOp(ref Aggregate val, Tri t);
        void InplaceOp3(ref Aggregate val1, ref Aggregate val2, ref Aggregate val3, Tri t); // to alleviate the cost of a virtual call

        Aggregate Roll(Tri[] tris, int start, int end);
        Aggregate GetVal(Tri t);
    }

    public class CountAggregator<Tri> : TriangleAggregator<int, Tri>
    {
        public static readonly CountAggregator<Tri> ONLY = new CountAggregator<Tri>();

        private CountAggregator() { }

        public int Op(int val1, int val2)
        {
            return val1 + val2;
        }

        public void InplaceOp(ref int val1, int val2)
        {
            val1 += val2;
        }

        public int GetIdentity()
        {
            return 0;
        }

        public void InplaceOp(ref int val, Tri t)
        {
            val++;
        }

        public int Roll(Tri[] tris, int start, int end)
        {
            return end - start;
        }

        public void InplaceOp3(ref int val1, ref int val2, ref int val3, Tri t)
        {
            val1 = val1 + 1;
            val2 = val2 + 1;
            val3 = val3 + 1;
        }

        public int GetVal(Tri t)
        {
            return 1;
        }


        public bool IsIdentity(int agg)
        {
            return agg == 0;
        }
    }

    public class BoundsCountAggregator<Tri> : TriangleAggregator<BoundAndCount, Tri>
            where Tri : Bounded
    {
        public static readonly BoundsCountAggregator<Tri> ONLY = new BoundsCountAggregator<Tri>();

        private BoundsCountAggregator() { }

        public BoundAndCount GetIdentity()
        {
            return new BoundAndCount(0, new Vector4f(float.MaxValue, float.MaxValue, float.MaxValue, 0), new Vector4f(float.MinValue, float.MinValue, float.MinValue, 0));
        }

        public BoundAndCount Op(BoundAndCount val1, BoundAndCount val2)
        {
            return new BoundAndCount(val1.Count + val2.Count, val1._min.Min(val2._min), val1._max.Max(val2._max)); 
        }

        public void InplaceOp(ref BoundAndCount val, Tri t)
        {
            Tuple<Vector4f, Vector4f> bounds = t.Bounds;

            val = val.Box.IsEmpty ? new BoundAndCount(val.Count + 1, bounds.Item1, bounds.Item2) : new BoundAndCount(val.Count + 1, val._min.Min(bounds.Item1), val._max.Max(bounds.Item2));
        }

        public void InplaceOp(ref BoundAndCount val1, BoundAndCount val2)
        {
            val1._min = val1._min.Min(val2._min);
            val1._max = val1._max.Max(val2._max);
            val1.Count += val2.Count;
        }

        public BoundAndCount Roll(Tri[] tris, int start, int end)
        {
            Tuple<Vector4f, Vector4f> bounds0 = tris[start].Bounds;
            Vector4f min=bounds0.Item1, max = bounds0.Item2;
            for (int k = start + 1; k < end; k++)
            {
                Tuple<Vector4f, Vector4f> bounds = tris[k].Bounds;
                min = min.Min(bounds.Item1);
                max = max.Max(bounds.Item2);
            }
            return new BoundAndCount(end - start, min, max);
        }

        public BoundAndCount GetVal(Tri t)
        {
            Tuple<Vector4f, Vector4f> bounds = t.Bounds;
            return new BoundAndCount(1, bounds.Item1, bounds.Item2);
        }

        public void InplaceOp3(ref BoundAndCount val1, ref BoundAndCount val2, ref BoundAndCount val3, Tri t)
        {
            Tuple<Vector4f, Vector4f> bounds = t.Bounds;

            val1 = val1.Box.IsEmpty ? new BoundAndCount(val1.Count + 1, bounds.Item1, bounds.Item2) : new BoundAndCount(val1.Count + 1, val1._min.Min(bounds.Item1), val1._max.Max(bounds.Item2));
            val2 = val2.Box.IsEmpty ? new BoundAndCount(val2.Count + 1, bounds.Item1, bounds.Item2) : new BoundAndCount(val2.Count + 1, val2._min.Min(bounds.Item1), val2._max.Max(bounds.Item2));
            val3 = val3.Box.IsEmpty ? new BoundAndCount(val3.Count + 1, bounds.Item1, bounds.Item2) : new BoundAndCount(val3.Count + 1, val3._min.Min(bounds.Item1), val3._max.Max(bounds.Item2));
        }

        public bool IsIdentity(BoundAndCount agg)
        {
            return agg.Count == 0;
        }
    }

    public struct BoundAndCount
    {
        public int Count;
        public Box3 Box { get { return new Box3(_min, _max); } }
        public Vector4f _min, _max;

        public BoundAndCount(int count, Vector4f min, Vector4f max)
        {
            Count = count;
            _min = min;
            _max = max;
        }
    }
}
