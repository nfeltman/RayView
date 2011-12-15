using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class RayDistributions
    {
        public static Ray3[] AlignedCircularFrustrum(CVector3 c1, CVector3 c2, float r1, float r2, int n)
        {
            Ray3[] ans = new Ray3[n];
            Random rng = new Random(19890203);
            CVector3 l = c2 - c1;
            CVector3 u = (new CVector3((float)rng.NextDouble(), (float)rng.NextDouble(), 0) ^ l).Normalized(); // something perpendicular to d
            CVector3 v = (u ^ l).Normalized(); // something perpendicular to u and d


            for (int k = 0; k < n; k++)
            {
                double theta = 2 * Math.PI * rng.NextDouble();
                float r = (float)Math.Sqrt(rng.NextDouble());
                CVector3 d = u * ((float)Math.Sin(theta) * r) + v * ((float)Math.Cos(theta) * r);
                ans[k] = new Ray3(c1 + d * r1, l + d * (r2 - r1));
            }
            return ans;
        }
    }
}
