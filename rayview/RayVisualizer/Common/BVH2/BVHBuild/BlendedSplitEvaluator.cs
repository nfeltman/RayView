using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class BlendedSplitEvaluator : TransitionlessEvaluator<BlendedSplitEvaluator.RayShuffleState, Unit, BoundAndCount>
    {
        private Segment3[] hits;
        private Ray3[] misses;
        private float _expo;
        private float w;

        public BlendedSplitEvaluator(FHRayResults res, float expo, float weightRays)
        {
            _expo = expo;
            hits = res.Hits;
            misses = res.Misses;
            w = weightRays;
        }
        public override RayShuffleState BeginEvaluations(int startTri, int endTri, BoundAndCount toBeSplit, RayShuffleState parentState)
        {
            int hitPart = 0;
            int missPart = 0;
            for (int k = 0; k < parentState.hitMax; k++)
            {
                if (toBeSplit.Box.DoesIntersectSegment(hits[k].Origin, hits[k].Difference))
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
                if (toBeSplit.Box.DoesIntersectRay(misses[k].Origin, misses[k].Direction))
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
            
            return new RayShuffleState() { missMax = missPart, hitMax = hitPart, topSA = toBeSplit.Box.SurfaceArea};
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

            int rayMax = state.hitMax + state.missMax;
            double leftRayProp = rayMax == 0 ? 1 : ((double)left_collisions) / rayMax;
            double rightRayProp = rayMax == 0 ? 1 : ((double)right_collisions) / rayMax;
            double leftSAProp = left.Box.SurfaceArea / state.topSA;
            double rightSAProp = right.Box.SurfaceArea / state.topSA;
            double leftCombinedProp = leftRayProp * w + leftSAProp * (1 - w);
            double rightCombinedProp = rightRayProp * w + rightSAProp * (1 - w);

            return new EvalResult<Unit>(leftCombinedProp * Math.Pow(left.Count - 1, _expo) + rightCombinedProp * Math.Pow(right.Count - 1, _expo), Unit.ONLY, true);
        }

        public override BlendedSplitEvaluator.RayShuffleState GetDefault()
        {
            return new RayShuffleState() { missMax = misses.Length, hitMax = hits.Length };
        }

        public struct RayShuffleState
        {
            public int missMax;
            public int hitMax;
            public double topSA;
        }
    }
}
