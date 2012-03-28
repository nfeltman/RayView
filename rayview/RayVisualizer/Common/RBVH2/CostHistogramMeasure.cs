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
    public static class CostHistogramMeasure
    {
        private class InternalVisitor : NodeVisitor<SimpleTraceResult, RBVH2Branch, RBVH2Leaf>
        {
            public Segment3 ShadowRay { get; set; }
            public Random rand = new Random(328472378);

            public SimpleTraceResult ForBranch(Branch<RBVH2Branch, RBVH2Leaf> branch)
            {
                if (!branch.Content.BBox.DoesIntersectSegment(ShadowRay.Origin, ShadowRay.Difference))
                    return new SimpleTraceResult(false, 1);

                
                TreeNode<RBVH2Branch, RBVH2Leaf> firstChild;
                switch(branch.Content.Kernel)
                {
                    case TraversalKernel.LeftFirst:
                        firstChild = branch.Left; break;
                    case TraversalKernel.RightFirst:
                        firstChild = branch.Right; break;
                    case TraversalKernel.UniformRandom:
                        firstChild = rand.NextDouble() < 0.5 ? branch.Left : branch.Right; break;
                    case TraversalKernel.FrontToBack:
                        firstChild = Kernels.LeftIsCloser(
                            branch.Left.OnContent((Boxed b) => b.BBox.GetCenter().Vec),
                            branch.Right.OnContent((Boxed b) => b.BBox.GetCenter().Vec),
                            ShadowRay.Origin) ? branch.Left : branch.Right;
                        break;
                    case TraversalKernel.BackToFront:
                        firstChild = Kernels.LeftIsCloser(branch.Left.OnContent(
                            (Boxed b) => b.BBox.GetCenter().Vec),
                            branch.Right.OnContent((Boxed b) => b.BBox.GetCenter().Vec),
                            ShadowRay.Origin) ? branch.Right : branch.Left;
                        break;
                    default:
                        throw new Exception("unsupported kernel!");
                }
                

                SimpleTraceResult firstRes = firstChild.Accept(this);
                firstRes.NumBBoxTests++;
                if (firstRes.Hits)
                    return firstRes;

                TreeNode<RBVH2Branch, RBVH2Leaf> secondChild = firstChild == branch.Left ? branch.Right : branch.Left;
                SimpleTraceResult secondRes = secondChild.Accept(this);
                secondRes.NumBBoxTests += firstRes.NumBBoxTests;
                return secondRes;
            }

            public SimpleTraceResult ForLeaf(Leaf<RBVH2Branch, RBVH2Leaf> leaf)
            {
                if (!leaf.Content.BBox.DoesIntersectSegment(ShadowRay.Origin, ShadowRay.Difference))
                    return new SimpleTraceResult(false, 1);
                Triangle[] prims = leaf.Content.Primitives;
                for (int k=0; k < prims.Length; k++)
                {
                    if (prims[k].IntersectsSegment(ShadowRay.Origin, ShadowRay.Difference))
                    {
                        return new SimpleTraceResult(true, 1);
                    }
                }
                return new SimpleTraceResult(false, 1);
            }
        }

        private struct SimpleTraceResult
        {
            public bool Hits;
            public int NumBBoxTests;
            public SimpleTraceResult(bool hits, int expectedBBoxTests)
            {
                Hits = hits;
                NumBBoxTests = expectedBBoxTests;
            }
        }

        public static RayCostHistogram GetTotalCost(RBVH2 tree, IEnumerable<ShadowQuery> shadows)
        {
            int numBins = 250;
            RayCostHistogram cost = new RayCostHistogram(numBins);
            InternalVisitor measure = new InternalVisitor();
            foreach (ShadowQuery shadow in shadows)
            {
                measure.ShadowRay = new Segment3(shadow.Origin, shadow.Difference);
                SimpleTraceResult res = tree.Accept(measure);
                if (res.Hits)
                {
                    if (res.NumBBoxTests >= numBins)
                    {
                        cost.outOfRangeHits++;
                        cost.outOfRangeBoth++;
                    }
                    else
                    {
                        cost.bboxCostBinsHits[res.NumBBoxTests]++;
                        cost.bboxCostBinsBoth[res.NumBBoxTests]++;
                    }
                }
                else
                {
                    if (res.NumBBoxTests >= numBins)
                    {
                        cost.outOfRangeMisses++;
                        cost.outOfRangeBoth++;
                    }
                    else
                    {
                        cost.bboxCostBinsMisses[res.NumBBoxTests]++;
                        cost.bboxCostBinsBoth[res.NumBBoxTests]++;
                    }
                }
            }
            return cost;
        }
    }

    public class RayCostHistogram
    {
        public int[] bboxCostBinsHits;
        public int[] bboxCostBinsMisses;
        public int[] bboxCostBinsBoth;
        public int outOfRangeHits;
        public int outOfRangeMisses;
        public int outOfRangeBoth;

        public RayCostHistogram(int numBins)
        {
            bboxCostBinsHits = new int[numBins];
            bboxCostBinsMisses = new int[numBins];
            bboxCostBinsBoth = new int[numBins];
        }
    }
}
