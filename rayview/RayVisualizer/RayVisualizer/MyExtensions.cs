using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using RayVisualizer.Common;

namespace RayVisualizer
{
    public static class MyExtensions
    {
        public static CVector3 ToC(this Vector3 v)
        {
            return new CVector3(v.X, v.Y, v.Z);
        }
        public static Vector3 ToGL(this CVector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
    }
}
