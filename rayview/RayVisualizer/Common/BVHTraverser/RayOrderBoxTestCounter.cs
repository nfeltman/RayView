using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class RayOrderBoxTestCounter : RayOrderOperations
    {
        private int[] _branchInspections;
        private int[] _leafInspections;
        public int[] BranchInspections { get { return _branchInspections; } }
        public int[] LeafInspections { get { return _leafInspections; } }
        public NodeMap<int> Inspections { get { return new NodeMap<int>(_branchInspections, _leafInspections); } }

        public RayOrderBoxTestCounter(int numBranches)
        {
            _branchInspections = new int[numBranches];
            _leafInspections = new int[numBranches+1];
        }

        public void RayCast(RayQuery cast)
        {
        }

        public void BoundingBoxTest(BVH2Node node)
        {
            node.Accept(b => ++_branchInspections[b.ID], l=> ++_leafInspections[l.ID]);
        }

        public void BoundingBoxHit(BVH2Node node)
        {
        }

        public void PrimitiveNodeInspection(BVH2Leaf leaf)
        {
        }

        public void PrimitiveNodePrimitiveHit(BVH2Leaf leaf, HitRecord hit)
        {
        }

        public void BranchNodeInspection(BVH2Branch branch)
        {
        }

        public void RayHitFound(HitRecord hit)
        {
        }
    }
}
