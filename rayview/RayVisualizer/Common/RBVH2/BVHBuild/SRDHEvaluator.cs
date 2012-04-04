using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Simd;

namespace RayVisualizer.Common
{
    public class SRDHEvaluator<Tri> : SplitEvaluator<SRDHEvaluator<Tri>.ShadowRayShuffleState, SRDHEvaluator<Tri>.ShadowRayMemoData, TraversalKernel, SRDHEvaluator<Tri>.ShadowRayTransitionState, BoundAndCount>
        where Tri : CenterIndexable
    {
        private Segment3[] _connected;
        private CompiledShadowRay<Tri>[] _broken;
        private float _alpha;
        private KernelOptions _options;

        public SRDHEvaluator(ShadowRayResults<Tri> res, float alpha, KernelOptions options)
        {
            _alpha = alpha;
            _connected = res.Connected;
            _broken = res.Broken;
            _options = options;
        }

        public ShadowRayShuffleState BeginEvaluations(int startTri, int endTri, BoundAndCount objectBounds, ShadowRayTransitionState parentState)
        {

            // filter "connected" buffer
            int connectedPart = BuildTools.SweepPartition(_connected, 0, parentState.connectedMax, seg => objectBounds.Box.DoesIntersectSegment(seg.Origin, seg.Difference));

            // filter "broken" buffer
            int brokenPart = 0;
            // perform the passed in filter
            int firstPart = parentState.InitialFilter == null ? parentState.brokenMax : BuildTools.SweepPartition(_broken, 0, parentState.brokenMax, parentState.InitialFilter);
            for (int k = 0; k < firstPart; k++)
            {
                if (objectBounds.Box.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference))
                {
                    if (brokenPart != k)
                    {
                        BuildTools.Swap(_broken, brokenPart, k);
                    }
                    SortHits(ref _broken[brokenPart], startTri, endTri);
                    brokenPart++;
                }
            }

            // check that I didn't screw that up
            for (int j = 0; j < brokenPart; j++)
            {
                for (int k = 0; k < _broken[j].IntersectedTriangles.Length; k++)
                {
                    if ((_broken[j].IntersectedTriangles[k].BuildIndex >= startTri && _broken[j].IntersectedTriangles[k].BuildIndex < endTri) != k < _broken[j].MaxIntersectedTriangles)
                    {
                        Console.WriteLine("chcking rays 0 through " + brokenPart);
                        Console.WriteLine("ray " + j + " (" + _broken[j].MaxIntersectedTriangles + "/" + _broken[j].IntersectedTriangles.Length + ") tri " + k);
                        Console.WriteLine("has index " + _broken[j].IntersectedTriangles[k].BuildIndex + " [" + startTri + "," + endTri + ") ");
                        throw new Exception("error in this here partition ");
                    }
                }
            }
            return new ShadowRayShuffleState(brokenPart, connectedPart, startTri, endTri);
        }

        private static void SortHits<Tri2>(ref CompiledShadowRay<Tri2> ray, int startTri, int endTri)
            where Tri2 : Indexable
        {
            ray.MaxIntersectedTriangles = BuildTools.SweepPartition(ray.IntersectedTriangles, 0, ray.IntersectedTriangles.Length,
                bt => (bt.BuildIndex >= startTri && bt.BuildIndex < endTri));
        }

        public EvalResult<ShadowRayMemoData> EvaluateSplit(BoundAndCount left, BoundAndCount right, ShadowRayShuffleState state, Func<CenterIndexable, bool> leftFilter)
        {
            Box3 leftBox = left.Box;
            Box3 rightBox = right.Box;

            double leftFactor = Math.Pow(left.Count, _alpha);
            double rightFactor = Math.Pow(right.Count, _alpha);

            if (state.brokenMax == 0 && state.connectedMax == 0)
            {
                // no rays here; revert to SAH
                double sahCost = leftBox.SurfaceArea * leftFactor + rightBox.SurfaceArea * rightFactor;
                return new EvalResult<ShadowRayMemoData>(sahCost, new ShadowRayMemoData(TraversalKernel.UniformRandom, null, null), false);
            }

            Vector4f leftCenter = leftBox.GetCenter().Vec;
            Vector4f rightCenter = rightBox.GetCenter().Vec;

            int sure_ltraversal = 0;
            int sure_rtraversal = 0;

            // test all the faux hits from the "connected" buffer
            for (int k = 0; k < state.connectedMax; k++)
            {
                if (leftBox.DoesIntersectSegment(_connected[k].Origin, _connected[k].Difference)) ++sure_ltraversal;
                if (rightBox.DoesIntersectSegment(_connected[k].Origin, _connected[k].Difference)) ++sure_rtraversal;
            }

            if (state.brokenMax == 0)
            {
                double unavoidablePart = sure_ltraversal * leftFactor + sure_rtraversal * rightFactor;
                return new EvalResult<ShadowRayMemoData>(unavoidablePart, new ShadowRayMemoData(TraversalKernel.UniformRandom, null, null), false);
            }
            else
            {

                int LF_extra_ltraversal = 0;
                int RF_extra_rtraversal = 0;
                int FTB_extra_ltraversal = 0;
                int FTB_extra_rtraversal = 0;
                int BTF_extra_ltraversal = 0;
                int BTF_extra_rtraversal = 0;

                // test all the (maybe faux) hits from the "broken" buffer
                for (int k = 0; k < state.brokenMax; k++)
                {
                    // figure out if it hit a child
                    InteractionCombination combo = GetInteractionType(_broken[k].IntersectedTriangles, _broken[k].MaxIntersectedTriangles, leftFilter);
                    bool originCloserLeft = _options.BTF_or_FTB && Kernels.LeftIsCloser(leftCenter, rightCenter, _broken[k].Ray.Origin);
                    switch (combo)
                    {
                        case InteractionCombination.HitNeither:
                            if (leftBox.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference)) ++sure_ltraversal;
                            if (rightBox.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference)) ++sure_rtraversal; break;
                        case InteractionCombination.HitBoth:
                            //if (!leftBox.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference))  throw new Exception("uh oh 1");
                            //if (!rightBox.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference)) throw new Exception("uh oh 2");
                            ++RF_extra_rtraversal;
                            ++LF_extra_ltraversal;
                            if (originCloserLeft) { ++BTF_extra_rtraversal; ++FTB_extra_ltraversal; }
                            else { ++FTB_extra_rtraversal; ++BTF_extra_ltraversal; }
                            break;
                        case InteractionCombination.HitOnlyLeft:
                            //if (!leftBox.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference)) throw new Exception("uh oh 3");
                            ++sure_ltraversal;
                            if (rightBox.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference))
                            {
                                ++RF_extra_rtraversal;
                                if (originCloserLeft) ++BTF_extra_rtraversal;
                                else ++FTB_extra_rtraversal;
                            }
                            break;
                        case InteractionCombination.HitOnlyRight:
                            //if (!rightBox.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference)) throw new Exception("uh oh 4");
                            ++sure_rtraversal;
                            if (leftBox.DoesIntersectSegment(_broken[k].Ray.Origin, _broken[k].Ray.Difference))
                            {
                                ++LF_extra_ltraversal;
                                if (originCloserLeft) ++FTB_extra_ltraversal;
                                else ++BTF_extra_ltraversal;
                            }
                            break;
                    }
                }

                double LF_extra_total = LF_extra_ltraversal * leftFactor + (_options.LeftFirst ? 0 : double.PositiveInfinity);
                double RF_extra_total = RF_extra_rtraversal * rightFactor + (_options.RightFirst ? 0 : double.PositiveInfinity);
                double FTB_extra_total = FTB_extra_ltraversal * leftFactor + FTB_extra_rtraversal * rightFactor + (_options.FrontToBack ? 0 : double.PositiveInfinity);
                double BTF_extra_total = BTF_extra_ltraversal * leftFactor + BTF_extra_rtraversal * rightFactor + (_options.BackToFront ? 0 : double.PositiveInfinity);

                double unavoidablePart = sure_ltraversal * leftFactor + sure_rtraversal * rightFactor;

                if (RF_extra_total <= Math.Min(Math.Min(LF_extra_total, FTB_extra_total), BTF_extra_total))
                    return new EvalResult<ShadowRayMemoData>(RF_extra_total + unavoidablePart, new ShadowRayMemoData(TraversalKernel.RightFirst, cRay =>
                    {
                        // traverse right first, so we build left first, so we need to filter out left ray set (right ray set is all)
                        // a ray is in the left ray set if it didn't hit any (triangle in the active set on the right of the filter)
                        for (int k = 0; k < cRay.IntersectedTriangles.Length; k++)
                        {
                            Tri t = cRay.IntersectedTriangles[k];
                            if (t.BuildIndex >= state.StartTri && t.BuildIndex < state.EndTri && !leftFilter(t))
                                return false;
                        }
                        return true;
                    }, null), false);

                if (LF_extra_total <= Math.Min(Math.Min(RF_extra_total, FTB_extra_total), BTF_extra_total))
                    return new EvalResult<ShadowRayMemoData>(LF_extra_total + unavoidablePart, new ShadowRayMemoData(TraversalKernel.LeftFirst, null,
                    cRay =>
                    {
                        // traverse left first, so we build right first, so we need to filter out right ray set (left ray set is all)
                        // a ray is in the right ray set if it didn't hit any (triangle in the active set on the left of the filter)
                        for (int k = 0; k < cRay.IntersectedTriangles.Length; k++)
                        {
                            Tri t = cRay.IntersectedTriangles[k];
                            if (t.BuildIndex >= state.StartTri && t.BuildIndex < state.EndTri && leftFilter(t))
                                return false;
                        }
                        return true;
                    }), false);

                if (FTB_extra_total <= Math.Min(Math.Min(LF_extra_total, RF_extra_total), BTF_extra_total))
                    return new EvalResult<ShadowRayMemoData>(FTB_extra_total + unavoidablePart, new ShadowRayMemoData(TraversalKernel.FrontToBack, cRay =>
                    {
                        // a ray is in the left ray set if it went left first or didn't hit any (triangle in the active set on the right of the filter)
                        bool originCloserLeft = Kernels.LeftIsCloser(leftCenter, rightCenter, cRay.Ray.Origin);
                        if (originCloserLeft) return true;
                        for (int k = 0; k < cRay.IntersectedTriangles.Length; k++)
                        {
                            Tri t = cRay.IntersectedTriangles[k];
                            if (t.BuildIndex >= state.StartTri && t.BuildIndex < state.EndTri && !leftFilter(t))
                                return false;
                        }
                        return true;
                    },
                    cRay =>
                    {
                        // a ray is in the right ray set if it went right first or didn't hit any (triangle in the active set on the left of the filter)
                        bool originCloserLeft = Kernels.LeftIsCloser(leftCenter, rightCenter, cRay.Ray.Origin);
                        if (!originCloserLeft) return true;
                        for (int k = 0; k < cRay.IntersectedTriangles.Length; k++)
                        {
                            Tri t = cRay.IntersectedTriangles[k];
                            if (t.BuildIndex >= state.StartTri && t.BuildIndex < state.EndTri && leftFilter(t))
                                return false;
                        }
                        return true;
                    }), true);

                if (BTF_extra_total <= Math.Min(Math.Min(LF_extra_total, RF_extra_total), FTB_extra_total))
                    return new EvalResult<ShadowRayMemoData>(BTF_extra_total + unavoidablePart, new ShadowRayMemoData(TraversalKernel.BackToFront, cRay =>
                    {
                        // a ray is in the left ray set if it went left first or didn't hit any (triangle in the active set on the right of the filter)
                        bool originCloserLeft = Kernels.LeftIsCloser(leftCenter, rightCenter, cRay.Ray.Origin);
                        if (!originCloserLeft) return true;
                        for (int k = 0; k < cRay.IntersectedTriangles.Length; k++)
                        {
                            Tri t = cRay.IntersectedTriangles[k];
                            if (t.BuildIndex >= state.StartTri && t.BuildIndex < state.EndTri && !leftFilter(t))
                                return false;
                        }
                        return true;
                    },
                    cRay =>
                    {
                        // a ray is in the right ray set if it went right first or didn't hit any (triangle in the active set on the left of the filter)
                        bool originCloserLeft = Kernels.LeftIsCloser(leftCenter, rightCenter, cRay.Ray.Origin);
                        if (originCloserLeft) return true;
                        for (int k = 0; k < cRay.IntersectedTriangles.Length; k++)
                        {
                            Tri t = cRay.IntersectedTriangles[k];
                            if (t.BuildIndex >= state.StartTri && t.BuildIndex < state.EndTri && leftFilter(t))
                                return false;
                        }
                        return true;
                    }), true);

                throw new Exception("Whoa! How did I get here? One of the tests above me should pass.");
            }
        }

        private static InteractionCombination GetInteractionType<Tri2>(Tri2[] points, int max, Func<CenterIndexable, bool> leftFilter)
            where Tri2 : CenterIndexable
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

        public BuildReport<ShadowRayTransitionState, TraversalKernel> FinishEvaluations(EvalResult<ShadowRayMemoData> selected, ShadowRayShuffleState currentState)
        {
            //pass through the filters
            ShadowRayTransitionState left = new ShadowRayTransitionState(currentState.brokenMax, currentState.connectedMax, selected.Data.LeftRaysFilter);
            ShadowRayTransitionState right = new ShadowRayTransitionState(currentState.brokenMax, currentState.connectedMax, selected.Data.RightRaysFilter);
            return new BuildReport<ShadowRayTransitionState, TraversalKernel>(selected.Data.kernel, left, right);
        }

        public ShadowRayTransitionState GetDefault()
        {
            return new ShadowRayTransitionState(_broken.Length, _connected.Length, null);
        }

        public struct ShadowRayMemoData
        {
            public TraversalKernel kernel;
            public Func<CompiledShadowRay<Tri>, bool> LeftRaysFilter;
            public Func<CompiledShadowRay<Tri>, bool> RightRaysFilter;

            public ShadowRayMemoData(TraversalKernel p, Func<CompiledShadowRay<Tri>, bool> leftRaysFilter, Func<CompiledShadowRay<Tri>, bool> rightRaysFilter)
            {
                kernel = p;
                LeftRaysFilter = leftRaysFilter;
                RightRaysFilter = rightRaysFilter;
            }
        }

        public struct ShadowRayShuffleState
        {
            public int connectedMax;
            public int brokenMax;
            public int StartTri, EndTri;

            public ShadowRayShuffleState(int brokenPart, int connectedPart, int startTri, int endTri)
            {
                brokenMax = brokenPart;
                connectedMax = connectedPart;
                StartTri = startTri;
                EndTri = endTri;
            }
        }

        public struct ShadowRayTransitionState
        {
            public int connectedMax;
            public int brokenMax;
            public Func<CompiledShadowRay<Tri>, bool> InitialFilter;

            public ShadowRayTransitionState(int brokenPart, int connectedPart, Func<CompiledShadowRay<Tri>, bool> initialFilter)
            {
                brokenMax = brokenPart;
                connectedMax = connectedPart;
                InitialFilter = initialFilter;
            }
        }
    }

    public class KernelOptions
    {
        public bool LeftFirst { get; set; }
        public bool RightFirst { get; set; }
        public bool FrontToBack { get; set; }
        public bool BackToFront { get; set; }
        public bool BTF_or_FTB { get { return BackToFront || FrontToBack; } }
    }
}
