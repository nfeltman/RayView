using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;
using System.IO;

namespace AnalysisEngine
{
    public static class BVHMetrics
    {
        public static float BranchSurfaceArea(this BVH2 bvh)
        {
            return bvh.RollUp((b, s1, s2) => s1 + s2 + b.BBox.SurfaceArea, l => 0f);
        }
        public static float LeafSurfaceArea(this BVH2 bvh)
        {
            return bvh.RollUp((b, s1, s2) => s1 + s2, l => l.BBox.SurfaceArea);
        }
        public static int BranchTraversalCost(this BVH2 bvh, FHRayResults res)
        {
            RayHitCostVisitor cost1 = new RayHitCostVisitor();
            RayMissCostVisitor cost2 = new RayMissCostVisitor();
            for (int k = 0; k < res.Hits.Length; k++)
            {
                cost1.ToTest = res.Hits[k];
                bvh.Accept(cost1);
            }
            for (int k = 0; k < res.Misses.Length; k++)
            {
                cost2.ToTest = res.Misses[k];
                bvh.Accept(cost2);
            }
            return cost1.IntersectionCount + cost2.IntersectionCount;
        }
        public static float ScaledLeafSurfaceArea(this BVH2 bvh)
        {
            return bvh.RollUp((b, s1, s2) => s1 + s2, l => l.BBox.SurfaceArea * (2*l.Primitives.Length-1));
        }
        public static float MaxDepth(this BVH2 bvh)
        {
            return bvh.RollUp((b, s1, s2) => 1 + Math.Max(s1, s2), l => 1);
        }
        public static float MaxLeafSize(this BVH2 bvh)
        {
            return bvh.RollUp((b, s1, s2) => Math.Max(s1, s2), l => l.Primitives.Length);
        }
        public static float NumPrims(this BVH2 bvh)
        {
            return bvh.RollUp((b, s1, s2) => s1+ s2, l => l.Primitives.Length);
        }
        public static bool IsConsistentWith(this BVH2 bvh1, BVH2 bvh2, int depth)
        {
            return ConsistencyCheck(bvh1.Root, bvh2.Root, depth);
        }

        private static bool ConsistencyCheck(BVH2Node bvh1, BVH2Node bvh2, int depth)
        {
            if (depth < 0) return true;
            return bvh1.Accept(
                b1 => bvh2.Accept(
                    b2 => b1.BBox.Equals(b2.BBox) &&
                        ((ConsistencyCheck(b1.Left, b2.Left, depth - 1) && ConsistencyCheck(b1.Right, b2.Right, depth - 1)) ||
                        (ConsistencyCheck(b1.Left, b2.Right, depth - 1) && ConsistencyCheck(b1.Right, b2.Left, depth - 1))),
                    l2 => false),
                l1 => bvh2.Accept(
                    b2 => false,
                    l2 => l1.BBox.Equals(l2.BBox)));
        }
    }
}
