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
        Vector3[] rayArray;
        Vector3 location;
        Vector3 forward, right;
        const float TURNSPEED = .03f;
        Matrix4 leftTransform = Matrix4.CreateRotationY(TURNSPEED);
        Matrix4 rightTransform = Matrix4.CreateRotationY(-TURNSPEED);
        const float MOVESPEED = 5f;

        public Program() : base(800, 600, new GraphicsMode(16, 16))
		{ } 

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            RaySet rays = RaySet.ReadFromFile(new FileStream("..\\..\\..\\..\\..\\traces\\evilTrace.txt", FileMode.Open, FileAccess.Read));
            rayArray = new Vector3[rays.Rays.Count * 2];
            for(int k=0;k<rays.Rays.Count;k++)
            {
                rayArray[k * 2 + 0] = new Vector3(rays.Rays[k].Origin.x, rays.Rays[k].Origin.y, rays.Rays[k].Origin.z);
                rayArray[k * 2 + 1] = new Vector3(rays.Rays[k].Direction.x, rays.Rays[k].Direction.y, rays.Rays[k].Direction.z)*20+rayArray[k*2];
            }

            location = new Vector3(-4, 0, -4);
            forward = new Vector3(1, 0, 0);
            right = new Vector3(0, 0, 1);

            GL.ClearColor(Color.SkyBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.DstColor);
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
                this.Exit();
                return;
            }
            if(Keyboard[Key.Left])
            {
                forward = Vector3.Transform(forward, leftTransform);
                right = Vector3.Transform(right, leftTransform);
            }
            if (Keyboard[Key.Right])
            {
                forward = Vector3.Transform(forward, rightTransform);
                right = Vector3.Transform(right, rightTransform);
            }
            if (Keyboard[Key.Up])
            {
                forward = Vector3.Transform(forward,Matrix4.CreateFromAxisAngle(right,TURNSPEED));
            }
            if (Keyboard[Key.Down])
            {
                forward = Vector3.Transform(forward, Matrix4.CreateFromAxisAngle(right, -TURNSPEED));
            }
            if (Keyboard[Key.W])
            {
                location += forward*MOVESPEED;
            }
            if (Keyboard[Key.S])
            {
                location -= forward * MOVESPEED;
            }
            if (Keyboard[Key.A])
            {
                location -= right * MOVESPEED;
            }
            if (Keyboard[Key.D])
            {
                location += right * MOVESPEED;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            
            Matrix4 lookat = Matrix4.LookAt(location,location+forward,Vector3.Cross(right,forward));
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);

            GL.Color4(.5,0,0,.25);
            GL.Begin(BeginMode.Lines);
            foreach(Vector3 v in rayArray)
                GL.Vertex3(v);
            GL.End();

            this.SwapBuffers();
            Thread.Sleep(1);
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
