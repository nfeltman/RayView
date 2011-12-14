#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2006-2008 the OpenTK Team.
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing detailed licensing details.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK.Input;
using RayVisualizer.Common;
using System.IO;

namespace RayVisualizer{

    public class Program : GameWindow
    {
        private SceneData scene;
        private ViewerState state;
        private MyKeyboard keyboard;

        private ExploreState explorerState;
        private BVHExploreState bvhState;

        public Program() : base(800, 600, new GraphicsMode(16, 16))
		{ } 

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            scene = new SceneData();

            if(File.Exists("..\\..\\..\\..\\..\\autostatesave.init"))
            {
                using (FileStream st = new FileStream("..\\..\\..\\..\\..\\autostatesave.init", FileMode.Open, FileAccess.Read))
                {
                    scene.RecoverState(new StreamReader(st));
                }
            }
            else
            {
                scene.Location = new Vector3(-4, 0, -4);
                scene.ForwardVec = new Vector3(1, 0, 0);
                scene.RightVec = new Vector3(0, 0, 1);
                scene.CrossPlaneDist = 100;
            }
            //scene.AllRays = RayFileParser.ReadFromFile(new FileStream("..\\..\\..\\..\\..\\traces\\castTrace.txt", FileMode.Open, FileAccess.Read));
            //scene.ActiveSet = scene.AllRays.Filter((CastHitQuery r,int i)=>r.Depth >= 1,null,null);
            BVH2 bvh = BVH2Builder.BuildFullBVH(Shapes.BuildSphere(new CVector3(0, 0, 0), new CVector3(100, 0, 0), new CVector3(0, 100, 0), 12).GetTriangleList(), (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea);
            //scene.BVHView = BVH2Builder.BuildFullBVH(Shapes.BuildParallelogram(new CVector3(0, 0, 0), new CVector3(100, 0, 0), new CVector3(20, 100, 0), 10, 10).GetTriangleList(), (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea).Root;

            keyboard = new MyKeyboard(Keyboard);
            explorerState = new ExploreState();
            state = bvhState = new BVHExploreState(bvh);

            GL.ClearColor(Color.LightGray);
            //GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Width, Height);

            double aspect_ratio = Width / (double)Height;

            OpenTK.Matrix4 perspective = OpenTK.Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)aspect_ratio, 1, 1500);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perspective);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (Keyboard[Key.Escape])
            {
                FileStream st = new FileStream("..\\..\\..\\..\\..\\autostatesave.init", FileMode.OpenOrCreate, FileAccess.Write);
                scene.SaveState(new StreamWriter(st));
                st.Close();
                this.Exit();
                return;
            }
            if (Keyboard[Key.Number1])
            {
                state = explorerState;
            }
            if (Keyboard[Key.Number2])
            {
                state = bvhState;
            }
            if (Keyboard[Key.Number3])
            {
                state = new CrossplaneState(new NullCrossplane());
            }
            if (Keyboard[Key.Number4])
            {
                state = new CrossplaneState(new IntersectionsCrossplane());
            }
            if (Keyboard[Key.Number5])
            {
                state = new CrossplaneState(new TrafficCrossplane());
            }

            state.OnUpdateFrame(scene, keyboard);
            keyboard.MarkKeysHandled();

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Vector3 up = Vector3.Cross(scene.RightVec, scene.ForwardVec);
            Matrix4 lookat = Matrix4.LookAt(scene.Location, scene.Location + scene.ForwardVec, up);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);

            IEnumerable<Viewable> viewables = state.CollectViewables(scene);
            foreach(Viewable v in viewables) v.DrawOpaquePart();
            foreach(Viewable v in viewables) v.DrawTransparentPart();

            SwapBuffers();
            
            Thread.Sleep(10);
        }

        [STAThread]
        public static void Main()
        {
            using (Program example = new Program())
            {
                example.Run(30.0, 0.0);
            }
        }
    }
}
