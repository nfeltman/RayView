﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class RayHitCostVisitor : BVH2Visitor<int>
    {
        private int _intersectionCount;
        private Segment3 _toTest;
        public int IntersectionCount { get { return _intersectionCount; } set { _intersectionCount = value; } }
        public Segment3 ToTest { get { return _toTest; } set { _toTest = value; } }

        public int ForBranch(BVH2Branch branch)
        {
            _intersectionCount++;
            if (branch.BBox.DoesIntersectInterval(_toTest.Origin, _toTest.Difference, new ClosedInterval(-.00001f, 1.00001f))) 
            {
                branch.Left.Accept(this);
                branch.Right.Accept(this);
            }
            return _intersectionCount;
        }

        public int ForLeaf(BVH2Leaf leaf)
        {
            _intersectionCount++;
            return _intersectionCount;
        }
    }
    public class LowRayHitCostVisitor : BVH2Visitor<int>
    {
        private int _intersectionCount;
        private Segment3 _toTest;
        public int IntersectionCount { get { return _intersectionCount; } set { _intersectionCount = value; } }
        public Segment3 ToTest { get { return _toTest; } set { _toTest = value; } }

        public int ForBranch(BVH2Branch branch)
        {
            _intersectionCount++;
            if (branch.BBox.DoesIntersectInterval(_toTest.Origin, _toTest.Difference, new ClosedInterval(0.000001f, 0.999999f)))
            {
                branch.Left.Accept(this);
                branch.Right.Accept(this);
            }
            return _intersectionCount;
        }

        public int ForLeaf(BVH2Leaf leaf)
        {
            _intersectionCount++;
            return _intersectionCount;
        }
    }

    public class RayMissCostVisitor : BVH2Visitor<int>
    {
        private int _intersectionCount;
        private Ray3 _toTest;
        public int IntersectionCount { get { return _intersectionCount; } set { _intersectionCount = value; } }
        public Ray3 ToTest { get { return _toTest; } set { _toTest = value; } }


        public int ForBranch(BVH2Branch branch)
        {
            _intersectionCount++;
            if (branch.BBox.DoesIntersectRay(_toTest.Origin, _toTest.Direction))
            {
                branch.Left.Accept(this);
                branch.Right.Accept(this);
            }
            return _intersectionCount;
        }

        public int ForLeaf(BVH2Leaf leaf)
        {
            _intersectionCount++;
            return _intersectionCount;
        }
    }
}
