using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class BVH2
    {
        private BVH2Node root;

        public Ret Accept<Ret>(BVH2Visitor<Ret> visitor)
        {
            return root.Accept(visitor);
        }
    }

    interface BVH2Node
    {
        Box3 BBox { get; set; }
        Ret Accept<Ret>(BVH2Visitor<Ret> visitor);
    }

    class BVH2Branch : BVH2Node
    {
        public BVH2Node Left { get; set; }
        public BVH2Node Right { get; set; }
        public Box3 BBox { get; set; }

        public Ret Accept<Ret>(BVH2Visitor<Ret> visitor)
        {
            return visitor.ForBranch(this);
        }
    }

    class BVH2Leaf : BVH2Node
    {
        public Box3 BBox { get; set; }

        public Ret Accept<Ret>(BVH2Visitor<Ret> visitor)
        {
            return visitor.ForLeaf(this);
        }
    }
}
