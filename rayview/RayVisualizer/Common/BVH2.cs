using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RayVisualizer.Common
{
    public class BVH2
    {
        private BVH2Node root;

        private BVH2() { }

        public Ret Accept<Ret>(BVH2Visitor<Ret> visitor)
        {
            return root.Accept(visitor);
        }

        public static BVH2 ReadFromFile(StreamReader s)
        {
            return new BVH2() { root = ParseNode(s) };
        }

        private static BVH2Node ParseNode(StreamReader s)
        {
            String[] a = s.ReadLine().Split(' ');
            if (a.Length == 0)
            {
                throw new IOException("Error reading file: encountered empty line.");
            }
            else if (a[0].Equals("bran"))
            {
                Box3 bbox = new Box3(float.Parse(a[4]), float.Parse(a[5]), float.Parse(a[6]), float.Parse(a[7]), float.Parse(a[8]), float.Parse(a[9]));
                BVH2Node left = ParseNode(s);
                BVH2Node right = ParseNode(s);
                return new BVH2Branch() { BBox = bbox, Left = left, Right = right };
            }
            else if (a[0].Equals("leaf"))
            {
                Box3 bbox = new Box3(float.Parse(a[3]), float.Parse(a[4]), float.Parse(a[5]), float.Parse(a[6]), float.Parse(a[7]), float.Parse(a[8]));
                int numTriangles = int.Parse(a[2]);
                Triangle[] tris = new Triangle[numTriangles];
                for (int k = 0; k < numTriangles; k++)
                {
                    String[] r = s.ReadLine().Split(' ');
                    if (!r[0].Equals("tri")) throw new IOException("Expected tri, got: "+r[0]);
                    tris[k] = new Triangle() { 
                        p1= new CVector3(float.Parse(r[1]),float.Parse(a[2]), float.Parse(r[3])),
                        p2= new CVector3(float.Parse(r[4]),float.Parse(a[5]), float.Parse(r[6])),
                        p3= new CVector3(float.Parse(r[7]),float.Parse(a[8]), float.Parse(r[9]))};
                }
                return new BVH2Leaf() { BBox = bbox, Primitives = tris };
            }
            else
            {
                throw new IOException("Unexpected line header: " + a[0]);
            }
        }
    }

    public interface BVH2Node
    {
        Box3 BBox { get; set; }
        Ret Accept<Ret>(BVH2Visitor<Ret> visitor);
    }

    public class BVH2Branch : BVH2Node
    {
        public BVH2Node Left { get; set; }
        public BVH2Node Right { get; set; }
        public Box3 BBox { get; set; }

        public Ret Accept<Ret>(BVH2Visitor<Ret> visitor)
        {
            return visitor.ForBranch(this);
        }
    }

    public class BVH2Leaf : BVH2Node
    {
        public Box3 BBox { get; set; }
        public Triangle[] Primitives { get; set; }

        public Ret Accept<Ret>(BVH2Visitor<Ret> visitor)
        {
            return visitor.ForLeaf(this);
        }
    }

}
