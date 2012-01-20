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

        public static BVH2 BuildFullBVH<StackState>(BuildTriangle[] tri, BVHSplitEvaluator<StackState, Unit> se)
        {
            return BuildStructure(tri, se, BVHNodeFactory.ONLY);
        }

        public static Tree BuildStructure<Node, Tree>(BuildTriangle[] tri, Func<int, Box3, int, Box3, float> est, NodeFactory<Node, Tree, Unit> builder)
        {
            return BuildStructure(tri, new StatelessSplitEvaluator(est), builder);
        }

        public static Tree BuildStructure<T, U, Node, Tree>(BuildTriangle[] tri, BVHSplitEvaluator<T, U> se, NodeFactory<Node, Tree, U> builder)
        {
            return BuildStructure(tri, se, builder, 1, true);
        }

        public static Tree BuildStructure<Node, Tree>(BuildTriangle[] tri, Func<int, Box3, int, Box3, float> est, NodeFactory<Node, Tree, Unit> builder, int mandatoryLeafSize, bool splitDegenerateNodes)
        {
            return BuildStructure(tri, new StatelessSplitEvaluator(est), builder, mandatoryLeafSize, splitDegenerateNodes);
        }

        public static Tree BuildStructure<T, U, Node, Tree>(BuildTriangle[] tri, BVHSplitEvaluator<T, U> se, NodeFactory<Node, Tree, U> builder, int mandatoryLeafSize, bool splitDegenerateNodes)
        {
            if (tri.Length == 0)
                throw new ArgumentException("BVH Cannot be empty");
            BoundBuilder b = new BoundBuilder(true);
            for (int k = 0; k < tri.Length; k++)
                b.AddTriangle(tri[k].t);
            BuildImmutables<T, U, Node, Tree> im = new BuildImmutables<T, U, Node, Tree>()
            { 
                costEstimator = se, 
                tris = tri,
                branchCounter = 0,
                leafCounter = 0,
                mandatoryLeafSize = mandatoryLeafSize,
                splitDegenerateNodes = splitDegenerateNodes,
                builder = builder
            };
            int len = tri.Length-1;
            Box3 topBox = b.GetBox();
            Node root = BuildNodeSegment(0, tri.Length, 0, topBox, im.costEstimator.GetDefaultState(topBox), im);
            return builder.BuildTree(root, im.branchCounter);
        }

        private static Node BuildNodeSegment<StackState, BranchData, Node, Tree>(int start, int end, int depth, Box3 objectBound, StackState evaluatorState, BuildImmutables<StackState, BranchData, Node, Tree> im)
        {
        //    if ((im.branchCounter & 1023) == 0)
        //        Console.WriteLine("I'm thiiiis far:" + im.branchCounter + " " + depth);

            int numTris = end - start;
            if (numTris <= 0)
                throw new ArgumentException("Cannot build a tree from "+numTris+" leaves.");
            // base cases
            if (numTris <= im.mandatoryLeafSize)
            {
                return im.builder.BuildLeaf(im.tris, start, end, im.leafCounter++, depth, objectBound);
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

            // recursive case
            int numBins = Math.Min(32, (int)(numTris * .05f + 4f));
            Box3 centroidBounds = BuildTools.FindCentroidBound(im.tris, start, end);
            CVector3 scaleVec = new CVector3(numBins / centroidBounds.XRange.Size, numBins / centroidBounds.YRange.Size, numBins / centroidBounds.ZRange.Size)/2;
            BestObjectPartition<BranchData> part = FindBestPartition(start, end, scaleVec, centroidBounds.TripleMin(), numBins, evaluatorState, im);

            if (part.isDegenerate && !im.splitDegenerateNodes)
            {
                return im.builder.BuildLeaf(im.tris, start, end, im.leafCounter++, depth, objectBound);
            }
            else
            {
                int id = im.branchCounter++;
                Node left = BuildNodeSegment(start, part.objectPartition, depth + 1, part.leftObjectBounds, im.costEstimator.SetState(part.leftObjectBounds, evaluatorState), im);
                Node right = BuildNodeSegment(part.objectPartition, end, depth + 1, part.rightObjectBounds, im.costEstimator.SetState(part.rightObjectBounds, evaluatorState), im);
                return im.builder.BuildBranch(left, right, part.branchBuildData, id, depth, objectBound);
            }
        }
        private static BestObjectPartition<BranchData> FindBestPartition<StackState, BranchData, Node, Tree>(int start, int end, CVector3 scale, CVector3 min, int numBlocks, StackState evaluatorState, BuildImmutables<StackState, BranchData, Node, Tree> im)
        {
            scale = scale * 2;
            int len = end - start;
            BuildTriangle[] tris = im.tris;

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
                int blockX = Math.Max(0, Math.Min(numBlocks - 1, (int)((t.center.x - min.x) * scale.x)));
                int blockY = Math.Max(0, Math.Min(numBlocks - 1, (int)((t.center.y - min.y) * scale.y)));
                int blockZ = Math.Max(0, Math.Min(numBlocks - 1, (int)((t.center.z - min.z) * scale.z)));
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
            BestBinPartition<BranchData> resX = xDegen ? null : ScorePartitions(blockCountsX, blockBoundsX, im.costEstimator, evaluatorState, new AASplit(SplitDimension.SplitX, min.x, scale.x));
            BestBinPartition<BranchData> resY = yDegen ? null : ScorePartitions(blockCountsY, blockBoundsY, im.costEstimator, evaluatorState, new AASplit(SplitDimension.SplitY, min.y, scale.y));
            BestBinPartition<BranchData> resZ = zDegen ? null : ScorePartitions(blockCountsZ, blockBoundsZ, im.costEstimator, evaluatorState, new AASplit(SplitDimension.SplitZ, min.z, scale.z));

            AASplit split;
            BestBinPartition<BranchData> res;
            // the partition function has a funky signature because it needs to have the EXACT SAME numerical precision properties as the binning check above
            if (!xDegen && (yDegen || resX.heuristicValue <= resY.heuristicValue) && (zDegen || resX.heuristicValue <= resZ.heuristicValue))
            {
                res = resX;
                split = new AASplit(SplitDimension.SplitX, min.x, scale.x, resX.binPartition);
            }
            else if (!yDegen && (zDegen || resY.heuristicValue <= resZ.heuristicValue))
            {
                res = resY;
                split = new AASplit(SplitDimension.SplitY, min.y, scale.y, resY.binPartition);
            }
            else if(!zDegen)
            {
                res = resZ;
                split = new AASplit(SplitDimension.SplitZ, min.z, scale.z, resZ.binPartition);
            }
            else
            {
                // triple degenerate
                if (!(xDegen && yDegen && zDegen))
                    throw new Exception("Oh god, my logic is wrong");
                Box3 box = blockBoundsX[0].GetBox();
                return new BestObjectPartition<BranchData>() { leftObjectBounds = box, rightObjectBounds = box, branchBuildData = default(BranchData), objectPartition = (start + end) / 2, isDegenerate = true };
            }

            int index = split.PerformPartition(tris, start, end);

            if (index >= end || index<=start)
            {
                throw new Exception("This shouldn't happen.");
            }

            return new BestObjectPartition<BranchData>() { leftObjectBounds = res.leftObjectBounds, rightObjectBounds = res.rightObjectBounds, branchBuildData = res.branchBuildData, objectPartition = index, isDegenerate = false };
        }

        private static BestBinPartition<BranchData> ScorePartitions<StackState, BranchData>(int[] blockCounts, BoundBuilder[] blockOBounds, BVHSplitEvaluator<StackState, BranchData> se, StackState evaluatorState, AASplit split)
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
                // float cost = ((forwardPrevCount + 3) >> 2) * forwardBox.SurfaceArea + ((backwardCountAccumulator[k + 1] + 3) >> 2) * backwardBoxAccumulator[k + 1].SurfaceArea;
                split.Threshold = k;
                EvalResult<BranchData> cost = se.EvaluateSplit(forwardPrevCount, forwardBox, backwardCountAccumulator[k + 1], backwardBoxAccumulator[k + 1], evaluatorState, split);
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

        private class BuildImmutables<StackState, BranchData, Node, Tree>
        {
            public BuildTriangle[] tris;
            public int mandatoryLeafSize;
            public bool splitDegenerateNodes;
            public BVHSplitEvaluator<StackState, BranchData> costEstimator;
            public NodeFactory<Node, Tree, BranchData> builder;
            public int branchCounter;
            public int leafCounter;
        }
    }
}
