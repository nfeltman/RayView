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
            where Tri : TriangleContainer
    {
        public static readonly BoundsCountAggregator<Tri> ONLY = new BoundsCountAggregator<Tri>();

        private BoundsCountAggregator() { }

        public BoundAndCount GetIdentity()
        {
            return new BoundAndCount(0, Box3.EMPTY);
        }

        public BoundAndCount Op(BoundAndCount val1, BoundAndCount val2)
        {
            return new BoundAndCount(val1.Count + val2.Count, val1.Box | val2.Box); 
        }

        public void InplaceOp(ref BoundAndCount val, Tri t)
        {
            Vector4f point1 = new Vector4f(t.T.p1.x, t.T.p1.y, t.T.p1.z, 0f);
            Vector4f point2 = new Vector4f(t.T.p2.x, t.T.p2.y, t.T.p2.z, 0f);
            Vector4f point3 = new Vector4f(t.T.p3.x, t.T.p3.y, t.T.p3.z, 0f);

            Vector4f triMax = point1.Max(point2).Max(point3);
            Vector4f triMin = point1.Min(point2).Min(point3);

            val = new BoundAndCount(val.Count + 1, val.Box.IsEmpty ? new Box3(triMin, triMax) : new Box3(val.Box.Min.Min(triMin), val.Box.Max.Max(triMax)));
        }

        public void InplaceOp(ref BoundAndCount val1, BoundAndCount val2)
        {
            val1.Box = val1.Box | val2.Box;
            val1.Count += val2.Count;
        }

        public BoundAndCount Roll(Tri[] tris, int start, int end)
        {
            BoundBuilder builder = new BoundBuilder(true);
            for (int k = start; k < end; k++)
                builder.AddTriangle(tris[k].T);
            return new BoundAndCount(end - start, builder.GetBox());
        }

        public BoundAndCount GetVal(Tri t)
        {
            Vector4f point1 = new Vector4f(t.T.p1.x, t.T.p1.y, t.T.p1.z, 0f);
            Vector4f point2 = new Vector4f(t.T.p2.x, t.T.p2.y, t.T.p2.z, 0f);
            Vector4f point3 = new Vector4f(t.T.p3.x, t.T.p3.y, t.T.p3.z, 0f);

            return new BoundAndCount(1, new Box3(point1.Min(point2).Min(point3), point1.Max(point2).Max(point3)));
        }

        public void InplaceOp3(ref BoundAndCount val1, ref BoundAndCount val2, ref BoundAndCount val3, Tri t)
        {
            Vector4f point1 = new Vector4f(t.T.p1.x, t.T.p1.y, t.T.p1.z, 0f);
            Vector4f point2 = new Vector4f(t.T.p2.x, t.T.p2.y, t.T.p2.z, 0f);
            Vector4f point3 = new Vector4f(t.T.p3.x, t.T.p3.y, t.T.p3.z, 0f);

            Vector4f triMax = point1.Max(point2).Max(point3);
            Vector4f triMin = point1.Min(point2).Min(point3);

            val1 = new BoundAndCount(val1.Count + 1, val1.Box.IsEmpty ? new Box3(triMin, triMax) : new Box3(val1.Box.Min.Min(triMin), val1.Box.Max.Max(triMax)));
            val2 = new BoundAndCount(val2.Count + 1, val2.Box.IsEmpty ? new Box3(triMin, triMax) : new Box3(val2.Box.Min.Min(triMin), val2.Box.Max.Max(triMax)));
            val3 = new BoundAndCount(val3.Count + 1, val3.Box.IsEmpty ? new Box3(triMin, triMax) : new Box3(val3.Box.Min.Min(triMin), val3.Box.Max.Max(triMax)));
        }


        public bool IsIdentity(BoundAndCount agg)
        {
            return agg.Count == 0;
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
