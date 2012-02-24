using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface Splitter
    {
        BestObjectPartition<MemoState, Aggregate> FindBestPartition<Tri, StackState, MemoState, Aggregate>(Tri[] tris, int start, int end, StackState evaluatorState, SplitEvaluator<StackState, MemoState, Aggregate> eval, TriangleAggregator<Aggregate, Tri> aggregator)
            where Tri : CenterIndexable;
    }

    public class BestObjectPartition<MemoState, Aggregate>
    {
        public int objectPartition;
        public Aggregate leftAggregate, rightAggregate;
        public EvalResult<MemoState> branchBuildData;
    }
}
