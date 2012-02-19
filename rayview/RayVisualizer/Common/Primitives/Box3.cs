using System;
using Mono.Simd;
using System.Runtime.CompilerServices;

namespace RayVisualizer.Common
{
    public struct Box3
    {
        /*
        private ClosedInterval _xRange;
        private ClosedInterval _yRange;
        private ClosedInterval _zRange;*
        public ClosedInterval XRange { get { return _xRange; } }
        public ClosedInterval YRange { get { return _yRange; } }
        public ClosedInterval ZRange { get { return _zRange; } }
        */

        private Vector4f _min, _max;

        public Vector4f Min { get { if (_surfaceArea < 0) throw new InvalidOperationException("Box is empty."); else return _min; } }
        public Vector4f Max { get { if (_surfaceArea < 0) throw new InvalidOperationException("Box is empty."); else return _max; } }
        public ClosedInterval XRange { get { return new ClosedInterval(_min.X, _max.X); } }
        public ClosedInterval YRange { get { return new ClosedInterval(_min.Y, _max.Y); } }
        public ClosedInterval ZRange { get { return new ClosedInterval(_min.Z, _max.Z); } }
        private float _surfaceArea;
        public float SurfaceArea { get { return _surfaceArea<0? 0 : _surfaceArea; } }
        public bool IsEmpty { get { return _surfaceArea < 0; } }

        public static readonly Box3 EMPTY = new Box3(1, -1, 1, 0, 1, 0);

        public Box3(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            if (minX <= maxX && minY <= maxY && minZ <= maxZ)
            {
                _min = new Vector4f(minX, minY, minZ, 0f);
                _max = new Vector4f(maxX, maxY, maxZ, 0f);
                float x = maxX - minX;
                float y = maxY - minY;
                float z = maxZ - minZ;
                _surfaceArea = 2 * (x * y + y * z + z * x);
            }
            else
            {
                _min = new Vector4f(float.NaN, float.NaN, float.NaN, 0);
                _max = new Vector4f(float.NaN, float.NaN, float.NaN, 0);
                _surfaceArea = -1;
            }
        }

        public Box3(Vector4f min, Vector4f max)
        {
            if (min.X <= max.X && min.Y <= max.Y && min.Z <= max.Z)
            {
                _min = min;
                _max = max;
                _min.W = 0;
                _max.W = 0;
                float x = max.X - min.X;
                float y = max.Y - min.Y;
                float z = max.Z - min.Z;
                _surfaceArea = 2 * (x * y + y * z + z * x);
            }
            else
            {
                _min = new Vector4f(float.NaN, float.NaN, float.NaN, 0);
                _max = new Vector4f(float.NaN, float.NaN, float.NaN, 0);
                _surfaceArea = -1;
            }
        }

        public Box3(params CVector3[] points)
        {
            if (points.Length == 0)
            {
                _min = new Vector4f(float.NaN, float.NaN, float.NaN, 0);
                _max = new Vector4f(float.NaN, float.NaN, float.NaN, 0);
                _surfaceArea = -1;
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
                _min = new Vector4f(minX, minY, minZ, 0f);
                _max = new Vector4f(maxX, maxY, maxZ, 0f);
                //_xRange = new ClosedInterval(minX, maxX);
                //_yRange = new ClosedInterval(minY, maxY);
                //_zRange = new ClosedInterval(minZ, maxZ);
                float x = maxX - minX;
                float y = maxY - minY;
                float z = maxZ - minZ;
                _surfaceArea = 2 * (x * y + y * z + z * x);
            }
        }

        public bool Contains(CVector3 point)
        {
            if (IsEmpty) return false;
            return _min.X <= point.x && _min.Y <= point.y && _min.Z <= point.z && _max.X >= point.x && _max.Y >= point.y && _max.Z >= point.z;
        }

        public CVector3 GetCenter()
        {
            return new CVector3((_min + _max) * new Vector4f(0.5f, 0.5f, 0.5f, 0f));
        }

        public CVector3 GetCenter2()
        {
            return new CVector3(_min + _max);
        }

        public CVector3 TripleMin()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Box is empty.");
            return new CVector3(_min);
        }

        public ClosedInterval IntersectSegment(CVector3 orig, CVector3 diff)
        {
            return IntersectInterval(orig, diff, new ClosedInterval(0, 1));
        }

        public ClosedInterval IntersectRay(CVector3 orig, CVector3 dir)
        {
            return IntersectInterval(orig, dir, ClosedInterval.POSITIVES);
        }

        public ClosedInterval IntersectLine(CVector3 orig, CVector3 dir)
        {
            return IntersectInterval(orig, dir, ClosedInterval.ALL);
        }

        public bool DoesIntersectSegment(CVector3 orig, CVector3 diff)
        {
            return DoesIntersectInterval(orig, diff, new ClosedInterval(0, 1));
        }

        public bool DoesIntersectRay(CVector3 orig, CVector3 dir)
        {
            return DoesIntersectInterval(orig, dir, ClosedInterval.POSITIVES);
        }

        public bool DoesIntersectLine(CVector3 orig, CVector3 dir)
        {
            return DoesIntersectInterval(orig, dir, ClosedInterval.ALL);
        }

        /*
         //This is 10x slower than the other implementation BECAUSE THE COMPILER IS BAD
        public ClosedInterval SlowIntersectInterval(CVector3 orig, CVector3 dir, ClosedInterval t_interval)
        {
            t_interval = t_interval & ((_xRange - orig.x) / dir.x);
            if (t_interval.IsEmpty) return t_interval;
            t_interval = t_interval & ((_yRange - orig.y) / dir.y);
            if (t_interval.IsEmpty) return t_interval;
            return t_interval & ((_zRange - orig.z) / dir.z);
        }
        

        public  ClosedInterval IntersectInterval(CVector3 orig, CVector3 dir, ClosedInterval t_interval)
        {
            ClosedInterval c1 = SlowIntersectInterval(orig, dir, t_interval);
            ClosedInterval c2 = FastIntersectInterval(orig, dir, t_interval);
            if ((c1.IsEmpty && c2.IsEmpty) || (c1.Min == c2.Min && c1.Max == c2.Max))
                return c1;
            SlowIntersectInterval(orig, dir, t_interval);
            FastIntersectInterval(orig, dir, t_interval);
            throw new Exception("asdflkjaslkf");
        }*/

        public ClosedInterval IntersectInterval(CVector3 orig, CVector3 dir, ClosedInterval t_interval)
        {
            float min=t_interval.Min, max=t_interval.Max;
            float pmin, pmax; //potential

            pmin = _min.X - orig.x;
            pmax = _max.X - orig.x;
            if (dir.x == 0 && (pmin > 0 || pmax < 0))
                return ClosedInterval.EMPTY;
            if (dir.x != 0)
            {
                pmin /= dir.x;
                pmax /= dir.x;
                if (dir.x < 0)
                {
                    float temp = pmin;
                    pmin = pmax;
                    pmax = temp;
                }
                if (pmin > min) min = pmin;
                if (pmax < max) max = pmax;
                if (pmin > pmax) return ClosedInterval.EMPTY;
            }

            pmin = _min.Y - orig.y;
            pmax = _max.Y - orig.y;
            if (dir.y == 0 && (pmin > 0 || pmax < 0))
                return ClosedInterval.EMPTY;
            if (dir.y != 0)
            {
                pmin /= dir.y;
                pmax /= dir.y;
                if (dir.y < 0)
                {
                    float temp = pmin;
                    pmin = pmax;
                    pmax = temp;
                }
                if (pmin > min) min = pmin;
                if (pmax < max) max = pmax;
                if (pmin > pmax) return ClosedInterval.EMPTY;
            }

            pmin = _min.Z - orig.z;
            pmax = _max.Z - orig.z;
            if (dir.z == 0 && (pmin > 0 || pmax < 0))
                return ClosedInterval.EMPTY;
            if (dir.z != 0)
            {
                pmin /= dir.z;
                pmax /= dir.z;
                if (dir.z < 0)
                {
                    float temp = pmin;
                    pmin = pmax;
                    pmax = temp;
                }
                if (pmin > min) min = pmin;
                if (pmax < max) max = pmax;
            }

            return new ClosedInterval(min,max);
        }
        
        public bool DoesIntersectInterval(CVector3 orig, CVector3 dir, ClosedInterval t_interval)
        {
            if (IsEmpty)
                return false;

            bool resp1 = DoesIntersectInterval_SIMD(orig, dir, t_interval);
            //bool resp2 = DoesIntersectInterval_Slow(orig, dir, t_interval);

            /*
            if (resp1 != resp2)
            {
                Console.WriteLine(resp1+" "+resp2);
                Console.WriteLine("{0} {1} {2}", orig, dir, t_interval);

                Vector4f dire = new Vector4f(1f / dir.x, 1f / dir.y, 1f / dir.z, 0);
                Vector4f ori = new Vector4f(orig.x, orig.y, orig.z, 0);

                Vector4f pmin0 = (_min - ori) * dire;
                Vector4f pmax0 = (_max - ori) * dire;
                pmin0.W = t_interval.Min;
                pmax0.W = t_interval.Max;

                Vector4f pmin = pmin0.Min(pmax0);
                Vector4f pmax = pmin0.Max(pmax0);

                Console.WriteLine("\n{0} {1}", dire.W, ori.W);
                Console.WriteLine((t_interval & ((XRange - orig.x) / dir.x)) + " " + (t_interval & ((YRange - orig.y) / dir.y)) + " " + (t_interval & ((ZRange - orig.z) / dir.z)));
                Console.WriteLine(pmin + " " + pmax + " " + pmin.CompareNotLessEqual(pmax) + " " + (pmin.CompareNotLessEqual(pmax) == Vector4f.Zero));

                throw new Exception("BAD SIMD!");
            }*/

            return resp1;
        }

        public bool DoesIntersectInterval_Slow(CVector3 orig, CVector3 dir, ClosedInterval t_interval)
        {
            float min = t_interval.Min, max = t_interval.Max;
            float pmin, pmax; //potential

            pmin = _min.X - orig.x;
            pmax = _max.X - orig.x;
            if (dir.x == 0 && (pmin > 0 || pmax < 0))
                return false;
            if (dir.x != 0)
            {
                pmin /= dir.x;
                pmax /= dir.x;
                if (dir.x < 0)
                {
                    float temp = pmin;
                    pmin = pmax;
                    pmax = temp;
                }
                if (pmin > min) min = pmin;
                if (pmax < max) max = pmax;
                if (pmin > pmax) return false;
            }

            pmin = _min.Y - orig.y;
            pmax = _max.Y - orig.y;
            if (dir.y == 0 && (pmin > 0 || pmax < 0))
                return false;
            if (dir.y != 0)
            {
                pmin /= dir.y;
                pmax /= dir.y;
                if (dir.y < 0)
                {
                    float temp = pmin;
                    pmin = pmax;
                    pmax = temp;
                }
                if (pmin > min) min = pmin;
                if (pmax < max) max = pmax;
                if (pmin > pmax) return false;
            }

            pmin = _min.Z - orig.z;
            pmax = _max.Z - orig.z;
            if (dir.z == 0 && (pmin > 0 || pmax < 0))
                return false;
            if (dir.z != 0)
            {
                pmin /= dir.z;
                pmax /= dir.z;
                if (dir.z < 0)
                {
                    float temp = pmin;
                    pmin = pmax;
                    pmax = temp;
                }
                if (pmin > min) min = pmin;
                if (pmax < max) max = pmax;
            }

            return max >= min;
        }

        private bool DoesIntersectInterval_SIMD(CVector3 origin, CVector3 direction, ClosedInterval t_interval)
        {
            if(direction.x !=0 && direction.y!=0 && direction.z!=0)
            {
                Vector4f dir = new Vector4f(direction.x, direction.y, direction.z, 1f);
                Vector4f ori = new Vector4f(origin.x, origin.y, origin.z, 1f);
                
                Vector4f pmin0 = ( _min - ori) / dir;
                Vector4f pmax0 = ( _max - ori) / dir;
                pmin0.W = t_interval.Min;
                pmax0.W = t_interval.Max;

                Vector4f pmin = pmin0.Min(pmax0);
                Vector4f pmax = pmin0.Max(pmax0);

                //Console.WriteLine("\n{0} {1}", dir.W, ori.W);
                //Console.WriteLine((t_interval & ((XRange - origin.x) / direction.x))+" "+(t_interval & ((YRange - origin.y) / direction.y))+" "+(t_interval & ((ZRange - origin.z) / direction.z)));
                //Console.WriteLine(pmin + " " + pmax + " " + pmin.CompareNotLessEqual(pmax) + " " + (pmin.CompareNotLessEqual(pmax) == Vector4f.Zero));
                
                if (pmin.CompareNotLessEqual(pmax) != Vector4f.Zero) return false;
                pmin = pmin.Shuffle(ShuffleSel.RotateRight);
                if (pmin.CompareNotLessEqual(pmax) != Vector4f.Zero) return false;
                pmin = pmin.Shuffle(ShuffleSel.RotateRight);
                if (pmin.CompareNotLessEqual(pmax) != Vector4f.Zero) return false;
                pmin = pmin.Shuffle(ShuffleSel.RotateRight);
                return pmin.CompareNotLessEqual(pmax) == Vector4f.Zero;
            }
            return DoesIntersectInterval_Slow(origin, direction, t_interval);
        }
        
        public static Box3 operator |(Box3 a, Box3 b)
        {
            if (a.IsEmpty) return b;
            if (b.IsEmpty) return a;
            return new Box3(a._min.Min(b._min), a._max.Max(b._max));
        }

        public static Box3 operator &(Box3 a, Box3 b)
        {
            if (a.IsEmpty || b.IsEmpty) return EMPTY;
            return new Box3(a._min.Max(b._min), a._max.Min(b._max));
        }

        public static bool operator <=(Box3 a, Box3 b)
        {
            if (a.IsEmpty) return true;
            return a._min.X >= b._min.X && a._min.Y >= b._min.Y && a._min.Z >= b._min.Z && a._max.X <= b._max.X && a._max.Y <= b._max.Y && a._min.Z <= b._max.Z;
        }

        public static bool operator >=(Box3 b, Box3 a)
        {
            if (a.IsEmpty) return true;
            return a._min.X >= b._min.X && a._min.Y >= b._min.Y && a._min.Z >= b._min.Z && a._max.X <= b._max.X && a._max.Y <= b._max.Y && a._min.Z <= b._max.Z;
        }
        /*
        public static Box3 operator |(Box3 a, CVector3 b)
        {
            return new Box3(a._xRange | b.x, a._yRange | b.y, a._zRange | b.z);
        }
        
        public override bool Equals(Object obj)
        {
            if (!(obj is Box3))
                return false;
            Box3 b = (Box3)obj;
            return _xRange.Min == b._xRange.Min && _xRange.Max == b._xRange.Max &&
                _yRange.Min == b._yRange.Min && _yRange.Max == b._yRange.Max &&
                _zRange.Min == b._zRange.Min && _zRange.Max == b._zRange.Max;
        }

        public override int GetHashCode()
        {
            return (int)((_xRange.Min + _xRange.Max + _yRange.Min + _yRange.Max + _zRange.Min + _zRange.Max)*int.MaxValue);
        }*/

        public override string ToString()
        {
            if (IsEmpty)
                return "[E]";
            return String.Format("<[{0}, {1}]x[{2}, {3}]x[{4}, {5}]>", _min.X, _max.X, _min.Y, _max.Y, _min.Z, _max.Z);
        }
    }
}
