using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public static class Shapes
    {
        /// <summary>
        /// Builds a rhombus between points p, p+u, p+v, and p+u+v with a triangles on the u side and b triangles on the v side.  Has 2ab triangles in total.
        /// </summary>
        public static Triangle[] BuildParallelogram(CVector3 p, CVector3 u, CVector3 v, int a, int b)
        {
            Triangle[] tris = new Triangle[2 * a * b];
            CVector3 x = u / a;
            CVector3 y = v / a;
            for (int k = 0; k < a; k++)
            {
                for (int j = 0; j < b; j++)
                {
                    CVector3 t1 = p + (k * x) + (j * y);
                    CVector3 t2 = t1 + x;
                    CVector3 t3 = t1 + y;
                    CVector3 t4 = t2 + y;
                    tris[(k + j * a) * 2] = new Triangle(t1, t2, t3);
                    tris[(k + j * a) * 2 + 1] = new Triangle(t3, t2, t4);
                }
            }
            return tris;
        }

        /// <summary>
        /// Builds a sphere centered at center, including top as a vertex, and with 'a' rings on a side.  Has 12a^2 triangles in total.
        /// </summary>
        public static Triangle[] BuildSphere(CVector3 center, CVector3 radius, CVector3 orientation, int a)
        {
            Triangle[] tris = new Triangle[12 * a * a];

            HemisphereHelper(tris, center, radius, orientation, a);

            // make the other half of the sphere by relfecting the triangles through the center
            int halfNumTris = 6 * a *a;
            CVector3 c2 = center * 2;
            for (int k = 0; k < halfNumTris; k++)
            {
                tris[k + halfNumTris] = new Triangle(c2 - tris[k].p1, c2 - tris[k].p2, c2 - tris[k].p3);
            }

            return tris;
        }

        public static Triangle[] BuildHemisphere(CVector3 center, CVector3 radius, CVector3 orientation, int a)
        {
            Triangle[] tris = new Triangle[6 * a * a];
            HemisphereHelper(tris, center, radius, orientation, a);
            return tris;
        }
        private static void HemisphereHelper(Triangle[] tris, CVector3 center, CVector3 radius, CVector3 orientation, int a)
        {

            CVector3 v2 = (orientation ^ radius).Normalized() * radius.Length();
            CVector3 v1 = (v2 ^ radius).Normalized() * radius.Length();

            //loop variants
            CVector3[] insideRing = new CVector3[] { center + radius, center + radius };
            float cumuH = 1;

            for (int k = 0; k < a; k++)
            {
                cumuH -= (2f * k + 1f) / (a * a);
                CVector3 circleCenter = radius * cumuH + center;
                float d = (float)Math.Sqrt(1 - cumuH * cumuH);
                int numPoints = 6 * (k + 1);
                CVector3[] outsideRing = new CVector3[numPoints + 1];
                for (int j = 0; j < numPoints; j++)
                {
                    double theta = 2 * Math.PI * j / numPoints;
                    outsideRing[j] = v1 * (d * (float)Math.Cos(theta)) + v2 * (d * (float)Math.Sin(theta)) + circleCenter;
                }
                outsideRing[numPoints] = outsideRing[0];
                int priorTris = 6 * k * k;
                // iterate over all six sectors; k*2+1 triangles per sector
                for (int sec = 0; sec < 6; sec++)
                {
                    // inward-pointing triangles
                    for (int j = 0; j <= k; j++)
                    {
                        tris[j + (k * 2 + 1) * sec + priorTris] = new Triangle(insideRing[sec * k + j], outsideRing[sec * (k + 1) + j], outsideRing[sec * (k + 1) + j + 1]);
                    }
                    //outward-pointing triangles
                    for (int j = 0; j < k; j++)
                    {
                        tris[j + (k * 2 + 1) * sec + priorTris + k + 1] = new Triangle(insideRing[sec * k + j + 1], insideRing[sec * k + j], outsideRing[sec * (k + 1) + j + 1]);
                    }
                }

                insideRing = outsideRing;
            }
        }
    }
}