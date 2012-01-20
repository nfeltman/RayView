using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    // does not assume this box has already been hit
    public class FullCostMeasure : RBVH2Visitor<ShadowCost>
    {
        public Segment3 ShadowRay { get; set; }

        public ShadowCost ForBranch(RBVH2Branch branch)
        {
            if (!branch.BBox.DoesIntersectSegment(ShadowRay.Origin, ShadowRay.Difference))
                return new ShadowCost(false, new RandomVariable(1, 0), new RandomVariable(0, 0));

            ShadowCost left = branch.Left.Accept(this);
            ShadowCost right = branch.Right.Accept(this);
            ShadowCost both = new ShadowCost(false, left.BBoxTests + right.BBoxTests, left.PrimitiveTests + right.PrimitiveTests);

            ShadowCost res;
            if (left.Hits && right.Hits)
            {
                res = ShadowCost.RandomSelect(branch.PLeft, left, right);
            }
            else if (!left.Hits && right.Hits)
            {
                res = ShadowCost.RandomSelect(branch.PLeft, both, right);
            }
            else if (left.Hits && !right.Hits)
            {
                res = ShadowCost.RandomSelect(branch.PLeft, left, both);
            }
            else
            {
                res = both;
            }
            res.BBoxTests.ExpectedValue += 1.0; // to account for the test from this node
            return res;
        }

        public ShadowCost ForLeaf(RBVH2Leaf leaf)
        {
            if (!leaf.BBox.DoesIntersectSegment(ShadowRay.Origin, ShadowRay.Difference))
                return new ShadowCost(false, new RandomVariable(1, 0), new RandomVariable(0, 0));
            Triangle[] prims = leaf.Primitives;
            int k=0;
            while(k < prims.Length)
            {
                if (prims[k++].IntersectRay(ShadowRay.Origin, ShadowRay.Difference) < 1) break;
            }
            return new ShadowCost(k != prims.Length, new RandomVariable(1, 0), new RandomVariable(k, 0));
        }
    }

    public class ShadowCost
    {
        public bool Hits;
        public RandomVariable BBoxTests;
        public RandomVariable PrimitiveTests;

        public ShadowCost(bool h, RandomVariable bbox, RandomVariable prim)
        {
            Hits = h;
            BBoxTests = bbox;
            PrimitiveTests = prim;
        }

        public static ShadowCost RandomSelect(double p, ShadowCost x, ShadowCost y)
        {
            return new ShadowCost(true, RandomVariable.RandomSelect(p, x.BBoxTests, y.BBoxTests), RandomVariable.RandomSelect(p, x.PrimitiveTests, y.PrimitiveTests));
        }
    }

    public struct RandomVariable
    {
        public double ExpectedValue;
        public double Variance;

        public RandomVariable(double e, double v)
        {
            ExpectedValue = e;
            Variance = v;
        }

        public static RandomVariable operator +(RandomVariable x, RandomVariable y)
        {
            // assume no covariance (x and y are independent)
            return new RandomVariable(x.ExpectedValue + y.ExpectedValue, x.Variance+y.Variance); 
        }

        public static RandomVariable RandomSelect(double p, RandomVariable x, RandomVariable y)
        {
            double q = 1 - p;
            double d = x.ExpectedValue-y.ExpectedValue;
            return new RandomVariable(p * x.ExpectedValue + q * y.ExpectedValue, p * x.Variance + q * y.Variance + p * q * d * d);
        }
    }
}
