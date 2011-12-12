using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;
using System.IO;

namespace AnalysisEngine
{
    class TraversalAnalysis
    {

        public static void RunTraversalComparerSuite(string tracesPath)
        {
            BVH2 bvh = BVH2Parser.ReadFromFile(new FileStream(tracesPath + "crown\\bvh.txt", FileMode.Open, FileAccess.Read));
            RaySet[] allrays = RayFileParser.ReadFromFile(new FileStream(tracesPath + "crown\\casts.txt", FileMode.Open, FileAccess.Read));
            StreamWriter writer = new StreamWriter(tracesPath + "crown\\RayOrder_vs_ODF_per_node.txt");
            RayOrderAdvantageQuantifier(bvh, allrays[1].Filter((r,i) => r.Kind == RayKind.FirstHit_Hit || r.Kind == RayKind.FirstHit_Miss), writer);
            writer.Close();
        }

        public static void RayOrderAdvantageQuantifier(BVH2 bvh, RaySet rays, StreamWriter writer)
        {
            RayOrderInspectionCounter ops = new RayOrderInspectionCounter(bvh.NumBranch);
            OrderedDepthFirstInspectionCounter ops2 = new OrderedDepthFirstInspectionCounter(bvh.NumBranch);
            foreach (RayQuery ray in rays)
            {
                RayOrderTraverser.RunTooledTraverser(bvh, ray, ops);
                OrderedDepthFirstTraverser.RunTooledTraverser(bvh, ray, ops2);
            }
            for (int k = 0; k < bvh.NumBranch; k++)
                writer.WriteLine(ops.BranchInspections[k] + " " + ops2.BranchInspections[k]);
        }
    }
}
