using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Input;

namespace RayVisualizer
{
    public class CrossplaneState : ViewerState
    {
        private CrossplaneBehavior cross;

        public CrossplaneState(CrossplaneBehavior cb)
        {
            cross = cb;
        }

        public virtual IEnumerable<Viewable> CollectViewables(SceneData scene)
        {
            return new Viewable[] {};
        }

        public virtual void OnUpdateFrame(SceneData scene, MyKeyboard keyboard)
        {
            bool cameraMoved = false;

            if (keyboard.IsDown(Key.Left))
            {
                scene.ForwardVec = Vector3.Transform(scene.ForwardVec, scene.LeftTransform);
                scene.RightVec = Vector3.Transform(scene.RightVec, scene.LeftTransform);
                cameraMoved = true;
            }
            if (keyboard.IsDown(Key.Right))
            {
                scene.ForwardVec = Vector3.Transform(scene.ForwardVec, scene.RightTransform);
                scene.RightVec = Vector3.Transform(scene.RightVec, scene.RightTransform);
                cameraMoved = true;
            }
            if (keyboard.IsDown(Key.Up))
            {
                scene.ForwardVec = Vector3.Transform(scene.ForwardVec, Matrix4.CreateFromAxisAngle(scene.RightVec, scene.TURNSPEED));
                cameraMoved = true;
            }
            if (keyboard.IsDown(Key.Down))
            {
                scene.ForwardVec = Vector3.Transform(scene.ForwardVec, Matrix4.CreateFromAxisAngle(scene.RightVec, -scene.TURNSPEED));
                cameraMoved = true;
            }
            if (keyboard.IsDown(Key.W))
            {
                scene.Location += scene.ForwardVec * scene.MOVESPEED;
                cameraMoved = true;
            }
            if (keyboard.IsDown(Key.S))
            {
                scene.Location -= scene.ForwardVec * scene.MOVESPEED;
                cameraMoved = true;
            }
            if (keyboard.IsDown(Key.A))
            {
                scene.Location -= scene.RightVec * scene.MOVESPEED;
                cameraMoved = true;
            }
            if (keyboard.IsDown(Key.D))
            {
                scene.Location += scene.RightVec * scene.MOVESPEED;
                cameraMoved = true;
            } 
            if (keyboard.IsDown(Key.K))
            {
                scene.CrossPlaneDist += scene.CROSSPLANE_SPEED;
                cameraMoved = true;
            } 
            if (keyboard.IsDown(Key.M))
            {
                scene.CrossPlaneDist -= scene.CROSSPLANE_SPEED;
                cameraMoved = true;
            }
            if (keyboard.IsDown(Key.G))
            {
                scene.CrossPlaneFrozen = true;
            //    cross.UpdateCrossPlane(scene);
            }
            if (keyboard.IsDown(Key.H))
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
