﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace AnalysisEngine
{
    static class BuildAnalysis
    {

        public static void BuildAndWriteBVHsNoRays(string tracesPath)
        {
            BVH2 given = BVH2Parser.ReadFromFile(new FileStream(tracesPath + "powerplant\\raw_bvh.txt", FileMode.Open, FileAccess.Read));
            RaySet allrays = RayFileParser.ReadFromFile(new FileStream(tracesPath + "powerplant\\casts.txt", FileMode.Open, FileAccess.Read));

            PrintBVHReport(given, "given");
            RaySet set = allrays.CastOnlyFilter((r, i) => i % 20 == 0 && r.Depth >= 1);
            FHRayResults res = RayCompiler.CompileCasts(set, given);
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
            RaySet allrays = RayFileParser.ReadFromFile(new FileStream(tracesPath + "powerplant\\casts.txt", FileMode.Open, FileAccess.Read));

            PrintBVHReport(given, "given");
            RaySet set = allrays.CastOnlyFilter((r, i) => r.Depth==0);
            FHRayResults res = RayCompiler.CompileCasts(set, given);
            Console.WriteLine("Beginning Runs!");
            
            for (int k = 0; k <= 10; k++)
            {
                float exponent = (k / 10f) * (.3f) + .7f;
                BuildTriangle[] tris = BVH2Builder.GetTriangleList(given);
                BinaryWriter writer = new BinaryWriter(new FileStream(tracesPath + "powerplant\\Built_BVHs\\WithRays\\eyerays_" + k, FileMode.CreateNew));
                BVH2 created = BVH2Builder.BuildFullBVH(tris, new RayCostEvaluator(res, exponent), new RayCostEvaluator.RayShuffleState(res));
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
                BVH2 createdN = BVH2Builder.BuildFullBVH(tris, createNuExpCostEstimator(exponent));
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

        public static void ReadAndCompareMethods(string tracesPath)
        {
            BVH2 given = BVH2Parser.ReadFromFile(new FileStream(tracesPath + "powerplant\\raw_bvh.txt", FileMode.Open, FileAccess.Read));
            RaySet allrays = RayFileParser.ReadFromFile(new FileStream(tracesPath + "powerplant\\casts.txt", FileMode.Open, FileAccess.Read));
            StreamWriter writer = null;// new StreamWriter(tracesPath + "powerplant\\results\\RaysVNoRays.txt", true);

            PrintBVHReport(given, "given");
            RaySet setBounces = allrays.CastOnlyFilter((r, i) => r.Depth>=1);
            FHRayResults resBounces = RayCompiler.CompileCasts(setBounces, given);
            RaySet setEye = allrays.CastOnlyFilter((r, i) => r.Depth==0);
            FHRayResults resEye = RayCompiler.CompileCasts(setEye, given);
            Console.WriteLine("Beginning Compare!");

            writer.WriteLine("% x | T_U(build[P_U*nu^x]) | T_U(build[P_U*mu^x]) | T_U(build[(P_B+10)*nu^x]) | T_U(build[(P_E+10)*nu^x]) | T_B(build[P_U*nu^x]) | T_B(build[P_U*mu^x]) | T_B(build[(P_B+10)*nu^x]) | T_B(build[(P_E+10)*nu^x]) | T_E(build[P_U*nu^x]) | T_E(build[P_U*mu^x]) | T_E(build[(P_B+10)*nu^x]) | T_E(build[(P_E+10)*nu^x])");
            Thread t = new Thread(() => {});
            t.Start();
            t.Join();
            for (int k = 0; k <= 10; k++)
            {
                float exponent = (k / 10f) * (.3f) + .7f;
                BVH2 bvh_nuNoRays = BVH2Parser.ReadFromFile(new FileStream(tracesPath + "powerplant\\Built_BVHs\\NoRays\\nu_" + k, FileMode.Open));
                BVH2 bvh_muNoRays = BVH2Parser.ReadFromFile(new FileStream(tracesPath + "powerplant\\Built_BVHs\\NoRays\\mu_" + k, FileMode.Open));
                BVH2 bvh_bounces = BVH2Parser.ReadFromFile(new FileStream(tracesPath + "powerplant\\Built_BVHs\\WithRays\\bounces_" + k, FileMode.Open));
                BVH2 bvh_eyeRays = BVH2Parser.ReadFromFile(new FileStream(tracesPath + "powerplant\\Built_BVHs\\WithRays\\eyerays_" + k, FileMode.Open));
                float v0=0, v1=0, v2=0, v3=0, v4=0, v5=0, v6=0, v7=0, v8=0, v9=0, v10=0, v11=0;
                Thread t1 = new Thread(()=>{v0 = bvh_nuNoRays.BranchSurfaceArea();});
                Thread t2 = new Thread(()=>{v1 = bvh_muNoRays.BranchSurfaceArea();});
                Thread t3 = new Thread(()=>{v2 = bvh_bounces.BranchSurfaceArea();});
                Thread t4 = new Thread(()=>{v3 = bvh_eyeRays.BranchSurfaceArea();});
                t1.Start(); t2.Start(); t3.Start(); t4.Start();
                t1.Join(); t2.Join(); t3.Join(); t4.Join();
                t1 = new Thread(()=>{v4 = bvh_nuNoRays.BranchTraversalCost(resBounces);});
                t2 = new Thread(()=>{v5 = bvh_muNoRays.BranchTraversalCost(resBounces);});
                t3 = new Thread(()=>{v6 = bvh_bounces.BranchTraversalCost(resBounces);});
                t4 = new Thread(()=>{v7 = bvh_eyeRays.BranchTraversalCost(resBounces);});
                t1.Start(); t2.Start(); t3.Start(); t4.Start();
                t1.Join(); t2.Join(); t3.Join(); t4.Join();
                t1 = new Thread(()=>{v8 = bvh_nuNoRays.BranchTraversalCost(resEye);});
                t2 = new Thread(()=>{v9 = bvh_muNoRays.BranchTraversalCost(resEye);});
                t3 = new Thread(()=>{v10 = bvh_bounces.BranchTraversalCost(resEye);});
                t4 = new Thread(()=>{v11 = bvh_eyeRays.BranchTraversalCost(resEye);});
                t1.Start(); t2.Start(); t3.Start(); t4.Start();
                t1.Join(); t2.Join(); t3.Join(); t4.Join();
                writer.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}",
                    exponent, v0, v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11);
                Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}",
                    exponent, v0, v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11);
                Console.WriteLine("Done with "+k+" at "+DateTime.Now);
            }

            writer.Close();
        }

        public static void BasicSAHBuildTest(string tracesPath)
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

        public static void BasicRDHBuildTest(string tracesPath)
        {
            Stopwatch st = new Stopwatch();
            BVH2 given = BVH2Parser.ReadFromFile(new FileStream(tracesPath + "powerplant\\raw_bvh.txt", FileMode.Open, FileAccess.Read));
            RaySet allrays = RayFileParser.ReadFromFile(new FileStream(tracesPath + "powerplant\\casts.txt", FileMode.Open, FileAccess.Read));

            RaySet set = allrays.CastOnlyFilter((r, i) => i % 200 == 0 && r.Depth>=1);
            FHRayResults res = RayCompiler.CompileCasts(set, given);
            BuildTriangle[] tris = BVH2Builder.GetTriangleList(given);
            st.Start();
            BVH2Builder.BuildBVH(tris, new RayCostEvaluator(res, .9f), new RayCostEvaluator.RayShuffleState(res), 4, false);
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
