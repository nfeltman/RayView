﻿using System;
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
        private class InternalSRDMVisitor : NodeVisitor<TraceResult, RBVH2Branch, RBVH2Leaf>
        {
            public Segment3 ShadowRay { get; set; }

            public TraceResult ForBranch(Branch<RBVH2Branch, RBVH2Leaf> branch)
            {
                if (!branch.Content.BBox.DoesIntersectSegment(ShadowRay.Origin, ShadowRay.Difference))
                {
                    return new TraceResult(false, new TraceCost(), new TraceCost(1, 0, 0, 0));
                }
                float pLeft = branch.Content.Kernel.GetProbLeftFirst(ShadowRay.Origin, ShadowRay.Difference);

                TraceResult left;
                TraceResult right;

                if (pLeft == 1)
                {
                    left = branch.Left.Accept(this);
                    if (left.Hits)
                    {
                        left.Spine.BBoxTests.ExpectedValue += 1.0;
                        return left;
                    }
                    right = branch.Right.Accept(this);
                }
                else if (pLeft == 0)
                {
                    right = branch.Right.Accept(this);
                    if (right.Hits)
                    {
                        right.Spine.BBoxTests.ExpectedValue += 1.0;
                        return right;
                    }
                    left = branch.Left.Accept(this);
                }
                else
                {
                    left = branch.Left.Accept(this);
                    right = branch.Right.Accept(this);
                }

                TraceCost bothSpine = left.Spine + right.Spine;
                TraceCost bothSide = left.Side + right.Side;

                TraceCost resSpine;
                TraceCost resSide;
                if (left.Hits && right.Hits)
                {
                    resSpine = TraceCost.RandomSelect(pLeft, left.Spine, right.Spine);
                    resSide = TraceCost.RandomSelect(pLeft, left.Side, right.Side);
                }
                else if (!left.Hits && right.Hits)
                {
                    if (!left.Spine.IsZero) throw new Exception("This isn't right!");
                    resSpine = TraceCost.RandomSelect(pLeft, bothSpine, right.Spine);
                    resSide = TraceCost.RandomSelect(pLeft, bothSide, right.Side);
                }
                else if (left.Hits && !right.Hits)
                {
                    if (!right.Spine.IsZero) throw new Exception("This isn't right!");
                    resSpine = TraceCost.RandomSelect(pLeft, left.Spine, bothSpine);
                    resSide = TraceCost.RandomSelect(pLeft, left.Side, bothSide);
                }
                else
                {
                    if (!bothSpine.IsZero) throw new Exception("This isn't right!");
                    bothSide.BBoxTests.ExpectedValue += 1.0;

                    return new TraceResult(false, bothSpine, bothSide);
                }
                resSpine.BBoxTests.ExpectedValue += 1.0;
                return new TraceResult(true, resSpine, resSide);
            }

            public TraceResult ForLeaf(Leaf<RBVH2Branch, RBVH2Leaf> leaf)
            {
                if (!leaf.Content.BBox.DoesIntersectSegment(ShadowRay.Origin, ShadowRay.Difference))
                    return new TraceResult(false, new TraceCost(), new TraceCost(1, 0, 0, 0));
                Triangle[] prims = leaf.Content.Primitives;
                for (int k = 0; k < prims.Length; k++ )
                {
                    if (prims[k].IntersectsSegment(new CVector3(ShadowRay.Origin), new CVector3(ShadowRay.Difference)))
                    {
                        return new TraceResult(true, new TraceCost(1, 0, k + 1, 0), new TraceCost());
                    }
                }
                return new TraceResult(false, new TraceCost(), new TraceCost(1, 0, prims.Length, 0));
            }

            // if you have a string {0,1}^m with n 1s, and you randomly permute the string, what is the average depth of the first 1?
            private float z(int n, int m)
            {
                if (n > m) throw new ArgumentException("n cannot be greater than m");
                if (n == m) return 1;
                return 1 + (m - n) * z(n, m - 1) / m;
            }
        }

        private struct TraceResult
        {
            public bool Hits;
            public TraceCost Spine;
            public TraceCost Side;
            public TraceResult(bool hits, TraceCost spine, TraceCost side)
            {
                if (!hits && !spine.IsZero) throw new Exception("Cannot have a non-zero spine cost if we hit.");
                Hits = hits;
                Spine = spine;
                Side = side;
            }
        }

        private class InternalOracleVisitor : NodeVisitor<int, RBVH2Branch, RBVH2Leaf>
        {
            public Segment3 ShadowRay { get; set; }

            public int ForBranch(Branch<RBVH2Branch, RBVH2Leaf> branch)
            {
                if (!branch.Content.BBox.DoesIntersectSegment(ShadowRay.Origin, ShadowRay.Difference))
                    return -1;

                int left = branch.Left.Accept(this);
                int right = branch.Right.Accept(this);
                
                if(left < 0 && right < 0) return -1;
                if (left < 0) return right + 1;
                if (right < 0) return left + 1;
                return Math.Min(left, right) + 1;
            }

            public int ForLeaf(Leaf<RBVH2Branch, RBVH2Leaf> leaf)
            {
                if (!leaf.Content.BBox.DoesIntersectSegment(ShadowRay.Origin, ShadowRay.Difference))
                    return -1;
                Triangle[] prims = leaf.Content.Primitives;
                for (int k = 0; k < prims.Length; k++)
                {
                    if (prims[k].IntersectsSegment(new CVector3(ShadowRay.Origin), new CVector3(ShadowRay.Difference)))
                    {
                        return 1;
                    }
                }
                return -1;
            }
        }

        public static FullTraceResult GetTotalCost(RBVH2 tree, IEnumerable<ShadowQuery> shadows)
        {
            FullTraceResult cost = new FullTraceResult(); //the default value is correct
            //cost.badqueries = new List<ShadowQuery>();
            InternalSRDMVisitor measure = new InternalSRDMVisitor();
            InternalOracleVisitor measureO = new InternalOracleVisitor();
            foreach (ShadowQuery shadow in shadows)
            {
                measureO.ShadowRay = measure.ShadowRay = new Segment3(shadow.Origin, shadow.Difference);
                TraceResult res = tree.Accept(measure);
                int res2 = tree.Accept(measureO);
                //if (res.Hits == shadow.Connected) cost.badqueries.Add(shadow); //Console.WriteLine("Error case ({0}/{1}): {2} -> {3}", res.Hits, !shadow.Connected, shadow.Origin, shadow.Difference);
                if (res.Hits)
                {
                    if (res2 < 0)
                        throw new Exception("Not right! If SRDM hits, so should oracle! ");
                    cost.Spine += res.Spine;
                    cost.SideTrees += res.Side;
                    cost.SpineOracleBBox += res2;
                    if (shadow.Connected) cost.topazHit_mantaMiss++; else cost.topazHit_mantaHit++;
                }
                else
                {
                    if (res2 != -1)
                        throw new Exception("Not right! If SRDM misses, so should oracle.");
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
        public int SpineOracleBBox;
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
