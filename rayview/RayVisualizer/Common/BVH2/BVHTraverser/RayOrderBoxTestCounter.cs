﻿using System;
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

        public void RayCast(CVector3 origin, CVector3 direction)
        {
        }

        public void BoundingBoxTest(TreeNode<BVH2Branch, BVH2Leaf> node)
        {
            node.Accept(b => ++_branchInspections[b.Content.ID], l=> ++_leafInspections[l.Content.ID]);
        }

        public void BoundingBoxHit(TreeNode<BVH2Branch, BVH2Leaf> node)
        {
        }

        public void PrimitiveNodeInspection(Leaf<BVH2Branch, BVH2Leaf> leaf)
        {
        }

        public void PrimitiveNodePrimitiveHit(Leaf<BVH2Branch, BVH2Leaf> leaf, HitRecord hit)
        {
        }

        public void BranchNodeInspection(Branch<BVH2Branch, BVH2Leaf> branch)
        {
        }

        public void RayHitFound(HitRecord hit)
        {
        }
    }
}
