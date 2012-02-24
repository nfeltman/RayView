using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    using BVH2 = Tree<BVH2Branch, BVH2Leaf>;
    using RBVH2 = Tree<RBVH2Branch, RBVH2Leaf>;
    using BackedBVH2 = Tree<BackedBVH2Branch, BackedBVH2Leaf>;
    using BackedRBVH2 = Tree<BackedRBVH2Branch, BackedRBVH2Leaf>;

    public class OracleCost
    {
        private class InternalVisitor : NodeVisitor<TraceResult, RBVH2Branch, RBVH2Leaf>
        {
            public Segment3 ShadowRay { get; set; }

            public TraceResult ForBranch(Branch<RBVH2Branch, RBVH2Leaf> branch)
            {
                if (!branch.Content.BBox.DoesIntersectSegment(ShadowRay.Origin, ShadowRay.Difference))
                    return new TraceResult(false, new TraceCost(new RandomVariable(1, 0), new RandomVariable(0, 0)));

                TraceResult left = branch.Left.Accept(this);
                TraceResult right = branch.Right.Accept(this);

                TraceResult res;
                if (left.Hits && right.Hits)
                {
                    res = left.Cost.BBoxTests.ExpectedValue < right.Cost.BBoxTests.ExpectedValue ? left : right;
                }
                else if (!left.Hits && right.Hits)
                {
                    res = right;
                }
                else if (left.Hits && !right.Hits)
                {
                    res = left;
                }
                else
                {
                    res = new TraceResult(false, left.Cost + right.Cost);
                }
                res.Cost.BBoxTests.ExpectedValue += 1.0; // to account for the test from this node
                return res;
            }

            public TraceResult ForLeaf(Leaf<RBVH2Branch, RBVH2Leaf> leaf)
            {
                if (!leaf.Content.BBox.DoesIntersectSegment(ShadowRay.Origin, ShadowRay.Difference))
                    return new TraceResult(false, new TraceCost(new RandomVariable(1, 0), new RandomVariable(0, 0)));
                Triangle[] prims = leaf.Content.Primitives;
                for (int k = 0; k < prims.Length; k++)
                {
                    if (prims[k].IntersectsSegment(ShadowRay.Origin, ShadowRay.Difference))
                        return new TraceResult(true, new TraceCost(new RandomVariable(1, 0), new RandomVariable(1, 0)));
                }
                return new TraceResult(false, new TraceCost(new RandomVariable(1, 0), new RandomVariable(prims.Length, 0)));
            }
        }

        private class TraceResult
        {
            public bool Hits;
            public TraceCost Cost;
            public TraceResult(bool hits, TraceCost cost)
            {
                Hits = hits;
                Cost = cost;
            }
        }

        public static OracleTraceResult GetTotalCost(RBVH2 tree, IEnumerable<Segment3> shadows)
        {
            OracleTraceResult cost = new OracleTraceResult(); //the default value is correct
            InternalVisitor measure = new InternalVisitor();
            foreach (Segment3 shadow in shadows)
            {
                measure.ShadowRay = shadow;
                TraceResult res = tree.Accept(measure);
                cost.NumRays++;
                if (res.Hits)
                {
                    cost.Hit += res.Cost;
                    cost.NumHits++;
                }
                else
                {
                    cost.NonHit += res.Cost;
                }
            }
            return cost;
        }
    }

    public class OracleTraceResult
    {
        public int NumRays;
        public int NumHits;
        public TraceCost Hit;
        public TraceCost NonHit;
    }
}
