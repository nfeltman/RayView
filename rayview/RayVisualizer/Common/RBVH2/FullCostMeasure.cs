using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
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
                    return new TraceResult(false, new TraceCost(), new TraceCost(1, 0, 0, 0));
                }

                TraceResult left;
                TraceResult right;

                if (branch.Content.PLeft == 1)
                {
                    left = branch.Left.Accept(this);
                    if (left.Hits)
                    {
                        left.Spine.BBoxTests.ExpectedValue += 1.0;
                        return left;
                    }
                    right = branch.Right.Accept(this);
                }
                else if (branch.Content.PLeft == 0)
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
                    resSpine = TraceCost.RandomSelect(branch.Content.PLeft, left.Spine, right.Spine);
                    resSide = TraceCost.RandomSelect(branch.Content.PLeft, left.Side, right.Side);
                }
                else if (!left.Hits && right.Hits)
                {
                    if (!left.Spine.IsZero) throw new Exception("This isn't right!");
                    resSpine = TraceCost.RandomSelect(branch.Content.PLeft, bothSpine, right.Spine);
                    resSide = TraceCost.RandomSelect(branch.Content.PLeft, bothSide, right.Side);
                }
                else if (left.Hits && !right.Hits)
                {
                    if (!right.Spine.IsZero) throw new Exception("This isn't right!");
                    resSpine = TraceCost.RandomSelect(branch.Content.PLeft, left.Spine, bothSpine);
                    resSide = TraceCost.RandomSelect(branch.Content.PLeft, left.Side, bothSide);
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
                int k = 0;
                int primtests = 0;
                while (k < prims.Length)
                {
                    primtests++;
                    if (prims[k].IntersectsSegment(ShadowRay.Origin, ShadowRay.Difference))
                    {
                        break;
                    }
                    k++;
                }
                if (k == prims.Length)
                {
                    return new TraceResult(false, new TraceCost(), new TraceCost(1, 0, prims.Length, 0));
                }
                else
                {
                    return new TraceResult(true, new TraceCost(1, 0, primtests, 0), new TraceCost());
                }
            }
        }

        private class TraceResult
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

        public static FullTraceResult GetTotalCost(RBVH2 tree, IEnumerable<ShadowQuery> shadows)
        {
            FullTraceResult cost = new FullTraceResult(); //the default value is correct
            InternalVisitor measure = new InternalVisitor();
            int topazBrok_mantaBrok = 0;
            int topazBrok_mantaCon = 0;
            int topazConn_mantaBrok = 0;
            int topazConn_mantaConn = 0;
            foreach (ShadowQuery shadow in shadows)
            {
                cost.NumRays++;
                measure.ShadowRay = new Segment3(shadow.Origin, shadow.Difference);
                TraceResult res = tree.Accept(measure);
                if (res.Hits)
                {
                    cost.Spine += res.Spine;
                    cost.SideTrees += res.Side;
                    if (shadow.Connected) topazBrok_mantaCon++; else topazBrok_mantaBrok++;
                }
                else
                {
                    if (!res.Spine.IsZero)
                        throw new Exception("This isn't right!  Complete miss implies spine cost should be zero.");
                    cost.NonHit += res.Side;
                    if (shadow.Connected) topazConn_mantaConn++; else topazConn_mantaBrok++;
                }
            }
            cost.NumHits = topazBrok_mantaBrok + topazBrok_mantaCon;
            double disagreement = ((double)topazConn_mantaBrok + topazBrok_mantaCon) / cost.NumRays; 
            Console.WriteLine("Disagreement = {0}.  TB/MC = {1}.  TC/MB = {2}. TB/MB = {3}. TC/MC = {4}.", disagreement, topazBrok_mantaCon, topazConn_mantaBrok, topazBrok_mantaBrok, topazConn_mantaConn);    
            if (cost.NumRays != 0 && disagreement > 0.005)
            {
                //throw new Exception("Badly disagree with results from ray file.");
            }
            return cost;
        }
    }

    public class FullTraceResult
    {
        public int NumRays;
        public int NumHits;
        public TraceCost Spine;
        public TraceCost SideTrees;
        public TraceCost NonHit;
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
