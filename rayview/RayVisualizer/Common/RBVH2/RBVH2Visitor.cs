using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public interface RBVH2Visitor<Ret>
    {
        Ret ForBranch(RBVH2Branch branch);
        Ret ForLeaf(RBVH2Leaf leaf);
    }
}
