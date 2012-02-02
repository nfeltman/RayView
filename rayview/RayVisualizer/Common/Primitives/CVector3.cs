using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public struct CVector3
    {
        public float x, y, z;
        public CVector3(float x0, float y0, float z0)
        {
            x = x0;
            y = y0;
            z = z0;
        }
        public CVector3 Normalized()
        {
            return this / Length();
        }
        public float Length()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }
        public float LengthSq()
        {
            return x * x + y * y + z * z;
        }
        public static CVector3 operator +(CVector3 v1, CVector3 v2)
        {
            return new CVector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }
        public static CVector3 operator -(CVector3 v1, CVector3 v2)
        {
            return new CVector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }
        public static CVector3 operator ^(CVector3 v1, CVector3 v2)
        {
            return new CVector3(v1.y * v2.z - v1.z * v2.y, v1.z * v2.x - v1.x * v2.z, v1.x * v2.y - v1.y * v2.x);
        }
        public static float operator *(CVector3 v1, CVector3 v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }
        public static CVector3 operator *(float c, CVector3 v)
        {
            return new CVector3(v.x * c, v.y * c, v.z * c);
        }
        public static CVector3 operator *(CVector3 v, float c)
        {
            return new CVector3(v.x * c, v.y * c, v.z * c);
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

    public class Segment3
    {
        public CVector3 Origin { get; set; }
        public CVector3 Difference { get; set; }

        public Segment3(CVector3 origin, CVector3 difference)
        {
            Origin = origin;
            Difference = difference;
        }
    }
    public class Ray3
    {
        public CVector3 Origin { get; set; }
        public CVector3 Direction { get; set; }

        public Ray3(CVector3 origin, CVector3 direction)
        {
            Origin = origin;
            Direction = direction;
        }
    }
}
