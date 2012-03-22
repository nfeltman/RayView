using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Simd;

namespace RayVisualizer.Common
{
    public struct CVector3
    {
        private Vector4f _vec;
        public float x { get { return _vec.X; } }
        public float y { get { return _vec.Y; } }
        public float z { get { return _vec.Z; } }
        public Vector4f Vec { get { return _vec; } }

        public CVector3(float x0, float y0, float z0)
        {
            _vec = new Vector4f(x0, y0, z0, 0);
        }
        public CVector3(Vector4f vec)
        {
            _vec = vec;
        }
        public CVector3 Normalized()
        {
            return this / Length();
        }
        public float Length()
        {
            Vector4f sq = _vec * _vec;
            return (float)Math.Sqrt(sq.X + sq.Y + sq.Z);
        }
        public float LengthSq()
        {
            Vector4f sq = _vec * _vec;
            return sq.X + sq.Y + sq.Z;
        }
        public static CVector3 operator +(CVector3 v1, CVector3 v2)
        {
            return new CVector3(v1._vec + v2._vec);
        }
        public static CVector3 operator -(CVector3 v1, CVector3 v2)
        {
            return new CVector3(v1._vec - v2._vec);
        }
        public static CVector3 operator ^(CVector3 v1, CVector3 v2)
        {
            return new CVector3(v1.y * v2.z - v1.z * v2.y, v1.z * v2.x - v1.x * v2.z, v1.x * v2.y - v1.y * v2.x);
        }
        public static float operator *(CVector3 v1, CVector3 v2)
        {
            Vector4f dot = v1._vec * v2._vec;
            return dot.X + dot.Y + dot.Z;
        }
        public static CVector3 operator *(float c, CVector3 v)
        {
            return new CVector3(v._vec * c);
        }
        public static CVector3 operator *(CVector3 v, float c)
        {
            return new CVector3(v._vec * c);
        }
        public static CVector3 operator /(CVector3 v, float c)
        {
            return new CVector3(v.x / c, v.y / c, v.z / c);
        }
        public static bool operator ==(CVector3 p1, CVector3 p2)
        {
            return p1.x == p2.x && p1.y == p2.y && p1.z == p2.z;
        }
        public static bool operator !=(CVector3 p1, CVector3 p2)
        {
            return p1.x != p2.x || p1.y != p2.y || p1.z != p2.z;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is CVector3))
                return false;
            CVector3 other = (CVector3)obj;
            return other.x == x && other.y == y && other.z == z;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode() + z.GetHashCode();
        }
        public override string ToString()
        {
            return String.Format("({0:0.0000000000}, {1:0.0000000000}, {2:0.0000000000})", x, y, z);
        }
    }

    public struct Segment3
    {
        private Vector4f _origin, _difference;
        public Vector4f Origin { get { return _origin; } }
        public Vector4f Difference { get { return _difference; } }

        public Segment3(Vector4f origin, Vector4f difference)
        {
            _origin = origin;
            _difference = difference;
        }

        public Segment3(CVector3 origin, CVector3 difference)
        {
            _origin = origin.Vec;
            _difference = difference.Vec;
        }
    }
    public struct Ray3
    {
        private Vector4f _origin, _direction;
        public Vector4f Origin { get { return _origin; } }
        public Vector4f Direction { get { return _direction; } }

        public Ray3(Vector4f origin, Vector4f direction)
        {
            _origin = origin;
            _direction = direction;
        }

        public Ray3(CVector3 origin, CVector3 direction)
        {
            _origin = origin.Vec;
            _direction = direction.Vec;
        }
    }
}
