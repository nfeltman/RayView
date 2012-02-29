using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public struct RBVH2Branch : Boxed, Weighted
    {
        public float PLeft { get; set; }
        public int Depth { get; set; }
        public Box3 BBox { get; set; }
        public int ID { get; set; }
    }

    public struct RBVH2Leaf : Primitived<Triangle>, Boxed
    {
        public Box3 BBox { get; set; }
        public int ID { get; set; }
        public int Depth { get; set; }
        public Triangle[] Primitives { get; set; }
        public int PrimCount { get { return Primitives.Length; } }
    }

    public struct BackedRBVH2Branch : Boxed, Weighted
    {
        public float PLeft { get; set; }
        public Box3 BBox { get; set; }
        public int ID { get; set; }
    }

    public struct BackedRBVH2Leaf : Boxed, Primitived<int>
    {
        public Box3 BBox { get; set; }
        public int ID { get; set; }
        public int[] Primitives { get; set; }
        public int PrimCount { get { return Primitives.Length; } }
    }
}
