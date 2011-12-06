﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace RayVisualizer.Common
{
    public struct Box3
    {
        private ClosedInterval _xRange;
        private ClosedInterval _yRange;
        private ClosedInterval _zRange;
        private float _surfaceArea;
        public float SurfaceArea { get { return _surfaceArea; } }
        public ClosedInterval XRange { get { return _xRange; } }
        public ClosedInterval YRange { get { return _yRange; } }
        public ClosedInterval ZRange { get { return _zRange; } }

        public static readonly Box3 EMPTY = new Box3(ClosedInterval.EMPTY, ClosedInterval.EMPTY, ClosedInterval.EMPTY);

        public Box3(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
            : this(new ClosedInterval(minX, maxX), new ClosedInterval(minY, maxY),new ClosedInterval(minZ, maxZ))
        {
        }

        public Box3(params CVector3[] points)
        {
            if (points.Length == 0)
            {
                _xRange = _yRange = _zRange = ClosedInterval.EMPTY;
            }
            else
            {
                float minX, maxX, minY, maxY, minZ, maxZ;
                minX = maxX = points[0].x;
                minY = maxY = points[0].y;
                minZ = maxZ = points[0].z;
                for (int k = 1; k < points.Length; k++)
                {
                    minX = Math.Min(minX, points[k].x);
                    maxX = Math.Max(maxX, points[k].x);
                    minY = Math.Min(minY, points[k].y);
                    maxY = Math.Max(maxY, points[k].y);
                    minZ = Math.Min(minZ, points[k].z);
                    maxZ = Math.Max(maxZ, points[k].z);
                }
                _xRange = new ClosedInterval(minX, maxX);
                _yRange = new ClosedInterval(minY, maxY);
                _zRange = new ClosedInterval(minZ, maxZ);
            }

            float x = _xRange.Size;
            float y = _yRange.Size;
            float z = _zRange.Size;
            _surfaceArea = 2 * (x * y + y * z + z * x);
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

        public CVector3 GetCenter()
        {
            return new CVector3((_xRange.Min + _xRange.Max) / 2, (_yRange.Min + _yRange.Max) / 2, (_zRange.Min + _zRange.Max) / 2);
        }

        public CVector3 GetCenter2()
        {
            return new CVector3(_xRange.Min + _xRange.Max, _yRange.Min + _yRange.Max, _zRange.Min + _zRange.Max);
        }

        public CVector3 TripleMin()
        {
            return new CVector3(_xRange.Min, _yRange.Min, _zRange.Min);
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

        public static Box3 operator |(Box3 a, CVector3 b)
        {
            return new Box3(a._xRange | b.x, a._yRange | b.y, a._zRange | b.z);
        }
    }
}
