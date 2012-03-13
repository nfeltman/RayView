using System;
using System.Linq;
using System.Collections.Generic;
using RayVisualizer.Common;
using System.IO;
using System.Diagnostics;
using Mono.Simd;

namespace Topaz
{
    using BVH2 = Tree<BVH2Branch, BVH2Leaf>;
    using RBVH2 = Tree<RBVH2Branch, RBVH2Leaf>;
    using BackedBVH2 = Tree<BackedBVH2Branch, BackedBVH2Leaf>;
    using BackedRBVH2 = Tree<BackedRBVH2Branch, BackedRBVH2Leaf>;
    
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
                Environment.Exit(-1);
            }
        }

        static void Main2(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No command specified. Use -h for help.");
                return;
            }

            string command = args[0];

            if (command.Equals("-h") || command.Equals("-help") || command.Equals("-?"))
            {
                Console.WriteLine("Commands: \n{0}",
                    "-makebvh Builds and outputs a bvh.",
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
                if (!dict.HasOne("-build") || !dict.ContainsKey("-eval") || dict["-eval"].Count == 0 || !dict.HasOne("-scene") || !dict.HasOne("-evalrays") || !dict.HasOne("-o"))
                {
                    Console.WriteLine("Usage: topaz -runexp -build <build method> -eval method_1[ method_k]* -scene <triangle source file> -buildrays <build ray source file> -evalrays <eval ray source file> -o <output file> ");
                    return;
                }
                Func<BasicBuildTriangle[], Func<BasicBuildTriangle, Triangle>, Func<Triangle, int, BasicBuildTriangle>, RaySet, RBVH2> build = GetBuildMethod<BasicBuildTriangle, Triangle, BasicBuildTriangle, RBVH2Branch, RBVH2Leaf>
                    (dict["-build"][0], RBVH5050Factory<BasicBuildTriangle>.ONLY, RBVHNodeFactory<BasicBuildTriangle>.ONLY, RBVH5050Factory<BasicBuildTriangle>.ONLY);
                Func<BasicBuildTriangle[]> tris = GetBuildTrianglesForBuild(dict["-scene"][0]);
                Func<RaySet> buildrays = GetRaysFromFile(dict["-buildrays"][0]);
                Func<RaySet> evalrays = dict.ContainsKey("-evalrays") ? GetRaysFromFile(dict["-evalrays"][0]) : null;
                Func<FileStream> output = GetFileWriteStream(dict["-o"][0]);
                Action<RBVH2, RaySet, StreamWriter>[] evalMethods = dict["-eval"].Select(GetEvalMethod).ToArray();
                if (build == null || tris == null || evalrays == null || output == null) return;
                foreach (var method in evalMethods) if (method == null) return;
                FileStream fileout = output();
                using (fileout)
                {
                    StreamWriter writer = new StreamWriter(fileout);
                    RBVH2 bvh = build(tris(), t => t.T, (tri, counter) => new BasicBuildTriangle(tri, counter), buildrays());
                    StandardRBVHStatsReport(bvh, writer);
                    bvh.Accept(ConsistencyCheck<RBVH2Branch,RBVH2Leaf>.ONLY, bvh.Root.Accept(b => b.Content.BBox, l => l.Content.BBox));
                    RaySet eval_rays = evalrays();
                    foreach (var method in evalMethods) method(bvh, eval_rays, writer);
                    writer.Flush();
                }
            }
            else if (command.Equals("-makebvh"))
            {
                var dict = ParseCommandOptions(args, 1);
                if (!dict.HasOne("-build") || !dict.HasOne("-scene") || !dict.HasOne("-buildrays") || !dict.HasOne("-o"))
                {
                    Console.WriteLine("Usage: topaz -makebvh -build <build method> -scene <triangle source file> -buildrays <build ray source file> -o <output file> ");
                    return;
                }
                Func<OBJBackedBuildTriangle[], Func<OBJBackedBuildTriangle, Triangle>, Func<int, int, OBJBackedBuildTriangle>, RaySet, BackedRBVH2> build = GetBuildMethod<OBJBackedBuildTriangle, int, OBJBackedBuildTriangle, BackedRBVH2Branch, BackedRBVH2Leaf>
                    (dict["-build"][0], 
                    BackedRBVH5050Factory<OBJBackedBuildTriangle>.ONLY,
                    BackedRBVHNodeFactory<OBJBackedBuildTriangle>.ONLY,
                    BackedRBVH5050Factory<OBJBackedBuildTriangle>.ONLY);
                Func<Tuple<OBJBackedBuildTriangle[], IList<Triangle>>> tris = GetOBJBuildTrianglesForBuild(dict["-scene"][0]);
                Func<RaySet> buildrays = GetRaysFromFile(dict["-buildrays"][0]);
                Func<FileStream> output = GetFileWriteStream(dict["-o"][0]);
                if (build == null || tris == null || output == null) return;
                FileStream fileout = output();
                using (fileout)
                {
                    Tuple<OBJBackedBuildTriangle[], IList<Triangle>> scene = tris();
                    IList<Triangle> backing = scene.Item2;
                    BackedRBVH2 bvh = build(scene.Item1, objTri => backing[objTri.OBJIndex], (objIndex, counter) => new OBJBackedBuildTriangle(counter, backing[objIndex], objIndex), buildrays());
                    bvh.Accept(new ConsistencyCheck<BackedRBVH2Branch, BackedRBVH2Leaf, int>(i => backing[i]), bvh.Root.Accept(b => b.Content.BBox, l => l.Content.BBox));
                    StreamWriter writer = new StreamWriter(fileout);
                    writer.Write("267534 202");
                    bvh.PrefixEnumerate(
                        (branch)=>
                        {
                            writer.Write(" 2 {0}", branch.PLeft);
                        },
                        (leaf)=>
                        {
                            writer.Write(" 3 {0}", leaf.Primitives.Length);
                            foreach (int i in leaf.Primitives)
                                writer.Write(" {0}", i);
                        });
                    writer.Write(" 9215");
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
                Func<BasicBuildTriangle[]> tris = GetBuildTrianglesForBuild(dict["-scene"][0]);
                Action<BasicBuildTriangle[], RaySet, StreamWriter> split = GetSplitMethod(dict["-split"][0]);
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
                TopazMisc.TestSIMD();
            }
            else if (command.Equals("-testunif"))
            {
                TopazMisc.TestAnalyticUniformFunctions();
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

        private static Func<Tri[], Func<Tri, Triangle>, Func<PrimT, int, Tri>, RaySet, Tree<TBranch, TLeaf>> GetBuildMethod<Tri, PrimT, TriB, TBranch, TLeaf>
            (string method, 
            NodeFactory<TriB, TBranch, TLeaf, Tree<TBranch, TLeaf>, Unit, BoundAndCount> fact5050, 
            NodeFactory<TriB, TBranch, TLeaf, Tree<TBranch, TLeaf>, float, BoundAndCount> factWeighted, 
            NodeFactory<TriB, TBranch, TLeaf, Tree<TBranch, TLeaf>, Unit, BoundAndCount> factBVHHelper)
            where Tri:TriB, CenterIndexable, Bounded
            where TBranch : Boxed, Weighted
            where TLeaf : Boxed, PrimCountable, Primitived<PrimT>
        {
            if (method.ToLower().Equals("bal50"))
            {
                return (tris, mapping, constructor, rays) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Starting BAL50 build. "); st.Start();
                    Tree<TBranch, TLeaf> build = GeneralBVH2Builder.BuildFullStructure<Tri, TriB, TBranch, TLeaf, Tree<TBranch, TLeaf>>(tris, (ln, lb, rn, rb) => Math.Abs(ln - rn), fact5050, TripleAASplitter.ONLY);
                    st.Stop(); Console.WriteLine("Done with BAL50 build.  Time(ms) = {0}", st.ElapsedMilliseconds);
                    return build;
                };
            }
            else if (method.ToLower().Equals("sah50"))
            {
                return (tris, mapping, constructor, rays) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Starting SAH50 build. "); st.Start();
                    Tree<TBranch, TLeaf> build = GeneralBVH2Builder.BuildFullStructure(tris, (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea, fact5050, TripleAASplitter.ONLY);
                    st.Stop(); Console.WriteLine("Done with SAH50 build. Time(ms) = {0}", st.ElapsedMilliseconds);
                    return build;
                };
            }
            else if (method.ToLower().Equals("rtsah"))
            {
                return (tris, mapping, constructor, rays) => {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Starting RTSAH build. "); st.Start();
                    Tree<TBranch, TLeaf> build = GeneralBVH2Builder.BuildFullStructure(tris, (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea, fact5050, TripleAASplitter.ONLY);
                    st.Stop(); Console.WriteLine("Done with RTSAH build. Time(ms) = {0}", st.ElapsedMilliseconds);
                    Console.WriteLine("Applying RTSAH ordering. "); st.Reset(); st.Start();
                    TreeOrdering.ApplyRTSAHOrdering(build);
                    //build.Root.Accept(br => { Console.WriteLine(br.Content.PLeft); }, le => { });
                    st.Stop(); Console.WriteLine("Done with RTSAH ordering. Time(ms) = {0}", st.ElapsedMilliseconds);
                    return build;
                };
            }
            else if (method.ToLower().Equals("srdh"))
            {
                return (tris, mapping, constructor, rays) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Starting SRDH helper build. "); st.Start();
                    Tree<TBranch, TLeaf> initialBuild = GeneralBVH2Builder.BuildStructure<Tri, TriB, Unit, Unit, Unit, Unit, TBranch, TLeaf, Tree<TBranch, TLeaf>, BoundAndCount>
                        (tris, new StatelessSplitEvaluator((ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea), factBVHHelper, BoundsCountAggregator<Tri>.ONLY, TripleAASplitter.ONLY, 4);
                    st.Stop(); Console.WriteLine("Done with SRDH helper build. Time(ms) = {0}", st.ElapsedMilliseconds);

                    Console.WriteLine("Starting SRDH ray compilation. "); st.Reset(); st.Start();
                    ShadowRayResults<Tri> res = ShadowRayCompiler.CompileCasts<PrimT,Tri,TBranch,TLeaf>(rays.ShadowQueries.Select(q => new Segment3(q.Origin, q.Difference)), initialBuild, mapping, constructor);
                    st.Stop(); Console.WriteLine("Done with SRDH ray compilation. Time(ms) = {0}", st.ElapsedMilliseconds);

                    Console.WriteLine("Starting SRDH main build. "); st.Reset(); st.Start();
                    Tree<TBranch, TLeaf> build = GeneralBVH2Builder.BuildFullStructure(res.Triangles, new ShadowRayCostEvaluator<Tri>(res, 1f), factWeighted, BoundsCountAggregator<Tri>.ONLY, TripleAASplitter.ONLY);
                    st.Stop(); Console.WriteLine("Done with SRDH main build. Time(ms) = {0}", st.ElapsedMilliseconds);

                    return build;
                };
            }
            else
            {
                Console.WriteLine("Method \"{0}\" not recognized.  Acceptible: bal50, sah50, rtsah, srdh", method);
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
                    FullTraceResult cost = FullCostMeasure.GetTotalCost(build, rays.ShadowQueries);
                    st.Stop(); Console.WriteLine("Done with PQ evaluation. Time(ms) = {0}", st.ElapsedMilliseconds);

                    PrintSimple("PQ NumRays", cost.NumRays, output);
                    PrintSimple("PQ NumBothHit", cost.topazHit_mantaHit, output);
                    PrintSimple("PQ NumBothMiss", cost.topazMiss_mantaMiss, output);
                    PrintSimple("PQ NumTopazMissMantaHit", cost.topazMiss_mantaHit, output);
                    PrintSimple("PQ NumTopazHitMantaMiss", cost.topazHit_mantaMiss, output);
                    PrintSimple("PQ Disagreement", cost.Disagreement, output);
                    PrintCost("PQ (Spine Oracle) ", cost.SpineOracle, output);
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

        private static Action<BasicBuildTriangle[], RaySet, StreamWriter> GetSplitMethod(string method)
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
                };
            }
            else if (method.ToLower().Equals("rad1"))
            {
                return (tris, rays, output) =>
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
                };
            }
            else
            {
                Console.WriteLine("Unrecognized split method \"{0}\".  Acceptible: AA3", method);
                return null;
            }
        }

        private static void SweepTestHelper(string dim, int numBins, float less, float times, StreamWriter output, BasicBuildTriangle[] tris, ShadowRayCostEvaluator<BasicBuildTriangle> eval, ShadowRayShuffleState evaluatorState)
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

            output.WriteLine("\"numSplits{0}\" \"count\" {1}", dim, numBins-1);
            output.WriteLine("\"splits{0}\" \"array\"{1}", dim, splits_locs);
            output.WriteLine("\"sah{0}\" \"array\"{1}", dim, sah_vals);
            output.WriteLine("\"srdh{0}\" \"array\"{1}", dim, srdh_vals);
        }

        private static void StandardRBVHStatsReport(RBVH2 build, StreamWriter output)
        {
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

        private static Func<BasicBuildTriangle[]> GetBuildTrianglesForBuild(string filename)
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

        private static Func<Tuple<OBJBackedBuildTriangle[], IList<Triangle>>> GetOBJBuildTrianglesForBuild(string filename)
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
                        IList<Triangle> backing = OBJParser.ParseOBJTriangles(f);
                        return new Tuple<OBJBackedBuildTriangle[], IList<Triangle>>(backing.GetOBJTriangleList(), backing);
                    }
                };
            }
            else
            {
                Console.WriteLine("Requires OBJ source file. Found extension: {0}", filename.ToLower());
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
            return () => File.Open(filename, FileMode.Create, FileAccess.Write);
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
    }
}
