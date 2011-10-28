using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using RayVisualizer.Common;

namespace RayVisualizer
{
    class ExploreState : ViewerState
    {
        private CrossplaneBehavior cross;

        public ExploreState(CrossplaneBehavior cb)
        {
            cross = cb;
        }

        public virtual void OnRenderFrame(SceneData scene, GameWindow w, FrameEventArgs e)
        {
            Vector3 up = Vector3.Cross(scene.RightVec, scene.ForwardVec);
            Matrix4 lookat = Matrix4.LookAt(scene.Location, scene.Location + scene.ForwardVec, up);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);

            //RAYS
            GL.Color4(.3, 0, 0, .1);
            GL.Begin(BeginMode.Lines);
            //foreach(RaySet set in scene.Rays)
            foreach (RayCast c in scene.Rays[1].Rays)
            {
                GL.Vertex3(c.Origin.x, c.Origin.y, c.Origin.z);
                GL.Vertex3(c.End.x, c.End.y, c.End.z);
            }
            GL.End();

            //CROSSPLANE
            cross.DrawResults(scene);
        }

        public virtual void OnUpdateFrame(SceneData scene, GameWindow w, FrameEventArgs e)
        {
            KeyboardDevice keyboard = w.Keyboard;
            bool cameraMoved = false;

            if (keyboard[Key.Left])
            {
                scene.ForwardVec = Vector3.Transform(scene.ForwardVec, scene.LeftTransform);
                scene.RightVec = Vector3.Transform(scene.RightVec, scene.LeftTransform);
                cameraMoved = true;
            }
            if (keyboard[Key.Right])
            {
                scene.ForwardVec = Vector3.Transform(scene.ForwardVec, scene.RightTransform);
                scene.RightVec = Vector3.Transform(scene.RightVec, scene.RightTransform);
                cameraMoved = true;
            }
            if (keyboard[Key.Up])
            {
                scene.ForwardVec = Vector3.Transform(scene.ForwardVec, Matrix4.CreateFromAxisAngle(scene.RightVec, scene.TURNSPEED));
                cameraMoved = true;
            }
            if (keyboard[Key.Down])
            {
                scene.ForwardVec = Vector3.Transform(scene.ForwardVec, Matrix4.CreateFromAxisAngle(scene.RightVec, -scene.TURNSPEED));
                cameraMoved = true;
            }
            if (keyboard[Key.W])
            {
                scene.Location += scene.ForwardVec * scene.MOVESPEED;
                cameraMoved = true;
            }
            if (keyboard[Key.S])
            {
                scene.Location -= scene.ForwardVec * scene.MOVESPEED;
                cameraMoved = true;
            }
            if (keyboard[Key.A])
            {
                scene.Location -= scene.RightVec * scene.MOVESPEED;
                cameraMoved = true;
            }
            if (keyboard[Key.D])
            {
                scene.Location += scene.RightVec * scene.MOVESPEED;
                cameraMoved = true;
            } 
            if (keyboard[Key.K])
            {
                scene.CrossPlaneDist += scene.CROSSPLANE_SPEED;
                cameraMoved = true;
            } 
            if (keyboard[Key.M])
            {
                scene.CrossPlaneDist -= scene.CROSSPLANE_SPEED;
                cameraMoved = true;
            }
            if (keyboard[Key.G])
            {
                scene.CrossPlaneFrozen = true;
            }
            if (keyboard[Key.H])
            {
                scene.CrossPlaneFrozen = false;
                cameraMoved = true;
            }
            if (cameraMoved && !scene.CrossPlaneFrozen)
            {
                cross.UpdateCrossPlane(scene);
            }
        }
    }
}
