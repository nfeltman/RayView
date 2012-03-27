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
        public static float GetProbLeftFirst(this TraversalKernel kernel, Vector4f origin, Vector4f difference)
        {
            switch (kernel)
            {
                case TraversalKernel.LeftFirst:
                    return 1f;
                case TraversalKernel.RightFirst:
                    return 0f;
                case TraversalKernel.UniformRandom:
                    return 0.5f;
                default:
                    throw new Exception("unsupported kernel!");
            }
        }
    }
}
