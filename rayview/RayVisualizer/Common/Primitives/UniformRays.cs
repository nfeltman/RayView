using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayVisualizer.Common
{
    public static class UniformRays
    {
        public static IntersectionReport GetReport(Box3 parent, Box3 left, Box3 right)
        {
            float psa = parent.SurfaceArea;
            float P_r = right.SurfaceArea / psa;
            float P_l = left.SurfaceArea / psa;
            float P_lr  = (CalculateExternalFormFactor(left, right) + (left & right).SurfaceArea) / psa;
            //float P_lr2 = (CalculateExternalFormFactor(right, left) + (left & right).SurfaceArea) / psa;
            //if (Math.Abs(P_lr - P_lr2) > .0001) throw new Exception("Internal error!");
            float P_jl = P_l - P_lr;
            float P_jr = P_r - P_lr;
            float P_e = 1 - P_lr - P_jl - P_jr;
            /*
            if (float.IsNaN(P_e))
            {
                Console.WriteLine("+++ {0}\n -> {1}\n -> {2}", parent, left, right);
                Console.WriteLine("P_jl: {1}, P_jr: {2}, P_lr: {3}, P_e: {4} ", 0, P_jl, P_jr, P_lr, P_e);
            }*/
            return new IntersectionReport(P_l, P_r, P_jl, P_jr, P_lr, P_e);
        }

        // calculate the proportion of radiance from the surface of source that intersects sink
        private static float CalculateExternalFormFactor(Box3 source, Box3 sink)
        {
            return CalculateFormFactor_PerDimension(source.XRange, source.YRange, source.ZRange, sink.XRange, sink.YRange, sink.ZRange)
                 + CalculateFormFactor_PerDimension(source.YRange, source.ZRange, source.XRange, sink.YRange, sink.ZRange, sink.XRange)
                 + CalculateFormFactor_PerDimension(source.ZRange, source.XRange, source.YRange, sink.ZRange, sink.XRange, sink.YRange);
        }

        // calculate the contributions from sink's ends on the d1 axis
        private static float CalculateFormFactor_PerDimension(ClosedInterval source_d1, ClosedInterval source_d2, ClosedInterval source_d3, ClosedInterval sink_d1, ClosedInterval sink_d2, ClosedInterval sink_d3)
        {
            // max side
            float F = 0;
            F += OpposingFormFactor(true, source_d1.Max, source_d2, source_d3, sink_d1.Min, sink_d2, sink_d3);
            F += OpposingFormFactor(false, source_d1.Min, source_d2, source_d3, sink_d1.Max, sink_d2, sink_d3);

            ClosedInterval positive_source_d2 = sink_d2.GreaterSpace() & source_d2;
            ClosedInterval negative_source_d2 = sink_d2.LesserSpace() & source_d2;
            ClosedInterval positive_source_d3 = sink_d3.GreaterSpace() & source_d3;
            ClosedInterval negative_source_d3 = sink_d3.LesserSpace() & source_d3;

            for (int k = 0; k <= 1; k++) // alternative over ends of the source in the d1 dimension
            {
                float source_d1_end = k == 0 ? source_d1.Max : source_d1.Min;
                ClosedInterval effective_sink_d1 = (k == 0 ? source_d1.GreaterSpace() : source_d1.LesserSpace()) & sink_d1;

                if (!effective_sink_d1.IsEmpty)
                {
                    F += PerpendicularFormFactor(source_d1_end, positive_source_d2, source_d3, effective_sink_d1, sink_d2.Max, sink_d3); // d2 high
                    F += PerpendicularFormFactor(source_d1_end, negative_source_d2, source_d3, effective_sink_d1, sink_d2.Min, sink_d3); // d2 low
                    F += PerpendicularFormFactor(source_d1_end, positive_source_d3, source_d2, effective_sink_d1, sink_d3.Max, sink_d2); // d3 high
                    F += PerpendicularFormFactor(source_d1_end, negative_source_d3, source_d2, effective_sink_d1, sink_d3.Min, sink_d2); // d3 low
                }
            }

            return F;
        }

        // calculate the form factor of two faces with opposite normals
        private static float OpposingFormFactor(bool positiveSourceNormal, float source_plane, ClosedInterval source_d1, ClosedInterval source_d2, float sink_plane, ClosedInterval sink_d1, ClosedInterval sink_d2)
        {
            if (source_plane == sink_plane || positiveSourceNormal != source_plane < sink_plane) return 0f;
            if (source_d1.Size == 0 || source_d2.Size == 0 || sink_d1.Size == 0 || sink_d2.Size == 0) return 0f;

            float z = Math.Abs(source_plane - sink_plane); //technically the abs doesn't matter, but I still like it

            double F = 0;
            for (int k1 = 0; k1 <= 1; k1++)
            {
                float u = k1 == 0 ? source_d1.Min : source_d1.Max;
                for (int k2 = 0; k2 <= 1; k2++)
                {
                    float v = k2 == 0 ? source_d2.Min : source_d2.Max;
                    for (int k3 = 0; k3 <= 1; k3++)
                    {
                        float x = k3 == 0 ? sink_d1.Min : sink_d1.Max;
                        for (int k4 = 0; k4 <= 1; k4++)
                        {
                            // this is fairly straight forward math; I don't think I need to explain anything
                            // that was a joke; see (http://dasan.sejong.ac.kr/~aschoi/Lab/s-07.pdf)
                            float y = k4 == 0 ? sink_d2.Min : sink_d2.Max;
                            //if (z == 0) Console.WriteLine("z is 0");
                            float a = (x - u) / z;
                            float b = (y - v) / z;
                            double sa = Math.Sqrt(1 + a * a);
                            double sb = Math.Sqrt(1 + b * b);
                            double H = b * sa * Math.Atan(b / sa) + a * sb * Math.Atan(a / sb) - Math.Log(1 + a * a + b * b) / 2;
                            int sign = ((k1 + k2 + k3 + k4) & 1) == 0 ? 1 : -1;
                            F += H * sign;
                        }
                    }
                }
            }

            //Console.WriteLine("opposing " + Math.Abs(z * z * F / Math.PI));
            return (float)Math.Abs(z * z * F / Math.PI);
        }

        // calculate the form factor of two faces with perpendicular normals
        private static float PerpendicularFormFactor(float source_d1, ClosedInterval source_d2, ClosedInterval source_d3, ClosedInterval sink_d1, float sink_d2, ClosedInterval sink_d3)
        {
            //Console.WriteLine("{0} {1} {2} {3} {4} {5}", source_d1, source_d2, source_d3, sink_d1, sink_d2, sink_d3);
            if (source_d2.Size == 0 || source_d3.Size == 0 || sink_d1.Size == 0 || sink_d3.Size == 0) return 0f;
            double F = 0;
            for (int k1 = 0; k1 <= 1; k1++)
            {
                float v = k1 == 0 ? sink_d3.Min : sink_d3.Max;
                for (int k2 = 0; k2 <= 1; k2++)
                {
                    float z = k2 == 0 ? sink_d1.Min : sink_d1.Max;
                    for (int k3 = 0; k3 <= 1; k3++)
                    {
                        float x = k3 == 0 ? source_d2.Min : source_d2.Max;
                        for (int k4 = 0; k4 <= 1; k4++)
                        {
                            // see (http://dasan.sejong.ac.kr/~aschoi/Lab/s-07.pdf)
                            float y = k4 == 0 ? source_d3.Min : source_d3.Max;

                            float a = (y - v);
                            float b = (source_d1 - z);
                            float c = (x - sink_d2);
                            if (a != 0 || b != 0 || c != 0)
                            {
                                double s = Math.Sqrt(c * c + b * b);
                                double G = a * s * Math.Atan2(a, s) + (a * a - b * b - c * c) * Math.Log(a * a + b * b + c * c) / 4;
                                int sign = ((k1 + k2 + k3 + k4) & 1) == 0 ? 1 : -1;
                                //if (double.IsNaN(G)) Console.WriteLine("problem here: a = {0}, s = {1}, L = {2}, atan2 = {3}", a, s, a * a + b * b + c * c, Math.Atan2(a, s));
                                F += G * sign;
                            }
                        }
                    }
                }
            }

            //Console.WriteLine("perp " +(float)Math.Abs(F / Math.PI));
            return (float)Math.Abs(F / Math.PI);
        }

        public static Ray3 RandomInternalRay(Box3 b, Random r)
        {

            // cosine weighted hemisphere
            double phi = 2.0f * Math.PI * r.NextDouble();
            double vv = r.NextDouble();
            double cosTheta = Math.Sqrt(vv);
            double sinTheta = Math.Sqrt(1 - vv);

            float d1 = (float)(Math.Cos(phi) * sinTheta);
            float d2 = (float)(Math.Sin(phi) * sinTheta);
            float up = (float)cosTheta;

            float a_z = b.XRange.Size * b.YRange.Size;
            float a_x = b.YRange.Size * b.ZRange.Size;
            float a_y = b.ZRange.Size * b.XRange.Size;

            double face = (a_x + a_y + a_z) * r.NextDouble();
            CVector3 origin, direction;
            if (face <= a_x)
            {
                float x, dx;
                if (r.NextDouble() <= 0.5)
                {
                    dx = up;
                    x = b.XRange.Min;
                }
                else
                {
                    dx = -up;
                    x = b.XRange.Max;
                }
                origin = new CVector3(x, b.YRange.UniformSample(r), b.ZRange.UniformSample(r));
                direction = new CVector3(dx, d1, d2);
            }
            else if (face <= a_x + a_y)
            {
                float y, dy;
                if (r.NextDouble() <= 0.5)
                {
                    dy = up;
                    y = b.YRange.Min;
                }
                else
                {
                    dy = -up;
                    y = b.YRange.Max;
                }
                origin = new CVector3(b.XRange.UniformSample(r), y, b.ZRange.UniformSample(r));
                direction = new CVector3(d1, dy, d2);
            }
            else
            {
                float z, dz;
                if (r.NextDouble() <= 0.5)
                {
                    dz = up;
                    z = b.ZRange.Min;
                }
                else
                {
                    dz = -up;
                    z = b.ZRange.Max;
                }
                origin = new CVector3(b.XRange.UniformSample(r), b.YRange.UniformSample(r), z);
                direction = new CVector3(d1, d2, dz);
            }

            //Console.WriteLine("{0} -> {1}", origin, direction);

            return new Ray3(origin, direction);
        }
    }

    public class IntersectionReport
    {
        public float Left, Right, JustLeft, JustRight, Both, Neither;

        public IntersectionReport(float left, float right, float justLeft, float justRight, float both, float neither)
        {
            Left = left;
            Right = right;
            JustLeft = justLeft;
            JustRight = justRight;
            Both = both;
            Neither = neither;
        }
    }
}
