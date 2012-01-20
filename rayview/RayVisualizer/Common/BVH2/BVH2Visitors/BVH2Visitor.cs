using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface BVH2Visitor<Ret>
    {
        Ret ForBranch(BVH2Branch branch);
        Ret ForLeaf(BVH2Leaf leaf);
    }
}
