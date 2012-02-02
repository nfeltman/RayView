using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public struct BoundBuilder
    {
        private float xMin, xMax, yMin, yMax, zMin, zMax;
        public BoundBuilder(bool b)
        {
            xMin = yMin = zMin = float.PositiveInfinity;
            xMax = yMax = zMax = float.NegativeInfinity;
        }
        public BoundBuilder(BoundBuilder b)
        {
            xMin = b.xMin;
            xMax = b.xMax;
            yMin = b.yMin;
            yMax = b.yMax;
            zMin = b.zMin;
            zMax = b.zMax;
        }
        public void Reset()
        {
            xMin = yMin = zMin = float.PositiveInfinity;
            xMax = yMax = zMax = float.NegativeInfinity;
        }
        public void AddPoint(CVector3 p)
        {
            if (p.x < xMin) xMin = p.x;
            if (p.x > xMax) xMax = p.x;
            if (p.y < yMin) yMin = p.y;
            if (p.y > yMax) yMax = p.y;
            if (p.z < zMin) zMin = p.z;
            if (p.z > zMax) zMax = p.z;
        }
        public void AddTriangle(Triangle t)
        {
            if (t.p1.x < xMin) xMin = t.p1.x;
            if (t.p1.x > xMax) xMax = t.p1.x;
            if (t.p2.x < xMin) xMin = t.p2.x;
            if (t.p2.x > xMax) xMax = t.p2.x;
            if (t.p3.x < xMin) xMin = t.p3.x;
            if (t.p3.x > xMax) xMax = t.p3.x;
            if (t.p1.y < yMin) yMin = t.p1.y;
            if (t.p1.y > yMax) yMax = t.p1.y;
            if (t.p2.y < yMin) yMin = t.p2.y;
            if (t.p2.y > yMax) yMax = t.p2.y;
            if (t.p3.y < yMin) yMin = t.p3.y;
            if (t.p3.y > yMax) yMax = t.p3.y;
            if (t.p1.z < zMin) zMin = t.p1.z;
            if (t.p1.z > zMax) zMax = t.p1.z;
            if (t.p2.z < zMin) zMin = t.p2.z;
            if (t.p2.z > zMax) zMax = t.p2.z;
            if (t.p3.z < zMin) zMin = t.p3.z;
            if (t.p3.z > zMax) zMax = t.p3.z;
        }
        public void AddBox(Box3 b)
        {
            if (b.XRange.Min < xMin) xMin = b.XRange.Min;
            if (b.XRange.Max > xMax) xMax = b.XRange.Max;
            if (b.YRange.Min < yMin) yMin = b.YRange.Min;
            if (b.YRange.Max > yMax) yMax = b.YRange.Max;
            if (b.ZRange.Min < zMin) zMin = b.ZRange.Min;
            if (b.ZRange.Max > zMax) zMax = b.ZRange.Max;
        }
        public void AddBox(BoundBuilder b)
        {
            if (b.xMin < xMin) xMin = b.xMin;
            if (b.xMax > xMax) xMax = b.xMax;
            if (b.yMin < yMin) yMin = b.yMin;
            if (b.yMax > yMax) yMax = b.yMax;
            if (b.zMin < zMin) zMin = b.zMin;
            if (b.zMax > zMax) zMax = b.zMax;
        }
        public Box3 GetBox()
        {
            return new Box3(xMin, xMax, yMin, yMax, zMin, zMax);
        }
    }
}
