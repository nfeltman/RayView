using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Simd;

namespace RayVisualizer.Common
{
    public interface TriangleAggregator<Aggregate>
    {
        void InplaceOp(ref Aggregate val, BuildTriangle t);
        Aggregate Op(Aggregate val1, Aggregate val2);
        void InplaceOp(ref Aggregate val1, Aggregate val2);
        Aggregate GetIdentity();
        Aggregate Roll(BuildTriangle[] tris, int start, int end);
    }

    public class CountAggregator : TriangleAggregator<int>
    {
        public static readonly CountAggregator ONLY = new CountAggregator();

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

        public void InplaceOp(ref int val, BuildTriangle t)
        {
            val++;
        }

        public int Roll(BuildTriangle[] tris, int start, int end)
        {
            return end - start;
        }
    }

    public class BoundsCountAggregator : TriangleAggregator<BoundAndCount>
    {
        public static readonly BoundsCountAggregator ONLY = new BoundsCountAggregator();

        private BoundsCountAggregator() { }

        public BoundAndCount GetIdentity()
        {
            return new BoundAndCount(0, Box3.EMPTY);
        }

        public BoundAndCount Op(BoundAndCount val1, BoundAndCount val2)
        {
            return new BoundAndCount(val1.Count + val2.Count, val1.Box | val2.Box); 
        }

        public void InplaceOp(ref BoundAndCount val, BuildTriangle t)
        {
            Vector4f min;
            Vector4f max;
            Vector4f point = new Vector4f(t.t.p1.x, t.t.p1.y, t.t.p1.z, 0f);
            if (val.Box.IsEmpty)
            {
                min = point;
                max = point;
            }
            else
            {
                min = val.Box.Min;
                max = val.Box.Max;
                min = min.Min(point);
                max = max.Max(point);
            }
            point = new Vector4f(t.t.p2.x, t.t.p2.y, t.t.p2.z, 0f);
            min = min.Min(point);
            max = max.Max(point);
            point = new Vector4f(t.t.p3.x, t.t.p3.y, t.t.p3.z, 0f);
            min = min.Min(point);
            max = max.Max(point);
            val = new BoundAndCount(val.Count + 1, new Box3(min, max));
        }

        public void InplaceOp(ref BoundAndCount val1, BoundAndCount val2)
        {
            val1.Box = val1.Box | val2.Box;
            val1.Count += val2.Count;
        }

        public BoundAndCount Roll(BuildTriangle[] tris, int start, int end)
        {
            BoundBuilder builder = new BoundBuilder(true);
            for (int k = start; k < end; k++)
                builder.AddTriangle(tris[k].t);
            return new BoundAndCount(end - start, builder.GetBox());
        }
    }

    public struct BoundAndCount
    {
        public int Count;
        public Box3 Box;

        public BoundAndCount(int count, Box3 builder)
        {
            Count = count;
            Box = builder;
        }
    }
}
