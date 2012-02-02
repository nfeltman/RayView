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

        public static TraceCost operator +(TraceCost x, TraceCost y)
        {
            // assume no covariance (x and y are independent)
            return new TraceCost(x.BBoxTests + y.BBoxTests, x.PrimitiveTests + y.PrimitiveTests);
        }

        public static TraceCost RandomSelect(double p, TraceCost x, TraceCost y)
        {
            return new TraceCost(RandomVariable.RandomSelect(p, x.BBoxTests, y.BBoxTests), RandomVariable.RandomSelect(p, x.PrimitiveTests, y.PrimitiveTests));
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
}
