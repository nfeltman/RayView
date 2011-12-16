using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using RayVisualizer.Common;

namespace AnalysisEngine
{
    class RunAnalysis
    {
        public static void Main()
        {
            string tracesPath = "traces\\";
            BuildAnalysis.SphereFocusExperiment(tracesPath);
            BuildAnalysis.HemisphereFocusExperiment(tracesPath);
            BuildAnalysis.PlaneFocusExperiment(tracesPath);
            BuildAnalysis.SphereFocusAndWeightExperiment(tracesPath);
            //BuildAnalysis.BuildTreeWithRayData(tracesPath);
            //SpeedTest(tracesPath);
        }

        /*
        public static void SpeedTest(string tracesPath)
        {
            BVH2 bvh = BVH2Parser.ReadFromFile(new FileStream(tracesPath + "powerplant\\raw_bvh.txt", FileMode.Open, FileAccess.Read));
            RaySet allrays = RayFileParser.ReadFromFile(new FileStream(tracesPath + "powerplant\\casts.txt", FileMode.Open, FileAccess.Read));
            RaySet set = allrays.CastOnlyFilter((r,i) => i%67==0 && r.Depth>=1);

            Console.WriteLine("Compiling...");
            FHRayResults res = RayCompiler.CompileCasts(set, bvh);

            Stopwatch st = new Stopwatch();
            st.Start();
            RayOrderOpCounter ops = new RayOrderOpCounter();
            int hitCount = 0, missCount = 0;
            int hitCount2 = 0, missCount2 = 0;
            for (int k = 0; k < rays.Length; k++)
            {
                if ((k & 4095) == 0)
                    Console.WriteLine("{0}/{1}", k, rays.Length);
                HitRecord rec = RayOrderTraverser.RunTooledTraverser(bvh, rays[k], ops);
                if (rec == null) missCount++; else hitCount++;
                if (rays[k].Kind == RayKind.FirstHit_Hit) hitCount2++; else missCount2++; 
            }
            st.Stop();
            Console.WriteLine("Took {0} seconds.  Box tests: {1}", st.Elapsed.TotalSeconds, ops.boundingBoxTests);

            Console.WriteLine("Hits: {0}; Misses: {1}; Sum = {2}", res.Hits.Length, res.Misses.Length, res.Hits.Length + res.Misses.Length);
            st.Reset();
            st.Start();
            RayHitCostVisitor cost1 = new RayHitCostVisitor();
            RayMissCostVisitor cost3 = new RayMissCostVisitor();
            for (int k = 0; k < res.Hits.Length; k++)
            {
                if ((k & 4095) == 0)
                    Console.WriteLine("{0}/{1}", k, res.Hits.Length);
                cost1.ToTest = res.Hits[k];
                bvh.Accept(cost1);
            }
            for (int k = 0; k < res.Misses.Length; k++)
            {
                if ((k & 2047) == 0)
                    Console.WriteLine("{0}/{1}", k, res.Misses.Length);
                cost3.ToTest = res.Misses[k];
                bvh.Accept(cost3);
            }
            st.Stop();
            Console.WriteLine("Took {0} seconds.  Box tests from hits: {1}; Box tests from misses: {2}. Sum = {3}", st.Elapsed.TotalSeconds, cost1.IntersectionCount, cost3.IntersectionCount, cost1.IntersectionCount + cost3.IntersectionCount);
            Console.ReadLine();
        }
        */
    }
}
