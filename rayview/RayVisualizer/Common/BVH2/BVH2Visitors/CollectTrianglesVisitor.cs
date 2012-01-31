using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common.BVH2Visitors
{
    public class CollectTrianglesVisitor : NodeVisitor<Unit, BVH2Branch, BVH2Leaf>, NodeVisitor<Unit,RBVH2Branch, RBVH2Leaf>
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

    public class CollectTrianglesAndHotnessVisitor : NodeVisitor<Unit, RBVH2Branch, RBVH2Leaf>
    {
        private Action<float, Triangle> _triangleAction;
        private float rarity;

        public CollectTrianglesAndHotnessVisitor(Action<float, Triangle> triangleAction)
        {
            rarity = 1;
            _triangleAction = triangleAction;
        }

        public Unit ForBranch(Branch<RBVH2Branch, RBVH2Leaf> branch)
        {
            float save = rarity;
            rarity = save * (save * branch.Content.PLeft + 1) / 2f;
            branch.Left.Accept((NodeVisitor<Unit, RBVH2Branch, RBVH2Leaf>)this);
            rarity = save * (2 - save * branch.Content.PLeft) / 2f;
            branch.Right.Accept((NodeVisitor<Unit, RBVH2Branch, RBVH2Leaf>)this);
            return Unit.ONLY;
        }

        public Unit ForLeaf(Leaf<RBVH2Branch, RBVH2Leaf> leaf)
        {
            foreach (Triangle t in leaf.Content.Primitives)
                _triangleAction(rarity,t);
            return Unit.ONLY;
        }
    }
}
