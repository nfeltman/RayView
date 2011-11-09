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
        public BVH2Node Root { get { return root; } }

        private BVH2() { }

        public Ret Accept<Ret>(BVH2Visitor<Ret> visitor)
        {
            return root.Accept(visitor);
        }

        public static BVH2 ReadFromFile(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            BVH2Node root = ParseNode(reader);
            int endSentinel = reader.ReadInt32();
            if (endSentinel != 9215) throw new IOException("Sentinel not found!");
            return new BVH2() { root = root };
        }

        private static BVH2Node ParseNode(BinaryReader reader)
        {
            int type = reader.ReadInt32();
            if (type == 0) //branch type
            {
                Box3 bbox = ReadBoundingBox(reader);
                BVH2Node left = ParseNode(reader);
                BVH2Node right = ParseNode(reader);
                return new BVH2Branch() { BBox = bbox, Left = left, Right = right };
            }
            else if (type == 1) //leaf type
            {
                int numTriangles = reader.ReadInt32();
                Box3 bbox = ReadBoundingBox(reader);
                Triangle[] tris = new Triangle[numTriangles];
                for (int k = 0; k < numTriangles; k++)
                {
                    tris[k] = new Triangle()
                    {
                        p1 = new CVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                        p2 = new CVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                        p3 = new CVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
                    };
                }
                return new BVH2Leaf() { BBox = bbox, Primitives = tris };
            }
            else
            {
                throw new IOException("Unexpected block header: " + type);
            }
        }

        private static Box3 ReadBoundingBox(BinaryReader reader)
        {
            return new Box3(reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle());
        }
    }

    public interface BVH2Node
    {
        Box3 BBox { get; set; }
        Ret Accept<Ret>(BVH2Visitor<Ret> visitor);
        Ret Accept<Ret>(Func<BVH2Branch, Ret> forBranch, Func<BVH2Leaf, Ret> forLeaf);
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
        public Ret Accept<Ret>(Func<BVH2Branch, Ret> forBranch, Func<BVH2Leaf, Ret> forLeaf)
        {
            return forBranch(this);
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
        public Ret Accept<Ret>(Func<BVH2Branch, Ret> forBranch, Func<BVH2Leaf, Ret> forLeaf)
        {
            return forLeaf(this);
        }

        public Tuple<float, int> FindClosestPositiveIntersection(CVector3 origin, CVector3 direction)
        {
            // TODO : have it take in a c vector

            float closestIntersection = float.PositiveInfinity;
            int closestIndex = -1;
            Triangle[] triangles = Primitives;
            for (int k = 0; k < triangles.Length; k++)
            {
                float intersection = triangles[k].IntersectRay(origin, direction);
                if (intersection != -1 && intersection>0.0005 && intersection < closestIntersection)
                {
                    closestIntersection = intersection;
                    closestIndex = k;
                }
            }
            return new Tuple<float, int>(closestIntersection, closestIndex);
        }
    }

}
