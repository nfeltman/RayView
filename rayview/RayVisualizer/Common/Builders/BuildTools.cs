using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public static class BuildTools
    {
        public static Box3 FindCentroidBound(BuildTriangle[] tris, int start, int end)
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
                    {
                        list[counter] = new BuildTriangle(t, counter);
                        ++counter;
                    }
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
                res[k] = new BuildTriangle(tris[k], k);
            }
            return res;
        }

        public static void Swap<T>(T[] arr, int loc1, int loc2)
        {
            T temp = arr[loc1];
            arr[loc1] = arr[loc2];
            arr[loc2] = temp;
        }

        public static int SweepPartition<T>(T[] arr, int start, int end, Func<T,bool> goesLeft)
        {
            int part = start;
            // filter "connected" buffer
            for (int k = start; k < end; k++)
            {
                if (goesLeft(arr[k]))
                {
                    if (part != k)
                    {
                        Swap(arr, part, k);
                    }
                    part++;
                }
            }
            return part;
        }

        /*
        public static int MinSwapsPartition<T>(T[] arr, int start, int end, Func<T, bool> goesLeft)
        {

        }
         */
    }
}
