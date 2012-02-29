using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface Splitter
    {
        BestObjectPartition<MemoState, Aggregate> PerformBestPartition<Tri, StackState, MemoState, Aggregate>(Tri[] tris, int start, int end, StackState evaluatorState, SplitEvaluator<StackState, MemoState, Aggregate> eval, TriangleAggregator<Aggregate, Tri> aggregator)
            where Tri : CenterIndexable;
    }

    // a splitter that doesn't actually perform the final split
    public interface SplitFinder
    {
        BestPartitionFound<Tri, MemoState, Aggregate> FindBestPartition<Tri, StackState, MemoState, Aggregate>(Tri[] tris, int start, int end, StackState evaluatorState, SplitEvaluator<StackState, MemoState, Aggregate> eval, TriangleAggregator<Aggregate, Tri> aggregator)
            where Tri : CenterIndexable;
    }

    public abstract class FullSplitter : Splitter, SplitFinder
    {
        public abstract BestPartitionFound<Tri, MemoState, Aggregate> FindBestPartition<Tri, StackState, MemoState, Aggregate>(Tri[] tris, int start, int end, StackState evaluatorState, SplitEvaluator<StackState, MemoState, Aggregate> eval, TriangleAggregator<Aggregate, Tri> aggregator)
            where Tri : CenterIndexable;

        public BestObjectPartition<MemoState, Aggregate> PerformBestPartition<Tri, StackState, MemoState, Aggregate>(Tri[] tris, int start, int end, StackState evaluatorState, SplitEvaluator<StackState, MemoState, Aggregate> eval, TriangleAggregator<Aggregate, Tri> aggregator)
            where Tri : CenterIndexable
        {
            BestPartitionFound<Tri, MemoState, Aggregate> best = FindBestPartition(tris, start, end, evaluatorState, eval, aggregator);
            int index = best.performSplit();
            return new BestObjectPartition<MemoState, Aggregate>() { leftAggregate = best.leftAggregate, rightAggregate = best.rightAggregate, branchBuildData = best.branchBuildData, objectPartition = index };
        }
    }

    public class BestObjectPartition<MemoState, Aggregate>
    {
        public int objectPartition;
        public Aggregate leftAggregate, rightAggregate;
        public EvalResult<MemoState> branchBuildData;
    }

    public class BestPartitionFound<Tri, MemoState, Aggregate>
    {
        public Func<int> performSplit;
        public double heuristicValue;
        public Aggregate leftAggregate, rightAggregate;
        public EvalResult<MemoState> branchBuildData;
    }
}
