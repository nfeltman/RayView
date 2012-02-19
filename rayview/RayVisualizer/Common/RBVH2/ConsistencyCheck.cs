using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class ConsistencyCheck : NodeVisitor<Unit, Box3, RBVH2Branch, RBVH2Leaf>
    {
        public static readonly ConsistencyCheck ONLY = new ConsistencyCheck();

        private ConsistencyCheck() { }

        public Unit ForBranch(Branch<RBVH2Branch, RBVH2Leaf> branch, Box3 parent)
        {
            if (!(branch.Content.BBox <= parent))
            {
                throw new Exception(String.Format("Inconsistent RBVH.  Branch {0}({1}) not <= its parent ({2})", branch.Content.ID, branch.Content.BBox, parent));
            }
            branch.Left.Accept(this, branch.Content.BBox);
            branch.Right.Accept(this, branch.Content.BBox);
            return Unit.ONLY;
        }

        public Unit ForLeaf(Leaf<RBVH2Branch, RBVH2Leaf> leaf, Box3 parent)
        {
            Box3 box = leaf.Content.BBox;
            if (!(leaf.Content.BBox <= parent)) throw new Exception(String.Format("Inconsistent RBVH.  Leaf {0}({1}) not <= its parent ({2})", leaf.Content.ID, leaf.Content.BBox, parent));
            foreach (Triangle t in leaf.Content.Primitives)
            {
                if (!box.Contains(t.p1) || !box.Contains(t.p2) || !box.Contains(t.p3)) 
                    throw new Exception(String.Format("Inconsistent RBVH.  Triangle {0} not within its leaf {1}({2})", t, leaf.Content.ID, leaf.Content.BBox));
            }
            return Unit.ONLY;
        }
    }
}
