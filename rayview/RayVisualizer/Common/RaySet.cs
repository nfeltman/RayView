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
    public class Ray
    {
        public CVector3 Origin { get; set; }
        public CVector3 Direction { get; set; }
    }
    public class Hit
    {
        public int ObjID { get; set; }
        public CVector3 loc { get; set; }
    }
    public class RaySet
    {
        public IList<Ray> Rays { get { return rays; } }
        public IList<Hit> Hits { get { return hits; } }

        private IList<Ray> rays;
        private IList<Hit> hits;

        public RaySet()
        {
            rays = new List<Ray>();
            hits = new List<Hit>();
        }

        public static RaySet ReadFromFile(Stream file)
        {
            RaySet set = new RaySet();
            StreamReader read = new StreamReader(file);
            string line;

            int count = 0;
            while ((line = read.ReadLine())!=null)
            {
                if ((count++ & 31) == 0)
                    continue;
                string[] a = line.Split(' ');
                set.Rays.Add(new Ray() {    Origin = new CVector3() { x=float.Parse(a[0]), y = float.Parse(a[1]), z = float.Parse(a[2])},
                                            Direction = new CVector3() { x = float.Parse(a[3]), y = float.Parse(a[4]), z = float.Parse(a[5]) }});
                set.Hits.Add(null);
            }

            return set;
        }
    }
}
