using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RayVisualizer.Common;

namespace Topaz
{
    using BVH2 = Tree<BVH2Branch, BVH2Leaf>;
    using RBVH2 = Tree<RBVH2Branch, RBVH2Leaf>;
    using BackedBVH2 = Tree<BackedBVH2Branch, BackedBVH2Leaf>;
    using BackedRBVH2 = Tree<BackedRBVH2Branch, BackedRBVH2Leaf>;

    public class TopazStreamWriter : IDisposable
    {
        private StreamWriter _output;
        private Stream _fileout;
        private string _filename;

        public TopazStreamWriter(string filename)
        {
            _filename = filename;
        }

        private void OpenStream()
        {
            if (_output == null)
            {
                _fileout = File.Open(_filename, FileMode.Create, FileAccess.Write);
                _output = new StreamWriter(_fileout);
            }
        }

        public void StandardRBVHStatsReport(RBVH2 build)
        {
            OpenStream();
            PrintSimple("Number Leaves", build.NumLeaves);
            PrintSimple("Height", build.RollUp((b, l, r) => Math.Max(l, r) + 1, le => 1));
        }

        public void PrintCost(string statPrefix, TraceCost cost)
        {
            OpenStream();
            PrintRandomVariable(statPrefix + "BBox Tests", cost.BBoxTests);
            PrintRandomVariable(statPrefix + "Prim Tests", cost.PrimitiveTests);
        }

        public void PrintRandomVariable(string stat, RandomVariable rv)
        {
            OpenStream();
            _output.WriteLine("\"{0}\" \"EXP\" {1}", stat, rv.ExpectedValue);
            _output.WriteLine("\"{0}\" \"STD\" {1}", stat, Math.Sqrt(rv.Variance));
        }

        public void PrintSimple(string stat, double d)
        {
            OpenStream();
            _output.WriteLine("\"{0}\" \"total\" {1}", stat, d);
        }

        public void PrintArray(string stat, int[] d)
        {
            OpenStream();
            _output.WriteLine("\"{0}\" \"array\" {1}", stat, String.Join(" ", d));
        }

        public void WriteLine(string format, params object[] args)
        {
            OpenStream();
            _output.WriteLine(format, args);
        }

        public void Write(string format, params object[] args)
        {
            OpenStream();
            _output.Write(format, args);
        }

        public void Dispose()
        {
            if (_output != null)
            {
                _output.Flush();
                _fileout.Flush();
                _output.Dispose();
                _fileout.Dispose();
            }
        }
    }
}
