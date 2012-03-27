using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common.BVH2Visitors
{
    public class CollectTrianglesVisitor<Tri> : NodeVisitor<Unit, BVH2Branch, BVH2Leaf>, NodeVisitor<Unit, RBVH2Branch, RBVH2Leaf>
    {
        private Action<Triangle> _triangleAction;

        public CollectTrianglesVisitor(Action<Triangle> triangleAction)
        {
            _triangleAction = triangleAction;
        }

        public Unit ForBranch(Branch<BVH2Branch, BVH2Leaf> branch)
        {
            branch.Left.Accept((NodeVisitor<Unit, BVH2Branch, BVH2Leaf>)this);
            branch.Right.Accept((NodeVisitor<Unit, BVH2Branch, BVH2Leaf>)this);
            return Unit.ONLY;
        }

        public Unit ForLeaf(Leaf<BVH2Branch, BVH2Leaf> leaf)
        {
            foreach (Triangle t in leaf.Content.Primitives)
                _triangleAction(t);
            return Unit.ONLY;
        }


        public Unit ForBranch(Branch<RBVH2Branch, RBVH2Leaf> branch)
        {
            branch.Left.Accept((NodeVisitor<Unit,RBVH2Branch, RBVH2Leaf>)this);
            branch.Right.Accept((NodeVisitor<Unit,RBVH2Branch, RBVH2Leaf>)this);
            return Unit.ONLY;
        }

        public Unit ForLeaf(Leaf<RBVH2Branch, RBVH2Leaf> leaf)
        {
            foreach (Triangle t in leaf.Content.Primitives)
                _triangleAction(t);
            return Unit.ONLY;
        }
    }
}
