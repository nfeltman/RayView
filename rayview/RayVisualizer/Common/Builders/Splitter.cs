using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface Splitter
    {
        BestObjectPartition<MemoState, Aggregate> FindBestPartition<StackState, MemoState, Aggregate>(BuildTriangle[] tris, int start, int end, StackState evaluatorState, BVHSplitEvaluator<StackState, MemoState, Aggregate> eval, TriangleAggregator<Aggregate> aggregator);
    }

    public class BestObjectPartition<MemoState, Aggregate>
    {
        public int objectPartition;
        public Aggregate leftAggregate, rightAggregate;
        public EvalResult<MemoState> branchBuildData;
    }
}
