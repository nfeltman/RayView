using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class SplitterComposer : FullSplitter
    {
        private SplitFinder _splitter1, _splitter2;

        public SplitterComposer(SplitFinder splitter1, SplitFinder splitter2)
        {
            _splitter1 = splitter1;
            _splitter2 = splitter2;
        }

        public override BestPartitionFound<Tri, MemoState, Aggregate> FindBestPartition<Tri, MemoState, Aggregate>(Tri[] tris, int start, int end, Evaluator<MemoState, Aggregate> eval, TriangleAggregator<Aggregate, Tri> aggregator) 
        {
            BestPartitionFound<Tri, MemoState, Aggregate> res1 = _splitter1.FindBestPartition(tris, start, end, eval, aggregator);
            BestPartitionFound<Tri, MemoState, Aggregate> res2 = _splitter2.FindBestPartition(tris, start, end, eval, aggregator);
            return res1.branchBuildData.Cost < res2.branchBuildData.Cost ? res1 : res2;
        }
    }
}
