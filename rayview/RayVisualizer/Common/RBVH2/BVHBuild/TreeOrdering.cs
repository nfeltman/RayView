using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class TreeOrdering
    {
        public static RBVH2 ApplyRTSAHOrdering(RBVH2 tree)
        {
            return new RBVH2(tree.RollUp(
                (br, left, right) =>
                {
                    IntersectionReport rep = UniformRays.GetReport(br.BBox, left.Box, right.Box);

                    float V = rep.JustLeft * left.V + rep.JustRight * right.V + rep.Both * left.V * right.V + rep.Neither;
                    float C_leftFirst = left.C + left.V * right.C;
                    float C_rightFirst = right.C + right.V * left.C;
                    float C = 1f + rep.JustLeft * left.C + rep.JustRight * right.C + Math.Min(C_leftFirst, C_rightFirst);
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
                    return new VisibilityRollUp(le.Primitives.Length > 0 ? 0.0f : 1.0f, 1.0f, le.BBox, new Leaf<RBVH2Branch, RBVH2Leaf>(
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

        
    }
}
