using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class RayOrderInspectionCounter : RayOrderOperations
    {
        private int[] _branchInspections;
        private int[] _leafInspections;
        public int[] BranchInspections { get { return _branchInspections; } }
        public int[] LeafInspections { get { return _leafInspections; } } 

        public RayOrderInspectionCounter(int numBranches)
        {
            _branchInspections = new int[numBranches];
            _leafInspections = new int[numBranches+1];
        }

        public void RayCast(RayCast cast)
        {
        }

        public void BoundingBoxTest(BVH2Node node)
        {
        }

        public void BoundingBoxHit(BVH2Node node)
        {
        }

        public void PrimitiveNodeInspection(BVH2Leaf leaf)
        {
            _leafInspections[leaf.ID]++;
        }

        public void PrimitiveNodePrimitiveHit(BVH2Leaf leaf, HitRecord hit)
        {
        }

        public void BranchNodeInspection(BVH2Branch branch)
        {
            _branchInspections[branch.ID]++;
        }

        public void RayHitFound(HitRecord hit)
        {
        }
    }
}
