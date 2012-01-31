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

        public static BVH2 BuildFullBVH<StackState, EntranceData, ExitData>(BuildTriangle[] tri, BVHSplitEvaluator<StackState, Unit, EntranceData, ExitData> se)
        {
            return BuildStructure(tri, se, BVHNodeFactory.ONLY);
        }

        public static RBVH2 BuildFullRBVH<StackState, EntranceData, ExitData>(BuildTriangle[] tri, BVHSplitEvaluator<StackState, float, EntranceData, ExitData> se)
        {
            return BuildStructure(tri, se, RBVHNodeFactory.ONLY);
        }

        public static Tree BuildStructure<BranchT, LeafT, Tree>(BuildTriangle[] tri, Func<int, Box3, int, Box3, float> est, NodeFactory<BranchT, LeafT, Tree, Unit> builder)
        {
            return BuildStructure(tri, new StatelessSplitEvaluator(est), builder);
        }

        public static Tree BuildStructure<StackState, BranchData, EntranceData, ExitData, BranchT, LeafT, Tree>(BuildTriangle[] tri, BVHSplitEvaluator<StackState, BranchData, EntranceData, ExitData> se, NodeFactory<BranchT, LeafT, Tree, BranchData> builder)
        {
            return BuildStructure(tri, se, builder, 1, true);
        }

        public static Tree BuildStructure<BranchT, LeafT, Tree>(BuildTriangle[] tri, Func<int, Box3, int, Box3, float> est, NodeFactory<BranchT, LeafT, Tree, Unit> builder, int mandatoryLeafSize, bool splitDegenerateNodes)
        {
            return BuildStructure(tri, new StatelessSplitEvaluator(est), builder, mandatoryLeafSize, splitDegenerateNodes);
        }

        public static Tree BuildStructure<StackState, BranchData, EntranceData, ExitData, BranchT, LeafT, Tree>(BuildTriangle[] tri, BVHSplitEvaluator<StackState, BranchData, EntranceData, ExitData> se, NodeFactory<BranchT, LeafT, Tree, BranchData> builder, int mandatoryLeafSize, bool splitDegenerateNodes)
        {
            if (tri.Length == 0)
                throw new ArgumentException("BVH Cannot be empty");
            BoundBuilder b = new BoundBuilder(true);
            for (int k = 0; k < tri.Length; k++)
                b.AddTriangle(tri[k].t);
            BuildImmutables<StackState, BranchData, EntranceData, ExitData, BranchT, LeafT> im = new BuildImmutables<StackState, BranchData, EntranceData, ExitData, BranchT, LeafT>()
            { 
                eval = se, 
                tris = tri,
                branchCounter = 0,
                leafCounter = 0,
                mandatoryLeafSize = mandatoryLeafSize,
                splitDegenerateNodes = splitDegenerateNodes,
                fact = builder
            };
            int len = tri.Length-1;
            Box3 topBox = b.GetBox();
            TreeNode<BranchT, LeafT> root = BuildNodeSegment(0, tri.Length, 0, topBox, se.GetDefault(), im).Item1;
            return builder.BuildTree(root, im.branchCounter);
        }

        private static Tuple<TreeNode<BranchT, LeafT>, ExitData> BuildNodeSegment<StackState, BranchData, EntranceData, ExitData, BranchT, LeafT>(int start, int end, int depth, Box3 objectBound, EntranceData parentState, BuildImmutables<StackState, BranchData, EntranceData, ExitData, BranchT, LeafT> im)
        {
        //    if ((im.branchCounter & 1023) == 0)
        //        Console.WriteLine("I'm thiiiis far:" + im.branchCounter + " " + depth);

            int numTris = end - start;
            if (numTris <= 0)
                throw new ArgumentException("Cannot build a tree from "+numTris+" leaves.");
            // base cases
            if (numTris <= im.mandatoryLeafSize)
            {
                return new Tuple<TreeNode<BranchT, LeafT>, ExitData>(new Leaf<BranchT, LeafT>(im.fact.BuildLeaf(im.tris, start, end, im.leafCounter++, depth, objectBound)), im.eval.GetLeafExit());
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

            StackState newState = im.eval.BeginEvaluations(start, end, objectBound, parentState);

            // recursive case
            BestObjectPartition<BranchData> part = FindBestPartition(im.tris, start, end, newState, im.eval);

            if (part.isDegenerate && !im.splitDegenerateNodes)
            {
                return new Tuple<TreeNode<BranchT, LeafT>, ExitData>( new Leaf<BranchT, LeafT>(im.fact.BuildLeaf(im.tris, start, end, im.leafCounter++, depth, objectBound)), im.eval.GetLeafExit());
            }
            else
            {
                int id = im.branchCounter++;
                EntranceData firstTransition = im.eval.PrepareFirstChild(part.branchBuildData, newState);
                Tuple<TreeNode<BranchT, LeafT>, ExitData> left = BuildNodeSegment(start, part.objectPartition, depth + 1, part.leftObjectBounds, firstTransition, im);
                EntranceData secondTransition = im.eval.PrepareSecondChild(left.Item2, part.branchBuildData, newState);
                Tuple<TreeNode<BranchT, LeafT>, ExitData> right = BuildNodeSegment(part.objectPartition, end, depth + 1, part.rightObjectBounds, secondTransition, im);
                ExitData exit = im.eval.EndBothChildren(left.Item2, right.Item2);
                return new Tuple<TreeNode<BranchT, LeafT>, ExitData>(new Branch<BranchT, LeafT>(left.Item1, right.Item1, im.fact.BuildBranch(left.Item1, right.Item1, part.branchBuildData, id, depth, objectBound)), exit);
            }
        }
        private static BestObjectPartition<BranchData> FindBestPartition<StackState, BranchData>(BuildTriangle[] tris, int start, int end, StackState evaluatorState, BVHSplitEvaluator<StackState, BranchData> eval)
        {
            int len = end - start;

            // calculate splits
            Box3 centroidBounds = BuildTools.FindCentroidBound(tris, start, end);
            int numBlocks = Math.Min(32, (int)(len * .05f + 4f));
            AASplitSeries seriesX = new AASplitSeries(SplitDimension.SplitX, centroidBounds.XRange.Min, numBlocks / centroidBounds.XRange.Size);
            AASplitSeries seriesY = new AASplitSeries(SplitDimension.SplitY, centroidBounds.YRange.Min, numBlocks / centroidBounds.YRange.Size);
            AASplitSeries seriesZ = new AASplitSeries(SplitDimension.SplitZ, centroidBounds.ZRange.Min, numBlocks / centroidBounds.ZRange.Size);

            // initialize counts and bounds
            int[] blockCountsX = new int[numBlocks];
            int[] blockCountsY = new int[numBlocks];
            int[] blockCountsZ = new int[numBlocks];
            BoundBuilder[] blockBoundsX = new BoundBuilder[numBlocks];
            BoundBuilder[] blockBoundsY = new BoundBuilder[numBlocks];
            BoundBuilder[] blockBoundsZ = new BoundBuilder[numBlocks];
            for (int k = 0; k < numBlocks; k++)
            {
                blockBoundsX[k] = new BoundBuilder(true);
                blockBoundsY[k] = new BoundBuilder(true);
                blockBoundsZ[k] = new BoundBuilder(true);
            }

            // calculate counts and bounds by placing every triangle into a bin
            for (int k = start; k < end; k++)
            {
                BuildTriangle t = tris[k];
                int blockX = Math.Max(0, Math.Min(numBlocks - 1, seriesX.GetPartition(t.center.x)));
                int blockY = Math.Max(0, Math.Min(numBlocks - 1, seriesY.GetPartition(t.center.y)));
                int blockZ = Math.Max(0, Math.Min(numBlocks - 1, seriesZ.GetPartition(t.center.z)));
                blockCountsX[blockX]++;
                blockCountsY[blockY]++;
                blockCountsZ[blockZ]++;
                blockBoundsX[blockX].AddTriangle(t.t);
                blockBoundsY[blockY].AddTriangle(t.t);
                blockBoundsZ[blockZ].AddTriangle(t.t);
            }

            // check for degeneracy in each dimension
            bool xDegen = blockCountsX[0] == len;
            bool yDegen = blockCountsY[0] == len;
            bool zDegen = blockCountsZ[0] == len;

            // conditionally score the partition groups
            BestBinPartition<BranchData> resX = xDegen ? null : ScorePartitions(blockCountsX, blockBoundsX, eval, evaluatorState, seriesX);
            BestBinPartition<BranchData> resY = yDegen ? null : ScorePartitions(blockCountsY, blockBoundsY, eval, evaluatorState, seriesY);
            BestBinPartition<BranchData> resZ = zDegen ? null : ScorePartitions(blockCountsZ, blockBoundsZ, eval, evaluatorState, seriesZ);

            AASplitSeries split;
            BestBinPartition<BranchData> res;
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
                Box3 box = blockBoundsX[0].GetBox();
                return new BestObjectPartition<BranchData>() { leftObjectBounds = box, rightObjectBounds = box, branchBuildData = default(BranchData), objectPartition = (start + end) / 2, isDegenerate = true };
            }

            int index = split.PerformPartition(tris, start, end, res.binPartition);

            return new BestObjectPartition<BranchData>() { leftObjectBounds = res.leftObjectBounds, rightObjectBounds = res.rightObjectBounds, branchBuildData = res.branchBuildData, objectPartition = index, isDegenerate = false };
        }

        private static BestBinPartition<BranchData> ScorePartitions<StackState, BranchData>(int[] blockCounts, BoundBuilder[] blockOBounds, BVHSplitEvaluator<StackState, BranchData> se, StackState evaluatorState, AASplitSeries split)
        {
            int numBlocks = blockCounts.Length;

            // build forward and backward box and count accumulators
            Box3[] backwardBoxAccumulator = new Box3[numBlocks];
            int[] backwardCountAccumulator = new int[numBlocks];
            int backwardPrevCount = 0;
            BoundBuilder builder = new BoundBuilder(true);
            for (int k = numBlocks - 1; k >= 0; k--)
            {
                backwardCountAccumulator[k] = backwardPrevCount = backwardPrevCount + blockCounts[k];
                builder.AddBox(blockOBounds[k]);
                backwardBoxAccumulator[k] = builder.GetBox();
            }

            // find smallest cost
            double minCost = double.PositiveInfinity;
            int bestPartition = -10;
            BranchData bestBuildData = default(BranchData);
            Box3 bestForwardBox = new Box3();
            int forwardPrevCount = 0;
            builder.Reset();
            for (int k = 0; k < numBlocks-1; k++)
            {
                forwardPrevCount = forwardPrevCount + blockCounts[k];
                builder.AddBox(blockOBounds[k]);
                Box3 forwardBox = builder.GetBox();
                EvalResult<BranchData> cost = se.EvaluateSplit(forwardPrevCount, forwardBox, backwardCountAccumulator[k + 1], backwardBoxAccumulator[k + 1], evaluatorState, split, k + 1);
                if (cost.Cost < minCost)
                {
                    bestPartition = k+1;
                    minCost = cost.Cost;
                    bestBuildData = cost.Data;
                    bestForwardBox = forwardBox;
                }
            }

            if (bestPartition == 0)
            {
                throw new Exception("Best partition shouldn't be the first.");
            }

            return new BestBinPartition<BranchData>() { 
                binPartition = bestPartition,
                heuristicValue = minCost,
                branchBuildData = bestBuildData,
                leftObjectBounds = bestForwardBox, 
                rightObjectBounds = backwardBoxAccumulator[bestPartition] };
        }

        private class BestBinPartition<BranchData>
        {
            public int binPartition;
            public double heuristicValue;
            public Box3 leftObjectBounds, rightObjectBounds;
            public BranchData branchBuildData;
        }

        private class BestObjectPartition<BranchData>
        {
            public bool isDegenerate;
            public int objectPartition;
            public Box3 leftObjectBounds, rightObjectBounds;
            public BranchData branchBuildData;
        }

        private class BuildImmutables<StackState, BranchData, EntranceData, ExitData, BranchT, LeafT>
        {
            public BuildTriangle[] tris;
            public int mandatoryLeafSize;
            public bool splitDegenerateNodes;
            public BVHSplitEvaluator<StackState, BranchData, EntranceData, ExitData> eval;
            public NodeFactory<BranchT, LeafT, BranchData> fact;
            public int branchCounter;
            public int leafCounter;
        }
    }
}
