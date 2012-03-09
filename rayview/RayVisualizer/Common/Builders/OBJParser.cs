using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RayVisualizer.Common
{
    public class OBJParser
    {
        public static List<Triangle> ParseOBJTriangles(Stream file)
        {
            List<Triangle> tris = new List<Triangle>();
            List<CVector3> vertices = new List<CVector3>();

            StreamReader reader = new StreamReader(file);

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line.Length == 0)
                    continue;
                string[] words = line.Split(' ');
                if (words[0].Equals("v"))
                {
                    float x = float.Parse(words[1]);
                    float y = float.Parse(words[2]);
                    float z = float.Parse(words[3]);
                    vertices.Add(new CVector3(x, y, z));
                }
                else if (words[0].Equals("f"))
                {
                    if (words.Length > 4) throw new Exception(String.Format("I only support up to triangles.  Found a face with {0} verteces.", words.Length-1));
                    // things are 1-indexed in the file format; we will work by 0-index
                    int index1 = int.Parse(words[1]) - 1;
                    int index2 = int.Parse(words[2]) - 1;
                    int index3 = int.Parse(words[3]) - 1;
                    tris.Add(new Triangle(vertices[index1], vertices[index2], vertices[index3]));
                }
                else
                {
                    throw new Exception(String.Format("Unrecognized item type {0}", words[0]));
                }
            }

            return tris;
        }
    }
}
