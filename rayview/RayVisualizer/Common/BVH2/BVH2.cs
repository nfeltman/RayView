using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RayVisualizer.Common
{
    public struct BVH2Branch : Boxed
    {
        public int Depth { get; set; }
        public Box3 BBox { get; set; }
        public int ID { get; set; }
    }

    public struct BVH2Leaf : Primitived<Triangle>, Boxed
    {
        public Box3 BBox { get; set; }
        public int ID { get; set; }
        public int Depth { get; set; }
        public Triangle[] Primitives { get; set; }
        public int PrimCount { get { return Primitives.Length; } }

        public HitRecord FindClosestPositiveIntersection(CVector3 origin, CVector3 direction, ClosedInterval tInterval)
        {
            // TODO : have it take in a c vector

            float closestIntersection = float.PositiveInfinity;
            int closestIndex = -1;
            Triangle[] triangles = Primitives;
            for (int k = 0; k < triangles.Length; k++)
            {
                float intersection = triangles[k].IntersectRay(origin, direction);
                if (!float.IsNaN(intersection) && intersection>0.001 && tInterval.Contains(intersection) && intersection < closestIntersection)
                {
                    closestIntersection = intersection;
                    closestIndex = k;
                }
            }
            if (closestIndex == -1)
                return null;
            return new HitRecord(triangles[closestIndex],closestIntersection, ID);
        }

    }

    public struct BackedBVH2Branch : Boxed
    {
        public int Depth { get; set; }
        public Box3 BBox { get; set; }
        public int ID { get; set; }
    }

    public struct BackedBVH2Leaf : Boxed, Primitived<int>
    {
        public Box3 BBox { get; set; }
        public int ID { get; set; }
        public int Depth { get; set; }
        public int[] Primitives { get; set; }
        public int PrimCount { get { return Primitives.Length; } }
    }

}
