using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class RayCostEvaluator : TransitionlessEvaluator<RayCostEvaluator.RayShuffleState, Unit, BoundAndCount>
    {
        private Segment3[] hits;
        private Ray3[] misses;
        private float _expo;

        public RayCostEvaluator(FHRayResults res, float expo)
        {
            _expo = expo;
            hits = res.Hits;
            misses = res.Misses;
        }

        public override RayCostEvaluator.RayShuffleState BeginEvaluations(int startTri, int endTri, BoundAndCount objectBounds, RayCostEvaluator.RayShuffleState parentState)
        {
            int hitPart = 0;
            int missPart = 0;
            for (int k = 0; k < parentState.hitMax; k++)
            {
                if (objectBounds.Box.DoesIntersectSegment(hits[k].Origin, hits[k].Difference))
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
                if (objectBounds.Box.DoesIntersectRay(misses[k].Origin, misses[k].Direction))
                {
                    if (missPart != k)
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

        public override EvalResult<Unit> EvaluateSplit(BoundAndCount left, BoundAndCount right, RayShuffleState state, Func<CenterIndexable, bool> leftFilter)
        {
            int left_collisions = 0;
            int right_collisions = 0;
            for (int k = 0; k < state.hitMax; k++)
            {
                if (left.Box.DoesIntersectSegment(hits[k].Origin, hits[k].Difference)) ++left_collisions;
                if (right.Box.DoesIntersectSegment(hits[k].Origin, hits[k].Difference)) ++right_collisions;
            }
            for (int k = 0; k < state.missMax; k++)
            {
                if (left.Box.DoesIntersectRay(misses[k].Origin, misses[k].Direction)) ++left_collisions;
                if (right.Box.DoesIntersectRay(misses[k].Origin, misses[k].Direction)) ++right_collisions;
            }
            /*
            for (int k = state.hitMax; k < hits.Length; k++)
                if (!leftBox.IntersectSegment(hits[k].Origin, hits[k].Difference).IsEmpty || !rightBox.IntersectSegment(hits[k].Origin, hits[k].Difference).IsEmpty)
                    throw new Exception("BAD STATE HIT");
            for (int k = state.missMax; k < misses.Length; k++)
                if (!leftBox.IntersectRay(misses[k].Origin, misses[k].Direction).IsEmpty || !rightBox.IntersectRay(misses[k].Origin, misses[k].Direction).IsEmpty)
                    throw new Exception("BAD STATE MISS");
             */
            return new EvalResult<Unit>(left_collisions * Math.Pow(left.Count - 1, _expo) + right_collisions * Math.Pow(right.Count - 1, _expo), Unit.ONLY, true);
        }
        
        public override RayCostEvaluator.RayShuffleState GetDefault()
        {
            return new RayShuffleState() { missMax = misses.Length, hitMax = hits.Length };
        }

        public struct RayShuffleState
        {
            public int missMax;
            public int hitMax;
        }
    }
}
