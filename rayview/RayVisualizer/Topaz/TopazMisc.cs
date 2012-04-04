using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Simd;
using System.Diagnostics;
using RayVisualizer.Common;

namespace Topaz
{
    public static class TopazMisc
    {
        public static void TestSIMD()
        {
            Console.WriteLine("GC max generation: {0}", GC.MaxGeneration);

            Console.WriteLine("SIMD Acceleration Mode: {0}.", SimdRuntime.AccelMode);
            Console.WriteLine("Vector4f (+) Acceleration Requirement: {0}. Met: {1}.", SimdRuntime.MethodAccelerationMode(typeof(Vector4f), "op_Addition", typeof(Vector4f), typeof(Vector4f)),
                SimdRuntime.IsMethodAccelerated(typeof(Vector4f), "op_Addition", typeof(Vector4f), typeof(Vector4f)));
            Console.WriteLine("Vector4f (-) Acceleration Requirement: {0}. Met: {1}.", SimdRuntime.MethodAccelerationMode(typeof(Vector4f), "op_Subtraction", typeof(Vector4f), typeof(Vector4f)),
                SimdRuntime.IsMethodAccelerated(typeof(Vector4f), "op_Subtraction", typeof(Vector4f), typeof(Vector4f)));
            Console.WriteLine("Vector4f (*) Acceleration Requirement: {0}. Met: {1}.", SimdRuntime.MethodAccelerationMode(typeof(Vector4f), "op_Multiply", typeof(Vector4f), typeof(Vector4f)),
                SimdRuntime.IsMethodAccelerated(typeof(Vector4f), "op_Multiply", typeof(Vector4f), typeof(Vector4f)));
            Console.WriteLine("Vector4f (/) Acceleration Requirement: {0}. Met: {1}.", SimdRuntime.MethodAccelerationMode(typeof(Vector4f), "op_Division", typeof(Vector4f), typeof(Vector4f)),
                SimdRuntime.IsMethodAccelerated(typeof(Vector4f), "op_Division", typeof(Vector4f), typeof(Vector4f)));
            Console.WriteLine("Vector4f (min) Acceleration Requirement: {0}. Met: {1}.", SimdRuntime.MethodAccelerationMode(typeof(VectorOperations), "Min", typeof(Vector4f), typeof(Vector4f)),
                SimdRuntime.IsMethodAccelerated(typeof(VectorOperations), "Min", typeof(Vector4f), typeof(Vector4f)));
            Console.WriteLine("Vector4f (max) Acceleration Requirement: {0}. Met: {1}.", SimdRuntime.MethodAccelerationMode(typeof(VectorOperations), "Max", typeof(Vector4f), typeof(Vector4f)),
                SimdRuntime.IsMethodAccelerated(typeof(VectorOperations), "Max", typeof(Vector4f), typeof(Vector4f)));

            int numiters = 100000000;
            Console.WriteLine("Speed test. Number iterations: {0}", numiters);
            Stopwatch st = new Stopwatch();
            float a1 = -.2f, a2 = -.3f, a3 = -.3f, a4 = -.5f;
            float b1 = 0.1f, b2 = 0.3f, b3 = 0.6f, b4 = -.3f;
            float z1 = 0f, z2 = 0f, z3 = 0f, z4 = 0f;
            float y1 = 0f, y2 = 0f, y3 = 0f, y4 = 0f;
            st.Start();
            for (int k = 0; k < numiters; k++)
            {
                z1 = z1 * z1 - y1 * y1 + a1; y1 = 2f * z1 * y1 + b1;
                z2 = z2 * z2 - y2 * y2 + a2; y2 = 2f * z2 * y2 + b2;
                z3 = z3 * z3 - y3 * y3 + a3; y3 = 2f * z3 * y3 + b3;
                z4 = z4 * z4 - y4 * y4 + a4; y4 = 2f * z4 * y4 + b4;
            }
            st.Stop();
            Console.WriteLine("Non-SIMD: Took {0} ms.", st.ElapsedMilliseconds);
            Console.WriteLine("<{0}, {1}, {2}, {3}>, <{4}, {5}, {6}, {7}>", z1, z2, z3, z4, y1, y2, y3, y4);
            st.Reset();
            st.Start();
            Vector4f a = new Vector4f(-.2f, -.3f, -.3f, -.5f);
            Vector4f b = new Vector4f(.1f, .3f, .6f, -.3f);
            Vector4f z = new Vector4f(0, 0, 0, 0);
            Vector4f y = new Vector4f(0, 0, 0, 0);
            for (int k = 0; k < numiters; k++)
            {
                z = z * z - y * y + a;
                y = z * y * 2 + b;
            }
            st.Stop();
            Console.WriteLine("SIMD: Took {0} ms.", st.ElapsedMilliseconds);
            Console.WriteLine("{0} {1}", z, y);
        }

        public static void TestAnalyticUniformFunctions()
        {
            Random r = new Random(971297);
            int testSize = 2000000;

            for (int j = 0; j < 50; j++)
            {
                Console.WriteLine(j);
                ClosedInterval range = new ClosedInterval(0, 100);
                float x1 = range.UniformSample(r), x2 = range.UniformSample(r);
                float y1 = range.UniformSample(r), y2 = range.UniformSample(r);
                float z1 = range.UniformSample(r), z2 = range.UniformSample(r);
                Box3 parent = new Box3(Math.Min(x1, x2), Math.Max(x1, x2), Math.Min(y1, y2), Math.Max(y1, y2), Math.Min(z1, z2), Math.Max(z1, z2));
                x1 = parent.XRange.UniformSample(r); x2 = parent.XRange.UniformSample(r);
                y1 = parent.YRange.UniformSample(r); y2 = parent.YRange.UniformSample(r);
                z1 = parent.ZRange.UniformSample(r); z2 = parent.ZRange.UniformSample(r);
                Box3 left = new Box3(Math.Min(x1, x2), Math.Max(x1, x2), Math.Min(y1, y2), Math.Max(y1, y2), Math.Min(z1, z2), Math.Max(z1, z2));
                x1 = parent.XRange.UniformSample(r); x2 = parent.XRange.UniformSample(r);
                y1 = parent.YRange.UniformSample(r); y2 = parent.YRange.UniformSample(r);
                z1 = parent.ZRange.UniformSample(r); z2 = parent.ZRange.UniformSample(r);
                Box3 right = new Box3(Math.Min(x1, x2), Math.Max(x1, x2), Math.Min(y1, y2), Math.Max(y1, y2), Math.Min(z1, z2), Math.Max(z1, z2));
                //Box3 parent = new Box3(-100, 100, -100, 200, 0, 200);
                //Box3 left = new Box3(35f, 40f, 10f, 90f, 10f, 90f);
                //Box3 right = new Box3(10f, 70f, 40f, 45f, 30f, 70f);
                IntersectionReport est = UniformRays.GetReport(parent, left, right);
                IntersectionReport mea = new IntersectionReport(0, 0, 0, 0, 0, 0);
                for (int k = 0; k < testSize; k++)
                {
                    Ray3 ray = UniformRays.RandomInternalRay(parent, r);
                    bool hitsLeft = left.DoesIntersectRay(ray.Origin, ray.Direction);
                    bool hitsRight = right.DoesIntersectRay(ray.Origin, ray.Direction);
                    if (hitsLeft)
                    {
                        mea.Left++;
                        if (hitsRight)
                        {
                            mea.Right++;
                            mea.Both++;
                        }
                        else
                        {
                            mea.JustLeft++;
                        }
                    }
                    else
                    {
                        if (hitsRight)
                        {
                            mea.Right++;
                            mea.JustRight++;
                        }
                        else
                        {
                            mea.Neither++;
                        }
                    }
                }
                float threshold = 0.002f;
                if (mea.Left / testSize - est.Left > threshold
                    || mea.Right / testSize - est.Right > threshold
                    || mea.JustLeft / testSize - est.JustLeft > threshold
                    || mea.JustRight / testSize - est.JustRight > threshold
                    || mea.Both / testSize - est.Both > threshold
                    || mea.Neither / testSize - est.Neither > threshold)
                {
                    Console.WriteLine("+++ {0}\n -> {1}\n -> {2}", parent, left, right);
                    Console.WriteLine("      Left: {0:0.00000} - {1:0.00000} = {2}", mea.Left / testSize, est.Left, mea.Left / testSize - est.Left);
                    Console.WriteLine("     Right: {0:0.00000} - {1:0.00000} = {2}", mea.Right / testSize, est.Right, mea.Right / testSize - est.Right);
                    Console.WriteLine(" Just Left: {0:0.00000} - {1:0.00000} = {2}", mea.JustLeft / testSize, est.JustLeft, mea.JustLeft / testSize - est.JustLeft);
                    Console.WriteLine("Just Right: {0:0.00000} - {1:0.00000} = {2}", mea.JustRight / testSize, est.JustRight, mea.JustRight / testSize - est.JustRight);
                    Console.WriteLine("      Both: {0:0.00000} - {1:0.00000} = {2}", mea.Both / testSize, est.Both, mea.Both / testSize - est.Both);
                    Console.WriteLine("   Neither: {0:0.00000} - {1:0.00000} = {2}", mea.Neither / testSize, est.Neither, mea.Neither / testSize - est.Neither);
                    Console.WriteLine("===================================");
                }
            }
        }
    }
}
