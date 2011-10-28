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
        public RayCast[] Rays { get { return rays; } }

        private RayCast[] rays;

        private RaySet()
        {
        }

        public static RaySet[] ReadFromFile(Stream file)
        {
            List<RayCast>[] sets = new List<RayCast>[0];

            StreamReader read = new StreamReader(file);
            string line;

            int count = 0;
            while ((line = read.ReadLine())!=null)
            {
                if ((count++ & 7) != 0)
                    continue;
                string[] a = line.Split(' ');
                CVector3 origin = new CVector3() { x = float.Parse(a[2]), y = float.Parse(a[3]), z = float.Parse(a[4]) };
                CVector3 end = new CVector3() { x = float.Parse(a[5]) + origin.x, 
                                                y = float.Parse(a[6]) + origin.y, 
                                                z = float.Parse(a[7]) + origin.z };
                int depth = int.Parse(a[1]);
                RayCast cast; 
                if (a[0].Equals("hit"))
                {
                    cast = new RayCast() { Hit = true, Depth=depth, Origin = origin, End = end };
                }
                else if (a[0].Equals("miss"))
                {
                    cast = new RayCast() { Hit = false, Depth=depth, Origin = origin, End = end };
                }
                else
                {
                    throw new Exception("Error parsing file.");
                }

                if (sets.Length <= cast.Depth)
                {
                    List<RayCast>[] sets2 = new List<RayCast>[cast.Depth+1];
                    Array.Copy(sets, sets2, sets.Length);
                    for (int k = sets.Length; k < sets2.Length; k++)
                        sets2[k] = new List<RayCast>();
                    sets = sets2;
                }

                sets[cast.Depth].Add(cast);
            }

            RaySet[] ret = new RaySet[sets.Length];
            for (int k = 0; k < ret.Length; k++)
            {
                ret[k] = new RaySet();
                ret[k].rays = sets[k].ToArray();
            }

            return ret;
        }
    }
}
