using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            return new BoundAndCount(0, new BoundBuilder(true));
        }

        public BoundAndCount Op(BoundAndCount val1, BoundAndCount val2)
        {
            BoundBuilder builder = new BoundBuilder(val1.Builder);
            builder.AddBox(val2.Builder);
            return new BoundAndCount(val1.Count + val2.Count, builder);
        }

        public void InplaceOp(ref BoundAndCount val, BuildTriangle t)
        {
            val.Builder.AddTriangle(t.t);
            val.Count++;
        }

        public void InplaceOp(ref BoundAndCount val1, BoundAndCount val2)
        {
            val1.Builder.AddBox(val2.Builder);
            val1.Count += val2.Count;
        }

        public BoundAndCount Roll(BuildTriangle[] tris, int start, int end)
        {
            BoundBuilder builder = new BoundBuilder();
            for (int k = start; k < end; k++)
                builder.AddTriangle(tris[k].t);
            return new BoundAndCount(end - start, builder);
        }
    }

    public struct BoundAndCount
    {
        public int Count;
        public BoundBuilder Builder;

        public BoundAndCount(int count, BoundBuilder builder)
        {
            Count = count;
            Builder = builder;
        }
    }
}
