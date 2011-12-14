using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;
using OpenTK.Graphics.OpenGL;
using RayVisualizer.Common.BVH2Visitors;

namespace RayVisualizer
{
    public interface Viewable
    {
        void DrawOpaquePart();
        void DrawTransparentPart();
    }

    public class CastHitsViewer : Viewable
    {
        private IEnumerable<CastHitQuery> _queries;

        public CastHitsViewer(IEnumerable<CastHitQuery> queries)
        {
            _queries = queries;
        }

        public void DrawTransparentPart()
        {
            GL.Color4(.3, 0, 0, .1);
            GL.Begin(BeginMode.Lines);
            //int counter = 0;
            foreach (CastHitQuery c in _queries)
            {
               // if ((counter++ & 15) != 0) continue;
                GL.Vertex3(c.Origin.x, c.Origin.y, c.Origin.z);
                CVector3 end = c.Origin + c.Difference;
                GL.Vertex3(end.x, end.y, end.z);
            }
            GL.End();
        }

        public void DrawOpaquePart(){}
    }

    public class BVHTriangleViewer : Viewable
    {
        private BVH2Node _bvh;

        public BVHTriangleViewer(BVH2 bvh)
        {
            _bvh = bvh.Root;
        }

        public BVHTriangleViewer(BVH2Node bvh)
        {
            _bvh = bvh;
        }

        public void DrawOpaquePart()
        {
            GL.Begin(BeginMode.Triangles);
            _bvh.Accept(new CollectTrianglesVisitor(t=>
            {
                CVector3  v= 100*(t.p1+t.p2+t.p3);
                GL.Color4(.5, (Math.Sin(v.x+v.y+v.z)+1)/2, 1, 1);
                GL.Vertex3(t.p1.ToGL());
                GL.Vertex3(t.p2.ToGL());
                GL.Vertex3(t.p3.ToGL());
            }));
            GL.End();
        }

        public void DrawTransparentPart() { }
    }

    public class TriangleViewer : Viewable
    {
        private IEnumerable<Triangle> _tris;

        public TriangleViewer(IEnumerable<Triangle> tris)
        {
            _tris = tris;
        }

        public TriangleViewer(IEnumerable<BuildTriangle> tris)
        {
            _tris = tris.Select(bt=>bt.t);
        }

        public void DrawOpaquePart()
        {
            GL.Begin(BeginMode.Triangles);
            foreach(Triangle t in _tris)
            {
                GL.Vertex3(t.p1.ToGL());
                GL.Vertex3(t.p2.ToGL());
                GL.Vertex3(t.p3.ToGL());
            };
            GL.End();
        }

        public void DrawTransparentPart() { }
    }
}
