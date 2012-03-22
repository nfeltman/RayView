using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    using BVH2 = Tree<BVH2Branch, BVH2Leaf>;
    using RBVH2 = Tree<RBVH2Branch, RBVH2Leaf>;
    using BackedBVH2 = Tree<BackedBVH2Branch, BackedBVH2Leaf>;
    using BackedRBVH2 = Tree<BackedRBVH2Branch, BackedRBVH2Leaf>;

    // does not assume this box has already been hit
    public static class FullCostMeasure
    {
        private class InternalVisitor : NodeVisitor<TraceResult, RBVH2Branch, RBVH2Leaf>
        {
            public Segment3 ShadowRay { get; set; }

            public TraceResult ForBranch(Branch<RBVH2Branch, RBVH2Leaf> branch)
            {
                if (!branch.Content.BBox.DoesIntersectSegment(ShadowRay.Origin, ShadowRay.Difference))
                {
                    return new TraceResult(false, new TraceCost(), new TraceCost(), new TraceCost(1, 0, 0, 0));
                }

                TraceResult left = branch.Left.Accept(this);
                TraceResult right = branch.Right.Accept(this);

                TraceCost bothSpine = left.Spine + right.Spine;
                TraceCost bothSide = left.Side + right.Side;

                TraceCost resSpine;
                TraceCost resSide;
                TraceCost resSpineOracle;
                if (left.Hits && right.Hits)
                {
                    resSpineOracle = left.SpineOracle.BBoxTests.ExpectedValue < right.SpineOracle.BBoxTests.ExpectedValue ? left.SpineOracle : right.SpineOracle;
                    resSpine = TraceCost.RandomSelect(branch.Content.PLeft, left.Spine, right.Spine);
                    resSide = TraceCost.RandomSelect(branch.Content.PLeft, left.Side, right.Side);
                }
                else if (!left.Hits && right.Hits)
                {
                    if (!left.Spine.IsZero) throw new Exception("This isn't right!");
                    resSpineOracle = right.SpineOracle;
                    resSpine = TraceCost.RandomSelect(branch.Content.PLeft, bothSpine, right.Spine);
                    resSide = TraceCost.RandomSelect(branch.Content.PLeft, bothSide, right.Side);
                }
                else if (left.Hits && !right.Hits)
                {
                    if (!right.Spine.IsZero) throw new Exception("This isn't right!");
                    resSpineOracle = left.SpineOracle;
                    resSpine = TraceCost.RandomSelect(branch.Content.PLeft, left.Spine, bothSpine);
                    resSide = TraceCost.RandomSelect(branch.Content.PLeft, left.Side, bothSide);
                }
                else
                {
                    if (!bothSpine.IsZero) throw new Exception("This isn't right!");
                    bothSide.BBoxTests.ExpectedValue += 1.0;

                    return new TraceResult(false, new TraceCost(), bothSpine, bothSide);
                }
                resSpine.BBoxTests.ExpectedValue += 1.0;
                resSpineOracle.BBoxTests.ExpectedValue += 1.0;
                return new TraceResult(true, resSpineOracle, resSpine, resSide);
            }

            public TraceResult ForLeaf(Leaf<RBVH2Branch, RBVH2Leaf> leaf)
            {
                if (!leaf.Content.BBox.DoesIntersectSegment(ShadowRay.Origin, ShadowRay.Difference))
                    return new TraceResult(false, new TraceCost(), new TraceCost(), new TraceCost(1, 0, 0, 0));
                Triangle[] prims = leaf.Content.Primitives;
                int k = 0;
                int primtests = 0;
                while (k < prims.Length)
                {
                    primtests++;
                    if (prims[k].IntersectsSegment(new CVector3(ShadowRay.Origin), new CVector3(ShadowRay.Difference)))
                    {
                        break;
                    }
                    k++;
                }
                if (k == prims.Length)
                {
                    return new TraceResult(false, new TraceCost(), new TraceCost(), new TraceCost(1, 0, prims.Length, 0));
                }
                else
                {
                    return new TraceResult(true, new TraceCost(1, 0, 1, 0), new TraceCost(1, 0, primtests, 0), new TraceCost());
                }
            }
        }

        private struct TraceResult
        {
            public bool Hits;
            public TraceCost SpineOracle;
            public TraceCost Spine;
            public TraceCost Side;
            public TraceResult(bool hits, TraceCost spineOracle, TraceCost spine, TraceCost side)
            {
                if (!hits && !spine.IsZero) throw new Exception("Cannot have a non-zero spine cost if we hit.");
                Hits = hits;
                Spine = spine;
                Side = side;
                SpineOracle = spineOracle;
            }
        }

        public static FullTraceResult GetTotalCost(RBVH2 tree, IEnumerable<ShadowQuery> shadows)
        {
            FullTraceResult cost = new FullTraceResult(); //the default value is correct
            //cost.badqueries = new List<ShadowQuery>();
            InternalVisitor measure = new InternalVisitor();
            foreach (ShadowQuery shadow in shadows)
            {
                measure.ShadowRay = new Segment3(shadow.Origin, shadow.Difference);
                TraceResult res = tree.Accept(measure);
                //if (res.Hits == shadow.Connected) cost.badqueries.Add(shadow); //Console.WriteLine("Error case ({0}/{1}): {2} -> {3}", res.Hits, !shadow.Connected, shadow.Origin, shadow.Difference);
                if (res.Hits)
                {
                    cost.Spine += res.Spine;
                    cost.SpineOracle += res.SpineOracle;
                    cost.SideTrees += res.Side;
                    if (shadow.Connected) cost.topazHit_mantaMiss++; else cost.topazHit_mantaHit++;
                }
                else
                {
                    if (!res.Spine.IsZero)
                        throw new Exception("This isn't right!  Complete miss implies spine cost should be zero.");
                    cost.NonHit += res.Side;
                    if (shadow.Connected) cost.topazMiss_mantaMiss++; else cost.topazMiss_mantaHit++;
                }
            }

            Console.WriteLine("Disagreement = {0}.", cost.Disagreement);
            if (cost.NumRays != 0 && cost.Disagreement > 0.02)
            {
                //throw new Exception("Badly disagree with results from ray file.");
            }
            return cost;
        }
    }

    public class FullTraceResult
    {
        public int topazHit_mantaHit = 0;
        public int topazHit_mantaMiss = 0;
        public int topazMiss_mantaHit = 0;
        public int topazMiss_mantaMiss = 0;
        public TraceCost SpineOracle;
        public TraceCost Spine;
        public TraceCost SideTrees;
        public TraceCost NonHit;
        //public List<ShadowQuery> badqueries;

        public int NumRays { get { return topazMiss_mantaMiss + topazMiss_mantaHit + topazHit_mantaMiss + topazHit_mantaHit; } }
        public double Disagreement { get { return ((double)topazHit_mantaMiss + topazMiss_mantaHit) / NumRays; } }
        /*
        public FullTraceResult(int numRays, int numHits, TraceCost spine, TraceCost sideTrees, TraceCost nonHit)
        {
            NumRays = numRays;
            NumHits = numHits;
            Spine = spine;
            SideTrees = sideTrees;
            NonHit = nonHit;
        }*/

        /*
        public static bool operator ==(FullTraceResult t1, FullTraceResult t2)
        {
            return t1.Hits == t2.Hits && t1.Cost == t2.Cost;
        }
        public static bool operator !=(FullTraceResult t1, FullTraceResult t2)
        {
            return t1.Hits != t2.Hits || t1.Cost != t2.Cost;
        }*/
    }
}
