﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RayVisualizer.Common
{
    class RunAnalysis
    {
        public static void Main()
        {
            string tracesPath="..\\..\\..\\..\\..\\traces\\";
            RunTraversalComparerSuite(tracesPath);
        }

        public static void RunBVHSuite(string tracesPath)
        {
            BVH2 bvh = BVH2.ReadFromFile(new FileStream(tracesPath + "crown\\bvh.txt", FileMode.Open, FileAccess.Read));
            StreamWriter writer = new StreamWriter(tracesPath + "crown\\Full_TU_vs_PU_nu.txt");
            Full_TU_vs_PU_nu(bvh, writer);
            writer.Close();
        }

        public static void RunTraversalComparerSuite(string tracesPath)
        {
            BVH2 bvh = BVH2.ReadFromFile(new FileStream(tracesPath + "crown\\bvh.txt", FileMode.Open, FileAccess.Read));
            RaySet[] allrays = RayFileParser.ReadFromFile(new FileStream(tracesPath + "crown\\casts.txt", FileMode.Open, FileAccess.Read));
            StreamWriter writer = new StreamWriter(tracesPath + "crown\\RayOrder_vs_ODF_per_node.txt");
            RayOrderAdvantageQuantifier(bvh, allrays[1].Filter(r => r.Kind == RayKind.FirstHit_Hit || r.Kind == RayKind.FirstHit_Miss), writer);
            writer.Close();
        }

        public static void SizeRatioFinder(string tracesPath)
        {
            BVH2 bvh = BVH2.ReadFromFile(new FileStream(tracesPath + "crown\\bvh.txt", FileMode.Open, FileAccess.Read));
            NodeMap<float> sizeRatios = new NodeMap<float>(bvh.NumBranch);
            // this looks incorrect; consider redoing
            bvh.PrefixEnumerate(
                b => b.DoLeftRight(lr => sizeRatios[lr] = lr.BBox.SurfaceArea / b.BBox.SurfaceArea),
                l => { });
            DumpToFile(sizeRatios.Branches, tracesPath + "crown\\bRatios.txt");
            DumpToFile(sizeRatios.Leaves, tracesPath + "crown\\lRatios.txt");
        }

        public static void Full_TU_vs_PU_nu(BVH2 bvh, StreamWriter writer)
        {
            // rolls over the tuple (surface area subsum, leaf count)
            bvh.RollUp(
                (b, leftSum, rightSum) =>
                {
                    float subSAsum = leftSum.Item1 + rightSum.Item1 + b.BBox.SurfaceArea;
                    int subLeafSum = leftSum.Item2 + rightSum.Item2;
                    writer.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", b.Depth,
                        subSAsum, subLeafSum, b.BBox.SurfaceArea, 
                        leftSum.Item1, leftSum.Item2,  b.Left.BBox.SurfaceArea, 
                        rightSum.Item1, rightSum.Item2, b.Right.BBox.SurfaceArea);
                    return new Tuple<float, int>(subSAsum, subLeafSum);
                },
                l => new Tuple<float, int>(0, 1));
        }

        public static void TraversalAmountOverSA(string tracesPath)
        {
            BVH2 bvh = BVH2.ReadFromFile(new FileStream(tracesPath + "crown\\crownBVH.txt", FileMode.Open, FileAccess.Read));
            RaySet[] allrays = RayFileParser.ReadFromFile(new FileStream(tracesPath + "crown\\crownCasts.txt", FileMode.Open, FileAccess.Read));
            RayOrderInspectionCounter ops = new RayOrderInspectionCounter(bvh.NumBranch);
            foreach (RayCast ray in allrays[1].Filter(r => r.Kind == RayKind.FirstHit_Hit || r.Kind == RayKind.FirstHit_Miss))
            {
                RayOrderTraverser.RunTraverser(bvh, ray, ops);
            }
            float[] leafQuotients = new float[bvh.NumLeaves];
            float[] branchQuotients = new float[bvh.NumBranch];
            bvh.PrefixEnumerate(
                b => branchQuotients[b.ID] = ops.BranchInspections[b.ID] / b.BBox.SurfaceArea,
                l => leafQuotients[l.ID] = ops.LeafInspections[l.ID] / l.BBox.SurfaceArea);
            DumpToFile(leafQuotients, tracesPath + "crown\\leafTravPerSA.txt");
            DumpToFile(branchQuotients, tracesPath + "crown\\branchTravPerSA.txt");
        }

        public static void RayOrderAdvantageQuantifier(BVH2 bvh, RaySet rays, StreamWriter writer)
        {
            RayOrderInspectionCounter ops = new RayOrderInspectionCounter(bvh.NumBranch);
            OrderedDepthFirstInspectionCounter ops2 = new OrderedDepthFirstInspectionCounter(bvh.NumBranch);
            foreach (RayCast ray in rays)
            {
                RayOrderTraverser.RunTraverser(bvh, ray, ops);
                OrderedDepthFirstTraverser.RunTraverser(bvh,ray,ops2);
            }
            for (int k = 0; k < bvh.NumBranch; k++)
                writer.WriteLine(ops.BranchInspections[k] + " " + ops2.BranchInspections[k]);
        }

        public static int[] GetClosedSubSums(BVH2 bvh, Func<BVH2Branch, int> forBranch, Func<BVH2Leaf, int> forLeaf)
        {
            int[] subSums = new int[bvh.NumLeaves];
            bvh.RollUp(
                (b,leftSum,rightSum) => 
                {
                    int subsum = leftSum+rightSum+forBranch(b);
                    subSums[b.ID] = subsum;
                    return subsum;
                },
                forLeaf);
            return subSums;
        }

        public static Tuple<T,T>[] GetValsForBranchChildren<T>(BVH2 bvh, NodeMap<T> map)
        {
            Tuple<T,T>[] vals = new Tuple<T,T>[bvh.NumBranch];
            bvh.PrefixEnumerate(
                b =>
                {
                    vals[b.ID] = new Tuple<T, T>(map[b.Left], map[b.Right]);
                },
                l => { }
            );
            return vals;
        }

        public static int[] FormExactHistogram(int[] data, int min, int max)
        {
            Console.WriteLine(max);
            int[] histogram = new int[max - min + 1];
            foreach (int i in data)
                if (i >= min && i <= max)
                    histogram[i-min]++;
            return histogram;
        }

        public static void DumpToFile(int[] data, string s)
        {
            StreamWriter writer = new StreamWriter(s);
            foreach (int i in data)
                writer.WriteLine(i);
            writer.Close();
        }

        public static void DumpToFile(float[] data, string s)
        {
            StreamWriter writer = new StreamWriter(s);
            foreach (float f in data)
                writer.WriteLine(f);
            writer.Close();
        }
    }

    public static class NodeExtensions
    {
        public static void DoLeftRight(this BVH2Branch n, Action<BVH2Node> action)
        {
            action(n.Left);
            action(n.Right);
        }
    }
}
