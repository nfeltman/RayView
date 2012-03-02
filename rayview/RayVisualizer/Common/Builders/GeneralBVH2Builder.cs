using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RayVisualizer.Common
{
    using BVH2 = Tree<BVH2Branch, BVH2Leaf>;
    using RBVH2 = Tree<RBVH2Branch, RBVH2Leaf>;
    using BackedBVH2 = Tree<BackedBVH2Branch, BackedBVH2Leaf>;
    using BackedRBVH2 = Tree<BackedRBVH2Branch, BackedRBVH2Leaf>;

    public static class GeneralBVH2Builder
    {

        public static BVH2 BuildFullBVH<Tri>(Tri[] tri, Func<int, Box3, int, Box3, float> est, Splitter splitter)
            where Tri : TriangleContainer, CenterIndexable
        {
            return BuildFullBVH(tri, new StatelessSplitEvaluator(est), splitter);
        }

        public static BVH2 BuildFullBVH<Tri, StackState, MemoState, EntranceData>(Tri[] tri, SplitEvaluator<StackState, MemoState, Unit, EntranceData, BoundAndCount> se, Splitter splitter)
            where Tri : TriangleContainer, CenterIndexable
        {
            return BuildFullStructure(tri, se, BVHNodeFactory.ONLY, BoundsCountAggregator<Tri>.ONLY, splitter);
        }

        public static Tree BuildFullStructure<Tri, TriB, BranchT, LeafT, Tree>(Tri[] tri, Func<int, Box3, int, Box3, float> est, NodeFactory<TriB, BranchT, LeafT, Tree, Unit, BoundAndCount> builder, Splitter splitter)
            where Tri : TriangleContainer, CenterIndexable, TriB
        {
            return BuildFullStructure(tri, new StatelessSplitEvaluator(est), builder, BoundsCountAggregator<Tri>.ONLY, splitter);
        }

        public static Tree BuildFullStructure<Tri, TriB, StackState, MemoState, BranchData, EntranceData, BranchT, LeafT, Tree, Aggregate>(Tri[] tri, SplitEvaluator<StackState, MemoState, BranchData, EntranceData, Aggregate> se, NodeFactory<TriB, BranchT, LeafT, Tree, BranchData, Aggregate> builder, TriangleAggregator<Aggregate, Tri> aggregator, Splitter splitter)
            where Tri : CenterIndexable, TriB
        {
            return BuildStructure(tri, se, builder, aggregator, splitter, 1);
        }

        public static Tree BuildStructure<Tri, TriB, StackState, MemoState, BranchData, EntranceData, BranchT, LeafT, Tree, Aggregate>(Tri[] tri, SplitEvaluator<StackState, MemoState, BranchData, EntranceData, Aggregate> splitEvaluator, NodeFactory<TriB, BranchT, LeafT, Tree, BranchData, Aggregate> nodeBuilder, TriangleAggregator<Aggregate, Tri> aggregator, Splitter splitter, int mandatoryLeafSize)
            where Tri : CenterIndexable, TriB
        {
            if (tri.Length == 0)
                throw new ArgumentException("BVH Cannot be empty");
            if (mandatoryLeafSize < 1)
                throw new ArgumentException("Mandatory leaf size cannot be less than 1.");
            BuildImmutables<Tri, TriB, StackState, MemoState, BranchData, EntranceData, BranchT, LeafT, Aggregate> im = new BuildImmutables<Tri, TriB, StackState, MemoState, BranchData, EntranceData, BranchT, LeafT, Aggregate>()
            { 
                tris = tri,
                branchCounter = 0,
                leafCounter = 0,
                mandatoryLeafSize = mandatoryLeafSize,
                eval = splitEvaluator,
                fact = nodeBuilder,
                aggregator = aggregator,
                splitter = splitter
            };
            TreeNode<BranchT, LeafT> root = BuildNodeSegment(0, tri.Length, 0, aggregator.Roll(tri, 0, tri.Length), splitEvaluator.GetDefault(), im);
            return nodeBuilder.BuildTree(root, im.branchCounter);
        }

        private static TreeNode<BranchT, LeafT> BuildNodeSegment<Tri, TriB, StackState, MemoData, BranchData, TransitionData, BranchT, LeafT, Aggregate>(int start, int end, int depth, Aggregate totalAggregate, TransitionData parentState, BuildImmutables<Tri, TriB, StackState, MemoData, BranchData, TransitionData, BranchT, LeafT, Aggregate> im)
            where Tri : CenterIndexable, TriB
        {
            int numTris = end - start;
            if (numTris <= 0)
                throw new ArgumentException("Cannot build a tree from "+numTris+" leaves.");
            // base cases
            if (numTris <= im.mandatoryLeafSize)
            {
                return new Leaf<BranchT, LeafT>(im.fact.BuildLeaf(im.tris, start, end, im.leafCounter++, depth, totalAggregate));
            }
            else if (numTris == 2)
            {
                Aggregate leftAgg = im.aggregator.GetVal(im.tris[start]);
                Aggregate rightAgg = im.aggregator.GetVal(im.tris[start + 1]);
                StackState newState = im.eval.BeginEvaluations(start, end, totalAggregate, parentState);
                EvalResult<MemoData> evaluation = im.eval.EvaluateSplit(leftAgg, rightAgg, newState, t => t.Index == start);
                BuildReport<TransitionData, BranchData> report = im.eval.FinishEvaluations(evaluation, newState);
                Leaf<BranchT, LeafT> left = new Leaf<BranchT, LeafT>(im.fact.BuildLeaf(im.tris, start, start + 1, im.leafCounter++, depth + 1, leftAgg));
                Leaf<BranchT, LeafT> right = new Leaf<BranchT, LeafT>(im.fact.BuildLeaf(im.tris, start + 1, end, im.leafCounter++, depth + 1, rightAgg));
                return new Branch<BranchT, LeafT>(left, right, im.fact.BuildBranch(left, right, report.BranchBuildData, im.branchCounter++, depth, totalAggregate));
            }
            else
            {
                StackState newState;
                int objectPartition;
                EvalResult<MemoData> buildData;
                Aggregate leftObjectBounds;
                Aggregate rightObjectBounds;
                if (IsCompetelyDegenerate(im.tris, start, end))
                {
                    // we're degenerate and we split the node
                    newState = im.eval.BeginEvaluations(start, end, totalAggregate, parentState);
                    objectPartition = (start + end) / 2;
                    leftObjectBounds = im.aggregator.Roll(im.tris, start, objectPartition);
                    rightObjectBounds = im.aggregator.Roll(im.tris, objectPartition, end);
                    Aggregate leftAggregate = im.aggregator.Roll(im.tris, start, objectPartition);
                    Aggregate rightAggregate = im.aggregator.Roll(im.tris, objectPartition, end);
                    buildData = im.eval.EvaluateSplit(leftAggregate, rightAggregate, newState, t => t.Index < objectPartition);
                }
                else
                {
                    newState = im.eval.BeginEvaluations(start, end, totalAggregate, parentState);
                    BestObjectPartition<MemoData, Aggregate> res = im.splitter.PerformBestPartition(im.tris, start, end, (la, ra, fil) => im.eval.EvaluateSplit(la, ra, newState, fil), im.aggregator);
                    if (res.objectPartition >= end || res.objectPartition <= start) throw new Exception("This shouldn't happen.");
                    objectPartition = res.objectPartition;
                    buildData = res.branchBuildData;
                    leftObjectBounds = res.leftAggregate;
                    rightObjectBounds = res.rightAggregate;
                }

                // recursive case
                BuildReport<TransitionData, BranchData> childTransitions = im.eval.FinishEvaluations(buildData, newState);
                int id = im.branchCounter++;
                TreeNode<BranchT, LeafT> left;
                TreeNode<BranchT, LeafT> right;
                if (buildData.BuildLeftFirst)
                {
                    left = BuildNodeSegment(start, objectPartition, depth + 1, leftObjectBounds, childTransitions.LeftTransition, im);
                    right = BuildNodeSegment(objectPartition, end, depth + 1, rightObjectBounds, childTransitions.RightTransition, im);
                }
                else
                {
                    right = BuildNodeSegment(objectPartition, end, depth + 1, rightObjectBounds, childTransitions.RightTransition, im);
                    left = BuildNodeSegment(start, objectPartition, depth + 1, leftObjectBounds, childTransitions.LeftTransition, im);
                }
                return new Branch<BranchT, LeafT>(left, right, im.fact.BuildBranch(left, right, childTransitions.BranchBuildData, id, depth, totalAggregate));
            }
        }


        private static bool IsCompetelyDegenerate<Tri>(Tri[] tri, int start, int end)
            where Tri : Centerable
        {
            CVector3 center = tri[start].Center;
            for (int k = start+1; k < end; k++)
            {
                if (tri[k].Center != center) return false;
            }
            return true;
        }

        private class BuildImmutables<Tri, TriB, StackState, MemoState, BranchData, EntranceData, BranchT, LeafT, Aggregate>
        {
            public Tri[] tris;
            public int mandatoryLeafSize;
            public int branchCounter;
            public int leafCounter;
            public SplitEvaluator<StackState, MemoState, BranchData, EntranceData, Aggregate> eval;
            public NodeFactory<TriB, BranchT, LeafT, BranchData, Aggregate> fact;
            public TriangleAggregator<Aggregate, Tri> aggregator;
            public Splitter splitter;
        }
    }
}
