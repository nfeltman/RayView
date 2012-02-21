using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class BVHNodeFactory : NodeFactory<BVH2Branch, BVH2Leaf, BVH2, Unit, BoundAndCount>
    {
        public static readonly BVHNodeFactory ONLY = new BVHNodeFactory();

        private BVHNodeFactory() { }

        public BVH2Branch BuildBranch(TreeNode<BVH2Branch, BVH2Leaf> left, TreeNode<BVH2Branch, BVH2Leaf> right, Unit branchData, int branchCounter, int depth, BoundAndCount boundingBox)
        {
            return new BVH2Branch() {ID = branchCounter, BBox = boundingBox.Box, Depth = depth };
        }

        public BVH2Leaf BuildLeaf(BuildTriangle[] tris, int start, int end, int leafCounter, int depth, BoundAndCount boundingBox)
        {
            Triangle[] prims = new Triangle[end - start];
            for (int k = 0; k < prims.Length; k++)
                prims[k] = tris[k + start].t;
            return new BVH2Leaf() { Primitives = prims, ID = leafCounter, Depth = depth, BBox = boundingBox.Box };
        }

        public BVH2 BuildTree(TreeNode<BVH2Branch, BVH2Leaf> root, int numBranches)
        {
            return new BVH2(root, numBranches);
        }
    }
}
