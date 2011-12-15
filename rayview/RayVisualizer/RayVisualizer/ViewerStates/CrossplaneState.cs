using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Input;
using RayVisualizer.Common;

namespace RayVisualizer
{
    public class CrossplaneState : ViewerState
    {
        private RaySet _rays;
        private CrossplaneBehavior _cross;
        private float _crossPlaneDist;
        private bool _crossPlaneFrozen;

        public CrossplaneState(CrossplaneBehavior cb, RaySet raySet)
        {
            _cross = cb;
            _crossPlaneDist = 100;
            _crossPlaneFrozen = false;
            _rays = raySet;
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
                _crossPlaneDist += scene.CROSSPLANE_SPEED;
                cameraMoved = true;
            } 
            if (keyboard.IsDown(Key.M))
            {
                _crossPlaneDist -= scene.CROSSPLANE_SPEED;
                cameraMoved = true;
            }
            if (keyboard.IsDown(Key.G))
            {
                _crossPlaneFrozen = true;
            //    cross.UpdateCrossPlane(scene);
            }
            if (keyboard.IsDown(Key.H))
            {
                _crossPlaneFrozen = false;
                cameraMoved = true;
            }
            if (cameraMoved && !_crossPlaneFrozen)
            {
                _cross.UpdateCrossPlane(scene, _crossPlaneDist, _rays);
            }
        }


        public void HibernateState(SceneData scene)
        {
        }

        public void ActivateState(SceneData scene)
        {
        }
    }
}
