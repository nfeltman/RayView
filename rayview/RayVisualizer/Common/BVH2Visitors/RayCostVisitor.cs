using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class RayHitCostVisitor : BVH2Visitor<int>
    {
        private int _intersectionCount;
        private FHRayHit _toTest;
        public int IntersectionCount { get { return _intersectionCount; } set { _intersectionCount = value; } }
        public FHRayHit ToTest { get { return _toTest; } set { _toTest = value; } }

        public int ForBranch(BVH2Branch branch)
        {
            _intersectionCount++;
            ClosedInterval intersectionInterval = branch.BBox.IntersectInterval(_toTest.Origin, _toTest.Difference, new ClosedInterval(-.00001f, 1.00001f));
            if (!intersectionInterval.IsEmpty) 
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
        private FHRayHit _toTest;
        public int IntersectionCount { get { return _intersectionCount; } set { _intersectionCount = value; } }
        public FHRayHit ToTest { get { return _toTest; } set { _toTest = value; } }

        public int ForBranch(BVH2Branch branch)
        {
            _intersectionCount++;
            ClosedInterval intersectionInterval = branch.BBox.IntersectInterval(_toTest.Origin, _toTest.Difference, new ClosedInterval(0.000001f, 0.999999f));
            if (!intersectionInterval.IsEmpty)
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
        private FHRayMiss _toTest;
        public int IntersectionCount { get { return _intersectionCount; } set { _intersectionCount = value; } }
        public FHRayMiss ToTest { get { return _toTest; } set { _toTest = value; } }


        public int ForBranch(BVH2Branch branch)
        {
            _intersectionCount++;
            if (!branch.BBox.IntersectRay(_toTest.Origin, _toTest.Direction).IsEmpty)
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
