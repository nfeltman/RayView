using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RayVisualizer.Common
{
    public static class GeneralBVH2Builder
    {
        public static BVH2 BuildBVHTest(BuildTriangle[] tri)
        {
            //return BuildBVH(tri, (left_nu, left_box, right_nu, right_box) => left_nu * left_box.SurfaceArea + right_nu * right_box.SurfaceArea);
            return BuildStructure(tri, (left_nu, left_box, right_nu, right_box) => ((left_nu + 3) >> 2) * left_box.SurfaceArea + ((right_nu + 3) >> 2) * right_box.SurfaceArea, BVHNodeFactory.ONLY, 4, false);
        }

        public static BVH2 BuildFullBVH(BuildTriangle[] tri, Func<int, Box3, int, Box3, float> est)
        {
            return BuildFullBVH(tri, new StatelessSplitEvaluator(est));
        }

        public static BVH2 BuildFullBVH<StackState, MemoState, EntranceData>(BuildTriangle[] tri, BVHSplitEvaluator<StackState, MemoState, Unit, EntranceData, BoundAndCount> se)
        {
            return BuildFullStructure(tri, se, BVHNodeFactory.ONLY, BoundsCountAggregator.ONLY);
        }

        public static RBVH2 BuildFullRBVH<StackState, MemoState, EntranceData>(BuildTriangle[] tri, BVHSplitEvaluator<StackState, MemoState, float, EntranceData, BoundAndCount> se)
        {
            return BuildFullStructure(tri, se, RBVHNodeFactory.ONLY, BoundsCountAggregator.ONLY);
        }

        public static Tree BuildFullStructure<BranchT, LeafT, Tree>(BuildTriangle[] tri, Func<int, Box3, int, Box3, float> est, NodeFactory<BranchT, LeafT, Tree, Unit, BoundAndCount> builder)
        {
            return BuildFullStructure(tri, new StatelessSplitEvaluator(est), builder, BoundsCountAggregator.ONLY);
        }

        public static Tree BuildFullStructure<StackState, MemoState, BranchData, EntranceData, BranchT, LeafT, Tree, Aggregate>(BuildTriangle[] tri, BVHSplitEvaluator<StackState, MemoState, BranchData, EntranceData, Aggregate> se, NodeFactory<BranchT, LeafT, Tree, BranchData, Aggregate> builder, TriangleAggregator<Aggregate> aggregator)
        {
            return BuildStructure(tri, se, builder, aggregator, 1, true);
        }

        public static Tree BuildStructure<BranchT, LeafT, Tree>(BuildTriangle[] tri, Func<int, Box3, int, Box3, float> est, NodeFactory<BranchT, LeafT, Tree, Unit, BoundAndCount> builder, int mandatoryLeafSize, bool splitDegenerateNodes)
        {
            return BuildStructure(tri, new StatelessSplitEvaluator(est), builder, BoundsCountAggregator.ONLY, mandatoryLeafSize, splitDegenerateNodes);
        }

        public static Tree BuildStructure<StackState, MemoState, BranchData, EntranceData, BranchT, LeafT, Tree, Aggregate>(BuildTriangle[] tri, BVHSplitEvaluator<StackState, MemoState, BranchData, EntranceData, Aggregate> se, NodeFactory<BranchT, LeafT, Tree, BranchData, Aggregate> builder, TriangleAggregator<Aggregate> aggregator, int mandatoryLeafSize, bool splitDegenerateNodes)
        {
            if (tri.Length == 0)
                throw new ArgumentException("BVH Cannot be empty");
            BuildImmutables<StackState, MemoState, BranchData, EntranceData, BranchT, LeafT, Aggregate> im = new BuildImmutables<StackState, MemoState, BranchData, EntranceData, BranchT, LeafT, Aggregate>()
            { 
                eval = se, 
                tris = tri,
                branchCounter = 0,
                leafCounter = 0,
                mandatoryLeafSize = mandatoryLeafSize,
                splitDegenerateNodes = splitDegenerateNodes,
                fact = builder,
                aggregator = aggregator
            };
            TreeNode<BranchT, LeafT> root = BuildNodeSegment(0, tri.Length, 0, aggregator.Roll(tri, 0, tri.Length), se.GetDefault(), im);
            return builder.BuildTree(root, im.branchCounter);
        }

        private static TreeNode<BranchT, LeafT> BuildNodeSegment<StackState, MemoData, BranchData, EntranceData, BranchT, LeafT, Aggregate>(int start, int end, int depth, Aggregate totalAggregate, EntranceData parentState, BuildImmutables<StackState, MemoData, BranchData, EntranceData, BranchT, LeafT, Aggregate> im)
        {
        //    if ((im.branchCounter & 1023) == 0)
        //        Console.WriteLine("I'm thiiiis far:" + im.branchCounter + " " + depth);

            int numTris = end - start;
            if (numTris <= 0)
                throw new ArgumentException("Cannot build a tree from "+numTris+" leaves.");
            // base cases
            if (numTris <= im.mandatoryLeafSize)
            {
                return new Leaf<BranchT, LeafT>(im.fact.BuildLeaf(im.tris, start, end, im.leafCounter++, depth, totalAggregate));
            }/*
            else if (numTris==2)
            {
                int id = im.branchCounter++;
                BuildTriangle lTri = im.tris[start];
                BuildTriangle rTri = im.tris[start+1];
                BoundBuilder build = new BoundBuilder(true);
                build.AddTriangle(lTri.t);
                RBVH2Node left = new RBVH2Leaf() { Primitives = new Triangle[] { lTri.t }, ID = im.leafCounter++, Depth = depth + 1, BBox = build.GetBox() };
                build.Reset();
                build.AddTriangle(rTri.t);
                RBVH2Node right = new RBVH2Leaf() { Primitives = new Triangle[] { rTri.t }, ID = im.leafCounter++, Depth = depth + 1, BBox = build.GetBox() };
                return new RBVH2Branch() { Left = left, Right = right, BBox = objectBound, ID = id, Depth = depth, PLeft = 0.5f };
            }*/

            StackState newState;
            int objectPartition;
            EvalResult<MemoData> buildData;
            Aggregate leftObjectBounds;
            Aggregate rightObjectBounds;
            if (IsCompetelyDegenerate(im.tris, start, end))
            {
                if (im.splitDegenerateNodes)
                {
                    // we're degenerate and we split the node
                    newState = im.eval.BeginEvaluations(start, end, totalAggregate, parentState);
                    objectPartition = (start + end) / 2;
                    leftObjectBounds = im.aggregator.Roll(im.tris, start, objectPartition);
                    rightObjectBounds = im.aggregator.Roll(im.tris, objectPartition, end);
                    Aggregate leftAggregate = im.aggregator.Roll(im.tris, start, objectPartition);
                    Aggregate rightAggregate = im.aggregator.Roll(im.tris, objectPartition, end);
                    buildData = im.eval.EvaluateSplit(leftAggregate, rightAggregate, newState, t => t.index < objectPartition);
                }
                else
                {
                    // we're degenerate and we don't split the node (so we just make a leaf)
                    return new Leaf<BranchT, LeafT>(im.fact.BuildLeaf(im.tris, start, end, im.leafCounter++, depth, totalAggregate));
                }
            }
            else
            {
                newState = im.eval.BeginEvaluations(start, end, totalAggregate, parentState);
                BestObjectPartition<MemoData, Aggregate> res = FindBestPartition(im.tris, start, end, newState, im.eval, im.aggregator);
                objectPartition = res.objectPartition;
                buildData = res.branchBuildData;
                leftObjectBounds = res.leftAggregate;
                rightObjectBounds = res.rightAggregate;
            }

            // recursive case
            BuildReport<EntranceData, BranchData> childTransitions = im.eval.FinishEvaluations(buildData, newState);
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
        private static BestObjectPartition<MemoState, Aggregate> FindBestPartition<StackState, MemoState, Aggregate>(BuildTriangle[] tris, int start, int end, StackState evaluatorState, BVHSplitEvaluator<StackState, MemoState, Aggregate> eval, TriangleAggregator<Aggregate> aggregator)
        {
            int len = end - start;

            // calculate splits
            Box3 centroidBounds = BuildTools.FindCentroidBound(tris, start, end);
            int numBlocks = Math.Min(32, (int)(len * .05f + 4f));
            AASplitSeries seriesX = new AASplitSeries(SplitDimension.SplitX, centroidBounds.XRange.Min, numBlocks / centroidBounds.XRange.Size);
            AASplitSeries seriesY = new AASplitSeries(SplitDimension.SplitY, centroidBounds.YRange.Min, numBlocks / centroidBounds.YRange.Size);
            AASplitSeries seriesZ = new AASplitSeries(SplitDimension.SplitZ, centroidBounds.ZRange.Min, numBlocks / centroidBounds.ZRange.Size);

            // initialize counts and bounds
            Aggregate[] blockCountsX = new Aggregate[numBlocks];
            Aggregate[] blockCountsY = new Aggregate[numBlocks];
            Aggregate[] blockCountsZ = new Aggregate[numBlocks];
            for (int k = 0; k < numBlocks; k++)
            {
                blockCountsX[k] = aggregator.GetIdentity();
                blockCountsY[k] = aggregator.GetIdentity();
                blockCountsZ[k] = aggregator.GetIdentity();
            }

            // degeneracy checks
            bool xDegen = true;
            bool yDegen = true;
            bool zDegen = true;

            // calculate counts and bounds by placing every triangle into a bin
            for (int k = start; k < end; k++)
            {
                BuildTriangle t = tris[k];
                int blockX = Math.Max(0, Math.Min(numBlocks - 1, seriesX.GetPartition(t.center.x)));
                int blockY = Math.Max(0, Math.Min(numBlocks - 1, seriesY.GetPartition(t.center.y)));
                int blockZ = Math.Max(0, Math.Min(numBlocks - 1, seriesZ.GetPartition(t.center.z)));
                aggregator.InplaceOp(ref blockCountsX[blockX], t);
                aggregator.InplaceOp(ref blockCountsY[blockY], t);
                aggregator.InplaceOp(ref blockCountsZ[blockZ], t);
                if (xDegen && blockX != 0) xDegen = false;
                if (yDegen && blockY != 0) yDegen = false;
                if (zDegen && blockZ != 0) zDegen = false;
            }

            // conditionally score the partition groups
            BestBinPartition<MemoState, Aggregate> resX = xDegen ? null : ScorePartitions(blockCountsX, eval, aggregator, evaluatorState, seriesX);
            BestBinPartition<MemoState, Aggregate> resY = yDegen ? null : ScorePartitions(blockCountsY, eval, aggregator, evaluatorState, seriesY);
            BestBinPartition<MemoState, Aggregate> resZ = zDegen ? null : ScorePartitions(blockCountsZ, eval, aggregator, evaluatorState, seriesZ);

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
            else if(!zDegen)
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

            return new BestObjectPartition<MemoState, Aggregate>() { leftAggregate = res.leftAggregate, rightAggregate = res.rightAggregate, branchBuildData = res.branchBuildData, objectPartition = index };
        }

        private static BestBinPartition<BranchData, Aggregate> ScorePartitions<StackState, BranchData, Aggregate>(Aggregate[] blockAggregates, BVHSplitEvaluator<StackState, BranchData, Aggregate> se, TriangleAggregator<Aggregate> aggregator, StackState evaluatorState, AASplitSeries split)
        {
            int numBlocks = blockAggregates.Length;

            // build forward and backward box and count accumulators
            Box3[] backwardBoxAccumulator = new Box3[numBlocks];
            Aggregate[] backwardAggAccumulator = new Aggregate[numBlocks];
            Aggregate backwardPrevCount = aggregator.GetIdentity();
            for (int k = numBlocks - 1; k >= 0; k--)
            {
                backwardAggAccumulator[k] = backwardPrevCount = aggregator.Op(backwardPrevCount, blockAggregates[k]);
            }

            // find smallest cost
            double minCost = double.PositiveInfinity;
            int bestPartition = -10;
            EvalResult<BranchData> bestBuildData = null;
            Aggregate bestForwardAgg = default(Aggregate);
            Aggregate forwardPrevAgg = aggregator.GetIdentity();
            for (int k = 0; k < numBlocks-1; k++)
            {
                aggregator.InplaceOp(ref forwardPrevAgg, blockAggregates[k]);
                EvalResult<BranchData> cost = se.EvaluateSplit(forwardPrevAgg, backwardAggAccumulator[k + 1], evaluatorState, split.GetFilter(k + 1));
                if (cost.Cost < minCost)
                {
                    bestPartition = k+1;
                    minCost = cost.Cost;
                    bestBuildData = cost;
                    bestForwardAgg = forwardPrevAgg;
                }
            }

            if (bestPartition == 0)
            {
                throw new Exception("Best partition shouldn't be the first.");
            }

            return new BestBinPartition<BranchData, Aggregate>()
            { 
                binPartition = bestPartition,
                heuristicValue = minCost,
                branchBuildData = bestBuildData,
                leftAggregate = bestForwardAgg,
                rightAggregate = backwardAggAccumulator[bestPartition]
            };
        }

        private static bool IsCompetelyDegenerate(BuildTriangle[] tri, int start, int end)
        {
            CVector3 center = tri[start].center;
            for (int k = start+1; k < end; k++)
            {
                if (tri[k].center != center) return false;
            }
            return true;
        }

        private class BestBinPartition<BranchData, Aggregate>
        {
            public int binPartition;
            public double heuristicValue;
            public Aggregate leftAggregate, rightAggregate;
            public EvalResult<BranchData> branchBuildData;
        }

        private class BestObjectPartition<BranchData, Aggregate>
        {
            public int objectPartition;
            public Aggregate leftAggregate, rightAggregate;
            public EvalResult<BranchData> branchBuildData;
        }

        private class BuildImmutables<StackState, MemoState, BranchData, EntranceData, BranchT, LeafT, Aggregate>
        {
            public BuildTriangle[] tris;
            public int mandatoryLeafSize;
            public bool splitDegenerateNodes;
            public BVHSplitEvaluator<StackState, MemoState, BranchData, EntranceData, Aggregate> eval;
            public NodeFactory<BranchT, LeafT, BranchData, Aggregate> fact;
            public int branchCounter;
            public int leafCounter;
            public TriangleAggregator<Aggregate> aggregator;
        }
    }
}
