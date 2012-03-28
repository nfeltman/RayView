using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;
using System.IO;
using System.Diagnostics;

namespace Topaz
{
    using BVH2 = Tree<BVH2Branch, BVH2Leaf>;
    using RBVH2 = Tree<RBVH2Branch, RBVH2Leaf>;
    using BackedBVH2 = Tree<BackedBVH2Branch, BackedBVH2Leaf>;
    using BackedRBVH2 = Tree<BackedRBVH2Branch, BackedRBVH2Leaf>;

    public static class SweepTests
    {
        public static void PerformAA3SweepTest(BasicBuildTriangle[] tris, RaySet rays, TopazStreamWriter output)
        {
            int numBins = 100;
            // calculate splits
            Stopwatch st = new Stopwatch();
            Box3 centroidBounds = BuildTools.FindCentroidBound(tris, 0, tris.Length);

            Console.WriteLine("Starting SRDH helper build. "); st.Start();
            BVH2 initialBuild = GeneralBVH2Builder.BuildStructure(tris, new StatelessSplitEvaluator((ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea), BVHNodeFactory<TriangleContainer>.ONLY, BoundsCountAggregator<BasicBuildTriangle>.ONLY, TripleAASplitter.ONLY, 4);
            st.Stop(); Console.WriteLine("Done with SRDH helper build. Time(ms) = {0}", st.ElapsedMilliseconds);

            Console.WriteLine("Starting SRDH ray compilation. "); st.Reset(); st.Start();
            ShadowRayResults<BasicBuildTriangle> res = ShadowRayCompiler.CompileCasts(rays.ShadowQueries, initialBuild);
            st.Stop(); Console.WriteLine("Done with SRDH ray compilation. Time(ms) = {0}", st.ElapsedMilliseconds);

            ShadowRayCostEvaluator<BasicBuildTriangle> eval = new ShadowRayCostEvaluator<BasicBuildTriangle>(res, 1.0f);

            var evaluatorState = eval.BeginEvaluations(0, tris.Length, BoundsCountAggregator<Bounded>.ONLY.Roll(tris, 0, tris.Length), eval.GetDefault());
            SweepTestHelper("X", numBins, centroidBounds.XRange.Min, numBins / centroidBounds.XRange.Size, output, tris, eval, evaluatorState);
            SweepTestHelper("Y", numBins, centroidBounds.YRange.Min, numBins / centroidBounds.YRange.Size, output, tris, eval, evaluatorState);
            SweepTestHelper("Z", numBins, centroidBounds.ZRange.Min, numBins / centroidBounds.ZRange.Size, output, tris, eval, evaluatorState);
        }

        public static void PerformRadialSweepTest(BasicBuildTriangle[] tris, RaySet rays, TopazStreamWriter output)
        {
            int numBins = 100;
            // calculate splits
            Stopwatch st = new Stopwatch();

            Console.WriteLine("Starting SRDH helper build. "); st.Start();
            BVH2 initialBuild = GeneralBVH2Builder.BuildStructure(tris, new StatelessSplitEvaluator((ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea), BVHNodeFactory<TriangleContainer>.ONLY, BoundsCountAggregator<BasicBuildTriangle>.ONLY, TripleAASplitter.ONLY, 4);
            st.Stop(); Console.WriteLine("Done with SRDH helper build. Time(ms) = {0}", st.ElapsedMilliseconds);

            Console.WriteLine("Starting SRDH ray compilation. "); st.Reset(); st.Start();
            ShadowRayResults<BasicBuildTriangle> res = ShadowRayCompiler.CompileCasts(rays.ShadowQueries, initialBuild);
            st.Stop(); Console.WriteLine("Done with SRDH ray compilation. Time(ms) = {0}", st.ElapsedMilliseconds);

            ShadowRayCostEvaluator<BasicBuildTriangle> eval = new ShadowRayCostEvaluator<BasicBuildTriangle>(res, 1.0f);

            var evaluatorState = eval.BeginEvaluations(0, tris.Length, BoundsCountAggregator<Bounded>.ONLY.Roll(tris, 0, tris.Length), eval.GetDefault());

            Box3 centroidBounds = BuildTools.FindCentroidBound(tris, 0, tris.Length);
            CVector3 center = centroidBounds.GetCenter();
            ClosedInterval distBound = BuildTools.FindDistanceBound(tris, center, 0, tris.Length);
            if (distBound.IsEmpty) throw new Exception("Distance Bound should not be empty.");
            if (distBound.Size == 0)
            {
                center = tris[0].Center;
                distBound = BuildTools.FindDistanceBound(tris, center, 0, tris.Length);
            }
            //Console.WriteLine("[{0} {1} {2}] <<{3} {4}>>", centroidBounds.XRange.Size, centroidBounds.YRange.Size, centroidBounds.ZRange.Size, distBound.Min, distBound.Max);

            float offset = distBound.Min;
            float factor = numBins / distBound.Size;



            string splits_locs = "";
            string sah_vals = "";
            string srdh_vals = "";

            SplitterHelper.RunSplitSweepTest(
                (int split, BoundAndCount leftAgg, BoundAndCount rightAgg, Func<CenterIndexable, bool> filter) =>
                {
                    //Console.WriteLine("{0}/{1}", leftAgg.Count, rightAgg.Count);
                    var cost = eval.EvaluateSplit(leftAgg, rightAgg, evaluatorState, filter);
                    splits_locs += String.Format(" {0}", split / factor + offset);
                    sah_vals += String.Format(" {0}", leftAgg.Box.SurfaceArea * (leftAgg.Count * 2 - 1) + rightAgg.Box.SurfaceArea * (rightAgg.Count * 2 - 1));
                    srdh_vals += String.Format(" {0}", cost.Cost);
                }, tris, new RadialSplitSeries(centroidBounds.GetCenter(), offset, factor), numBins, BoundsCountAggregator<BasicBuildTriangle>.ONLY);

            output.WriteLine("\"numSplits\" \"count\" {0}", numBins - 1);
            output.WriteLine("\"splits\" \"array\"{0}", splits_locs);
            output.WriteLine("\"sah\" \"array\"{0}", sah_vals);
            output.WriteLine("\"srdh\" \"array\"{0}", srdh_vals);
        }


        private static void SweepTestHelper(string dim, int numBins, float less, float times, TopazStreamWriter output, BasicBuildTriangle[] tris, ShadowRayCostEvaluator<BasicBuildTriangle> eval, ShadowRayCostEvaluator<BasicBuildTriangle>.ShadowRayShuffleState evaluatorState)
        {
            string splits_locs = "";
            string sah_vals = "";
            string srdh_vals = "";

            SplitterHelper.RunSplitSweepTest(
                (int split, BoundAndCount leftAgg, BoundAndCount rightAgg, Func<CenterIndexable, bool> filter) =>
                {
                    var cost = eval.EvaluateSplit(leftAgg, rightAgg, evaluatorState, filter);
                    splits_locs += String.Format(" {0}", split / times + less);
                    sah_vals += String.Format(" {0}", leftAgg.Box.SurfaceArea * (leftAgg.Count * 2 - 1) + rightAgg.Box.SurfaceArea * (rightAgg.Count * 2 - 1));
                    srdh_vals += String.Format(" {0}", cost.Cost);
                }, tris, new XAASplitSeries(less, times), numBins, BoundsCountAggregator<BasicBuildTriangle>.ONLY);

            output.WriteLine("\"numSplits{0}\" \"count\" {1}", dim, numBins - 1);
            output.WriteLine("\"splits{0}\" \"array\"{1}", dim, splits_locs);
            output.WriteLine("\"sah{0}\" \"array\"{1}", dim, sah_vals);
            output.WriteLine("\"srdh{0}\" \"array\"{1}", dim, srdh_vals);
        }
    }
}
