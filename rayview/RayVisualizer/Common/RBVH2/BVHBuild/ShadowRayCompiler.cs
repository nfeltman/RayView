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

    public class ShadowRayCompiler
    {
        
        public static ShadowRayResults<BasicBuildTriangle> CompileCasts(IEnumerable<ShadowQuery> set, BVH2 bvh)
        {
            return CompileCasts<Triangle, BasicBuildTriangle, BVH2Branch, BVH2Leaf>(
                set.Select(q => new Segment3(q.Origin, q.Difference)), 
                bvh, 
                bbt => bbt.T, 
                (tri, counter) => new BasicBuildTriangle(tri, counter));
        }

        public static ShadowRayResults<OBJBackedBuildTriangle> CompileCasts(IEnumerable<ShadowQuery> set, BackedBVH2 bvh, Func<int, Triangle> realTris)
        {
            return CompileCasts<int, OBJBackedBuildTriangle, BackedBVH2Branch, BackedBVH2Leaf>(
                set.Select(q => new Segment3(q.Origin, q.Difference)), 
                bvh, 
                obbt => realTris(obbt.OBJIndex), 
                (prim, counter)=>new OBJBackedBuildTriangle(counter,realTris(prim),prim));
        }

        public static ShadowRayResults<BuildTri> CompileCasts<PrimT, BuildTri, IncBranch, IncLeaf>(IEnumerable<Segment3> set, Tree<IncBranch, IncLeaf> bvh, Func<BuildTri, Triangle> realTris, Func<PrimT, int, BuildTri> makeBuildTri)
            where IncLeaf : Primitived<PrimT>, Boxed
            where IncBranch : Boxed
        {
            List<BuildTri> tris = new List<BuildTri>();
            int counter = 0;
            TreeNode<SCBranch, SCLeaf<PrimT, BuildTri>> root = bvh.RollUp<TreeNode<SCBranch, SCLeaf<PrimT, BuildTri>>>(
                (branch, left, right)
                    => new Branch<SCBranch, SCLeaf<PrimT, BuildTri>>(left, right, new SCBranch(branch.BBox)),
                (leaf)
                    =>
                {
                    var ret = new Leaf<SCBranch, SCLeaf<PrimT, BuildTri>>(new SCLeaf<PrimT, BuildTri>(leaf.BBox, leaf.Primitives, prim => makeBuildTri(prim, counter++)));
                    tris.AddRange(ret.Content.tris);
                    return ret;
                });

            List<Segment3> connected = new List<Segment3>();
            List<CompiledShadowRay<BuildTri>> broken = new List<CompiledShadowRay<BuildTri>>();

            SCIntersector<PrimT, BuildTri> vis = new SCIntersector<PrimT, BuildTri>() { RealTri = realTris };
            foreach (Segment3 q in set)
            {
                vis.Intersected = new List<BuildTri>();
                vis.Ray = new Segment3(q.Origin, q.Difference);
                if (root.Accept(vis))
                    connected.Add(vis.Ray);
                else
                    broken.Add(new CompiledShadowRay<BuildTri>() { IntersectedTriangles = vis.Intersected.ToArray(), Ray = vis.Ray });
            }

            return new ShadowRayResults<BuildTri>() { Connected = connected.ToArray(), Broken = broken.ToArray(), Triangles = tris.ToArray() };
        }

        private class SCIntersector<PrimT, Tri> : NodeVisitor<bool, SCBranch, SCLeaf<PrimT, Tri>>
        {
            public List<Tri> Intersected { get; set; }
            public Segment3 Ray { get; set; }
            public Func<Tri, Triangle> RealTri { get; set; }

            public bool ForBranch(Branch<SCBranch, SCLeaf<PrimT, Tri>> branch)
            {
                if (branch.Content.BBox.DoesIntersectSegment(Ray.Origin, Ray.Difference))
                {
                    return branch.Left.Accept(this) & branch.Right.Accept(this);
                }
                return true;
            }

            public bool ForLeaf(Leaf<SCBranch, SCLeaf<PrimT, Tri>> leaf)
            {
                bool ret = true;
                if (leaf.Content.BBox.DoesIntersectSegment(Ray.Origin, Ray.Difference))
                {
                    Tri[] tris = leaf.Content.tris;
                    for (int k = 0; k < tris.Length; k++ )
                    {
                        float intersection = RealTri(tris[k]).IntersectRay(Ray.Origin, Ray.Difference);
                        if (intersection > 0 && intersection < 1 && !float.IsNaN(intersection))
                        {
                            Intersected.Add(tris[k]);
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
            public SCBranch(Box3 box)
            {
                BBox = box;
            }
        }

        private struct SCLeaf<PrimT, Tri>
        {
            public Box3 BBox;
            public Tri[] tris;
            public SCLeaf(Box3 box, PrimT[] prims, Func<PrimT, Tri> cons)
            {
                BBox = box;
                tris = new Tri[prims.Length];
                for (int k = 0; k < tris.Length; k++)
                    tris[k] = cons(prims[k]);
            }
        }
    }

    public struct CompiledShadowRay<Tri>
    {
        public Tri[] IntersectedTriangles; // the center of the bounding boxes of intersected triangles
        public int MaxIntersectedTriangles;
        public Segment3 Ray;
    }

    public class ShadowRayResults<Tri>
    {
        public Tri[] Triangles { get; set; }
        public Segment3[] Connected { get; set; }
        public CompiledShadowRay<Tri>[] Broken { get; set; }
    }


}
