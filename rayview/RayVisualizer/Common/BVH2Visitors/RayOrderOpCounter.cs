using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace RayVisualizer.Common
{
    public class RayOrderOpCounter 
    {
        public static void RunOpCounter(BVH2 bvh, RayCast ray, RayOrderOperations ops)
        {
            PriorityQueue<float, Union<BVH2Node, Triangle>> q = new PriorityQueue<float, Union<BVH2Node, Triangle>>();

            ops.rayCasts++;
            ops.boundingBoxTests++;
            ClosedInterval rootInterval = bvh.Root.BBox.IntersectRay(ray.Origin,ray.Direction);
            if (!rootInterval.IsEmpty)
            {
                ops.boundingBoxHits++;
                q.Enqueue(rootInterval.Min, new Union<BVH2Node, Triangle>(bvh.Root));
            }
            
            CVector3 origin = ray.Origin;
            CVector3 direction;
            if (ray.Kind == RayKind.IntersectionHit || ray.Kind == RayKind.IntersectionMiss)
                direction = ray.Direction;
            else
                throw new Exception("Only for first-hit rays! Not any-hit!");

            while(!q.IsEmpty)
            {
                KeyValuePair<float, Union<BVH2Node, Triangle>> pair = q.Dequeue();
                if (pair.Value.Run(n => n.Accept(
                    branch =>
                    {
                        ops.branchNodeInspections++;

                        ClosedInterval leftIntersection = branch.Left.BBox.IntersectRay(origin, direction);
                        ClosedInterval rightIntersection = branch.Right.BBox.IntersectRay(origin, direction);
                        ops.boundingBoxTests+=2;

                        if (!leftIntersection.IsEmpty)
                        {
                            ops.boundingBoxHits++;
                            q.Enqueue(leftIntersection.Min, new Union<BVH2Node, Triangle>(branch.Left));
                        }
                        if (!rightIntersection.IsEmpty)
                        {
                            ops.boundingBoxHits++;
                            q.Enqueue(rightIntersection.Min, new Union<BVH2Node, Triangle>(branch.Right));
                        }
                        return false; //do not stop queueing
                    },
                    leaf =>
                    {
                        ops.primitiveNodeInspections++;
                        Tuple<float, int> closestIntersection = leaf.FindClosestPositiveIntersection(origin, direction);
                        if (closestIntersection.Item2 != -1)
                        {
                            CVector3 point = (direction * closestIntersection.Item1) + origin;
                            Triangle t = leaf.Primitives[closestIntersection.Item2];
                            float de = (point-t.p1) * ((t.p1-t.p2)^(t.p3-t.p2));
                          //  if (ray.Kind != RayKind.IntersectionHit)
                          //     Console.WriteLine("AHAHAHAH");
                            leaf.FindClosestPositiveIntersection(origin, direction);
                            ops.primitiveNodePrimitiveHits++;
                            q.Enqueue(closestIntersection.Item1, new Union<BVH2Node, Triangle>(leaf.Primitives[closestIntersection.Item2]));
                        }
                        return false; //do not stop queueing
                    }), 
                    triangle => 
                    {
                        ops.primitiveIntersectionReached++;
                        return true; //stop queueing
                    })) break;
            }
        }
    }

    public class RayOrderOperations
    {
        public int rayCasts;
        public int boundingBoxTests;
        public int boundingBoxHits;
        public int primitiveNodeInspections;
        public int primitiveNodePrimitiveHits;
        public int branchNodeInspections;
        public int primitiveIntersectionReached;
    }
}
