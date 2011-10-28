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
        private int[,] bins;
        private float step;
        CVector3 right, up, center; 
        List<CVector3> traffic;

        public void UpdateCrossPlane(SceneData scene)
        {
            traffic = new List<CVector3>();

            CVector3 n = scene.ForwardVec.ToC();
            CVector3 p = scene.Location.ToC() + (n * scene.CrossPlaneDist);

            right = scene.RightVec.ToC();
            up = right ^ n;

            float minU = float.MaxValue;
            float maxU = float.MinValue;
            float minV = float.MaxValue;
            float maxV = float.MinValue;

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
                    traffic.Add(q);
                    float u = (q - p) * right;
                    float v = (q - p) * up;
                    minU = Math.Min(minU, u);
                    maxU = Math.Max(maxU, u);
                    minV = Math.Min(minV, v);
                    maxV = Math.Max(maxV, v);
                }
            }

            step = Math.Max(maxU - minU, maxV - minV)/20;
            float medU = (minU + maxU) / 2;
            float medV = (minV + maxV) / 2;
            center = p + (medU * right) + (medV * up);

            bins = new int[21,21];

            foreach (CVector3 q in traffic)
            {
                int u = (int)(((((q - p) * right)-medU)/step)+0.5);
                int v = (int)(((((q - p) * up)-medV)/step)+0.5);
                bins[u+10,v+10]++;
            }
        }

        public void DrawResults(SceneData scene)
        {
            GL.PointSize(20);
            GL.Begin(BeginMode.Points);
            GL.Color4(0, 0, 1, .4);
            if (bins != null)
            {
                //foreach (CVector3 q in traffic)
                //{
                    //GL.Vertex3(q.ToGL());
                //}
                for(int k=0;k<21;k++)
                    for(int j=0;j<21;j++)
                        if (bins[k, j] > 0)
                        {
                            //Console.WriteLine(k+" "+j);
                            GL.Vertex3((center + ((k-10)*step*right) + ((j-10)*step*up)).ToGL());
                        }
            }

            GL.End();
        }
    }
}
