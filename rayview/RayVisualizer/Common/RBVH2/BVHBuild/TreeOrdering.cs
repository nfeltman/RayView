using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class TreeOrdering
    {
        public static void ApplyRTSAHOrdering(RBVH2 tree)
        {
            tree.RollUp(
                (br, left, right) =>
                {
                    float P_lr = PierceBoth(br.BBox, left.Box, right.Box);
                    float P_jl = left.Box.SurfaceArea / br.BBox.SurfaceArea - P_lr;
                    float P_jr = right.Box.SurfaceArea / br.BBox.SurfaceArea - P_lr;
                    float P_e = 1 - P_lr - P_jl - P_jr;
                    float V = P_jl * left.V + P_jr * right.V + P_lr * left.V * right.V + P_e;
                    float C_leftFirst = left.C + left.V * right.C;
                    float C_rightFirst = right.C + right.V * left.C;
                    br.PLeft = C_leftFirst < C_rightFirst ? 1f : 0f; // go left first if the left cost is lower
                    float C = 1f + P_jl * left.C + P_jr * right.C + Math.Min(C_leftFirst, C_rightFirst);
                    return new VisibilityRollUp(V, C, br.BBox); 
                },
                le => 
                {
                    return new VisibilityRollUp(le.Primitives.Length > 0 ? 0.0f : 1.0f, 1.0f, le.BBox); 
                });
        }

        private struct VisibilityRollUp
        {
            public float V;
            public float C;
            public Box3 Box;
            public VisibilityRollUp(float v, float c, Box3 box)
            {
                V = v;
                C = c;
                Box = box;
            }
        }

        private static float PierceBoth(Box3 parent, Box3 left, Box3 right)
        {
            // TODO: Fill this in.  It's nasty.  Maybe look at Thiago's implementation.
            // calculate the proportion of uniform rays intersecting the parent box which intersect both children.
            return 0f;
        }

        // calculate the proportion of radiance from the surface of source that intersects sink
        private static float CalculateFormFactor(Box3 source, Box3 sink)
        {
            return 0f;
        }

        private static float OpposingFormFactor(ClosedInterval source_d1, ClosedInterval source_d2, ClosedInterval sink_d1, ClosedInterval sink_d2)
        {
            return 0f;
        }
    }
}
