using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common.BVH2Visitors
{
    // if we've made it into the visitor, the ray intersects with the bounding box
    class OrderedDepthFirstOpCounter : BVH2Visitor<HitRecord>
    {
        private OrderedDepthFirstOperations _ops;
        private RayCast _ray;
        private ClosedInterval _c;

        private OrderedDepthFirstOpCounter(OrderedDepthFirstOperations ops, RayCast ray)
        {
            _ops = ops;
            _ray = ray;
            _c = ClosedInterval.POSITIVES;
        }

        public HitRecord ForBranch(BVH2Branch branch)
        {
            _ops.boundingBoxTests += 2;
            _ops.branchNodeInspections++;
            ClosedInterval leftInterval = branch.Left.BBox.IntersectRay(_ray.Origin, _ray.Direction) & _c;
            ClosedInterval rightInterval = branch.Right.BBox.IntersectRay(_ray.Origin, _ray.Direction) & _c;

            if (leftInterval.IsEmpty && rightInterval.IsEmpty)
            {
                return null;
            }
            else if (leftInterval.IsEmpty && !rightInterval.IsEmpty)
            {
                _ops.boundingBoxHits++;
                return branch.Left.Accept(this);
            }
            else if (!leftInterval.IsEmpty && rightInterval.IsEmpty)
            {
                _ops.boundingBoxHits++;
                return branch.Right.Accept(this);
            }
            else
            {
                _ops.boundingBoxHits += 2;
                if (leftInterval.Min <= rightInterval.Min)
                {
                    HitRecord leftRes = branch.Left.Accept(this);
                    if (rightInterval > _c)
                    {
                        return leftRes;
                    }
                    else
                    {
                        HitRecord rightRes = branch.Right.Accept(this);
                        if (rightRes == null) return leftRes;
                        return leftRes==null || rightRes.t_value < leftRes.t_value ? rightRes : leftRes;
                    }
                }
                else
                {
                    HitRecord rightRes = branch.Right.Accept(this);
                    if (leftInterval > _c)
                    {
                        return rightRes;
                    }
                    else
                    {
                        HitRecord leftRes = branch.Right.Accept(this);
                        if (leftRes == null) return rightRes;
                        return rightRes == null || leftRes.t_value < rightRes.t_value ? leftRes : rightRes;
                    }
                }
            }
        }

        public HitRecord ForLeaf(BVH2Leaf leaf)
        {
            _ops.primitiveNodeInspections++;
            Tuple<float,int> res = leaf.FindClosestPositiveIntersection(_ray.Origin, _ray.Direction);
            if (_c.Contains(res.Item1))
                return new HitRecord(leaf.Primitives[res.Item2], res.Item1);
            return null;
        }

        public static HitRecord RunOpCounter(BVH2 bvh, RayCast ray, OrderedDepthFirstOperations ops)
        {
            if (!(ray.Kind == RayKind.FirstHit_Hit || ray.Kind == RayKind.FirstHit_Miss))
                throw new Exception("Only for first-hit rays! Not any-hit!");
            ops.rayCasts++;
            ops.boundingBoxTests++;
            ClosedInterval interval = bvh.Root.BBox.IntersectRay(ray.Origin, ray.Direction);
            if (interval.IsEmpty) return null;
            ops.boundingBoxHits++;
            HitRecord result = bvh.Accept(new OrderedDepthFirstOpCounter(ops, ray));
            if (result != null) ops.rayHitFound++;
            return result;
        }
    }

    public class OrderedDepthFirstOperations
    {
        public int rayCasts;
        public int boundingBoxTests;
        public int boundingBoxHits;
        public int primitiveNodeInspections;
        public int primitiveNodePrimitiveHits;
        public int branchNodeInspections;
        public int rayHitFound;
    }
}
