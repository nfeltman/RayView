using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public static class SplitterHelper
    {
        public static BestBinPartition<MemoState, Aggregate> ScorePartitions<StackState, MemoState, Aggregate>(Aggregate[] blockAggregates, SplitEvaluator<StackState, MemoState, Aggregate> se, TriangleAggregator<Aggregate> aggregator, StackState evaluatorState, SplitSeries split)
        {
            int numBlocks = blockAggregates.Length;

            // build forward and backward box and count accumulators
            Box3[] backwardBoxAccumulator = new Box3[numBlocks];
            Aggregate[] backwardAggAccumulator = new Aggregate[numBlocks];
            Aggregate backwardPrevAgg = aggregator.GetIdentity();
            for (int k = numBlocks - 1; k >= 0; k--)
            {
                aggregator.InplaceOp(ref backwardPrevAgg, blockAggregates[k]);
                backwardAggAccumulator[k] = backwardPrevAgg;
            }

            // find smallest cost
            double minCost = double.PositiveInfinity;
            int bestPartition = -10;
            EvalResult<MemoState> bestBuildData = null;
            Aggregate bestForwardAgg = default(Aggregate);
            Aggregate forwardPrevAgg = aggregator.GetIdentity();
            for (int k = 0; k < numBlocks - 1; k++)
            {
                aggregator.InplaceOp(ref forwardPrevAgg, blockAggregates[k]);
                EvalResult<MemoState> cost = se.EvaluateSplit(forwardPrevAgg, backwardAggAccumulator[k + 1], evaluatorState, split.GetFilter<CenterIndexable>(k + 1));
                if (cost.Cost < minCost)
                {
                    bestPartition = k + 1;
                    minCost = cost.Cost;
                    bestBuildData = cost;
                    bestForwardAgg = forwardPrevAgg;
                }
            }

            return new BestBinPartition<MemoState, Aggregate>()
            {
                binPartition = bestPartition,
                heuristicValue = minCost,
                branchBuildData = bestBuildData,
                leftAggregate = bestForwardAgg,
                rightAggregate = backwardAggAccumulator[bestPartition]
            };
        }

        public static void Swap<Tri>(Tri[] tri, int loc1, int loc2)
            where Tri : CenterIndexable
        {
            Tri temp = tri[loc1];
            tri[loc1] = tri[loc2];
            tri[loc2] = temp;
            tri[loc1].Index = loc1;
            tri[loc2].Index = loc2;
        }

        public static void RunSplitSweepTest<Aggregate, Tri, TriB>(Action<int, Aggregate, Aggregate, Func<TriB, bool>> emitter, Tri[] tris, SplitSeries series, int numBins, TriangleAggregator<Aggregate, Tri> aggregator)
            where Tri : CenterIndexable
            where TriB : Tri
        {
            // initialize counts and bounds
            Aggregate[] blockAggregates = new Aggregate[numBins];
            for (int k = 0; k < numBins; k++)
            {
                blockAggregates[k] = aggregator.GetIdentity();
            }

            // calculate counts and bounds by placing every triangle into a bin
            for (int k = 0; k < tris.Length; k++)
            {
                Tri t = tris[k];
                int blockX = Math.Max(0, Math.Min(numBins - 1, series.GetBucket(t)));
                aggregator.InplaceOp(ref blockAggregates[blockX], t);
            }

            // build forward and backward box and count accumulators
            Box3[] backwardBoxAccumulator = new Box3[numBins];
            Aggregate[] backwardAggAccumulator = new Aggregate[numBins];
            Aggregate backwardPrevAgg = aggregator.GetIdentity();
            for (int k = numBins - 1; k >= 0; k--)
            {
                aggregator.InplaceOp(ref backwardPrevAgg, blockAggregates[k]);
                backwardAggAccumulator[k] = backwardPrevAgg;
            }

            Aggregate forwardPrevAgg = aggregator.GetIdentity();
            for (int k = 0; k < numBins - 1; k++)
            {
                aggregator.InplaceOp(ref forwardPrevAgg, blockAggregates[k]);
                emitter(k+1, forwardPrevAgg, backwardAggAccumulator[k + 1], series.GetFilter<TriB>(k+1));
            }
        }
    }

    public class BestBinPartition<MemoState, Aggregate>
    {
        public int binPartition;
        public double heuristicValue;
        public Aggregate leftAggregate, rightAggregate;
        public EvalResult<MemoState> branchBuildData;
    }


}
