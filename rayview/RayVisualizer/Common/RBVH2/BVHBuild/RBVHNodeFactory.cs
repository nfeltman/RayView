using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class RBVHNodeFactory : NodeFactory<RBVH2Branch, RBVH2Leaf, RBVH2, float, BoundAndCount>
    {
        public static readonly RBVHNodeFactory ONLY = new RBVHNodeFactory();

        private RBVHNodeFactory() { }

        public RBVH2Branch BuildBranch(TreeNode<RBVH2Branch, RBVH2Leaf> left, TreeNode<RBVH2Branch, RBVH2Leaf> right, float pLeft, int branchCounter, int depth, BoundAndCount boundingBox)
        {
            return new RBVH2Branch() { ID = branchCounter, BBox = boundingBox.Box, Depth = depth, PLeft = pLeft };
        }

        public RBVH2Leaf BuildLeaf(BuildTriangle[] tris, int start, int end, int leafCounter, int depth, BoundAndCount boundingBox)
        {
            Triangle[] prims = new Triangle[end-start];
            for (int k = 0; k < prims.Length; k++)
                prims[k] = tris[k + start].t;
            return new RBVH2Leaf() { Primitives = prims, ID = leafCounter, Depth = depth, BBox = boundingBox.Box };
        }

        public RBVH2 BuildTree(TreeNode<RBVH2Branch, RBVH2Leaf> root, int numBranches)
        {
            return new RBVH2(root, numBranches);
        }
    }
}
