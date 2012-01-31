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

    public class RaysViewer : Viewable
    {
        private IEnumerable<Segment3> _queries;

        public RaysViewer(IEnumerable<Segment3> queries)
        {
            _queries = queries;
        }

        public RaysViewer(IEnumerable<Ray3> queries, float len)
        {
            _queries = queries.Select(ray => new Segment3(ray.Origin, ray.Direction * len));
        }

        public void DrawTransparentPart()
        {
            GL.Color4(.3, 0, 0, .1);
            GL.Begin(BeginMode.Lines);
            //int counter = 0;
            foreach (Segment3 c in _queries)
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
        public TreeNode<BVH2Branch,BVH2Leaf> BVH { get; set; }

        public BVHTriangleViewer(BVH2 bvh)
        {
            BVH = bvh.Root;
        }

        public BVHTriangleViewer(TreeNode<BVH2Branch, BVH2Leaf> bvh)
        {
            BVH = bvh;
        }

        public void DrawOpaquePart()
        {
            GL.Begin(BeginMode.Triangles);
            BVH.Accept((NodeVisitor<Unit, BVH2Branch, BVH2Leaf>)new CollectTrianglesVisitor(t =>
            {
                float[] vals = { t.p1.x, t.p1.x, t.p1.y, t.p2.x, t.p2.y, t.p2.z, t.p3.x, t.p3.y, t.p3.z };
                byte[] hash = new byte[4];
                int shifty = 0;
                byte[] flippy = { 125, 34, 2, 213, 199, 226, 70};
                int flipdex=0;
                foreach (float v in vals)
                {
                    byte[] asBytes = BitConverter.GetBytes(v);
                    for (int k = 0; k < 4; k++)
                    {
                        hash[k] += (byte)(((asBytes[k] >> shifty) + (asBytes[k] << (8 - shifty))) ^ flippy[flipdex]);
                        shifty = (shifty + 3) & 7;
                        flipdex++; if (flipdex > 6) flipdex = 0;
                    }
                }

                GL.Color4((hash[0] ^ hash[1]) / 512f + .1f, (hash[2] ^ hash[3]) / 512f + .4f, .8f, 1);
                GL.Vertex3(t.p1.ToGL());
                GL.Vertex3(t.p2.ToGL());
                GL.Vertex3(t.p3.ToGL());
            }));
            GL.End();
        }

        public void DrawTransparentPart() { }
    }

    public class RBVHTriangleViewer : Viewable
    {
        public TreeNode<RBVH2Branch, RBVH2Leaf> RBVH { get; set; }

        public RBVHTriangleViewer(RBVH2 rbvh)
        {
            RBVH = rbvh.Root;
        }

        public RBVHTriangleViewer(TreeNode<RBVH2Branch, RBVH2Leaf> rbvh)
        {
            RBVH = rbvh;
        }

        public void DrawOpaquePart()
        {
            GL.Begin(BeginMode.Triangles);
            RBVH.Accept((NodeVisitor<Unit, RBVH2Branch, RBVH2Leaf>)new CollectTrianglesAndHotnessVisitor((h,t) =>
            {
                float[] vals = { t.p1.x, t.p1.x, t.p1.y, t.p2.x, t.p2.y, t.p2.z, t.p3.x, t.p3.y, t.p3.z };
                byte[] hash = new byte[4];
                int shifty = 0;
                byte[] flippy = { 125, 34, 2, 213, 199, 226, 70 };
                int flipdex = 0;
                foreach (float v in vals)
                {
                    byte[] asBytes = BitConverter.GetBytes(v);
                    for (int k = 0; k < 4; k++)
                    {
                        hash[k] += (byte)(((asBytes[k] >> shifty) + (asBytes[k] << (8 - shifty))) ^ flippy[flipdex]);
                        shifty = (shifty + 3) & 7;
                        flipdex++; if (flipdex > 6) flipdex = 0;
                    }
                }
                
                //GL.Color4((hash[0] ^ hash[1]) / 512f + .1f, (hash[2] ^ hash[3]) / 512f + .4f, .8f, 1);
                GL.Color4(h, h, .8, 1);
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
