using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;
using System.IO;
using System.Diagnostics;

namespace AnalysisEngine
{
    static class BuildAnalysis
    {
        public static void RunBVHBuildSuite(string tracesPath)
        {
            BVH2 given = BVH2Parser.ReadFromFile(new FileStream(tracesPath + "powerplant\\raw_bvh.txt", FileMode.Open, FileAccess.Read));
            RaySet[] allrays = RayFileParser.ReadFromFile(new FileStream(tracesPath + "powerplant\\casts.txt", FileMode.Open, FileAccess.Read));
            StreamWriter writer = new StreamWriter(tracesPath + "powerplant\\BVH_Building.txt");

            PrintBVHReport(given, "given");
            RaySet set = allrays[1].Filter((r, i) => i%20==0 &&(r.Kind == RayKind.FirstHit_Hit || r.Kind == RayKind.FirstHit_Miss));
            for (int k = 2; k < allrays.Length; k++)
                set = set + allrays[k].Filter((r, i) => i%20==0 &&(r.Kind == RayKind.FirstHit_Hit || r.Kind == RayKind.FirstHit_Miss));
            RayQuery[] rays = set.FlattenAndCopy();
            FHRayResults res = RayCompiler.TooledCompileQuerySet(set,given,10000,i=>Console.WriteLine("Compiling: {0}/{1}",i,rays.Length));
            Console.WriteLine("Beginning Runs!");
            //BuildTreeWithRayData(given, res);

            writer.Close();
        }

        public static void BuildAndWriteBVHs(string tracesPath)
        {
            BVH2 given = BVH2Parser.ReadFromFile(new FileStream(tracesPath + "powerplant\\raw_bvh.txt", FileMode.Open, FileAccess.Read));
            RaySet[] allrays = RayFileParser.ReadFromFile(new FileStream(tracesPath + "powerplant\\casts.txt", FileMode.Open, FileAccess.Read));

            PrintBVHReport(given, "given");
            RaySet set = allrays[1].Filter((r, i) => i % 20 == 0 && (r.Kind == RayKind.FirstHit_Hit || r.Kind == RayKind.FirstHit_Miss));
            for (int k = 2; k < allrays.Length; k++)
                set = set + allrays[k].Filter((r, i) => i % 20 == 0 && (r.Kind == RayKind.FirstHit_Hit || r.Kind == RayKind.FirstHit_Miss));
            RayQuery[] rays = set.FlattenAndCopy();
            FHRayResults res = RayCompiler.TooledCompileQuerySet(set, given, 10000, i => Console.WriteLine("Compiling: {0}/{1}", i, rays.Length));
            Console.WriteLine("Beginning Runs!");

            for (int k = 0; k <= 10; k++)
            {
                float exponent = (k / 10f) * (.3f) + .7f;
                BuildTriangle[] tris = BVH2Builder.GetTriangleList(given);
                BVH2 createdN = BVH2Builder.BuildFullBVH(tris, createNuExpCostEstimator(exponent));
                BinaryWriter writer = new BinaryWriter(new FileStream(tracesPath + "powerplant\\Built_BVHs\\NoRays\\nu_" + k, FileMode.CreateNew));
                createdN.WriteToFile(writer);
                writer.Close();
                tris = BVH2Builder.GetTriangleList(given);
                BVH2 createdM = BVH2Builder.BuildFullBVH(tris, createMuExpCostEstimator(exponent));
                writer = new BinaryWriter(new FileStream(tracesPath + "powerplant\\Built_BVHs\\NoRays\\mu_" + k, FileMode.CreateNew));
                createdM.WriteToFile(writer);
                writer.Close();
                Console.WriteLine("{0}% done!", (k / .1f));
            }

        }

        public static void BuildTreeWithRayData(string tracesPath)
        {
            BVH2 given = BVH2Parser.ReadFromFile(new FileStream(tracesPath + "powerplant\\raw_bvh.txt", FileMode.Open, FileAccess.Read));
            RaySet[] allrays = RayFileParser.ReadFromFile(new FileStream(tracesPath + "powerplant\\casts.txt", FileMode.Open, FileAccess.Read));

            PrintBVHReport(given, "given");
            RaySet set = allrays[1].Filter((r, i) => (r.Kind == RayKind.FirstHit_Hit || r.Kind == RayKind.FirstHit_Miss));
            for (int k = 2; k < allrays.Length; k++)
                set = set + allrays[k].Filter((r, i) => (r.Kind == RayKind.FirstHit_Hit || r.Kind == RayKind.FirstHit_Miss));
            RayQuery[] rays = set.FlattenAndCopy();
            FHRayResults res = RayCompiler.TooledCompileQuerySet(set, given, 10000, i => Console.WriteLine("Compiling: {0}/{1}", i, rays.Length));
            Console.WriteLine("Beginning Runs!");


            for (int k = 0; k <= 10; k++)
            {
                float exponent = (k / 10f) * (.3f) + .7f;
                BuildTriangle[] tris = BVH2Builder.GetTriangleList(given);
                BinaryWriter writer = new BinaryWriter(new FileStream(tracesPath + "powerplant\\Built_BVHs\\WithRays\\allrays_" + k, FileMode.CreateNew));
                BVH2 created = BVH2Builder.BuildFullBVH(tris, new RayCostEvaluator(res, exponent), new RayCostEvaluator.RayShuffleState() { hitMax = res.Hits.Length, missMax = res.Misses.Length });
                created.WriteToFile(writer);
                writer.Close();
                Console.WriteLine("{0}% done!", (k / .1f));
            }
        }

        public static void BuildComparisonTests(BVH2 given, FHRayResults res, StreamWriter writer)
        {
            writer.WriteLine("x | T_U(build[P_U*nu^x]) | T_U(build[P_U*mu^x]) | T_R(build[P_U*nu^x]) | T_R(build[P_U*mu^x])");
            for (int k = 0; k <= 10; k++)
            {
                float exponent = (k / 30f) * (.3f) + .7f;
                BuildTriangle[] tris = BVH2Builder.GetTriangleList(given);
                //BVH2 createdN = BVH2Builder.BuildFullBVH(tris, createNuExpCostEstimator(exponent));
                tris = BVH2Builder.GetTriangleList(given);
                BVH2 createdM = BVH2Builder.BuildFullBVH(tris, createMuExpCostEstimator(exponent));
                writer.WriteLine("{0} {1}", 
                    exponent, 
                    //createdN.BranchSurfaceArea(),
                    //createdM.BranchSurfaceArea(),
                    //createdN.BranchTraversalCost(res),
                    createdM.BranchTraversalCost(res));
                Console.WriteLine("{0}% done!", (k/.1f));
            }
        }

        public static void BasicBuildTest(string tracesPath)
        {
            Stopwatch st = new Stopwatch();
            BVH2 given = BVH2Parser.ReadFromFile(new FileStream(tracesPath + "powerplant\\raw_bvh.txt", FileMode.Open, FileAccess.Read));
            BuildTriangle[] tris = BVH2Builder.GetTriangleList(given);
            st.Start();
            BVH2Builder.BuildBVHTest(tris);
            st.Stop();
            Console.WriteLine("Took {0:.000} seconds", st.Elapsed.TotalSeconds);
            Console.ReadLine();
        }

        public static Func<int, Box3, int, Box3, float> createNuExpCostEstimator(float sizeExpo)
        {
            return (left_nu, left_box, right_nu, right_box) => (float)(Math.Pow(left_nu, sizeExpo) * left_box.SurfaceArea + Math.Pow(right_nu, sizeExpo) * right_box.SurfaceArea);
        }
        public static Func<int, Box3, int, Box3, float> createMuExpCostEstimator(float sizeExpo)
        {
            return (left_nu, left_box, right_nu, right_box) => (float)(Math.Pow(left_nu - 1, sizeExpo) * left_box.SurfaceArea + Math.Pow(right_nu - 1, sizeExpo) * right_box.SurfaceArea);
        }

        public static void PrintBVHReport(BVH2 bvh, string name)
        {
            Console.WriteLine("Printing BVH \"{0}\" with {1} leaves.", name, bvh.NumLeaves);
            Console.WriteLine("\t             NumPrims = {0}", bvh.NumPrims());
            Console.WriteLine("\t          MaxLeafSize = {0}", bvh.MaxLeafSize());
            Console.WriteLine("\t             MaxDepth = {0}", bvh.MaxDepth());
            Console.WriteLine("\t    BranchSurfaceArea = {0}", bvh.BranchSurfaceArea());
            Console.WriteLine("\t      LeafSurfaceArea = {0}", bvh.LeafSurfaceArea());
            Console.WriteLine("\tScaledLeafSurfaceArea = {0}", bvh.ScaledLeafSurfaceArea());
        }
    }
}
