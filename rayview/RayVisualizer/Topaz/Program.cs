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
                    "-runexp Run an experiment."
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
                Action<RBVH2, RaySet, StreamWriter>[] evalMethods = dict["eval"].Select(GetEvalMethod).ToArray();
                if (build == null || tris == null || buildrays == null || evalrays == null || output == null) return;
                foreach (var method in evalMethods) if (method == null) return;
                FileStream fileout = output();
                using (fileout)
                {
                    StreamWriter writer = new StreamWriter(fileout);
                    RBVH2 bvh = build(tris(), buildrays(), writer);
                    foreach (var method in evalMethods) method(bvh, evalrays(), writer);
                    writer.Flush();
                }
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
            if (method.ToLower().Equals("bal"))
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
            else if (method.ToLower().Equals("sah"))
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
            }/*
            else if (method.ToLower().Equals("rtsah"))
            {
                return (tris, rays, output) => {
                    output.WriteLine("% Nothing here Yet!");
                };
            }
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
                Console.WriteLine("Method \'{0}\' not recognized.  Acceptible: bal50, sah50, rtsah, ordsah, srdh, oraclesah", method);
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
                    output.WriteLine("\n% PQ-traversal");
                    StandardRBVHEvaluationReport(build, cost, output);
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
                    output.WriteLine("% Oracle traversal");
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
    }
}
