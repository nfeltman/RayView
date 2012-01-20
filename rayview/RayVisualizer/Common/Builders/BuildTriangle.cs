using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public class BuildTriangle
    {
        public Triangle t;
        public CVector3 center;
        public int index;

        public BuildTriangle(Triangle init, int loc)
        {
            t = init;
            index = loc;
            Box3 box = new Box3(init.p1, init.p2, init.p3);
            center = box.GetCenter();
        }
    }
    
}
