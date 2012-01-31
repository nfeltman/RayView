using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class ShadowRayCostEvaluator : TransitionlessEvaluator<ShadowRayCostEvaluator.ShadowRayShuffleState, float>
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

        public override ShadowRayShuffleState BeginEvaluations(int startTri, int endTri, Box3 objectBounds, ShadowRayShuffleState parentState)
        {
            // filter "connected" buffer
            int connectedPart = BuildTools.SweepPartition(_connected, 0, parentState.connectedMax, seg => objectBounds.DoesIntersectSegment(seg.Origin, seg.Difference));

            // filter "broken" buffer
            int brokenPart = 0;
            for (int k = 0; k < parentState.brokenMax; k++)
            {
                if (objectBounds.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference))
                {
                    if (connectedPart != k)
                    {
                        BuildTools.Swap(_broken, brokenPart, k);
                    }
                    SortHits(_broken[k], startTri, endTri);
                    brokenPart++;
                }
            }
            return new ShadowRayShuffleState(brokenPart, connectedPart);
        }

        private static void SortHits(CompiledShadowRay ray, int startTri, int endTri)
        {
            ray.MaxIntersectedTriangles = BuildTools.SweepPartition(ray.IntersectedTriangles, 0, ray.IntersectedTriangles.Length,
                bt => (bt.index >= startTri && bt.index < endTri));
        }

        public override EvalResult<float> EvaluateSplit(int leftNu, Box3 leftBox, int rightNu, Box3 rightBox, ShadowRayShuffleState state, AASplitSeries split, int threshold)
        {
            int left_sure_traversal = 0;
            int right_sure_traversal = 0;
            int left_maybe_traversal = 0;
            int right_maybe_traversal = 0;
            // test all the faux hits from the "connected" buffer
            for (int k = 0; k < state.connectedMax; k++)
            {
                if (leftBox.DoesIntersectSegment(_connected[k].Origin, _connected[k].Difference)) ++left_sure_traversal;
                if (rightBox.DoesIntersectSegment(_connected[k].Origin, _connected[k].Difference)) ++right_sure_traversal;
            }
            // test all the (maybe faux) hits from the "broken" buffer
            for (int k = 0; k < state.brokenMax; k++)
            {
                // figure out if it hit a child
                InteractionCombination combo = split.GetInteractionType(_broken[k].IntersectedTriangles, _broken[k].MaxIntersectedTriangles, threshold);
                // if it's a hit on the parent, it must be a hit for at least one of the children
                switch (combo)
                {
                    case InteractionCombination.HitNeither:
                        if (leftBox.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference))  ++left_sure_traversal;
                        if (rightBox.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference)) ++right_sure_traversal; break;
                    case InteractionCombination.HitBoth:
                        ++left_maybe_traversal;
                        ++right_maybe_traversal; break;
                    case InteractionCombination.HitOnlyLeft:
                        ++left_sure_traversal;
                        if (rightBox.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference)) ++right_maybe_traversal; break;
                    case InteractionCombination.HitOnlyRight:
                        ++right_sure_traversal;
                        if (leftBox.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference)) ++left_maybe_traversal; break;
                }
            }
            // there's a +c_i for every ray which we will neglect to add, since it exists despite the split
            // calculate whether we should do left first or right first
            double leftFactor = Math.Pow(leftNu, _alpha);
            double rightFactor = Math.Pow(rightNu, _alpha);
            double leftAvoidable = left_maybe_traversal * leftFactor;
            double rightAvoidable = right_maybe_traversal * rightFactor;
            double unavoidablePart = left_sure_traversal * leftFactor + right_sure_traversal * rightFactor;
            return leftAvoidable < rightAvoidable ? new EvalResult<float>(leftAvoidable + unavoidablePart, 1f) : new EvalResult<float>(rightAvoidable + unavoidablePart, 0f);
        }

        public override ShadowRayCostEvaluator.ShadowRayShuffleState GetDefault()
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
