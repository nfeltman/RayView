using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class ShadowRayCompiler
    {
        public static ShadowRayResults CompileCasts(RaySet set, BVH2 bvh)
        {
            List<Segment3> hits = new List<Segment3>();
            List<Segment3> misses = new List<Segment3>();
            return new ShadowRayResults();
        }
    }

    public class CompiledShadowRay
    {
        public BuildTriangle[] IntersectedTriangles; // the center of the bounding boxes of intersected triangles
        public int MaxIntersectedTriangles;
        public Segment3 Ray;
    }

    public class ShadowRayResults
    {
        public Segment3[] Connected { get; set; }
        public CompiledShadowRay[] Broken { get; set; }
    }
}
