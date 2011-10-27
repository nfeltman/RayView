using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RayVisualizer.Common
{
    public struct CVector3
    {
        public float x, y, z;
    }
    public class RayCast
    {
        public bool Hit { get; set; }
        public int Depth { get; set; }
        public int ObjID { get; set; }
        public CVector3 Origin { get; set; }
        public CVector3 End { get; set; } //unit vector if no hit
    }
    public class RaySet
    {
        public IList<RayCast> Rays { get { return rays; } }

        private IList<RayCast> rays;

        public RaySet()
        {
            rays = new List<RayCast>();
        }

        public static RaySet ReadFromFile(Stream file)
        {
            RaySet set = new RaySet();
            StreamReader read = new StreamReader(file);
            string line;

            int count = 0;
            while ((line = read.ReadLine())!=null)
            {
                if ((count++ & 7) != 0)
                    continue;
                string[] a = line.Split(' ');
                CVector3 origin = new CVector3() { x = float.Parse(a[2]), y = float.Parse(a[3]), z = float.Parse(a[4]) };
                CVector3 end = new CVector3() { x = float.Parse(a[5]), y = float.Parse(a[6]), z = float.Parse(a[7]) };
                int depth = int.Parse(a[1]);
                if(depth>0)
                if (a[0].Equals("hit"))
                {
                    set.Rays.Add(new RayCast() { Hit = true, Depth=depth, Origin = origin, End = end });
                }
                else if (a[0].Equals("miss"))
                {
                    set.Rays.Add(new RayCast() { Hit = false, Depth=depth, Origin = origin, End = end });
                }
                else
                {
                    throw new Exception("Error parsing file.");
                }
            }

            return set;
        }
    }
}
