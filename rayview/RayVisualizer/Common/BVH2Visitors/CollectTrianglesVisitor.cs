using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common.BVH2Visitors
{
    public class CollectTrianglesVisitor : BVH2Visitor<byte>
    {
        private Action<Triangle> _triangleAction;

        public CollectTrianglesVisitor(Action<Triangle> triangleAction)
        {
            _triangleAction = triangleAction;
        }

        public byte ForBranch(BVH2Branch branch)
        {
            branch.Left.Accept(this);
            branch.Right.Accept(this);
            return 0;
        }

        public byte ForLeaf(BVH2Leaf leaf)
        {
            foreach (Triangle t in leaf.Primitives)
                _triangleAction(t);
            return 0;
        }
    }
}
