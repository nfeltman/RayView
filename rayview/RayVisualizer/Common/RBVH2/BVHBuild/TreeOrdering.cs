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

    public class TreeOrdering
    {
        private const float TRAV = 1.0f;
        private const float PRIM = 1.0f;

        public static RBVH2 ApplyRTSAHOrdering(RBVH2 tree)
        {
            return new RBVH2(tree.RollUp(
                (br, left, right) =>
                {
                    IntersectionReport P = UniformRays.GetReport(br.BBox, left.Box, right.Box);

                    float V = P.JustLeft * left.V + P.JustRight * right.V + P.Both * left.V * right.V + P.Neither;
                    float C_leftFirst = TRAV + P.Left * left.C + (P.JustRight + P.Both * left.V) * (TRAV + right.C) + P.Neither * TRAV;
                    float C_rightFirst = TRAV + P.Right * right.C + (P.JustLeft + P.Both * right.V) * (TRAV + left.C) + P.Neither * TRAV;
                    float C = Math.Min(C_leftFirst, C_rightFirst);
                    return new VisibilityRollUp(V, C, br.BBox, new Branch<RBVH2Branch, RBVH2Leaf>(left.Tree, right.Tree,
                        new RBVH2Branch()
                        {
                            BBox = br.BBox,
                            Depth = br.Depth,
                            ID = br.ID,
                            PLeft = C_leftFirst < C_rightFirst ? 1f : 0f // go left first if the left cost is lower
                        }));
                },
                le =>
                {
                    return new VisibilityRollUp(le.Primitives.Length > 0 ? 0.0f : 1.0f, PRIM, le.BBox, new Leaf<RBVH2Branch, RBVH2Leaf>(
                        new RBVH2Leaf() {
                            BBox = le.BBox,
                            Depth = le.Depth,
                            ID = le.ID,
                            Primitives = le.Primitives.ToArray()
                        })); 
                }).Tree, tree.NumBranch);
        }

        private class VisibilityRollUp
        {
            public float V;
            public float C;
            public Box3 Box;
            public TreeNode<RBVH2Branch, RBVH2Leaf> Tree;
            public VisibilityRollUp(float v, float c, Box3 box, TreeNode<RBVH2Branch, RBVH2Leaf> tree)
            {
                V = v;
                C = c;
                Box = box;
                Tree = tree;
            }
        }

        public static BackedRBVH2 ApplyRTSAHOrdering(BackedRBVH2 tree)
        {
            return new BackedRBVH2(tree.RollUp(
                (br, left, right) =>
                {
                    IntersectionReport P = UniformRays.GetReport(br.BBox, left.Box, right.Box);

                    float V = P.JustLeft * left.V + P.JustRight * right.V + P.Both * left.V * right.V + P.Neither;
                    float C_leftFirst = TRAV + P.Left * left.C + (P.JustRight + P.Both * left.V) * (TRAV + right.C) + P.Neither * TRAV;
                    float C_rightFirst = TRAV + P.Right * right.C + (P.JustLeft + P.Both * right.V) * (TRAV + left.C) + P.Neither * TRAV;
                    float C = Math.Min(C_leftFirst, C_rightFirst);
                    return new BackedVisibilityRollUp(V, C, br.BBox, new Branch<BackedRBVH2Branch, BackedRBVH2Leaf>(left.Tree, right.Tree,
                        new BackedRBVH2Branch()
                        {
                            BBox = br.BBox,
                            ID = br.ID,
                            PLeft = C_leftFirst < C_rightFirst ? 1f : 0f // go left first if the left cost is lower
                        }));
                },
                le =>
                {
                    return new BackedVisibilityRollUp(le.Primitives.Length > 0 ? 0.0f : 1.0f, PRIM, le.BBox, new Leaf<BackedRBVH2Branch, BackedRBVH2Leaf>(
                        new BackedRBVH2Leaf()
                        {
                            BBox = le.BBox,
                            ID = le.ID,
                            Primitives = le.Primitives.ToArray()
                        }));
                }).Tree, tree.NumBranch);
        }

        private class BackedVisibilityRollUp
        {
            public float V;
            public float C;
            public Box3 Box;
            public TreeNode<BackedRBVH2Branch, BackedRBVH2Leaf> Tree;
            public BackedVisibilityRollUp(float v, float c, Box3 box, TreeNode<BackedRBVH2Branch, BackedRBVH2Leaf> tree)
            {
                V = v;
                C = c;
                Box = box;
                Tree = tree;
            }
        }
    }
}
