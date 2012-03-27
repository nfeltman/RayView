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

    public class RBVH5050Factory<T> : NodeFactory<T, RBVH2Branch, RBVH2Leaf, RBVH2, Unit, BoundAndCount>
        where T : TriangleContainer
    {
        public static readonly RBVH5050Factory<T> ONLY = new RBVH5050Factory<T>();

        private RBVH5050Factory() { }

        public RBVH2Branch BuildBranch(TreeNode<RBVH2Branch, RBVH2Leaf> left, TreeNode<RBVH2Branch, RBVH2Leaf> right, Unit unit, int branchCounter, int depth, BoundAndCount boundingBox)
        {
            return new RBVH2Branch() { ID = branchCounter, BBox = boundingBox.Box, Depth = depth, Kernel = TraversalKernel.UniformRandom };
        }

        public RBVH2Leaf BuildLeaf<Tri>(Tri[] tris, int start, int end, int leafCounter, int depth, BoundAndCount boundingBox)
            where Tri : T
        {
            Triangle[] prims = new Triangle[end-start];
            for (int k = 0; k < prims.Length; k++)
                prims[k] = tris[k + start].T;
            return new RBVH2Leaf() { Primitives = prims, ID = leafCounter, Depth = depth, BBox = boundingBox.Box };
        }

        public RBVH2 BuildTree(TreeNode<RBVH2Branch, RBVH2Leaf> root, int numBranches)
        {
            return new RBVH2(root, numBranches);
        }
    }

    public class BackedRBVH5050Factory<T> : NodeFactory<T, BackedRBVH2Branch, BackedRBVH2Leaf, BackedRBVH2, Unit, BoundAndCount>
        where T : OBJBacked
    {
        public static readonly BackedRBVH5050Factory<T> ONLY = new BackedRBVH5050Factory<T>();

        private BackedRBVH5050Factory() { }

        public BackedRBVH2Branch BuildBranch(TreeNode<BackedRBVH2Branch, BackedRBVH2Leaf> left, TreeNode<BackedRBVH2Branch, BackedRBVH2Leaf> right, Unit unit, int branchCounter, int depth, BoundAndCount boundingBox)
        {
            return new BackedRBVH2Branch() { ID = branchCounter, BBox = boundingBox.Box, Kernel = TraversalKernel.UniformRandom };
        }

        public BackedRBVH2Leaf BuildLeaf<Tri>(Tri[] tris, int start, int end, int leafCounter, int depth, BoundAndCount boundingBox)
            where Tri : T
        {
            int[] prims = new int[end - start];
            for (int k = 0; k < prims.Length; k++)
                prims[k] = tris[k + start].OBJIndex;
            return new BackedRBVH2Leaf() { Primitives = prims, ID = leafCounter, BBox = boundingBox.Box };
        }

        public BackedRBVH2 BuildTree(TreeNode<BackedRBVH2Branch, BackedRBVH2Leaf> root, int numBranches)
        {
            return new BackedRBVH2(root, numBranches);
        }
    }
}
