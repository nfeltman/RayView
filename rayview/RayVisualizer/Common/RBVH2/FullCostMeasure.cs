﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    // does not assume this box has already been hit
    public class FullCostMeasure : NodeVisitor<TraceResult, RBVH2Branch, RBVH2Leaf>
    {
        public Segment3 ShadowRay { get; set; }

        public TraceResult ForBranch(Branch<RBVH2Branch, RBVH2Leaf> branch)
        {
            if (!branch.Content.BBox.DoesIntersectSegment(ShadowRay.Origin, ShadowRay.Difference))
                return new TraceResult(false, new TraceCost(1, 0, 0, 0));

            TraceResult left;
            TraceResult right;

            if (branch.Content.PLeft == 1)
            {
                left = branch.Left.Accept(this);
                if (left.Hits)
                {
                    left.Cost.BBoxTests.ExpectedValue += 1.0;
                    return left;
                }
                right = branch.Right.Accept(this);
            }
            else if (branch.Content.PLeft == 0)
            {
                right = branch.Right.Accept(this);
                if (right.Hits)
                {
                    right.Cost.BBoxTests.ExpectedValue += 1.0;
                    return right;
                }
                left = branch.Left.Accept(this);
            }
            else
            {
                left = branch.Left.Accept(this);
                right = branch.Right.Accept(this);
            }

            TraceCost both = left.Cost + right.Cost;

            TraceCost res;
            if (left.Hits && right.Hits)
            {
                res = TraceCost.RandomSelect(branch.Content.PLeft, left.Cost, right.Cost);
            }
            else if (!left.Hits && right.Hits)
            {
                res = TraceCost.RandomSelect(branch.Content.PLeft, both, right.Cost);
            }
            else if (left.Hits && !right.Hits)
            {
                res = TraceCost.RandomSelect(branch.Content.PLeft, left.Cost, both);
            }
            else
            {
                res = both;
            }
            res.BBoxTests.ExpectedValue += 1.0; // to account for the test from this node
            return new TraceResult(left.Hits || right.Hits, res);
        }

        public TraceResult ForLeaf(Leaf<RBVH2Branch, RBVH2Leaf> leaf)
        {
            if (!leaf.Content.BBox.DoesIntersectSegment(ShadowRay.Origin, ShadowRay.Difference))
                return new TraceResult(false, new TraceCost(new RandomVariable(1, 0), new RandomVariable(0, 0)));
            Triangle[] prims = leaf.Content.Primitives;
            int k = 0;
            int primtests=0;
            while (k < prims.Length)
            {
                primtests++;
                if (prims[k].IntersectRay(ShadowRay.Origin, ShadowRay.Difference) < 1)
                {
                    break;
                }
                k++;
            }
            return new TraceResult(k != prims.Length, new TraceCost(new RandomVariable(1, 0), new RandomVariable(primtests, 0)));
        }

        public static TraceCost GetTotalCost(RBVH2 tree, IEnumerable<Segment3> shadows)
        {
            TraceCost cost = new TraceCost(); //the default value is correct
            FullCostMeasure measure = new FullCostMeasure();
            foreach (Segment3 shadow in shadows)
            {
                measure.ShadowRay = shadow;
                cost = cost + tree.Accept(measure).Cost;
            }
            return cost;
        }
    }
}
