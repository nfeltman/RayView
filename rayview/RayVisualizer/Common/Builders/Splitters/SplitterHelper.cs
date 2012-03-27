using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public static class SplitterHelper
    {
        public static BestBinPartition<MemoState, Aggregate> ScorePartitions<MemoState, Aggregate>(Aggregate[] blockAggregates, Evaluator<MemoState, Aggregate> evaluator, TriangleAggregator<Aggregate> aggregator, SplitSeries split)
        {
            int numBlocks = blockAggregates.Length;

            // build forward and backward box and count accumulators
            Aggregate[] backwardAggAccumulator = new Aggregate[numBlocks];
            Aggregate backwardPrevAgg = aggregator.GetIdentity();
            for (int k = numBlocks - 1; k >= 0; k--)
            {
                aggregator.InplaceOp(ref backwardPrevAgg, blockAggregates[k]);
                backwardAggAccumulator[k] = backwardPrevAgg;
            }

            // find smallest cost
            int bestPartition = 1;
            Aggregate forwardPrevAgg = blockAggregates[0];
            EvalResult<MemoState> bestBuildData = evaluator(forwardPrevAgg, backwardAggAccumulator[1], split.GetFilter<CenterIndexable>(1));
            Aggregate bestForwardAgg = forwardPrevAgg;
            for (int k = 1; k < numBlocks - 1; k++)
            {
                if (aggregator.IsIdentity(blockAggregates[k]))
                    continue;
                aggregator.InplaceOp(ref forwardPrevAgg, blockAggregates[k]);
                EvalResult<MemoState> cost = evaluator(forwardPrevAgg, backwardAggAccumulator[k + 1], split.GetFilter<CenterIndexable>(k + 1));
                if (cost.Cost < bestBuildData.Cost)
                {
                    bestPartition = k + 1;
                    bestBuildData = cost;
                    bestForwardAgg = forwardPrevAgg;
                }
            }

            return new BestBinPartition<MemoState, Aggregate>()
            {
                binPartition = bestPartition,
                bestEvalResult = bestBuildData,
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
            tri[loc1].BuildIndex = loc1;
            tri[loc2].BuildIndex = loc2;
        }

        public static void RunSplitSweepTest<Aggregate, Tri>(Action<int, Aggregate, Aggregate, Func<CenterIndexable, bool>> emitter, Tri[] tris, SplitSeries series, int numBins, TriangleAggregator<Aggregate, Tri> aggregator)
            where Tri : CenterIndexable
            //where TriB : Tri
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
                emitter(k+1, forwardPrevAgg, backwardAggAccumulator[k + 1], series.GetFilter<CenterIndexable>(k+1));
            }
        }
    }

    public class BestBinPartition<MemoState, Aggregate>
    {
        public double heuristicValue { get { return bestEvalResult.Cost; } }
        public int binPartition;
        public Aggregate leftAggregate, rightAggregate;
        public EvalResult<MemoState> bestEvalResult;
    }


}
