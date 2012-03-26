using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Simd;

namespace RayVisualizer.Common
{
    public struct Triangle
    {
        public CVector3 p1, p2, p3;

        public Triangle(CVector3 point1, CVector3 point2, CVector3 point3)
        {
            p1 = point1;
            p2 = point2;
            p3 = point3;
        }

        public float IntersectRay(CVector3 origin, CVector3 direction)
        {
            throw new Exception("probably bad implementation! don't use this!");
            /*
            // form a tetrahedron with the triangle and the ray origin
            CVector3 e1 = p1 - origin;
            CVector3 e2 = p2 - origin;
            CVector3 e3 = p3 - origin;

            // find normals to all of the non-triangle sides of the tetrahedron
            // calulate intersections between those normals and the direction vector
            float val1 = (e1 ^ e2) * direction;
            float val2 = (e2 ^ e3) * direction;
            float val3 = (e3 ^ e1) * direction;
            
            // if any pair of dotproducts have opposite sign, it misses
            if (val1 * val2 < 0 || val2 * val3 < 0 || val3 * val1 < 0)
                return float.NaN;

            // find the actual intersection location
            CVector3 norm = (p1 - p3) ^ (p2 - p3);
            float t = ((p1 - origin) * norm) / (direction * norm);
                        
            return t;
             */
        }
        
        public float IntersectLine(CVector3 origin, CVector3 direction, ClosedInterval interval)
        {
            CVector3 edge0 = p1 - p3;
            CVector3 edge1 = p3 - p2;
            CVector3 normal = edge0 ^ edge1;
            float rcp = 1.0f / (normal * direction);
            CVector3 edge2 = p3 - origin;
            float t = (normal * edge2) * rcp;
            if (t > interval.Max || t < interval.Min)
                return float.NaN;
            CVector3 interm = edge2 ^ direction;
            float u = (interm * edge1) * rcp;
            if (u < 0.0f)
                return float.NaN;
            float v = (interm * edge0) * rcp;
            if (u + v > 1.0f || v < 0.0f)
                return float.NaN;

            return t;
        }

        public bool IntersectsSegment(CVector3 origin, CVector3 difference)
        {
            const float T_EPSILON = 0.0001f;
            float res = IntersectLine(origin, difference, new ClosedInterval(T_EPSILON, 1 - T_EPSILON));
            return !float.IsNaN(res);
        }

        public bool IntersectsSegment(Vector4f origin, Vector4f difference)
        {
            const float T_EPSILON = 0.0001f;
            float res = IntersectLine(new CVector3(origin), new CVector3(difference), new ClosedInterval(T_EPSILON, 1 - T_EPSILON));
            return !float.IsNaN(res);
        }

        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}]", p1, p2, p3);
        }
    }
}
