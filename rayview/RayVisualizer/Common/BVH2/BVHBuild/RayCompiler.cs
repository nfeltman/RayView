using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    using BVH2 = Tree<BVH2Branch, BVH2Leaf>;
    using RBVH2 = Tree<RBVH2Branch, RBVH2Leaf>;
    using BackedBVH2 = Tree<BackedBVH2Branch, BackedBVH2Leaf>;
    using BackedRBVH2 = Tree<BackedRBVH2Branch, BackedRBVH2Leaf>;

    public class RayCompiler
    {
        public static FHRayResults CompileCasts(RaySet set, BVH2 bvh)
        {
            List<Segment3> hits = new List<Segment3>();
            List<Ray3> misses = new List<Ray3>();
            NullRayOrderOperations ops = new NullRayOrderOperations();
            foreach (CastHitQuery ray in set.CastHitQueries)
            {
                HitRecord rec = RayOrderTraverser.RunTooledTraverser(bvh, ray.Origin, ray.Difference, ops);
                if (rec == null)
                {
                    misses.Add(new Ray3(ray.Origin, ray.Difference));
                }
                else
                {
                    hits.Add(new Segment3(ray.Origin, ray.Difference * rec.t_value));
                }
            }
            foreach (CastMissQuery ray in set.CastMissQueries)
            {
                HitRecord rec = RayOrderTraverser.RunTooledTraverser(bvh, ray.Origin, ray.Direction, ops);
                if (rec == null)
                {
                    misses.Add(new Ray3(ray.Origin, ray.Direction));
                }
                else
                {
                    hits.Add(new Segment3(ray.Origin, ray.Direction * rec.t_value));
                }
            }
            return new FHRayResults() { Hits = hits.ToArray(), Misses = misses.ToArray() };
        }

        public static FHRayResults CompileCasts(IEnumerable<Ray3> allCasts, BVH2 bvh)
        {
            List<Segment3> hits = new List<Segment3>();
            List<Ray3> misses = new List<Ray3>();
            NullRayOrderOperations ops = new NullRayOrderOperations();
            foreach (Ray3 ray in allCasts)
            {
                HitRecord rec = RayOrderTraverser.RunTooledTraverser(bvh, ray.Origin, ray.Direction, ops);
                if (rec == null)
                {
                    misses.Add(new Ray3(ray.Origin, ray.Direction));
                }
                else
                {
                    hits.Add(new Segment3(ray.Origin, ray.Direction * rec.t_value));
                }
            }
            return new FHRayResults() { Hits = hits.ToArray(), Misses = misses.ToArray() };
        }
    }

    public class FHRayResults
    {
        public Segment3[] Hits { get; set; }
        public Ray3[] Misses { get; set; }
    }
}
