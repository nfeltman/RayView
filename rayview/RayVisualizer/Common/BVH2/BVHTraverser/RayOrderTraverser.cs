using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace RayVisualizer.Common
{
    using BVH2 = Tree<BVH2Branch, BVH2Leaf>;
    using RBVH2 = Tree<RBVH2Branch, RBVH2Leaf>;
    using BackedBVH2 = Tree<BackedBVH2Branch, BackedBVH2Leaf>;
    using BackedRBVH2 = Tree<BackedRBVH2Branch, BackedRBVH2Leaf>;

    // alias, because Union<...,...> is too long
    using QueueItem = Union<TreeNode<BVH2Branch,BVH2Leaf>, HitRecord>;
    
    public class RayOrderTraverser 
    {
        public static void RunTooledTraverser(BVH2 bvh, RaySet set, RayOrderOperations ops)
        {
            foreach (CastHitQuery hit in set.CastHitQueries)
            {
                RunTooledTraverser(bvh,hit.Origin,hit.Difference,ops);
            }
            foreach (CastMissQuery hit in set.CastMissQueries)
            {
                RunTooledTraverser(bvh, hit.Origin, hit.Direction, ops);
            }
        }
        public static HitRecord RunTooledTraverser(BVH2 bvh, CVector3 origin, CVector3 direction, RayOrderOperations ops)
        {
            PriorityQueue<float, QueueItem> q = new PriorityQueue<float, QueueItem>();

            ops.RayCast(origin,direction);
            ops.BoundingBoxTest(bvh.Root);

            ClosedInterval rootInterval = bvh.Root.BBox().IntersectRay(origin, direction);
            if (!rootInterval.IsEmpty)
            {
                ops.BoundingBoxHit(bvh.Root);
                q.Enqueue(rootInterval.Min, new QueueItem(bvh.Root));
            }

            //float t_found = float.PositiveInfinity; //use this to skip past nodes when a 
            while(!q.IsEmpty)
            {
                KeyValuePair<float, QueueItem> pair = q.Dequeue();
                //if (pair.Key > t_found) throw new Exception("Somehow we skipped past the found intersection.");
                HitRecord intersection = pair.Value.Run(n => /*pair.Key == t_found ? null :*/ n.Accept(
                    // pattern match based on the type of queue item
                    (Branch<BVH2Branch,BVH2Leaf> branch) =>
                    {
                        ops.BranchNodeInspection(branch);

                        ClosedInterval leftIntersection = branch.Left.BBox().IntersectRay(origin, direction);
                        ClosedInterval rightIntersection = branch.Right.BBox().IntersectRay(origin, direction);
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
                    (Leaf<BVH2Branch, BVH2Leaf> leaf) =>
                    {
                        ops.PrimitiveNodeInspection(leaf);
                        HitRecord closestIntersection = leaf.Content.FindClosestPositiveIntersection(origin, direction, ClosedInterval.POSITIVES);
                        if (closestIntersection != null)
                        {
                            ops.PrimitiveNodePrimitiveHit(leaf, closestIntersection);
                            //t_found = closestIntersection.t_value;
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
        void RayCast(CVector3 origin, CVector3 direction);
        void BoundingBoxTest(TreeNode<BVH2Branch, BVH2Leaf> node);
        void BoundingBoxHit(TreeNode<BVH2Branch, BVH2Leaf> node);
        void PrimitiveNodeInspection(Leaf<BVH2Branch, BVH2Leaf> leaf);
        void PrimitiveNodePrimitiveHit(Leaf<BVH2Branch, BVH2Leaf> leaf, HitRecord hit);
        void BranchNodeInspection(Branch<BVH2Branch, BVH2Leaf> branch);
        void RayHitFound(HitRecord hit);
    }

    public class NullRayOrderOperations : RayOrderOperations
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
