﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;
using OpenTK.Graphics.OpenGL;

namespace RayVisualizer
{
    public class IntersectionsCrossplane : CrossplaneBehavior
    {
        private List<CVector3> Hits { get; set; }
        private List<CVector3> Misses { get; set; }

        public IntersectionsCrossplane()
        {
            Hits = new List<CVector3>();
            Misses = new List<CVector3>();
        }

        public void UpdateCrossPlane(SceneData scene)
        {
            if (!scene.CrossPlaneFrozen)
            {
                Hits = new List<CVector3>();
                Misses = new List<CVector3>();
                CVector3 n = scene.ForwardVec.ToC();
                CVector3 p = scene.Location.ToC() + (n * scene.CrossPlaneDist);

                //foreach(RaySet set in scene.Rays)
                foreach (RayCast c in scene.Rays[1].Rays)
                {
                    if (c.Hit)
                    {
                        //test if the start and end are strictly on opposite sides of the plane
                        if (((c.Origin - p) * n) * ((c.End - p) * n) < 0)
                        {
                            //compute plane-line intersection
                            CVector3 d = c.End - c.Origin;
                            float t = ((p - c.Origin) * n) / (d * n);
                            CVector3 q = d * t + c.Origin;
                            Hits.Add(q);
                        }
                    }
                    else
                    {
                        CVector3 d = c.End - c.Origin;
                        //test for ray-plane intersection
                        if (((c.Origin - p) * n) * (d * n) < 0)
                        {
                            //compute plane-line intersection
                            float t = ((p - c.Origin) * n) / (d * n);
                            CVector3 q = d * t + c.Origin;
                            Misses.Add(q);
                        }
                    }
                }
            }
        }
        public void DrawResults(SceneData scene)
        {
            GL.PointSize(4);
            GL.Begin(BeginMode.Points);
            GL.Color4(0, 0, 1, .4);
            foreach (CVector3 p in Hits)
            {
                GL.Vertex3(p.ToGL());
            }
            GL.Color4(0, 1, 0, .4);
            foreach (CVector3 p in Misses)
            {
                GL.Vertex3(p.ToGL());
            }
            GL.End();
        }
    }
}
