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
            TraversalAmountOverSA(tracesPath);
        }

        public static void SizeRatioFinder(string tracesPath)
        {
            BVH2 bvh = BVH2.ReadFromFile(new FileStream(tracesPath + "crown\\crownBVH.txt", FileMode.Open, FileAccess.Read));
            float[] branchSizeRatios = new float[bvh.NumBranch];
            float[] leafSizeRatios = new float[bvh.NumLeaves];
            Action<float, BVH2Node> assign = (f, n) => n.Accept(
                b => branchSizeRatios[b.ID] = b.BBox.SurfaceArea / f, 
                l => leafSizeRatios[l.ID] = l.BBox.SurfaceArea / f);
            bvh.EnumerateAll(b => { assign(b.BBox.SurfaceArea, b.Left); assign(b.BBox.SurfaceArea, b.Right); }, l => { });
            DumpToFile(branchSizeRatios, tracesPath + "crown\\bRatios.txt");
            DumpToFile(leafSizeRatios, tracesPath + "crown\\lRatios.txt");
        }

        public static void TraversalAmountOverSA(string tracesPath)
        {
            BVH2 bvh = BVH2.ReadFromFile(new FileStream(tracesPath + "crown\\crownBVH.txt", FileMode.Open, FileAccess.Read));
            RaySet[] allrays = RayFileParser.ReadFromFile(new FileStream(tracesPath + "crown\\crownCasts.txt", FileMode.Open, FileAccess.Read));
            RayOrderInspectionCounter ops = new RayOrderInspectionCounter(bvh.NumBranch);
            foreach (RayCast ray in allrays[1].Where(r => r.Kind == RayKind.FirstHit_Hit || r.Kind == RayKind.FirstHit_Miss))
            {
                RayOrderTraverser.RunTraverser(bvh, ray, ops);
            }
            float[] leafQuotients = new float[bvh.NumLeaves];
            float[] branchQuotients = new float[bvh.NumBranch];
            bvh.EnumerateAll(
                b => branchQuotients[b.ID] = ops.BranchInspections[b.ID] / b.BBox.SurfaceArea,
                l => leafQuotients[l.ID] = ops.LeafInspections[l.ID] / l.BBox.SurfaceArea);
            DumpToFile(leafQuotients, tracesPath + "crown\\leafTravPerSA.txt");
            DumpToFile(branchQuotients, tracesPath + "crown\\branchTravPerSA.txt");
        }

        public static void RayOrderAdvantageQuantifier(string tracesPath)
        {
            BVH2 bvh = BVH2.ReadFromFile(new FileStream(tracesPath + "crown\\crownBVH.txt", FileMode.Open, FileAccess.Read));
            RaySet[] allrays = RayFileParser.ReadFromFile(new FileStream(tracesPath + "crown\\crownCasts.txt", FileMode.Open, FileAccess.Read));
            RayOrderInspectionCounter ops = new RayOrderInspectionCounter(bvh.NumBranch);
            foreach (RayCast ray in allrays[1].Where(r => r.Kind == RayKind.FirstHit_Hit || r.Kind == RayKind.FirstHit_Miss))
            {
                RayOrderTraverser.RunTraverser(bvh, ray, ops);
            }

            // TODO: Run ODF traversal method and print off difference.

            DumpToFile(ops.LeafInspections, tracesPath + "crown\\leafInspections.txt");
            DumpToFile(ops.BranchInspections, tracesPath + "crown\\branchInspections.txt");
        }

        public static void TestIntersections(string tracesPath)
        {
            BVH2 bvh = BVH2.ReadFromFile(new FileStream(tracesPath + "crown\\crownBVH.txt", FileMode.Open, FileAccess.Read));
            RaySet[] allrays = RayFileParser.ReadFromFile(new FileStream(tracesPath + "crown\\crownCasts.txt", FileMode.Open, FileAccess.Read));

            RayOrderOpCounter roOps = new RayOrderOpCounter();
            OrderedDepthFirstOperations odfOps = new OrderedDepthFirstOperations();
            int myOwnCount = 0;

            int[] leafIntersectionCount = new int[bvh.NumLeaves];

            int k = -1;
            foreach (RayCast ray in allrays[1].Where(r => r.Kind == RayKind.FirstHit_Hit || r.Kind == RayKind.FirstHit_Miss))
            {
                k++;
                if ((k * 100 / allrays[1].Length) != (k - 1) * 100 / allrays[1].Length)
                    Console.WriteLine(k * 100 / allrays[1].Length);

                if (ray.Kind == RayKind.FirstHit_Hit)
                    myOwnCount++;
                HitRecord closestIntersection = RayOrderTraverser.RunTraverser(bvh, ray, roOps);
                //HitRecord closestIntersection2 = OrderedDepthFirstOpCounter.RunTraverser(bvh, ray, odfOps);
                
                if(closestIntersection!=null)
                    leafIntersectionCount[closestIntersection.leafID]++;

            //    if (closestIntersection != null && firstGen[k].Kind != RayKind.FirstHit_Hit)
            //        Console.WriteLine("AHAHAHAH " + closestIntersection.t_value);
            //    if (closestIntersection2 != null && firstGen[k].Kind != RayKind.FirstHit_Hit)
            //        Console.WriteLine("HEHEHEHE " + closestIntersection.t_value);
            }

            DumpToFile(leafIntersectionCount, tracesPath + "crown\\leafPopularity.txt");
            int[] histogram = FormExactHistogram(leafIntersectionCount, 0, leafIntersectionCount.Max());
            DumpToFile(histogram, tracesPath + "crown\\leafPopularityHisto.txt");
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
}
