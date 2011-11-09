using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace RayVisualizer.Common
{
    // alias, because Union<...,...> is too long
    using QueueItem = Union<BVH2Node, HitRecord>;
    
    public class RayOrderOpCounter 
    {
        public static HitRecord RunOpCounter(BVH2 bvh, RayCast ray, RayOrderOperations ops)
        {

            if (!(ray.Kind == RayKind.FirstHit_Hit || ray.Kind == RayKind.FirstHit_Miss))
                throw new Exception("Only for first-hit rays! Not any-hit!");

            PriorityQueue<float, QueueItem> q = new PriorityQueue<float, QueueItem>();
            
            ops.rayCasts++;
            ops.boundingBoxTests++;
            ClosedInterval rootInterval = bvh.Root.BBox.IntersectRay(ray.Origin,ray.Direction);
            if (!rootInterval.IsEmpty)
            {
                ops.boundingBoxHits++;
                q.Enqueue(rootInterval.Min, new QueueItem(bvh.Root));
            }
            
            CVector3 origin = ray.Origin;
            CVector3 direction = ray.Direction;

            while(!q.IsEmpty)
            {
                KeyValuePair<float, QueueItem> pair = q.Dequeue();
                HitRecord intersection = pair.Value.Run(n => n.Accept(
                    // pattern match based on the type of queue item
                    (BVH2Branch branch) =>
                    {
                        ops.branchNodeInspections++;

                        ClosedInterval leftIntersection = branch.Left.BBox.IntersectRay(origin, direction);
                        ClosedInterval rightIntersection = branch.Right.BBox.IntersectRay(origin, direction);
                        ops.boundingBoxTests += 2;

                        if (!leftIntersection.IsEmpty)
                        {
                            ops.boundingBoxHits++;
                            q.Enqueue(leftIntersection.Min, new QueueItem(branch.Left));
                        }
                        if (!rightIntersection.IsEmpty)
                        {
                            ops.boundingBoxHits++;
                            q.Enqueue(rightIntersection.Min, new QueueItem(branch.Right));
                        }
                        return (HitRecord)null; //do not stop queue processing
                    },
                    (BVH2Leaf leaf) =>
                    {
                        ops.primitiveNodeInspections++;
                        Tuple<float, int> closestIntersection = leaf.FindClosestPositiveIntersection(origin, direction);
                        if (closestIntersection.Item2 != -1)
                        {
                            if (ray.Kind != RayKind.FirstHit_Hit)
                                Console.WriteLine("AHAHAHAH " + closestIntersection.Item1);
                            ops.primitiveNodePrimitiveHits++;
                            q.Enqueue(closestIntersection.Item1, new QueueItem(new HitRecord(leaf.Primitives[closestIntersection.Item2], 0)));
                        }
                        return (HitRecord)null; //do not stop queue processing
                    }),
                    (HitRecord hitRecord) =>
                    {
                        ops.rayHitFound++;
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

        public HitRecord(Triangle tri, float t) { triangle = tri; t_value = t; }
    }

    public class RayOrderOperations
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
