using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;

namespace Topaz
{
    using BVH2 = Tree<BVH2Branch, BVH2Leaf>;
    using RBVH2 = Tree<RBVH2Branch, RBVH2Leaf>;
    using BackedBVH2 = Tree<BackedBVH2Branch, BackedBVH2Leaf>;
    using BackedRBVH2 = Tree<BackedRBVH2Branch, BackedRBVH2Leaf>;
    using System.Diagnostics;

    public static class EvalMethods
    {

        public static void PerformPQEvaluation(RBVH2 build, RaySet rays, TopazStreamWriter output)
        {
            Stopwatch st = new Stopwatch();
            Console.WriteLine("Starting standard PQ evaluation. "); st.Start();
            FullTraceResult cost = FullCostMeasure.GetTotalCost(build, rays.ShadowQueries);
            st.Stop(); Console.WriteLine("Done with PQ evaluation. Time(ms) = {0}", st.ElapsedMilliseconds);

            output.PrintSimple("PQ NumRays", cost.NumRays);
            output.PrintSimple("PQ NumBothHit", cost.topazHit_mantaHit);
            output.PrintSimple("PQ NumBothMiss", cost.topazMiss_mantaMiss);
            output.PrintSimple("PQ NumTopazMissMantaHit", cost.topazMiss_mantaHit);
            output.PrintSimple("PQ NumTopazHitMantaMiss", cost.topazHit_mantaMiss);
            output.PrintSimple("PQ Disagreement", cost.Disagreement);
            output.PrintCost("PQ (Spine Oracle) ", cost.SpineOracle);
            output.PrintCost("PQ (Spine) ", cost.Spine);
            output.PrintCost("PQ (Side) ", cost.SideTrees);
            output.PrintCost("PQ (Non-Hit) ", cost.NonHit);
            output.PrintCost("PQ (Total) ", cost.Spine + cost.SideTrees + cost.NonHit);

            
        }

        public static void PerformOracleEvaluation(RBVH2 build, RaySet rays, TopazStreamWriter output)
        {
            Stopwatch st = new Stopwatch();
            Console.WriteLine("Starting oracle evaluation. "); st.Reset(); st.Start();
            OracleTraceResult cost = OracleCost.GetTotalCost(build, rays.ShadowQueries.Select(q => new Segment3(q.Origin, q.Difference)));
            st.Stop(); Console.WriteLine("Done with Oracle evaluation. Time(ms) = {0}", st.ElapsedMilliseconds);

            output.PrintSimple("Oracle NumRays", cost.NumRays);
            output.PrintSimple("Oracle NumHits", cost.NumHits);
            output.PrintCost("Oracle (Hit) ", cost.Hit);
            output.PrintCost("Oracle (Non-Hit) ", cost.NonHit);
            output.PrintCost("Oracle (Total) ", cost.NonHit + cost.Hit); 
        }

        public static void PerformRayHistogramEvaluation(RBVH2 build, RaySet rays, TopazStreamWriter output)
        {
            Stopwatch st = new Stopwatch();
            Console.WriteLine("Starting ray histogram evaluation. "); st.Reset(); st.Start();
            RayCostHistogram cost = CostHistogramMeasure.GetTotalCost(build, rays.ShadowQueries);
            st.Stop(); Console.WriteLine("Done with ray histogram evaluation. Time(ms) = {0}", st.ElapsedMilliseconds);

            output.PrintArray("BBoxTest Histogram (Hits)", cost.bboxCostBinsHits);
            output.PrintArray("BBoxTest Histogram (Misses)", cost.bboxCostBinsMisses);
            output.PrintArray("BBoxTest Histogram (Both)", cost.bboxCostBinsBoth);
            output.PrintSimple("BBoxTest OutOfRange (Hits)", cost.outOfRangeHits);
            output.PrintSimple("BBoxTest OutOfRange (Misses)", cost.outOfRangeMisses);
            output.PrintSimple("BBoxTest OutOfRange (Both)", cost.outOfRangeBoth);
        }
    }
}
