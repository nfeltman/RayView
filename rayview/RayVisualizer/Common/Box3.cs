using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public struct Box3
    {
        private ClosedInterval _xRange, _yRange, _zRange;
        private float _surfaceArea;
        public float SurfaceArea { get { return _surfaceArea; } }

        public Box3(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
            : this(new ClosedInterval(minX, maxX), new ClosedInterval(minY, maxY),new ClosedInterval(minZ, maxZ))
        {
        }

        public Box3(ClosedInterval xRange, ClosedInterval yRange, ClosedInterval zRange)
        {
            _xRange = xRange;
            _yRange = yRange;
            _zRange = zRange;

            float x = _xRange.Size;
            float y = _yRange.Size;
            float z = _zRange.Size;
            _surfaceArea = 2 * (x * y + y * z + z * x);
        }

        public ClosedInterval IntersectSegment(CVector3 orig, CVector3 end)
        {
            return IntersectInterval(orig, end-orig, new ClosedInterval(0, 1));
        }

        public ClosedInterval IntersectRay(CVector3 orig, CVector3 dir)
        {
            return IntersectInterval(orig, dir, ClosedInterval.POSITIVES);
        }

        public ClosedInterval IntersectLine(CVector3 orig, CVector3 dir)
        {
            return IntersectInterval(orig, dir, ClosedInterval.ALL);
        }

        public ClosedInterval IntersectInterval(CVector3 orig, CVector3 dir, ClosedInterval t_interval)
        {
            t_interval = t_interval & ((_xRange - orig.x) / dir.x);
            if (t_interval.IsEmpty) return t_interval;
            t_interval = t_interval & ((_yRange - orig.y) / dir.y);
            if (t_interval.IsEmpty) return t_interval;
            return t_interval & ((_zRange - orig.z) / dir.z);
        }

        public static Box3 operator |(Box3 a, Box3 b)
        {
            return new Box3(a._xRange | b._xRange, a._yRange | b._yRange, a._zRange | b._zRange);
        }
    }
}
