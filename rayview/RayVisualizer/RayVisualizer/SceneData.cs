using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using RayVisualizer.Common;
using System.IO;

namespace RayVisualizer
{
    public class SceneData
    {
        public Vector3 Location { get; set; }
        public Vector3 ForwardVec { get; set; }
        public Vector3 RightVec { get; set; }
        public Vector3 UpwardVec { get { return Vector3.Cross(RightVec, ForwardVec); } }

        public float TURNSPEED = .03f;
        public Matrix4 LeftTransform { get { return Matrix4.CreateRotationY(TURNSPEED); } }
        public Matrix4 RightTransform { get { return Matrix4.CreateRotationY(-TURNSPEED); } }
        public float MOVESPEED = 5f;
        public float CROSSPLANE_SPEED = 5f;

        private List<Viewable> _viewables;
        public List<Viewable> Viewables { get { return _viewables; } }
        
        public SceneData()
        {
            _viewables = new List<Viewable>();
        }

        public void SaveState(StreamWriter w)
        {
            w.WriteLine("{0} {1} {2}", Location.X, Location.Y, Location.Z);
            w.WriteLine("{0} {1} {2}", ForwardVec.X, ForwardVec.Y, ForwardVec.Z);
            w.WriteLine("{0} {1} {2}", RightVec.X, RightVec.Y, RightVec.Z);
           // w.WriteLine("{0}", CrossPlaneDist);
            w.Flush();
        }

        public void RecoverState(StreamReader r)
        {
            String[] a = r.ReadLine().Split(' ');
            Location = new Vector3(float.Parse(a[0]), float.Parse(a[1]), float.Parse(a[2]));
            a = r.ReadLine().Split(' ');
            ForwardVec = new Vector3(float.Parse(a[0]), float.Parse(a[1]), float.Parse(a[2]));
            a = r.ReadLine().Split(' ');
            RightVec = new Vector3(float.Parse(a[0]), float.Parse(a[1]), float.Parse(a[2]));
           // CrossPlaneDist = float.Parse(r.ReadLine());
        }
    }
}
