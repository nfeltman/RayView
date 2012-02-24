using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    using BVH2 = Tree<BVH2Branch, BVH2Leaf>;
    using RBVH2 = Tree<RBVH2Branch, RBVH2Leaf>;
    using BackedBVH2 = Tree<BackedBVH2Branch, BackedBVH2Leaf>;
    using BackedRBVH2 = Tree<BackedRBVH2Branch, BackedRBVH2Leaf>;

    public class BVHNodeFactory : NodeFactory<TriangleContainer, BVH2Branch, BVH2Leaf, BVH2, Unit, BoundAndCount>
    {
        public static readonly BVHNodeFactory ONLY = new BVHNodeFactory();

        private BVHNodeFactory() { }

        public BVH2Branch BuildBranch(TreeNode<BVH2Branch, BVH2Leaf> left, TreeNode<BVH2Branch, BVH2Leaf> right, Unit branchData, int branchCounter, int depth, BoundAndCount boundingBox)
        {
            return new BVH2Branch() {ID = branchCounter, BBox = boundingBox.Box, Depth = depth };
        }

        public BVH2Leaf BuildLeaf<Tri>(Tri[] tris, int start, int end, int leafCounter, int depth, BoundAndCount boundingBox)
            where Tri : TriangleContainer
        {
            Triangle[] prims = new Triangle[end - start];
            for (int k = 0; k < prims.Length; k++)
                prims[k] = tris[k + start].T;
            return new BVH2Leaf() { Primitives = prims, ID = leafCounter, Depth = depth, BBox = boundingBox.Box };
        }

        public BVH2 BuildTree(TreeNode<BVH2Branch, BVH2Leaf> root, int numBranches)
        {
            return new BVH2(root, numBranches);
        }
    }

    public class BackedBVHNodeFactory : NodeFactory<OBJBackedBuildTriangle, BackedBVH2Branch, BackedBVH2Leaf, BackedBVH2, Unit, BoundAndCount>
    {
        public static readonly BackedBVHNodeFactory ONLY = new BackedBVHNodeFactory();

        private BackedBVHNodeFactory() { }

        public BackedBVH2Branch BuildBranch(TreeNode<BackedBVH2Branch, BackedBVH2Leaf> left, TreeNode<BackedBVH2Branch, BackedBVH2Leaf> right, Unit buildData, int branchCounter, int depth, BoundAndCount boundingBox)
        {
            return new BackedBVH2Branch() { ID = branchCounter, BBox = boundingBox.Box };
        }

        public BackedBVH2Leaf BuildLeaf<Tri>(Tri[] tris, int start, int end, int leafCounter, int depth, BoundAndCount boundingBox)
            where Tri : OBJBackedBuildTriangle
        {
            int[] prims = new int[end - start];
            for (int k = 0; k < prims.Length; k++)
                prims[k] = tris[k + start].OBJIndex;
            return new BackedBVH2Leaf() { Primitives = prims, ID = leafCounter, BBox = boundingBox.Box };
        }

        public BackedBVH2 BuildTree(TreeNode<BackedBVH2Branch, BackedBVH2Leaf> root, int numBranches)
        {
            return new BackedBVH2(root, numBranches);
        }
    }
}
