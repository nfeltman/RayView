using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class OrderedDepthFirstInspectionCounter : OrderedDepthFirstOperations
    {
        private int[] _branchInspections;
        private int[] _leafInspections;
        public int[] BranchInspections { get { return _branchInspections; } }
        public int[] LeafInspections { get { return _leafInspections; } }

        public OrderedDepthFirstInspectionCounter(int numBranches)
        {
            _branchInspections = new int[numBranches];
            _leafInspections = new int[numBranches+1];
        }

        public void RayCast(CVector3 origin, CVector3 direction)
        {
        }

        public void BoundingBoxTest(TreeNode<BVH2Branch, BVH2Leaf> node)
        {
        }

        public void BoundingBoxHit(TreeNode<BVH2Branch, BVH2Leaf> node)
        {
        }

        public void PrimitiveNodeInspection(Leaf<BVH2Branch, BVH2Leaf> leaf)
        {
            _leafInspections[leaf.Content.ID]++;
        }

        public void PrimitiveNodePrimitiveHit(Leaf<BVH2Branch, BVH2Leaf> leaf, HitRecord hit)
        {
        }

        public void BranchNodeInspection(Branch<BVH2Branch, BVH2Leaf> branch)
        {
            _branchInspections[branch.Content.ID]++;
        }

        public void RayHitFound(HitRecord hit)
        {
        }
    }
}
