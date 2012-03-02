using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class RadialSplitter : FullSplitter
    {
        public static readonly RadialSplitter ONLY = new RadialSplitter();

        private RadialSplitter() { }

        public override BestPartitionFound<Tri, MemoState, Aggregate> 
            FindBestPartition<Tri, MemoState, Aggregate>
            (Tri[] tris, int start, int end, Evaluator<MemoState, Aggregate> eval, TriangleAggregator<Aggregate, Tri> aggregator) 
        {
            int len = end - start;
            int numBlocks = Math.Min(20, (int)(len * .05f + 4f));

            // calculate splits
            CVector3 center = BuildTools.FindCentroidBound(tris, start, end).GetCenter();
            ClosedInterval distBound = BuildTools.FindDistanceBound(tris, center, start, end);
            if (distBound.IsEmpty) throw new Exception("Distance Bound should not be empty.");
            if (distBound.Size == 0)
            {
                center = tris[start].Center;
                distBound = BuildTools.FindDistanceBound(tris, center, start, end);
            }
            if (distBound.Size == 0) throw new Exception("Now I give up.");
            RadialSplitSeries split = new RadialSplitSeries(center, distBound.Min, numBlocks / distBound.Size);

            // initialize counts and bounds
            Aggregate[] blockAgg = new Aggregate[numBlocks];
            for (int k = 0; k < numBlocks; k++)
            {
                blockAgg[k] = aggregator.GetIdentity();
            }

            // calculate counts and bounds by placing every triangle into a bin
            for (int k = start; k < end; k++)
            {
                Tri t = tris[k];
                int block = Math.Max(0, Math.Min(numBlocks - 1, split.GetBucket(t)));
                aggregator.InplaceOp(ref blockAgg[block], t);
            }

            // conditionally score the partition groups
            BestBinPartition<MemoState, Aggregate> res = SplitterHelper.ScorePartitions(blockAgg, eval, aggregator, split);
            
            return new BestPartitionFound<Tri, MemoState, Aggregate>() { 
                leftAggregate = res.leftAggregate, 
                rightAggregate = res.rightAggregate, 
                branchBuildData = res.bestEvalResult,
                performSplit = () => { return split.PerformPartition(tris, start, end, res.binPartition); }
            };
        }
    }
}
