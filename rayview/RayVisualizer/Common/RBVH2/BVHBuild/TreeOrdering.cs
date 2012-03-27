using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class TreeOrdering
    {
        private const float TRAV = 1.0f;
        private const float PRIM = 3.0f;

        public static void ApplyRTSAHOrdering<TBranch, TLeaf>(Tree<TBranch, TLeaf> tree)
            where TBranch : Weighted, Boxed
            where TLeaf : Boxed, PrimCountable
        {
            tree.RollUpNodes(
                (br, left, right) =>
                {
                    IntersectionReport P = UniformRays.GetReport(br.Content.BBox, left.Box, right.Box);

                    double V = P.JustLeft * left.V + P.JustRight * right.V + P.Both * left.V * right.V + P.Neither;
                    double C_leftFirst = TRAV + P.Left * left.C + (P.JustRight + P.Both * left.V) * (TRAV + right.C) + P.Neither * TRAV;
                    double C_rightFirst = TRAV + P.Right * right.C + (P.JustLeft + P.Both * right.V) * (TRAV + left.C) + P.Neither * TRAV;
                    double C = Math.Min(C_leftFirst, C_rightFirst);
                    // since br.content can be a value type, I have to play some stupid games here to get the copy to carry through
                    br.Content.Kernel = C_leftFirst < C_rightFirst ? TraversalKernel.LeftFirst : TraversalKernel.RightFirst; // go left first if the left cost is lower
                    return new VisibilityRollUp(V, C, br.Content.BBox);
                },
                le =>
                {
                    return new VisibilityRollUp(le.Content.PrimCount > 0 ? 0.0f : 1.0f, TRAV + PRIM * le.Content.PrimCount, le.Content.BBox);
                });
        }

        private class VisibilityRollUp
        {
            public double V;
            public double C;
            public Box3 Box;
            public VisibilityRollUp(double v, double c, Box3 box)
            {
                V = v;
                C = c;
                Box = box;
            }
        }
    }
}
