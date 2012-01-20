using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class ShadowRayCostEvaluator : BVHSplitEvaluator<ShadowRayCostEvaluator.ShadowRayShuffleState, float>
    {
        private Segment3[] _connected;
        private CompiledShadowRay[] _broken;
        private float _alpha;

        public ShadowRayCostEvaluator(ShadowRayResults res, float alpha)
        {
            _alpha = alpha;
            _connected = res.Connected;
            _broken = res.Broken;
        }

        public ShadowRayShuffleState SetState(Box3 objectBounds, ShadowRayShuffleState parentState)
        {
            int connectedPart = 0;
            for (int k = 0; k < parentState.connectedMax; k++)
            {
                if (objectBounds.DoesIntersectSegment(_connected[k].Origin, _connected[k].Difference))
                {
                    if (connectedPart != k)
                    {
                        Segment3 temp = _connected[k];
                        _connected[k] = _connected[connectedPart];
                        _connected[connectedPart] = temp;
                    }
                    connectedPart++;
                }
            }
            int brokenPart = 0;
            for (int k = 0; k < parentState.brokenMax; k++)
            {
                if (objectBounds.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference))
                {
                    if (connectedPart != k)
                    {
                        CompiledShadowRay temp = _broken[k];
                        _broken[k] = _broken[brokenPart];
                        _broken[brokenPart] = temp;
                    }
                    brokenPart++;
                }
            }
            return new ShadowRayShuffleState(brokenPart, connectedPart);
        }

        public EvalResult<float> EvaluateSplit(int leftNu, Box3 leftBox, int rightNu, Box3 rightBox, ShadowRayShuffleState state, AASplit split)
        {
            int left_sure_collisions = 0;
            int right_sure_collisions = 0;
            int left_maybe_collisions = 0;
            int right_maybe_collisions = 0;
            // test all the faux hits from the "connected" buffer
            for (int k = 0; k < state.connectedMax; k++)
            {
                if (leftBox.DoesIntersectSegment(_connected[k].Origin, _connected[k].Difference)) ++left_sure_collisions;
                if (rightBox.DoesIntersectSegment(_connected[k].Origin, _connected[k].Difference)) ++right_sure_collisions;
            }
            // test all the (maybe faux) hits from the "broken" buffer
            for (int k = 0; k < state.brokenMax; k++)
            {
                // figure out if it hit a child
                InteractionCombination combo = split.GetInteractionType(_broken[k].IntersectedTriangles, _broken[k].MaxIntersectedTriangles);
                // if it's a hit on the parent, it must be a hit for at least one of the children
                switch (combo)
                {
                    case InteractionCombination.HitBoth:
                        ++left_maybe_collisions;
                        ++right_maybe_collisions; break;
                    case InteractionCombination.HitOnlyLeft:
                        ++left_sure_collisions;
                        if (rightBox.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference)) ++right_maybe_collisions; break;
                    case InteractionCombination.HitOnlyRight:
                        ++right_sure_collisions;
                        if (leftBox.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference)) ++left_maybe_collisions; break;
                }
            }

            // calculate whether we should do left first or right first
            double leftFactor = Math.Pow(leftNu - 1, _alpha);
            double rightFactor = Math.Pow(rightNu - 1, _alpha);
            double leftAvoidable = left_maybe_collisions * leftFactor;
            double rightAvoidable = right_maybe_collisions * rightFactor;
            double unavoidablePart = left_sure_collisions * leftFactor + right_sure_collisions * rightFactor;
            return leftAvoidable < rightAvoidable ? new EvalResult<float>(leftAvoidable + unavoidablePart, 1f) : new EvalResult<float>(rightAvoidable + unavoidablePart, 0f);
        }

        public ShadowRayCostEvaluator.ShadowRayShuffleState GetDefaultState(Box3 toBeDivided)
        {
            return new ShadowRayShuffleState(_broken.Length, _connected.Length);
        }

        public struct ShadowRayShuffleState
        {
            public int connectedMax;
            public int brokenMax;

            public ShadowRayShuffleState(int brokenPart, int connectedPart)
            {
                brokenMax = brokenPart;
                connectedMax = connectedPart;
            }
        }
    }
}
