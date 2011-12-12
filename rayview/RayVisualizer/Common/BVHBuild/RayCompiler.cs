using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class RayCompiler
    {
        public static FHRayResults CompileQuerySet(RaySet set, BVH2 bvh)
        {
            List<FHRayHit> hits = new List<FHRayHit>();
            List<FHRayMiss> misses = new List<FHRayMiss>();
            NullRayOrderOperations ops = new NullRayOrderOperations();
            foreach (RayQuery ray in set)
            {
                HitRecord rec = RayOrderTraverser.RunTooledTraverser(bvh, ray, ops);
                if (rec == null)
                {
                    misses.Add(new FHRayMiss() { Origin = ray.Origin, Direction = ray.Direction });
                }
                else
                {
                    hits.Add(new FHRayHit() { Origin = ray.Origin, Difference = ray.Direction * rec.t_value });
                }
            }
            return new FHRayResults() { Hits = hits.ToArray(), Misses = misses.ToArray() };
        }

        public static FHRayResults TooledCompileQuerySet(RaySet set, BVH2 bvh, int mod, Action<int> toCall)
        {
            List<FHRayHit> hits = new List<FHRayHit>();
            List<FHRayMiss> misses = new List<FHRayMiss>();
            NullRayOrderOperations ops = new NullRayOrderOperations();
            int k=0;
            foreach (RayQuery ray in set)
            {
                if (k % mod == 0)
                    toCall(k);
                k++;

                HitRecord rec = RayOrderTraverser.RunTooledTraverser(bvh, ray, ops);
                if (rec == null)
                {
                    misses.Add(new FHRayMiss() { Origin = ray.Origin, Direction = ray.Direction });
                }
                else
                {
                    hits.Add(new FHRayHit() { Origin = ray.Origin, Difference = ray.Direction * rec.t_value });
                }
            }
            return new FHRayResults() { Hits = hits.ToArray(), Misses = misses.ToArray() };
        }
    }

    public class FHRayResults
    {
        public FHRayHit[] Hits { get; set; }
        public FHRayMiss[] Misses { get; set; }
    }
}
