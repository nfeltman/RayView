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
        private SceneData _scene;
        private ViewerState _activeState;
        private MyKeyboard _keyboard;

        private List<ViewerState> _statesList;

        public Program() : base(800, 600, new GraphicsMode(16, 16))
		{ } 

        private void InitializeStatesAndViews()
        {
            //scene.AllRays = RayFileParser.ReadFromFile(new FileStream("..\\..\\..\\..\\..\\traces\\castTrace.txt", FileMode.Open, FileAccess.Read));
            //scene.ActiveSet = scene.AllRays.Filter((CastHitQuery r,int i)=>r.Depth >= 1,null,null);
            //BVH2 bvh = BVH2Builder.BuildFullBVH(Shapes.BuildParallelogram(new CVector3(0, 0, 0), new CVector3(100, 0, 0), new CVector3(20, 100, 0), 10, 10).GetTriangleList(), (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea);
            BVH2 bvh = BVH2Builder.BuildFullBVH(Shapes.BuildSphere(new CVector3(0, 0, 0), new CVector3(100, 0, 0), new CVector3(0, 100, 0), 12).GetTriangleList(), (ln, lb, rn, rb) => (ln - 1) * lb.SurfaceArea + (rn - 1) * rb.SurfaceArea);
            Ray3[] rays = RayDistributions.AlignedCircularFrustrum(new CVector3(0, 0, 0), new CVector3(100, 0, 0), 50, 20, 3000);

            BVHTriangleViewer bvhViewer = new BVHTriangleViewer(bvh);
            _scene.Viewables.Add(bvhViewer);
            _scene.Viewables.Add(new RaysViewer(rays,1));

            // states
            _statesList = new List<ViewerState>();
            _statesList.Add(new ExploreState());
            _statesList.Add(new BVHExploreState(bvhViewer));
            _activeState = _statesList[0];
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _scene = new SceneData();
            _keyboard = new MyKeyboard(Keyboard);

            if(File.Exists("..\\..\\..\\..\\..\\autostatesave.init"))
            {
                using (FileStream st = new FileStream("..\\..\\..\\..\\..\\autostatesave.init", FileMode.Open, FileAccess.Read))
                {
                    _scene.RecoverState(new StreamReader(st));
                }
            }
            else
            {
                _scene.Location = new Vector3(-4, 0, -4);
                _scene.ForwardVec = new Vector3(1, 0, 0);
                _scene.RightVec = new Vector3(0, 0, 1);
            }
            InitializeStatesAndViews();

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
                _scene.SaveState(new StreamWriter(st));
                st.Close();
                this.Exit();
                return;
            }

            int stateSelected = getStateSelection();
            if (stateSelected != -1 && _statesList.Count > stateSelected)
            {
                ViewerState newState = _statesList[stateSelected];
                if (newState != _activeState)
                {
                    _activeState.HibernateState(_scene);
                    newState.ActivateState(_scene);
                    _activeState = newState;
                }
            }

            _activeState.OnUpdateFrame(_scene, _keyboard);
            _keyboard.MarkKeysHandled();

        }

        private int getStateSelection()
        {
            if (Keyboard[Key.Number1])
                return 0;
            else if (Keyboard[Key.Number2])
                return 1;
            else if (Keyboard[Key.Number3])
                return 2;
            else if (Keyboard[Key.Number4])
                return 3;
            else if (Keyboard[Key.Number5])
                return 4;
            else if (Keyboard[Key.Number6])
                return 5;
            else if (Keyboard[Key.Number7])
                return 6;
            else if (Keyboard[Key.Number8])
                return 7;
            else if (Keyboard[Key.Number9])
                return 8;
            else if (Keyboard[Key.Number0])
                return 9;
            return -1;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Vector3 up = Vector3.Cross(_scene.RightVec, _scene.ForwardVec);
            Matrix4 lookat = Matrix4.LookAt(_scene.Location, _scene.Location + _scene.ForwardVec, up);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);

            foreach (Viewable v in _scene.Viewables) v.DrawOpaquePart();
            foreach (Viewable v in _scene.Viewables) v.DrawTransparentPart();

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
