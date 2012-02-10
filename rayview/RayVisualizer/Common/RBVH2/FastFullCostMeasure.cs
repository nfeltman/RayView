using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class FastFullCostMeasure : NodeVisitor<WideTraceResult, bool[], RBVH2Branch, RBVH2Leaf>
    {
        public Segment3[] ShadowRays { get; set; }
        public Pool<bool[]> MaskPool { get; set; }
        public Pool<TraceResult[]> ResultPool { get; set; }
        public int NumRays { get; set; }

        public WideTraceResult ForBranch(Branch<RBVH2Branch, RBVH2Leaf> branch, bool[] mask)
        {
            bool[] subMask = MaskPool.GetItem();
            Array.Clear(subMask, 0, NumRays);

            int passes = 0;
            for (int k = 0; k < NumRays; k++)
            {
                if (mask[k])
                {
                   // Console.WriteLine("{0} {1}", branch.Content.BBox, ShadowRays[k]==null);
                    if (branch.Content.BBox.DoesIntersectSegment(ShadowRays[k].Origin, ShadowRays[k].Difference))
                    {
                        subMask[k] = true;
                        passes++;
                    }
                }
            }

            if (passes == 0)
            {
                MaskPool.ReturnItem(ref subMask);
                TraceResult[] results = ResultPool.GetItem();
                for (int k = 0; k < NumRays; k++)
                {
                    results[k] = mask[k] ? new TraceResult(false, new TraceCost(new RandomVariable(1, 0), new RandomVariable(0, 0))) : new TraceResult();
                }
                return new WideTraceResult(results);
            }

            if (branch.Content.PLeft == 1 || branch.Content.PLeft == 0)
            {
                bool leftFirst = branch.Content.PLeft == 1;
                TreeNode<RBVH2Branch, RBVH2Leaf> first = leftFirst ? branch.Left : branch.Right;
                TreeNode<RBVH2Branch, RBVH2Leaf> second = leftFirst ? branch.Right : branch.Left;
                WideTraceResult firstResult = first.Accept(this, subMask);
                bool allHit = true;
                for (int k = 0; k < NumRays; k++)
                    if (mask[k] && !firstResult.Results[k].Hits)
                    {
                        allHit = false; break;
                    }
                if (allHit) // all hits on the first node, so we don't need to test the second node
                {
                    MaskPool.ReturnItem(ref subMask);
                    for (int k = 0; k < NumRays; k++)
                        if (mask[k]) firstResult.Results[k].Cost.BBoxTests.ExpectedValue += 1.0;
                    return new WideTraceResult(firstResult.Results);
                }
                for (int k = 0; k < NumRays; k++)
                    if (subMask[k] && firstResult.Results[k].Hits) subMask[k] = false; // if we hit on the first child, don't traverse with it on the second child
                WideTraceResult secondResult = second.Accept(this, subMask);
                for (int k = 0; k < NumRays; k++)
                {
                    if (subMask[k])
                    {
                        firstResult.Results[k].Hits = secondResult.Results[k].Hits;
                        firstResult.Results[k].Cost = firstResult.Results[k].Cost + secondResult.Results[k].Cost;
                    }
                    if (mask[k])
                        firstResult.Results[k].Cost.BBoxTests.ExpectedValue += 1.0;
                }
                ResultPool.ReturnItem(ref secondResult.Results);
                MaskPool.ReturnItem(ref subMask);
                return new WideTraceResult(firstResult.Results);
            }
            else
            {
                WideTraceResult leftResult = branch.Left.Accept(this, subMask);
                WideTraceResult rightResult = branch.Right.Accept(this, subMask);
                for (int k = 0; k < NumRays; k++)
                {
                    if (subMask[k])
                    {
                        TraceCost both = leftResult.Results[k].Cost + rightResult.Results[k].Cost;
                        if (leftResult.Results[k].Hits)
                        {
                            if (rightResult.Results[k].Hits)
                                leftResult.Results[k].Cost = TraceCost.RandomSelect(branch.Content.PLeft, leftResult.Results[k].Cost, rightResult.Results[k].Cost);
                            else
                                leftResult.Results[k].Cost = TraceCost.RandomSelect(branch.Content.PLeft, leftResult.Results[k].Cost, both);
                        }
                        else
                        {
                            if (rightResult.Results[k].Hits)
                            {
                                leftResult.Results[k] = new TraceResult(true, TraceCost.RandomSelect(branch.Content.PLeft, both, rightResult.Results[k].Cost));
                            }
                            else
                            {
                                leftResult.Results[k].Cost = both;
                            }
                        }
                    }
                    if (mask[k])
                        leftResult.Results[k].Cost.BBoxTests.ExpectedValue += 1.0;
                }
                ResultPool.ReturnItem(ref rightResult.Results);
                MaskPool.ReturnItem(ref subMask);
                return new WideTraceResult(leftResult.Results);
            }
        }

        public WideTraceResult ForLeaf(Leaf<RBVH2Branch, RBVH2Leaf> leaf, bool[] mask)
        {
            Triangle[] prims = leaf.Content.Primitives;
            TraceResult[] results = ResultPool.GetItem();
            for (int j = 0; j < NumRays; j++)
            {
                if (mask[j])
                {
                    if (leaf.Content.BBox.DoesIntersectSegment(ShadowRays[j].Origin, ShadowRays[j].Difference))
                    {
                        int k = 0;
                        int primtests = 0;
                        while (k < prims.Length)
                        {
                            primtests++;
                            if (prims[k].IntersectRay(ShadowRays[j].Origin, ShadowRays[j].Difference) < 1) break;
                            k++;
                        }
                        results[j] = new TraceResult(k != prims.Length, new TraceCost(1, 0, primtests, 0));
                    }
                    else
                    {
                        results[j] = new TraceResult(false, new TraceCost(1, 0, 0, 0));
                    }
                }
                else
                {
                    results[j] = new TraceResult(false, new TraceCost(0, 0, 0, 0));
                }
            }

            return new WideTraceResult(results);
        }

        public static TraceCost GetTotalCost(RBVH2 tree, IEnumerable<Segment3> shadows, int ray_width)
        {
            TraceCost cost = new TraceCost(); //the default value is correct
            FastFullCostMeasure measure = new FastFullCostMeasure();
            IEnumerator<Segment3> en = shadows.GetEnumerator();
            en.MoveNext();
            Segment3[] segments = new Segment3[ray_width];
            measure.MaskPool = new Pool<bool[]>(() => new bool[ray_width]);
            measure.ResultPool = new Pool<TraceResult[]>(() => new TraceResult[ray_width]);
            measure.ShadowRays = segments;
            bool keepGoing = true;
            while(keepGoing)
            {
                int k;
                bool[] initialMask = measure.MaskPool.GetItem();
                Array.Clear(initialMask, 0, ray_width);
                for (k = 0; k < segments.Length && keepGoing; k++)
                {
                    segments[k] = en.Current;
                    //if (segments[k] == null) Console.WriteLine(k);
                    initialMask[k] = true;
                    keepGoing = en.MoveNext();
                }
                measure.NumRays = k;
                for (; k < segments.Length; k++)
                {
                    segments[k] = null;
                }
                WideTraceResult res = tree.Accept(measure, initialMask);
                for (k = 0; k < measure.NumRays;k++)
                    cost = cost + res.Results[k].Cost;
                measure.MaskPool.ReturnItem(ref initialMask);
                measure.ResultPool.ReturnItem(ref res.Results);
            }
            return cost;
        }
    }

    public class WideTraceResult
    {
        public TraceResult[] Results;

        public WideTraceResult(TraceResult[] results)
        {
            Results = results;
        }
    }
}
