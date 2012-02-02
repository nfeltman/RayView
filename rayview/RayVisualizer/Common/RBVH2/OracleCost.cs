using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class OracleCost : NodeVisitor<TraceResult, RBVH2Branch, RBVH2Leaf>
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
            for (int k=0; k < prims.Length;k++)
            {
                if (prims[k].IntersectRay(ShadowRay.Origin, ShadowRay.Difference) < 1)
                    return new TraceResult(false, new TraceCost(new RandomVariable(1, 0), new RandomVariable(1, 0)));
            }
            return new TraceResult(false, new TraceCost(new RandomVariable(1, 0), new RandomVariable(prims.Length, 0)));
        }

        public static TraceCost GetTotalCost(RBVH2 tree, IEnumerable<Segment3> shadows)
        {
            TraceCost cost = new TraceCost(); //the default value is correct
            OracleCost measure = new OracleCost();
            foreach (Segment3 shadow in shadows)
            {
                measure.ShadowRay = shadow;
                cost = cost + tree.Accept(measure).Cost;
            }
            return cost;
        }
    }
}
