﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    // if we've made it into the visitor, the ray intersects with the bounding box
    public class OrderedDepthFirstTraverser
    {
        private class ODFVisitor : NodeVisitor<HitRecord,BVH2Branch,BVH2Leaf>
        {
            private OrderedDepthFirstOperations _ops;
            private CVector3 origin;
            private CVector3 direction;
            private ClosedInterval _c;

            public ODFVisitor(OrderedDepthFirstOperations ops, CVector3 origin, CVector3 direction)
            {
                this.origin = origin;
                this.direction = direction;
                _ops = ops;
                _c = ClosedInterval.POSITIVES;
            }

            public HitRecord ForBranch(Branch<BVH2Branch,BVH2Leaf> branch)
            {
                _ops.BranchNodeInspection(branch);
                _ops.BoundingBoxTest(branch.Left);
                _ops.BoundingBoxTest(branch.Right);

                ClosedInterval leftInterval = branch.Left.BBox().IntersectInterval(origin, direction, _c);
                ClosedInterval rightInterval = branch.Right.BBox().IntersectInterval(origin, direction, _c);

                if (leftInterval.IsEmpty && rightInterval.IsEmpty)
                {
                    return null;
                }
                else if (!rightInterval.IsEmpty && leftInterval.IsEmpty)
                {
                    _ops.BoundingBoxHit(branch.Right);
                    return branch.Right.Accept(this);
                }
                else if (!leftInterval.IsEmpty && rightInterval.IsEmpty)
                {
                    _ops.BoundingBoxHit(branch.Left);
                    return branch.Left.Accept(this);
                }
                else
                {
                    _ops.BoundingBoxHit(branch.Left);
                    _ops.BoundingBoxHit(branch.Right);
                    if (leftInterval.Min < rightInterval.Min)
                    {
                        HitRecord leftRes = branch.Left.Accept(this);
                        if (rightInterval.EntirelyGreater(_c))
                        {
                            return leftRes;
                        }
                        else
                        {
                            HitRecord rightRes = branch.Right.Accept(this);
                            if (rightRes == null) return leftRes;
                            return rightRes;
                        }
                    }
                    else
                    {
                        HitRecord rightRes = branch.Right.Accept(this);
                        if (leftInterval.EntirelyGreater(_c))
                        {
                            return rightRes;
                        }
                        else
                        {
                            HitRecord leftRes = branch.Left.Accept(this);
                            if (leftRes == null) return rightRes;
                            return leftRes;
                        }
                    }
                }
            }

            public HitRecord ForLeaf(Leaf<BVH2Branch, BVH2Leaf> leaf)
            {
                _ops.PrimitiveNodeInspection(leaf);
                HitRecord res = leaf.Content.FindClosestPositiveIntersection(origin, direction, _c);
                if (res != null)
                {
                    _ops.PrimitiveNodePrimitiveHit(leaf, res);
                    //_c = new ClosedInterval(_c.Min, res.t_value);
                    return res;
                }
                return null;
            }
        }

        public static HitRecord RunTooledTraverser(BVH2 bvh, CVector3 origin, CVector3 direction, OrderedDepthFirstOperations ops)
        {
            ops.RayCast(origin, direction);

            //root bbox test
            ops.BoundingBoxTest(bvh.Root);
            ClosedInterval interval = bvh.Root.BBox().IntersectRay(origin, direction);
            if (interval.IsEmpty) return null;
            ops.BoundingBoxHit(bvh.Root);
            
            //descend
            HitRecord result = bvh.Accept(new ODFVisitor(ops, origin, direction));
            if (result != null) ops.RayHitFound(result);
            return result;
        }
    }

    public interface OrderedDepthFirstOperations
    {
        void RayCast(CVector3 origin, CVector3 direction);
        void BoundingBoxTest(TreeNode<BVH2Branch, BVH2Leaf> node);
        void BoundingBoxHit(TreeNode<BVH2Branch, BVH2Leaf> node);
        void PrimitiveNodeInspection(Leaf<BVH2Branch, BVH2Leaf> leaf);
        void PrimitiveNodePrimitiveHit(Leaf<BVH2Branch, BVH2Leaf> leaf, HitRecord hit);
        void BranchNodeInspection(Branch<BVH2Branch, BVH2Leaf> branch);
        void RayHitFound(HitRecord hit);
    }
    public class NullOrderedDepthFirstOperations : OrderedDepthFirstOperations
    {
        public void RayCast(CVector3 origin, CVector3 direction) { }
        public void BoundingBoxTest(TreeNode<BVH2Branch, BVH2Leaf> node) { }
        public void BoundingBoxHit(TreeNode<BVH2Branch, BVH2Leaf> node) { }
        public void PrimitiveNodeInspection(Leaf<BVH2Branch, BVH2Leaf> leaf) { }
        public void PrimitiveNodePrimitiveHit(Leaf<BVH2Branch, BVH2Leaf> leaf, HitRecord hit) { }
        public void BranchNodeInspection(Branch<BVH2Branch, BVH2Leaf> branch) { }
        public void RayHitFound(HitRecord hit) { }
    }
}
