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
    using System.Security.Permissions;
    using Topaz.FileParser;
    
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
                Console.WriteLine("Commands: \n" +
                    "-h Display this help.\n" +
                    "-makebvh Builds and outputs a bvh.\n" +
                    "-runexp Run a full build and evaluate experiment.\n" +
                    "-simdtest Run a test of simd capabilities.\n" +
                    "-sweep Run a sweep experiment.\n" +
                    "-testunif Test uniform box code."
                    );
            }
            else if (command.Equals("-evalbvh"))
            {
                var dict = ParseCommandOptions(args, 1);
                //foreach (string s in dict.Keys) Console.WriteLine(s+": "+dict[s].Count);
                if (!dict.HasPositive("-method") || !dict.HasOne("-bvh") || !dict.HasOne("-scene") || !dict.HasOne("-evalrays") || !dict.HasOne("-o"))
                {
                    Console.WriteLine("Usage: topaz -evalbvh -method method_1[ method_k]* -bvh <bvh file> -scene <triangle source file> -evalrays <eval ray source file> -o <output file> ");
                    return;
                }
                Func<List<Triangle>, Tree<RBVH2Branch, RBVH2Leaf>> build = GetBVHFromFile(dict["-bvh"][0]);
                Func<Tuple<OBJBackedBuildTriangle[], List<Triangle>>> tris = GetOBJBuildTrianglesForBuild(dict["-scene"][0]);
                Func<RaySet> evalrays = dict.ContainsKey("-evalrays") ? GetRaysFromFile(dict["-evalrays"][0]) : null;
                TopazStreamWriter output = GetFileWriteStream(dict["-o"][0]);
                Action<RBVH2, RaySet, TopazStreamWriter>[] evalMethods = dict["-method"].Select(GetEvalMethod).ToArray();
                if (build == null || tris == null || evalrays == null || output == null) return;
                foreach (var method in evalMethods) if (method == null) return;
                using (output)
                {
                    RBVH2 bvh = build(tris().Item2);
                    bvh.Accept(ConsistencyCheck<RBVH2Branch, RBVH2Leaf>.ONLY, bvh.Root.Accept(b => b.Content.BBox, l => l.Content.BBox));
                    output.StandardRBVHStatsReport(bvh);
                    RaySet eval_rays = evalrays();
                    foreach (var method in evalMethods) method(bvh, eval_rays, output);
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
                    BackedRBVHConstantFactory<OBJBackedBuildTriangle>.ONLY_5050,
                    BackedRBVHConstantFactory<OBJBackedBuildTriangle>.ONLY_FTB,
                    BackedRBVHConstantFactory<OBJBackedBuildTriangle>.ONLY_BTF,
                    BackedRBVHNodeFactory<OBJBackedBuildTriangle>.ONLY,
                    BackedRBVHConstantFactory<OBJBackedBuildTriangle>.ONLY_5050);
                Func<Tuple<OBJBackedBuildTriangle[], List<Triangle>>> tris = GetOBJBuildTrianglesForBuild(dict["-scene"][0]);
                Func<RaySet> buildrays = GetRaysFromFile(dict["-buildrays"][0]);
                TopazStreamWriter output = GetFileWriteStream(dict["-o"][0]);
                if (build == null || tris == null || output == null) return;
                using (output)
                {
                    Tuple<OBJBackedBuildTriangle[], List<Triangle>> scene = tris();
                    IList<Triangle> backing = scene.Item2;
                    BackedRBVH2 bvh = build(scene.Item1, objTri => backing[objTri.OBJIndex], (objIndex, counter) => new OBJBackedBuildTriangle(counter, backing[objIndex], objIndex), buildrays());
                    bvh.Accept(new ConsistencyCheck<BackedRBVH2Branch, BackedRBVH2Leaf, int>(i => backing[i]), bvh.Root.Accept(b => b.Content.BBox, l => l.Content.BBox));
                    BVHTextParser.WriteBVH_Text(bvh, output);
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
                Action<BasicBuildTriangle[], RaySet, TopazStreamWriter> split = GetSplitMethod(dict["-split"][0]);
                Func<RaySet> buildrays = GetRaysFromFile(dict["-buildrays"][0]);
                TopazStreamWriter output = GetFileWriteStream(dict["-o"][0]);
                if (tris == null || buildrays == null || output == null) return;
                using (output)
                {
                    split(tris(), buildrays(), output);
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
        private static bool HasPositive<T, U>(this Dictionary<T, IList<U>> dict, T command)
        {
            return dict.ContainsKey(command) && dict[command].Count > 0;
        }

        private static Func<Tri[], Func<Tri, Triangle>, Func<PrimT, int, Tri>, RaySet, Tree<TBranch, TLeaf>> GetBuildMethod<Tri, PrimT, TriB, TBranch, TLeaf>
            (string method,
            NodeFactory<TriB, TBranch, TLeaf, Tree<TBranch, TLeaf>, Unit, BoundAndCount> fact5050,
            NodeFactory<TriB, TBranch, TLeaf, Tree<TBranch, TLeaf>, Unit, BoundAndCount> factFTB,
            NodeFactory<TriB, TBranch, TLeaf, Tree<TBranch, TLeaf>, Unit, BoundAndCount> factBTF,
            NodeFactory<TriB, TBranch, TLeaf, Tree<TBranch, TLeaf>, TraversalKernel, BoundAndCount> factWeighted, 
            NodeFactory<TriB, TBranch, TLeaf, Tree<TBranch, TLeaf>, Unit, BoundAndCount> factBVHHelper)
            where Tri:TriB, CenterIndexable, Bounded
            where TBranch : Boxed, Weighted
            where TLeaf : Boxed, PrimCountable, Primitived<PrimT>
        {
            if (method.ToLower().Equals("sah-5050"))
            {
                return (tris, mapping, constructor, rays) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Starting SAH50 build. "); st.Start();
                    Tree<TBranch, TLeaf> build = GeneralBVH2Builder.BuildStructure(tris, 
                        new StatelessSplitEvaluator((ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea), 
                        fact5050, 
                        BoundsCountAggregator<Tri>.ONLY, 
                        TripleAASplitter.ONLY, 
                        1);
                    st.Stop(); Console.WriteLine("Done with SAH50 build. Time(ms) = {0}", st.ElapsedMilliseconds);
                    return build;
                };
            }
            else if (method.ToLower().Equals("sah-ftb"))
            {
                return (tris, mapping, constructor, rays) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Starting SAH50 build. "); st.Start();
                    Tree<TBranch, TLeaf> build = GeneralBVH2Builder.BuildStructure(tris,
                        new StatelessSplitEvaluator((ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea),
                        factFTB,
                        BoundsCountAggregator<Tri>.ONLY,
                        TripleAASplitter.ONLY,
                        1);
                    st.Stop(); Console.WriteLine("Done with SAH50 build. Time(ms) = {0}", st.ElapsedMilliseconds);
                    return build;
                };
            }
            else if (method.ToLower().Equals("sah-btf"))
            {
                return (tris, mapping, constructor, rays) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Starting SAH50 build. "); st.Start();
                    Tree<TBranch, TLeaf> build = GeneralBVH2Builder.BuildStructure(tris,
                        new StatelessSplitEvaluator((ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea),
                        factBTF,
                        BoundsCountAggregator<Tri>.ONLY,
                        TripleAASplitter.ONLY,
                        1);
                    st.Stop(); Console.WriteLine("Done with SAH50 build. Time(ms) = {0}", st.ElapsedMilliseconds);
                    return build;
                };
            }
            else if (method.ToLower().Equals("rtsah"))
            {
                return (tris, mapping, constructor, rays) => {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Starting RTSAH build. "); st.Start();
                    Tree<TBranch, TLeaf> build = GeneralBVH2Builder.BuildStructure(
                        tris, 
                        new StatelessSplitEvaluator((ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea), 
                        fact5050, 
                        BoundsCountAggregator<Tri>.ONLY,
                        TripleAASplitter.ONLY,
                        1);
                    st.Stop(); Console.WriteLine("Done with RTSAH build. Time(ms) = {0}", st.ElapsedMilliseconds);
                    Console.WriteLine("Applying RTSAH ordering. "); st.Reset(); st.Start();
                    TreeOrdering.ApplyRTSAHOrdering(build);
                    //build.Root.Accept(br => { Console.WriteLine(br.Content.PLeft); }, le => { });
                    st.Stop(); Console.WriteLine("Done with RTSAH ordering. Time(ms) = {0}", st.ElapsedMilliseconds);
                    return build;
                };
            }
            else if (method.ToLower().Equals("srdh-10"))
            {
                return (tris, mapping, constructor, rays) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Starting SRDHv2 helper build. "); st.Start();
                    Tree<TBranch, TLeaf> initialBuild = GeneralBVH2Builder.BuildStructure<Tri, TriB, Unit, Unit, Unit, Unit, TBranch, TLeaf, Tree<TBranch, TLeaf>, BoundAndCount>
                        (tris, new StatelessSplitEvaluator((ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea), factBVHHelper, BoundsCountAggregator<Tri>.ONLY, TripleAASplitter.ONLY, 4);
                    st.Stop(); Console.WriteLine("Done with SRDH helper build. Time(ms) = {0}", st.ElapsedMilliseconds);

                    Console.WriteLine("Starting SRDHv2 ray compilation. "); st.Reset(); st.Start();
                    ShadowRayResults<Tri> res = ShadowRayCompiler.CompileCasts<PrimT, Tri, TBranch, TLeaf>(rays.ShadowQueries.Select(q => new Segment3(q.Origin, q.Difference)), initialBuild, mapping, constructor);
                    st.Stop(); Console.WriteLine("Done with SRDH ray compilation. Time(ms) = {0}", st.ElapsedMilliseconds);

                    Console.WriteLine("Starting SRDHv2 main build. "); st.Reset(); st.Start();
                    Tree<TBranch, TLeaf> build = GeneralBVH2Builder.BuildStructure(
                        res.Triangles,
                        new SRDHEvaluator<Tri>(res, 1f, new KernelOptions()
                            {
                                LeftFirst = true,
                                RightFirst = true,
                                BackToFront = false,
                                FrontToBack = false
                            }),
                        factWeighted,
                        BoundsCountAggregator<Tri>.ONLY,
                        TripleAASplitter.ONLY,
                        1);
                    st.Stop(); Console.WriteLine("Done with SRDHv2 main build. Time(ms) = {0}", st.ElapsedMilliseconds);

                    return build;
                };
            }
            else if (method.ToLower().Equals("srdh-ftb"))
            {
                return (tris, mapping, constructor, rays) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Starting SRDHv2 helper build. "); st.Start();
                    Tree<TBranch, TLeaf> initialBuild = GeneralBVH2Builder.BuildStructure<Tri, TriB, Unit, Unit, Unit, Unit, TBranch, TLeaf, Tree<TBranch, TLeaf>, BoundAndCount>
                        (tris, new StatelessSplitEvaluator((ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea), factBVHHelper, BoundsCountAggregator<Tri>.ONLY, TripleAASplitter.ONLY, 4);
                    st.Stop(); Console.WriteLine("Done with SRDH helper build. Time(ms) = {0}", st.ElapsedMilliseconds);

                    Console.WriteLine("Starting SRDHv2 ray compilation. "); st.Reset(); st.Start();
                    ShadowRayResults<Tri> res = ShadowRayCompiler.CompileCasts<PrimT, Tri, TBranch, TLeaf>(rays.ShadowQueries.Select(q => new Segment3(q.Origin, q.Difference)), initialBuild, mapping, constructor);
                    st.Stop(); Console.WriteLine("Done with SRDH ray compilation. Time(ms) = {0}", st.ElapsedMilliseconds);

                    Console.WriteLine("Starting SRDHv2 main build. "); st.Reset(); st.Start();
                    Tree<TBranch, TLeaf> build = GeneralBVH2Builder.BuildStructure(
                        res.Triangles,
                        new SRDHEvaluator<Tri>(res, 1f, new KernelOptions()
                        {
                            LeftFirst = false,
                            RightFirst = false,
                            BackToFront = false,
                            FrontToBack = true
                        }),
                        factWeighted,
                        BoundsCountAggregator<Tri>.ONLY,
                        TripleAASplitter.ONLY,
                        1);
                    st.Stop(); Console.WriteLine("Done with SRDHv2 main build. Time(ms) = {0}", st.ElapsedMilliseconds);

                    return build;
                };
            }
            else if (method.ToLower().Equals("srdh-btf"))
            {
                return (tris, mapping, constructor, rays) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Starting SRDHv2 helper build. "); st.Start();
                    Tree<TBranch, TLeaf> initialBuild = GeneralBVH2Builder.BuildStructure<Tri, TriB, Unit, Unit, Unit, Unit, TBranch, TLeaf, Tree<TBranch, TLeaf>, BoundAndCount>
                        (tris, new StatelessSplitEvaluator((ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea), factBVHHelper, BoundsCountAggregator<Tri>.ONLY, TripleAASplitter.ONLY, 4);
                    st.Stop(); Console.WriteLine("Done with SRDH helper build. Time(ms) = {0}", st.ElapsedMilliseconds);

                    Console.WriteLine("Starting SRDHv2 ray compilation. "); st.Reset(); st.Start();
                    ShadowRayResults<Tri> res = ShadowRayCompiler.CompileCasts<PrimT, Tri, TBranch, TLeaf>(rays.ShadowQueries.Select(q => new Segment3(q.Origin, q.Difference)), initialBuild, mapping, constructor);
                    st.Stop(); Console.WriteLine("Done with SRDH ray compilation. Time(ms) = {0}", st.ElapsedMilliseconds);

                    Console.WriteLine("Starting SRDHv2 main build. "); st.Reset(); st.Start();
                    Tree<TBranch, TLeaf> build = GeneralBVH2Builder.BuildStructure(
                        res.Triangles,
                        new SRDHEvaluator<Tri>(res, 1f, new KernelOptions()
                        {
                            LeftFirst = false,
                            RightFirst = false,
                            BackToFront = true,
                            FrontToBack = false
                        }),
                        factWeighted,
                        BoundsCountAggregator<Tri>.ONLY,
                        TripleAASplitter.ONLY,
                        1);
                    st.Stop(); Console.WriteLine("Done with SRDHv2 main build. Time(ms) = {0}", st.ElapsedMilliseconds);

                    return build;
                };
            }
            else if (method.ToLower().Equals("srdh-btf-ftb"))
            {
                return (tris, mapping, constructor, rays) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Starting SRDHv2 helper build. "); st.Start();
                    Tree<TBranch, TLeaf> initialBuild = GeneralBVH2Builder.BuildStructure<Tri, TriB, Unit, Unit, Unit, Unit, TBranch, TLeaf, Tree<TBranch, TLeaf>, BoundAndCount>
                        (tris, new StatelessSplitEvaluator((ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea), factBVHHelper, BoundsCountAggregator<Tri>.ONLY, TripleAASplitter.ONLY, 4);
                    st.Stop(); Console.WriteLine("Done with SRDH helper build. Time(ms) = {0}", st.ElapsedMilliseconds);

                    Console.WriteLine("Starting SRDHv2 ray compilation. "); st.Reset(); st.Start();
                    ShadowRayResults<Tri> res = ShadowRayCompiler.CompileCasts<PrimT, Tri, TBranch, TLeaf>(rays.ShadowQueries.Select(q => new Segment3(q.Origin, q.Difference)), initialBuild, mapping, constructor);
                    st.Stop(); Console.WriteLine("Done with SRDH ray compilation. Time(ms) = {0}", st.ElapsedMilliseconds);

                    Console.WriteLine("Starting SRDHv2 main build. "); st.Reset(); st.Start();
                    Tree<TBranch, TLeaf> build = GeneralBVH2Builder.BuildStructure(
                        res.Triangles,
                        new SRDHEvaluator<Tri>(res, 1f, new KernelOptions()
                        {
                            LeftFirst = false,
                            RightFirst = false,
                            BackToFront = true,
                            FrontToBack = true
                        }),
                        factWeighted,
                        BoundsCountAggregator<Tri>.ONLY,
                        TripleAASplitter.ONLY,
                        1);
                    st.Stop(); Console.WriteLine("Done with SRDHv2 main build. Time(ms) = {0}", st.ElapsedMilliseconds);

                    return build;
                };
            }
            else if (method.ToLower().Equals("srdh-all"))
            {
                return (tris, mapping, constructor, rays) =>
                {
                    Stopwatch st = new Stopwatch();
                    Console.WriteLine("Starting SRDHv2 helper build. "); st.Start();
                    Tree<TBranch, TLeaf> initialBuild = GeneralBVH2Builder.BuildStructure<Tri, TriB, Unit, Unit, Unit, Unit, TBranch, TLeaf, Tree<TBranch, TLeaf>, BoundAndCount>
                        (tris, new StatelessSplitEvaluator((ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea), factBVHHelper, BoundsCountAggregator<Tri>.ONLY, TripleAASplitter.ONLY, 4);
                    st.Stop(); Console.WriteLine("Done with SRDH helper build. Time(ms) = {0}", st.ElapsedMilliseconds);

                    Console.WriteLine("Starting SRDHv2 ray compilation. "); st.Reset(); st.Start();
                    ShadowRayResults<Tri> res = ShadowRayCompiler.CompileCasts<PrimT, Tri, TBranch, TLeaf>(rays.ShadowQueries.Select(q => new Segment3(q.Origin, q.Difference)), initialBuild, mapping, constructor);
                    st.Stop(); Console.WriteLine("Done with SRDH ray compilation. Time(ms) = {0}", st.ElapsedMilliseconds);

                    Console.WriteLine("Starting SRDHv2 main build. "); st.Reset(); st.Start();
                    Tree<TBranch, TLeaf> build = GeneralBVH2Builder.BuildStructure(
                        res.Triangles,
                        new SRDHEvaluator<Tri>(res, 1f, new KernelOptions()
                        {
                            LeftFirst = true,
                            RightFirst = true,
                            BackToFront = true,
                            FrontToBack = true
                        }),
                        factWeighted,
                        BoundsCountAggregator<Tri>.ONLY,
                        TripleAASplitter.ONLY,
                        1);
                    st.Stop(); Console.WriteLine("Done with SRDHv2 main build. Time(ms) = {0}", st.ElapsedMilliseconds);

                    return build;
                };
            }
            else
            {
                Console.WriteLine("Method \"{0}\" not recognized.  Acceptible: sah-5050, sah-FTB, sah-BTF, rtsah, srdh", method);
                return null;
            }
        }

        private static Action<RBVH2, RaySet, TopazStreamWriter> GetEvalMethod(string method)
        {
            if (method.ToLower().Equals("pq"))
            {
                return EvalMethods.PerformPQEvaluation;
            }
            else if (method.ToLower().Equals("rayhisto"))
            {
                return EvalMethods.PerformRayHistogramEvaluation;
            }
            else
            {
                Console.WriteLine("Unrecognized evaluation method \"{0}\".  Acceptible: pq, rayhisto", method);
                return null;
            }
        }

        private static Action<BasicBuildTriangle[], RaySet, TopazStreamWriter> GetSplitMethod(string method)
        {
            if (method.ToLower().Equals("aa3"))
            {
                return SweepTests.PerformAA3SweepTest;
            }
            else if (method.ToLower().Equals("rad1"))
            {
                return SweepTests.PerformRadialSweepTest;
            }
            else
            {
                Console.WriteLine("Unrecognized split method \"{0}\".  Acceptible: AA3", method);
                return null;
            }
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


        private static Func<Tuple<OBJBackedBuildTriangle[], List<Triangle>>> GetOBJBuildTrianglesForBuild(string filename)
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
                        List<Triangle> backing = OBJParser.ParseOBJTriangles(f);
                        return new Tuple<OBJBackedBuildTriangle[], List<Triangle>>(backing.GetOBJTriangleList(), backing);
                    }
                };
            }
            else
            {
                Console.WriteLine("Requires OBJ source file. Found extension: {0}", filename.ToLower());
                return null;
            }
        }

        private static Func<List<Triangle>, Tree<RBVH2Branch, RBVH2Leaf>> GetBVHFromFile(string filename)
        {
            if (filename.ToLower().EndsWith(".bvh"))
            {
                if (!File.Exists(filename))
                {
                    Console.WriteLine("Could not find .bvh file: {0}", filename);
                    return null;
                }
                FileStream f = File.OpenRead(filename);
                return (tris) =>
                {
                    using (f)
                    {
                        return BVHTextParser.ReadRBVH_Text(new StreamReader(f), tris);
                    }
                };
            }
            else
            {
                Console.WriteLine("Unrecognized extension for bvh file: {0}", filename.ToLower());
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

        private static TopazStreamWriter GetFileWriteStream(string filename)
        {
            return new TopazStreamWriter(filename);
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
