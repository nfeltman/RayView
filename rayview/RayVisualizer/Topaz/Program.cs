using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;
using System.IO;
using System.Diagnostics;

namespace Topaz
{
    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Main2(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void Main2(string[] args)
        {
            // Console.WriteLine("Topaz Running...");
            if (args.Length == 0)
            {
                Console.WriteLine("No command specified. Use -h for help.");
                return;
            }

            string command = args[0];

            if (command.Equals("-h") || command.Equals("-help") || command.Equals("-?"))
            {
                Console.WriteLine("Commands: \n{0}",
                    "-h Display this help.",
                    "-runexp Run an experiment.",
                    "-testunif Test uniform box code."
                    );
            }
            else if (command.Equals("-runexp"))
            {
                var dict = ParseCommandOptions(args, 1);
                if (!dict.HasOne("-build") || !dict.ContainsKey("-eval") || dict["-eval"].Count == 0 || !dict.HasOne("-scene") || !dict.HasOne("-buildrays") || !dict.HasOne("-evalrays") || !dict.HasOne("-o"))
                {
                    Console.WriteLine("Usage: topaz -runexp -build <build method> -eval method_1[ method_k]* -scene <triangle source file> -buildrays <build ray source file> -evalrays <eval ray source file> -o <output file> ");
                    return;
                }
                Func<BuildTriangle[], RaySet, StreamWriter, RBVH2> build = GetBuildMethod(dict["-build"][0]);
                Func<BuildTriangle[]> tris = GetBuildTrianglesForBuild(dict["-scene"][0]);
                Func<RaySet> buildrays = GetRaysFromFile(dict["-buildrays"][0]);
                Func<RaySet> evalrays = GetRaysFromFile(dict["-evalrays"][0]);
                Func<FileStream> output = GetFileWriteStream(dict["-o"][0]);
                Action<RBVH2, RaySet, StreamWriter>[] evalMethods = dict["-eval"].Select(GetEvalMethod).ToArray();
                if (build == null || tris == null || buildrays == null || evalrays == null || output == null) return;
                foreach (var method in evalMethods) if (method == null) return;
                FileStream fileout = output();
                using (fileout)
                {
                    StreamWriter writer = new StreamWriter(fileout);
                    RBVH2 bvh = build(tris(), buildrays(), writer);
                    RaySet eval_rays = evalrays();
                    foreach (var method in evalMethods) method(bvh, eval_rays, writer);
                    writer.Flush();
                }
            }
            else if (command.Equals("-testunif"))
            {
                TestAnalyticUniformFunctions();
            }
            else
            {
                Console.WriteLine("Command not recognized: \'{0}\'", command);
            }
            // Console.WriteLine("Topaz Done.");
        }

        private static bool HasOne<T,U>(this Dictionary<T, IList<U>> dict, T command)
        {
            return dict.ContainsKey(command) && dict[command].Count == 1;
        }

        private static Func<BuildTriangle[], RaySet, StreamWriter, RBVH2> GetBuildMethod(string method)
        {
            if (method.ToLower().Equals("bal50"))
            {
                return (tris, rays, output) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Scene loaded: {0} triangles and {1} rays", tris.Length, "?");
                    Console.Write("Starting build... "); st.Start();
                    RBVH2 build = GeneralBVH2Builder.BuildFullStructure(tris, (ln, lb, rn, rb) => Math.Abs(ln - rn), RBVH5050Factory.ONLY);
                    st.Stop(); Console.WriteLine("done. Time(ms) = {0}", st.ElapsedMilliseconds);
                    return build;
                };
            }
            else if (method.ToLower().Equals("sah50"))
            {
                return (tris, rays, output) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Scene loaded: {0} triangles and {1} rays", tris.Length, "?");
                    Console.Write("Starting build... "); st.Start();
                    RBVH2 build = GeneralBVH2Builder.BuildFullStructure(tris, (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea, RBVH5050Factory.ONLY);
                    st.Stop(); Console.WriteLine("done. Time(ms) = {0}", st.ElapsedMilliseconds);
                    return build;
                };
            }
            else if (method.ToLower().Equals("rtsah"))
            {
                return (tris, rays, output) => {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Scene loaded: {0} triangles and {1} rays", tris.Length, "?");
                    Console.Write("Starting build... "); st.Start();
                    RBVH2 build = GeneralBVH2Builder.BuildFullStructure(tris, (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea, RBVH5050Factory.ONLY);
                    st.Stop(); Console.WriteLine("done. Time(ms) = {0}", st.ElapsedMilliseconds);
                    return TreeOrdering.ApplyRTSAHOrdering(build);
                };
            }/*
            else if (method.ToLower().Equals("ordsah"))
            {
                return (tris, rays, output) =>
                {
                    output.WriteLine("% Nothing here Yet!");
                };
            }*/
            else if (method.ToLower().Equals("srdh"))
            {
                return (tris, rays, output) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.Write("Starting helper build... "); st.Start();
                    BVH2 initialBuild = GeneralBVH2Builder.BuildStructure(tris, new StatelessSplitEvaluator((ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea), BVHNodeFactory.ONLY, CountAggregator.ONLY, 4, true);
                    st.Stop(); Console.WriteLine("done. Time(ms) = {0}", st.ElapsedMilliseconds);

                    Console.Write("Starting ray compilation... "); st.Reset(); st.Start();
                    ShadowRayResults res = ShadowRayCompiler.CompileCasts(rays, initialBuild);
                    st.Stop(); Console.WriteLine("done. Time(ms) = {0}", st.ElapsedMilliseconds);

                    Console.Write("Starting main build... "); st.Reset(); st.Start();
                    RBVH2 build = GeneralBVH2Builder.BuildFullRBVH(res.Triangles, new ShadowRayCostEvaluator(res, 1f));
                    st.Stop(); Console.WriteLine("done. Time(ms) = {0}", st.ElapsedMilliseconds);
                    
                    return build;
                };
            }
            else
            {
                Console.WriteLine("Method \"{0}\" not recognized.  Acceptible: bal50, sah50, rtsah, ordsah, srdh, oraclesah", method);
                return null;
            }
        }

        private static Action<RBVH2, RaySet, StreamWriter> GetEvalMethod(string method)
        {
            if (method.ToLower().Equals("pq"))
            {
                return (build, rays, output) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.Write("Starting standard evaluation... "); st.Start();
                    TraceCost cost = FastFullCostMeasure.GetTotalCost(build, rays.ShadowQueries.Select(q => new Segment3(q.Origin, q.Difference)), 24);
                    st.Stop(); Console.WriteLine("done. Time(ms) = {0}", st.ElapsedMilliseconds);
                    Console.Write("Starting slow standard evaluation... "); st.Reset(); st.Start();
                    TraceCost cost2 = FullCostMeasure.GetTotalCost(build, rays.ShadowQueries.Select(q => new Segment3(q.Origin, q.Difference)));
                    st.Stop(); Console.WriteLine("done. Time(ms) = {0}", st.ElapsedMilliseconds);
                    output.WriteLine("\n% PQ-traversal (fast)");
                    StandardRBVHEvaluationReport(build, cost, output);
                    output.WriteLine("\n% PQ-traversal (slow)");
                    StandardRBVHEvaluationReport(build, cost2, output);
                };
            }
            else if (method.ToLower().Equals("oracle"))
            {
                return (build, rays, output) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.Write("Starting oracle evaluation... "); st.Reset(); st.Start();
                    TraceCost cost = OracleCost.GetTotalCost(build, rays.ShadowQueries.Select(q => new Segment3(q.Origin, q.Difference)));
                    st.Stop(); Console.WriteLine("done. Time(ms) = {0}", st.ElapsedMilliseconds);
                    output.WriteLine("\n% Oracle traversal");
                    StandardRBVHEvaluationReport(build, cost, output);
                };
            }
            else
            {
                Console.WriteLine("Unrecognized evaluation method \"{0}\".  Acceptible: pq, oracle", method);
                return null;
            }
        }

        private static void StandardRBVHEvaluationReport(RBVH2 build, TraceCost cost, StreamWriter output)
        {
            output.WriteLine("{0} {1} % Number Branches (reported/measured)", build.NumBranch, build.RollUp((b, l, r) => l + r + 1, le => 0));
            output.WriteLine("{0} {1} % Number Leaves (reported/measured)", build.NumLeaves, build.RollUp((b, l, r) => l + r, le => 1));
            output.WriteLine("{0} % Number Primitives", build.NumLeaves, build.RollUp((b, l, r) => l + r, le => le.Primitives.Length));
            output.WriteLine("{0} % Height (1-based)", build.RollUp((b, l, r) => Math.Max(l, r) + 1, le => 1));
            output.WriteLine("{0} % Exp[BBoxTests]", cost.BBoxTests.ExpectedValue);
            output.WriteLine("{0} % StD[BBoxTests]", Math.Sqrt(cost.BBoxTests.Variance));
            output.WriteLine("{0} % Exp[PrimTests]", cost.PrimitiveTests.ExpectedValue);
            output.WriteLine("{0} % StD[PrimTests]", Math.Sqrt(cost.PrimitiveTests.Variance));
        }

        private static Func<BuildTriangle[]> GetBuildTrianglesForBuild(string filename)
        {
            if (filename.ToLower().EndsWith(".obj"))
            {
                if (!File.Exists(filename))
                {
                    Console.WriteLine("Could not find .obj file: {0}", filename);
                    return null;
                }
                FileStream f = File.OpenRead(filename);
                return () =>
                    {
                        using (f)
                        {
                            return BuildTools.GetTriangleList(OBJParser.ParseOBJTriangles(f));
                        }
                    };
            }
            else
            {
                Console.WriteLine("Unrecognized extension for triangle source file: {0}", filename.ToLower());
                return null;
            }
        }

        private static Func<RaySet> GetRaysFromFile(string filename)
        {
            if (filename.ToLower().EndsWith(".ray"))
            {
                if (!File.Exists(filename))
                {
                    Console.WriteLine("Could not find .ray file: {0}", filename);
                    return null;
                }
                FileStream f = File.OpenRead(filename);
                return () =>
                {
                    using (f)
                    {
                        return RayFileParser.ReadFromFile2(f);
                    }
                };
            }
            else
            {
                Console.WriteLine("Unrecognized extension for ray source file: {0}", filename.ToLower());
                return null;
            }
        }

        private static Func<FileStream> GetFileWriteStream(string filename)
        {
            /*
            if (!Directory.Exists(filename))
            {
                Console.WriteLine("Directory for file does not exist: {0}", filename);
                return null;
            }
             */
            return () => File.OpenWrite(filename);
        }

        private static Dictionary<string, IList<string>> ParseCommandOptions(string[] args, int startAt)
        {
            Dictionary<string, IList<string>> dict = new Dictionary<string, IList<string>>();
            int k = startAt;
            while ( k < args.Length)
            {
                string option = args[k];
                List<string> rest = new List<string>();
                for (int j = k+1; j < args.Length && !args[j].StartsWith("-"); j++)
                {
                    rest.Add(args[j]);
                    k++;
                }
                dict[option] = rest;
                k++;
            }

            return dict;
        }

        private static void TestAnalyticUniformFunctions()
        {
            Random r = new Random(971297);
            int testSize = 2000000;

            for (int j = 0; j < 1000; j++)
            {
                Console.WriteLine(j);
                ClosedInterval range = new ClosedInterval(0, 100);
                float x1 = range.UniformSample(r), x2 = range.UniformSample(r);
                float y1 = range.UniformSample(r), y2 = range.UniformSample(r);
                float z1 = range.UniformSample(r), z2 = range.UniformSample(r);
                Box3 parent = new Box3(Math.Min(x1, x2), Math.Max(x1, x2), Math.Min(y1, y2), Math.Max(y1, y2), Math.Min(z1, z2), Math.Max(z1, z2));
                x1 = parent.XRange.UniformSample(r); x2 = parent.XRange.UniformSample(r);
                y1 = parent.YRange.UniformSample(r); y2 = parent.YRange.UniformSample(r);
                z1 = parent.ZRange.UniformSample(r); z2 = parent.ZRange.UniformSample(r);
                Box3 left = new Box3(Math.Min(x1, x2), Math.Max(x1, x2), Math.Min(y1, y2), Math.Max(y1, y2), Math.Min(z1, z2), Math.Max(z1, z2));
                x1 = parent.XRange.UniformSample(r); x2 = parent.XRange.UniformSample(r);
                y1 = parent.YRange.UniformSample(r); y2 = parent.YRange.UniformSample(r);
                z1 = parent.ZRange.UniformSample(r); z2 = parent.ZRange.UniformSample(r);
                Box3 right = new Box3(Math.Min(x1, x2), Math.Max(x1, x2), Math.Min(y1, y2), Math.Max(y1, y2), Math.Min(z1, z2), Math.Max(z1, z2));
                //Box3 parent = new Box3(-100, 100, -100, 200, 0, 200);
                //Box3 left = new Box3(35f, 40f, 10f, 90f, 10f, 90f);
                //Box3 right = new Box3(10f, 70f, 40f, 45f, 30f, 70f);
                IntersectionReport est = UniformRays.GetReport(parent, left, right);
                IntersectionReport mea = new IntersectionReport(0, 0, 0, 0, 0, 0);
                for (int k = 0; k < testSize; k++)
                {
                    Ray3 ray = UniformRays.RandomInternalRay(parent, r);
                    bool hitsLeft = left.DoesIntersectRay(ray.Origin, ray.Direction);
                    bool hitsRight = right.DoesIntersectRay(ray.Origin, ray.Direction);
                    if (hitsLeft)
                    {
                        mea.Left++;
                        if (hitsRight)
                        {
                            mea.Right++;
                            mea.Both++;
                        }
                        else
                        {
                            mea.JustLeft++;
                        }
                    }
                    else
                    {
                        if (hitsRight)
                        {
                            mea.Right++;
                            mea.JustRight++;
                        }
                        else
                        {
                            mea.Neither++;
                        }
                    }
                }
                float threshold = 0.002f;
                if (mea.Left / testSize - est.Left > threshold
                    || mea.Right / testSize - est.Right > threshold
                    || mea.JustLeft / testSize - est.JustLeft > threshold
                    || mea.JustRight / testSize - est.JustRight > threshold
                    || mea.Both / testSize - est.Both > threshold
                    || mea.Neither / testSize - est.Neither > threshold)
                {
                    Console.WriteLine("+++ {0}\n -> {1}\n -> {2}", parent, left, right);
                    Console.WriteLine("      Left: {0:0.00000} - {1:0.00000} = {2}", mea.Left / testSize, est.Left, mea.Left / testSize - est.Left);
                    Console.WriteLine("     Right: {0:0.00000} - {1:0.00000} = {2}", mea.Right / testSize, est.Right, mea.Right / testSize - est.Right);
                    Console.WriteLine(" Just Left: {0:0.00000} - {1:0.00000} = {2}", mea.JustLeft / testSize, est.JustLeft, mea.JustLeft / testSize - est.JustLeft);
                    Console.WriteLine("Just Right: {0:0.00000} - {1:0.00000} = {2}", mea.JustRight / testSize, est.JustRight, mea.JustRight / testSize - est.JustRight);
                    Console.WriteLine("      Both: {0:0.00000} - {1:0.00000} = {2}", mea.Both / testSize, est.Both, mea.Both / testSize - est.Both);
                    Console.WriteLine("   Neither: {0:0.00000} - {1:0.00000} = {2}", mea.Neither / testSize, est.Neither, mea.Neither / testSize - est.Neither);
                    Console.WriteLine("===================================");
                }
            }
            Console.ReadLine();
        }
    }
}
