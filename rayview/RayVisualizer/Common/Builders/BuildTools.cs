﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    using BVH2 = Tree<BVH2Branch, BVH2Leaf>;
    using RBVH2 = Tree<RBVH2Branch, RBVH2Leaf>;
    using BackedBVH2 = Tree<BackedBVH2Branch, BackedBVH2Leaf>;
    using BackedRBVH2 = Tree<BackedRBVH2Branch, BackedRBVH2Leaf>;
    using Mono.Simd;

    public static class BuildTools
    {
        public static Box3 FindCentroidBound<Tri>(Tri[] tris, int start, int end)
            where Tri : Centerable
        {
            if (start == end)
                return Box3.EMPTY;
            CVector3 c0 = tris[start].Center;
            Vector4f min = c0.Vec;
            Vector4f max = min;
            for (int k = start + 1; k < end; k++)
            {
                Vector4f c = tris[k].Center.Vec;
                min = min.Min(c);
                max = max.Max(c);
            }
            return new Box3(min, max);
        }

        public static ClosedInterval FindDistanceBound<Tri>(Tri[] tris, CVector3 center, int start, int end)
            where Tri : Centerable
        {
            if (start == end)
                return ClosedInterval.EMPTY;
            float d = (tris[start].Center - center).Length();
            float min = d;
            float max = d;
            for (int k = start + 1; k < end; k++)
            {
                d = (tris[k].Center - center).Length();
                min= Math.Min(min, d);
                max= Math.Max(max, d);
            }
            return new ClosedInterval(min, max);
        }

        public static BasicBuildTriangle[] GetTriangleList(this BVH2 bvh)
        {
            int numTris = bvh.RollUp((branch, left, right) => left + right, leaf => leaf.Primitives.Length);
            BasicBuildTriangle[] list = new BasicBuildTriangle[numTris];
            int counter = 0;
            bvh.PrefixEnumerate(
                b => { },
                l =>
                {
                    foreach (Triangle t in l.Primitives)
                    {
                        list[counter] = new BasicBuildTriangle(t, counter);
                        ++counter;
                    }
                });
            if (counter != list.Length)
                throw new Exception("This shouldn't have happened!");
            return list;
        }

        public static BasicBuildTriangle[] GetTriangleList(this IList<Triangle> tris)
        {
            BasicBuildTriangle[] res = new BasicBuildTriangle[tris.Count];
            for (int k = 0; k < res.Length; k++)
            {
                res[k] = new BasicBuildTriangle(tris[k], k);
            }
            return res;
        }

        public static OBJBackedBuildTriangle[] GetOBJTriangleList(this IList<Triangle> tris)
        {
            OBJBackedBuildTriangle[] res = new OBJBackedBuildTriangle[tris.Count];
            for (int k = 0; k < res.Length; k++)
            {
                res[k] = new OBJBackedBuildTriangle(k, tris[k], k);
            }
            return res;
        }

        public static void Swap<T>(T[] arr, int loc1, int loc2)
        {
            T temp = arr[loc1];
            arr[loc1] = arr[loc2];
            arr[loc2] = temp;
        }

        public static int SweepPartition<T>(T[] arr, int start, int end, Func<T,bool> goesPrior)
        {
            int part = start;
            // filter "connected" buffer
            for (int k = start; k < end; k++)
            {
                if (goesPrior(arr[k]))
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
