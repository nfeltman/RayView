using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;
using System.IO;

namespace AnalysisEngine
{
    static class StaticEstimatorAnalysis
    {
        public static void RunSAHAnalysisNoRays(string tracesPath)
        {
            BVH2 bvh = BVH2Parser.ReadFromFile(new FileStream(tracesPath + "crown\\bvh.txt", FileMode.Open, FileAccess.Read));
            StreamWriter writer = new StreamWriter(tracesPath + "crown\\Full_TU_vs_PU.txt");
            Full_TU_vs_PU(bvh, writer);
            writer.Close();
        }

        public static void RunFullSAHAnalysis(string tracesPath)
        {
            StreamWriter writer = new StreamWriter(tracesPath + "powerplant\\Full_T_vs_P.txt");
            Console.WriteLine("Reading BVH");
            BVH2 bvh = BVH2Parser.ReadFromFile(new FileStream(tracesPath + "powerplant\\bvh.txt", FileMode.Open, FileAccess.Read));
            Console.WriteLine("Reading Casts");
            RaySet allrays = RayFileParser.ReadFromFile1(new FileStream(tracesPath + "powerplant\\casts.txt", FileMode.Open, FileAccess.Read));
            RaySet rays = allrays.CastOnlyFilter((r, i) => r.Depth>=1);
            Console.Write("Starting Analysis");
            Full_T_vs_P(bvh, rays, writer);
            writer.Close();
        }

        public static void Full_TU_vs_PU(BVH2 bvh, StreamWriter writer)
        {
            // rolls over the tuple (surface area subsum, leaf count)
            bvh.RollUpNodes(
                (b, leftSum, rightSum) =>
                {
                    float subSAsum = leftSum.Item1 + rightSum.Item1 + b.Content.BBox.SurfaceArea;
                    int subLeafSum = leftSum.Item2 + rightSum.Item2;
                    writer.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", b.Content.Depth,
                        subSAsum, subLeafSum, b.BBox().SurfaceArea,
                        leftSum.Item1, leftSum.Item2, b.Left.BBox().SurfaceArea,
                        rightSum.Item1, rightSum.Item2, b.Right.BBox().SurfaceArea);
                    return new Tuple<float, int>(subSAsum, subLeafSum);
                },
                l => new Tuple<float, int>(0, 1));
        }

        public static void Full_T_vs_P(BVH2 bvh, RaySet rays, StreamWriter writer)
        {
            RayOrderInspectionCounter ops = new RayOrderInspectionCounter(bvh.NumBranch);
            RayOrderTraverser.RunTooledTraverser(bvh, rays, ops);
            NodeMap<int> p_r = ops.Inspections;
            // rolls over the tuple (nu, T_U, T_R)
            bvh.RollUpNodes(
                (b, leftSum, rightSum) =>
                {
                    float t_u = leftSum.Item2 + rightSum.Item2 + b.Content.BBox.SurfaceArea;
                    int t_r = leftSum.Item3 + rightSum.Item3 + ops.BranchInspections[b.Content.ID];
                    int nu = leftSum.Item1 + rightSum.Item1;
                    writer.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}",
                        b.Content.Depth,
                        b.Content.BBox.SurfaceArea, ops.BranchInspections[b.Content.ID], // P_U(b), P_R(b)
                        leftSum.Item1, b.Left.BBox().SurfaceArea, leftSum.Item2, p_r[b.Left], leftSum.Item3, // nu(left(b)), P_U(left(b)), T_U(left(b)), P_R(left(b)), T_R(left(b))
                        rightSum.Item1, b.Right.BBox().SurfaceArea, rightSum.Item2, p_r[b.Right], rightSum.Item3); // nu(left(b)), P_U(right(b)), T_U(right(b)), P_R(right(b)), T_R(right(b))
                    return new Tuple<int, float, int>(nu, t_u, t_r);
                },
                l => new Tuple<int, float, int>(1, 0, 0));
        }
    }
}
