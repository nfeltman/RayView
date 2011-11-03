using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public struct Box3
    {
        private ClosedInterval _xRange, _yRange, _zRange;

        public Box3(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            _xRange = new ClosedInterval(minX, maxX);
            _yRange = new ClosedInterval(minY, maxY);
            _zRange = new ClosedInterval(minZ, maxZ);
        }

        public bool IntersectSegment(CVector3 orig, CVector3 end)
        {
            return IntersectAll(orig, end-orig, new ClosedInterval(0, 1));
        }

        public bool IntersectRay(CVector3 orig, CVector3 dir)
        {
            return IntersectAll(orig, dir, ClosedInterval.POSITIVES);
        }

        public bool IntersectLine(CVector3 orig, CVector3 dir)
        {
            return IntersectAll(orig, dir, ClosedInterval.ALL);
        }

        private bool IntersectAll(CVector3 orig, CVector3 dir, ClosedInterval t_interval)
        {
            t_interval = t_interval & ((_xRange - orig.x) / dir.x);
            if (t_interval.IsEmpty) return false;
            t_interval = t_interval & ((_yRange - orig.y) / dir.y);
            if (t_interval.IsEmpty) return false;
            return (t_interval & ((_zRange - orig.z) / dir.z)).IsEmpty;
        }
    }
}
