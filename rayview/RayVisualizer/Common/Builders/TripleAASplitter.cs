using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class TripleAASplitter : Splitter
    {
        public static readonly TripleAASplitter ONLY = new TripleAASplitter();

        private TripleAASplitter() { }

        public BestObjectPartition<MemoState, Aggregate> FindBestPartition<StackState, MemoState, Aggregate>(BuildTriangle[] tris, int start, int end, StackState evaluatorState, SplitEvaluator<StackState, MemoState, Aggregate> eval, TriangleAggregator<Aggregate> aggregator)
        {
            int len = end - start;

            // calculate splits
            Box3 centroidBounds = BuildTools.FindCentroidBound(tris, start, end);
            int numBlocks = Math.Min(32, (int)(len * .05f + 4f));
            XAASplitSeries seriesX = new XAASplitSeries(centroidBounds.XRange.Min, numBlocks / centroidBounds.XRange.Size);
            YAASplitSeries seriesY = new YAASplitSeries(centroidBounds.YRange.Min, numBlocks / centroidBounds.YRange.Size);
            ZAASplitSeries seriesZ = new ZAASplitSeries(centroidBounds.ZRange.Min, numBlocks / centroidBounds.ZRange.Size);

            // initialize counts and bounds
            Aggregate[] blockAggX = new Aggregate[numBlocks];
            Aggregate[] blockAggY = new Aggregate[numBlocks];
            Aggregate[] blockAggZ = new Aggregate[numBlocks];
            for (int k = 0; k < numBlocks; k++)
            {
                blockAggX[k] = aggregator.GetIdentity();
                blockAggY[k] = aggregator.GetIdentity();
                blockAggZ[k] = aggregator.GetIdentity();
            }

            // degeneracy checks
            bool xDegen = true;
            bool yDegen = true;
            bool zDegen = true;

            // calculate counts and bounds by placing every triangle into a bin
            for (int k = start; k < end; k++)
            {
                BuildTriangle t = tris[k];
                int blockX = Math.Max(0, Math.Min(numBlocks - 1, seriesX.GetBucket(t)));
                int blockY = Math.Max(0, Math.Min(numBlocks - 1, seriesY.GetBucket(t)));
                int blockZ = Math.Max(0, Math.Min(numBlocks - 1, seriesZ.GetBucket(t)));
                aggregator.InplaceOp3(ref blockAggX[blockX], ref blockAggY[blockY], ref blockAggZ[blockZ], t);
                if (xDegen && blockX != 0) xDegen = false;
                if (yDegen && blockY != 0) yDegen = false;
                if (zDegen && blockZ != 0) zDegen = false;
            }

            // conditionally score the partition groups
            BestBinPartition<MemoState, Aggregate> resX = xDegen ? null : SplitterHelper.ScorePartitions(blockAggX, eval, aggregator, evaluatorState, seriesX);
            BestBinPartition<MemoState, Aggregate> resY = yDegen ? null : SplitterHelper.ScorePartitions(blockAggY, eval, aggregator, evaluatorState, seriesY);
            BestBinPartition<MemoState, Aggregate> resZ = zDegen ? null : SplitterHelper.ScorePartitions(blockAggZ, eval, aggregator, evaluatorState, seriesZ);

            AASplitSeries split;
            BestBinPartition<MemoState, Aggregate> res;
            // the partition function has a funky signature because it needs to have the EXACT SAME numerical precision properties as the binning check above
            if (!xDegen && (yDegen || resX.heuristicValue <= resY.heuristicValue) && (zDegen || resX.heuristicValue <= resZ.heuristicValue))
            {
                res = resX;
                split = seriesX;
            }
            else if (!yDegen && (zDegen || resY.heuristicValue <= resZ.heuristicValue))
            {
                res = resY;
                split = seriesY;
            }
            else if (!zDegen)
            {
                res = resZ;
                split = seriesZ;
            }
            else
            {
                // triple degenerate
                if (!(xDegen && yDegen && zDegen)) throw new Exception("Oh god, my logic is wrong");
                throw new Exception("Checked for degeneracy above.  Why am I here?!");
            }

            int index = split.PerformPartition(tris, start, end, res.binPartition);

            if (index >= end || index <= start)
            {
                throw new Exception("This shouldn't happen.");
            }

            return new BestObjectPartition<MemoState, Aggregate>() { leftAggregate = res.leftAggregate, rightAggregate = res.rightAggregate, branchBuildData = res.branchBuildData, objectPartition = index };
        }
    }
}
