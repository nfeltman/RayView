using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RayVisualizer.Common
{
    public static class BVH2Builder
    {        
        public static BVH2 BuildBVHTest(BuildTriangle[] tri)
        {
            //return BuildBVH(tri, (left_nu, left_box, right_nu, right_box) => left_nu * left_box.SurfaceArea + right_nu * right_box.SurfaceArea);
            return BuildBVH(tri, (left_nu, left_box, right_nu, right_box) => ((left_nu + 3) >> 2) * left_box.SurfaceArea + ((right_nu + 3) >> 2) * right_box.SurfaceArea, 4, false);
        }

        public static BVH2 BuildFullBVH(BuildTriangle[] tri, Func<int, Box3, int, Box3, float> est)
        {
            return BuildFullBVH(tri, new StatelessSplitEvaluator(est));
        }

        public static BVH2 BuildFullBVH<T>(BuildTriangle[] tri, SplitEvaluator<T> se)
        {
            return BuildBVH(tri, se, 1, true);
        }

        public static BVH2 BuildBVH(BuildTriangle[] tri, Func<int, Box3, int, Box3, float> est, int mandatoryLeafSize, bool splitDegenerateNodes)
        {
            return BuildBVH(tri, new StatelessSplitEvaluator(est), mandatoryLeafSize, splitDegenerateNodes);
        }

        public static BVH2 BuildBVH<T>(BuildTriangle[] tri, SplitEvaluator<T> se, int mandatoryLeafSize, bool splitDegenerateNodes)
        {
            if (tri.Length == 0)
                throw new ArgumentException("BVH Cannot be empty");
            BoundBuilder b = new BoundBuilder(true);
            for (int k = 0; k < tri.Length; k++)
                b.AddTriangle(tri[k].t);
            BuildImmutables<T> im = new BuildImmutables<T>() { 
                costEstimator = se, 
                tris = tri,
                branchCounter = 0,
                leafCounter = 0,
                mandatoryLeafSize = mandatoryLeafSize,
                splitDegenerateNodes = splitDegenerateNodes
            };
            int len = tri.Length-1;
            Box3 topBox = b.GetBox();
            BVH2Node root = BuildNodeSegment(0, tri.Length, 0, topBox, im.costEstimator.GetDefaultState(topBox), im);
            return new BVH2(root, im.branchCounter);
        }

        private static BVH2Node BuildNodeSegment<T>(int start, int end, int depth, Box3 objectBound, T evaluatorState, BuildImmutables<T> im)
        {
        //    if ((im.branchCounter & 1023) == 0)
        //        Console.WriteLine("I'm thiiiis far:" + im.branchCounter + " " + depth);

            int numTris = end - start;
            if (numTris <= 0)
                throw new ArgumentException("Cannot build a tree from "+numTris+" leaves.");
            // base cases
            if (numTris <= im.mandatoryLeafSize)
            {
                Triangle[] prims = new Triangle[numTris];
                for (int k = 0; k < prims.Length; k++)
                    prims[k] = im.tris[k + start].t;
                return new BVH2Leaf() { Primitives = prims, ID = im.leafCounter++, Depth = depth, BBox = objectBound };
            }
            else if (numTris==2)
            {
                int id = im.branchCounter++;
                BuildTriangle lTri = im.tris[start];
                BuildTriangle rTri = im.tris[start+1];
                BoundBuilder build = new BoundBuilder(true);
                build.AddTriangle(lTri.t);
                BVH2Node left = new BVH2Leaf() { Primitives = new Triangle[] { lTri.t }, ID = im.leafCounter++, Depth = depth + 1, BBox = build.GetBox() };
                build.Reset();
                build.AddTriangle(rTri.t);
                BVH2Node right = new BVH2Leaf() { Primitives = new Triangle[] { rTri.t }, ID = im.leafCounter++, Depth = depth + 1, BBox = build.GetBox() };
                return new BVH2Branch() { Left = left, Right = right, BBox = objectBound, ID = id, Depth = depth };
            }

            // recursive case
            int numBins = Math.Min(32, (int)(numTris * .05f + 4f));
            Box3 centroidBounds = FindCentroidBound(im.tris, start, end);
            CVector3 scaleVec = new CVector3(numBins / centroidBounds.XRange.Size, numBins / centroidBounds.YRange.Size, numBins / centroidBounds.ZRange.Size)/2;
            BestObjectPartition part = FindBestPartition(start, end, scaleVec, centroidBounds.TripleMin(), numBins, evaluatorState, im);

            if (part.isDegenerate && !im.splitDegenerateNodes)
            {
                Triangle[] prims = new Triangle[numTris];
                for (int k = 0; k < prims.Length; k++)
                    prims[k] = im.tris[k + start].t;
                return new BVH2Leaf() { BBox = objectBound, Depth = depth, ID = im.leafCounter++, Primitives = prims };
            }
            else
            {
                int id = im.branchCounter++;
                BVH2Node left = BuildNodeSegment(start, part.objectPartition, depth + 1, part.leftBox, im.costEstimator.SetState(part.leftBox,evaluatorState), im);
                BVH2Node right = BuildNodeSegment(part.objectPartition, end, depth + 1, part.rightBox, im.costEstimator.SetState(part.rightBox,evaluatorState), im);
                return new BVH2Branch() { Left = left, Right = right, BBox = objectBound, ID = id, Depth = depth };
            }
        }
        private static BestObjectPartition FindBestPartition<T>(int start, int end, CVector3 scale, CVector3 min, int numBlocks, T evaluatorState, BuildImmutables<T> im)
        {
            scale = scale * 2;
            int len = end - start;
            BuildTriangle[] tris = im.tris;
            //calculate block counts and bounds
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
            BestBinPartition resX = xDegen ? null : ScorePartitions(blockCountsX, blockBoundsX, im.costEstimator, evaluatorState);
            BestBinPartition resY = yDegen ? null : ScorePartitions(blockCountsY, blockBoundsY, im.costEstimator, evaluatorState);
            BestBinPartition resZ = zDegen ? null : ScorePartitions(blockCountsZ, blockBoundsZ, im.costEstimator, evaluatorState);
            
            int index;
            BestBinPartition res;
            // the partition function has a funky signature because it needs to have the EXACT SAME numerical precision properties as the binning check above
            if (!xDegen && (yDegen || resX.heuristicValue <= resY.heuristicValue) && (zDegen || resX.heuristicValue <= resZ.heuristicValue))
            {
                res = resX;
                index = PerformPartitionX(resX.binPartition, min.x, scale.x, tris, start, end);
            }
            else if (!yDegen && (zDegen || resY.heuristicValue <= resZ.heuristicValue))
            {
                res = resY;
                index = PerformPartitionY(resY.binPartition, min.y, scale.y, tris, start, end);
            }
            else if(!zDegen)
            {
                res = resZ;
                index = PerformPartitionZ(resZ.binPartition, min.z, scale.z, tris, start, end);
            }
            else
            {
                // triple degenerate
                if (!(xDegen && yDegen && zDegen))
                    throw new Exception("Oh god, my logic is wrong");
                Box3 box = blockBoundsX[0].GetBox();
                return new BestObjectPartition() { leftBox = box, rightBox = box, objectPartition = (start + end) / 2, isDegenerate = true };
            }

            if (index >= end || index<=start)
            {
                throw new Exception("This shouldn't happen.");
            }

            return new BestObjectPartition() { leftBox = res.leftBox, rightBox = res.rightBox, objectPartition = index, isDegenerate = false };
        }

        private static BestBinPartition ScorePartitions<T>(int[] blockCounts, BoundBuilder[] blockBounds, SplitEvaluator<T> se, T evaluatorState)
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
                builder.AddBox(blockBounds[k]);
                backwardBoxAccumulator[k] = builder.GetBox();
            }

            // find smallest cost
            float minCost = float.PositiveInfinity;
            int bestPartition = -10;
            Box3 bestForwardBox = new Box3();
            int forwardPrevCount = 0;
            builder.Reset();
            for (int k = 0; k < numBlocks-1; k++)
            {
                forwardPrevCount = forwardPrevCount + blockCounts[k];
                builder.AddBox(blockBounds[k]);
                Box3 forwardBox = builder.GetBox();
                // float cost = ((forwardPrevCount + 3) >> 2) * forwardBox.SurfaceArea + ((backwardCountAccumulator[k + 1] + 3) >> 2) * backwardBoxAccumulator[k + 1].SurfaceArea;
                float cost = se.EvaluateSplit(forwardPrevCount, forwardBox, backwardCountAccumulator[k + 1], backwardBoxAccumulator[k + 1], evaluatorState);
                if (cost < minCost)
                {
                    bestPartition = k+1;
                    minCost = cost;
                    bestForwardBox = forwardBox;
                }
            }

            if (bestPartition == 0)
            {
                throw new Exception("Best partition shouldn't be the first.");
            }

            return new BestBinPartition() { 
                binPartition = bestPartition,
                heuristicValue = minCost, 
                leftBox = bestForwardBox, 
                rightBox = backwardBoxAccumulator[bestPartition] };
        }

        private class BestBinPartition
        {
            public int binPartition;
            public float heuristicValue;
            public Box3 leftBox, rightBox;
        }

        private class BestObjectPartition
        {
            public bool isDegenerate;
            public int objectPartition;
            public Box3 leftBox, rightBox;
        }

        private static int PerformPartitionX(int partVal, float less, float times, BuildTriangle[] tri, int start, int end)
        {
            int partLoc = start; // the first larger-than-partVal element

            for (int k = start; k < end; k++)
            {
                if ((tri[k].center.x-less)*times < partVal)
                {
                    BuildTriangle temp = tri[k];
                    tri[k] = tri[partLoc];
                    tri[partLoc] = temp;
                    partLoc++;
                }
            }

            return partLoc;
        }

        private static int PerformPartitionY(int partVal, float less, float times, BuildTriangle[] tri, int start, int end)
        {
            int partLoc = start; // the first larger-than-partVal element

            for (int k = start; k < end; k++)
            {
                if ((tri[k].center.y - less) * times < partVal)
                {
                    BuildTriangle temp = tri[k];
                    tri[k] = tri[partLoc];
                    tri[partLoc] = temp;
                    partLoc++;
                }
            }

            return partLoc;
        }

        private static int PerformPartitionZ(int partVal, float less, float times, BuildTriangle[] tri, int start, int end)
        {
            int partLoc = start; // the first larger-than-partVal element

            for (int k = start; k < end; k++)
            {
                if ((tri[k].center.z - less) * times < partVal)
                {
                    BuildTriangle temp = tri[k];
                    tri[k] = tri[partLoc];
                    tri[partLoc] = temp;
                    partLoc++;
                }
            }

            return partLoc;
        }

        public struct BoundBuilder
        {
            private float xMin, xMax, yMin, yMax, zMin, zMax;
            public BoundBuilder(bool b)
            {
                xMin = yMin = zMin = float.PositiveInfinity;
                xMax = yMax = zMax = float.NegativeInfinity;
            }
            public void Reset()
            {
                xMin = yMin = zMin = float.PositiveInfinity;
                xMax = yMax = zMax = float.NegativeInfinity;
            }
            public void AddPoint(CVector3 p)
            {
                if (p.x < xMin) xMin = p.x;
                if (p.x > xMax) xMax = p.x;
                if (p.y < yMin) yMin = p.y;
                if (p.y > yMax) yMax = p.y;
                if (p.z < zMin) zMin = p.z;
                if (p.z > zMax) zMax = p.z;
            }
            public void AddTriangle(Triangle t)
            {
                if (t.p1.x < xMin) xMin = t.p1.x;
                if (t.p1.x > xMax) xMax = t.p1.x;
                if (t.p2.x < xMin) xMin = t.p2.x;
                if (t.p2.x > xMax) xMax = t.p2.x;
                if (t.p3.x < xMin) xMin = t.p3.x;
                if (t.p3.x > xMax) xMax = t.p3.x;
                if (t.p1.y < yMin) yMin = t.p1.y;
                if (t.p1.y > yMax) yMax = t.p1.y;
                if (t.p2.y < yMin) yMin = t.p2.y;
                if (t.p2.y > yMax) yMax = t.p2.y;
                if (t.p3.y < yMin) yMin = t.p3.y;
                if (t.p3.y > yMax) yMax = t.p3.y;
                if (t.p1.z < zMin) zMin = t.p1.z;
                if (t.p1.z > zMax) zMax = t.p1.z;
                if (t.p2.z < zMin) zMin = t.p2.z;
                if (t.p2.z > zMax) zMax = t.p2.z;
                if (t.p3.z < zMin) zMin = t.p3.z;
                if (t.p3.z > zMax) zMax = t.p3.z;
            }
            public void AddBox(Box3 b)
            {
                if (b.XRange.Min < xMin) xMin = b.XRange.Min;
                if (b.XRange.Max > xMax) xMax = b.XRange.Max;
                if (b.YRange.Min < yMin) yMin = b.YRange.Min;
                if (b.YRange.Max > yMax) yMax = b.YRange.Max;
                if (b.ZRange.Min < zMin) zMin = b.ZRange.Min;
                if (b.ZRange.Max > zMax) zMax = b.ZRange.Max;
            }
            public void AddBox(BoundBuilder b)
            {
                if (b.xMin < xMin) xMin = b.xMin;
                if (b.xMax > xMax) xMax = b.xMax;
                if (b.yMin < yMin) yMin = b.yMin;
                if (b.yMax > yMax) yMax = b.yMax;
                if (b.zMin < zMin) zMin = b.zMin;
                if (b.zMax > zMax) zMax = b.zMax;
            }
            public Box3 GetBox()
            {
                return new Box3(xMin, xMax, yMin, yMax, zMin, zMax);
            }
        }

        private static Box3 FindCentroidBound(BuildTriangle[] tris, int start, int end)
        {
            if (start == end)
                return Box3.EMPTY;
            CVector3 c0 = tris[start].center;
            float minX = c0.x;
            float maxX = c0.x;
            float minY = c0.y;
            float maxY = c0.y;
            float minZ = c0.z;
            float maxZ = c0.z;
            for (int k = start + 1; k < end; k++)
            {
                CVector3 c = tris[k].center;
                minX = Math.Min(minX, c.x);
                maxX = Math.Max(maxX, c.x);
                minY = Math.Min(minY, c.y);
                maxY = Math.Max(maxY, c.y);
                minZ = Math.Min(minZ, c.z);
                maxZ = Math.Max(maxZ, c.z);
            }
            return new Box3(minX, maxX, minY, maxY, minZ, maxZ);
        }

        private class BuildImmutables<T>
        {
            public BuildTriangle[] tris;
            public int mandatoryLeafSize;
            public bool splitDegenerateNodes;
            public SplitEvaluator<T> costEstimator;
            public int branchCounter;
            public int leafCounter;
        }

        public static BuildTriangle[] GetTriangleList(BVH2 bvh)
        {
            int numTris = bvh.RollUp((branch, left, right) => left + right, leaf => leaf.Primitives.Length);
            BuildTriangle[] list = new BuildTriangle[numTris];
            int counter = 0;
            bvh.PrefixEnumerate(
                b => { },
                l => 
                {
                    foreach (Triangle t in l.Primitives)
                        list[counter++] = new BuildTriangle(t);
                });
            if (counter != list.Length)
                throw new Exception("This shouldn't have happened!");
            return list;
        }

        public static BuildTriangle[] GetTriangleList(this Triangle[] tris)
        {
            BuildTriangle[] res = new BuildTriangle[tris.Length];
            for (int k = 0; k < res.Length; k++)
            {
                res[k] = new BuildTriangle(tris[k]);
            }
            return res;
        }
    }

    public struct BuildTriangle
    {
        public Triangle t;
        public CVector3 center;

        public BuildTriangle(Triangle init)
        {
            t = init;
            Box3 box = new Box3(init.p1, init.p2, init.p3);
            center = box.GetCenter();
        }
    }
}
