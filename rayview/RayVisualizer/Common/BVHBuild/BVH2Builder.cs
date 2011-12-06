using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class BVH2Builder
    {
        public static BVH2 BuildBVHSAH(BuildTriangle[] tri)
        {
            //return BuildBVH(tri, (left_nu, left_box, right_nu, right_box) => left_nu * left_box.SurfaceArea + right_nu * right_box.SurfaceArea);
            return BuildBVH(tri, (left_nu, left_box, right_nu, right_box) => blockShuffleThing(left_nu) * left_box.SurfaceArea + blockShuffleThing(right_nu) * right_box.SurfaceArea);
        }

        private static int blockShuffleThing(int i)
        {
            return (i+((1<<2)-1)) >> 2;
        }

        public static BVH2 BuildBVH(BuildTriangle[] tri, Func<int, Box3, int, Box3, float> costEstimator)
        {
            BuildImmutables im = new BuildImmutables() { costEstimator = costEstimator, tris = tri, branchCounter = 0, leafCounter = 0 };
            BVH2Node root = BuildNodeSegment(0, tri.Length, 0, im);
            return new BVH2(root, im.branchCounter);
        }

        private static BVH2Node BuildNodeSegment(int start, int end, int depth, BuildImmutables im)
        {
            // base case
            if (end - start <= 4)
            {
                if (end * 100 / im.tris.Length != start * 100 / im.tris.Length)
                    Console.WriteLine(start * 100 / im.tris.Length);
                Triangle[] prims = new Triangle[end-start];
                for (int k = 0; k < prims.Length; k++)
                {
                    prims[k] = im.tris[k + start].t;
                }
                return new BVH2Leaf() { Primitives = prims, ID = im.branchCounter, Depth = depth, BBox = FindObjectBound(im.tris, start, end) };
            }

            // recursive case
            int numBins = Math.Min(32, (int)((end - start) * .05f + 4f));
            Box3 centroidBounds = FindCentroidBound(im.tris, start, end);
            CVector3 scaleVec = new CVector3(numBins / centroidBounds.XRange.Size, numBins / centroidBounds.YRange.Size, numBins / centroidBounds.ZRange.Size)/2;
            int bestPartitionLoc = FindBestPartition(start, end, scaleVec, centroidBounds.TripleMin(), numBins, im);
            BVH2Node left = BuildNodeSegment(start, bestPartitionLoc, depth + 1, im);
            BVH2Node right = BuildNodeSegment(bestPartitionLoc, end, depth + 1, im);
            return new BVH2Branch() { Left = left, Right = right, BBox = FindObjectBound(im.tris, start, end), ID = im.branchCounter++, Depth = depth };
        }

        private static int FindBestPartition(int start, int end, CVector3 scale, CVector3 min, int numBlocks, BuildImmutables im)
        {
            //calculate block counts and bounds
            int[] blockCountsX = new int[numBlocks];
            int[] blockCountsY = new int[numBlocks];
            int[] blockCountsZ = new int[numBlocks];
            Box3[] blockBoundsX = new Box3[numBlocks];
            Box3[] blockBoundsY = new Box3[numBlocks];
            Box3[] blockBoundsZ = new Box3[numBlocks];
            for (int k = 0; k < numBlocks; k++)
            {
                blockBoundsX[k] = blockBoundsY[k] = blockBoundsZ[k] = Box3.EMPTY;
            }
            for (int k = start; k < end; k++)
            {
                BuildTriangle t = im.tris[k];
                CVector3 diff = (t.center - min) * 2;
                int blockX = Math.Max(0, Math.Min(numBlocks - 1, (int)(diff.x * scale.x)));
                int blockY = Math.Max(0, Math.Min(numBlocks - 1, (int)(diff.y * scale.y)));
                int blockZ = Math.Max(0, Math.Min(numBlocks - 1, (int)(diff.z * scale.z)));
                blockCountsX[blockX]++;
                blockCountsY[blockY]++;
                blockCountsZ[blockZ]++;
                blockBoundsX[blockX] = blockBoundsX[blockX] | t.box;
                blockBoundsY[blockY] = blockBoundsY[blockY] | t.box;
                blockBoundsZ[blockZ] = blockBoundsZ[blockZ] | t.box;
            }

            BestPartition resX = ScorePartitions(blockCountsX, blockBoundsX, im.costEstimator);
            BestPartition resY = ScorePartitions(blockCountsY, blockBoundsY, im.costEstimator);
            BestPartition resZ = ScorePartitions(blockCountsZ, blockBoundsZ, im.costEstimator);
            int index;
            float partVal;
            BestPartition res;
            if (resX.heuristicValue < resY.heuristicValue && resX.heuristicValue < resZ.heuristicValue)
            {
                res = resX;
                partVal = (resX.partition) / scale.x /2 + min.x;
                index = PerformPartition(partVal, Dimension3.X, im.tris, start, end);
            }
            else if (resY.heuristicValue < resX.heuristicValue && resY.heuristicValue < resZ.heuristicValue)
            {
                res = resY;
                partVal = (resY.partition) / scale.y/2 + min.y;
                index = PerformPartition(partVal, Dimension3.Y, im.tris, start, end);
            }
            else
            {
                res = resZ;
                partVal = (resZ.partition) / scale.z/2 + min.z;
                index = PerformPartition(partVal, Dimension3.Z, im.tris, start, end);
            }

            if (index == start || index == end)
            {
               // for (int k = start; k < end; k++)
               //     Console.WriteLine(k + ": " + im.tris[k].center.x + " " + im.tris[k].center.y + " " + im.tris[k].center.z);
                Console.WriteLine("index: " + index);
                Console.WriteLine("partVal: " + partVal);
                Math.Sin(20);
            }

            return index;
        }

        private static BestPartition ScorePartitions(int[] blockCounts, Box3[] blockBounds, Func<int, Box3, int, Box3, float> costEstimator)
        {
            int numBlocks = blockCounts.Length;

            // build forward and backward box and count accumulators
            Box3[] forwardBoxAccumulator = new Box3[numBlocks];
            Box3[] backwardBoxAccumulator = new Box3[numBlocks];
            Box3 forwardPrevBox = Box3.EMPTY;
            Box3 backwordPrevBox = Box3.EMPTY;
            int[] forwardCountAccumulator = new int[numBlocks];
            int[] backwardCountAccumulator = new int[numBlocks];
            int forwardPrevCount = 0;
            int backwardPrevCount = 0;
            for (int k = 0; k < numBlocks; k++)
            {
                int j = numBlocks - k - 1;
                   forwardBoxAccumulator[k] = forwardPrevBox    = forwardPrevBox    | blockBounds[k];
                  backwardBoxAccumulator[j] = backwordPrevBox   = backwordPrevBox   | blockBounds[j];
                 forwardCountAccumulator[k] = forwardPrevCount  = forwardPrevCount  + blockCounts[k];
                backwardCountAccumulator[j] = backwardPrevCount = backwardPrevCount + blockCounts[j];
            }

            // find smallest cost
            float minCost = float.PositiveInfinity;
            int bestPartition = -10;
            for (int k = 0; k < numBlocks-1; k++)
            {
                float cost = costEstimator(forwardCountAccumulator[k], forwardBoxAccumulator[k], backwardCountAccumulator[k + 1], backwardBoxAccumulator[k + 1]);
                if (cost < minCost)
                {
                    bestPartition = k+1;
                    minCost = cost;
                }
            }

            if (bestPartition == 0)
            {
             //  for (int k = 0; k < numBlocks; k++)
             //       Console.WriteLine(k + ": " + blockCounts[k] + " " + forwardCountAccumulator[k] + " " + backwardCountAccumulator[k]);
                Math.Sin(20);
            }

            return new BestPartition() { partition = bestPartition, wholeBoundingBox = forwardBoxAccumulator[numBlocks - 1], heuristicValue = minCost };
        }

        private struct BestPartition
        {
            public int partition;
            public float heuristicValue;
            public Box3 wholeBoundingBox;
        }

        private static int PerformPartition(float partVal, Dimension3 dim, BuildTriangle[] tri, int start, int end)
        {
            int partLoc = start; // the first larger-than-partVal element

            if (dim == Dimension3.X)
            {
                for (int k = start; k < end; k++)
                    if (tri[k].center.x < partVal)
                    {
                        Swap(tri, k, partLoc);
                        partLoc++;
                    }
            }
            else if (dim == Dimension3.Y)
            {
                for (int k = start; k < end; k++)
                    if (tri[k].center.y < partVal)
                    {
                        Swap(tri, k, partLoc);
                        partLoc++;
                    }
            }
            else
            {
                for (int k = start; k < end; k++)
                    if (tri[k].center.z < partVal)
                    {
                        Swap(tri, k, partLoc);
                        partLoc++;
                    }
            }
            return partLoc;
        }

        private static void Swap(BuildTriangle[] tri, int i1, int i2)
        {
            BuildTriangle temp = tri[i1];
            tri[i1] = tri[i2];
            tri[i2] = temp;
        }

        private static Box3 FindObjectBound(BuildTriangle[] tris, int start, int end)
        {
            if (start == end)
                return Box3.EMPTY;
            float minX = tris[start].box.XRange.Min;
            float maxX = tris[start].box.XRange.Max;
            float minY = tris[start].box.YRange.Min;
            float maxY = tris[start].box.YRange.Max;
            float minZ = tris[start].box.ZRange.Min;
            float maxZ = tris[start].box.ZRange.Max;
            for (int k = start+1; k < end; k++)
            {
                minX = Math.Min(minX, tris[k].box.XRange.Min);
                maxX = Math.Max(maxX, tris[k].box.XRange.Max);
                minY = Math.Min(minY, tris[k].box.YRange.Min);
                maxY = Math.Max(maxY, tris[k].box.YRange.Max);
                minZ = Math.Min(minZ, tris[k].box.ZRange.Min);
                maxZ = Math.Max(maxZ, tris[k].box.ZRange.Max);
            }
            return new Box3(minX, maxX, minY, maxY, minZ, maxZ);
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

        private class BuildImmutables
        {
            public BuildTriangle[] tris;
            public Func<int, Box3, int, Box3, float> costEstimator;
            public int branchCounter;
            public int leafCounter;
        }

        private enum Dimension3
        {
            X, Y, Z
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
    }

    public struct BuildTriangle
    {
        public Triangle t;
        public Box3 box;
        public CVector3 center;

        public BuildTriangle(Triangle init)
        {
            t = init;
            box = new Box3(init.p1, init.p2, init.p3);
            center = box.GetCenter();
        }
    }
}
