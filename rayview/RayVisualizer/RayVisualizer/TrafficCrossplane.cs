using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;
using OpenTK.Graphics.OpenGL;

namespace RayVisualizer
{
    class TrafficCrossplane : CrossplaneBehavior
    {
        private Dictionary<Tuple<int,int>,int> bins;
        CVector3 right, up, p;

        private const float BINSIZE = 10;

        public void UpdateCrossPlane(SceneData scene)
        {
            bins = new Dictionary<Tuple<int, int>, int>();

            CVector3 n = scene.ForwardVec.ToC();
            p = scene.Location.ToC() + (n * scene.CrossPlaneDist);

            right = scene.RightVec.ToC();
            up = right ^ n;

            //foreach(RaySet set in scene.Rays)
            foreach (RayCast c in scene.Rays[1].Rays)
            {
                float a1 = (c.Origin - p) * n;
                CVector3 d = c.End - c.Origin;
                if ((c.Hit && a1 * ((c.End - p) * n) < 0 ) || (!c.Hit && a1 * (d * n) < 0))
                {
                    //compute plane-line intersection
                    float t = -a1 / (d * n);
                    CVector3 q = d * t + c.Origin;
                    float u = (q - p) * right;
                    float v = (q - p) * up;
                    int ubin = (int)(u / BINSIZE + .5);
                    int vbin = (int)(v / BINSIZE + .5);
                    Tuple<int,int> bin = new Tuple<int,int>(ubin,vbin);
                    if (bins.ContainsKey(bin))
                        bins[bin] = bins[bin] + 1;
                    else
                        bins[bin] = 1;
                }
            }
        }

        public void DrawResults(SceneData scene)
        {
            GL.PointSize(20);
            GL.Begin(BeginMode.Quads);
            if (bins != null)
            {
                foreach (KeyValuePair<Tuple<int,int>,int> kv in bins)
                {
                    Tuple<int, int> key = kv.Key;
                    float val = Math.Min(kv.Value / 10f, 1);
                    GL.Color4(val, 1-val, 0, .4);
                    GL.Vertex3((((key.Item1) * BINSIZE * right) + ((key.Item2) * BINSIZE * up) + p).ToGL());
                    GL.Vertex3((((key.Item1) * BINSIZE * right) + ((key.Item2-1) * BINSIZE * up) + p).ToGL());
                    GL.Vertex3((((key.Item1-1) * BINSIZE * right) + ((key.Item2-1) * BINSIZE * up) + p).ToGL());
                    GL.Vertex3((((key.Item1-1) * BINSIZE * right) + ((key.Item2) * BINSIZE * up) + p).ToGL());
                }
            }

            GL.End();
        }
    }
}