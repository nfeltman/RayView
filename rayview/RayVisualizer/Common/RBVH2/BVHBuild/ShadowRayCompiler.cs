using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class ShadowRayCompiler
    {
        public static ShadowRayResults CompileCasts(IEnumerable<Segment3> set, BVH2 bvh)
        {
            List<BuildTriangle> tris = new List<BuildTriangle>();
            int counter = 0;
            TreeNode<SCBranch, SCLeaf> root = bvh.RollUp<TreeNode<SCBranch, SCLeaf>>(
                (branch, left, right)
                    => new Branch<SCBranch, SCLeaf>(left, right, new SCBranch(branch)),
                (leaf)
                    =>
                    {
                        var ret = new Leaf<SCBranch, SCLeaf>(new SCLeaf(leaf, ref counter));
                        tris.AddRange(ret.Content.tris);
                        return ret;
                    });

            List<Segment3> connected = new List<Segment3>();
            List<CompiledShadowRay> broken = new List<CompiledShadowRay>();

            SCIntersector vis = new SCIntersector();
            foreach (Segment3 q in set)
            {
                vis.Intersected = new List<BuildTriangle>();
                vis.Ray = new Segment3(q.Origin, q.Difference);
                if (root.Accept(vis))
                    connected.Add(vis.Ray);
                else
                    broken.Add(new CompiledShadowRay() { IntersectedTriangles = vis.Intersected.ToArray(), Ray = vis.Ray });
            }

            return new ShadowRayResults() { Connected = connected.ToArray(), Broken = broken.ToArray(), Triangles=tris.ToArray() };
        }

        public static ShadowRayResults CompileCasts(IEnumerable<ShadowQuery> set, BVH2 bvh)
        {
            return CompileCasts(set.Select(q => new Segment3(q.Origin, q.Difference)), bvh);
        }

        public static ShadowRayResults CompileCasts(RaySet set, BVH2 bvh)
        {
            return CompileCasts(set.ShadowQueries, bvh);
        }

        private class SCIntersector : NodeVisitor<bool, SCBranch, SCLeaf>
        {
            public List<BuildTriangle> Intersected { get; set; }
            public Segment3 Ray { get; set; }

            public bool ForBranch(Branch<SCBranch, SCLeaf> branch)
            {
                if (branch.Content.BBox.DoesIntersectSegment(Ray.Origin, Ray.Difference))
                {
                    return branch.Left.Accept(this) & branch.Right.Accept(this);
                }
                return true;
            }

            public bool ForLeaf(Leaf<SCBranch, SCLeaf> leaf)
            {
                bool ret = true;
                if (leaf.Content.BBox.DoesIntersectSegment(Ray.Origin, Ray.Difference))
                {
                    foreach(BuildTriangle t in leaf.Content.tris)
                    {
                        float intersection = t.t.IntersectRay(Ray.Origin,Ray.Difference);
                        if (intersection > 0 && intersection < 1 && !float.IsNaN(intersection))
                        {
                            Intersected.Add(t);
                            ret = false;
                        }
                    }
                }
                return ret;
            }
        }


        private struct SCBranch
        {
            public Box3 BBox;
            public SCBranch(BVH2Branch br)
            {
                BBox = br.BBox;
            }
        }

        private struct SCLeaf
        {
            public Box3 BBox;
            public BuildTriangle[] tris;
            public SCLeaf(BVH2Leaf le, ref int counter)
            {
                BBox = le.BBox;
                tris = new BuildTriangle[le.Primitives.Length];
                for (int k = 0; k < tris.Length; k++)
                    tris[k] = new BuildTriangle(le.Primitives[k], counter++);
            }
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
        public BuildTriangle[] Triangles { get; set; }
        public Segment3[] Connected { get; set; }
        public CompiledShadowRay[] Broken { get; set; }
    }
}
