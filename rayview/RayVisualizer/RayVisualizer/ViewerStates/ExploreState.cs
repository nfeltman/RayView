using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using RayVisualizer.Common;
using System.IO;
using RayVisualizer.Common.BVH2Visitors;
using System.Collections;

namespace RayVisualizer
{
    class ExploreState : ViewerState
    {
        public ExploreState()
        {
        }

        public virtual IEnumerable<Viewable> CollectViewables(SceneData scene)
        {
            return new Viewable[] {};
        }

        public virtual void OnUpdateFrame(SceneData scene, MyKeyboard keyboard)
        {
            if (keyboard.IsDown(Key.Left))
            {
                scene.ForwardVec = Vector3.Transform(scene.ForwardVec, scene.LeftTransform);
                scene.RightVec = Vector3.Transform(scene.RightVec, scene.LeftTransform);
            }
            if (keyboard.IsDown(Key.Right))
            {
                scene.ForwardVec = Vector3.Transform(scene.ForwardVec, scene.RightTransform);
                scene.RightVec = Vector3.Transform(scene.RightVec, scene.RightTransform);
            }
            if (keyboard.IsDown(Key.Up))
            {
                if(keyboard.IsDown(Key.ControlLeft)||keyboard.IsDown(Key.ControlRight))
                    scene.Location += scene.UpwardVec * scene.MOVESPEED;
                else
                    scene.ForwardVec = Vector3.Transform(scene.ForwardVec, Matrix4.CreateFromAxisAngle(scene.RightVec, scene.TURNSPEED));
            }
            if (keyboard.IsDown(Key.Down))
            {
                if (keyboard.IsDown(Key.ControlLeft) || keyboard.IsDown(Key.ControlRight))
                    scene.Location -= scene.UpwardVec * scene.MOVESPEED;
                else
                    scene.ForwardVec = Vector3.Transform(scene.ForwardVec, Matrix4.CreateFromAxisAngle(scene.RightVec, -scene.TURNSPEED));
            }
            if (keyboard.IsDown(Key.W))
            {
                scene.Location += scene.ForwardVec * scene.MOVESPEED;
            }
            if (keyboard.IsDown(Key.S))
            {
                scene.Location -= scene.ForwardVec * scene.MOVESPEED;
            }
            if (keyboard.IsDown(Key.A))
            {
                scene.Location -= scene.RightVec * scene.MOVESPEED;
            }
            if (keyboard.IsDown(Key.D))
            {
                scene.Location += scene.RightVec * scene.MOVESPEED;
            } 
        }
    }
}
