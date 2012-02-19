﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class ShadowRayCostEvaluator : BVHSplitEvaluator<ShadowRayCostEvaluator.ShadowRayShuffleState, ShadowRayCostEvaluator.ShadowRayMemoData, float, ShadowRayCostEvaluator.ShadowRayShuffleState, BoundAndCount>
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

        public ShadowRayShuffleState BeginEvaluations(int startTri, int endTri, BoundAndCount objectBounds, ShadowRayShuffleState parentState)
        {
            // filter "connected" buffer
            int connectedPart = BuildTools.SweepPartition(_connected, 0, parentState.connectedMax, seg => objectBounds.Box.DoesIntersectSegment(seg.Origin, seg.Difference));

            // filter "broken" buffer
            int brokenPart = 0;
            for (int k = 0; k < parentState.brokenMax; k++)
            {
                if (objectBounds.Box.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference))
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

        public EvalResult<ShadowRayMemoData> EvaluateSplit(BoundAndCount left, BoundAndCount right, ShadowRayShuffleState state, Func<BuildTriangle, bool> leftFilter)
        {
            int left_sure_traversal = 0;
            int right_sure_traversal = 0;
            int left_maybe_traversal = 0;
            int right_maybe_traversal = 0;
            // test all the faux hits from the "connected" buffer
            for (int k = 0; k < state.connectedMax; k++)
            {
                if (left.Box.DoesIntersectSegment(_connected[k].Origin, _connected[k].Difference)) ++left_sure_traversal;
                if (right.Box.DoesIntersectSegment(_connected[k].Origin, _connected[k].Difference)) ++right_sure_traversal;
            }
            // test all the (maybe faux) hits from the "broken" buffer
            for (int k = 0; k < state.brokenMax; k++)
            {
                // figure out if it hit a child
                InteractionCombination combo = GetInteractionType(_broken[k].IntersectedTriangles, _broken[k].MaxIntersectedTriangles, leftFilter);
                // if it's a hit on the parent, it must be a hit for at least one of the children
                switch (combo)
                {
                    case InteractionCombination.HitNeither:
                        if (left.Box.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference))  ++left_sure_traversal;
                        if (right.Box.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference)) ++right_sure_traversal; break;
                    case InteractionCombination.HitBoth:
                        ++left_maybe_traversal;
                        ++right_maybe_traversal; break;
                    case InteractionCombination.HitOnlyLeft:
                        ++left_sure_traversal;
                        if (right.Box.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference)) ++right_maybe_traversal; break;
                    case InteractionCombination.HitOnlyRight:
                        ++right_sure_traversal;
                        if (left.Box.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference)) ++left_maybe_traversal; break;
                }
            }
            // there's a +c_i for every ray which we will neglect to add, since it exists despite the split
            // calculate whether we should do left first or right first
            double leftFactor = Math.Pow(left.Count, _alpha);
            double rightFactor = Math.Pow(right.Count, _alpha);
            double leftAvoidable = left_maybe_traversal * leftFactor;
            double rightAvoidable = right_maybe_traversal * rightFactor;
            double unavoidablePart = left_sure_traversal * leftFactor + right_sure_traversal * rightFactor;
            //Console.WriteLine("{0,4}-{1,4} m{2} {3} s{4,4} {5,4} u{6}", leftNu, rightNu, left_maybe_traversal, right_maybe_traversal, left_sure_traversal, right_sure_traversal, unavoidablePart);
            return leftAvoidable < rightAvoidable ? new EvalResult<ShadowRayMemoData>(leftAvoidable + unavoidablePart, new ShadowRayMemoData(1f, leftFilter), true)
                : new EvalResult<ShadowRayMemoData>(rightAvoidable + unavoidablePart, new ShadowRayMemoData(0f, t => !leftFilter(t)), true);
        }

        private static InteractionCombination GetInteractionType(BuildTriangle[] points, int max, Func<BuildTriangle,bool> leftFilter)
        {
            if (max == 0)
                return InteractionCombination.HitNeither;
                if (leftFilter(points[0]))
                {
                    for (int k = 1; k < max; k++)
                        if (!leftFilter(points[k]))
                            return InteractionCombination.HitBoth;
                    return InteractionCombination.HitOnlyLeft;
                }
                else
                {
                    for (int k = 1; k < max; k++)
                        if (leftFilter(points[k]))
                            return InteractionCombination.HitBoth;
                    return InteractionCombination.HitOnlyRight;
                }
        }

        private enum InteractionCombination
        {
            HitOnlyLeft, HitOnlyRight, HitBoth, HitNeither
        }

        public BuildReport<ShadowRayCostEvaluator.ShadowRayShuffleState, float> FinishEvaluations(EvalResult<ShadowRayMemoData> selected, ShadowRayCostEvaluator.ShadowRayShuffleState currentState)
        {
            // filter all of the rays which have an intersection on the "dominant" side of this division 
            int part = BuildTools.SweepPartition(_broken, 0, currentState.brokenMax, cRay => 
            {
                Func<BuildTriangle, bool> selector = selected.Data.DominantSideFilter;
                for (int k = 0; k < cRay.MaxIntersectedTriangles; k++)
                    if (selector(cRay.IntersectedTriangles[k]))
                        return false;
                return true;
            });
            ShadowRayShuffleState left = selected.BuildLeftFirst ? new ShadowRayShuffleState(part, currentState.connectedMax) : new ShadowRayShuffleState(currentState.brokenMax, currentState.connectedMax);
            ShadowRayShuffleState right = selected.BuildLeftFirst ? new ShadowRayShuffleState(currentState.brokenMax, currentState.connectedMax) : new ShadowRayShuffleState(part, currentState.connectedMax);
            return new BuildReport<ShadowRayShuffleState, float>(selected.Data.pLeft, left, right);
        }

        public ShadowRayCostEvaluator.ShadowRayShuffleState GetDefault()
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

        public struct ShadowRayMemoData
        {
            public float pLeft;
            public Func<BuildTriangle, bool> DominantSideFilter;

            public ShadowRayMemoData(float p, Func<BuildTriangle, bool> firstSideFilter)
            {
                pLeft = p;
                DominantSideFilter = firstSideFilter;
            }
        }
    }
}
