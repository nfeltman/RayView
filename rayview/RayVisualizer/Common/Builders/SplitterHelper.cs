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
                EvalResult<MemoState> cost = se.EvaluateSplit(forwardPrevAgg, backwardAggAccumulator[k + 1], evaluatorState, split.GetFilter(k + 1));
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

        public static void Swap(BuildTriangle[] tri, int loc1, int loc2)
        {
            BuildTriangle temp = tri[loc1];
            tri[loc1] = tri[loc2];
            tri[loc2] = temp;
            tri[loc1].index = loc1;
            tri[loc2].index = loc2;
        }

        public static void RunSplitSweepTest<StackState, MemoState, BranchData, TransitionData, Aggregate>(Action<int, double> emitter, BuildTriangle[] tris, SplitSeries series, int numBins, SplitEvaluator<StackState, MemoState, BranchData, TransitionData, Aggregate> eval, TriangleAggregator<Aggregate> aggregator)
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
                BuildTriangle t = tris[k];
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

            StackState evaluatorState = eval.BeginEvaluations(0, tris.Length, backwardPrevAgg, eval.GetDefault());
            Aggregate forwardPrevAgg = aggregator.GetIdentity();
            for (int k = 0; k < numBins - 1; k++)
            {
                aggregator.InplaceOp(ref forwardPrevAgg, blockAggregates[k]);
                EvalResult<MemoState> cost = eval.EvaluateSplit(forwardPrevAgg, backwardAggAccumulator[k + 1], evaluatorState, series.GetFilter(k + 1));
                emitter(k+1, cost.Cost);
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
