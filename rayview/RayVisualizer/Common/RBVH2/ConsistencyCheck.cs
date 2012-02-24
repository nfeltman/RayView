using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class ConsistencyCheck<TBranch, TLeaf> : NodeVisitor<Unit, Box3, TBranch, TLeaf>
        where TBranch : Boxed
        where TLeaf : Boxed, Primitived<Triangle>
    {
        public static readonly ConsistencyCheck<TBranch, TLeaf> ONLY = new ConsistencyCheck<TBranch, TLeaf>();

        private ConsistencyCheck() { }

        public Unit ForBranch(Branch<TBranch, TLeaf> branch, Box3 parent)
        {
            if (!(branch.Content.BBox <= parent))
            {
                throw new Exception(String.Format("Inconsistent RBVH.  Branch ({0}) not <= its parent ({1})",branch.Content.BBox, parent));
            }
            branch.Left.Accept(this, branch.Content.BBox);
            branch.Right.Accept(this, branch.Content.BBox);
            return Unit.ONLY;
        }

        public Unit ForLeaf(Leaf<TBranch, TLeaf> leaf, Box3 parent)
        {
            Box3 box = leaf.Content.BBox;
            if (!(leaf.Content.BBox <= parent)) throw new Exception(String.Format("Inconsistent RBVH.  Leaf ({0}) not <= its parent ({1})", leaf.Content.BBox, parent));
            foreach (Triangle t in leaf.Content.Primitives)
            {
                if (!box.Contains(t.p1) || !box.Contains(t.p2) || !box.Contains(t.p3)) 
                    throw new Exception(String.Format("Inconsistent RBVH.  Triangle {0} not within its leaf ({1})", t, leaf.Content.BBox));
            }
            return Unit.ONLY;
        }
    }

    public class ConsistencyCheck<TBranch, TLeaf, Prim> : NodeVisitor<Unit, Box3, TBranch, TLeaf>
        where TBranch : Boxed
        where TLeaf : Boxed, Primitived<Prim>
    {
        private Func<Prim, Triangle> _map;

        public ConsistencyCheck(Func<Prim, Triangle> map) { _map = map; }

        public Unit ForBranch(Branch<TBranch, TLeaf> branch, Box3 parent)
        {
            if (!(branch.Content.BBox <= parent))
            {
                throw new Exception(String.Format("Inconsistent RBVH.  Branch ({0}) not <= its parent ({1})", branch.Content.BBox, parent));
            }
            branch.Left.Accept(this, branch.Content.BBox);
            branch.Right.Accept(this, branch.Content.BBox);
            return Unit.ONLY;
        }

        public Unit ForLeaf(Leaf<TBranch, TLeaf> leaf, Box3 parent)
        {
            Box3 box = leaf.Content.BBox;
            if (!(leaf.Content.BBox <= parent)) throw new Exception(String.Format("Inconsistent RBVH.  Leaf ({0}) not <= its parent ({1})", leaf.Content.BBox, parent));
            foreach (Prim pr in leaf.Content.Primitives)
            {
                Triangle t = _map(pr);
                if (!box.Contains(t.p1) || !box.Contains(t.p2) || !box.Contains(t.p3))
                    throw new Exception(String.Format("Inconsistent RBVH.  Triangle {0} not within its leaf ({1})", t, leaf.Content.BBox));
            }
            return Unit.ONLY;
        }
    }
}
