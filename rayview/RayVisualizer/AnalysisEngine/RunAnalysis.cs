using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RayVisualizer.Common
{
    class RunAnalysis
    {
        public static void Main()
        {
            BVH2.ReadFromFile(new StreamReader("..\\..\\..\\..\\..\\traces\\bvhTrace.txt"));
            Console.Read();
        }
    }
}
