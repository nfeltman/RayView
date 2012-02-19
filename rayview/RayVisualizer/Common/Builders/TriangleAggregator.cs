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
        void InplaceOp3(ref Aggregate val1, ref Aggregate val2, ref Aggregate val3, BuildTriangle t); // to alleviate the cost of a virtual call
        Aggregate Op(Aggregate val1, Aggregate val2);
        void InplaceOp(ref Aggregate val1, Aggregate val2);
        Aggregate GetIdentity();
        Aggregate Roll(BuildTriangle[] tris, int start, int end);
        Aggregate GetVal(BuildTriangle t);
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

        public void InplaceOp3(ref int val1, ref int val2, ref int val3, BuildTriangle t)
        {
            val1 = val1 + 1;
            val2 = val2 + 1;
            val3 = val3 + 1;
        }

        public int GetVal(BuildTriangle t)
        {
            return 1;
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
            Vector4f point1 = new Vector4f(t.t.p1.x, t.t.p1.y, t.t.p1.z, 0f);
            Vector4f point2 = new Vector4f(t.t.p2.x, t.t.p2.y, t.t.p2.z, 0f);
            Vector4f point3 = new Vector4f(t.t.p3.x, t.t.p3.y, t.t.p3.z, 0f);

            Vector4f triMax = point1.Max(point2).Max(point3);
            Vector4f triMin = point1.Min(point2).Min(point3);

            val = new BoundAndCount(val.Count + 1, val.Box.IsEmpty ? new Box3(triMin, triMax) : new Box3(val.Box.Min.Min(triMin), val.Box.Max.Max(triMax)));
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

        public BoundAndCount GetVal(BuildTriangle t)
        {
            Vector4f point1 = new Vector4f(t.t.p1.x, t.t.p1.y, t.t.p1.z, 0f);
            Vector4f point2 = new Vector4f(t.t.p2.x, t.t.p2.y, t.t.p2.z, 0f);
            Vector4f point3 = new Vector4f(t.t.p3.x, t.t.p3.y, t.t.p3.z, 0f);

            return new BoundAndCount(1, new Box3(point1.Min(point2).Min(point3), point1.Max(point2).Max(point3)));
        }

        public void InplaceOp3(ref BoundAndCount val1, ref BoundAndCount val2, ref BoundAndCount val3, BuildTriangle t)
        {
            Vector4f point1 = new Vector4f(t.t.p1.x, t.t.p1.y, t.t.p1.z, 0f);
            Vector4f point2 = new Vector4f(t.t.p2.x, t.t.p2.y, t.t.p2.z, 0f);
            Vector4f point3 = new Vector4f(t.t.p3.x, t.t.p3.y, t.t.p3.z, 0f);

            Vector4f triMax = point1.Max(point2).Max(point3);
            Vector4f triMin = point1.Min(point2).Min(point3);

            val1 = new BoundAndCount(val1.Count + 1, val1.Box.IsEmpty ? new Box3(triMin, triMax) : new Box3(val1.Box.Min.Min(triMin), val1.Box.Max.Max(triMax)));
            val2 = new BoundAndCount(val2.Count + 1, val2.Box.IsEmpty ? new Box3(triMin, triMax) : new Box3(val2.Box.Min.Min(triMin), val2.Box.Max.Max(triMax)));
            val3 = new BoundAndCount(val3.Count + 1, val3.Box.IsEmpty ? new Box3(triMin, triMax) : new Box3(val3.Box.Min.Min(triMin), val3.Box.Max.Max(triMax)));
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
