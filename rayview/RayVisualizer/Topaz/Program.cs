using System;
using System.Linq;
using System.Collections.Generic;
using RayVisualizer.Common;
using System.IO;
using System.Diagnostics;
using Mono.Simd;

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
                    "-runexp Run a full build and evaluate experiment.",
                    "-simdtest Run a test of simd capabilities.",
                    "-sweep Run a sweep experiment.",
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
                    bvh.Accept(ConsistencyCheck.ONLY, bvh.Root.Accept(b => b.Content.BBox, l => l.Content.BBox));
                    Console.WriteLine("Consistent.");
                    RaySet eval_rays = evalrays();
                    foreach (var method in evalMethods) method(bvh, eval_rays, writer);
                    writer.Flush();
                }
            }
            else if (command.Equals("-sweep"))
            {
                var dict = ParseCommandOptions(args, 1);
                if (!dict.HasOne("-split") || !dict.HasOne("-scene") || !dict.HasOne("-buildrays") || !dict.HasOne("-o"))
                {
                    Console.WriteLine("{0} {1} {2} {3} ", !dict.HasOne("-split"), !dict.HasOne("-scene") , !dict.HasOne("-buildrays") , !dict.HasOne("-o"));
                    //Console.WriteLine("Usage: topaz -sweep -split <split method> -scene <triangle source file> -buildrays <build ray source file> -o <output file> ");
                    return;
                }
                Func<BuildTriangle[]> tris = GetBuildTrianglesForBuild(dict["-scene"][0]);
                Action<BuildTriangle[], RaySet, StreamWriter> split = GetSplitMethod(dict["-split"][0]);
                Func<RaySet> buildrays = GetRaysFromFile(dict["-buildrays"][0]);
                Func<FileStream> output = GetFileWriteStream(dict["-o"][0]);
                if (tris == null || buildrays == null || output == null) return;
                FileStream fileout = output();
                using (fileout)
                {
                    StreamWriter writer = new StreamWriter(fileout);
                    split(tris(), buildrays(), writer);
                    writer.Flush();
                }
            }
            else if (command.Equals("-simdtest"))
            {
                Console.WriteLine("SIMD Acceleration Mode: {0}.", SimdRuntime.AccelMode);
                Console.WriteLine("Vector4f (+) Acceleration Requirement: {0}. Met: {1}.", SimdRuntime.MethodAccelerationMode(typeof(Vector4f), "op_Addition", typeof(Vector4f), typeof(Vector4f)),
                    SimdRuntime.IsMethodAccelerated(typeof(Vector4f), "op_Addition", typeof(Vector4f), typeof(Vector4f)));
                Console.WriteLine("Vector4f (-) Acceleration Requirement: {0}. Met: {1}.", SimdRuntime.MethodAccelerationMode(typeof(Vector4f), "op_Subtraction", typeof(Vector4f), typeof(Vector4f)),
                    SimdRuntime.IsMethodAccelerated(typeof(Vector4f), "op_Subtraction", typeof(Vector4f), typeof(Vector4f)));
                Console.WriteLine("Vector4f (*) Acceleration Requirement: {0}. Met: {1}.", SimdRuntime.MethodAccelerationMode(typeof(Vector4f), "op_Multiply", typeof(Vector4f), typeof(Vector4f)),
                    SimdRuntime.IsMethodAccelerated(typeof(Vector4f), "op_Multiply", typeof(Vector4f), typeof(Vector4f)));
                Console.WriteLine("Vector4f (/) Acceleration Requirement: {0}. Met: {1}.", SimdRuntime.MethodAccelerationMode(typeof(Vector4f), "op_Division", typeof(Vector4f), typeof(Vector4f)),
                    SimdRuntime.IsMethodAccelerated(typeof(Vector4f), "op_Division", typeof(Vector4f), typeof(Vector4f)));
                Console.WriteLine("Vector4f (min) Acceleration Requirement: {0}. Met: {1}.", SimdRuntime.MethodAccelerationMode(typeof(VectorOperations), "Min", typeof(Vector4f), typeof(Vector4f)),
                    SimdRuntime.IsMethodAccelerated(typeof(VectorOperations), "Min", typeof(Vector4f), typeof(Vector4f)));
                Console.WriteLine("Vector4f (max) Acceleration Requirement: {0}. Met: {1}.", SimdRuntime.MethodAccelerationMode(typeof(VectorOperations), "Max", typeof(Vector4f), typeof(Vector4f)),
                    SimdRuntime.IsMethodAccelerated(typeof(VectorOperations), "Max", typeof(Vector4f), typeof(Vector4f)));

                int numiters = 100000000;
                Console.WriteLine("Speed test. Number iterations: {0}", numiters);
                Stopwatch st = new Stopwatch();
                float a1 = -.2f, a2 = -.3f, a3 = -.3f, a4 = -.5f;
                float b1 = 0.1f, b2 = 0.3f, b3 = 0.6f, b4 = -.3f;
                float z1 = 0f, z2 = 0f, z3 = 0f, z4 = 0f;
                float y1 = 0f, y2 = 0f, y3 = 0f, y4 = 0f;
                st.Start();
                for (int k = 0; k < numiters; k++)
                {
                    z1 = z1 * z1 - y1 * y1 + a1; y1 = 2f * z1 * y1 + b1;
                    z2 = z2 * z2 - y2 * y2 + a2; y2 = 2f * z2 * y2 + b2;
                    z3 = z3 * z3 - y3 * y3 + a3; y3 = 2f * z3 * y3 + b3;
                    z4 = z4 * z4 - y4 * y4 + a4; y4 = 2f * z4 * y4 + b4;
                }
                st.Stop();
                Console.WriteLine("Non-SIMD: Took {0} ms.", st.ElapsedMilliseconds);
                Console.WriteLine("<{0}, {1}, {2}, {3}>, <{4}, {5}, {6}, {7}>", z1, z2, z3, z4, y1, y2, y3, y4);
                st.Reset();
                st.Start();
                Vector4f a = new Vector4f(-.2f, -.3f, -.3f, -.5f);
                Vector4f b = new Vector4f(.1f, .3f, .6f, -.3f);
                Vector4f z = new Vector4f(0, 0, 0, 0);
                Vector4f y = new Vector4f(0, 0, 0, 0);
                for (int k = 0; k < numiters; k++)
                {
                    z = z * z - y * y + a;
                    y = z * y * 2 + b;
                }
                st.Stop();
                Console.WriteLine("SIMD: Took {0} ms.", st.ElapsedMilliseconds);
                Console.WriteLine("{0} {1}", z, y);
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
                    Console.WriteLine("Starting BAL50 build. "); st.Start();
                    RBVH2 build = GeneralBVH2Builder.BuildFullStructure(tris, (ln, lb, rn, rb) => Math.Abs(ln - rn), RBVH5050Factory.ONLY, TripleAASplitter.ONLY);
                    st.Stop(); Console.WriteLine("Done with BAL50 build.  Time(ms) = {0}", st.ElapsedMilliseconds);
                    StandardRBVHStatsReport(build, output);
                    return build;
                };
            }
            else if (method.ToLower().Equals("sah50"))
            {
                return (tris, rays, output) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Starting SAH50 build. "); st.Start();
                    RBVH2 build = GeneralBVH2Builder.BuildFullStructure(tris, (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea, RBVH5050Factory.ONLY, TripleAASplitter.ONLY);
                    st.Stop(); Console.WriteLine("Done with SAH50 build. Time(ms) = {0}", st.ElapsedMilliseconds);
                    StandardRBVHStatsReport(build, output);
                    return build;
                };
            }
            else if (method.ToLower().Equals("rtsah"))
            {
                return (tris, rays, output) => {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Starting RTSAH build. "); st.Start();
                    RBVH2 build = GeneralBVH2Builder.BuildFullStructure(tris, (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea, RBVH5050Factory.ONLY, TripleAASplitter.ONLY);
                    StandardRBVHStatsReport(build, output);
                    st.Stop(); Console.WriteLine("Done with RTSAH build. Time(ms) = {0}", st.ElapsedMilliseconds);
                    Console.WriteLine("Applying RTSAH ordering. "); st.Reset(); st.Start();
                    build = TreeOrdering.ApplyRTSAHOrdering(build);
                    st.Stop(); Console.WriteLine("Done with RTSAH ordering. Time(ms) = {0}", st.ElapsedMilliseconds);
                    /*build.PrefixEnumerate(
                        (b) => { if (b.Depth <= 5) Console.WriteLine("[{0} ({3}): {1} {2}]", b.ID, b.PLeft, b.BBox.SurfaceArea, b.Depth); },
                        (l) => { if (l.Depth <= 5) Console.WriteLine("<{0} {1}: {2} {3}>", l.ID, l.Depth, l.Primitives.Length, l.BBox.SurfaceArea); });*/
                    return build;
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
                    Console.WriteLine("Starting SRDH helper build. "); st.Start();
                    BVH2 initialBuild = GeneralBVH2Builder.BuildStructure(tris, new StatelessSplitEvaluator((ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea), BVHNodeFactory.ONLY, BoundsCountAggregator.ONLY, TripleAASplitter.ONLY, 4);
                    st.Stop(); Console.WriteLine("Done with SRDH helper build. Time(ms) = {0}", st.ElapsedMilliseconds);

                    Console.WriteLine("Starting SRDH ray compilation. "); st.Reset(); st.Start();
                    ShadowRayResults res = ShadowRayCompiler.CompileCasts(rays, initialBuild);
                    st.Stop(); Console.WriteLine("Done with SRDH ray compilation. Time(ms) = {0}", st.ElapsedMilliseconds);

                    Console.WriteLine("Starting SRDH main build. "); st.Reset(); st.Start();
                    RBVH2 build = GeneralBVH2Builder.BuildFullStructure(res.Triangles, new ShadowRayCostEvaluator(res, 1f), RBVHNodeFactory.ONLY, BoundsCountAggregator.ONLY, TripleAASplitter.ONLY);
                    st.Stop(); Console.WriteLine("Done with SRDH main build. Time(ms) = {0}", st.ElapsedMilliseconds);

                    StandardRBVHStatsReport(build, output);
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
                    Console.WriteLine("Starting standard PQ evaluation. "); st.Start();
                    FullTraceResult cost = FullCostMeasure.GetTotalCost(build, rays.ShadowQueries.Select(q => new Segment3(q.Origin, q.Difference)));
                    st.Stop(); Console.WriteLine("Done with PQ evaluation. Time(ms) = {0}", st.ElapsedMilliseconds);

                    PrintSimple("PQ NumRays", cost.NumRays, output);
                    PrintSimple("PQ NumHits", cost.NumHits, output);
                    PrintCost("PQ (Spine) ", cost.Spine, output);
                    PrintCost("PQ (Side) ", cost.SideTrees, output);
                    PrintCost("PQ (Non-Hit) ", cost.NonHit, output);
                    PrintCost("PQ (Total) ", cost.Spine + cost.SideTrees + cost.NonHit, output);
                };
            }
            else if (method.ToLower().Equals("oracle"))
            {
                return (build, rays, output) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Starting oracle evaluation. "); st.Reset(); st.Start();
                    OracleTraceResult cost = OracleCost.GetTotalCost(build, rays.ShadowQueries.Select(q => new Segment3(q.Origin, q.Difference)));
                    st.Stop(); Console.WriteLine("Done with Oracle evaluation. Time(ms) = {0}", st.ElapsedMilliseconds);

                    PrintSimple("Oracle NumRays", cost.NumRays, output);
                    PrintSimple("Oracle NumHits", cost.NumHits, output);
                    PrintCost("Oracle (Hit) ", cost.Hit, output);
                    PrintCost("Oracle (Non-Hit) ", cost.NonHit, output);
                    PrintCost("Oracle (Total) ", cost.NonHit + cost.Hit, output); 
                };
            }
            else
            {
                Console.WriteLine("Unrecognized evaluation method \"{0}\".  Acceptible: pq, oracle", method);
                return null;
            }
        }

        private static Action<BuildTriangle[], RaySet, StreamWriter> GetSplitMethod(string method)
        {
            if (method.ToLower().Equals("aa3"))
            {
                return (tris, rays, output) =>
                {
                    int numBins = 100;
                    // calculate splits
                    Stopwatch st = new Stopwatch();
                    Box3 centroidBounds = BuildTools.FindCentroidBound(tris, 0, tris.Length);

                    Console.WriteLine("Starting SRDH helper build. "); st.Start();
                    BVH2 initialBuild = GeneralBVH2Builder.BuildStructure(tris, new StatelessSplitEvaluator((ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea), BVHNodeFactory.ONLY, BoundsCountAggregator.ONLY, TripleAASplitter.ONLY, 4);
                    st.Stop(); Console.WriteLine("Done with SRDH helper build. Time(ms) = {0}", st.ElapsedMilliseconds);

                    Console.WriteLine("Starting SRDH ray compilation. "); st.Reset(); st.Start();
                    ShadowRayResults res = ShadowRayCompiler.CompileCasts(rays, initialBuild);
                    st.Stop(); Console.WriteLine("Done with SRDH ray compilation. Time(ms) = {0}", st.ElapsedMilliseconds);
                    
                    ShadowRayCostEvaluator eval = new ShadowRayCostEvaluator(res, 1.0f);

                    output.WriteLine("\"numbins\" \"x\" {0}", numBins);
                    output.WriteLine("\"splitParamsX\" \"sub\" {0}", centroidBounds.XRange.Min);
                    output.WriteLine("\"splitParamsX\" \"times\" {0}", numBins / centroidBounds.XRange.Size);
                    SplitterHelper.RunSplitSweepTest(
                        (split, cost)=>{
                            Console.WriteLine(split);
                            output.WriteLine("\"xSplit\" \"{0}\" {1}", split, cost);
                        }, tris, new XAASplitSeries(centroidBounds.XRange.Min, numBins / centroidBounds.XRange.Size), numBins, eval, BoundsCountAggregator.ONLY);
                    output.WriteLine("\"numbins\" \"y\" {0}", numBins);
                    output.WriteLine("\"splitParamsY\" \"sub\" {0}", centroidBounds.YRange.Min);
                    output.WriteLine("\"splitParamsY\" \"times\" {0}", numBins / centroidBounds.YRange.Size);
                    SplitterHelper.RunSplitSweepTest(
                        (split, cost) =>
                        {
                            Console.WriteLine(split);
                            output.WriteLine("\"ySplit\" \"{0}\" {1}", split, cost);
                        }, tris, new YAASplitSeries(centroidBounds.YRange.Min, numBins / centroidBounds.YRange.Size), numBins, eval, BoundsCountAggregator.ONLY);
                    output.WriteLine("\"numbins\" \"z\" {0}", numBins);
                    output.WriteLine("\"splitParamsZ\" \"sub\" {0}", centroidBounds.ZRange.Min);
                    output.WriteLine("\"splitParamsZ\" \"times\" {0}", numBins / centroidBounds.ZRange.Size);
                    SplitterHelper.RunSplitSweepTest(
                        (split, cost) =>
                        {
                            Console.WriteLine(split);
                            output.WriteLine("\"zSplit\" \"{0}\" {1}", split, cost);
                        }, tris, new XAASplitSeries(centroidBounds.ZRange.Min, numBins / centroidBounds.ZRange.Size), numBins, eval, BoundsCountAggregator.ONLY);
                };
            }
            else
            {
                Console.WriteLine("Unrecognized split method \"{0}\".  Acceptible: AA3", method);
                return null;
            }
        }

        private static void StandardRBVHStatsReport(RBVH2 build, StreamWriter output)
        {
            //output.WriteLine("{0} {1} % Number Branches (reported/measured)", build.NumBranch, build.RollUp((b, l, r) => l + r + 1, le => 0));
            //output.WriteLine("{0} {1} % Number Leaves (reported/measured)", build.NumLeaves, build.RollUp((b, l, r) => l + r, le => 1));
            PrintSimple("Number Leaves", build.NumLeaves, output);
            PrintSimple("Height", build.RollUp((b, l, r) => Math.Max(l, r) + 1, le => 1), output);
        }

        private static void PrintCost(string statPrefix, TraceCost cost, StreamWriter output)
        {
            PrintRandomVariable(statPrefix + "BBox Tests", cost.BBoxTests, output);
            PrintRandomVariable(statPrefix + "Prim Tests", cost.PrimitiveTests, output);
        }

        private static void PrintRandomVariable(string stat, RandomVariable rv, StreamWriter output)
        {
            output.WriteLine("\"{0}\" \"EXP\" {1}", stat, rv.ExpectedValue);
            output.WriteLine("\"{0}\" \"STD\" {1}", stat, Math.Sqrt(rv.Variance));
        }

        private static void PrintSimple(string stat, double d, StreamWriter output)
        {
            output.WriteLine("\"{0}\" \"total\" {1}", stat, d);
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

            for (int j = 0; j < 50; j++)
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
        }
    }
}
