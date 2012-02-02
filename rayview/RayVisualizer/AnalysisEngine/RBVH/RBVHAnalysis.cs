using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RayVisualizer.Common;
using System.IO;

namespace AnalysisEngine
{
    public class RBVHAnalysis
    {
        public static void ReadTestRays(string tracesPath)
        {
            RaySet allrays = RayFileParser.ReadFromFile2(new FileStream(tracesPath + "scientist\\raydump.ray", FileMode.Open, FileAccess.Read));
            int count = 0;
            foreach(ShadowQuery q in allrays.ShadowQueries)
            {
                if (q.Connected)
                    count++;
            }
            Console.WriteLine("asdfhdslkjfhksadjhflksdajh "+count);
        }

        public static void ReadTestTris(string tracesPath)
        {
            List<Triangle> tris = OBJParser.ParseOBJTriangles(new FileStream(tracesPath + "scientist\\triangles.obj", FileMode.Open, FileAccess.Read));
            Console.WriteLine("asdfhdslkjfhksadjhflksdajh ");
        }
    }
}
