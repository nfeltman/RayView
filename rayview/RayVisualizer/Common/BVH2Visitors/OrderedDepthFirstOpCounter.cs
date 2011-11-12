using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    // if we've made it into the visitor, the ray intersects with the bounding box
    public class OrderedDepthFirstOpCounter
    {

        private class ODFVisitor : BVH2Visitor<HitRecord>
        {
            private OrderedDepthFirstOperations _ops;
            private RayCast _ray;
            private ClosedInterval _c;

            public ODFVisitor(OrderedDepthFirstOperations ops, RayCast ray)
            {
                _ops = ops;
                _ray = ray;
                _c = ClosedInterval.POSITIVES;
            }

            public HitRecord ForBranch(BVH2Branch branch)
            {
                _ops.branchNodeInspections++;
                _ops.boundingBoxTests += 2;

                ClosedInterval leftInterval = branch.Left.BBox.IntersectInterval(_ray.Origin, _ray.Direction, _c);
                ClosedInterval rightInterval = branch.Right.BBox.IntersectInterval(_ray.Origin, _ray.Direction, _c);

                if (leftInterval.IsEmpty && rightInterval.IsEmpty)
                {
                    return null;
                }
                else if (!rightInterval.IsEmpty && leftInterval.IsEmpty)
                {
                    _ops.boundingBoxHits++;
                    return branch.Right.Accept(this);
                }
                else if (!leftInterval.IsEmpty && rightInterval.IsEmpty)
                {
                    _ops.boundingBoxHits++;
                    return branch.Left.Accept(this);
                }
                else
                {
                    _ops.boundingBoxHits += 2;
                    if (leftInterval.Min < rightInterval.Min)
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
                            return rightRes;
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
                            HitRecord leftRes = branch.Left.Accept(this);
                            if (leftRes == null) return rightRes;
                            return leftRes;
                        }
                    }
                }
            }

            public HitRecord ForLeaf(BVH2Leaf leaf)
            {
                _ops.primitiveNodeInspections++;
                HitRecord res = leaf.FindClosestPositiveIntersection(_ray.Origin, _ray.Direction, _c);
                if (res != null)
                {
                    _ops.primitiveNodePrimitiveHits++;
                    //_c = new ClosedInterval(_c.Min, res.t_value);
                    return res;
                }
                return null;
            }
        }

        public static HitRecord RunTraverser(BVH2 bvh, RayCast ray, OrderedDepthFirstOperations ops)
        {
            if (!(ray.Kind == RayKind.FirstHit_Hit || ray.Kind == RayKind.FirstHit_Miss))
                throw new Exception("Only for first-hit rays! Not any-hit!");
            
            ops.rayCasts++;

            //root bbox test
            ops.boundingBoxTests++;
            ClosedInterval interval = bvh.Root.BBox.IntersectRay(ray.Origin, ray.Direction);
            if (interval.IsEmpty) return null;
            ops.boundingBoxHits++;
            
            //descend
            HitRecord result = bvh.Accept(new ODFVisitor(ops, ray));
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
