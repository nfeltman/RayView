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
                if (args.Length != 5)
                {
                    Console.WriteLine("Usage: topaz -runexp <method> <triangle source file> <ray source file> <output file>");
                    return;
                }
                Action<BuildTriangle[], RaySet, StreamWriter> method = GetMethod(args[1]);
                Func<BuildTriangle[]> tris = GetBuildTrianglesForBuild(args[2]);
                Func<RaySet> rays = GetRaysForBuild(args[3]);
                Func<FileStream> output = GetFileWriteStream(args[4]);
                if (method == null || tris == null || rays == null || output == null) return;
                FileStream fileout = output();
                using (fileout)
                {
                    StreamWriter writer = new StreamWriter(fileout);
                    method(tris(), rays(), writer);
                    writer.Flush();
                }
            }
            else
            {
                Console.WriteLine("Command not recognized: \'{0}\'", command);
            }
            // Console.WriteLine("Topaz Done.");
        }

        private static Action<BuildTriangle[], RaySet, StreamWriter> GetMethod(string method)
        {
            if (method.ToLower().Equals("bal50"))
            {
                return (tris, rays, output) =>
                {
                    Console.WriteLine("Scene loaded: {0} triangles and {1} rays", tris.Length, "?");
                    Console.Write("Starting build... ");
                    RBVH2 build = GeneralBVH2Builder.BuildFullStructure(tris, (ln, lb, rn, rb) => Math.Abs(ln - rn), RBVH5050Factory.ONLY);
                    Console.WriteLine("done.");

                    Stopwatch st = new Stopwatch();
                    Console.Write("Starting evaluation... ");
                    st.Start();
                    TraceCost cost = FastFullCostMeasure.GetTotalCost(build, rays.ShadowQueries.Select(q => new Segment3(q.Origin, q.Difference)), 24);
                    st.Stop();
                    Console.WriteLine("done. Time(ms) = {0}", st.ElapsedMilliseconds);
                    StandardRBVHEvaluationReport(build, cost, output);
                };
            }
            else if (method.ToLower().Equals("sah50"))
            {
                return (tris, rays, output) =>
                {
                    Console.WriteLine("Scene loaded: {0} triangles and {1} rays", tris.Length, "?");
                    Console.Write("Starting build... ");
                    RBVH2 build = GeneralBVH2Builder.BuildFullStructure(tris, (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea, RBVH5050Factory.ONLY);
                    Console.WriteLine("done.");

                    Stopwatch st = new Stopwatch();
                    Console.Write("Starting evaluation... ");
                    st.Start();
                    TraceCost cost = FastFullCostMeasure.GetTotalCost(build, rays.ShadowQueries.Select(q => new Segment3(q.Origin, q.Difference)), 24);
                    st.Stop();
                    Console.WriteLine("done. Time(ms) = {0}", st.ElapsedMilliseconds);
                    StandardRBVHEvaluationReport(build, cost, output);
                };
            }
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
            }
            else if (method.ToLower().Equals("srdh"))
            {
                return (tris, rays, output) =>
                {
                    output.WriteLine("% Nothing here Yet!");/*
                    BVH2 initialBuild = GeneralBVH2Builder.BuildStructure(tris, new StatelessSplitEvaluator((ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea), BVHNodeFactory.ONLY, CountAggregator.ONLY, 4, true);
                    ShadowRayResults res = ShadowRayCompiler.CompileCasts(rays, initialBuild);
                    RBVH2 build = GeneralBVH2Builder.BuildFullRBVH(res.Triangles, new ShadowRayCostEvaluator(res, 1f));*/
                };
            }
            else if (method.ToLower().Equals("oraclesah"))
            {
                return (tris, rays, output) =>
                {
                    Console.WriteLine("Scene loaded: {0} triangles and {1} rays", tris.Length, "?");
                    Console.Write("Starting build... ");
                    RBVH2 build = GeneralBVH2Builder.BuildFullStructure(tris, (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea, RBVH5050Factory.ONLY);
                    Console.WriteLine("done.");
                    Console.Write("Starting evaluation... ");
                    TraceCost cost = OracleCost.GetTotalCost(build, rays.ShadowQueries.Select(q => new Segment3(q.Origin, q.Difference)));
                    Console.WriteLine("done.");
                    StandardRBVHEvaluationReport(build, cost, output);
                };
            }
            else
            {
                Console.WriteLine("Method \'{0}\' not recognized.  Acceptible: bal50, sah50, rtsah, ordsah, srdh, oraclesah", method);
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

        private static Func<RaySet> GetRaysForBuild(string filename)
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
    }
}
