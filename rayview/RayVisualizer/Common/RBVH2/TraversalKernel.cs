using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Simd;

namespace RayVisualizer.Common
{
    public enum TraversalKernel
    {
        LeftFirst = 11,
        RightFirst = 12,
        UniformRandom = 13,
        FrontToBack = 14,
        BackToFront = 15
    }

    public static class Kernels
    {

        public static bool LeftIsCloser(Vector4f leftCenter, Vector4f rightCenter, Vector4f origin)
        {
            leftCenter = leftCenter - origin;
            leftCenter = leftCenter * leftCenter;
            rightCenter = rightCenter - origin;
            rightCenter = rightCenter * rightCenter;

            return leftCenter.X + leftCenter.Y + leftCenter.Z < rightCenter.X + rightCenter.Y + rightCenter.Z;
        }
    }
}
