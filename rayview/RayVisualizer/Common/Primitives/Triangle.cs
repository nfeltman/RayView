using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        }

        public bool IntersectsSegment(CVector3 origin, CVector3 difference)
        {
            float res = IntersectRay(origin, difference);
            return !float.IsNaN(res) && res < 1;
        }
    }
}
