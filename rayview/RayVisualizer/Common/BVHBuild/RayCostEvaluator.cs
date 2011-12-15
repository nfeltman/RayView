using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class RayCostEvaluator : SplitEvaluator<RayCostEvaluator.RayShuffleState>
    {
        Segment3[] hits;
        Ray3[] misses;
        float _expo;

        public RayCostEvaluator(FHRayResults res, float expo)
        {
            _expo = expo;
            hits = res.Hits;
            misses = res.Misses;
        }
        public RayShuffleState SetState(Box3 toBeSplit, RayShuffleState parentState)
        {
            int hitPart = 0;
            int missPart = 0;
            for (int k = 0; k < parentState.hitMax; k++)
            {
                if (toBeSplit.DoesIntersectSegment(hits[k].Origin, hits[k].Difference))
                {
                    if (hitPart != k)
                    {
                        Segment3 temp = hits[k];
                        hits[k] = hits[hitPart];
                        hits[hitPart] = temp;
                    }
                    hitPart++;
                }
            } 
            for (int k = 0; k < parentState.missMax; k++)
            {
                if (toBeSplit.DoesIntersectRay(misses[k].Origin, misses[k].Direction))
                {
                    if (hitPart != k)
                    {
                        Ray3 temp = misses[k];
                        misses[k] = misses[missPart];
                        misses[missPart] = temp;
                    }
                    missPart++;
                }
            }
            /*
            for (int k = parentState.hitMax; k < hits.Length; k++)
                if (!toBeSplit.IntersectSegment(hits[k].Origin, hits[k].Difference).IsEmpty)
                    throw new Exception("BAD STATE HIT");
            for (int k = parentState.missMax; k < misses.Length; k++)
                if (!toBeSplit.IntersectRay(misses[k].Origin, misses[k].Direction).IsEmpty)
                    throw new Exception("BAD STATE MISS");
             */
            return new RayShuffleState() { missMax = missPart, hitMax = hitPart };
        }

        public float EvaluateSplit(int leftNu, Box3 leftBox, int rightNu, Box3 rightBox, RayShuffleState state)
        {
            int left_collisions = 0;
            int right_collisions = 0;
            for (int k = 0; k < state.hitMax; k++)
            {
                if (leftBox.DoesIntersectSegment(hits[k].Origin, hits[k].Difference)) ++left_collisions;
                if (rightBox.DoesIntersectSegment(hits[k].Origin, hits[k].Difference)) ++right_collisions;
            }
            for (int k = 0; k < state.missMax; k++)
            {
                if (leftBox.DoesIntersectRay(misses[k].Origin, misses[k].Direction)) ++left_collisions;
                if (rightBox.DoesIntersectRay(misses[k].Origin, misses[k].Direction)) ++right_collisions;
            }
            /*
            for (int k = state.hitMax; k < hits.Length; k++)
                if (!leftBox.IntersectSegment(hits[k].Origin, hits[k].Difference).IsEmpty || !rightBox.IntersectSegment(hits[k].Origin, hits[k].Difference).IsEmpty)
                    throw new Exception("BAD STATE HIT");
            for (int k = state.missMax; k < misses.Length; k++)
                if (!leftBox.IntersectRay(misses[k].Origin, misses[k].Direction).IsEmpty || !rightBox.IntersectRay(misses[k].Origin, misses[k].Direction).IsEmpty)
                    throw new Exception("BAD STATE MISS");
             * */
            return (float)(left_collisions * Math.Pow(leftNu,_expo) + right_collisions * Math.Pow(rightNu,_expo));
        }

        public struct RayShuffleState
        {
            public int missMax;
            public int hitMax;

            public RayShuffleState(FHRayResults res)
            {
                missMax = res.Misses.Length;
                hitMax = res.Hits.Length;
            }
        }
    }
}
