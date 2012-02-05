using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public struct TraceCost
    {
        public RandomVariable BBoxTests;
        public RandomVariable PrimitiveTests;

        public TraceCost(RandomVariable bbox, RandomVariable prim)
        {
            BBoxTests = bbox;
            PrimitiveTests = prim;
        }

        public TraceCost(double bbox_exp, double bbox_var, double prim_exp, double prim_var)
        {
            BBoxTests = new RandomVariable(bbox_exp, bbox_var);
            PrimitiveTests = new RandomVariable(prim_exp, prim_var);
        }

        public static TraceCost operator +(TraceCost x, TraceCost y)
        {
            // assume no covariance (x and y are independent)
            return new TraceCost(x.BBoxTests.ExpectedValue + y.BBoxTests.ExpectedValue, x.BBoxTests.Variance + y.BBoxTests.Variance, 
                x.PrimitiveTests.ExpectedValue + y.PrimitiveTests.ExpectedValue, x.PrimitiveTests.Variance + y.PrimitiveTests.Variance);
        }

        public static TraceCost RandomSelect(double p, TraceCost x, TraceCost y)
        {
            return new TraceCost(RandomVariable.RandomSelect(p, x.BBoxTests, y.BBoxTests), RandomVariable.RandomSelect(p, x.PrimitiveTests, y.PrimitiveTests));
        }

        public static bool operator ==(TraceCost t1, TraceCost t2)
        {
            return t1.BBoxTests.ExpectedValue == t2.BBoxTests.ExpectedValue 
                && t1.BBoxTests.Variance == t2.BBoxTests.Variance
                && t1.PrimitiveTests.ExpectedValue == t2.PrimitiveTests.ExpectedValue
                && t1.PrimitiveTests.Variance == t2.PrimitiveTests.Variance;
        }

        public static bool operator !=(TraceCost t1, TraceCost t2)
        {
            return t1.BBoxTests.ExpectedValue != t2.BBoxTests.ExpectedValue
                || t1.BBoxTests.Variance != t2.BBoxTests.Variance
                || t1.PrimitiveTests.ExpectedValue != t2.PrimitiveTests.ExpectedValue
                || t1.PrimitiveTests.Variance != t2.PrimitiveTests.Variance;
        }

        public override string ToString()
        {
            return String.Format("({0}/{1}, {2}/{3})", BBoxTests.ExpectedValue, BBoxTests.Variance, PrimitiveTests.ExpectedValue, PrimitiveTests.Variance);
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
            return new RandomVariable(x.ExpectedValue + y.ExpectedValue, x.Variance + y.Variance);
        }

        public static RandomVariable RandomSelect(double p, RandomVariable x, RandomVariable y)
        {
            double q = 1 - p;
            double d = x.ExpectedValue - y.ExpectedValue;
            return new RandomVariable(p * x.ExpectedValue + q * y.ExpectedValue, p * x.Variance + q * y.Variance + p * q * d * d);
        }
    }

    public struct TraceResult
    {
        public bool Hits;
        public TraceCost Cost;

        public TraceResult(bool h, TraceCost cost)
        {
            Hits = h;
            Cost = cost;
        }

        public static bool operator ==(TraceResult t1, TraceResult t2)
        {
            return t1.Hits == t2.Hits && t1.Cost == t2.Cost;
        }
        public static bool operator !=(TraceResult t1, TraceResult t2)
        {
            return t1.Hits != t2.Hits || t1.Cost != t2.Cost;
        }
    }
}
