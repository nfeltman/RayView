using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace RayVisualizer.Common
{
    // alias, because Union<...,...> is too long
    using QueueItem = Union<BVH2Node, HitRecord>;
    
    public class RayOrderTraverser 
    {
        public static HitRecord RunTraverser(BVH2 bvh, RayCast ray, RayOrderOperations ops)
        {

            if (!(ray.Kind == RayKind.FirstHit_Hit || ray.Kind == RayKind.FirstHit_Miss))
                throw new Exception("Only for first-hit rays! Not any-hit!");

            PriorityQueue<float, QueueItem> q = new PriorityQueue<float, QueueItem>();

            CVector3 origin = ray.Origin;
            CVector3 direction = ray.Direction;//.Normalized();

            ops.RayCast(ray);
            ops.BoundingBoxTest(bvh.Root);

            ClosedInterval rootInterval = bvh.Root.BBox.IntersectRay(origin, direction);
            if (!rootInterval.IsEmpty)
            {
                ops.BoundingBoxHit(bvh.Root);
                q.Enqueue(rootInterval.Min, new QueueItem(bvh.Root));
            }

            while(!q.IsEmpty)
            {
                KeyValuePair<float, QueueItem> pair = q.Dequeue();
                HitRecord intersection = pair.Value.Run(n => n.Accept(
                    // pattern match based on the type of queue item
                    (BVH2Branch branch) =>
                    {
                        ops.BranchNodeInspection(branch);

                        ClosedInterval leftIntersection = branch.Left.BBox.IntersectRay(origin, direction);
                        ClosedInterval rightIntersection = branch.Right.BBox.IntersectRay(origin, direction);
                        ops.BoundingBoxTest(branch.Left);
                        ops.BoundingBoxTest(branch.Right);

                        if (!leftIntersection.IsEmpty)
                        {
                            ops.BoundingBoxHit(branch.Left);
                            q.Enqueue(leftIntersection.Min, new QueueItem(branch.Left));
                        }
                        if (!rightIntersection.IsEmpty)
                        {
                            ops.BoundingBoxHit(branch.Right);
                            q.Enqueue(rightIntersection.Min, new QueueItem(branch.Right));
                        }
                        return (HitRecord)null; //do not stop queue processing
                    },
                    (BVH2Leaf leaf) =>
                    {
                        ops.PrimitiveNodeInspection(leaf);
                        HitRecord closestIntersection = leaf.FindClosestPositiveIntersection(origin, direction, ClosedInterval.POSITIVES);
                        if (closestIntersection != null)
                        {
                            ops.PrimitiveNodePrimitiveHit(leaf,closestIntersection);
                            q.Enqueue(closestIntersection.t_value, new QueueItem(closestIntersection));
                        }
                        return (HitRecord)null; //do not stop queue processing
                    }),
                    (HitRecord hitRecord) =>
                    {
                        ops.RayHitFound(hitRecord);
                        return hitRecord; //we've found outr hit; stop queue processing
                    });
                if (intersection != null) return intersection;
            }

            return null; // no hit was found!
        }
    }

    public class HitRecord
    {
        public Triangle triangle;
        public float t_value;
        public int leafID;

        public HitRecord(Triangle tri, float t, int leafIDNum) { triangle = tri; t_value = t; leafID = leafIDNum; }
    }

    public interface RayOrderOperations
    {
        void RayCast(RayCast cast);
        void BoundingBoxTest(BVH2Node node);
        void BoundingBoxHit(BVH2Node node);
        void PrimitiveNodeInspection(BVH2Leaf leaf);
        void PrimitiveNodePrimitiveHit(BVH2Leaf leaf, HitRecord hit);
        void BranchNodeInspection(BVH2Branch branch);
        void RayHitFound(HitRecord hit);
    }
}
